// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using Microsoft.CodeAnalysis.CSharp;

namespace ExperimentsV2;

// E09: структурный вектор + K-NN vs baseline bigram.
// Baseline:  bigram на BPE токенах
// E09 KNN:   K ближайших соседей по структурному вектору
public sealed class E09StructuralKnn
{
    private readonly int _k;
    public E09StructuralKnn(int k = 5) => _k = k;

    public OperationResult<ExperimentV2Result> Run(
        string csharpSource,
        IReadOnlyList<(string Path, string Text)> files,
        int tier = 1)
    {
        if (string.IsNullOrWhiteSpace(csharpSource))
            return OperationResult<ExperimentV2Result>.Fail("Нет источника для E09");

        var tokenVectors = BuildTokenVectors(files);
        if (tokenVectors.Count < 10)
            return OperationResult<ExperimentV2Result>.Fail("Слишком мало AST узлов");

        int split = (int)(tokenVectors.Count * 0.8);
        var train = tokenVectors.Take(split).ToList();
        var val   = tokenVectors.Skip(split).ToList();

        // Baseline: bigram BPE
        var t0 = Stopwatch();
        var bigramCounts = new Dictionary<int, Dictionary<int, int>>();
        for (int i = 0; i < train.Count - 1; i++)
        {
            int cur = train[i].BpeId, next = train[i + 1].BpeId;
            if (!bigramCounts.TryGetValue(cur, out var d))
                bigramCounts[cur] = d = new Dictionary<int, int>();
            d[next] = d.GetValueOrDefault(next, 0) + 1;
        }

        var globalFreq = new Dictionary<int, int>();
        for (int i = 0; i < train.Count - 1; i++)
            globalFreq[train[i + 1].BpeId] = globalFreq.GetValueOrDefault(train[i + 1].BpeId, 0) + 1;
        int globalFallback = globalFreq.Count > 0 ? globalFreq.MaxBy(kv => kv.Value).Key : 0;

        int baseCorrect = 0;
        for (int i = 0; i < val.Count - 1; i++)
        {
            int cur = val[i].BpeId, next = val[i + 1].BpeId;
            int pred = bigramCounts.TryGetValue(cur, out var d) && d.Count > 0
                ? d.MaxBy(kv => kv.Value).Key : globalFallback;
            if (pred == next) baseCorrect++;
        }
        double baselineAcc = (val.Count - 1) > 0 ? baseCorrect / (double)(val.Count - 1) : 0;
        double baselineMs  = ElapsedMs(t0);

        // E09 K-NN
        var t1  = Stopwatch();
        var knn = new StructuralKnnPredictor(_k);
        var trainingPairs = train.Zip(train.Skip(1))
            .Select(p => (p.First.Vector, p.Second.BpeId))
            .ToList();
        knn.Train(trainingPairs);

        // Limit val sample to avoid O(n²) timeout on larger tiers
        var valSample  = val.Take(500).ToList();
        int knnCorrect = 0;
        for (int i = 0; i < valSample.Count - 1; i++)
        {
            int pred = knn.Predict(valSample[i].Vector);
            if (pred == valSample[i + 1].BpeId) knnCorrect++;
        }
        double knnAcc     = (valSample.Count - 1) > 0 ? knnCorrect / (double)(valSample.Count - 1) : 0;
        double enhancedMs = ElapsedMs(t1);

        var valVectors = valSample.Select(x => x.Vector).ToList();
        double avgDist = knn.AverageNearestNeighborDistance(valVectors);

        return OperationResult<ExperimentV2Result>.Ok(new ExperimentV2Result
        {
            ExperimentId      = "E09-STRUCTURAL-KNN",
            Tier              = tier,
            BaselinePrimary   = baselineAcc,
            EnhancedPrimary   = knnAcc,
            PrimaryMetricName = "top1_accuracy",
            LowerIsBetter     = false,
            ScalingSignal     = avgDist,
            ScalingSignalNote = "avg_nearest_neighbor_dist (меньше = соседи ближе = K-NN работает)",
            BaselineTimeMs    = baselineMs,
            EnhancedTimeMs    = enhancedMs,
            ShouldScaleUp     = avgDist > 3.0,
            Notes             = $"tokens={tokenVectors.Count}, k={_k}, " +
                                $"baseAcc={baselineAcc:P1}, knnAcc={knnAcc:P1}, " +
                                $"avgDist={avgDist:F2}, trainPairs={trainingPairs.Count}"
        });
    }

    private static List<TokenWithVector> BuildTokenVectors(
        IReadOnlyList<(string Path, string Text)> files)
    {
        var result = new List<TokenWithVector>();

        for (short fileId = 0; fileId < files.Count; fileId++)
        {
            var (_, text) = files[fileId];
            var tree  = CSharpSyntaxTree.ParseText(text);
            var nodes = tree.GetRoot().DescendantNodes().ToList();

            var kinds   = nodes.Select(n => (int)n.Kind()).Distinct().OrderBy(x => x).ToList();
            var kindMap = kinds.Select((k, i) => (k, i)).ToDictionary(p => p.k, p => p.i);

            foreach (var node in nodes)
            {
                int bpeId  = kindMap[(int)node.Kind()];
                var vector = StructuralVectorExtractor.Extract(node, fileId);
                result.Add(new TokenWithVector(bpeId, vector));
            }
        }
        return result;
    }

    private readonly record struct TokenWithVector(int BpeId, StructuralVector Vector);

    private static long   Stopwatch() => System.Diagnostics.Stopwatch.GetTimestamp();
    private static double ElapsedMs(long t0)
        => (System.Diagnostics.Stopwatch.GetTimestamp() - t0)
           / (double)System.Diagnostics.Stopwatch.Frequency * 1000.0;
}
