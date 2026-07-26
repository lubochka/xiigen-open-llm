// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// This unit applies one add-phase topology mutation through the topology registry.
public sealed class AddPhaseTopologyMutationUnit : ILogicalUnit<AddPhaseCommand, LogicalPhaseEntry>
{
    // This id identifies the add-phase mutation unit in evidence and registries.
    public const string Id = "unit-topology-add-phase-v1";

    // This role names the topology phase mutation responsibility.
    public const string Role = "topology_phase_mutation";

    // This domain tag scopes the unit to logical topology management.
    public const string DomainTag = "logical_topology";

    // This confidence value marks deterministic registry mutation as fully trusted after validation.
    public const double Confidence = 1.0;

    // The registry is nullable so invalid construction can fail through ExecuteAsync instead of throwing.
    private readonly ILogicalTopologyRegistry? _registry;

    // Create the unit with the registry it mutates.
    public AddPhaseTopologyMutationUnit(ILogicalTopologyRegistry? registry)
    {
        // Store the registry for execution.
        _registry = registry;
    }

    // Execute one add-phase command through the shared logical-unit contract.
    public Task<OperationResult<LogicalUnitResult<LogicalPhaseEntry>>> ExecuteAsync(
        LogicalUnitRequest<AddPhaseCommand> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalPhaseEntry>>.Fail("Cancelled.", "CANCELLED"));

        // Reject invalid construction before using the registry.
        if (_registry is null)
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalPhaseEntry>>.Fail("Topology registry is required.", "TOPOLOGY_REGISTRY_REQUIRED"));

        // Reject missing commands because the unit cannot infer a topology mutation.
        if (request.Input is null)
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalPhaseEntry>>.Fail("Add-phase command is required.", "COMMAND_REQUIRED"));

        // Reject empty reasons because topology mutations must be auditable.
        if (string.IsNullOrWhiteSpace(request.Input.Reason))
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalPhaseEntry>>.Fail("Mutation reason is required.", "EMPTY_REASON"));

        // Apply the phase mutation through the registry contract.
        var stored = _registry.AddOrUpdatePhase(request.Input.Phase);

        // Return registry validation failures through the logical-unit envelope.
        if (!stored.IsSuccess)
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalPhaseEntry>>.Fail(stored.ErrorMessage, stored.ErrorCode));

        // Read the stored phase from the registry result.
        var phase = stored.Value!.Phase;

        // Build the unit result from the stored registry entry.
        var result = new LogicalUnitResult<LogicalPhaseEntry>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            stored.Value,
            Confidence,
            phase.PredicateIds,
            phase.DomainIds,
            phase.SourceIds,
            []);

        // Return the mutation result through the shared logical-unit envelope.
        return Task.FromResult(OperationResult<LogicalUnitResult<LogicalPhaseEntry>>.Ok(result));
    }
}
