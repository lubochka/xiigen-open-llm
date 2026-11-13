// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler29;

// SemanticCompilerPipeline: the complete E01-E27 five-stage pipeline.
// Stage order is MANDATORY (E20 Rule A — StackDetector before normalisation):
//   1. AstTokenizer      (E01)  — split code into AST-level tokens, preserve case
//   2. StackDetector     (E20)  — MUST be stage 2, before any normalisation
//   3. RouteClassifier   (E18)  — classify domain from predicate profile
//   4. SemanticNormalizer (E15) — stem + stop-word removal
//   5. FamilyCollapser   (E16)  — collapse families → semantic ops
//   6. LogicExtractor    (E17)  — extract logical predicates (VocabSize=22)
//   7. Decomposer        (E21)  — optional: only when prompt has multiple tasks
//   8. TemplateBeautifier (E23) — optional: only when generating code output
public sealed class SemanticCompilerPipeline : BaseService, ISemanticCompiler29
{
    // Stage 1 — tokenize raw code into syntactic units
    private readonly AstTokenizer       _astTokenizer;
    // Stage 2 — detect stack BEFORE normalisation (E20 Rule A)
    private readonly StackDetector      _stackDetector;
    // Stage 3 — classify into D-04 canonical domain
    private readonly RouteClassifier    _routeClassifier;
    // Stage 4 — normalize: stem + stop-word removal
    private readonly SemanticNormalizer _normalizer;
    // Stage 5 — collapse word families to semantic op root
    private readonly FamilyCollapser    _collapser;
    // Stage 6 — extract logical predicates (22 vocab)
    private readonly LogicExtractor     _extractor;

    // Full constructor: all stages injected (fabric-first, Rule 2).
    public SemanticCompilerPipeline(
        ICompilerStorage     storage,
        ICompilerEventLogger logger,
        AstTokenizer?        tokenizer   = null,
        StackDetector?       stack       = null,
        RouteClassifier?     router      = null,
        SemanticNormalizer?  normalizer  = null,
        FamilyCollapser?     collapser   = null,
        LogicExtractor?      extractor   = null)
        : base(storage, logger)
    {
        _astTokenizer    = tokenizer  ?? new AstTokenizer(storage, logger);
        _stackDetector   = stack      ?? new StackDetector(storage, logger);
        _routeClassifier = router     ?? new RouteClassifier(storage, logger);
        _normalizer      = normalizer ?? new SemanticNormalizer(storage, logger);
        _collapser       = collapser  ?? new FamilyCollapser(storage, logger);
        _extractor       = extractor  ?? new LogicExtractor(storage, logger);
    }

    // Run the full pipeline and return a PipelineOutput.
    // Stage order enforced by sequential assignment — grep line numbers to verify.
    public OperationResult<PipelineOutput> Compile(PipelineInput input)
    {
        if (string.IsNullOrWhiteSpace(input.Text))
            return OperationResult<PipelineOutput>.Fail("Input text is empty", "EMPTY_INPUT");

        // Stage 1: tokenize raw text (preserve case for E20).
        var tokenResult = _astTokenizer.Tokenize(input.Text);
        if (!tokenResult.IsSuccess) return tokenResult.MapError<PipelineOutput>();
        var rawTokens = tokenResult.Value!;

        // Stage 2: StackDetector on RAW text (E20 Rule A — must precede normalisation).
        string stack = _stackDetector.Detect(input.Text);

        // Stage 3: route to domain via predicate profile.
        string[] rawTokenStrings = rawTokens;
        var domain = _routeClassifier.Classify(rawTokenStrings);

        // Stage 4: normalize — lowercase, stem, remove stop words.
        string[] normalized = _normalizer.Normalize(input.Text);

        // Stage 5: collapse families to semantic ops.
        string[] collapsed  = _collapser.Collapse(normalized);

        // Stage 6: extract logical predicates.
        string[] predicates = _extractor.Extract(collapsed);

        // Compute compression ratio: original word count / predicate count.
        int originalCount  = input.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        double compression = predicates.Length == 0 ? 1.0
            : (double)originalCount / predicates.Length;

        return OperationResult<PipelineOutput>.Ok(
            new PipelineOutput(domain, predicates, stack, compression));
    }
}
