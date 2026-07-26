// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace OrchestratorModel;

using AgentInfrastructure;

// In-memory episode history store.
// Stub implementation — real persistence comes with 14.ContinuousLearning.
// Inherits BaseService (Rule 2).
public sealed class InMemoryEpisodeMemory : BaseService, IEpisodeMemory
{
    // Stored history entries — evicts oldest when capacity is reached.
    private readonly List<(string Task, IReadOnlyList<AgentAlgorithms.SubTask> Decomposition)>
        _history = new();

    // Maximum number of stored entries before eviction begins.
    private readonly int _maxEntries;

    // FC-12: BaseService requires ISessionStorage as first constructor parameter.
    public InMemoryEpisodeMemory(ISessionStorage storage, int maxEntries = 1000)
        : base(storage)
        => _maxEntries = maxEntries;

    // Store a task and its decomposition. Evicts oldest when capacity exceeded.
    public OperationResult<bool> Store(
        string taskDescription, IReadOnlyList<AgentAlgorithms.SubTask> decomposition)
    {
        if (string.IsNullOrWhiteSpace(taskDescription))
            return OperationResult<bool>.Fail("Task description cannot be empty.", "EMPTY_TASK");

        // Evict the oldest entry when the buffer is full.
        if (_history.Count >= _maxEntries)
            _history.RemoveAt(0);

        _history.Add((taskDescription, decomposition));
        return OperationResult<bool>.Ok(true);
    }

    // Recall up to topK entries whose task description contains the query substring.
    public OperationResult<IReadOnlyList<string>> Recall(
        string taskDescription, int topK = 3)
    {
        var matches = _history
            .Where(h => h.Task.Contains(taskDescription, StringComparison.OrdinalIgnoreCase))
            .TakeLast(topK)
            .Select(h => h.Task)
            .ToList();
        return OperationResult<IReadOnlyList<string>>.Ok(matches);
    }
}
