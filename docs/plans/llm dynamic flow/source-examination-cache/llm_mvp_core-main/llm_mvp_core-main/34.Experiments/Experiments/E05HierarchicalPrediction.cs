namespace Experiments;

// ─── E05: Two-level hierarchical Markov chain ─────────────────────────────────
//
// Baseline: single-level bigram Markov on raw BPE token IDs.
// Enhanced: two-level hierarchical Markov using K-Means clusters.
//
// Requires vocabSize ≥ 100 to be meaningful (two levels need enough tokens to
// separate into distinct clusters). At Scale 0 (< 100 unique tokens), returns
// Fail and recommends Scale 1.
//
// Adaptive K: K = min(50, uniqueTokens / 5) to guarantee ≥5 tokens per cluster.
public sealed class E05HierarchicalPrediction
{
    private const int K = 50;

    public OperationResult<ExperimentResult> Run(IReadOnlyList<int> tokens)
    {
        if (tokens.Count < 10)
            return OperationResult<ExperimentResult>.Fail("Too few tokens for E05");

        int splitAt = (int)(tokens.Count * 0.8);
        var train = tokens.Take(splitAt).ToList();
        var val   = tokens.Skip(splitAt).ToList();

        // ── Baseline: flat bigram ─────────────────────────────────────────────────
        long basMemStart = GC.GetAllocatedBytesForCurrentThread();
        var t0 = System.Diagnostics.Stopwatch.GetTimestamp();
        var bigram = new BigramMarkov();
        bigram.Train(train);

        int baseCorrect = 0, baseTotal = val.Count - 1;
        for (int i = 0; i < baseTotal; i++)
        {
            var pred = bigram.Predict(val[i]);
            if (pred.IsSuccess && pred.Value == val[i + 1]) baseCorrect++;
        }
        double baselineAcc = baseTotal > 0 ? baseCorrect / (double)baseTotal : 0.0;
        double baselineMs  = ElapsedMs(t0);
        double baselineMemMb = (GC.GetAllocatedBytesForCurrentThread() - basMemStart) / 1_048_576.0;

        // ── Guard: must have enough distinct tokens for a meaningful two-level hierarchy ──
        // vocabSize would be 128 for ASCII — always passes. Check seenTokens instead.
        int vocabSize = train.Max() + 1;
        int seenCount = new HashSet<int>(train).Count;
        if (seenCount < 50)
            return OperationResult<ExperimentResult>.Fail(
                $"E05 нужно ≥50 уникальных токенов, нашли {seenCount}. " +
                "С BPE токенизатором этот guard не будет срабатывать.");

        // ── Enhanced: two-level hierarchical Markov ───────────────────────────────
        long enhMemStart = GC.GetAllocatedBytesForCurrentThread();
        var t1 = System.Diagnostics.Stopwatch.GetTimestamp();

        var tokenCounts = new int[vocabSize];
        var cooccur     = new int[vocabSize][];
        for (int i = 0; i < vocabSize; i++) cooccur[i] = new int[vocabSize];

        for (int i = 0; i < train.Count; i++)
        {
            tokenCounts[train[i]]++;
            if (i < train.Count - 1) cooccur[train[i]][train[i + 1]]++;
        }

        var seenTokens = Enumerable.Range(0, vocabSize)
            .Where(t => tokenCounts[t] > 0).ToList();

        int k = Math.Max(5, Math.Min(K, seenTokens.Count / 5));

        var features = seenTokens
            .Select(t =>
            {
                double tot = tokenCounts[t];
                return cooccur[t].Select(c => c / tot).ToArray();
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

        var trainClusters = train.Select(t => tokenToCluster.GetValueOrDefault(t, 0)).ToList();

        var clusterBigram = new BigramMarkov();
        clusterBigram.Train(trainClusters);

        var clusterMembers = new Dictionary<int, List<int>>();
        for (int c = 0; c < k; c++) clusterMembers[c] = [];
        foreach (var t in seenTokens)
            clusterMembers[tokenToCluster[t]].Add(t);

        var tokenToLocal = new Dictionary<int, int>();
        foreach (var (c, members) in clusterMembers)
            for (int idx = 0; idx < members.Count; idx++)
                tokenToLocal[members[idx]] = idx;

        var withinBigrams = new BigramMarkov?[k];
        for (int c = 0; c < k; c++)
        {
            var localSeq = new List<int>();
            for (int i = 0; i < train.Count; i++)
            {
                if (tokenToCluster.GetValueOrDefault(train[i], -1) == c)
                    localSeq.Add(tokenToLocal[train[i]]);
            }
            if (localSeq.Count >= 2)
            {
                var wb = new BigramMarkov();
                wb.Train(localSeq);
                withinBigrams[c] = wb;
            }
        }

        int enhCorrect = 0, enhTotal = val.Count - 1;
        for (int i = 0; i < enhTotal; i++)
        {
            int curToken   = val[i];
            int curCluster = tokenToCluster.GetValueOrDefault(curToken, 0);

            var clusterPred = clusterBigram.Predict(curCluster);
            if (!clusterPred.IsSuccess) continue;
            int predCluster = clusterPred.Value;

            var wb = predCluster < withinBigrams.Length ? withinBigrams[predCluster] : null;
            int predLocal = 0;
            if (wb is not null)
            {
                int curLocal = tokenToLocal.GetValueOrDefault(curToken, 0);
                var localPred = wb.Predict(curLocal);
                if (localPred.IsSuccess) predLocal = localPred.Value;
            }

            if (!clusterMembers.TryGetValue(predCluster, out var mems)) continue;
            if (predLocal >= 0 && predLocal < mems.Count)
            {
                if (mems[predLocal] == val[i + 1]) enhCorrect++;
            }
        }

        double enhancedAcc = enhTotal > 0 ? enhCorrect / (double)enhTotal : 0.0;
        double enhancedMs  = ElapsedMs(t1);
        double enhancedMemMb = (GC.GetAllocatedBytesForCurrentThread() - enhMemStart) / 1_048_576.0;

        return OperationResult<ExperimentResult>.Ok(new ExperimentResult
        {
            ExperimentId = "E05",
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
            Notes = $"K={k}, vocabSize={vocabSize}, seenTokens={seenTokens.Count}"
        });
    }

    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
