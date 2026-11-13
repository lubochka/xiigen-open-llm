// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler;

using Microsoft.CodeAnalysis.CSharp;
using SemanticCompiler.Completion;
using SemanticCompiler.Prediction;

// SemanticCompilerFacade wires the full E01-E27 pipeline: Compression → Routing →
// Planning → Translation → Beautification.
// Entry point: Compile(source) → CompilationResult.
public sealed class SemanticCompilerFacade : ISemanticCompiler
{
    private readonly StackDetector      _stackDetector;
    private readonly RouteClassifier    _routeClassifier;
    private readonly Decomposer         _decomposer;
    private readonly SemanticCompilerOptions _options;
    private readonly StructuralKnnPredictor _structuralKnn;
    private readonly MethodKnnPredictor _methodKnn;
    private readonly FileActionKnnPredictor _fileActionKnn;
    private readonly List<string>       _compiledSources = [];
    private MethodFrequencyTable?       _methodFrequencyTable;
    private bool                        _methodFrequencyDirty = true;
    private GrammarMaskFilter?          _grammarMaskFilter;
    private Dictionary<string, int>?    _grammarKindToIdx;
    private bool                        _grammarMaskDirty = true;

    public SemanticCompilerFacade(
        StackDetector?   stackDetector   = null,
        RouteClassifier? routeClassifier = null,
        Decomposer?      decomposer      = null,
        SemanticCompilerOptions? options = null,
        // FIX-KS: lets a caller supply a predictor with a different neighbour count.
        // Last and optional, so every existing call stays source-compatible and the
        // default behaviour is exactly the previous k = 5.
        MethodKnnPredictor? methodKnnPredictor = null)
    {
        _stackDetector   = stackDetector   ?? new StackDetector();
        _routeClassifier = routeClassifier ?? new RouteClassifier();
        _decomposer      = decomposer      ?? new Decomposer();
        _options         = options         ?? SemanticCompilerOptions.Default;
        _structuralKnn   = new StructuralKnnPredictor(k: 5);
        _methodKnn       = methodKnnPredictor ?? new MethodKnnPredictor(k: 5);
        _fileActionKnn   = new FileActionKnnPredictor(k: 3);
    }

    // ── ISemanticCompiler implementation ───────────────────────────────────────

    public OperationResult<StackLabel> DetectStack(string source)
        => _stackDetector.Detect(source);

    public OperationResult<SemanticCompiler29.Domain> ClassifyDomain(string prompt)
    {
        var lower = prompt.ToLowerInvariant();
        // Keyword fast-path (no training required for production use)
        return OperationResult<SemanticCompiler29.Domain>.Ok(RouteClassifier.ClassifyByKeywords(lower));
    }

    public OperationResult<IReadOnlyList<string>> NormalizeSource(string csharpSource)
    {
        var words = SemanticNormalizer.Normalize(csharpSource);
        return OperationResult<IReadOnlyList<string>>.Ok(words);
    }

    public OperationResult<IReadOnlyList<string>> CollapseToOps(IReadOnlyList<string> stemmedWords)
    {
        var ops = FamilyCollapser.Collapse(stemmedWords.ToList());
        return OperationResult<IReadOnlyList<string>>.Ok(ops);
    }

    public OperationResult<IReadOnlyList<int>> ExtractPredicates(string csharpSource)
        => LogicExtractor.Extract(csharpSource);

    public OperationResult<IReadOnlyList<PromptSegment>> SplitPrompt(string prompt)
        => _decomposer.Split(prompt);

    public OperationResult<int> DetectStopSignal(
        string taskDescription, bool hasIterations, bool hasTechTerms)
        => OperationResult<int>.Ok(
            StopSignalDetector.Detect(taskDescription, hasIterations, hasTechTerms));

    public OperationResult<string> BeautifyPredicates(
        IReadOnlyList<int> predicates, StackLabel stack)
    {
        // Try template retrieval first (E23)
        string stackName = stack switch
        {
            StackLabel.CSharp      => TemplateBeautifier.CSHARP,
            StackLabel.TypeScript  => TemplateBeautifier.TYPESCRIPT,
            _                      => TemplateBeautifier.GENERIC,
        };
        var template = TemplateBeautifier.FindBest(predicates, stackName);
        if (template != null)
            return OperationResult<string>.Ok(template.Code);

        // Fallback to CSharpBeautifier comment block
        return OperationResult<string>.Ok(CSharpBeautifier.Beautify(predicates, stack));
    }

    // Rank completion candidates by receiver-local method call frequency observed so far.
    public OperationResult<IReadOnlyList<string>> RankCompletions(
        string receiver,
        IReadOnlyList<string> candidates)
    {
        if (candidates.Count == 0)
            return OperationResult<IReadOnlyList<string>>.Ok([]);

        var table = EnsureMethodFrequencyTable();
        if (!table.IsSuccess)
            return OperationResult<IReadOnlyList<string>>.Ok(candidates.ToArray());

        return OperationResult<IReadOnlyList<string>>.Ok(table.Value!.RankMethods(receiver, candidates));
    }

    // ── Full pipeline ───────────────────────────────────────────────────────────

    public OperationResult<CompilationResult> Compile(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return OperationResult<CompilationResult>.Fail(
                "Source cannot be empty", "EMPTY_SOURCE");

        // E20 CRITICAL: StackDetector BEFORE SemanticNormalizer
        var stackResult = _stackDetector.Detect(source);
        if (!stackResult.IsSuccess)
            return OperationResult<CompilationResult>.Fail(
                stackResult.ErrorMessage, stackResult.ErrorCode);

        // E10: strip XML docs, block/line comments, string literals, and extra blanks.
        // C# source only; natural language prompts must keep literal documentation markers.
        if (_options.UseRegexPreprocessor && stackResult.Value == StackLabel.CSharp)
            source = RegexPreprocessor.Normalize(source);

        var normalizedResult = NormalizeSource(source);
        if (!normalizedResult.IsSuccess)
            return OperationResult<CompilationResult>.Fail(
                normalizedResult.ErrorMessage, normalizedResult.ErrorCode);

        var opsResult = CollapseToOps(normalizedResult.Value!);
        if (!opsResult.IsSuccess)
            return OperationResult<CompilationResult>.Fail(
                opsResult.ErrorMessage, opsResult.ErrorCode);

        var predicatesResult = LogicExtractor.Extract(source);
        if (!predicatesResult.IsSuccess)
            return OperationResult<CompilationResult>.Fail(
                predicatesResult.ErrorMessage, predicatesResult.ErrorCode);

        var segmentsResult = _decomposer.Split(source);
        if (!segmentsResult.IsSuccess)
            return OperationResult<CompilationResult>.Fail(
                segmentsResult.ErrorMessage, segmentsResult.ErrorCode);

        var domain = RouteClassifier.ClassifyByKeywords(source);

        RememberCompiledSource(source, stackResult.Value);
        var predictedNextKinds = PredictNextKinds(source, stackResult.Value);
        var predictedNextTokens = PredictStructuralNextTokens(source, stackResult.Value);
        var predictedNextMethodType = PredictNextMethodType(source, stackResult.Value);
        var predictedFileAction = PredictFileAction(source, stackResult.Value);

        var result = new CompilationResult(
            Stack:           stackResult.Value,
            Domain:          domain,
            Predicates:      predicatesResult.Value!,
            Ops:             opsResult.Value!,
            NormalizedWords: normalizedResult.Value!,
            Segments:        segmentsResult.Value!)
        {
            PredictedNextKinds = predictedNextKinds,
            PredictedNextTokens = predictedNextTokens,
            PredictedNextMethodType = predictedNextMethodType,
            PredictedFileAction = predictedFileAction
        };

        return OperationResult<CompilationResult>.Ok(result);
    }

    // Train the E13 method-level KNN predictor from ordered C# project sources.
    public void TrainMethodKnn(IReadOnlyList<string> csharpSources)
    {
        var allMethods = new List<PackedMethod>();
        for (short fileId = 0; fileId < Math.Min(csharpSources.Count, short.MaxValue); fileId++)
        {
            var extracted = MethodExtractor.Extract(csharpSources[fileId], fileId);
            if (extracted.IsSuccess)
                allMethods.AddRange(extracted.Value!);
        }

        // Admit the batch by the number of pairs the predictor can actually learn from,
        // not by the raw method count: Train clears the table first, so a batch with no
        // adjacent same-file pairs would wipe memory that is still answering queries.
        if (CountEligiblePairs(allMethods) >= 4)
            _methodKnn.Train(allMethods);
    }

    // FIX-KS: the same ingress, but the target side of each pair is trained from the
    // method's outer body instead of its full syntax. The legacy TrainMethodKnn above
    // is left alone and current callers keep using it; this is a separate entry point.
    public OperationResult<int> TrainMethodKnnWithTargetScope(IReadOnlyList<string> csharpSources)
    {
        var allMethods = new List<PackedMethod>();
        var scopes = new Dictionary<(short FileId, int IndexInFile), HashSet<string>>();
        for (short fileId = 0; fileId < Math.Min(csharpSources.Count, short.MaxValue); fileId++)
        {
            var extracted = MethodExtractor.ExtractWithTargetScope(csharpSources[fileId], fileId);
            if (!extracted.IsSuccess)
                continue;

            allMethods.AddRange(extracted.Value!.Methods);
            foreach (var (key, scope) in extracted.Value!.TargetScopes)
                scopes[key] = scope;
        }

        // Same admission as the legacy path, so an unusable batch cannot wipe memory
        // here either. Below this threshold nothing is replaced and the caller is told.
        if (CountEligiblePairs(allMethods) < 4)
            return OperationResult<int>.Fail(
                "Fewer than four adjacent same-file method pairs.",
                "INSUFFICIENT_ADJACENT_METHOD_PAIRS");

        return _methodKnn.TrainWithTargetScope(new MethodScopeMaterial(allMethods, scopes));
    }

    // One definition of adjacency for both ingresses, so they cannot drift apart:
    // neighbouring positions of the original list, same FileId, consecutive IndexInFile.
    private static int CountEligiblePairs(IReadOnlyList<PackedMethod> methods)
    {
        var eligiblePairs = 0;
        for (var index = 0; index < methods.Count - 1; index++)
        {
            if (methods[index].FileId == methods[index + 1].FileId &&
                methods[index + 1].IndexInFile == methods[index].IndexInFile + 1)
                eligiblePairs++;
        }

        return eligiblePairs;
    }

    // Train the E14 file-action KNN predictor from C# project sources.
    public void TrainFileActionKnn(IReadOnlyList<string> csharpSources)
    {
        var examples = csharpSources
            .Where(source => !string.IsNullOrWhiteSpace(source))
            // FIX-KHC transfer, plan 24-file-action-shared-parse.md, from KH-COST-01.
            // Исходник урока разбирается ОДИН раз, и одно и то же дерево уходит и в
            // признаки, и в метку. Прежде каждый из двух вызовов разбирал текст сам,
            // то есть одно дерево строилось дважды на каждый урок. Значения не
            // меняются: оба потребителя получают то же дерево того же исходника.
            // Порядок «сначала признаки, затем метка» и условие о четырёх уроках
            // ниже сохранены.
            .Select(source =>
            {
                // Единственный разбор исходника этого урока, и он просится у
                // разметчика: настройки разбора остаются у одного хозяина.
                var root = ActionLabelGenerator.Parse(source);

                // Оба потребителя получают одно и то же дерево, в прежнем порядке.
                return (Features: ActionLabelGenerator.FeatureVector(root),
                        Label: ActionLabelGenerator.Label(root));
            })
            .ToArray();

        if (examples.Length >= 4)
            _fileActionKnn.Train(examples);
    }

    // Train the opt-in E09 structural KNN predictor from C# project sources.
    public void TrainStructuralKnn(IReadOnlyList<string> csharpSources)
    {
        var pairs = new List<(StructuralVector vector, int nextToken)>();
        for (short fileId = 0; fileId < Math.Min(csharpSources.Count, short.MaxValue); fileId++)
        {
            var source = csharpSources[fileId];
            if (string.IsNullOrWhiteSpace(source))
                continue;

            var nodes = CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().ToArray();
            // FIX-AST: a separate cache per file, so sibling answers are never
            // carried across trees. Pairs of all files are still collected
            // first and handed to a single Train below.
            var cache = new SiblingFeatureCache();
            for (var index = 0; index < nodes.Length - 1; index++)
            {
                var vector = StructuralVectorExtractor.Extract(nodes[index], fileId, cache);
                var nextToken = StructuralVectorExtractor.NodeTypeId(nodes[index + 1]);
                pairs.Add((vector, nextToken));
            }
        }

        if (pairs.Count >= 10)
            _structuralKnn.Train(pairs);
    }

    // Keep C# sources in memory so completion ranking can be trained lazily from the session.
    private void RememberCompiledSource(string source, StackLabel stack)
    {
        if (stack != StackLabel.CSharp || string.IsNullOrWhiteSpace(source))
            return;

        _compiledSources.Add(source);
        _methodFrequencyDirty = true;
        _grammarMaskDirty = true;
    }

    // Build or refresh the receiver/method frequency table only when ranking is requested.
    private OperationResult<MethodFrequencyTable> EnsureMethodFrequencyTable()
    {
        _methodFrequencyTable ??= new MethodFrequencyTable();
        if (!_methodFrequencyDirty)
            return OperationResult<MethodFrequencyTable>.Ok(_methodFrequencyTable);

        if (_compiledSources.Count == 0)
            return OperationResult<MethodFrequencyTable>.Ok(_methodFrequencyTable);

        var trained = _methodFrequencyTable.Train(_compiledSources);
        if (!trained.IsSuccess)
            return OperationResult<MethodFrequencyTable>.Fail(trained.ErrorMessage, trained.ErrorCode);

        _methodFrequencyDirty = false;
        return OperationResult<MethodFrequencyTable>.Ok(_methodFrequencyTable);
    }

    // Produce E05 grammar-masked AST continuation predictions for the current C# source.
    private IReadOnlyList<int>? PredictNextKinds(string source, StackLabel stack)
    {
        if (stack != StackLabel.CSharp || _compiledSources.Count == 0)
            return null;

        var filter = EnsureGrammarMaskFilter();
        if (!filter.IsSuccess || _grammarKindToIdx is null)
            return null;

        var tokens = GrammarMaskFilter.TokenizeAstKinds(source, _grammarKindToIdx);
        if (tokens.Count < 3)
            return [];

        var predictions = new List<int>(tokens.Count - 2);
        for (var index = 2; index < tokens.Count; index++)
            predictions.Add(filter.Value!.PredictNext(tokens[index - 2], tokens[index - 1]));

        return predictions;
    }

    // Build or refresh the grammar-mask filter from all C# sources seen in this facade session.
    private OperationResult<GrammarMaskFilter> EnsureGrammarMaskFilter()
    {
        _grammarMaskFilter ??= new GrammarMaskFilter();
        if (!_grammarMaskDirty)
            return OperationResult<GrammarMaskFilter>.Ok(_grammarMaskFilter);

        var vocabulary = GrammarMaskFilter.BuildVocabulary(_compiledSources);
        if (!vocabulary.IsSuccess)
            return OperationResult<GrammarMaskFilter>.Fail(vocabulary.ErrorMessage, vocabulary.ErrorCode);

        _grammarKindToIdx = vocabulary.Value!.KindToIdx;
        var trained = _grammarMaskFilter.Train(vocabulary.Value.Tokens, _compiledSources, _grammarKindToIdx);
        if (!trained.IsSuccess)
            return OperationResult<GrammarMaskFilter>.Fail(trained.ErrorMessage, trained.ErrorCode);

        _grammarMaskDirty = false;
        return OperationResult<GrammarMaskFilter>.Ok(_grammarMaskFilter);
    }

    // Produce E09 structural KNN next-token predictions only when explicitly enabled and trained.
    private IReadOnlyList<int> PredictStructuralNextTokens(string source, StackLabel stack)
    {
        if (!_options.UseStructuralKnn || stack != StackLabel.CSharp || _structuralKnn.TrainingSize < 10)
            return [];

        var nodes = CSharpSyntaxTree.ParseText(source).GetRoot().DescendantNodes().ToArray();
        if (nodes.Length == 0)
            return [];

        return nodes
            .Select(node => _structuralKnn.PredictFromNode(node))
            .ToArray();
    }

    // Predict the E13 next method type when enabled, trained, and C# input is available.
    private string? PredictNextMethodType(string source, StackLabel stack)
    {
        if (!_options.UseMethodKnn || stack != StackLabel.CSharp || _methodKnn.TrainingSize < 4)
            return null;

        var extracted = MethodExtractor.Extract(source);
        if (!extracted.IsSuccess || extracted.Value!.Count == 0)
            return null;

        return _methodKnn.Predict(extracted.Value[^1]);
    }

    // Predict the E14 file action label when enabled, trained, and C# input is available.
    private string? PredictFileAction(string source, StackLabel stack)
    {
        if (!_options.UseFileActionKnn || stack != StackLabel.CSharp || _fileActionKnn.TrainingSize < 4)
            return null;

        var label = _fileActionKnn.Predict(ActionLabelGenerator.FeatureVector(source));
        return ActionLabelGenerator.Labels[label];
    }
}
