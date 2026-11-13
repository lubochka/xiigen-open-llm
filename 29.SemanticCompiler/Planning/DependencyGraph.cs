// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SemanticCompiler29;

// DependencyGraph: detect ordering and parallelism between domains.
// E35: +30.0% — correctly identifies A→B (sequential) vs A||B (parallel).
// Default edges (from E35 + E49 experiment findings):
//   PLAN → ARCH → CODE_Q    (planning before architecture before implementation)
//   PLAN → ARCH → REFACTOR  (planning before architecture before refactoring)
//   CODE_Q → BUG            (understand code before debugging it)
// This is the domain-level equivalent of 22.AgentAlgorithms.AgentOrchestrator.
// SemanticCompiler29 version operates on Domain[] not SubTask[].
public sealed class DependencyGraph : BaseService
{
    // Standard constructor: inject storage and logger without constructing them here.
    public DependencyGraph(ICompilerStorage storage, ICompilerEventLogger logger)
        : base(storage, logger) { }

    // Directed edges: prerequisite domain → set of domains that depend on it.
    private readonly Dictionary<Domain, HashSet<Domain>> _edges = [];

    // Add a directed edge: prerequisite must complete before dependent.
    public void AddEdge(Domain prerequisite, Domain dependent)
    {
        if (!_edges.TryGetValue(prerequisite, out var deps))
            _edges[prerequisite] = deps = [];
        deps.Add(dependent);
    }

    // Build the default dependency set validated by E35 + E49.
    public static DependencyGraph BuildDefault(ICompilerStorage storage, ICompilerEventLogger logger)
    {
        var g = new DependencyGraph(storage, logger);
        g.AddEdge(Domain.PLAN, Domain.ARCH);     // plan before architecture
        g.AddEdge(Domain.ARCH, Domain.CODE_Q);   // architecture before implementation
        g.AddEdge(Domain.ARCH, Domain.REFACTOR); // architecture before refactoring
        g.AddEdge(Domain.CODE_Q, Domain.BUG);    // understand code before debugging
        return g;
    }

    // Topological sort via BFS (Breadth-First Search) Kahn's algorithm.
    // Returns levels — each level contains domains that can execute in parallel.
    // Returns DEPENDENCY_CYCLE error code when a circular dependency is detected.
    public OperationResult<Domain[][]> Sort(Domain[] domains)
    {
        var present = new HashSet<Domain>(domains);
        var inDegree = present.ToDictionary(d => d, _ => 0);

        foreach (var (prereq, deps) in _edges)
        {
            if (!present.Contains(prereq)) continue;
            foreach (var dep in deps)
                if (inDegree.ContainsKey(dep))
                    inDegree[dep]++;
        }

        var queue = new Queue<Domain>(inDegree.Where(kv => kv.Value == 0).Select(kv => kv.Key));
        var levels = new List<Domain[]>();
        int processed = 0;

        while (queue.Count > 0)
        {
            int count = queue.Count;
            var level = new Domain[count];
            for (int i = 0; i < count; i++)
                level[i] = queue.Dequeue();

            levels.Add(level);
            processed += level.Length;

            foreach (var d in level)
            {
                if (!_edges.TryGetValue(d, out var deps)) continue;
                foreach (var dep in deps)
                {
                    if (!inDegree.ContainsKey(dep)) continue;
                    if (--inDegree[dep] == 0) queue.Enqueue(dep);
                }
            }
        }

        if (processed != domains.Length)
            return OperationResult<Domain[][]>.Fail(
                "Circular dependency detected", "DEPENDENCY_CYCLE");

        return OperationResult<Domain[][]>.Ok(levels.ToArray());
    }
}
