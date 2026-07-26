// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ExperimentsV2;

// E13: method-level K-NN — AST-packed strings instead of tokens.
//
// Hypothesis: knowing the structure of method N (KindBag), K-NN predicts
// the canonical type of method N+1 better than a frequency baseline.
// Validates Variant A (neural net on AST method corpus) statistically.
//
// Baseline:  majority-class frequency predictor over 6 canonical types
// Enhanced:  K-NN (k=5, Jaccard on KindBag)
// Primary metric: top1_accuracy (higher is better)
// Scaling signal: avg Jaccard distance to nearest neighbour (> 0.6 = too little data)
public sealed class E13MethodLevelKnn
{
    private readonly int _k;
    public E13MethodLevelKnn(int k = 5) => _k = k;

    public OperationResult<ExperimentV2Result> Run(
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (files.Count == 0)
            return OperationResult<ExperimentV2Result>.Fail("No files for E13");

        var allMethods = new List<PackedMethod>();
        for (short fid = 0; fid < (short)Math.Min(files.Count, short.MaxValue); fid++)
        {
            var op = MethodExtractor.Extract(files[fid].Text, fid);
            if (op.IsSuccess) allMethods.AddRange(op.Value!);
        }

        if (allMethods.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Too few methods: {allMethods.Count}. Need ≥ 4.");

        var pairs = new List<(PackedMethod Current, PackedMethod Next)>();
        for (int i = 0; i < allMethods.Count - 1; i++)
            if (allMethods[i].FileId == allMethods[i + 1].FileId &&
                allMethods[i + 1].IndexInFile == allMethods[i].IndexInFile + 1)
                pairs.Add((allMethods[i], allMethods[i + 1]));

        if (pairs.Count < 4)
            return OperationResult<ExperimentV2Result>.Fail(
                $"Too few method pairs: {pairs.Count}. Need ≥ 4.");

        int split      = (int)(pairs.Count * 0.8);
        var trainPairs = pairs.Take(split).ToList();
        var valPairs   = pairs.Skip(split).ToList();

        // ── Baseline: frequency predictor ────────────────────────────────────
        var t0 = Stopwatch();
        var typeFreq = new Dictionary<string, int>();
        foreach (var (_, next) in trainPairs)
        {
            string typ = MethodKnnPredictor.CanonicalType(next.KindBag);
            typeFreq[typ] = typeFreq.GetValueOrDefault(typ) + 1;
        }
        string mostFreq    = typeFreq.Count > 0 ? typeFreq.MaxBy(kv => kv.Value).Key : "Mixed";
        int baseCorrect    = valPairs.Count(p => MethodKnnPredictor.CanonicalType(p.Next.KindBag) == mostFreq);
        double baselineAcc = valPairs.Count > 0 ? baseCorrect / (double)valPairs.Count : 0;
        double baselineMs  = ElapsedMs(t0);

        // ── Enhanced: K-NN (Jaccard) ──────────────────────────────────────────
        var t1 = Stopwatch();
        var knn = new MethodKnnPredictor(_k);
        knn.Train(allMethods.Take((int)(allMethods.Count * 0.8)).ToList());

        int knnCorrect = 0;
        foreach (var (current, next) in valPairs)
        {
            string predicted = knn.Predict(current);
            if (predicted == MethodKnnPredictor.CanonicalType(next.KindBag))
                knnCorrect++;
        }
        double knnAcc     = valPairs.Count > 0 ? knnCorrect / (double)valPairs.Count : 0;
        double enhancedMs = ElapsedMs(t1);

        double avgDist  = knn.AverageNearestNeighbourDistance(valPairs.Select(p => p.Current).ToList());
        string typeDist = string.Join(", ", typeFreq.OrderByDescending(kv => kv.Value)
                                                    .Select(kv => $"{kv.Key}={kv.Value}"));

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E13-METHOD-KNN",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = knnAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = avgDist,
            ScalingSignalNote = "avg_jaccard_dist (> 0.6 = too few method pairs, scale up)",
            ShouldScaleUp     = avgDist > 0.6 || valPairs.Count < 5,
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            Notes = $"methods={allMethods.Count}, pairs={pairs.Count}, " +
                    $"trainPairs={trainPairs.Count}, k={_k}, " +
                    $"baseAcc={baselineAcc:P1}, knnAcc={knnAcc:P1}, " +
                    $"avgDist={avgDist:F3}, types=[{typeDist}]"
        });
    }

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
