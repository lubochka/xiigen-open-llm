namespace Experiments;

// ─── E02: Shannon entropy of BPE tokens vs cluster IDs ────────────────────────
//
// Primary metric: compression-sparseness = 1 − H/Hmax.
// Both baseline and enhanced are normalised against the same reference ceiling
// (log2 of the BPE vocabulary size) so they are directly comparable.
// Higher sparseness = better compressibility.
//
// Baseline: Shannon entropy of the raw BPE token sequence.
// Enhanced: Shannon entropy of K-Means cluster-ID sequence (K=50).
public sealed class E02ShannonCompression
{
    private const int K = 50;

    public OperationResult<ExperimentResult> Run(IReadOnlyList<int> tokens)
    {
        if (tokens.Count < 10)
            return OperationResult<ExperimentResult>.Fail("Too few tokens for E02");

        int splitAt = (int)(tokens.Count * 0.8);
        var train = tokens.Take(splitAt).ToList();

        // ── Baseline: Shannon entropy of raw BPE tokens ─────────────────────────
        long basMemStart = GC.GetAllocatedBytesForCurrentThread();
        var  t0          = System.Diagnostics.Stopwatch.GetTimestamp();
        double baselineH  = ShannonEntropy(tokens);
        double baselineMs = ElapsedMs(t0);
        double baselineMemMb = (GC.GetAllocatedBytesForCurrentThread() - basMemStart) / 1_048_576.0;

        // ── Enhanced: cluster IDs via K-Means ───────────────────────────────────
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

        int k = Math.Min(K, seenTokens.Count);

        var features = seenTokens
            .Select(t =>
            {
                double total = tokenCounts[t];
                return cooccur[t].Select(c => c / total).ToArray();
            })
            .ToList();

        var kmeans    = new MachineLearning.KMeansAlgorithm();
        var fitResult = kmeans.Fit(features, k, maxIterations: 50);
        if (!fitResult.IsSuccess)
            return OperationResult<ExperimentResult>.Fail("K-Means: " + fitResult.ErrorMessage);

        int[] assignments = fitResult.Value!;
        var tokenToCluster = new Dictionary<int, int>();
        for (int i = 0; i < seenTokens.Count; i++)
            tokenToCluster[seenTokens[i]] = assignments[i];

        var clusterIds = tokens
            .Select(t => tokenToCluster.GetValueOrDefault(t, 0))
            .ToList();

        double enhancedH  = ShannonEntropy(clusterIds);
        double enhancedMs = ElapsedMs(t1);
        double enhancedMemMb = (GC.GetAllocatedBytesForCurrentThread() - enhMemStart) / 1_048_576.0;

        // ── Normalise both against the same reference ceiling ────────────────────
        // hMax = log2(BPE vocab size) — same denominator for fair comparison.
        // Sparseness = 1 − H/hMax: fraction of maximum entropy that is NOT used.
        // Higher = more compressible. Both are now on the same scale.
        double hMax = vocabSize > 1 ? Math.Log2(vocabSize) : 1.0;
        double baselineSparse = Math.Clamp(1.0 - baselineH / hMax, 0.0, 1.0);
        double enhancedSparse = Math.Clamp(1.0 - enhancedH / hMax, 0.0, 1.0);

        double entropyImprovePct = baselineH > 0
            ? (baselineH - enhancedH) / baselineH * 100.0
            : 0;

        return OperationResult<ExperimentResult>.Ok(new ExperimentResult
        {
            ExperimentId = "E02",
            Scale        = 0,
            Baseline = new RunMetrics
            {
                AccuracyFraction  = baselineSparse,
                TimeMs            = baselineMs,
                MemoryMb          = baselineMemMb,
                DatasetTokenCount = tokens.Count
            },
            Enhanced = new RunMetrics
            {
                AccuracyFraction  = enhancedSparse,
                TimeMs            = enhancedMs,
                MemoryMb          = enhancedMemMb,
                DatasetTokenCount = tokens.Count
            },
            Notes = $"baselineEntropy={baselineH:F3} bits, enhancedEntropy={enhancedH:F3} bits, " +
                    $"entropyImprovement={entropyImprovePct:F1}%, hMax={hMax:F2}, K={k}"
        });
    }

    public static double ShannonEntropy(IReadOnlyList<int> tokens)
    {
        if (tokens.Count == 0) return 0.0;

        var counts = new Dictionary<int, int>();
        foreach (int t in tokens)
            counts[t] = counts.GetValueOrDefault(t, 0) + 1;

        double n = tokens.Count;
        double h = 0.0;
        foreach (int c in counts.Values)
        {
            double p = c / n;
            h -= p * Math.Log2(p);
        }
        return h;
    }

    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
