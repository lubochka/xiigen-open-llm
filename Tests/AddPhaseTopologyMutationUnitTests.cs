// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace LlmFromScratch.Tests;

public sealed class AddPhaseTopologyMutationUnitTests
{
    [Fact]
    public async Task AddPhaseTopologyMutationUnit_ValidCommand_StoresPhase()
    {
        // Create the topology registry for this unit test.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Create the add-phase unit.
        var unit = new AddPhaseTopologyMutationUnit(registry);

        // Execute a valid add-phase command.
        var result = await unit.ExecuteAsync(BuildRequest(BuildCommand()));

        // The unit execution must succeed through the shared envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The registry entry must contain the requested phase id.
        Assert.Equal("implementation", result.Value!.Output.Phase.PhaseId);

        // The first registry mutation must produce revision one.
        Assert.Equal(1, result.Value.Output.Revision);
    }

    [Fact]
    public async Task AddPhaseTopologyMutationUnit_OutputCarriesPredicateDomainAndEvidenceIds()
    {
        // Create the add-phase unit with a real registry.
        var unit = new AddPhaseTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

        // Execute a valid add-phase command.
        var result = await unit.ExecuteAsync(BuildRequest(BuildCommand()));

        // The unit execution must succeed before output metadata is checked.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Predicate ids must come from the stored phase.
        Assert.Equal(["CREATE", "ASSIGN"], result.Value!.PredicateIds);

        // Domain tags must come from the stored phase domain ownership.
        Assert.Equal(["technical_design", "implementation"], result.Value.DomainTags);

        // Evidence ids must come from the phase source ids.
        Assert.Equal(["IMPLEMENTATION-PLAN"], result.Value.EvidenceIds);
    }

    [Fact]
    public async Task AddPhaseTopologyMutationUnit_InvalidPhase_FailsCleanly()
    {
        // Create the add-phase unit with a real registry.
        var unit = new AddPhaseTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

        // Build a command whose phase has no id.
        var command = new AddPhaseCommand(new LogicalPhase("", "Broken", "Missing id.", ["domain"], [], []), "Try invalid phase.");

        // Execute the invalid command.
        var result = await unit.ExecuteAsync(BuildRequest(command));

        // Invalid phases must fail through the logical-unit envelope.
        Assert.False(result.IsSuccess);

        // The registry error code must be preserved by the unit.
        Assert.Equal("EMPTY_PHASE_ID", result.ErrorCode);
    }

    [Fact]
    public async Task AddPhaseTopologyMutationUnit_EmptyReason_FailsCleanly()
    {
        // Create the add-phase unit with a real registry.
        var unit = new AddPhaseTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

        // Build a command without an audit reason.
        var command = new AddPhaseCommand(BuildPhase(), " ");

        // Execute the command without a reason.
        var result = await unit.ExecuteAsync(BuildRequest(command));

        // Empty mutation reasons must fail.
        Assert.False(result.IsSuccess);

        // The error code must identify the missing reason.
        Assert.Equal("EMPTY_REASON", result.ErrorCode);
    }

    [Fact]
    public async Task AddPhaseTopologyMutationUnit_MissingRegistry_FailsCleanly()
    {
        // Create an invalid unit without a registry.
        var unit = new AddPhaseTopologyMutationUnit(null);

        // Execute a valid command against the invalid unit.
        var result = await unit.ExecuteAsync(BuildRequest(BuildCommand()));

        // Missing registry must fail through the unit envelope.
        Assert.False(result.IsSuccess);

        // The error code must identify the missing registry dependency.
        Assert.Equal("TOPOLOGY_REGISTRY_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public async Task AddPhaseTopologyMutationUnit_CancelledRequest_FailsCleanly()
    {
        // Create the add-phase unit with a real registry.
        var unit = new AddPhaseTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

        // Create a token source for cancellation.
        using var source = new CancellationTokenSource();

        // Cancel before unit execution.
        source.Cancel();

        // Execute with a cancelled token.
        var result = await unit.ExecuteAsync(BuildRequest(BuildCommand()), source.Token);

        // Cancellation must fail through the unit envelope.
        Assert.False(result.IsSuccess);

        // The error code must identify cancellation.
        Assert.Equal("CANCELLED", result.ErrorCode);
    }

    private static LogicalUnitRequest<AddPhaseCommand> BuildRequest(AddPhaseCommand command)
    {
        // Build the standard request shape for the add-phase unit.
        return new LogicalUnitRequest<AddPhaseCommand>(
            "run-add-phase-unit-test",
            AddPhaseTopologyMutationUnit.Id,
            AddPhaseTopologyMutationUnit.Role,
            AddPhaseTopologyMutationUnit.DomainTag,
            command,
            new LogicalExecutionFlags(),
            new Dictionary<string, object> { ["source"] = "phase40" });
    }

    private static AddPhaseCommand BuildCommand()
    {
        // Build a valid add-phase command.
        return new AddPhaseCommand(BuildPhase(), "Register implementation as a logical topology phase.");
    }

    private static LogicalPhase BuildPhase()
    {
        // Build a valid implementation phase.
        return new LogicalPhase(
            "implementation",
            "Implementation",
            "Owns implementation orchestration.",
            ["technical_design", "implementation"],
            ["create", "assign"],
            ["implementation-plan"]);
    }
}
