// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler;

// DependencyGraph detects ordering constraints between task segments.
// E35: +30% dependency_accuracy — determines A→B (sequential) vs A||B (parallel).
// Uses role-based rules: ARCH before CODE, READ before WRITE.
public sealed class DependencyGraph
{
    // Dependency: prerequisite role must complete before dependent role
    private readonly Dictionary<string, HashSet<string>> _deps
        = new(StringComparer.OrdinalIgnoreCase);

    public void AddDependency(string prerequisiteRole, string dependentRole)
    {
        if (!_deps.TryGetValue(prerequisiteRole, out var set))
            _deps[prerequisiteRole] = set = new(StringComparer.OrdinalIgnoreCase);
        set.Add(dependentRole);
    }

    // Topological sort by BFS (Breadth-First Search) levels.
    // Each level is a group of segments that can execute in parallel.
    // Returns DEPENDENCY_CYCLE on circular dependencies.
    public OperationResult<IReadOnlyList<IReadOnlyList<PromptSegment>>> Sort(
        IReadOnlyList<PromptSegment> segments)
    {
        // Assign role to each segment using op-family heuristic
        var byRole = segments
            .GroupBy(s => InferRole(s.Text), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        var inDegree = byRole.Keys.ToDictionary(r => r, _ => 0, StringComparer.OrdinalIgnoreCase);

        foreach (var (prereq, dependents) in _deps)
        {
            if (!byRole.ContainsKey(prereq)) continue;
            foreach (var dep in dependents)
                if (inDegree.ContainsKey(dep))
                    inDegree[dep]++;
        }

        var queue = new Queue<string>(
            inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));

        var levels    = new List<IReadOnlyList<PromptSegment>>();
        int processed = 0;

        while (queue.Count > 0)
        {
            int count      = queue.Count;
            var levelRoles = new List<string>(count);
            for (int i = 0; i < count; i++)
                levelRoles.Add(queue.Dequeue());

            var levelSegs = levelRoles.SelectMany(r => byRole[r]).ToList();
            levels.Add(levelSegs);
            processed += levelSegs.Count;

            foreach (var role in levelRoles)
            {
                if (!_deps.TryGetValue(role, out var dependents)) continue;
                foreach (var dep in dependents)
                {
                    if (!inDegree.ContainsKey(dep)) continue;
                    if (--inDegree[dep] == 0) queue.Enqueue(dep);
                }
            }
        }

        if (processed != segments.Count)
            return OperationResult<IReadOnlyList<IReadOnlyList<PromptSegment>>>.Fail(
                "Circular dependency detected", "DEPENDENCY_CYCLE");

        return OperationResult<IReadOnlyList<IReadOnlyList<PromptSegment>>>.Ok(levels);
    }

    // Infer role from segment text using op-family profile
    private static string InferRole(string text)
    {
        var ops = FamilyCollapser.CollapseString(text);
        if (ops.Contains("GET_OP"))    return "reader";
        if (ops.Contains("UPDATE_OP") || ops.Contains("CREATE_OP")) return "coder";
        if (ops.Contains("PROCESS_OP")) return "refactorer";
        if (ops.Contains("VALIDATE_OP")) return "tester";
        return "coder";
    }

    // Default dependency set validated by E35 + E49 experiments
    public static DependencyGraph BuildDefault()
    {
        var g = new DependencyGraph();
        g.AddDependency("reader", "coder");       // code changes wait for reads
        g.AddDependency("reader", "refactorer");  // refactoring needs read first
        return g;
    }
}
