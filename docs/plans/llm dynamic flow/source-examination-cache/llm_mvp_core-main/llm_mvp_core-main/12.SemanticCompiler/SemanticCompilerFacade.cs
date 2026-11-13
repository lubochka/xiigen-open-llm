namespace SemanticCompiler;

// SemanticCompilerFacade wires the full E01-E27 pipeline: Compression → Routing →
// Planning → Translation → Beautification.
// Entry point: Compile(source) → CompilationResult.
public sealed class SemanticCompilerFacade : ISemanticCompiler
{
    private readonly StackDetector      _stackDetector;
    private readonly RouteClassifier    _routeClassifier;
    private readonly Decomposer         _decomposer;

    public SemanticCompilerFacade(
        StackDetector?   stackDetector   = null,
        RouteClassifier? routeClassifier = null,
        Decomposer?      decomposer      = null)
    {
        _stackDetector   = stackDetector   ?? new StackDetector();
        _routeClassifier = routeClassifier ?? new RouteClassifier();
        _decomposer      = decomposer      ?? new Decomposer();
    }

    // ── ISemanticCompiler implementation ───────────────────────────────────────

    public OperationResult<StackLabel> DetectStack(string source)
        => _stackDetector.Detect(source);

    public OperationResult<DomainLabel> ClassifyDomain(string prompt)
    {
        var lower = prompt.ToLowerInvariant();
        // Keyword fast-path (no training required for production use)
        return OperationResult<DomainLabel>.Ok(RouteClassifier.ClassifyByKeywords(lower));
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

        var result = new CompilationResult(
            Stack:           stackResult.Value,
            Domain:          domain,
            Predicates:      predicatesResult.Value!,
            Ops:             opsResult.Value!,
            NormalizedWords: normalizedResult.Value!,
            Segments:        segmentsResult.Value!);

        return OperationResult<CompilationResult>.Ok(result);
    }
}
