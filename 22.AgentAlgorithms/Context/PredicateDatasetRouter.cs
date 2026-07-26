// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms;

// Production source for E22 dataset routing.
// Predicts the relevant dataset buckets from an E17 predicate vector with KNN voting.
public sealed class PredicateDatasetRouter
{
    public static readonly string[] DatasetNames =
        ["Generic", "CSharp", "TypeScript", "BugFix", "Orchestration", "Tests"];

    private readonly List<(int[] Predicates, bool[] Datasets)> _train = [];
    private readonly int _k;
    private readonly int _datasetCount;

    public PredicateDatasetRouter(int k = 3, int datasetCount = 6)
    {
        _k = k;
        _datasetCount = datasetCount;
    }

    public void Train(IReadOnlyList<(int[] Predicates, bool[] Datasets)> examples)
    {
        _train.Clear();
        _train.AddRange(examples);
    }

    public bool[] Predict(int[] queryPredicates)
    {
        if (_train.Count == 0)
            return Enumerable.Repeat(true, _datasetCount).ToArray();

        var neighbours = _train
            .OrderBy(row => JaccardDistance(queryPredicates, row.Predicates))
            .Take(_k)
            .ToArray();

        var result = new bool[_datasetCount];
        for (var index = 0; index < _datasetCount; index++)
        {
            var votes = neighbours.Count(row => index < row.Datasets.Length && row.Datasets[index]);
            result[index] = votes > _k / 2;
        }

        return result;
    }

    public static bool[] AutoLabel(string source)
    {
        var label = new bool[DatasetNames.Length];
        var lower = source.ToLowerInvariant();

        label[0] = source.Length > 100;
        label[1] = lower.Contains("namespace ", StringComparison.Ordinal) ||
                   lower.Contains("using system", StringComparison.Ordinal);
        label[2] = (lower.Contains("interface ", StringComparison.Ordinal) && lower.Contains("injectable", StringComparison.Ordinal)) ||
                   lower.Contains("export default", StringComparison.Ordinal) ||
                   lower.Contains("@module", StringComparison.Ordinal);
        label[3] = lower.Contains("throw ", StringComparison.Ordinal) ||
                   lower.Contains("catch (", StringComparison.Ordinal) ||
                   NeedsNullCheck(source);
        label[4] = (lower.Contains("flow", StringComparison.Ordinal) && lower.Contains("phase", StringComparison.Ordinal)) ||
                   lower.Contains("orchestrat", StringComparison.Ordinal) ||
                   lower.Contains("pipeline", StringComparison.Ordinal);
        label[5] = lower.Contains("[fact]", StringComparison.Ordinal) ||
                   lower.Contains("[test]", StringComparison.Ordinal) ||
                   lower.Contains("assert.", StringComparison.Ordinal) ||
                   lower.Contains("[testmethod]", StringComparison.Ordinal);

        return label;
    }

    private static bool NeedsNullCheck(string source)
    {
        var lower = source.ToLowerInvariant();
        return lower.Contains(".", StringComparison.Ordinal) &&
               !lower.Contains(" null", StringComparison.Ordinal) &&
               !lower.Contains("is null", StringComparison.Ordinal);
    }

    private static double JaccardDistance(int[] left, int[] right)
    {
        var leftSet = new HashSet<int>(left);
        var rightSet = new HashSet<int>(right);
        var intersection = leftSet.Count(rightSet.Contains);
        var union = leftSet.Count + rightSet.Count - intersection;
        return union == 0 ? 0.0 : 1.0 - (double)intersection / union;
    }
}
