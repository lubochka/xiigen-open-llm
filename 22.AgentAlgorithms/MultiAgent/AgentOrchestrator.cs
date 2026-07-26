// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms;

// ─── Sub-task ─────────────────────────────────────────────────────────────────

public sealed record SubTask(string Description, string RequiredRole);

// ─── Agent orchestrator ───────────────────────────────────────────────────────

// Coordinates multiple specialized sub-agents to complete a complex task.
// Breaks the task into sub-tasks, sorts them by dependency levels (E35),
// executes each level in parallel with Task.WhenAll (E49), and propagates
// results from earlier levels as context to later levels.
public sealed class AgentOrchestrator
{
    private readonly Dictionary<string, SubAgentDefinition> _agents
        = new(StringComparer.OrdinalIgnoreCase);
    private readonly IAgentMessageBus     _bus;
    private readonly ComplexTaskDecomposer _decomposer;
    private readonly DependencyGraph       _deps;

    public AgentOrchestrator(
        IAgentMessageBus      bus,
        ComplexTaskDecomposer decomposer,
        DependencyGraph?      deps = null)
    {
        _bus        = bus;
        _decomposer = decomposer;
        _deps       = deps ?? BuildDefaultDependencies();
    }

    // Register a sub-agent so the orchestrator can route tasks to it.
    public void Register(SubAgentDefinition agent)
        => _agents[agent.Role] = agent;

    public async Task<OperationResult<string>> RunAsync(string task)
    {
        var decomposed = _decomposer.Decompose(task);
        if (!decomposed.IsSuccess)
            return OperationResult<string>.Fail(decomposed.ErrorMessage, "DECOMPOSITION_FAILED");

        var sortResult = _deps.Sort(decomposed.Value!.ToList());
        if (!sortResult.IsSuccess)
            return OperationResult<string>.Fail(sortResult.ErrorMessage, sortResult.ErrorCode);

        var accumulatedResults = new List<string>();

        foreach (var level in sortResult.Value!)
        {
            // Context from all earlier levels is forwarded to tasks in this level
            string context = accumulatedResults.Count > 0
                ? string.Join("\n", accumulatedResults)
                : string.Empty;

            // Run all tasks in this level concurrently (E49: Task.WhenAll per level)
            var levelTasks = level.Select(subTask => ExecuteSubTask(subTask, context));
            var levelResults = await Task.WhenAll(levelTasks);

            foreach (var r in levelResults)
            {
                if (!r.IsSuccess)
                    return OperationResult<string>.Fail(r.ErrorMessage, r.ErrorCode);
                accumulatedResults.Add(r.Value!);
            }
        }

        return OperationResult<string>.Ok(string.Join("\n\n", accumulatedResults));
    }

    private async Task<OperationResult<string>> ExecuteSubTask(SubTask subTask, string context)
    {
        if (!_agents.TryGetValue(subTask.RequiredRole, out var agent))
            return OperationResult<string>.Fail(
                $"No sub-agent registered for role '{subTask.RequiredRole}'.",
                "MISSING_SUBAGENT");

        string taskDesc = string.IsNullOrEmpty(context)
            ? subTask.Description
            : $"{subTask.Description}\n[Context: {context}]";

        return await _bus.AssignTaskAsync(agent.Name, taskDesc);
    }

    // Default dependency relationships from E35 and E49 experiments.
    private static DependencyGraph BuildDefaultDependencies()
    {
        var g = new DependencyGraph();
        g.AddDependency("reader", "coder");       // code changes wait for reads to complete
        g.AddDependency("architect", "coder");    // coding waits for architecture
        g.AddDependency("architect", "refactorer");
        return g;
    }
}
