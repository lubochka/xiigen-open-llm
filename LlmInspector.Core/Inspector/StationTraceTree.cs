// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

using System.Collections.ObjectModel;

// This tree stores station traces by trace id and updates existing cards on completion events.
public sealed class StationTraceTree
{
    // The roots collection contains every top-level orchestrator card.
    public ObservableCollection<StationCardViewModel> Roots { get; } = new();

    // The trace id index allows completion events to update the existing card.
    private readonly Dictionary<string, StationCardViewModel> _byTraceId = new(StringComparer.Ordinal);

    // Pending cards wait here when their parent start event has not arrived yet.
    private readonly List<StationCardViewModel> _pending = [];

    // Upsert accepts both start and completion events for a station.
    public void Upsert(StationTrace trace)
    {
        // Update the existing card when this is a completion event for a known trace id.
        if (_byTraceId.TryGetValue(trace.TraceId, out var existing))
        {
            // Apply the latest trace state to the existing card.
            existing.Update(trace);

            // Stop because completion must not add a duplicate card.
            return;
        }

        // Create a new card for the first event seen for this trace id.
        var card = new StationCardViewModel(trace);

        // Index the card immediately so later completion events can find it.
        _byTraceId[trace.TraceId] = card;

        // Attach root cards directly to the tree roots collection.
        if (trace.ParentTraceId is null)
        {
            // Add the root station card to the root collection.
            Roots.Add(card);
        }
        // Attach child cards when the parent is already known.
        else if (_byTraceId.TryGetValue(trace.ParentTraceId, out var parent))
        {
            // Add the child under the parent wave group.
            parent.AddChild(card);
        }
        // Defer child cards when their parent event arrives later.
        else
        {
            // Store the card until its parent is available.
            _pending.Add(card);
        }

        // Try to attach any pending cards that became attachable.
        FlushPending();
    }

    // FlushPending attaches cards whose parent has arrived since they were queued.
    private void FlushPending()
    {
        // Keep scanning while a pass attaches at least one pending card.
        bool flushed;

        // Repeat because attaching one parent may unlock its own children.
        do
        {
            // Start each pass assuming no pending card will be attached.
            flushed = false;

            // Walk backward so removals do not disturb remaining indices.
            for (var index = _pending.Count - 1; index >= 0; index--)
            {
                // Read the pending child card.
                var child = _pending[index];

                // Skip malformed non-root cards that still have no parent id.
                if (child.Trace.ParentTraceId is not { } parentTraceId)
                    continue;

                // Skip this child until its parent card exists.
                if (!_byTraceId.TryGetValue(parentTraceId, out var parent))
                    continue;

                // Attach the child under the parent.
                parent.AddChild(child);

                // Remove the child from the pending queue.
                _pending.RemoveAt(index);

                // Mark the pass as productive so nested pending children get another chance.
                flushed = true;
            }
        }
        while (flushed);
    }
}
