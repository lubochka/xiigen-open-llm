// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms;

using SemanticCompiler;

// Topological dependency graph for sub-agent roles (E35: ARCH→CODE_Q/REFACTOR ordering).
// AddDependency(prereq, dependent): roles with role=dependent must wait for role=prereq.
// Sort uses Kahn's BFS algorithm — each level contains tasks that can run in parallel.
// Roles absent from the input task list are ignored (their dependencies count as satisfied).
public sealed class DependencyGraph
{
    // Maps each prerequisite role to the set of roles that depend on it.
    private readonly Dictionary<string, HashSet<string>> _dependencies
        = new(StringComparer.OrdinalIgnoreCase);

    public void AddDependency(string prerequisiteRole, string dependentRole)
    {
        if (!_dependencies.TryGetValue(prerequisiteRole, out var deps))
        {
            deps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _dependencies[prerequisiteRole] = deps;
        }
        deps.Add(dependentRole);
    }

    public static bool IsSequentialPair(string taskA, string taskB)
    {
        var aTags = FamilyCollapser.CollapseString(taskA ?? string.Empty).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var bTags = FamilyCollapser.CollapseString(taskB ?? string.Empty).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var aCreates = aTags.Contains("CREATE_OP") || aTags.Contains("UPDATE_OP");
        var bGets = bTags.Contains("GET_OP");
        var overlap = aTags.Count(bTags.Contains) >= 2;
        return (aCreates && bGets) || overlap;
    }

    // Topological sort by BFS levels. Each level is a list of tasks that can run in parallel.
    // Returns DEPENDENCY_CYCLE if a cycle is detected.
    public OperationResult<IReadOnlyList<IReadOnlyList<SubTask>>> Sort(IReadOnlyList<SubTask> tasks)
    {
        // Group tasks by role (only roles present in the task list are considered)
        var byRole = tasks
            .GroupBy(t => t.RequiredRole, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

        // Compute in-degree: count prerequisite roles that have tasks in the input
        var inDegree = byRole.Keys.ToDictionary(r => r, _ => 0, StringComparer.OrdinalIgnoreCase);

        foreach (var (prereq, dependents) in _dependencies)
        {
            if (!byRole.ContainsKey(prereq)) continue; // prereq has no tasks → dependency satisfied
            foreach (var dep in dependents)
            {
                if (inDegree.ContainsKey(dep))
                    inDegree[dep]++;
            }
        }

        // Kahn's BFS: start with roles that have no unmet prerequisites
        var queue = new Queue<string>(
            inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));

        var levels   = new List<IReadOnlyList<SubTask>>();
        int processed = 0;

        while (queue.Count > 0)
        {
            // Collect all roles at the current level (those enqueued together)
            int count      = queue.Count;
            var levelRoles = new List<string>(count);
            for (int i = 0; i < count; i++)
                levelRoles.Add(queue.Dequeue());

            var levelTasks = levelRoles.SelectMany(r => byRole[r]).ToList();
            levels.Add(levelTasks);
            processed += levelTasks.Count;

            // Reduce in-degree of downstream roles
            foreach (var role in levelRoles)
            {
                if (!_dependencies.TryGetValue(role, out var dependents)) continue;
                foreach (var dep in dependents)
                {
                    if (!inDegree.ContainsKey(dep)) continue;
                    if (--inDegree[dep] == 0)
                        queue.Enqueue(dep);
                }
            }
        }

        if (processed != tasks.Count)
            return OperationResult<IReadOnlyList<IReadOnlyList<SubTask>>>.Fail(
                "Circular dependency detected between roles.", "DEPENDENCY_CYCLE");

        return OperationResult<IReadOnlyList<IReadOnlyList<SubTask>>>.Ok(levels);
    }
}
