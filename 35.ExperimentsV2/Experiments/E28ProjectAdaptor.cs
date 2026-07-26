// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E28: ProjectAdaptor — project-specific specialization improves accuracy.
// Baseline:  bigram model trained on all files (generic)
// Enhanced:  bigram model trained only on project-A files (adapted)
// Hypothesis: project-specific patterns (OperationResult.Fail, etc.) are learned better
//             even with fewer training examples.
// Metric:    top1_accuracy on project-A test files
public sealed class E28ProjectAdaptor
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> allFiles,
        int tier = 1)
    {
        if (allFiles.Count < 6)
            return OperationResult<ExperimentV2Result>.Fail("Need >= 6 files");

        int adaptSplit = (int)(allFiles.Count * 0.6);
        var projectA   = allFiles.Take(adaptSplit).ToList();
        var testFiles  = allFiles.Skip(adaptSplit).ToList();

        var genericPairs = ExtractPredPairs(allFiles);
        var adaptedPairs = ExtractPredPairs(projectA);
        var testPairs    = ExtractPredPairs(testFiles);

        if (testPairs.Count < 5)
            return OperationResult<ExperimentV2Result>.Fail("Too few test pairs");

        // Build bigram models.
        var genericModel = BuildBigram(genericPairs);
        var adaptedModel = BuildBigram(adaptedPairs);

        int Predict(Dictionary<int, Dictionary<int, int>> model, int current,
                    Dictionary<int, Dictionary<int, int>> fallback)
        {
            if (model.TryGetValue(current, out var d) && d.Count > 0)
                return d.MaxBy(kv => kv.Value).Key;
            if (fallback.TryGetValue(current, out var df) && df.Count > 0)
                return df.MaxBy(kv => kv.Value).Key;
            return 0;
        }

        // ── Baseline: generic model ───────────────────────────────────────────
        var t0 = Stopwatch();
        int baseOk = testPairs.Count(p =>
            Predict(genericModel, p.Current, genericModel) == p.Next);
        double baselineAcc = baseOk / (double)testPairs.Count;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: adapted model (with generic fallback) ───────────────────
        var t1 = Stopwatch();
        int enhOk = testPairs.Count(p =>
            Predict(adaptedModel, p.Current, genericModel) == p.Next);
        double enhancedAcc = enhOk / (double)testPairs.Count;
        double enhancedMs  = ElapsedMs(t1);

        double overlap = ComputePredicateOverlap(adaptedPairs, testPairs);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E28-PROJECT-ADAPTOR",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = enhancedAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = overlap,
            ScalingSignalNote = "predicate_overlap (higher = more gain from adaptation)",
            ShouldScaleUp     = testPairs.Count < 20,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"allFiles={allFiles.Count}, adaptFiles={projectA.Count}, " +
                    $"testPairs={testPairs.Count}, genericPairs={genericPairs.Count}, " +
                    $"adaptedPairs={adaptedPairs.Count}, overlap={overlap:F2}, " +
                    $"baseAcc={baselineAcc:P1}, enhAcc={enhancedAcc:P1}"
        });
    }

    private static List<(int Current, int Next)> ExtractPredPairs(
        IReadOnlyList<(string Path, string Text)> files)
    {
        var preds = new List<int>();
        foreach (var (_, text) in files)
        {
            var r = LogicExtractor.Extract(text);
            if (r.IsSuccess) preds.AddRange(r.Value!);
        }
        return preds.Zip(preds.Skip(1)).Select(p => (p.First, p.Second)).ToList();
    }

    private static Dictionary<int, Dictionary<int, int>> BuildBigram(
        IEnumerable<(int Current, int Next)> pairs)
    {
        var model = new Dictionary<int, Dictionary<int, int>>();
        foreach (var (cur, nxt) in pairs)
        {
            if (!model.TryGetValue(cur, out var d)) model[cur] = d = new();
            d[nxt] = d.GetValueOrDefault(nxt, 0) + 1;
        }
        return model;
    }

    private static double ComputePredicateOverlap(
        IEnumerable<(int, int)> a, IEnumerable<(int, int)> b)
    {
        var setA = a.Select(p => p.Item1).ToHashSet();
        var setB = b.Select(p => p.Item1).ToHashSet();
        int inter = setA.Count(x => setB.Contains(x));
        int union  = setA.Count + setB.Count - inter;
        return union == 0 ? 0 : (double)inter / union;
    }

    private static long   Stopwatch()     => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
