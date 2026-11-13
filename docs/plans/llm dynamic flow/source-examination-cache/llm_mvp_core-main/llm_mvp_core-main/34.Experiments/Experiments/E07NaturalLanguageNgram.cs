namespace Experiments;

// ─── E07: Bigram on natural language words vs unigram baseline ─────────────────
//
// Hypothesis: a bigram on word tokens predicts the next word far better than
// always guessing the most frequent word ("the" ≈ 5.7% of Alice).
//
// Baseline: unigram — always predict the single most-frequent word in training.
// Enhanced: bigram Markov on word tokens, same as E03 but with words not chars.
public sealed class E07NaturalLanguageNgram
{
    public OperationResult<ExperimentResult> Run(AliceDataset data, int scale = 0)
    {
        if (data.Tokens.Count < 20)
            return OperationResult<ExperimentResult>.Fail("Too few word tokens for E07");

        int split  = (int)(data.Tokens.Count * 0.8);
        var train  = data.Tokens.Take(split).ToList();
        var val    = data.Tokens.Skip(split).ToList();

        // ── Baseline: always predict most frequent word in training ──────────────
        long basMemStart = GC.GetAllocatedBytesForCurrentThread();
        var  t0          = System.Diagnostics.Stopwatch.GetTimestamp();

        var freq = new Dictionary<int, int>();
        foreach (int tok in train)
            freq[tok] = freq.GetValueOrDefault(tok, 0) + 1;
        int unigramPred = freq.Count > 0 ? freq.MaxBy(kv => kv.Value).Key : 0;

        int baseCorrect = 0, baseTotal = val.Count - 1;
        for (int i = 0; i < baseTotal; i++)
            if (val[i + 1] == unigramPred) baseCorrect++;

        double baselineAcc   = baseTotal > 0 ? baseCorrect / (double)baseTotal : 0;
        double baselineMs    = ElapsedMs(t0);
        double baselineMemMb = (GC.GetAllocatedBytesForCurrentThread() - basMemStart) / 1_048_576.0;

        // ── Enhanced: bigram on word tokens ──────────────────────────────────────
        long enhMemStart = GC.GetAllocatedBytesForCurrentThread();
        var  t1          = System.Diagnostics.Stopwatch.GetTimestamp();

        var bigram = new BigramMarkov();
        bigram.Train(train);

        int enhCorrect = 0, enhTotal = val.Count - 1;
        for (int i = 0; i < enhTotal; i++)
        {
            var pred = bigram.Predict(val[i]);
            if (pred.IsSuccess && pred.Value == val[i + 1]) enhCorrect++;
        }
        double enhancedAcc   = enhTotal > 0 ? enhCorrect / (double)enhTotal : 0;
        double enhancedMs    = ElapsedMs(t1);
        double enhancedMemMb = (GC.GetAllocatedBytesForCurrentThread() - enhMemStart) / 1_048_576.0;

        // Find the string for the unigram prediction for notes
        string topWord = data.Vocab.FirstOrDefault(kv => kv.Value == unigramPred).Key ?? "?";

        return OperationResult<ExperimentResult>.Ok(new ExperimentResult
        {
            ExperimentId = "E07",
            Scale        = 0,
            Baseline = new RunMetrics
            {
                AccuracyFraction  = baselineAcc,
                TimeMs            = baselineMs,
                MemoryMb          = baselineMemMb,
                DatasetTokenCount = data.Tokens.Count
            },
            Enhanced = new RunMetrics
            {
                AccuracyFraction  = enhancedAcc,
                TimeMs            = enhancedMs,
                MemoryMb          = enhancedMemMb,
                DatasetTokenCount = data.Tokens.Count
            },
            Notes = $"words={data.Tokens.Count}, vocab={data.Vocab.Count}, " +
                    $"topWord='{topWord}'({baselineAcc * 100:F1}%), " +
                    $"bigramAcc={enhancedAcc * 100:F1}%"
        });
    }

    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
