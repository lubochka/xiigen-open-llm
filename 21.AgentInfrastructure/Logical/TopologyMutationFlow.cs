// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;
using System.Globalization;

// This flow executes topology mutation commands through typed logical units.
public sealed class TopologyMutationFlow : ILogicalFlow<LogicalTopologyCommand>
{
    // This flow id identifies the runtime topology mutation flow.
    public const string FlowId = "flow-topology-mutation-v1";

    // This scenario id identifies the first topology mutation scenario family.
    public const string ScenarioId = "logical_topology_mutation";

    // This unit handles add-domain commands for the first mutation slice.
    private readonly ILogicalUnit<AddDomainCommand, LogicalDomainEntry>? _addDomainUnit;

    // This unit handles add-phase commands for the phase mutation slice.
    private readonly ILogicalUnit<AddPhaseCommand, LogicalPhaseEntry>? _addPhaseUnit;

    // This unit handles add-sub-phase commands for the sub-phase mutation slice.
    private readonly ILogicalUnit<AddSubPhaseCommand, LogicalSubPhaseEntry>? _addSubPhaseUnit;

    // This unit handles add-transition commands for the transition mutation slice.
    private readonly ILogicalUnit<AddTransitionCommand, LogicalTransitionEntry>? _addTransitionUnit;

    // Create the flow with the topology mutation units supported in this phase.
    public TopologyMutationFlow(
        ILogicalUnit<AddDomainCommand, LogicalDomainEntry>? addDomainUnit,
        ILogicalUnit<AddPhaseCommand, LogicalPhaseEntry>? addPhaseUnit = null,
        ILogicalUnit<AddSubPhaseCommand, LogicalSubPhaseEntry>? addSubPhaseUnit = null,
        ILogicalUnit<AddTransitionCommand, LogicalTransitionEntry>? addTransitionUnit = null)
    {
        // Store the add-domain unit so execution can fail through the flow envelope when it is absent.
        _addDomainUnit = addDomainUnit;

        // Store the add-phase unit so phase command support stays explicitly configured.
        _addPhaseUnit = addPhaseUnit;

        // Store the add-sub-phase unit so sub-phase command support stays explicitly configured.
        _addSubPhaseUnit = addSubPhaseUnit;

        // Store the add-transition unit so transition command support stays explicitly configured.
        _addTransitionUnit = addTransitionUnit;
    }

    // Execute one topology mutation command through the shared logical-flow contract.
    public async Task<OperationResult<LogicalFlowResult>> ExecuteAsync(
        LogicalFlowRequest<LogicalTopologyCommand> request,
        CancellationToken cancellationToken = default)
    {
        // Return cancellation through the shared operation envelope.
        if (cancellationToken.IsCancellationRequested)
            return OperationResult<LogicalFlowResult>.Fail("Cancelled.", "CANCELLED");

        // Reject a missing request instead of dereferencing it.
        if (request is null)
            return OperationResult<LogicalFlowResult>.Fail("Topology mutation request is required.", "FLOW_REQUEST_REQUIRED");

        // Refuse requests for any other flow id so the flow cannot be accidentally misbound.
        if (!string.Equals(request.FlowId, FlowId, StringComparison.Ordinal))
            return OperationResult<LogicalFlowResult>.Fail($"Unsupported flow id '{request.FlowId}'.", "UNSUPPORTED_FLOW_ID");

        // Refuse requests for any other scenario id so this phase stays scoped to topology mutation.
        if (!string.Equals(request.ScenarioId, ScenarioId, StringComparison.Ordinal))
            return OperationResult<LogicalFlowResult>.Fail($"Unsupported scenario id '{request.ScenarioId}'.", "UNSUPPORTED_SCENARIO_ID");

        // Route add-domain commands to the add-domain unit.
        if (request.Input is AddDomainCommand addDomainCommand)
            return await ExecuteAddDomainAsync(request, addDomainCommand, cancellationToken);

        // Route add-phase commands to the add-phase unit.
        if (request.Input is AddPhaseCommand addPhaseCommand)
            return await ExecuteAddPhaseAsync(request, addPhaseCommand, cancellationToken);

        // Route add-sub-phase commands to the add-sub-phase unit.
        if (request.Input is AddSubPhaseCommand addSubPhaseCommand)
            return await ExecuteAddSubPhaseAsync(request, addSubPhaseCommand, cancellationToken);

        // Route add-transition commands to the add-transition unit.
        if (request.Input is AddTransitionCommand addTransitionCommand)
            return await ExecuteAddTransitionAsync(request, addTransitionCommand, cancellationToken);

        // Reject every other topology mutation until its own unit is added in a later phase.
        return OperationResult<LogicalFlowResult>.Fail($"Unsupported topology command '{request.Input?.GetType().Name ?? "null"}'.", "UNSUPPORTED_TOPOLOGY_COMMAND");
    }

    // Execute the add-domain branch and normalize its unit result into a flow result.
    private async Task<OperationResult<LogicalFlowResult>> ExecuteAddDomainAsync(
        LogicalFlowRequest<LogicalTopologyCommand> request,
        AddDomainCommand command,
        CancellationToken cancellationToken)
    {
        // Reject invalid construction before calling the add-domain unit.
        if (_addDomainUnit is null)
            return OperationResult<LogicalFlowResult>.Fail("Add-domain topology unit is required.", "ADD_DOMAIN_UNIT_REQUIRED");

        // Build the typed unit request from the flow request.
        var unitRequest = new LogicalUnitRequest<AddDomainCommand>(
            request.RunId,
            AddDomainTopologyMutationUnit.Id,
            AddDomainTopologyMutationUnit.Role,
            AddDomainTopologyMutationUnit.DomainTag,
            command,
            request.Flags,
            request.Metadata);

        // Execute the add-domain unit through its logical-unit interface.
        var unitResult = await _addDomainUnit.ExecuteAsync(unitRequest, cancellationToken);

        // Preserve unit failures at the flow boundary.
        if (!unitResult.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(unitResult.ErrorMessage, unitResult.ErrorCode);

        // Read the stored domain entry from the unit result.
        var entry = unitResult.Value!.Output;

        // Read the stored domain from the entry for human and metric output.
        var domain = entry.Domain;

        // Build the summary that describes this topology mutation as one successful flow action.
        var summary = new LogicalFlowSummary(
            FlowId,
            1,
            0,
            1,
            "topology_mutation_success",
            false,
            true,
            false,
            "READY_FOR_ASSISTANT_ROUTE",
            unitResult.Value.Confidence,
            "confidence_reported_by_add_domain_mutation_unit",
            $"domain={domain.DomainId}; revision={entry.Revision}",
            0,
            0);

        // Build metrics that downstream tests and evidence readers can inspect without parsing prose.
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Record that the requested mutation succeeded.
            ["topology_mutation_success"] = true.ToString(),

            // Record the command kind that was applied.
            ["mutation_command"] = nameof(AddDomainCommand),

            // Record the stored domain id.
            ["stored_domain_id"] = domain.DomainId,

            // Record the stored revision number.
            ["stored_revision"] = entry.Revision.ToString(CultureInfo.InvariantCulture),

            // Record the unit confidence as a stable invariant-culture value.
            ["unit_confidence"] = unitResult.Value.Confidence.ToString("0.00", CultureInfo.InvariantCulture)
        };

        // Build the human-readable result without implying any unsupported topology operation exists yet.
        var finalHumanResult = $"Added or updated logical domain '{domain.DomainId}' at revision {entry.Revision}.";

        // Build the flow result from the unit output and normalized metrics.
        var flowResult = new LogicalFlowResult(
            FlowId,
            ScenarioId,
            "DOMAIN_MUTATION_APPLIED",
            finalHumanResult,
            summary,
            metrics,
            unitResult.Value.EvidenceIds,
            unitResult.Value.Warnings);

        // Return the successful flow result through the shared envelope.
        return OperationResult<LogicalFlowResult>.Ok(flowResult);
    }

    // Execute the add-phase branch and normalize its unit result into a flow result.
    private async Task<OperationResult<LogicalFlowResult>> ExecuteAddPhaseAsync(
        LogicalFlowRequest<LogicalTopologyCommand> request,
        AddPhaseCommand command,
        CancellationToken cancellationToken)
    {
        // Reject invalid construction before calling the add-phase unit.
        if (_addPhaseUnit is null)
            return OperationResult<LogicalFlowResult>.Fail("Add-phase topology unit is required.", "ADD_PHASE_UNIT_REQUIRED");

        // Build the typed unit request from the flow request.
        var unitRequest = new LogicalUnitRequest<AddPhaseCommand>(
            request.RunId,
            AddPhaseTopologyMutationUnit.Id,
            AddPhaseTopologyMutationUnit.Role,
            AddPhaseTopologyMutationUnit.DomainTag,
            command,
            request.Flags,
            request.Metadata);

        // Execute the add-phase unit through its logical-unit interface.
        var unitResult = await _addPhaseUnit.ExecuteAsync(unitRequest, cancellationToken);

        // Preserve unit failures at the flow boundary.
        if (!unitResult.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(unitResult.ErrorMessage, unitResult.ErrorCode);

        // Read the stored phase entry from the unit result.
        var entry = unitResult.Value!.Output;

        // Read the stored phase from the entry for human and metric output.
        var phase = entry.Phase;

        // Build the summary that describes this topology mutation as one successful flow action.
        var summary = new LogicalFlowSummary(
            FlowId,
            1,
            0,
            1,
            "topology_mutation_success",
            false,
            true,
            false,
            "READY_FOR_ASSISTANT_ROUTE",
            unitResult.Value.Confidence,
            "confidence_reported_by_add_phase_mutation_unit",
            $"phase={phase.PhaseId}; revision={entry.Revision}",
            0,
            0);

        // Build metrics that downstream tests and evidence readers can inspect without parsing prose.
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Record that the requested mutation succeeded.
            ["topology_mutation_success"] = true.ToString(),

            // Record the command kind that was applied.
            ["mutation_command"] = nameof(AddPhaseCommand),

            // Record the stored phase id.
            ["stored_phase_id"] = phase.PhaseId,

            // Record the stored revision number.
            ["stored_revision"] = entry.Revision.ToString(CultureInfo.InvariantCulture),

            // Record the unit confidence as a stable invariant-culture value.
            ["unit_confidence"] = unitResult.Value.Confidence.ToString("0.00", CultureInfo.InvariantCulture)
        };

        // Build the human-readable result without implying any unsupported topology operation exists yet.
        var finalHumanResult = $"Added or updated logical phase '{phase.PhaseId}' at revision {entry.Revision}.";

        // Build the flow result from the unit output and normalized metrics.
        var flowResult = new LogicalFlowResult(
            FlowId,
            ScenarioId,
            "PHASE_MUTATION_APPLIED",
            finalHumanResult,
            summary,
            metrics,
            unitResult.Value.EvidenceIds,
            unitResult.Value.Warnings);

        // Return the successful flow result through the shared envelope.
        return OperationResult<LogicalFlowResult>.Ok(flowResult);
    }

    // Execute the add-sub-phase branch and normalize its unit result into a flow result.
    private async Task<OperationResult<LogicalFlowResult>> ExecuteAddSubPhaseAsync(
        LogicalFlowRequest<LogicalTopologyCommand> request,
        AddSubPhaseCommand command,
        CancellationToken cancellationToken)
    {
        // Reject invalid construction before calling the add-sub-phase unit.
        if (_addSubPhaseUnit is null)
            return OperationResult<LogicalFlowResult>.Fail("Add-sub-phase topology unit is required.", "ADD_SUB_PHASE_UNIT_REQUIRED");

        // Build the typed unit request from the flow request.
        var unitRequest = new LogicalUnitRequest<AddSubPhaseCommand>(
            request.RunId,
            AddSubPhaseTopologyMutationUnit.Id,
            AddSubPhaseTopologyMutationUnit.Role,
            AddSubPhaseTopologyMutationUnit.DomainTag,
            command,
            request.Flags,
            request.Metadata);

        // Execute the add-sub-phase unit through its logical-unit interface.
        var unitResult = await _addSubPhaseUnit.ExecuteAsync(unitRequest, cancellationToken);

        // Preserve unit failures at the flow boundary.
        if (!unitResult.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(unitResult.ErrorMessage, unitResult.ErrorCode);

        // Read the stored sub-phase entry from the unit result.
        var entry = unitResult.Value!.Output;

        // Read the stored sub-phase from the entry for human and metric output.
        var subPhase = entry.SubPhase;

        // Build the summary that describes this topology mutation as one successful flow action.
        var summary = new LogicalFlowSummary(
            FlowId,
            1,
            0,
            1,
            "topology_mutation_success",
            false,
            true,
            false,
            "READY_FOR_ASSISTANT_ROUTE",
            unitResult.Value.Confidence,
            "confidence_reported_by_add_sub_phase_mutation_unit",
            $"sub_phase={subPhase.SubPhaseId}; revision={entry.Revision}",
            0,
            0);

        // Build metrics that downstream tests and evidence readers can inspect without parsing prose.
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Record that the requested mutation succeeded.
            ["topology_mutation_success"] = true.ToString(),

            // Record the command kind that was applied.
            ["mutation_command"] = nameof(AddSubPhaseCommand),

            // Record the stored sub-phase id.
            ["stored_sub_phase_id"] = subPhase.SubPhaseId,

            // Record the parent phase id.
            ["stored_parent_phase_id"] = subPhase.PhaseId,

            // Record the stored revision number.
            ["stored_revision"] = entry.Revision.ToString(CultureInfo.InvariantCulture),

            // Record the unit confidence as a stable invariant-culture value.
            ["unit_confidence"] = unitResult.Value.Confidence.ToString("0.00", CultureInfo.InvariantCulture)
        };

        // Build the human-readable result without implying any unsupported topology operation exists yet.
        var finalHumanResult = $"Added or updated logical sub-phase '{subPhase.SubPhaseId}' at revision {entry.Revision}.";

        // Build the flow result from the unit output and normalized metrics.
        var flowResult = new LogicalFlowResult(
            FlowId,
            ScenarioId,
            "SUB_PHASE_MUTATION_APPLIED",
            finalHumanResult,
            summary,
            metrics,
            unitResult.Value.EvidenceIds,
            unitResult.Value.Warnings);

        // Return the successful flow result through the shared envelope.
        return OperationResult<LogicalFlowResult>.Ok(flowResult);
    }

    // Execute the add-transition branch and normalize its unit result into a flow result.
    private async Task<OperationResult<LogicalFlowResult>> ExecuteAddTransitionAsync(
        LogicalFlowRequest<LogicalTopologyCommand> request,
        AddTransitionCommand command,
        CancellationToken cancellationToken)
    {
        // Reject invalid construction before calling the add-transition unit.
        if (_addTransitionUnit is null)
            return OperationResult<LogicalFlowResult>.Fail("Add-transition topology unit is required.", "ADD_TRANSITION_UNIT_REQUIRED");

        // Build the typed unit request from the flow request.
        var unitRequest = new LogicalUnitRequest<AddTransitionCommand>(
            request.RunId,
            AddTransitionTopologyMutationUnit.Id,
            AddTransitionTopologyMutationUnit.Role,
            AddTransitionTopologyMutationUnit.DomainTag,
            command,
            request.Flags,
            request.Metadata);

        // Execute the add-transition unit through its logical-unit interface.
        var unitResult = await _addTransitionUnit.ExecuteAsync(unitRequest, cancellationToken);

        // Preserve unit failures at the flow boundary.
        if (!unitResult.IsSuccess)
            return OperationResult<LogicalFlowResult>.Fail(unitResult.ErrorMessage, unitResult.ErrorCode);

        // Read the stored transition entry from the unit result.
        var entry = unitResult.Value!.Output;

        // Read the stored transition from the entry for human and metric output.
        var transition = entry.Transition;

        // Build the summary that describes this topology mutation as one successful flow action.
        var summary = new LogicalFlowSummary(
            FlowId,
            1,
            0,
            1,
            "topology_mutation_success",
            false,
            true,
            false,
            "READY_FOR_ASSISTANT_ROUTE",
            unitResult.Value.Confidence,
            "confidence_reported_by_add_transition_mutation_unit",
            $"transition={transition.TransitionId}; revision={entry.Revision}",
            0,
            0);

        // Build metrics that downstream tests and evidence readers can inspect without parsing prose.
        var metrics = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            // Record that the requested mutation succeeded.
            ["topology_mutation_success"] = true.ToString(),

            // Record the command kind that was applied.
            ["mutation_command"] = nameof(AddTransitionCommand),

            // Record the stored transition id.
            ["stored_transition_id"] = transition.TransitionId,

            // Record the transition source id.
            ["stored_from_topology_id"] = transition.FromTopologyId,

            // Record the transition target id.
            ["stored_to_topology_id"] = transition.ToTopologyId,

            // Record the stored revision number.
            ["stored_revision"] = entry.Revision.ToString(CultureInfo.InvariantCulture),

            // Record the unit confidence as a stable invariant-culture value.
            ["unit_confidence"] = unitResult.Value.Confidence.ToString("0.00", CultureInfo.InvariantCulture)
        };

        // Build the human-readable result without implying any unsupported topology operation exists yet.
        var finalHumanResult = $"Added or updated logical transition '{transition.TransitionId}' at revision {entry.Revision}.";

        // Build the flow result from the unit output and normalized metrics.
        var flowResult = new LogicalFlowResult(
            FlowId,
            ScenarioId,
            "TRANSITION_MUTATION_APPLIED",
            finalHumanResult,
            summary,
            metrics,
            unitResult.Value.EvidenceIds,
            unitResult.Value.Warnings);

        // Return the successful flow result through the shared envelope.
        return OperationResult<LogicalFlowResult>.Ok(flowResult);
    }
}
