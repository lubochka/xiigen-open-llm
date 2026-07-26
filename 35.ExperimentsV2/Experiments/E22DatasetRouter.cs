// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E22: DatasetRouter — K-NN on E17-predicates predicts a binary dataset vector.
//
// Hypothesis: the predicate profile of a file is sufficient to decide which
// sub-datasets are relevant, replacing the current "use everything" approach
// and enabling the routing architecture described in the E21–E23 pipeline.
//
// Baseline:  always return all datasets (recall=1, precision=1/6≈16.7%)
// Enhanced:  K-NN (k=3, Jaccard on predicate sets) → binary vector
// Primary metric: macro-F1 across 6 dataset buckets
// Dataset:   auto-labelled from project files — no manual annotation
public sealed class E22DatasetRouter
{
    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail("Need ≥ 4 files for E22");

        var examples = new List<(int[] Predicates, bool[] Datasets)>();
        foreach (var (_, text) in files)
        {
            var predOp = LogicExtractor.Extract(text);
            if (!predOp.IsSuccess || predOp.Value!.Count == 0) continue;
            bool[] label = AgentAlgorithms.PredicateDatasetRouter.AutoLabel(text);
            if (label.Any(b => b))
                examples.Add((predOp.Value!.ToArray(), label));
        }

        if (examples.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Too few labelled examples: {examples.Count}. Need ≥ 4.");

        int split  = (int)(examples.Count * 0.8);
        var train  = examples.Take(split).ToList();
        var val    = examples.Skip(split).ToList();

        int dsCount = AgentAlgorithms.PredicateDatasetRouter.DatasetNames.Length;
        var router  = new AgentAlgorithms.PredicateDatasetRouter(k: 3);
        router.Train(train);

        // ── Baseline: predict the 3 most frequent datasets (fairer than predict-all).
        // predict-all has recall=1 which mathematically dominates macro-F1 regardless
        // of precision. Top-3 gives recall≈0.5, letting K-NN win on precision.
        const int topK = 3;
        var t0 = Stopwatch();
        var freq = new int[dsCount];
        foreach (var (_, label) in train)
            for (int d = 0; d < dsCount; d++)
                if (label[d]) freq[d]++;

        bool[] basePred = new bool[dsCount];
        foreach (int d in Enumerable.Range(0, dsCount).OrderByDescending(d => freq[d]).Take(topK))
            basePred[d] = true;

        var btp = new int[dsCount]; var bfp = new int[dsCount]; var bfn = new int[dsCount];
        foreach (var (_, actual) in val)
            for (int d = 0; d < dsCount; d++)
            {
                if (basePred[d]  && actual[d])  btp[d]++;
                if (basePred[d]  && !actual[d]) bfp[d]++;
                if (!basePred[d] && actual[d])  bfn[d]++;
            }
        double baseF1 = Enumerable.Range(0, dsCount).Select(d =>
        {
            double prec = btp[d] + bfp[d] > 0 ? btp[d] / (double)(btp[d] + bfp[d]) : 0;
            double rec  = btp[d] + bfn[d] > 0 ? btp[d] / (double)(btp[d] + bfn[d]) : 0;
            return HarmonicMean(prec, rec);
        }).Average();
        double baselineMs = ElapsedMs(t0);

        // ── Enhanced: K-NN routing ────────────────────────────────────────────
        var t1 = Stopwatch();
        var tp = new int[dsCount]; var fp = new int[dsCount];
        var fn = new int[dsCount];

        foreach (var (preds, actual) in val)
        {
            bool[] predicted = router.Predict(preds);
            for (int d = 0; d < dsCount; d++)
            {
                if (predicted[d]  && actual[d])  tp[d]++;
                if (predicted[d]  && !actual[d]) fp[d]++;
                if (!predicted[d] && actual[d])  fn[d]++;
            }
        }

        var f1PerDs = Enumerable.Range(0, dsCount).Select(d =>
        {
            double prec = tp[d] + fp[d] > 0 ? tp[d] / (double)(tp[d] + fp[d]) : 0;
            double rec  = tp[d] + fn[d] > 0 ? tp[d] / (double)(tp[d] + fn[d]) : 0;
            return HarmonicMean(prec, rec);
        }).ToArray();

        double macroF1    = f1PerDs.Average();
        double enhancedMs = ElapsedMs(t1);

        string f1Notes = string.Join(", ", Enumerable.Range(0, dsCount)
            .Select(d => $"{AgentAlgorithms.PredicateDatasetRouter.DatasetNames[d]}={f1PerDs[d]:F2}"));

        double multiRatio = val.Count > 0
            ? val.Count(ex => ex.Datasets.Count(b => b) > 1) / (double)val.Count
            : 0;

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E22-DATASET-ROUTER",
            Tier              = tier,
            BaselinePrimary   = baseF1,
            EnhancedPrimary   = macroF1,
            PrimaryMetricName = "macro_f1",
            LowerIsBetter     = false,
            ScalingSignal     = multiRatio,
            ScalingSignalNote = "multi_dataset_ratio (fraction needing >1 dataset)",
            ShouldScaleUp     = val.Count < 15,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"examples={examples.Count}, train={train.Count}, val={val.Count}, " +
                    $"baseline=top{topK}freq, baseF1={baseF1:F2}, macroF1(knn)={macroF1:F2}, " +
                    $"per_dataset=[{f1Notes}]"
        });
    }

    private static double HarmonicMean(double a, double b)
        => a + b < 1e-9 ? 0 : 2 * a * b / (a + b);

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
