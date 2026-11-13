namespace ExperimentsV2;

// E34v2: EpisodeQueryMatcher — BM25 (E15 words) + K-NN (E16 tags) rank fusion.
// Baseline v2:  BM25 only on E15-normalized words (beats E16-tags-only)
// Enhanced v2:  rank fusion = α × BM25_score + (1-α) × KNN_jaccard_score
//               α tuned automatically on first half of validation
// Metric:       hit@1 (correct episode at rank 1)
public sealed class E34HybridMatcher
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var data     = EpisodeCorpusLoader.Expand();
        var rng      = new Random(42);
        var shuffled = data.OrderBy(_ => rng.Next()).ToList();
        int split    = (int)(shuffled.Count * 0.8);
        var train    = shuffled.Take(split).ToList();
        var val      = shuffled.Skip(split).ToList();

        var typeToId = data.Select(x => x.Type).Distinct()
            .Select((t, i) => (t, i)).ToDictionary(x => x.t, x => x.i);

        // ── Baseline: BM25 only ───────────────────────────────────────────────
        var t0 = Stopwatch();
        var bm25 = new BM25EpisodeMatcher();
        bm25.Index(train.Select(x => (x.Prompt, typeToId[x.Type])).ToList());

        int base1 = 0, base3 = 0;
        foreach (var (prompt, _, type) in val)
        {
            int expected = typeToId[type];
            var results  = bm25.Search(prompt, topK: 3);
            if (results.Count > 0 && results[0].EpisodeId == expected) base1++;
            if (results.Any(r => r.EpisodeId == expected))             base3++;
        }
        double baseAcc1   = base1 / (double)val.Count;
        double baseAcc3   = base3 / (double)val.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: BM25 + K-NN rank fusion ────────────────────────────────
        var t1 = Stopwatch();
        double bestAlpha = TuneAlpha(train, val.Take(val.Count / 2).ToList(), typeToId, bm25);

        int enh1 = 0, enh3 = 0;
        var valSecond = val.Skip(val.Count / 2).ToList();
        foreach (var (prompt, _, type) in valSecond)
        {
            int expected   = typeToId[type];
            var fusedRanks = FusedRank(prompt, train, typeToId, bm25, bestAlpha, topK: 3);
            if (fusedRanks.Count > 0 && fusedRanks[0].EpisodeId == expected) enh1++;
            if (fusedRanks.Any(r => r.EpisodeId == expected))                 enh3++;
        }
        double enhAcc1    = valSecond.Count > 0 ? enh1 / (double)valSecond.Count : 0;
        double enhAcc3    = valSecond.Count > 0 ? enh3 / (double)valSecond.Count : 0;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E34V2-HYBRID-MATCHER",
            Tier              = tier,
            BaselinePrimary   = baseAcc1,
            EnhancedPrimary   = enhAcc1,
            PrimaryMetricName = "hit_at_1",
            LowerIsBetter     = false,
            ScalingSignal     = enhAcc3,
            ScalingSignalNote = "hit@3 (достаточно для оркестратора)",
            ShouldScaleUp     = val.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"trainN={train.Count}, valN={val.Count}, bestAlpha={bestAlpha:F2}, " +
                    $"baseHit1={baseAcc1:P1} hit3={baseAcc3:P1}, " +
                    $"enhHit1={enhAcc1:P1} hit3={enhAcc3:P1}"
        });
    }

    private static List<(int EpisodeId, double Score)> FusedRank(
        string query,
        List<(string Prompt, int Count, string Type)> train,
        Dictionary<string, int> typeToId,
        BM25EpisodeMatcher bm25,
        double alpha,
        int topK)
    {
        var bm25Results = bm25.Search(query, topK: typeToId.Count);
        double maxBm25  = bm25Results.Count > 0 ? bm25Results[0].Score : 1.0;
        var bm25Scores  = bm25Results.ToDictionary(
            r => r.EpisodeId,
            r => maxBm25 > 0 ? r.Score / maxBm25 : 0.0);

        var qTags     = FamilyCollapser.CollapseString(query).ToHashSet();
        var knnScores = new Dictionary<int, double>();
        foreach (var (prompt, _, type) in train)
        {
            int    epId  = typeToId[type];
            var    tTags = FamilyCollapser.CollapseString(prompt).ToHashSet();
            double sim   = JaccardSim(qTags, tTags);
            knnScores[epId] = Math.Max(knnScores.GetValueOrDefault(epId, 0), sim);
        }

        var allIds = bm25Scores.Keys.Union(knnScores.Keys).ToHashSet();
        return allIds
            .Select(id => (
                EpisodeId: id,
                Score: alpha       * bm25Scores.GetValueOrDefault(id, 0)
                     + (1 - alpha) * knnScores.GetValueOrDefault(id, 0)))
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .ToList();
    }

    private static double TuneAlpha(
        List<(string Prompt, int Count, string Type)> train,
        List<(string Prompt, int Count, string Type)> val,
        Dictionary<string, int> typeToId,
        BM25EpisodeMatcher bm25)
    {
        double bestAlpha = 0.5;
        int    bestHits  = 0;
        for (double a = 0.1; a <= 0.9; a += 0.1)
        {
            int hits = val.Count(v =>
            {
                int expected = typeToId[v.Type];
                var results  = FusedRank(v.Prompt, train, typeToId, bm25, a, topK: 1);
                return results.Count > 0 && results[0].EpisodeId == expected;
            });
            if (hits > bestHits) { bestHits = hits; bestAlpha = a; }
        }
        return bestAlpha;
    }

    private static double JaccardSim(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return 1.0;
        int inter = a.Count(x => b.Contains(x));
        int union = a.Count + b.Count - inter;
        return union == 0 ? 0 : (double)inter / union;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
