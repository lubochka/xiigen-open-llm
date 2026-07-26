// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel;

using AgentInfrastructure;

// Breaks a task description into ordered sub-episodes.
// E32 +11%: PLAN → 4 sub-episodes, ARCH → 3, REFACTOR → 3, BUG → 2, CODE_Q → 1.
public sealed class EpisodeDecomposer : BaseService, IEpisodeDecomposer
{
    // Expected sub-episode count per domain (E32 validated counts).
    private static readonly Dictionary<TaskDomain, int> ExpectedSubEpisodes = new()
    {
        [TaskDomain.PLAN]     = 4,
        [TaskDomain.ARCH]     = 3,
        [TaskDomain.REFACTOR] = 3,
        [TaskDomain.BUG]      = 2,
        [TaskDomain.CODE_Q]   = 1,
    };

    // Verbs that start a false conjunction (right side depends on left side).
    // E.g. "find X and log them" — "log them" is NOT independent because "them" refers to X.
    private static readonly string[] FalseConjStarters =
        ["find", "check", "read", "get", "list", "show", "search", "look"];

    // FC-12: ISessionStorage first parameter → base(storage).
    public EpisodeDecomposer(ISessionStorage storage) : base(storage) { }

    // Decompose taskDescription into at most ExpectedSubEpisodes[domain] sub-tasks.
    public OperationResult<IReadOnlyList<AgentAlgorithms.SubTask>> Decompose(
        string taskDescription, TaskDomain domain)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return OperationResult<IReadOnlyList<AgentAlgorithms.SubTask>>.Fail(
                "Task description cannot be empty.", "EMPTY_TASK");

        int targetCount = ExpectedSubEpisodes.GetValueOrDefault(domain, 2);
        var parts       = SplitByConjunctions(taskDescription);

        // Assign domain-appropriate roles and cap at target count.
        var subTasks = parts
            .Take(targetCount)
            .Select((p, i) => new AgentAlgorithms.SubTask(p.Trim(), AssignRole(domain, i)))
            .ToList();

        // Fallback: if split produced nothing, treat the whole description as one task.
        if (subTasks.Count == 0)
            subTasks.Add(new AgentAlgorithms.SubTask(taskDescription, AssignRole(domain, 0)));

        return OperationResult<IReadOnlyList<AgentAlgorithms.SubTask>>.Ok(subTasks);
    }

    // Split at " and " / " then " / " after that " conjunctions.
    // A conjunction is kept only when the right side starts with an independent imperative verb.
    private static List<string> SplitByConjunctions(string text)
    {
        var parts = text.Split(
            [" and ", " then ", " after that "],
            StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length < 2) return [text];

        // Check only the second segment: if it starts with a false-conjunction verb the
        // right side depends on the left side and should not become a separate episode.
        bool secondIsIndependent = !FalseConjStarters.Any(
            v => parts[1].Trim().ToLower().StartsWith(v + " "));

        return secondIsIndependent ? parts.ToList() : [text];
    }

    // Assign a role to position i based on domain (E32 role mapping).
    private static string AssignRole(TaskDomain domain, int position) => domain switch
    {
        TaskDomain.ARCH     => position == 0 ? "architect"   : "implementer",
        TaskDomain.PLAN     => position == 0 ? "planner"     : "executor",
        TaskDomain.REFACTOR => position == 0 ? "implementer" : "reviewer",
        TaskDomain.BUG      => position == 0 ? "debugger"    : "implementer",
        _                   => "assistant",
    };
}
