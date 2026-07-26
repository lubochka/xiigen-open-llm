// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E50: DomainGraphClosure — dependency graph stabilises after N new domains.
// Baseline:  average edge_delta (graph keeps changing, no convergence)
// Enhanced:  final edge_delta (after enough domains added, graph stabilises < 5%)
// Metric:    final_edge_delta (lower is better) + closure_round
public sealed class E50DomainGraphClosure
{
    private const double EdgeDeltaThreshold = 0.05; // 5% change = stable

    private static readonly (string Name, int[] Predicates)[] AllDomains =
    [
        // Initially known domains
        ("CODE_QUESTION", [7, 9, 7, 9, 3, 7]),
        ("BUG_FIX",       [1, 2, 9, 1, 2, 0, 9]),
        ("CODE_CHANGE",   [8, 6, 4, 8, 3, 8, 4]),
        ("ORCHESTRATION", [5, 6, 3, 5, 6, 6, 3]),
        ("ARCHITECTURE",  [4, 8, 3, 4, 4, 8, 3]),
        // New domains added one by one
        ("DEVOPS",         [5, 6, 8, 4, 5, 3, 6]),
        ("SECURITY",       [1, 4, 2, 1, 0, 9, 2]),
        ("DATA_ANALYTICS", [7, 5, 9, 7, 5, 6, 3]),
        ("MOBILE",         [8, 3, 7, 8, 4, 6, 7]),
        ("TESTING",        [4, 9, 2, 4, 1, 3, 9]),
    ];

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        const double threshold = 0.35;
        var deltas = new List<double>();
        var prevEdges = new HashSet<(int, int)>();
        int closureRound = AllDomains.Length;

        var t0 = Stopwatch();
        for (int n = 2; n <= AllDomains.Length; n++)
        {
            var current = AllDomains.Take(n).ToArray();
            var currentEdges = BuildEdges(current, threshold);

            if (n > 2)
            {
                int added   = currentEdges.Count(e => !prevEdges.Contains(e));
                int removed = prevEdges.Count(e => !currentEdges.Contains(e));
                int changed = added + removed;
                int total   = Math.Max(1, currentEdges.Count + prevEdges.Count);
                double delta = changed / (double)total;
                deltas.Add(delta);

                if (delta < EdgeDeltaThreshold && closureRound == AllDomains.Length)
                    closureRound = n;
            }
            prevEdges = currentEdges;
        }
        double enhancedMs = ElapsedMs(t0);

        double avgDelta   = deltas.Count > 0 ? deltas.Average() : 1.0;
        double finalDelta = deltas.Count > 0 ? deltas[^1]       : 1.0;

        string deltaStr = string.Join(",", deltas.Select((d, i) => $"D{i + 3}={d:P0}"));

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E50-DOMAIN-GRAPH-CLOSURE",
            Tier              = tier,
            BaselinePrimary   = avgDelta,
            EnhancedPrimary   = finalDelta,
            PrimaryMetricName = "final_edge_delta",
            LowerIsBetter     = true,
            ScalingSignal     = closureRound / (double)AllDomains.Length,
            ScalingSignalNote = "closure_ratio (lower = graph stabilises sooner)",
            ShouldScaleUp     = AllDomains.Length < 8,
            BaselineTimeMs    = enhancedMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"domains={AllDomains.Length}, threshold={threshold:F2}, " +
                    $"closureRound={closureRound}/{AllDomains.Length}, " +
                    $"avgDelta={avgDelta:P1}, finalDelta={finalDelta:P1}, " +
                    $"deltas=[{deltaStr}]"
        });
    }

    private static HashSet<(int, int)> BuildEdges(
        (string Name, int[] Predicates)[] domains, double threshold)
    {
        var edges = new HashSet<(int, int)>();
        for (int i = 0; i < domains.Length; i++)
            for (int j = i + 1; j < domains.Length; j++)
            {
                var a = domains[i].Predicates.ToHashSet();
                var b = domains[j].Predicates.ToHashSet();
                int inter = a.Count(p => b.Contains(p));
                int union = a.Count + b.Count - inter;
                if (union > 0 && (double)inter / union > threshold)
                    edges.Add((i, j));
            }
        return edges;
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
