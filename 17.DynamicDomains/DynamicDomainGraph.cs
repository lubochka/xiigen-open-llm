// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace DynamicDomains;

public sealed record DynamicDomainTransition(string FromId, string ToId);

// E50 production graph closure over sub-phase transition topology.
public static class DomainGraphClosure
{
    public static bool HasAnyDependency(
        IReadOnlyCollection<string> routedIds,
        IReadOnlyCollection<DynamicDomainTransition> transitions)
    {
        return BuildPredecessorMap(routedIds, transitions).Values.Any(value => value > 0);
    }

    public static IReadOnlyDictionary<string, int> BuildPredecessorMap(
        IReadOnlyCollection<string> routedIds,
        IReadOnlyCollection<DynamicDomainTransition> transitions)
    {
        var predecessorCounts = routedIds.ToDictionary(id => id, _ => 0, StringComparer.Ordinal);
        var adjacency = BuildAdjacency(transitions);
        foreach (var source in routedIds)
        {
            foreach (var target in ReachableTargets(source, adjacency))
            {
                if (!string.Equals(source, target, StringComparison.Ordinal) && predecessorCounts.ContainsKey(target))
                    predecessorCounts[target]++;
            }
        }

        return predecessorCounts;
    }

    internal static IReadOnlyDictionary<string, string[]> BuildAdjacency(IReadOnlyCollection<DynamicDomainTransition> transitions)
    {
        return transitions
            .GroupBy(transition => transition.FromId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group.Select(transition => transition.ToId).Distinct(StringComparer.Ordinal).ToArray(),
                StringComparer.Ordinal);
    }

    internal static IEnumerable<string> ReachableTargets(string source, IReadOnlyDictionary<string, string[]> adjacency)
    {
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var stack = new Stack<string>();
        stack.Push(source);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            if (!seen.Add(current))
                continue;

            yield return current;
            if (!adjacency.TryGetValue(current, out var next))
                continue;

            foreach (var target in next)
                stack.Push(target);
        }
    }
}

// E49 production topology expander over transition bridges.
public static class OrchestratorExpander
{
    public static string[] FindBridgeSubPhases(
        IReadOnlyCollection<string> routedIds,
        IReadOnlyCollection<DynamicDomainTransition> transitions)
    {
        if (routedIds.Count < 2 || transitions.Count == 0)
            return [];

        var routed = routedIds.ToHashSet(StringComparer.Ordinal);
        var adjacency = DomainGraphClosure.BuildAdjacency(transitions);
        var bridges = new HashSet<string>(StringComparer.Ordinal);
        foreach (var source in routed)
        {
            foreach (var target in routed.Where(id => !string.Equals(id, source, StringComparison.Ordinal)))
            {
                foreach (var bridge in FindPath(source, target, adjacency).Skip(1).SkipLast(1))
                    bridges.Add(bridge);
            }
        }

        return bridges.OrderBy(id => id, StringComparer.Ordinal).ToArray();
    }

    private static string[] FindPath(string source, string target, IReadOnlyDictionary<string, string[]> adjacency)
    {
        var queue = new Queue<string[]>();
        var seen = new HashSet<string>(StringComparer.Ordinal) { source };
        queue.Enqueue([source]);

        while (queue.Count > 0)
        {
            var path = queue.Dequeue();
            var current = path[^1];
            if (string.Equals(current, target, StringComparison.Ordinal))
                return path;
            if (!adjacency.TryGetValue(current, out var next))
                continue;

            foreach (var candidate in next)
            {
                if (!seen.Add(candidate))
                    continue;
                queue.Enqueue([.. path, candidate]);
            }
        }

        return [];
    }
}
