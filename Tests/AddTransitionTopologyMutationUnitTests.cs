// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace LlmFromScratch.Tests;

public sealed class AddTransitionTopologyMutationUnitTests
{
    [Fact]
    public async Task AddTransitionTopologyMutationUnit_ValidCommand_StoresTransition()
    {
        // Create the topology registry for this unit test.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Create the add-transition unit.
        var unit = new AddTransitionTopologyMutationUnit(registry);

        // Execute a valid add-transition command.
        var result = await unit.ExecuteAsync(BuildRequest(BuildCommand()));

        // The unit execution must succeed through the shared envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The registry entry must contain the requested transition id.
        Assert.Equal("technical-to-implementation", result.Value!.Output.Transition.TransitionId);

        // The first registry mutation must produce revision one.
        Assert.Equal(1, result.Value.Output.Revision);
    }

    [Fact]
    public async Task AddTransitionTopologyMutationUnit_OutputCarriesPredicateDomainAndEvidenceIds()
    {
        // Create the add-transition unit with a real registry.
        var unit = new AddTransitionTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

        // Execute a valid add-transition command.
        var result = await unit.ExecuteAsync(BuildRequest(BuildCommand()));

        // The unit execution must succeed before output metadata is checked.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Predicate ids must come from the stored transition.
        Assert.Equal(["DECLARE", "RETURN"], result.Value!.PredicateIds);

        // Domain tags must identify the source and target topology ids.
        Assert.Equal(["technical_design", "implementation"], result.Value.DomainTags);

        // Evidence ids must come from the transition source ids.
        Assert.Equal(["MULTI-PHASE-PLAN"], result.Value.EvidenceIds);
    }

    [Fact]
    public async Task AddTransitionTopologyMutationUnit_InvalidTransition_FailsCleanly()
    {
        // Create the add-transition unit with a real registry.
        var unit = new AddTransitionTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

        // Build a command whose transition has no id.
        var command = new AddTransitionCommand(new LogicalTransition("", "phase", "a", "phase", "b", "Missing id.", [], [], []), "Try invalid transition.");

        // Execute the invalid command.
        var result = await unit.ExecuteAsync(BuildRequest(command));

        // Invalid transitions must fail through the logical-unit envelope.
        Assert.False(result.IsSuccess);

        // The registry error code must be preserved by the unit.
        Assert.Equal("EMPTY_TRANSITION_ID", result.ErrorCode);
    }

    [Fact]
    public async Task AddTransitionTopologyMutationUnit_EmptyReason_FailsCleanly()
    {
        // Create the add-transition unit with a real registry.
        var unit = new AddTransitionTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

        // Build a command without an audit reason.
        var command = new AddTransitionCommand(BuildTransition(), " ");

        // Execute the command without a reason.
        var result = await unit.ExecuteAsync(BuildRequest(command));

        // Empty mutation reasons must fail.
        Assert.False(result.IsSuccess);

        // The error code must identify the missing reason.
        Assert.Equal("EMPTY_REASON", result.ErrorCode);
    }

    [Fact]
    public async Task AddTransitionTopologyMutationUnit_MissingRegistry_FailsCleanly()
    {
        // Create an invalid unit without a registry.
        var unit = new AddTransitionTopologyMutationUnit(null);

        // Execute a valid command against the invalid unit.
        var result = await unit.ExecuteAsync(BuildRequest(BuildCommand()));

        // Missing registry must fail through the unit envelope.
        Assert.False(result.IsSuccess);

        // The error code must identify the missing registry dependency.
        Assert.Equal("TOPOLOGY_REGISTRY_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public async Task AddTransitionTopologyMutationUnit_CancelledRequest_FailsCleanly()
    {
        // Create the add-transition unit with a real registry.
        var unit = new AddTransitionTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

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

    private static LogicalUnitRequest<AddTransitionCommand> BuildRequest(AddTransitionCommand command)
    {
        // Build the standard request shape for the add-transition unit.
        return new LogicalUnitRequest<AddTransitionCommand>(
            "run-add-transition-unit-test",
            AddTransitionTopologyMutationUnit.Id,
            AddTransitionTopologyMutationUnit.Role,
            AddTransitionTopologyMutationUnit.DomainTag,
            command,
            new LogicalExecutionFlags(),
            new Dictionary<string, object> { ["source"] = "phase50" });
    }

    private static AddTransitionCommand BuildCommand()
    {
        // Build a valid add-transition command.
        return new AddTransitionCommand(BuildTransition(), "Register technical design to implementation as a logical topology transition.");
    }

    private static LogicalTransition BuildTransition()
    {
        // Build a valid technical-design to implementation transition.
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
}
