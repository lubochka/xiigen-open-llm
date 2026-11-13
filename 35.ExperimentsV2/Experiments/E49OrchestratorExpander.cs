// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E49: OrchestratorExpander — infer dependencies of new domains via predicate overlap.
// Baseline:  no dependencies (linear graph — all domains independent)
// Enhanced:  Jaccard overlap of predicate profiles → add edge if overlap > threshold
// Hypothesis: new domain's predicate distribution predicts which existing domains
//             it depends on (DevOps overlaps with ORCHESTRATION and ARCHITECTURE).
// Metric:    dependency_accuracy + graph_precision
public sealed class E49OrchestratorExpander
{
    // Known domains with typical predicate ID profiles
    private static readonly (string Name, int[] Predicates)[] KnownDomains =
    [
        ("CODE_QUESTION", [7, 9, 7, 9, 3, 7]),          // GET_CALL, CONDITION
        ("BUG_FIX",       [1, 2, 9, 1, 2, 0, 9]),       // NULL_GUARD, FAIL, CONDITION
        ("CODE_CHANGE",   [8, 6, 4, 8, 3, 8, 4]),       // CREATE_OBJ, ASSIGN, VALIDATE
        ("ORCHESTRATION", [5, 6, 3, 5, 6, 6, 3]),       // ITERATE, ASSIGN
        ("ARCHITECTURE",  [4, 8, 3, 4, 4, 8, 3]),       // VALIDATE, CREATE, VALIDATE
    ];

    // New domains with known correct dependencies (indices into KnownDomains)
    private static readonly (string Name, int[] Predicates, int[] DependsOn)[] NewDomains =
    [
        // DevOps depends on ORCHESTRATION(3) and ARCHITECTURE(4)
        ("DEVOPS",         [5, 6, 8, 4, 5, 6, 3, 4], [3, 4]),
        // Security depends on CODE_CHANGE(2) and BUG_FIX(1)
        ("SECURITY",       [1, 4, 2, 1, 2, 0, 9, 2], [1, 2]),
        // Data Analytics depends on CODE_QUESTION(0) and ORCHESTRATION(3)
        ("DATA_ANALYTICS", [7, 5, 3, 9, 7, 5, 6, 7], [0, 3]),
    ];

    public OperationResult<ExperimentV2Result> Run(int tier = 1)
    {
        const double threshold = 0.35;
        int totalDecisions = NewDomains.Length * KnownDomains.Length;
        int totalTrueEdges = NewDomains.Sum(nd => nd.DependsOn.Length);

        var t0 = Stopwatch();

        // ── Baseline: no dependencies (always predict NOT depends) ────────────
        // Correct for all non-edge pairs; wrong for all true edges
        int baseCorrect = totalDecisions - totalTrueEdges;
        double baseAcc  = baseCorrect / (double)totalDecisions;

        // ── Enhanced: Jaccard overlap → edge if overlap > threshold ───────────
        int enhCorrect = 0, predEdgeCount = 0, trueEdgeHitCount = 0;
        foreach (var (_, newPreds, dependsOn) in NewDomains)
        {
            var newSet = newPreds.ToHashSet();
            for (int d = 0; d < KnownDomains.Length; d++)
            {
                var domSet = KnownDomains[d].Predicates.ToHashSet();
                int inter  = newSet.Count(p => domSet.Contains(p));
                int union  = newSet.Count + domSet.Count - inter;
                double jacc = union == 0 ? 0 : (double)inter / union;

                bool predicted = jacc > threshold;
                bool actual    = dependsOn.Contains(d);
                if (predicted == actual) enhCorrect++;
                if (predicted) predEdgeCount++;
                if (predicted && actual) trueEdgeHitCount++;
            }
        }
        double enhAcc        = enhCorrect / (double)totalDecisions;
        double graphPrecision = predEdgeCount > 0 ? trueEdgeHitCount / (double)predEdgeCount : 0;
        double enhancedMs    = ElapsedMs(t0);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E49-ORCHESTRATOR-EXPANDER",
            Tier              = tier,
            BaselinePrimary   = baseAcc,
            EnhancedPrimary   = enhAcc,
            PrimaryMetricName = "dependency_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = graphPrecision,
            ScalingSignalNote = "graph_precision (correct edges / predicted edges)",
            ShouldScaleUp     = NewDomains.Length < 5,
            BaselineTimeMs    = enhancedMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"newDomains={NewDomains.Length}, knownDomains={KnownDomains.Length}, " +
                    $"threshold={threshold:F2}, predEdges={predEdgeCount}, trueEdges={totalTrueEdges}, " +
                    $"hits={trueEdgeHitCount}, baseAcc={baseAcc:P1}, enhAcc={enhAcc:P1}, " +
                    $"precision={graphPrecision:P1}"
        });
    }

    private static long   Stopwatch()   => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
