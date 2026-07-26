// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace LlmFromScratch.Tests;

public sealed class TopologyMutationFlowTests
{
    [Fact]
    public async Task TopologyMutationFlow_AddDomainCommand_StoresDomain()
    {
        // Create the registry that the add-domain unit mutates.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Create the add-domain unit for the topology flow.
        var unit = new AddDomainTopologyMutationUnit(registry);

        // Create the topology mutation flow with the add-domain unit.
        var flow = new TopologyMutationFlow(unit);

        // Execute a valid add-domain request.
        var result = await flow.ExecuteAsync(BuildRequest(BuildAddDomainCommand()));

        // The flow execution must succeed through the shared envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The flow verdict must name the applied domain mutation.
        Assert.Equal("DOMAIN_MUTATION_APPLIED", result.Value!.OverallVerdict);

        // The stored domain id must appear in the flow metrics.
        Assert.Equal("technical_design", result.Value.Metrics["stored_domain_id"]);

        // The registry must contain the domain after flow execution.
        var stored = registry.ResolveDomain("technical_design");

        // The registry lookup must succeed.
        Assert.True(stored.IsSuccess, stored.ErrorMessage);
    }

    [Fact]
    public async Task TopologyMutationFlow_AddDomainCommand_ReportsStableMetrics()
    {
        // Create the topology mutation flow with a real add-domain unit.
        var flow = new TopologyMutationFlow(new AddDomainTopologyMutationUnit(new InMemoryLogicalTopologyRegistry()));

        // Execute a valid add-domain request.
        var result = await flow.ExecuteAsync(BuildRequest(BuildAddDomainCommand()));

        // The flow execution must succeed before metrics are inspected.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The success metric must be machine-readable.
        Assert.Equal("True", result.Value!.Metrics["topology_mutation_success"]);

        // The mutation command metric must name the typed command.
        Assert.Equal(nameof(AddDomainCommand), result.Value.Metrics["mutation_command"]);

        // The first mutation must be revision one.
        Assert.Equal("1", result.Value.Metrics["stored_revision"]);

        // The deterministic unit confidence must be preserved by the flow.
        Assert.Equal("1.00", result.Value.Metrics["unit_confidence"]);

        // The summary must expose the same confidence as the scaling signal.
        Assert.Equal(1.0, result.Value.Summary.ScalingSignal);
    }

    [Fact]
    public async Task TopologyMutationFlow_AddPhaseCommand_StoresPhase()
    {
        // Create the registry that the add-phase unit mutates.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Create the topology mutation flow with both supported units.
        var flow = new TopologyMutationFlow(
            new AddDomainTopologyMutationUnit(registry),
            new AddPhaseTopologyMutationUnit(registry));

        // Execute a valid add-phase request.
        var result = await flow.ExecuteAsync(BuildRequest(BuildAddPhaseCommand()));

        // The flow execution must succeed through the shared envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The flow verdict must name the applied phase mutation.
        Assert.Equal("PHASE_MUTATION_APPLIED", result.Value!.OverallVerdict);

        // The stored phase id must appear in the flow metrics.
        Assert.Equal("implementation", result.Value.Metrics["stored_phase_id"]);

        // The registry must contain the phase after flow execution.
        var stored = registry.ResolvePhase("implementation");

        // The registry lookup must succeed.
        Assert.True(stored.IsSuccess, stored.ErrorMessage);
    }

    [Fact]
    public async Task TopologyMutationFlow_AddPhaseCommand_ReportsStableMetrics()
    {
        // Create one registry for both topology mutation units.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Create the topology mutation flow with both supported units.
        var flow = new TopologyMutationFlow(
            new AddDomainTopologyMutationUnit(registry),
            new AddPhaseTopologyMutationUnit(registry));

        // Execute a valid add-phase request.
        var result = await flow.ExecuteAsync(BuildRequest(BuildAddPhaseCommand()));

        // The flow execution must succeed before metrics are inspected.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The success metric must be machine-readable.
        Assert.Equal("True", result.Value!.Metrics["topology_mutation_success"]);

        // The mutation command metric must name the typed command.
        Assert.Equal(nameof(AddPhaseCommand), result.Value.Metrics["mutation_command"]);

        // The first mutation must be revision one.
        Assert.Equal("1", result.Value.Metrics["stored_revision"]);

        // The deterministic unit confidence must be preserved by the flow.
        Assert.Equal("1.00", result.Value.Metrics["unit_confidence"]);
    }

    [Fact]
    public async Task TopologyMutationFlow_AddPhaseCommand_MissingUnitFailsCleanly()
    {
        // Create the topology mutation flow without the add-phase unit.
        var flow = new TopologyMutationFlow(new AddDomainTopologyMutationUnit(new InMemoryLogicalTopologyRegistry()));

        // Execute a valid add-phase request.
        var result = await flow.ExecuteAsync(BuildRequest(BuildAddPhaseCommand()));

        // Missing phase unit must fail through the flow envelope.
        Assert.False(result.IsSuccess);

        // The error code must identify the missing phase unit.
        Assert.Equal("ADD_PHASE_UNIT_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public async Task TopologyMutationFlow_AddSubPhaseCommand_StoresSubPhase()
    {
        // Create the registry that the add-sub-phase unit mutates.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Create the topology mutation flow with all supported units.
        var flow = new TopologyMutationFlow(
            new AddDomainTopologyMutationUnit(registry),
            new AddPhaseTopologyMutationUnit(registry),
            new AddSubPhaseTopologyMutationUnit(registry));

        // Execute a valid add-sub-phase request.
        var result = await flow.ExecuteAsync(BuildRequest(BuildAddSubPhaseCommand()));

        // The flow execution must succeed through the shared envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The flow verdict must name the applied sub-phase mutation.
        Assert.Equal("SUB_PHASE_MUTATION_APPLIED", result.Value!.OverallVerdict);

        // The stored sub-phase id must appear in the flow metrics.
        Assert.Equal("implementation_planning", result.Value.Metrics["stored_sub_phase_id"]);

        // The registry must contain the sub-phase after flow execution.
        var stored = registry.ResolveSubPhase("implementation_planning");

        // The registry lookup must succeed.
        Assert.True(stored.IsSuccess, stored.ErrorMessage);
    }

    [Fact]
    public async Task TopologyMutationFlow_AddSubPhaseCommand_ReportsStableMetrics()
    {
        // Create one registry for all topology mutation units.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Create the topology mutation flow with all supported units.
        var flow = new TopologyMutationFlow(
            new AddDomainTopologyMutationUnit(registry),
            new AddPhaseTopologyMutationUnit(registry),
            new AddSubPhaseTopologyMutationUnit(registry));

        // Execute a valid add-sub-phase request.
        var result = await flow.ExecuteAsync(BuildRequest(BuildAddSubPhaseCommand()));

        // The flow execution must succeed before metrics are inspected.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The success metric must be machine-readable.
        Assert.Equal("True", result.Value!.Metrics["topology_mutation_success"]);

        // The mutation command metric must name the typed command.
        Assert.Equal(nameof(AddSubPhaseCommand), result.Value.Metrics["mutation_command"]);

        // The parent phase id must be visible in metrics.
        Assert.Equal("implementation", result.Value.Metrics["stored_parent_phase_id"]);

        // The deterministic unit confidence must be preserved by the flow.
        Assert.Equal("1.00", result.Value.Metrics["unit_confidence"]);
    }

    [Fact]
    public async Task TopologyMutationFlow_AddSubPhaseCommand_MissingUnitFailsCleanly()
    {
        // Create the topology mutation flow without the add-sub-phase unit.
        var flow = new TopologyMutationFlow(
            new AddDomainTopologyMutationUnit(new InMemoryLogicalTopologyRegistry()),
            new AddPhaseTopologyMutationUnit(new InMemoryLogicalTopologyRegistry()));

        // Execute a valid add-sub-phase request.
        var result = await flow.ExecuteAsync(BuildRequest(BuildAddSubPhaseCommand()));

        // Missing sub-phase unit must fail through the flow envelope.
        Assert.False(result.IsSuccess);

        // The error code must identify the missing sub-phase unit.
        Assert.Equal("ADD_SUB_PHASE_UNIT_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public async Task TopologyMutationFlow_AddTransitionCommand_StoresTransition()
    {
        // Create the registry that the add-transition unit mutates.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Create the topology mutation flow with all supported units.
        var flow = BuildFullFlow(registry);

        // Execute a valid add-transition request.
        var result = await flow.ExecuteAsync(BuildRequest(BuildAddTransitionCommand()));

        // The flow execution must succeed through the shared envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The flow verdict must name the applied transition mutation.
        Assert.Equal("TRANSITION_MUTATION_APPLIED", result.Value!.OverallVerdict);

        // The stored transition id must appear in the flow metrics.
        Assert.Equal("technical-to-implementation", result.Value.Metrics["stored_transition_id"]);

        // The registry must contain the transition after flow execution.
        var stored = registry.ResolveTransition("technical-to-implementation");

        // The registry lookup must succeed.
        Assert.True(stored.IsSuccess, stored.ErrorMessage);
    }

    [Fact]
    public async Task TopologyMutationFlow_AddTransitionCommand_ReportsStableMetrics()
    {
        // Create one registry for all topology mutation units.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Create the topology mutation flow with all supported units.
        var flow = BuildFullFlow(registry);

        // Execute a valid add-transition request.
        var result = await flow.ExecuteAsync(BuildRequest(BuildAddTransitionCommand()));

        // The flow execution must succeed before metrics are inspected.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The success metric must be machine-readable.
        Assert.Equal("True", result.Value!.Metrics["topology_mutation_success"]);

        // The mutation command metric must name the typed command.
        Assert.Equal(nameof(AddTransitionCommand), result.Value.Metrics["mutation_command"]);

        // The transition source id must be visible in metrics.
        Assert.Equal("technical_design", result.Value.Metrics["stored_from_topology_id"]);

        // The transition target id must be visible in metrics.
        Assert.Equal("implementation", result.Value.Metrics["stored_to_topology_id"]);
    }

    [Fact]
    public async Task TopologyMutationFlow_AddTransitionCommand_MissingUnitFailsCleanly()
    {
        // Create the topology mutation flow without the add-transition unit.
        var flow = new TopologyMutationFlow(
            new AddDomainTopologyMutationUnit(new InMemoryLogicalTopologyRegistry()),
            new AddPhaseTopologyMutationUnit(new InMemoryLogicalTopologyRegistry()),
            new AddSubPhaseTopologyMutationUnit(new InMemoryLogicalTopologyRegistry()));

        // Execute a valid add-transition request.
        var result = await flow.ExecuteAsync(BuildRequest(BuildAddTransitionCommand()));

        // Missing transition unit must fail through the flow envelope.
        Assert.False(result.IsSuccess);

        // The error code must identify the missing transition unit.
        Assert.Equal("ADD_TRANSITION_UNIT_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public async Task TopologyMutationFlow_UnsupportedCommand_FailsCleanly()
    {
        // Create the topology mutation flow with a real add-domain unit.
        var flow = new TopologyMutationFlow(new AddDomainTopologyMutationUnit(new InMemoryLogicalTopologyRegistry()));

        // Build a request with a command that has no unit in this phase.
        var request = BuildRequest(new UnsupportedTopologyCommand("Try unsupported mutation."));

        // Execute the unsupported command.
        var result = await flow.ExecuteAsync(request);

        // Unsupported commands must fail through the flow envelope.
        Assert.False(result.IsSuccess);

        // The error code must identify unsupported topology command routing.
        Assert.Equal("UNSUPPORTED_TOPOLOGY_COMMAND", result.ErrorCode);
    }

    [Fact]
    public async Task TopologyMutationFlow_UnsupportedFlowId_FailsCleanly()
    {
        // Create the topology mutation flow with a real add-domain unit.
        var flow = new TopologyMutationFlow(new AddDomainTopologyMutationUnit(new InMemoryLogicalTopologyRegistry()));

        // Build a request with the wrong flow id.
        var request = BuildRequest(BuildAddDomainCommand()) with { FlowId = "other-flow" };

        // Execute the wrongly-bound request.
        var result = await flow.ExecuteAsync(request);

        // Wrong flow ids must fail through the flow envelope.
        Assert.False(result.IsSuccess);

        // The error code must identify flow id mismatch.
        Assert.Equal("UNSUPPORTED_FLOW_ID", result.ErrorCode);
    }

    [Fact]
    public async Task TopologyMutationFlow_UnsupportedScenarioId_FailsCleanly()
    {
        // Create the topology mutation flow with a real add-domain unit.
        var flow = new TopologyMutationFlow(new AddDomainTopologyMutationUnit(new InMemoryLogicalTopologyRegistry()));

        // Build a request with the wrong scenario id.
        var request = BuildRequest(BuildAddDomainCommand()) with { ScenarioId = "other-scenario" };

        // Execute the wrongly-scoped request.
        var result = await flow.ExecuteAsync(request);

        // Wrong scenario ids must fail through the flow envelope.
        Assert.False(result.IsSuccess);

        // The error code must identify scenario id mismatch.
        Assert.Equal("UNSUPPORTED_SCENARIO_ID", result.ErrorCode);
    }

    [Fact]
    public async Task TopologyMutationFlow_MissingAddDomainUnit_FailsCleanly()
    {
        // Create the topology mutation flow without its required add-domain unit.
        var flow = new TopologyMutationFlow(null);

        // Execute a valid add-domain request against the invalid flow.
        var result = await flow.ExecuteAsync(BuildRequest(BuildAddDomainCommand()));

        // Missing units must fail through the flow envelope.
        Assert.False(result.IsSuccess);

        // The error code must identify the missing unit.
        Assert.Equal("ADD_DOMAIN_UNIT_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public async Task TopologyMutationFlow_CancelledRequest_FailsCleanly()
    {
        // Create the topology mutation flow with a real add-domain unit.
        var flow = new TopologyMutationFlow(new AddDomainTopologyMutationUnit(new InMemoryLogicalTopologyRegistry()));

        // Create a token source for cancellation.
        using var source = new CancellationTokenSource();

        // Cancel before flow execution starts.
        source.Cancel();

        // Execute the flow with a cancelled token.
        var result = await flow.ExecuteAsync(BuildRequest(BuildAddDomainCommand()), source.Token);

        // Cancellation must fail through the flow envelope.
        Assert.False(result.IsSuccess);

        // The error code must identify cancellation.
        Assert.Equal("CANCELLED", result.ErrorCode);
    }

    private static LogicalFlowRequest<LogicalTopologyCommand> BuildRequest(LogicalTopologyCommand command)
    {
        // Build the standard request shape for topology mutation flow tests.
        return new LogicalFlowRequest<LogicalTopologyCommand>(
            "run-topology-flow-test",
            TopologyMutationFlow.FlowId,
            TopologyMutationFlow.ScenarioId,
            command,
            new LogicalExecutionFlags(),
            new Dictionary<string, object> { ["source"] = "phase36" });
    }

    private static TopologyMutationFlow BuildFullFlow(InMemoryLogicalTopologyRegistry registry)
    {
        // Build the topology mutation flow with every mutation unit currently supported.
        return new TopologyMutationFlow(
            new AddDomainTopologyMutationUnit(registry),
            new AddPhaseTopologyMutationUnit(registry),
            new AddSubPhaseTopologyMutationUnit(registry),
            new AddTransitionTopologyMutationUnit(registry));
    }

    private static AddDomainCommand BuildAddDomainCommand()
    {
        // Build a valid add-domain topology mutation command.
        return new AddDomainCommand(BuildDomain(), "Register technical design as an orchestrator domain.");
    }

    private static LogicalDomain BuildDomain()
    {
        // Build a valid technical-design domain for flow tests.
        return new LogicalDomain(
            "technical_design",
            "Technical Design",
            "Owns technical design orchestration.",
            ["declare", "return"],
            ["technical-design-plan"]);
    }

    private static AddPhaseCommand BuildAddPhaseCommand()
    {
        // Build a valid add-phase topology mutation command.
        return new AddPhaseCommand(BuildPhase(), "Register implementation as an orchestrator phase.");
    }

    private static LogicalPhase BuildPhase()
    {
        // Build a valid implementation phase for flow tests.
        return new LogicalPhase(
            "implementation",
            "Implementation",
            "Owns implementation orchestration.",
            ["technical_design", "implementation"],
            ["create", "assign"],
            ["implementation-plan"]);
    }

    private static AddSubPhaseCommand BuildAddSubPhaseCommand()
    {
        // Build a valid add-sub-phase topology mutation command.
        return new AddSubPhaseCommand(BuildSubPhase(), "Register implementation planning as an orchestrator sub-phase.");
    }

    private static LogicalSubPhase BuildSubPhase()
    {
        // Build a valid implementation planning sub-phase for flow tests.
        return new LogicalSubPhase(
            "implementation_planning",
            "implementation",
            "Implementation Planning",
            "Owns implementation planning orchestration.",
            ["implementation"],
            ["create", "iterate"],
            ["implementation-plan"]);
    }

    private static AddTransitionCommand BuildAddTransitionCommand()
    {
        // Build a valid add-transition topology mutation command.
        return new AddTransitionCommand(BuildTransition(), "Register technical design to implementation as an orchestrator transition.");
    }

    private static LogicalTransition BuildTransition()
    {
        // Build a valid technical design to implementation transition for flow tests.
        return new LogicalTransition(
            "technical-to-implementation",
            "phase",
            "technical_design",
            "phase",
            "implementation",
            "Carries technical design decisions into implementation.",
            ["declare", "return"],
            ["preserve_constraints"],
            ["multi-phase-plan"]);
    }

    // This command gives the flow an unsupported command type for routing tests.
    private sealed record UnsupportedTopologyCommand(string Reason) : LogicalTopologyCommand(Reason);
}
