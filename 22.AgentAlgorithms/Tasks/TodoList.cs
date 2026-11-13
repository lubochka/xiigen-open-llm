// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentAlgorithms;

// ─── Task complexity ──────────────────────────────────────────────────────────

public enum TaskComplexity { Simple, Medium, Complex }

// ─── Agent task item ──────────────────────────────────────────────────────────

public sealed record AgentTaskItem(
    string         Description,
    string         RequiredRole,
    TaskComplexity Complexity,
    bool           IsCompleted = false);

// ─── Todo list ────────────────────────────────────────────────────────────────

// Tracks the sub-tasks identified by the agent and their completion status.
public sealed class TodoList
{
    private readonly List<AgentTaskItem> _items = [];

    public IReadOnlyList<AgentTaskItem> Items => _items;

    public void Add(AgentTaskItem item) => _items.Add(item);

    // Mark the item at the given index as completed.
    public OperationResult<bool> Complete(int index)
    {
        if (index < 0 || index >= _items.Count)
            return OperationResult<bool>.Fail($"Index {index} is out of range.", "OUT_OF_RANGE");

        _items[index] = _items[index] with { IsCompleted = true };
        return OperationResult<bool>.Ok(true);
    }

    public int CompletedCount   => _items.Count(i => i.IsCompleted);
    public int PendingCount     => _items.Count(i => !i.IsCompleted);
    public bool AllCompleted    => _items.Count > 0 && _items.All(i => i.IsCompleted);
}
