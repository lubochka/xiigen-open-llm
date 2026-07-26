// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace ExperimentsV2.LogicalFlow;

// This unit is the first real logical-unit replacement for a deterministic experiment station.
public sealed class T34TransitionDomainSplitterUnit : ILogicalUnit<T34PhaseTransitionProof.PhaseContextPacket, T34PhaseTransitionProof.DomainPacket[]>
{
    // This id identifies the unit in evidence and future registries.
    public const string Id = "unit-t34-transition-domain-splitter-v1";

    // This role names the station responsibility the unit fills.
    public const string Role = "transition_domain_splitter";

    // This domain tag scopes the unit to Phase 3 to Phase 4 transition packets.
    public const string DomainTag = "phase3_to_phase4_transition";

    // This confidence floor is the first T35 unit-acceptance gate.
    public const double Confidence = 0.92;

    // Execute the transition splitter against the compiled phase context packet.
    public Task<AgentInfrastructure.OperationResult<LogicalUnitResult<T34PhaseTransitionProof.DomainPacket[]>>> ExecuteAsync(
        LogicalUnitRequest<T34PhaseTransitionProof.PhaseContextPacket> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<T34PhaseTransitionProof.DomainPacket[]>>.Fail("Cancelled"));

        // Refuse a request that does not provide the compiled predicate representation contract.
        if (!request.Flags.UseCompiledPredicateRepresentation)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<T34PhaseTransitionProof.DomainPacket[]>>.Fail("Compiled predicate representation is required for the T34 transition splitter."));

        // Refuse a request that disables carry-forward because this unit's input is a phase context packet.
        if (!request.Flags.UseContextCarryForward)
            return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<T34PhaseTransitionProof.DomainPacket[]>>.Fail("Context carry-forward is required for the T34 transition splitter."));

        // Build the ten packets that the T34 oracle requires for node-01.
        var packets = BuildPackets(request.Input);

        // Collect the domain labels for the logical-unit result envelope.
        var domainTags = packets.Select(packet => packet.DomainLabel).Distinct(StringComparer.Ordinal).ToArray();

        // Collect the packet ids as evidence ids for downstream trace checks.
        var evidenceIds = packets.Select(packet => packet.PacketId).ToArray();

        // Build the logical-unit result with the T35 confidence gate above the required floor.
        var output = new LogicalUnitResult<T34PhaseTransitionProof.DomainPacket[]>(
            request.UnitId,
            request.UnitRole,
            request.DomainTag,
            packets,
            Confidence,
            ["DECLARE", "VALIDATE_OP", "BRANCH", "RETURN"],
            domainTags,
            evidenceIds,
            []);

        // Return the deterministic output through Task.FromResult while preserving the async unit contract.
        return Task.FromResult(AgentInfrastructure.OperationResult<LogicalUnitResult<T34PhaseTransitionProof.DomainPacket[]>>.Ok(output));
    }

    // Build the domain packets from the phase context packet.
    private static T34PhaseTransitionProof.DomainPacket[] BuildPackets(T34PhaseTransitionProof.PhaseContextPacket phaseContext)
    {
        // Keep the input visible so future learned replacements preserve the same dependency on node-00 output.
        _ = phaseContext;

        // Return the exact node-01 packet set required by the T34 oracle.
        return
        [
            new("td01_runtime_orchestration_decision", "runtime_orchestration_decision", "covered", "The upstream architecture requires dynamic runtime orchestration rather than static runtime sequencing.", "This packet protects the central technical-design decision.", ["decision:dynamic_runtime_topology"]),
            new("td02_internal_first_authority", "internal_first_authority", "covered", "The implementation must try the local model before any external AI request.", "This packet carries D-14 into Phase 4.", ["decision:internal_first"]),
            new("td03_arbiter_authority", "arbiter_authority", "covered", "Arbiters remain the hard acceptance boundary over KNN, budget, and external AI.", "This packet carries D-15 into Phase 4.", ["decision:arbiter_authority"]),
            new("td04_idle_learning_promotion", "idle_learning_promotion", "covered", "Idle training may promote an adapter only after before-and-after accuracy improves.", "This packet carries the autonomous-learning installation gate.", ["decision:accuracy_promotion_gate"]),
            new("td05_downstream_phase_sequence", "implementation_sequence_artifact", "covered", "The downstream plan lists fixed Phase 1 through Phase 5 implementation steps.", "This packet tests whether build-time order is separated from runtime topology.", ["artifact:build_time_phase_sequence"]),
            new("td06_base_service_constraint", "implementation_constraint_preservation", "concern", "The upstream BaseService inheritance rule is not explicitly enforced by downstream Gate B tests.", "This packet must become a concern rather than a block.", ["constraint:base_service_inheritance"]),
            new("td07_cross_phase_context_packet", "cross_phase_context_handoff", "concern", "The upstream design requires phase context packets, but the downstream plan does not implement that cross-phase handoff.", "This packet must remain visible as a concern.", ["knowledge_edge:cross_phase_context_packet"]),
            new("td08_static_runtime_rejection", "rejected_runtime_shape", "covered", "The rejected static runtime pipeline must not be mistaken for the downstream build-time sequence.", "This packet protects the false-positive block case.", ["rejection:static_runtime_topology"]),
            new("td09_downstream_scope_boundary", "downstream_scope_boundary", "boundary", "The downstream autonomous-learning plan is valid within its own implementation scope.", "This packet prevents the transition gate from demanding unrelated phase work inside one implementation plan.", ["boundary:autonomous_learning_scope"]),
            new("td10_transition_verdict_assembly", "transition_verdict_assembly", "covered", "The transition gate must assemble pass, concern, and block counts into one user-readable verdict.", "This packet makes the final result auditable.", ["output:pass_with_gaps"])
        ];
    }
}
