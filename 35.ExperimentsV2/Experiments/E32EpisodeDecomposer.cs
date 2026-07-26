// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E32: EpisodeDecomposer — predict how many sub-episodes a task needs.
// Training:  10 episodes × 9 phrasings = 90 examples
// Baseline:  always predict median (3 sub-episodes)
// Enhanced:  K-NN on FamilyCollapser tags of the prompt
// Metric:    count_accuracy (exact) + ±1_accuracy (within one)
public sealed class E32EpisodeDecomposer
{
    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        var data     = EpisodeCorpusLoader.Expand();
        var rng      = new Random(42);
        var shuffled = data.OrderBy(_ => rng.Next()).ToList();
        int split    = (int)(shuffled.Count * 0.8);
        var train    = shuffled.Take(split).ToList();
        var val      = shuffled.Skip(split).ToList();

        // E32-check: verify all 5 episode types present in val-set
        var valTypes    = val.Select(x => x.Type).Distinct().OrderBy(t => t).ToList();
        var allTypes    = new[] { "BUILD", "DESIGN", "FIX", "MIGRATE", "RESEARCH" };
        var missingTypes = allTypes.Except(valTypes).ToList();
        Console.WriteLine($"  E32 val-set types: [{string.Join(", ", valTypes)}] (n={val.Count})");
        if (missingTypes.Count > 0)
        {
            Console.WriteLine($"  E32 ⚠ Missing types in val: [{string.Join(", ", missingTypes)}] → applying StratifiedSplit");
            var (st, sv) = StratifiedSplit.Split(data, x => x.Type, trainRatio: 0.8, seed: 42);
            train = st;
            val   = sv;
            valTypes = val.Select(x => x.Type).Distinct().OrderBy(t => t).ToList();
            Console.WriteLine($"  E32 after stratified: val types=[{string.Join(", ", valTypes)}] n={val.Count}");
        }
        else
        {
            Console.WriteLine("  E32 ✓ All episode types present in val-set");
        }

        int median = train.Select(x => x.SubEpisodeCount)
            .OrderBy(x => x).ElementAt(train.Count / 2);

        var trainFeatures = train.Select(x =>
            (Tags: FamilyCollapser.CollapseString(x.Prompt).ToArray(),
             Count: x.SubEpisodeCount)).ToList();

        // ── Baseline: always predict median ──────────────────────────────────
        var t0        = Stopwatch();
        int baseExact = val.Count(x => x.SubEpisodeCount == median);
        int basePm1   = val.Count(x => Math.Abs(x.SubEpisodeCount - median) <= 1);
        double baseAcc  = baseExact / (double)val.Count;
        double basePm1A = basePm1   / (double)val.Count;
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: K-NN on family tags ────────────────────────────────────
        var t1 = Stopwatch();
        int enhExact = 0, enhPm1 = 0;
        foreach (var (prompt, expected, _) in val)
        {
            var qTags = FamilyCollapser.CollapseString(prompt);
            int predicted = trainFeatures
                .OrderBy(t => TagSetDistance(qTags.ToHashSet(), new HashSet<string>(t.Tags)))
                .Take(3)
                .GroupBy(t => t.Count)
                .OrderByDescending(g => g.Count())
                .First().Key;
            if (predicted == expected)               enhExact++;
            if (Math.Abs(predicted - expected) <= 1) enhPm1++;
        }
        double enhAcc  = enhExact / (double)val.Count;
        double enhPm1A = enhPm1   / (double)val.Count;
        double enhancedMs = ElapsedMs(t1);

        var typeStats = val.GroupBy(x => x.Type)
            .Select(g =>
            {
                int ok = g.Count(x =>
                {
                    var qT  = FamilyCollapser.CollapseString(x.Prompt);
                    int pred = trainFeatures
                        .OrderBy(t => TagSetDistance(qT.ToHashSet(), new HashSet<string>(t.Tags)))
                        .Take(3).GroupBy(t => t.Count)
                        .OrderByDescending(gg => gg.Count()).First().Key;
                    return pred == x.SubEpisodeCount;
                });
                return $"{g.Key}={ok}/{g.Count()}";
            });

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E32-EPISODE-DECOMPOSER",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "count_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = enhPm1A,
            ScalingSignalNote = "±1_accuracy (within one of correct count)",
            ShouldScaleUp     = val.Count < 10,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"trainN={train.Count}, valN={val.Count}, median={median}, " +
                    $"baseExact={baseAcc:P1} ±1={basePm1A:P1}, " +
                    $"enhExact={enhAcc:P1} ±1={enhPm1A:P1}, " +
                    $"by_type=[{string.Join(",", typeStats)}]"
        });
    }

    private static double TagSetDistance(HashSet<string> a, HashSet<string> b)
    {
        if (a.Count == 0 && b.Count == 0) return 0;
        int inter = a.Count(x => b.Contains(x));
        int union = a.Count + b.Count - inter;
        return union == 0 ? 0 : 1.0 - (double)inter / union;
    }

    private static long   Stopwatch()     => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
