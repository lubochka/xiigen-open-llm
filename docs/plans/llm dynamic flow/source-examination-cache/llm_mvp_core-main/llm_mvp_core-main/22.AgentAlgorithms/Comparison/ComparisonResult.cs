namespace AgentAlgorithms;

// Metrics from a side-by-side run of original vs enhanced context assembly.
public sealed record ComparisonResult
{
    // ── Original variant (ISemanticSearchProvider + raw file text) ──────────────
    public int    OriginalTokensUsed     { get; init; }
    public int    OriginalFilesInContext  { get; init; }
    public int    OriginalLlmCallCount   { get; init; }
    public double OriginalRankingTimeMs  { get; init; }

    // ── Enhanced variant (AstRelevanceRanker/BM25 + AstPackedContextBuilder) ────
    public int    EnhancedTokensUsed     { get; init; }
    public int    EnhancedFilesInContext  { get; init; }
    public int    EnhancedLlmCallCount   { get; init; }
    public double EnhancedRankingTimeMs  { get; init; }

    // ── AST-packing metrics ───────────────────────────────────────────────────
    public int    EnhancedFilesViaPacking  { get; init; }  // files that fit via AST packing
    public double AstPackingRatio          { get; init; }  // original chars / packed chars

    // ── Processing speed (E11: AST tokens give 2.7× faster processing) ───────
    // Total pipeline time: packing + BM25 ranking + context assembly.
    public double ProcessingSpeedMs { get; init; }

    // ── Roslyn validation (set by callers that also use RoslynSyntaxValidator) ──
    public int    SyntaxErrorsCaughtBeforeRetry { get; init; }

    // ── Context texts (passed to LlmJudge) ───────────────────────────────────
    public string OriginalContextText  { get; init; } = "";
    public string EnhancedContextText  { get; init; } = "";
    public string TaskDescription      { get; init; } = "";

    // ── v4.0 new metrics (E18 / E20 / E21 / E23 / E25-27) ───────────────────
    public bool   StackDetectedCorrectly    { get; init; }       // E20: language detected before normalization
    public double TaskBoundaryAccuracy      { get; init; }       // E21: ≥ 0.9 (RoleBasedTaskDecomposer v2)
    public double DomainClassAccuracy       { get; init; }       // E18: ≥ 0.9 (RouteClassifier)
    public double TemplateMatchRate         { get; init; }       // E23: 1.0 (TemplateBeautifier)
    public double PromptTranslExactMatch    { get; init; }       // E25-27: ≥ 0.9 (PromptTranslator)
}

// Verdict returned by LlmJudge after comparing the two context variants.
public sealed record JudgmentResult
{
    public string Winner          { get; init; } = "";   // "A", "B", or "tie"
    public string RelevanceWinner { get; init; } = "";
    public string CoverageWinner  { get; init; } = "";
    public string ClarityWinner   { get; init; } = "";
    public string Reasoning       { get; init; } = "";
}
