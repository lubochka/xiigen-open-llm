// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

// This unit applies one add-sub-phase topology mutation through the topology registry.
public sealed class AddSubPhaseTopologyMutationUnit : ILogicalUnit<AddSubPhaseCommand, LogicalSubPhaseEntry>
{
    // This id identifies the add-sub-phase mutation unit in evidence and registries.
    public const string Id = "unit-topology-add-sub-phase-v1";

    // This role names the topology sub-phase mutation responsibility.
    public const string Role = "topology_sub_phase_mutation";

    // This domain tag scopes the unit to logical topology management.
    public const string DomainTag = "logical_topology";

    // This confidence value marks deterministic registry mutation as fully trusted after validation.
    public const double Confidence = 1.0;

    // The registry is nullable so invalid construction can fail through ExecuteAsync instead of throwing.
    private readonly ILogicalTopologyRegistry? _registry;

    // Create the unit with the registry it mutates.
    public AddSubPhaseTopologyMutationUnit(ILogicalTopologyRegistry? registry)
    {
        // Store the registry for execution.
        _registry = registry;
    }

    // Execute one add-sub-phase command through the shared logical-unit contract.
    public Task<OperationResult<LogicalUnitResult<LogicalSubPhaseEntry>>> ExecuteAsync(
        LogicalUnitRequest<AddSubPhaseCommand> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalSubPhaseEntry>>.Fail("Cancelled.", "CANCELLED"));

        // Reject invalid construction before using the registry.
        if (_registry is null)
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalSubPhaseEntry>>.Fail("Topology registry is required.", "TOPOLOGY_REGISTRY_REQUIRED"));

        // Reject missing commands because the unit cannot infer a topology mutation.
        if (request.Input is null)
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalSubPhaseEntry>>.Fail("Add-sub-phase command is required.", "COMMAND_REQUIRED"));

        // Reject empty reasons because topology mutations must be auditable.
        if (string.IsNullOrWhiteSpace(request.Input.Reason))
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalSubPhaseEntry>>.Fail("Mutation reason is required.", "EMPTY_REASON"));

        // Apply the sub-phase mutation through the registry contract.
        var stored = _registry.AddOrUpdateSubPhase(request.Input.SubPhase);

        // Return registry validation failures through the logical-unit envelope.
        if (!stored.IsSuccess)
            return Task.FromResult(OperationResult<LogicalUnitResult<LogicalSubPhaseEntry>>.Fail(stored.ErrorMessage, stored.ErrorCode));

        // Read the stored sub-phase from the registry result.
        var subPhase = stored.Value!.SubPhase;

        // Build the unit result from the stored registry entry.
        var result = new LogicalUnitResult<LogicalSubPhaseEntry>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            stored.Value,
            Confidence,
            subPhase.PredicateIds,
            subPhase.DomainIds,
            subPhase.SourceIds,
            []);

        // Return the mutation result through the shared logical-unit envelope.
        return Task.FromResult(OperationResult<LogicalUnitResult<LogicalSubPhaseEntry>>.Ok(result));
    }
}
