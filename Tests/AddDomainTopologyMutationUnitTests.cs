// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure;
using AgentInfrastructure.Logical;

namespace LlmFromScratch.Tests;

public sealed class AddDomainTopologyMutationUnitTests
{
    [Fact]
    public async Task AddDomainTopologyMutationUnit_ValidCommand_StoresDomain()
    {
        // Create the topology registry for this unit test.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Create the add-domain unit.
        var unit = new AddDomainTopologyMutationUnit(registry);

        // Execute a valid add-domain command.
        var result = await unit.ExecuteAsync(BuildRequest(BuildCommand()));

        // The unit execution must succeed through the shared envelope.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // The registry entry must contain the requested domain id.
        Assert.Equal("technical_design", result.Value!.Output.Domain.DomainId);

        // The first registry mutation must produce revision one.
        Assert.Equal(1, result.Value.Output.Revision);
    }

    [Fact]
    public async Task AddDomainTopologyMutationUnit_OutputCarriesPredicateDomainAndEvidenceIds()
    {
        // Create the add-domain unit with a real registry.
        var unit = new AddDomainTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

        // Execute a valid add-domain command.
        var result = await unit.ExecuteAsync(BuildRequest(BuildCommand()));

        // The unit execution must succeed before output metadata is checked.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Predicate ids must come from the stored domain.
        Assert.Equal(["DECLARE", "RETURN"], result.Value!.PredicateIds);

        // Domain tags must include the stored domain id.
        Assert.Equal(["technical_design"], result.Value.DomainTags);

        // Evidence ids must come from the domain source ids.
        Assert.Equal(["TECHNICAL-DESIGN-PLAN"], result.Value.EvidenceIds);
    }

    [Fact]
    public async Task AddDomainTopologyMutationUnit_InvalidDomain_FailsCleanly()
    {
        // Create the add-domain unit with a real registry.
        var unit = new AddDomainTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

        // Build a command whose domain has no id.
        var command = new AddDomainCommand(new LogicalDomain("", "Broken", "Missing id.", [], []), "Try invalid domain.");

        // Execute the invalid command.
        var result = await unit.ExecuteAsync(BuildRequest(command));

        // Invalid domains must fail through the logical-unit envelope.
        Assert.False(result.IsSuccess);

        // The registry error code must be preserved by the unit.
        Assert.Equal("EMPTY_DOMAIN_ID", result.ErrorCode);
    }

    [Fact]
    public async Task AddDomainTopologyMutationUnit_EmptyReason_FailsCleanly()
    {
        // Create the add-domain unit with a real registry.
        var unit = new AddDomainTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

        // Build a command without an audit reason.
        var command = new AddDomainCommand(BuildDomain(), " ");

        // Execute the command without a reason.
        var result = await unit.ExecuteAsync(BuildRequest(command));

        // Empty mutation reasons must fail.
        Assert.False(result.IsSuccess);

        // The error code must identify the missing reason.
        Assert.Equal("EMPTY_REASON", result.ErrorCode);
    }

    [Fact]
    public async Task AddDomainTopologyMutationUnit_MissingRegistry_FailsCleanly()
    {
        // Create an invalid unit without a registry.
        var unit = new AddDomainTopologyMutationUnit(null);

        // Execute a valid command against the invalid unit.
        var result = await unit.ExecuteAsync(BuildRequest(BuildCommand()));

        // Missing registry must fail through the unit envelope.
        Assert.False(result.IsSuccess);

        // The error code must identify the missing registry dependency.
        Assert.Equal("TOPOLOGY_REGISTRY_REQUIRED", result.ErrorCode);
    }

    [Fact]
    public async Task AddDomainTopologyMutationUnit_CancelledRequest_FailsCleanly()
    {
        // Create the add-domain unit with a real registry.
        var unit = new AddDomainTopologyMutationUnit(new InMemoryLogicalTopologyRegistry());

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

    private static LogicalUnitRequest<AddDomainCommand> BuildRequest(AddDomainCommand command)
    {
        // Build the standard request shape for the add-domain unit.
        return new LogicalUnitRequest<AddDomainCommand>(
            "run-add-domain-unit-test",
            AddDomainTopologyMutationUnit.Id,
            AddDomainTopologyMutationUnit.Role,
            AddDomainTopologyMutationUnit.DomainTag,
            command,
            new LogicalExecutionFlags(),
            new Dictionary<string, object> { ["source"] = "phase35" });
    }

    private static AddDomainCommand BuildCommand()
    {
        // Build a valid add-domain command.
        return new AddDomainCommand(BuildDomain(), "Register technical design as a logical topology domain.");
    }

    private static LogicalDomain BuildDomain()
    {
        // Build a valid technical-design domain.
        return new LogicalDomain(
            "technical_design",
            "Technical Design",
            "Owns technical design orchestration.",
            ["declare", "return"],
            ["technical-design-plan"]);
    }
}
