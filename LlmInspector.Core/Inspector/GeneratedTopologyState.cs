// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

using AgentInfrastructure.Logical;

// This record describes one topology node generated from completed station output.
public sealed record GeneratedTopologyNode(
    string Key,
    string Kind,
    string Id,
    string Name,
    string Purpose,
    string? ParentKey);

// This record describes one transition edge generated from completed station output.
public sealed record GeneratedTopologyTransition(
    string Key,
    string TransitionId,
    string FromKey,
    string ToKey,
    string Purpose);

// This snapshot exposes generated topology state without exposing mutable collections.
public sealed record GeneratedTopologySnapshot(
    IReadOnlyList<GeneratedTopologyNode> Nodes,
    IReadOnlyList<GeneratedTopologyTransition> Transitions,
    int PendingPhaseCount,
    int PendingSubPhaseCount,
    int PendingTransitionCount);

// This state machine turns completed trace outputs into generated topology nodes and edges.
public sealed class GeneratedTopologyState
{
    // Generated topology nodes are keyed by normalized kind and id.
    private readonly Dictionary<string, GeneratedTopologyNode> _nodes = new(StringComparer.Ordinal);

    // Generated transition edges are keyed by transition id.
    private readonly Dictionary<string, GeneratedTopologyTransition> _transitions = new(StringComparer.Ordinal);

    // Pending phases wait here until their generated domain parent appears.
    private readonly List<LogicalPhaseEntry> _pendingPhases = [];

    // Pending sub-phases wait here until their generated phase parent appears.
    private readonly List<LogicalSubPhaseEntry> _pendingSubPhases = [];

    // Pending transitions wait here until both generated endpoints appear.
    private readonly List<LogicalTransitionEntry> _pendingTransitions = [];

    // Snapshot returns a stable read-only view of generated topology state.
    public GeneratedTopologySnapshot Snapshot()
    {
        // Read generated nodes in deterministic display order.
        var nodes = _nodes.Values
            .OrderBy(node => NodeSortOrder(node.Kind))
            .ThenBy(node => node.Id, StringComparer.Ordinal)
            .ToArray();

        // Read generated transitions in deterministic id order.
        var transitions = _transitions.Values
            .OrderBy(transition => transition.TransitionId, StringComparer.Ordinal)
            .ToArray();

        // Return the state and pending counts as one immutable snapshot.
        return new GeneratedTopologySnapshot(
            nodes,
            transitions,
            _pendingPhases.Count,
            _pendingSubPhases.Count,
            _pendingTransitions.Count);
    }

    // ApplyTrace reads one completed station trace and updates generated topology state.
    public bool ApplyTrace(StationTrace trace)
    {
        // Ignore in-flight traces because they do not have generated output yet.
        if (trace.CompletedAt is null)
            return false;

        // Ignore traces without output because there is nothing to inspect.
        if (trace.Output is null)
            return false;

        // Apply the completed output tree.
        return ApplyOutput(trace.Output);
    }

    // ApplyOutput reads one completed output property tree and updates generated topology state.
    public bool ApplyOutput(IReadOnlyList<PropertyNode> output)
    {
        // Track whether this output changed generated topology state.
        var changed = false;

        // Add a domain node when the output contains a domain entry.
        if (TryFindDomainEntry(output, out var domainEntry))
            changed |= AddDomain(domainEntry);

        // Add a phase node or buffer it when its generated parent has not arrived yet.
        if (TryFindPhaseEntry(output, out var phaseEntry))
            changed |= AddPhase(phaseEntry);

        // Add a sub-phase node or buffer it when its generated parent has not arrived yet.
        if (TryFindSubPhaseEntry(output, out var subPhaseEntry))
            changed |= AddSubPhase(subPhaseEntry);

        // Add a transition edge or buffer it when either generated endpoint has not arrived yet.
        if (TryFindTransitionEntry(output, out var transitionEntry))
            changed |= AddTransition(transitionEntry);

        // Give pending entries a chance to attach after this output changed state.
        changed |= FlushPendingTopology();

        // Return whether generated topology state changed.
        return changed;
    }

    // AddDomain stores a domain node when this domain has not already been observed.
    private bool AddDomain(LogicalDomainEntry entry)
    {
        // Read the generated domain id from the trace output.
        var domainId = entry.Domain.DomainId;

        // Build the normalized state key for this generated domain.
        var key = TopologyNodeKey("domain", domainId);

        // Treat duplicate domain observations as already satisfied without reporting a change.
        if (_nodes.ContainsKey(key))
            return false;

        // Store the generated domain node for later rendering.
        _nodes[key] = new GeneratedTopologyNode(key, "domain", domainId, entry.Domain.Name, entry.Domain.Purpose, ParentKey: null);

        // Report that generated topology state changed.
        return true;
    }

    // AddPhase stores a phase node beneath its first known domain parent.
    private bool AddPhase(LogicalPhaseEntry entry, bool bufferWhenMissing = true)
    {
        // Read the generated phase id from the trace output.
        var phaseId = entry.Phase.PhaseId;

        // Build the normalized state key for this generated phase.
        var key = TopologyNodeKey("phase", phaseId);

        // Treat duplicate phase observations as already satisfied without reporting a change.
        if (_nodes.ContainsKey(key))
            return false;

        // Read the first parent domain id from the generated output.
        var domainId = entry.Phase.DomainIds.FirstOrDefault();

        // Buffer the phase when its generated parent domain has not arrived yet.
        if (domainId is null || !_nodes.ContainsKey(TopologyNodeKey("domain", domainId)))
            return BufferPhase(entry, bufferWhenMissing);

        // Build the normalized parent key from the generated parent id.
        var parentKey = TopologyNodeKey("domain", domainId);

        // Store the generated phase node under its parent domain.
        _nodes[key] = new GeneratedTopologyNode(key, "phase", phaseId, entry.Phase.Name, entry.Phase.Purpose, parentKey);

        // Report that generated topology state changed.
        return true;
    }

    // AddSubPhase stores a sub-phase node beneath its known phase parent.
    private bool AddSubPhase(LogicalSubPhaseEntry entry, bool bufferWhenMissing = true)
    {
        // Read the generated sub-phase id from the trace output.
        var subPhaseId = entry.SubPhase.SubPhaseId;

        // Build the normalized state key for this generated sub-phase.
        var key = TopologyNodeKey("subphase", subPhaseId);

        // Treat duplicate sub-phase observations as already satisfied without reporting a change.
        if (_nodes.ContainsKey(key))
            return false;

        // Build the normalized parent phase key from the generated output.
        var parentKey = TopologyNodeKey("phase", entry.SubPhase.PhaseId);

        // Buffer the sub-phase when its generated parent phase has not arrived yet.
        if (!_nodes.ContainsKey(parentKey))
            return BufferSubPhase(entry, bufferWhenMissing);

        // Store the generated sub-phase node under its parent phase.
        _nodes[key] = new GeneratedTopologyNode(key, "subphase", subPhaseId, entry.SubPhase.Name, entry.SubPhase.Purpose, parentKey);

        // Report that generated topology state changed.
        return true;
    }

    // AddTransition stores a transition edge when both generated endpoints are known.
    private bool AddTransition(LogicalTransitionEntry entry, bool bufferWhenMissing = true)
    {
        // Read the generated transition id from the trace output.
        var transitionId = entry.Transition.TransitionId;

        // Treat duplicate transition observations as already satisfied without reporting a change.
        if (_transitions.ContainsKey(transitionId))
            return false;

        // Build the generated source node key from the transition output.
        var fromKey = TopologyNodeKey(entry.Transition.FromTopologyKind, entry.Transition.FromTopologyId);

        // Build the generated target node key from the transition output.
        var toKey = TopologyNodeKey(entry.Transition.ToTopologyKind, entry.Transition.ToTopologyId);

        // Buffer the transition when the source endpoint has not arrived yet.
        if (!_nodes.ContainsKey(fromKey))
            return BufferTransition(entry, bufferWhenMissing);

        // Buffer the transition when the target endpoint has not arrived yet.
        if (!_nodes.ContainsKey(toKey))
            return BufferTransition(entry, bufferWhenMissing);

        // Build the normalized transition key.
        var key = "transition:" + transitionId;

        // Store the generated transition edge for later rendering.
        _transitions[transitionId] = new GeneratedTopologyTransition(key, transitionId, fromKey, toKey, entry.Transition.Purpose);

        // Report that generated topology state changed.
        return true;
    }

    // BufferPhase stores an out-of-order phase until its parent domain appears.
    private bool BufferPhase(LogicalPhaseEntry entry, bool bufferWhenMissing)
    {
        // Report unresolved when the caller only wants to test pending entries.
        if (!bufferWhenMissing)
            return false;

        // Add the pending phase only once.
        if (!_pendingPhases.Any(candidate => string.Equals(candidate.Phase.PhaseId, entry.Phase.PhaseId, StringComparison.Ordinal)))
            _pendingPhases.Add(entry);

        // Report that buffering changed internal topology state.
        return true;
    }

    // BufferSubPhase stores an out-of-order sub-phase until its parent phase appears.
    private bool BufferSubPhase(LogicalSubPhaseEntry entry, bool bufferWhenMissing)
    {
        // Report unresolved when the caller only wants to test pending entries.
        if (!bufferWhenMissing)
            return false;

        // Add the pending sub-phase only once.
        if (!_pendingSubPhases.Any(candidate => string.Equals(candidate.SubPhase.SubPhaseId, entry.SubPhase.SubPhaseId, StringComparison.Ordinal)))
            _pendingSubPhases.Add(entry);

        // Report that buffering changed internal topology state.
        return true;
    }

    // BufferTransition stores an out-of-order transition until both endpoints appear.
    private bool BufferTransition(LogicalTransitionEntry entry, bool bufferWhenMissing)
    {
        // Report unresolved when the caller only wants to test pending entries.
        if (!bufferWhenMissing)
            return false;

        // Add the pending transition only once.
        if (!_pendingTransitions.Any(candidate => string.Equals(candidate.Transition.TransitionId, entry.Transition.TransitionId, StringComparison.Ordinal)))
            _pendingTransitions.Add(entry);

        // Report that buffering changed internal topology state.
        return true;
    }

    // FlushPendingTopology attaches buffered children and transitions whenever parents have arrived.
    private bool FlushPendingTopology()
    {
        // Track whether any pending entry became visible.
        var anyProgress = false;

        // Continue flushing while one attachment may unlock another attachment.
        var progressed = true;

        // Keep trying until a full pass attaches nothing.
        while (progressed)
        {
            // Reset progress at the start of each pass.
            progressed = false;

            // Try to attach pending phases in reverse order so removals are safe.
            for (var index = _pendingPhases.Count - 1; index >= 0; index--)
            {
                // Attach this phase only when its parent domain is now known.
                if (!AddPhase(_pendingPhases[index], bufferWhenMissing: false))
                    continue;

                // Remove the phase from the pending buffer after it attaches.
                _pendingPhases.RemoveAt(index);

                // Mark progress so dependent sub-phases get another pass.
                progressed = true;

                // Mark that visible topology changed.
                anyProgress = true;
            }

            // Try to attach pending sub-phases in reverse order so removals are safe.
            for (var index = _pendingSubPhases.Count - 1; index >= 0; index--)
            {
                // Attach this sub-phase only when its parent phase is now known.
                if (!AddSubPhase(_pendingSubPhases[index], bufferWhenMissing: false))
                    continue;

                // Remove the sub-phase from the pending buffer after it attaches.
                _pendingSubPhases.RemoveAt(index);

                // Mark progress so dependent transitions get another pass.
                progressed = true;

                // Mark that visible topology changed.
                anyProgress = true;
            }

            // Try to attach pending transitions in reverse order so removals are safe.
            for (var index = _pendingTransitions.Count - 1; index >= 0; index--)
            {
                // Attach this transition only when both endpoints are now known.
                if (!AddTransition(_pendingTransitions[index], bufferWhenMissing: false))
                    continue;

                // Remove the transition from the pending buffer after it attaches.
                _pendingTransitions.RemoveAt(index);

                // Mark progress in case another pending edge depends on this attachment.
                progressed = true;

                // Mark that visible topology changed.
                anyProgress = true;
            }
        }

        // Return whether pending entries became visible.
        return anyProgress;
    }

    // TopologyNodeKey normalizes generated topology references to state lookup keys.
    public static string TopologyNodeKey(string topologyKind, string topologyId)
    {
        // Normalize the generated kind so different naming styles resolve to the same topology type.
        var kind = topologyKind.Trim().Replace("_", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal).ToLowerInvariant();

        // Choose the topology kind for known generated topology element names.
        var normalizedKind = kind switch
        {
            "logicaldomain" or "domain" => "domain",
            "logicalphase" or "phase" => "phase",
            "logicalsubphase" or "subphase" => "subphase",
            _ => kind
        };

        // Return the generated lookup key.
        return normalizedKind + ":" + topologyId;
    }

    // NodeSortOrder keeps higher-level topology nodes before lower-level nodes.
    private static int NodeSortOrder(string kind)
    {
        // Return the sort order for known topology kinds.
        return kind switch
        {
            "domain" => 0,
            "phase" => 1,
            "subphase" => 2,
            _ => 3
        };
    }

    // TryFindDomainEntry searches the full output tree for a domain entry.
    private static bool TryFindDomainEntry(IReadOnlyList<PropertyNode> nodes, out LogicalDomainEntry entry)
    {
        // Initialize the out value for the not-found path.
        entry = default!;

        // Examine every node in this tree level.
        foreach (var node in nodes)
        {
            // Try to parse this node as a domain branch.
            if (TryBuildDomainEntry(node, out entry))
                return true;

            // Search child nodes recursively.
            if (TryFindDomainEntry(node.Children, out entry))
                return true;
        }

        // Report that no domain entry was found.
        return false;
    }

    // TryFindPhaseEntry searches the full output tree for a phase entry.
    private static bool TryFindPhaseEntry(IReadOnlyList<PropertyNode> nodes, out LogicalPhaseEntry entry)
    {
        // Initialize the out value for the not-found path.
        entry = default!;

        // Examine every node in this tree level.
        foreach (var node in nodes)
        {
            // Try to parse this node as a phase branch.
            if (TryBuildPhaseEntry(node, out entry))
                return true;

            // Search child nodes recursively.
            if (TryFindPhaseEntry(node.Children, out entry))
                return true;
        }

        // Report that no phase entry was found.
        return false;
    }

    // TryFindSubPhaseEntry searches the full output tree for a sub-phase entry.
    private static bool TryFindSubPhaseEntry(IReadOnlyList<PropertyNode> nodes, out LogicalSubPhaseEntry entry)
    {
        // Initialize the out value for the not-found path.
        entry = default!;

        // Examine every node in this tree level.
        foreach (var node in nodes)
        {
            // Try to parse this node as a sub-phase branch.
            if (TryBuildSubPhaseEntry(node, out entry))
                return true;

            // Search child nodes recursively.
            if (TryFindSubPhaseEntry(node.Children, out entry))
                return true;
        }

        // Report that no sub-phase entry was found.
        return false;
    }

    // TryFindTransitionEntry searches the full output tree for a transition entry.
    private static bool TryFindTransitionEntry(IReadOnlyList<PropertyNode> nodes, out LogicalTransitionEntry entry)
    {
        // Initialize the out value for the not-found path.
        entry = default!;

        // Examine every node in this tree level.
        foreach (var node in nodes)
        {
            // Try to parse this node as a transition branch.
            if (TryBuildTransitionEntry(node, out entry))
                return true;

            // Search child nodes recursively.
            if (TryFindTransitionEntry(node.Children, out entry))
                return true;
        }

        // Report that no transition entry was found.
        return false;
    }

    // TryBuildDomainEntry converts one property-node branch into a lightweight domain entry.
    private static bool TryBuildDomainEntry(PropertyNode node, out LogicalDomainEntry entry)
    {
        // Initialize the out value for the not-found path.
        entry = default!;

        // Only a node named Domain can represent a LogicalDomain value.
        if (!string.Equals(node.Name, "Domain", StringComparison.Ordinal))
            return false;

        // Read the required domain id child.
        var domainId = ChildValue(node, "DomainId");

        // Reject domain branches without an id.
        if (string.IsNullOrWhiteSpace(domainId))
            return false;

        // Read the optional display name.
        var name = ChildValue(node, "Name") ?? domainId;

        // Read the optional purpose.
        var purpose = ChildValue(node, "Purpose") ?? string.Empty;

        // Build the lightweight domain entry used by generated topology state.
        entry = new LogicalDomainEntry(
            new LogicalDomain(domainId, name, purpose, [], []),
            0,
            DateTimeOffset.UtcNow);

        // Report that a domain entry was parsed.
        return true;
    }

    // TryBuildPhaseEntry converts one property-node branch into a lightweight phase entry.
    private static bool TryBuildPhaseEntry(PropertyNode node, out LogicalPhaseEntry entry)
    {
        // Initialize the out value for the not-found path.
        entry = default!;

        // Only a node named Phase can represent a LogicalPhase value.
        if (!string.Equals(node.Name, "Phase", StringComparison.Ordinal))
            return false;

        // Read the required phase id child.
        var phaseId = ChildValue(node, "PhaseId");

        // Reject phase branches without an id.
        if (string.IsNullOrWhiteSpace(phaseId))
            return false;

        // Read the optional display name.
        var name = ChildValue(node, "Name") ?? phaseId;

        // Read the optional purpose.
        var purpose = ChildValue(node, "Purpose") ?? string.Empty;

        // Read parent domain ids from the generated output tree.
        var domainIds = ChildArrayValues(node, "DomainIds");

        // Build the lightweight phase entry used by generated topology state.
        entry = new LogicalPhaseEntry(
            new LogicalPhase(phaseId, name, purpose, domainIds, [], []),
            0,
            DateTimeOffset.UtcNow);

        // Report that a phase entry was parsed.
        return true;
    }

    // TryBuildSubPhaseEntry converts one property-node branch into a lightweight sub-phase entry.
    private static bool TryBuildSubPhaseEntry(PropertyNode node, out LogicalSubPhaseEntry entry)
    {
        // Initialize the out value for the not-found path.
        entry = default!;

        // Only a node named SubPhase can represent a LogicalSubPhase value.
        if (!string.Equals(node.Name, "SubPhase", StringComparison.Ordinal))
            return false;

        // Read the required sub-phase id child.
        var subPhaseId = ChildValue(node, "SubPhaseId");

        // Reject sub-phase branches without an id.
        if (string.IsNullOrWhiteSpace(subPhaseId))
            return false;

        // Read the required parent phase id child.
        var phaseId = ChildValue(node, "PhaseId");

        // Reject sub-phase branches without a parent phase id.
        if (string.IsNullOrWhiteSpace(phaseId))
            return false;

        // Read the optional display name.
        var name = ChildValue(node, "Name") ?? subPhaseId;

        // Read the optional purpose.
        var purpose = ChildValue(node, "Purpose") ?? string.Empty;

        // Read domain ids from the generated output tree.
        var domainIds = ChildArrayValues(node, "DomainIds");

        // Build the lightweight sub-phase entry used by generated topology state.
        entry = new LogicalSubPhaseEntry(
            new LogicalSubPhase(subPhaseId, phaseId, name, purpose, domainIds, [], []),
            0,
            DateTimeOffset.UtcNow);

        // Report that a sub-phase entry was parsed.
        return true;
    }

    // TryBuildTransitionEntry converts one property-node branch into a lightweight transition entry.
    private static bool TryBuildTransitionEntry(PropertyNode node, out LogicalTransitionEntry entry)
    {
        // Initialize the out value for the not-found path.
        entry = default!;

        // Only a node named Transition can represent a LogicalTransition value.
        if (!string.Equals(node.Name, "Transition", StringComparison.Ordinal))
            return false;

        // Read the required transition id child.
        var transitionId = ChildValue(node, "TransitionId");

        // Reject transition branches without an id.
        if (string.IsNullOrWhiteSpace(transitionId))
            return false;

        // Read the required source topology kind.
        var fromTopologyKind = ChildValue(node, "FromTopologyKind");

        // Reject transition branches without a source topology kind.
        if (string.IsNullOrWhiteSpace(fromTopologyKind))
            return false;

        // Read the required source topology id.
        var fromTopologyId = ChildValue(node, "FromTopologyId");

        // Reject transition branches without a source topology id.
        if (string.IsNullOrWhiteSpace(fromTopologyId))
            return false;

        // Read the required target topology kind.
        var toTopologyKind = ChildValue(node, "ToTopologyKind");

        // Reject transition branches without a target topology kind.
        if (string.IsNullOrWhiteSpace(toTopologyKind))
            return false;

        // Read the required target topology id.
        var toTopologyId = ChildValue(node, "ToTopologyId");

        // Reject transition branches without a target topology id.
        if (string.IsNullOrWhiteSpace(toTopologyId))
            return false;

        // Read the optional purpose.
        var purpose = ChildValue(node, "Purpose") ?? string.Empty;

        // Read predicate ids from the generated output tree.
        var predicateIds = ChildArrayValues(node, "PredicateIds");

        // Read constraint ids from the generated output tree.
        var constraintIds = ChildArrayValues(node, "ConstraintIds");

        // Read source ids from the generated output tree.
        var sourceIds = ChildArrayValues(node, "SourceIds");

        // Build the lightweight transition entry used by generated topology state.
        entry = new LogicalTransitionEntry(
            new LogicalTransition(transitionId, fromTopologyKind, fromTopologyId, toTopologyKind, toTopologyId, purpose, predicateIds, constraintIds, sourceIds),
            0,
            DateTimeOffset.UtcNow);

        // Report that a transition entry was parsed.
        return true;
    }

    // ChildValue returns one named scalar child value from a property-node branch.
    private static string? ChildValue(PropertyNode node, string childName)
    {
        // Find the requested child node by exact name.
        var child = node.Children.FirstOrDefault(candidate => string.Equals(candidate.Name, childName, StringComparison.Ordinal));

        // Return the scalar value if the child exists.
        return child?.Value;
    }

    // ChildArrayValues returns indexed string values from one named child branch.
    private static string[] ChildArrayValues(PropertyNode node, string childName)
    {
        // Find the requested array branch by exact name.
        var child = node.Children.FirstOrDefault(candidate => string.Equals(candidate.Name, childName, StringComparison.Ordinal));

        // Return an empty array when the branch is absent.
        if (child is null)
            return [];

        // Read indexed scalar children in order.
        return child.Children
            .Where(candidate => candidate.Value is not null)
            .Select(candidate => candidate.Value!)
            .ToArray();
    }
}
