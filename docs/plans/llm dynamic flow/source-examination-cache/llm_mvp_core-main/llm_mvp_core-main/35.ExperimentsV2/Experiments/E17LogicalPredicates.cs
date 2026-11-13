namespace ExperimentsV2;

// E17: logical predicates — replace identifier-based tokens with 12 fixed AST predicates.
//
// Hypothesis: predicate sequences (NULL_GUARD, ITERATE, HANDLE_ERROR, …) capture
// the control-flow skeleton of code. Bigram over this minimal vocabulary achieves
// lower perplexity than synonym-collapsed identifier tokens (Level2), because the
// vocabulary is small enough for the bigram to be well-populated at small scale.
//
// Baseline:  bigram on Level2Tokens (synonym ops — E16 enhanced)
// Enhanced:  bigram on Level3Tokens (12 logical predicates from AST roles)
// Primary metric: perplexity (lower is better)
// Scaling signal: predicate_density = predicates / AST nodes (~0.05–0.15 is typical)
public sealed class E17LogicalPredicates
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("No files for E17");

        string combined = string.Join("\n", files.Select(f => f.Text));

        var level2 = SemanticTokenizerPipeline.Level2Tokens(combined);
        var level3 = SemanticTokenizerPipeline.Level3Tokens(combined);

        if (level2.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Too few Level2 tokens: {level2.Count}. Need ≥ 10.");

        // Level3 falls back to Level2 when < 4 predicates were found —
        // report the fallback in notes but treat it as valid.
        bool usedFallback = level3.Count > 0 &&
                            level3.All(t => !IsKnownPredicate(t));

        // ── Baseline: bigram on Level2 ────────────────────────────────────────
        var t0 = Stopwatch();
        double baselinePerp = RunBigramPerplexity(level2);
        double baselineMs   = ElapsedMs(t0);

        // ── Enhanced: bigram on Level3 ────────────────────────────────────────
        var t1 = Stopwatch();
        double enhancedPerp = RunBigramPerplexity(level3);
        double enhancedMs   = ElapsedMs(t1);
        var (_, vocab3) = SemanticTokenizerPipeline.Encode(level3);

        int predCount = level3.Count(IsKnownPredicate);
        double density = level3.Count > 0 ? predCount / (double)level3.Count : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E17-LOGICAL-PREDICATES",
            Tier              = tier,
            BaselinePrimary   = baselinePerp,
            EnhancedPrimary   = enhancedPerp,
            PrimaryMetricName = "perplexity",
            LowerIsBetter     = true,
            ScalingSignal     = density,
            ScalingSignalNote = "predicate_density = predicates/level3 (< 0.3 = sparse → scale up)",
            ShouldScaleUp     = density < 0.3 || level3.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"files={files.Count}, " +
                    $"level2tokens={level2.Count}, level3tokens={level3.Count}, " +
                    $"vocab3={vocab3.Count}, predCount={predCount}, density={density:F3}" +
                    (usedFallback ? " [fallback to Level2]" : "")
        });
    }

    private static readonly HashSet<string> KnownPredicates = new(StringComparer.Ordinal)
    {
        "NULL_GUARD","FAIL","ITERATE","HANDLE_ERROR","AWAIT","THROW",
        "DECLARE","ASSIGN","CREATE","CALL","BRANCH","RETURN"
    };

    private static bool IsKnownPredicate(string t) => KnownPredicates.Contains(t);

    private static double RunBigramPerplexity(List<string> tokens)
    {
        if (tokens.Count < 4) return double.MaxValue;
        int split      = (int)(tokens.Count * 0.8);
        var trainToks  = tokens.Take(split).ToList();
        var valToks    = tokens.Skip(split).ToList();
        if (valToks.Count < 2) return double.MaxValue;

        var (trainEnc, trainVocab) = SemanticTokenizerPipeline.Encode(trainToks);
        int unkId = trainVocab.Count;
        var valEnc = valToks
            .Select(t => trainVocab.TryGetValue(t, out int id) ? id : unkId)
            .ToList();

        var model   = new BigramModelV2();
        var trainOp = model.Train(trainEnc, vocabSizeHint: unkId + 1);
        if (!trainOp.IsSuccess) return double.MaxValue;

        var evalOp = model.Evaluate(valEnc);
        return evalOp.IsSuccess ? evalOp.Value!.perplexity : double.MaxValue;
    }

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
