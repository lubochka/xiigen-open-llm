namespace ExperimentsV2;

// E16: synonym collapse — map stemmed tokens to ~10 semantic op categories.
//
// Hypothesis: collapsing synonyms to a fixed op vocabulary (GET_OP, CREATE_OP, …)
// removes surface-form noise and lowers bigram perplexity compared to stemmed-only
// tokens (Level1), because op sequences encode consistent intent patterns.
//
// Baseline:  bigram on Level1Tokens (stop+stem, no collapse)
// Enhanced:  bigram on Level2Tokens (synonym-collapsed to semantic ops)
// Primary metric: perplexity (lower is better)
// Scaling signal: op_coverage = fraction of tokens that mapped to an op
public sealed class E16SynonymCollapse
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("No files for E16");

        string combined = string.Join("\n", files.Select(f => f.Text));

        var level1 = SemanticTokenizerPipeline.Level1Tokens(combined);
        var level2 = SemanticTokenizerPipeline.Level2Tokens(combined);

        if (level1.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Too few Level1 tokens: {level1.Count}. Need ≥ 10.");

        // ── Baseline: bigram on Level1 ────────────────────────────────────────
        var t0 = Stopwatch();
        double baselinePerp = RunBigramPerplexity(level1);
        double baselineMs   = ElapsedMs(t0);

        // ── Enhanced: bigram on Level2 ────────────────────────────────────────
        var t1 = Stopwatch();
        double enhancedPerp = RunBigramPerplexity(level2);
        double enhancedMs   = ElapsedMs(t1);
        var (_, vocab2) = SemanticTokenizerPipeline.Encode(level2);

        // Op coverage: fraction of tokens that are in the _OP suffix set
        int opCount     = level2.Count(t => t.EndsWith("_OP", StringComparison.Ordinal));
        double coverage = level2.Count > 0 ? opCount / (double)level2.Count : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E16-SYNONYM-COLLAPSE",
            Tier              = tier,
            BaselinePrimary   = baselinePerp,
            EnhancedPrimary   = enhancedPerp,
            PrimaryMetricName = "perplexity",
            LowerIsBetter     = true,
            ScalingSignal     = coverage,
            ScalingSignalNote = "op_coverage = ops/tokens (< 0.05 → mostly non-op code; > 0.5 = good signal)",
            ShouldScaleUp     = coverage < 0.02 || level2.Count < 50,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"files={files.Count}, " +
                    $"level1tokens={level1.Count}, level2tokens={level2.Count}, " +
                    $"vocab2={vocab2.Count}, opCount={opCount}, opCoverage={coverage:P1}"
        });
    }

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
