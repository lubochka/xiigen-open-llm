// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmInspector.Inspector;

using AgentInfrastructure;
using AgentInfrastructure.Logical;

// This decorator emits traces around today's concrete topology storage boundary.
public sealed class TracingStorageDecorator : ILogicalTopologyRegistry
{
    // The wrapped registry remains the source of topology storage behavior.
    private readonly ILogicalTopologyRegistry _inner;

    // This callback sends storage traces to the inspector session.
    private readonly Action<StationTrace> _emitTrace;

    // Create a tracing wrapper for one topology registry.
    public TracingStorageDecorator(ILogicalTopologyRegistry inner, Action<StationTrace> emitTrace)
    {
        // Store the concrete registry so mutations and reads still use production code.
        _inner = inner;

        // Store the trace callback so this decorator stays UI-neutral.
        _emitTrace = emitTrace;
    }

    // Add or update one domain while emitting a storage trace.
    public OperationResult<LogicalDomainEntry> AddOrUpdateDomain(LogicalDomain domain) =>
        Trace("InMemoryLogicalTopologyRegistry.AddOrUpdateDomain", "topology_storage_add_domain", domain, () => _inner.AddOrUpdateDomain(domain));

    // Resolve one domain while emitting a storage trace.
    public OperationResult<LogicalDomainEntry> ResolveDomain(string domainId) =>
        Trace("InMemoryLogicalTopologyRegistry.ResolveDomain", "topology_storage_resolve_domain", domainId, () => _inner.ResolveDomain(domainId));

    // List all domains while emitting a storage trace.
    public OperationResult<IReadOnlyList<LogicalDomainEntry>> ListDomains() =>
        Trace("InMemoryLogicalTopologyRegistry.ListDomains", "topology_storage_list_domains", "list-domains", () => _inner.ListDomains());

    // Add or update one phase while emitting a storage trace.
    public OperationResult<LogicalPhaseEntry> AddOrUpdatePhase(LogicalPhase phase) =>
        Trace("InMemoryLogicalTopologyRegistry.AddOrUpdatePhase", "topology_storage_add_phase", phase, () => _inner.AddOrUpdatePhase(phase));

    // Resolve one phase while emitting a storage trace.
    public OperationResult<LogicalPhaseEntry> ResolvePhase(string phaseId) =>
        Trace("InMemoryLogicalTopologyRegistry.ResolvePhase", "topology_storage_resolve_phase", phaseId, () => _inner.ResolvePhase(phaseId));

    // List all phases while emitting a storage trace.
    public OperationResult<IReadOnlyList<LogicalPhaseEntry>> ListPhases() =>
        Trace("InMemoryLogicalTopologyRegistry.ListPhases", "topology_storage_list_phases", "list-phases", () => _inner.ListPhases());

    // Add or update one sub-phase while emitting a storage trace.
    public OperationResult<LogicalSubPhaseEntry> AddOrUpdateSubPhase(LogicalSubPhase subPhase) =>
        Trace("InMemoryLogicalTopologyRegistry.AddOrUpdateSubPhase", "topology_storage_add_sub_phase", subPhase, () => _inner.AddOrUpdateSubPhase(subPhase));

    // Resolve one sub-phase while emitting a storage trace.
    public OperationResult<LogicalSubPhaseEntry> ResolveSubPhase(string subPhaseId) =>
        Trace("InMemoryLogicalTopologyRegistry.ResolveSubPhase", "topology_storage_resolve_sub_phase", subPhaseId, () => _inner.ResolveSubPhase(subPhaseId));

    // List all sub-phases while emitting a storage trace.
    public OperationResult<IReadOnlyList<LogicalSubPhaseEntry>> ListSubPhases() =>
        Trace("InMemoryLogicalTopologyRegistry.ListSubPhases", "topology_storage_list_sub_phases", "list-sub-phases", () => _inner.ListSubPhases());

    // Add or update one transition while emitting a storage trace.
    public OperationResult<LogicalTransitionEntry> AddOrUpdateTransition(LogicalTransition transition) =>
        Trace("InMemoryLogicalTopologyRegistry.AddOrUpdateTransition", "topology_storage_add_transition", transition, () => _inner.AddOrUpdateTransition(transition));

    // Resolve one transition while emitting a storage trace.
    public OperationResult<LogicalTransitionEntry> ResolveTransition(string transitionId) =>
        Trace("InMemoryLogicalTopologyRegistry.ResolveTransition", "topology_storage_resolve_transition", transitionId, () => _inner.ResolveTransition(transitionId));

    // List all transitions while emitting a storage trace.
    public OperationResult<IReadOnlyList<LogicalTransitionEntry>> ListTransitions() =>
        Trace("InMemoryLogicalTopologyRegistry.ListTransitions", "topology_storage_list_transitions", "list-transitions", () => _inner.ListTransitions());

    // Trace wraps one storage operation with matching start and completion events.
    private OperationResult<TOutput> Trace<TOutput>(
        string stationName,
        string stationRole,
        object? inputValue,
        Func<OperationResult<TOutput>> operation)
    {
        // Create the start trace from the inherited unit context.
        var start = CreateStartTrace(stationName, stationRole, inputValue);

        // Emit the storage start before the wrapped registry operation runs.
        _emitTrace(start);

        // Execute the wrapped storage operation exactly once.
        var result = operation();

        // Emit the completion trace with the full operation result.
        _emitTrace(CompleteTrace(start, result));

        // Return the wrapped result unchanged.
        return result;
    }

    // Create the in-flight station trace for one storage call.
    private static StationTrace CreateStartTrace(string stationName, string stationRole, object? inputValue)
    {
        // Read the current parent trace context.
        var context = TraceContextScope.Current;

        // Use the inherited run id or create one when storage is invoked directly.
        var runId = context?.RunId ?? Guid.NewGuid().ToString("N");

        // Use the inherited parent trace id so storage attaches below its calling unit.
        var parentTraceId = context?.ParentTraceId;

        // Use the inherited depth or root depth when storage is invoked directly.
        var depth = context?.Depth ?? 0;

        // Use the inherited wave index for sibling grouping.
        var waveIndex = context?.WaveIndex ?? 0;

        // Create a fresh trace id for this storage station.
        var traceId = Guid.NewGuid().ToString("N");

        // Serialise the storage input for the inspector input panel.
        var input = PropertyNodeSerialiser.Serialise(inputValue);

        // Return the start trace without output because the storage call is still running.
        return new StationTrace(
            traceId,
            parentTraceId,
            runId,
            depth,
            waveIndex,
            StationKind.Storage,
            stationName,
            stationRole,
            input,
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null,
            StationResourceSampler.Capture());
    }

    // Convert a wrapped storage result into the matching completion trace.
    private static StationTrace CompleteTrace<TOutput>(StationTrace start, OperationResult<TOutput> result)
    {
        // Serialise the full operation envelope so stored entries can be discovered from nested output fields.
        var output = PropertyNodeSerialiser.Serialise(result);

        // Copy the start trace identity and attach completion fields.
        return start with
        {
            Output = output,
            ErrorCode = result.IsSuccess ? null : result.ErrorCode,
            ErrorMessage = result.IsSuccess ? null : result.ErrorMessage,
            CompletedAt = DateTimeOffset.UtcNow,
            Resources = StationResourceSampler.Capture()
        };
    }
}
