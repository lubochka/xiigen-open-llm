namespace Experiments;

// ─── E01: Semantic token families via K-Means clustering ──────────────────────
//
// Baseline: bigram Markov on raw BPE integer token IDs.
// Enhanced: K-Means (K=50) on one-hot frequency vectors → bigram on cluster IDs.
//
// Adaptive K: requires at least 50 unique tokens (K = min(50, uniqueTokens/5)).
// If uniqueTokens < 50, Scale 0 is too small → return Fail, recommend Scale 1.
public sealed class E01SemanticFamilies
{
    private const int K = 50;

    public OperationResult<ExperimentResult> Run(IReadOnlyList<int> tokens)
    {
        if (tokens.Count < 10)
            return OperationResult<ExperimentResult>.Fail("Too few tokens for E01");

        int splitAt = (int)(tokens.Count * 0.8);
        var train = tokens.Take(splitAt).ToList();
        var val   = tokens.Skip(splitAt).ToList();

        // ── Baseline: raw bigram on BPE token IDs ──────────────────────────────
        long basMemStart = GC.GetAllocatedBytesForCurrentThread();
        var t0 = System.Diagnostics.Stopwatch.GetTimestamp();
        var baselineMarkov = new BigramMarkov();
        var trainResult = baselineMarkov.Train(train);
        if (!trainResult.IsSuccess)
            return OperationResult<ExperimentResult>.Fail("Baseline train: " + trainResult.ErrorMessage);

        var baselineAcc = baselineMarkov.Evaluate(val);
        if (!baselineAcc.IsSuccess)
            return OperationResult<ExperimentResult>.Fail("Baseline eval: " + baselineAcc.ErrorMessage);

        double baselineMs  = ElapsedMs(t0);
        double baselineMemMb = (GC.GetAllocatedBytesForCurrentThread() - basMemStart) / 1_048_576.0;

        // ── Enhanced: K-Means clustering → bigram on cluster IDs ──────────────
        long enhMemStart = GC.GetAllocatedBytesForCurrentThread();
        var t1 = System.Diagnostics.Stopwatch.GetTimestamp();

        int vocabSize = train.Max() + 1;
        var tokenCounts = new int[vocabSize];
        var cooccur     = new int[vocabSize][];
        for (int i = 0; i < vocabSize; i++) cooccur[i] = new int[vocabSize];

        for (int i = 0; i < train.Count; i++)
        {
            tokenCounts[train[i]]++;
            if (i < train.Count - 1) cooccur[train[i]][train[i + 1]]++;
        }

        var seenTokens = Enumerable.Range(0, vocabSize)
            .Where(t => tokenCounts[t] > 0)
            .ToList();

        // Adaptive K: needs at least 5 tokens per cluster on average.
        int adaptiveK = Math.Max(5, Math.Min(K, seenTokens.Count / 5));
        if (adaptiveK < 10)
            return OperationResult<ExperimentResult>.Fail(
                $"Scale 0 too small for K-Means: need ≥50 unique tokens, got {seenTokens.Count}. " +
                "Run at Scale 1.");

        int k = adaptiveK;

        var featureVectors = seenTokens
            .Select(t =>
            {
                double total = tokenCounts[t];
                return cooccur[t].Select(c => c / total).ToArray();
            })
            .ToList();

        var kmeans = new MachineLearning.KMeansAlgorithm();
        var fitResult = kmeans.Fit(featureVectors, k, maxIterations: 50);
        if (!fitResult.IsSuccess)
            return OperationResult<ExperimentResult>.Fail("K-Means: " + fitResult.ErrorMessage);

        int[] assignments = fitResult.Value!;
        var tokenToCluster = new Dictionary<int, int>();
        for (int i = 0; i < seenTokens.Count; i++)
            tokenToCluster[seenTokens[i]] = assignments[i];

        var trainClusters = train.Select(t => tokenToCluster.GetValueOrDefault(t, 0)).ToList();
        var valClusters   = val.Select(t => tokenToCluster.GetValueOrDefault(t, 0)).ToList();

        var enhancedMarkov = new BigramMarkov();
        var enhTrainResult = enhancedMarkov.Train(trainClusters);
        if (!enhTrainResult.IsSuccess)
            return OperationResult<ExperimentResult>.Fail("Enhanced train: " + enhTrainResult.ErrorMessage);

        var enhancedAcc = enhancedMarkov.Evaluate(valClusters);
        if (!enhancedAcc.IsSuccess)
            return OperationResult<ExperimentResult>.Fail("Enhanced eval: " + enhancedAcc.ErrorMessage);

        double enhancedMs  = ElapsedMs(t1);
        double enhancedMemMb = (GC.GetAllocatedBytesForCurrentThread() - enhMemStart) / 1_048_576.0;

        return OperationResult<ExperimentResult>.Ok(new ExperimentResult
        {
            ExperimentId = "E01",
            Scale        = 0,
            Baseline = new RunMetrics
            {
                AccuracyFraction  = baselineAcc.Value,
                TimeMs            = baselineMs,
                MemoryMb          = baselineMemMb,
                DatasetTokenCount = tokens.Count
            },
            Enhanced = new RunMetrics
            {
                AccuracyFraction  = enhancedAcc.Value,
                TimeMs            = enhancedMs,
                MemoryMb          = enhancedMemMb,
                DatasetTokenCount = tokens.Count
            },
            Notes = $"K={k}, adaptiveK={adaptiveK}, vocabSize={vocabSize}, seenTokens={seenTokens.Count}"
        });
    }

    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
