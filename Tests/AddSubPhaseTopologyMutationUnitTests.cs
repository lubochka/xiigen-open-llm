// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace LlmFromScratch.Tests;

public sealed class AddSubPhaseTopologyMutationUnitTests
{
    [Fact]
    public async Task AddSubPhaseTopologyMutationUnit_ValidCommand_StoresSubPhase()
    {
        // Create the topology registry for this unit test.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Create the add-sub-phase unit.
        var unit = new AddSubPhaseTopologyMutationUnit(registry);

        // Execute a valid add-sub-phase command.
        var result = await unit.ExecuteAsync(BuildRequest(BuildCommand()));

        // The unit execution must succeed through the shared envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The registry entry must contain the requested sub-phase id.
        Assert.Equal("implementation_planning", result.Value!.Output.SubPhase.SubPhaseId);

        // The first registry mutation must produce revision one.
        Assert.Equal(1, result.Value.Output.Revision);
    }

    [Fact]
    public async Task AddSubPhaseTopologyMutationUnit_OutputCarriesPredicateDomainAndEvidenceIds()
    {
        // Create the add-sub-phase unit with a real registry.
        var unit = new AddSubPhaseTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

        // Execute a valid add-sub-phase command.
        var result = await unit.ExecuteAsync(BuildRequest(BuildCommand()));

        // The unit execution must succeed before output metadata is checked.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Predicate ids must come from the stored sub-phase.
        Assert.Equal(["CREATE", "ITERATE"], result.Value!.PredicateIds);

        // Domain tags must come from the stored sub-phase domain ownership.
        Assert.Equal(["implementation"], result.Value.DomainTags);

        // Evidence ids must come from the sub-phase source ids.
        Assert.Equal(["IMPLEMENTATION-PLAN"], result.Value.EvidenceIds);
    }

    [Fact]
    public async Task AddSubPhaseTopologyMutationUnit_InvalidSubPhase_FailsCleanly()
    {
        // Create the add-sub-phase unit with a real registry.
        var unit = new AddSubPhaseTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

        // Build a command whose sub-phase has no id.
        var command = new AddSubPhaseCommand(new LogicalSubPhase("", "phase", "Broken", "Missing id.", ["domain"], [], []), "Try invalid sub-phase.");

        // Execute the invalid command.
        var result = await unit.ExecuteAsync(BuildRequest(command));

        // Invalid sub-phases must fail through the logical-unit envelope.
        Assert.False(result.IsSuccess);

        // The registry error code must be preserved by the unit.
        Assert.Equal("EMPTY_SUB_PHASE_ID", result.ErrorCode);
    }

    [Fact]
    public async Task AddSubPhaseTopologyMutationUnit_EmptyReason_FailsCleanly()
    {
        // Create the add-sub-phase unit with a real registry.
        var unit = new AddSubPhaseTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

        // Build a command without an audit reason.
        var command = new AddSubPhaseCommand(BuildSubPhase(), " ");

        // Execute the command without a reason.
        var result = await unit.ExecuteAsync(BuildRequest(command));

        // Empty mutation reasons must fail.
        Assert.False(result.IsSuccess);

        // The error code must identify the missing reason.
        Assert.Equal("EMPTY_REASON", result.ErrorCode);
    }

    [Fact]
    public async Task AddSubPhaseTopologyMutationUnit_MissingRegistry_FailsCleanly()
    {
        // Create an invalid unit without a registry.
        var unit = new AddSubPhaseTopologyMutationUnit(null);

        // Execute a valid command against the invalid unit.
        var result = await unit.ExecuteAsync(BuildRequest(BuildCommand()));

        // Missing registry must fail through the unit envelope.
        Assert.False(result.IsSuccess);

        // The error code must identify the missing registry dependency.
        Assert.Equal("TOPOLOGY_REGISTRY_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public async Task AddSubPhaseTopologyMutationUnit_CancelledRequest_FailsCleanly()
    {
        // Create the add-sub-phase unit with a real registry.
        var unit = new AddSubPhaseTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

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

    private static LogicalUnitRequest<AddSubPhaseCommand> BuildRequest(AddSubPhaseCommand command)
    {
        // Build the standard request shape for the add-sub-phase unit.
        return new LogicalUnitRequest<AddSubPhaseCommand>(
            "run-add-sub-phase-unit-test",
            AddSubPhaseTopologyMutationUnit.Id,
            AddSubPhaseTopologyMutationUnit.Role,
            AddSubPhaseTopologyMutationUnit.DomainTag,
            command,
            new LogicalExecutionFlags(),
            new Dictionary<string, object> { ["source"] = "phase45" });
    }

    private static AddSubPhaseCommand BuildCommand()
    {
        // Build a valid add-sub-phase command.
        return new AddSubPhaseCommand(BuildSubPhase(), "Register implementation planning as a logical topology sub-phase.");
    }

    private static LogicalSubPhase BuildSubPhase()
    {
        // Build a valid implementation planning sub-phase.
        return new LogicalSubPhase(
            "implementation_planning",
            "implementation",
            "Implementation Planning",
            "Owns implementation planning orchestration.",
            ["implementation"],
            ["create", "iterate"],
            ["implementation-plan"]);
    }
}
