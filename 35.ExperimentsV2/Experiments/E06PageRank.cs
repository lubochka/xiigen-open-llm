// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using GraphEngine;
using GraphEngine.Models;

namespace ExperimentsV2;

// ─── E06: PageRank on call graph vs raw frequency for method ranking ──────────

// Hypothesis: a method called frequently FROM many important callers (as measured
// by PageRank on the call graph) is more architecturally central than a method
// that is only called many times overall.  PageRank-based ranking should achieve
// a higher MRR than frequency-count ranking when predicting the callee.
//
// Baseline:  rank all callees by raw call frequency (global count).
// Enhanced:  rank all callees by PageRank score in the call graph.
// Metric:    MRR = (1/|queries|) × Σ 1/rank(actual_callee).
//            A rank of 1 = perfect; rank 2 = 0.5; rank 3 = 0.33; etc.
//
// Scaling signal: Gini coefficient of PageRank scores — near 0 = uniform (useless),
//                 near 1 = concentrated (some methods are "hubs").
//                 Graph needs ≥ Tier 2 to develop meaningful hubs.
public sealed class E06PageRank
{
    // Run the experiment.
    // 'callPairs' is a list of (callerMethod, calleeMethod) edges.
    // A caller may appear multiple times (once per call site).
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Caller, string Callee)> callPairs,
        int tier = 1)
    {
        if (callPairs.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("No call pairs for E06");

        var t0 = Stopwatch();

        // ── Build call graph ─────────────────────────────────────────────────────
        var graph       = new KnowledgeGraph();
        var allMethods  = callPairs.SelectMany(p => new[] { p.Caller, p.Callee })
                                   .Distinct(StringComparer.Ordinal)
                                   .ToList();

        foreach (var m in allMethods)
            graph.AddNode(new GraphNode { Id = m, Name = m, Type = "Method" });

        // Aggregate call counts: one weighted edge per (caller, callee) pair.
        var edgeCounts = new Dictionary<(string, string), int>();
        foreach (var (caller, callee) in callPairs)
        {
            var key = (caller, callee);
            edgeCounts[key] = edgeCounts.GetValueOrDefault(key, 0) + 1;
        }

        foreach (var ((caller, callee), count) in edgeCounts)
        {
            if (caller != callee)  // skip self-loops
                graph.AddEdge(new GraphEdge
                {
                    From         = caller,
                    To           = callee,
                    RelationType = "Calls",
                    Weight       = count
                });
        }

        // ── Compute PageRank ──────────────────────────────────────────────────────
        new PageRankCalculator().ComputePageRank(graph);

        // ── Baseline: frequency count of each callee across all call pairs ────────
        var calleeFreq = new Dictionary<string, int>();
        foreach (var (_, callee) in callPairs)
            calleeFreq[callee] = calleeFreq.GetValueOrDefault(callee, 0) + 1;

        // Sorted candidate lists (all methods as ranked by each strategy).
        var freqRanked = allMethods
            .OrderByDescending(m => calleeFreq.GetValueOrDefault(m, 0))
            .ToList();

        var pagerankRanked = allMethods
            .OrderByDescending(m => graph.TryGetNode(m, out var node) ? node.PageRankScore : 0)
            .ToList();

        double baselineMs = ElapsedMs(t0);
        var    t1         = Stopwatch();

        // ── Compute MRR for each strategy ─────────────────────────────────────────
        // For each (caller, callee) pair we ask: given the caller made a call,
        // what rank does the actual callee get in the global ranked list?
        double baseMrrSum = 0, enhMrrSum = 0;
        int    queries    = callPairs.Count;

        foreach (var (_, callee) in callPairs)
        {
            int freqRank     = freqRanked.IndexOf(callee) + 1;     // 1-based
            int pagerankRank = pagerankRanked.IndexOf(callee) + 1;  // 1-based
            if (freqRank     > 0) baseMrrSum += 1.0 / freqRank;
            if (pagerankRank > 0) enhMrrSum  += 1.0 / pagerankRank;
        }

        double baseMrr    = queries > 0 ? baseMrrSum / queries : 0;
        double enhMrr     = queries > 0 ? enhMrrSum  / queries : 0;
        double enhancedMs = ElapsedMs(t1);

        // ── Gini coefficient of PageRank scores ───────────────────────────────────
        var scores = allMethods
            .Select(m => graph.TryGetNode(m, out var n) ? n.PageRankScore : 0)
            .OrderBy(s => s)
            .ToList();
        double gini = GiniCoefficient(scores);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E06-PAGERANK-RANKING",
            Tier              = tier,
            BaselinePrimary   = baseMrr,
            EnhancedPrimary   = enhMrr,
            PrimaryMetricName = "mrr",
            LowerIsBetter     = false,
            ScalingSignal     = gini,
            ScalingSignalNote = "pagerank_gini (higher = more concentrated = hubs exist)",
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes             = $"methods={allMethods.Count}, edges={edgeCounts.Count}, " +
                                $"gini={gini:F3}, baseMrr={baseMrr:F3}, enhMrr={enhMrr:F3}",
            // Gini < 0.3 = call graph too flat for PageRank to distinguish hubs.
            // Auto-escalate to a larger tier where the graph develops real structure.
            ShouldScaleUp     = gini < 0.3
        });
    }

    // Gini coefficient of a sorted list of non-negative values.
    // Returns 0 when all values are equal, approaches 1 when one dominates.
    public static double GiniCoefficient(IReadOnlyList<double> sortedValues)
    {
        int n = sortedValues.Count;
        if (n == 0) return 0;
        double sum    = sortedValues.Sum();
        if (sum == 0) return 0;

        double weightedSum = 0;
        for (int i = 0; i < n; i++)
            weightedSum += (i + 1) * sortedValues[i];

        return (2.0 * weightedSum) / (n * sum) - (n + 1.0) / n;
    }

    private static long   Stopwatch()     => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
