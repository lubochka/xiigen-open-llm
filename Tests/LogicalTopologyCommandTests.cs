// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace LlmFromScratch.Tests;

public sealed class LogicalTopologyCommandTests
{
    [Fact]
    public void AddDomainCommand_IsLogicalTopologyCommand()
    {
        // Build the add-domain command for a technical-design domain.
        LogicalTopologyCommand command = new AddDomainCommand(BuildDomain(), "Add technical design coverage.");

        // The command must remain assignable through the base command type.
        Assert.IsType<AddDomainCommand>(command);

        // The base command must preserve the human reason.
        Assert.Equal("Add technical design coverage.", command.Reason);
    }

    [Fact]
    public void AddDomainCommand_CarriesExactDomainPayload()
    {
        // Build the source domain payload.
        var domain = BuildDomain();

        // Build the add-domain command around that payload.
        var command = new AddDomainCommand(domain, "Register the domain before mutation units exist.");

        // The command must preserve the exact domain object.
        Assert.Same(domain, command.Domain);

        // The domain id must be visible to future mutation units.
        Assert.Equal("technical_design", command.Domain.DomainId);
    }

    [Fact]
    public void AddPhaseCommand_IsLogicalTopologyCommand()
    {
        // Build the add-phase command for an implementation phase.
        LogicalTopologyCommand command = new AddPhaseCommand(BuildPhase(), "Add implementation phase coverage.");

        // The command must remain assignable through the base command type.
        Assert.IsType<AddPhaseCommand>(command);

        // The base command must preserve the human reason.
        Assert.Equal("Add implementation phase coverage.", command.Reason);
    }

    [Fact]
    public void AddPhaseCommand_CarriesExactPhasePayload()
    {
        // Build the source phase payload.
        var phase = BuildPhase();

        // Build the add-phase command around that payload.
        var command = new AddPhaseCommand(phase, "Register the phase before mutation units exist.");

        // The command must preserve the exact phase object.
        Assert.Same(phase, command.Phase);

        // The phase id must be visible to future mutation units.
        Assert.Equal("implementation", command.Phase.PhaseId);
    }

    [Fact]
    public void AddSubPhaseCommand_IsLogicalTopologyCommand()
    {
        // Build the add-sub-phase command for an implementation planning sub-phase.
        LogicalTopologyCommand command = new AddSubPhaseCommand(BuildSubPhase(), "Add implementation planning sub-phase coverage.");

        // The command must remain assignable through the base command type.
        Assert.IsType<AddSubPhaseCommand>(command);

        // The base command must preserve the human reason.
        Assert.Equal("Add implementation planning sub-phase coverage.", command.Reason);
    }

    [Fact]
    public void AddSubPhaseCommand_CarriesExactSubPhasePayload()
    {
        // Build the source sub-phase payload.
        var subPhase = BuildSubPhase();

        // Build the add-sub-phase command around that payload.
        var command = new AddSubPhaseCommand(subPhase, "Register the sub-phase before mutation units exist.");

        // The command must preserve the exact sub-phase object.
        Assert.Same(subPhase, command.SubPhase);

        // The sub-phase id must be visible to future mutation units.
        Assert.Equal("implementation_planning", command.SubPhase.SubPhaseId);
    }

    [Fact]
    public void AddTransitionCommand_IsLogicalTopologyCommand()
    {
        // Build the add-transition command for a phase handoff.
        LogicalTopologyCommand command = new AddTransitionCommand(BuildTransition(), "Add technical to implementation transition coverage.");

        // The command must remain assignable through the base command type.
        Assert.IsType<AddTransitionCommand>(command);

        // The base command must preserve the human reason.
        Assert.Equal("Add technical to implementation transition coverage.", command.Reason);
    }

    [Fact]
    public void AddTransitionCommand_CarriesExactTransitionPayload()
    {
        // Build the source transition payload.
        var transition = BuildTransition();

        // Build the add-transition command around that payload.
        var command = new AddTransitionCommand(transition, "Register the transition before mutation units exist.");

        // The command must preserve the exact transition object.
        Assert.Same(transition, command.Transition);

        // The transition id must be visible to future mutation units.
        Assert.Equal("technical-to-implementation", command.Transition.TransitionId);
    }

    [Fact]
    public void LogicalTopologyCommand_Hierarchy_IsDiscriminatedByRuntimeType()
    {
        // Build the topology commands through the base type.
        LogicalTopologyCommand[] commands =
        [
            // Add one runtime domain command.
            new AddDomainCommand(BuildDomain(), "Add one runtime domain."),

            // Add one runtime phase command.
            new AddPhaseCommand(BuildPhase(), "Add one runtime phase."),

            // Add one runtime sub-phase command.
            new AddSubPhaseCommand(BuildSubPhase(), "Add one runtime sub-phase."),

            // Add one runtime transition command.
            new AddTransitionCommand(BuildTransition(), "Add one runtime transition.")
        ];

        // Switch on the runtime command types exactly as the future mutation flow will do.
        var targetTypes = commands.Select(command => command switch
        {
            // AddDomainCommand should be recognized as the add-domain branch.
            AddDomainCommand => "domain",

            // AddPhaseCommand should be recognized as the add-phase branch.
            AddPhaseCommand => "phase",

            // AddSubPhaseCommand should be recognized as the add-sub-phase branch.
            AddSubPhaseCommand => "sub-phase",

            // AddTransitionCommand should be recognized as the add-transition branch.
            AddTransitionCommand => "transition",

            // Unknown command subtypes should remain explicit future work.
            _ => "unknown"
        }).ToArray();

        // The typed branches must be selected without string operation fields.
        Assert.Equal(["domain", "phase", "sub-phase", "transition"], targetTypes);
    }

    private static LogicalDomain BuildDomain()
    {
        // Build a valid domain payload for command-shape tests.
        return new LogicalDomain(
            "technical_design",
            "Technical Design",
            "Owns technical design orchestration.",
            ["DECLARE", "RETURN"],
            ["technical-design-plan"]);
    }

    private static LogicalPhase BuildPhase()
    {
        // Build a valid phase payload for command-shape tests.
        return new LogicalPhase(
            "implementation",
            "Implementation",
            "Owns implementation orchestration.",
            ["technical_design"],
            ["CREATE", "ASSIGN"],
            ["implementation-plan"]);
    }

    private static LogicalSubPhase BuildSubPhase()
    {
        // Build a valid sub-phase payload for command-shape tests.
        return new LogicalSubPhase(
            "implementation_planning",
            "implementation",
            "Implementation Planning",
            "Owns implementation planning orchestration.",
            ["implementation"],
            ["CREATE", "ITERATE"],
            ["implementation-plan"]);
    }

    private static LogicalTransition BuildTransition()
    {
        // Build a valid transition payload for command-shape tests.
        return new LogicalTransition(
            "technical-to-implementation",
            "phase",
            "technical_design",
            "phase",
            "implementation",
            "Carries technical design decisions into implementation.",
            ["DECLARE", "RETURN"],
            ["PRESERVE_CONSTRAINTS"],
            ["multi-phase-plan"]);
    }
}
