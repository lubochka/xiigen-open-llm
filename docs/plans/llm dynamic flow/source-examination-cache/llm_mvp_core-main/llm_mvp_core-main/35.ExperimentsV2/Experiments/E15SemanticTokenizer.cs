namespace ExperimentsV2;

// E15: semantic tokenizer — stopword removal + dictionary-free stemming.
//
// Hypothesis: stripping stopwords and collapsing inflected forms (Level1) reduces
// the vocabulary while preserving semantic content, yielding lower bigram perplexity
// than raw identifier words (Level0).
//
// Baseline:  bigram on Level0Tokens (raw camelCase-split identifier words)
// Enhanced:  bigram on Level1Tokens (stopword removal + suffix stemming)
// Primary metric: perplexity (lower is better)
// Scaling signal: vocab_ratio = vocab_size / token_count  (lower = more reuse)
public sealed class E15SemanticTokenizer
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("No files for E15");

        string combined = string.Join("\n", files.Select(f => f.Text));

        var level0 = SemanticTokenizerPipeline.Level0Tokens(combined);
        var level1 = SemanticTokenizerPipeline.Level1Tokens(combined);

        if (level0.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Too few Level0 tokens: {level0.Count}. Need ≥ 10.");
        if (level1.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Too few Level1 tokens: {level1.Count}. Need ≥ 10.");

        // ── Baseline: bigram on Level0 ────────────────────────────────────────
        var t0 = Stopwatch();
        double baselinePerp = RunBigramPerplexity(level0);
        double baselineMs   = ElapsedMs(t0);

        // ── Enhanced: bigram on Level1 ────────────────────────────────────────
        var t1 = Stopwatch();
        double enhancedPerp = RunBigramPerplexity(level1);
        double enhancedMs   = ElapsedMs(t1);

        var (_, vocab0) = SemanticTokenizerPipeline.Encode(level0);
        var (_, vocab1) = SemanticTokenizerPipeline.Encode(level1);
        double vocabRatio = vocab1.Count / (double)Math.Max(level1.Count, 1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E15-SEMANTIC-TOKENIZER",
            Tier              = tier,
            BaselinePrimary   = baselinePerp,
            EnhancedPrimary   = enhancedPerp,
            PrimaryMetricName = "perplexity",
            LowerIsBetter     = true,
            ScalingSignal     = vocabRatio,
            ScalingSignalNote = "vocab_ratio = vocab/tokens (< 0.2 = good reuse; > 0.5 → scale up)",
            ShouldScaleUp     = vocabRatio > 0.5 || level1.Count < 50,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"files={files.Count}, " +
                    $"level0tokens={level0.Count}, vocab0={vocab0.Count}, " +
                    $"level1tokens={level1.Count}, vocab1={vocab1.Count}, " +
                    $"vocabRatio={vocabRatio:F3}"
        });
    }

    // Encode train tokens; map val OOV to unkId so BigramModelV2 can cover them
    // via Laplace smoothing instead of collapsing to epsilon.
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
