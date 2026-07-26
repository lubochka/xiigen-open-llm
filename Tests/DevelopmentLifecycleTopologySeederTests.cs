// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace LlmFromScratch.Tests;

using AgentInfrastructure;
using AgentInfrastructure.Logical;
using ExperimentsV2.LogicalFlow;
using OrchestratorModel.Logical;
using Domain = SemanticCompiler29.Domain;

public sealed class DevelopmentLifecycleTopologySeederTests
{
    [Fact]
    public void DevelopmentLifecycleTopologySeeder_RegistersSevenPhasesAndThirtyOneSubPhases()
    {
        // Build empty runtime registries.
        var topology = new InMemoryLogicalTopologyRegistry();
        var flows = new InMemoryLogicalFlowRegistry();

        // Seed the full development lifecycle.
        var result = DevelopmentLifecycleTopologySeeder.Seed(topology, flows);

        // The seed operation must succeed through the shared envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The canonical development lifecycle has seven phases and 31 sub-phases.
        Assert.Equal(7, result.Value!.PhaseCount);
        Assert.Equal(31, result.Value.SubPhaseCount);

        // The topology registry must contain the same counts.
        Assert.Equal(7, topology.ListPhases().Value!.Count);
        Assert.Equal(31, topology.ListSubPhases().Value!.Count);

        // Phase and sub-phase transitions must both be registered.
        Assert.Equal(36, topology.ListTransitions().Value!.Count);
    }

    [Fact]
    public void DevelopmentLifecycleTopologySeeder_RegistersFlowForEverySubPhaseWithoutOverwritingSpecificSecurityFlow()
    {
        // Build runtime registries.
        var topology = new InMemoryLogicalTopologyRegistry();
        var flows = new InMemoryLogicalFlowRegistry();

        // Register the specific security flow before the generic lifecycle seed.
        var securityRegistration = flows.RegisterFactory(
            SecurityVerification34FlowAdapter.FlowId,
            SecurityVerification34FlowAdapter.ScenarioId,
            "1.0.0",
            "3",
            "3.4",
            "security-verification",
            ["security-and-authorization-model"],
            new SecurityVerification34FlowFactory());
        Assert.True(securityRegistration.IsSuccess, securityRegistration.ErrorMessage);

        // Seed the lifecycle.
        var result = DevelopmentLifecycleTopologySeeder.Seed(topology, flows);
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The seeder should add generic flows for the other 30 sub-phases only.
        Assert.Equal(30, result.Value!.RegisteredFlowCount);

        // The registry must expose one string-input flow slot per sub-phase.
        var descriptors = flows.ListDescriptors().Where(descriptor => !string.IsNullOrWhiteSpace(descriptor.SubPhaseId)).ToArray();
        Assert.Equal(31, descriptors.Select(descriptor => descriptor.SubPhaseId).Distinct(StringComparer.Ordinal).Count());

        // Sub-phase 3.4 must still point to the specific security-verification flow.
        var securityDescriptor = Assert.Single(descriptors, descriptor => descriptor.SubPhaseId == "3.4");
        Assert.Equal(SecurityVerification34FlowAdapter.FlowId, securityDescriptor.FlowId);
    }

    [Fact]
    public async Task UniversalNineNodeDevelopmentFlow_ExecutesNineLogicalStations()
    {
        // Build one generic lifecycle flow for the design-system sub-phase.
        var flow = new UniversalNineNodeDevelopmentFlow("1", "1.1", "design-system", ["software-development"], ["UI"]);

        // Execute the flow.
        var result = await flow.ExecuteAsync(new LogicalFlowRequest<string>(
            "universal-nine-node-test",
            $"{UniversalNineNodeDevelopmentFlow.FlowIdPrefix}-1.1",
            $"{UniversalNineNodeDevelopmentFlow.ScenarioIdPrefix}-1.1",
            "Create design system tokens for primary buttons.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // The flow must succeed through the shared envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // All nine canonical nodes must execute.
        Assert.Equal("9", result.Value!.Metrics["node_count"]);
        Assert.Equal("True", result.Value.Metrics["universal_nine_node_flow"]);
        Assert.Equal("node-01-domain-splitter", result.Value.Metrics["node_01_unit_id"]);
        Assert.Equal("node-09-formatter", result.Value.Metrics["node_09_unit_id"]);

        // Universal flows now execute concrete semantic/planning/quality station behavior.
        Assert.Equal("PASS", result.Value.OverallVerdict);
        Assert.Empty(result.Value.TransitionSignals);
        Assert.DoesNotContain(result.Value.Warnings, warning => warning.StartsWith("generic_node_requires_domain_learning:", StringComparison.Ordinal));
    }

    [Fact]
    public async Task UniversalNineNodeDevelopmentFlow_CollectsNodeTransitionSignals()
    {
        // Build one flow with a single injected node that emits an advisory transition signal.
        var definitions = UniversalNineNodeDevelopmentFlow.NodeDefinitions;
        var signalingNodeIndex = 3;
        var signal = new PhaseTransitionSignal(
            Domain.CODE_Q,
            "QA review required for the completed implementation branch.",
            definitions[signalingNodeIndex].NodeId,
            "4.1",
            ["transition-evidence-qa"]);
        var units = UniversalNineNodeDevelopmentFlow.SharedNodeUnits;
        units[signalingNodeIndex] = new SignalingUniversalNodeUnit(units[signalingNodeIndex], signal);
        var flow = new UniversalNineNodeDevelopmentFlow(
            "4",
            "4.1",
            "backend-services",
            ["software-development"],
            ["IMPLEMENT"],
            units);

        // Execute the flow.
        var result = await flow.ExecuteAsync(new LogicalFlowRequest<string>(
            "universal-transition-signal-test",
            $"{UniversalNineNodeDevelopmentFlow.FlowIdPrefix}-4.1",
            $"{UniversalNineNodeDevelopmentFlow.ScenarioIdPrefix}-4.1",
            "Implement the backend service and signal QA review.",
            new LogicalExecutionFlags(),
            new Dictionary<string, object>()));

        // The flow must surface the node signal without interrupting execution.
        Assert.True(result.IsSuccess, result.ErrorMessage);
        var emitted = Assert.Single(result.Value!.TransitionSignals);
        Assert.Equal(definitions[signalingNodeIndex].NodeId, emitted.SourceNodeId);
        Assert.Equal(Domain.CODE_Q, emitted.SuggestedDomain);
    }

    [Fact]
    public async Task LogicalOrchestratorAssistant_DefaultCanRouteDesignPromptToUniversalNineNodeFlow()
    {
        // Build the default assistant with the full lifecycle seed.
        var assistant = LogicalOrchestratorAssistant.CreateDefault(new LifecycleAssistantNullSessionStorage());
        Assert.True(assistant.IsSuccess, assistant.ErrorMessage);

        // Run a non-security prompt through adaptive routing.
        var result = await assistant.Value!.RunAsync("Create a design system token guide for UI buttons and typography.");

        // The assistant run must succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The selected flow must be the generic nine-node development flow for sub-phase 1.1.
        Assert.Equal($"{UniversalNineNodeDevelopmentFlow.FlowIdPrefix}-1.1", result.Value!.SelectedFlowId);
        Assert.Equal("True", result.Value.Metrics["universal_nine_node_flow"]);
        Assert.Equal("1.1", result.Value.Metrics["sub_phase_id"]);
        Assert.Equal("node-09-formatter", result.Value.Metrics["node_09_unit_id"]);
    }
}

file sealed class LifecycleAssistantNullSessionStorage : ISessionStorage
{
    public Task<OperationResult<bool>> SaveAsync(string sessionId, AgentState state)
    {
        // The assistant tests do not persist sessions.
        return Task.FromResult(OperationResult<bool>.Ok(true));
    }

    public Task<OperationResult<AgentState>> LoadAsync(string sessionId)
    {
        // The assistant tests never load sessions from storage.
        return Task.FromResult(OperationResult<AgentState>.Fail("Not persisted.", "NULL_STORAGE"));
    }

    public Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
    {
        // The assistant tests expose an empty storage surface.
        return Task.FromResult(OperationResult<IReadOnlyList<string>>.Ok((IReadOnlyList<string>)Array.Empty<string>()));
    }
}

file sealed class SignalingUniversalNodeUnit : ILogicalUnit<UniversalNodeState, UniversalNodeState>
{
    private readonly ILogicalUnit<UniversalNodeState, UniversalNodeState> _inner;
    private readonly PhaseTransitionSignal _signal;

    public SignalingUniversalNodeUnit(
        ILogicalUnit<UniversalNodeState, UniversalNodeState> inner,
        PhaseTransitionSignal signal)
    {
        _inner = inner;
        _signal = signal;
    }

    public async Task<OperationResult<LogicalUnitResult<UniversalNodeState>>> ExecuteAsync(
        LogicalUnitRequest<UniversalNodeState> request,
        CancellationToken cancellationToken = default)
    {
        var inner = await _inner.ExecuteAsync(request, cancellationToken);
        if (!inner.IsSuccess || inner.Value is null)
            return inner;

        var admitted = inner.Value;
        var output = admitted.Output with
        {
            EvidenceIds = admitted.Output.EvidenceIds
                .Concat(_signal.EvidenceIds)
                .Distinct(StringComparer.Ordinal)
                .ToArray()
        };

        return OperationResult<LogicalUnitResult<UniversalNodeState>>.Ok(new LogicalUnitResult<UniversalNodeState>(
            admitted.UnitId,
            admitted.UnitRole,
            admitted.DomainTag,
            output,
            admitted.Confidence,
            admitted.PredicateIds.ToArray(),
            admitted.DomainTags.ToArray(),
            output.EvidenceIds,
            admitted.Warnings.ToArray())
        {
            TransitionSignal = _signal
        });
    }
}
