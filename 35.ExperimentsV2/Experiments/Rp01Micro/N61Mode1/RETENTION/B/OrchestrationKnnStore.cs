// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.RegularExpressions;

public sealed class OrchestrationKnnStore
{
    private readonly List<OrchestrationTrainingPair> _pairs = [];

    public int Count => _pairs.Count;

    public void Add(OrchestrationTrainingPair pair)
    {
        if (_pairs.Any(existing => existing.PairId.Equals(pair.PairId, StringComparison.Ordinal)))
            return;

        _pairs.Add(pair);
    }

    public IReadOnlyList<OrchestrationTrainingPair> ExportPairs()
        => _pairs.OrderBy(pair => pair.PairId, StringComparer.Ordinal).ToArray();

    public void ImportPairs(IEnumerable<OrchestrationTrainingPair> pairs)
    {
        _pairs.Clear();
        foreach (var pair in pairs)
            Add(pair);
    }

    public OrchestrationTrainingPair? FindNearest(string prompt, PairMode? mode = null)
        => FindNearestWithScore(prompt, mode)?.Pair;

    public OrchestrationNearestPair? FindNearestWithScore(string prompt, PairMode? mode = null)
    {
        var promptTokens = Tokenize(prompt).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return _pairs
            .Where(pair => mode is null || pair.Mode == mode)
            .Select(pair => new OrchestrationNearestPair(
                pair,
                Jaccard(promptTokens, Tokenize(pair.Prompt).ToHashSet(StringComparer.OrdinalIgnoreCase))))
            .OrderByDescending(item => item.Score)
            .ThenBy(item => item.Pair.PairId, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    public static IReadOnlyList<string> Tokenize(string text)
        => Regex.Matches(text ?? string.Empty, @"[\p{L}\p{N}]+")
            .Select(match => match.Value.ToLowerInvariant())
            .Where(token => token.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    private static double Jaccard(IReadOnlySet<string> left, IReadOnlySet<string> right)
    {
        if (left.Count == 0 && right.Count == 0)
            return 1.0;
        if (left.Count == 0 || right.Count == 0)
            return 0.0;

        var intersection = left.Intersect(right, StringComparer.OrdinalIgnoreCase).Count();
        var union = left.Union(right, StringComparer.OrdinalIgnoreCase).Count();
        return union == 0 ? 0.0 : intersection / (double)union;
    }
}

public sealed record OrchestrationNearestPair(
    OrchestrationTrainingPair Pair,
    double Score);
