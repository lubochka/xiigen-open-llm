namespace Experiments;

// ─── E03: Trigram (context-2) vs bigram (context-1) Markov chain ──────────────

// Hypothesis: extending the Markov context from 1 preceding token (bigram) to
// 2 preceding tokens (trigram) captures more of the deterministic structure of
// C# code and improves next-token prediction accuracy by 5–15%.
//
// At Scale 0 the vocabulary is small enough that the trigram key dictionary stays
// tractable.  A bigram fallback handles unseen (prev2, prev1) pairs.
//
// Baseline: order-1 bigram Markov P(next | prev).
// Enhanced: order-2 trigram Markov P(next | prev2, prev1) with bigram fallback.
public sealed class E03SparseWindow
{
    // Run Scale-0 experiment on the provided token sequence.
    public OperationResult<ExperimentResult> Run(IReadOnlyList<int> tokens)
    {
        if (tokens.Count < 10)
            return OperationResult<ExperimentResult>.Fail("Too few tokens for E03");

        int splitAt = (int)(tokens.Count * 0.8);
        var train = tokens.Take(splitAt).ToList();
        var val   = tokens.Skip(splitAt).ToList();

        // ── Baseline: bigram ─────────────────────────────────────────────────────
        long basMemStart = GC.GetAllocatedBytesForCurrentThread();
        var t0 = System.Diagnostics.Stopwatch.GetTimestamp();
        var bigram = new BigramMarkov();
        bigram.Train(train);

        // Evaluate bigram on the validation set using bigram pairs.
        int baseCorrect = 0, baseTotal = val.Count - 1;
        for (int i = 0; i < baseTotal; i++)
        {
            var pred = bigram.Predict(val[i]);
            if (pred.IsSuccess && pred.Value == val[i + 1]) baseCorrect++;
        }
        double baselineAcc = baseTotal > 0 ? baseCorrect / (double)baseTotal : 0.0;
        double baselineMs  = ElapsedMs(t0);
        double baselineMemMb = (GC.GetAllocatedBytesForCurrentThread() - basMemStart) / 1_048_576.0;

        // ── Enhanced: trigram with bigram fallback ───────────────────────────────
        long enhMemStart = GC.GetAllocatedBytesForCurrentThread();
        var t1      = System.Diagnostics.Stopwatch.GetTimestamp();
        var trigram = new TrigramMarkov();
        trigram.Train(train);

        // Evaluate trigram on the validation set using trigram pairs.
        // The first element can only be predicted with a bigram (prev2 unknown).
        int enhCorrect = 0, enhTotal = val.Count - 2;
        for (int i = 0; i < enhTotal; i++)
        {
            var pred = trigram.Predict(val[i], val[i + 1]);
            if (pred.IsSuccess && pred.Value == val[i + 2]) enhCorrect++;
        }
        double enhancedAcc = enhTotal > 0 ? enhCorrect / (double)enhTotal : 0.0;
        double enhancedMs  = ElapsedMs(t1);
        double enhancedMemMb = (GC.GetAllocatedBytesForCurrentThread() - enhMemStart) / 1_048_576.0;

        return OperationResult<ExperimentResult>.Ok(new ExperimentResult
        {
            ExperimentId = "E03",
            Scale        = 0,
            Baseline = new RunMetrics
            {
                AccuracyFraction  = baselineAcc,
                TimeMs            = baselineMs,
                MemoryMb          = baselineMemMb,
                DatasetTokenCount = tokens.Count
            },
            Enhanced = new RunMetrics
            {
                AccuracyFraction  = enhancedAcc,
                TimeMs            = enhancedMs,
                MemoryMb          = enhancedMemMb,
                DatasetTokenCount = tokens.Count
            },
            Notes = $"bigramVocab={bigram.VocabSize}, trainTokens={train.Count}"
        });
    }

    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
