// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace MachineLearning;

// K-Means clustering: iteratively assign points to nearest centroid and recompute centroids.
// Converges when no assignments change between iterations (or max iterations reached).
public sealed class KMeansAlgorithm
{
    private List<double[]> _centroids = [];
    private bool _fitted;

    public OperationResult<int[]> Fit(List<double[]> points, int k, int maxIterations = 100)
    {
        if (points.Count == 0) return OperationResult<int[]>.Fail("No data points");
        if (k <= 0 || k > points.Count)
            return OperationResult<int[]>.Fail($"k={k} invalid for {points.Count} points");

        int dims = points[0].Length;
        var rng  = new Random(42);

        // K-means++ initialisation: pick first centroid at random, then by distance
        _centroids = [points[rng.Next(points.Count)].ToArray()];
        while (_centroids.Count < k)
        {
            var distances = points.Select(p => _centroids.Min(c => SqDist(p, c))).ToArray();
            double total  = distances.Sum();
            double thresh  = rng.NextDouble() * total;
            double cumul   = 0;
            for (int i = 0; i < points.Count; i++)
            {
                cumul += distances[i];
                if (cumul >= thresh) { _centroids.Add(points[i].ToArray()); break; }
            }
        }

        int[] assignments = new int[points.Count];

        for (int iter = 0; iter < maxIterations; iter++)
        {
            bool changed = false;
            for (int i = 0; i < points.Count; i++)
            {
                int best    = 0;
                double bestD = double.MaxValue;
                for (int c = 0; c < k; c++)
                {
                    double d = SqDist(points[i], _centroids[c]);
                    if (d < bestD) { bestD = d; best = c; }
                }
                if (assignments[i] != best) { assignments[i] = best; changed = true; }
            }
            if (!changed) break;

            // Recompute centroids
            for (int c = 0; c < k; c++)
            {
                var members = points.Where((_, i) => assignments[i] == c).ToList();
                if (members.Count == 0) continue;
                _centroids[c] = new double[dims];
                foreach (var p in members)
                    for (int d = 0; d < dims; d++)
                        _centroids[c][d] += p[d] / members.Count;
            }
        }

        _fitted = true;
        return OperationResult<int[]>.Ok(assignments);
    }

    public OperationResult<int> Predict(double[] point)
    {
        if (!_fitted) return OperationResult<int>.Fail("Model not fitted");
        int best = 0;
        double bestD = double.MaxValue;
        for (int c = 0; c < _centroids.Count; c++)
        {
            double d = SqDist(point, _centroids[c]);
            if (d < bestD) { bestD = d; best = c; }
        }
        return OperationResult<int>.Ok(best);
    }

    public IReadOnlyList<double[]> Centroids => _centroids.AsReadOnly();

    private static double SqDist(double[] a, double[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length; i++) { double d = a[i] - b[i]; sum += d * d; }
        return sum;
    }
}

public sealed class KMeansService : BaseService
{
    private readonly KMeansAlgorithm _algo = new();

    public KMeansService(IModelStateStorage storage, IPredictionLogger logger)
        : base(storage, logger) { }

    public OperationResult<int[]> Cluster(
        string operationId, List<double[]> points, int k, int maxIter = 100)
    {
        if (Idempotency.TryGetResult(operationId, out _))
            return OperationResult<int[]>.Fail("Already processed — retrieve stored result");

        var result = _algo.Fit(points, k, maxIter);
        if (result.IsSuccess)
        {
            Idempotency.Record(operationId, new() { ["fitted"] = true });
            Storage.Save("kmeans", new() { ["k"] = k });
        }
        return result;
    }

    public OperationResult<int> Predict(double[] point) => _algo.Predict(point);
    public IReadOnlyList<double[]> Centroids => _algo.Centroids;
}
