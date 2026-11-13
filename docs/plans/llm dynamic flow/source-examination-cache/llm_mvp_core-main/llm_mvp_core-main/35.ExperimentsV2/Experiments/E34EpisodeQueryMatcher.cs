namespace ExperimentsV2;

// E34: EpisodeQueryMatcher — match a new prompt to the closest known episode.
// Training:  90 examples (10 episodes × 9 phrasings) from EPISODE-QUERY-MAPPING
// Baseline:  keyword matching (domain-specific words → episode id)
// Enhanced:  K-NN on FamilyCollapser tags
// Metric:    hit@1 (10 classes, random baseline = 10%)
public sealed class E34EpisodeQueryMatcher
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var expanded = EpisodeCorpusLoader.Expand();
        int total    = expanded.Count;
        var data     = expanded
            .Select((x, i) => (x.Prompt, x.SubEpisodeCount, x.Type,
                EpisodeId: EpisodeIdFromIndex(i, total)))
            .ToList();

        var rng      = new Random(42);
        var shuffled = data.OrderBy(_ => rng.Next()).ToList();
        int split    = (int)(shuffled.Count * 0.8);
        var train    = shuffled.Take(split).ToList();
        var val      = shuffled.Skip(split).ToList();

        var trainK = train.Select(x => (
            Tags:  FamilyCollapser.CollapseString(x.Prompt),
            EpId:  x.EpisodeId)).ToList();

        int PredictKnn(string prompt)
        {
            var qTags = FamilyCollapser.CollapseString(prompt);
            return trainK
                .OrderBy(t => TagDist(qTags, t.Tags))
                .Take(3)
                .GroupBy(t => t.EpId)
                .OrderByDescending(g => g.Count())
                .First().Key;
        }

        int PredictKeyword(string prompt)
        {
            var lower = prompt.ToLowerInvariant();
            if (lower.Contains("modules") || lower.Contains("coupled") || lower.Contains("tangled")) return 3;
            if (lower.Contains("business") || lower.Contains("architecture") || lower.Contains("diagrams")) return 1;
            if (lower.Contains("generate") || lower.Contains("output") || lower.Contains("wrong")) return 2;
            if (lower.Contains("migration") || lower.Contains("migrate") || lower.Contains("move")) return 5;
            if (lower.Contains("wordpress") || lower.Contains("plugin") || lower.Contains("embed")) return 6;
            if (lower.Contains("marketplace") || lower.Contains("14 platform")) return 7;
            if (lower.Contains("legacy") || lower.Contains("old code") || lower.Contains("knowledge")) return 8;
            if (lower.Contains("guidelines") || lower.Contains("instructions") || lower.Contains("stale")) return 9;
            if (lower.Contains("orchestration") || lower.Contains("advisor")) return 10;
            return 4; // default: foundation/missing
        }

        // ── Baseline ──────────────────────────────────────────────────────────
        var t0 = Stopwatch();
        int baseOk    = val.Count(x => PredictKeyword(x.Prompt) == x.EpisodeId);
        double baseAcc = baseOk / (double)val.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced ──────────────────────────────────────────────────────────
        var t1 = Stopwatch();
        int enhOk     = val.Count(x => PredictKnn(x.Prompt) == x.EpisodeId);
        double enhAcc  = enhOk / (double)val.Count;
        double enhancedMs = ElapsedMs(t1);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E34-EPISODE-QUERY-MATCHER",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "hit_at_1",
            LowerIsBetter     = false,
            ScalingSignal     = (double)val.Select(x => x.EpisodeId).Distinct().Count() / 10.0,
            ScalingSignalNote = "episode_coverage (fraction of episodes represented in val)",
            ShouldScaleUp     = val.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"trainN={train.Count}, valN={val.Count}, " +
                    $"baseAcc={baseAcc:P1}, enhAcc={enhAcc:P1}"
        });
    }

    private static int EpisodeIdFromIndex(int idx, int total)
        => idx / (total / 10) + 1;

    private static double TagDist(List<string> a, List<string> b)
    {
        var sa = new HashSet<string>(a);
        var sb = new HashSet<string>(b);
        int inter = sa.Count(x => sb.Contains(x));
        int union  = sa.Count + sb.Count - inter;
        return union == 0 ? 0 : 1.0 - (double)inter / union;
    }

    private static long   Stopwatch()     => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
