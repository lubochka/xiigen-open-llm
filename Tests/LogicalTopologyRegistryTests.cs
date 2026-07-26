// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;

namespace LlmFromScratch.Tests;

public sealed class LogicalTopologyRegistryTests
{
    [Fact]
    public void LogicalTopologyRegistry_AddDomain_RegistersDomain()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the technical design domain.
        var added = registry.AddOrUpdateDomain(BuildDomain("technical_design", "Technical Design"));

        // The add operation must succeed through the runtime envelope.
        Assert.True(added.IsSuccess, added.ErrorMessage);

        // The stored domain id must match the caller id.
        Assert.Equal("technical_design", added.Value!.Domain.DomainId);

        // The first mutation must receive revision one.
        Assert.Equal(1, added.Value.Revision);
    }

    [Fact]
    public void LogicalTopologyRegistry_AddDomain_NormalizesPredicateAndSourceIds()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add a domain with duplicated and differently-cased identifiers.
        var added = registry.AddOrUpdateDomain(new LogicalDomain(
            "implementation",
            "Implementation",
            "Owns code construction work.",
            ["declare", "DECLARE", " return ", ""],
            ["doc-1", "DOC-1", " plan-a "]));

        // The add operation must succeed before normalization is checked.
        Assert.True(added.IsSuccess, added.ErrorMessage);

        // Predicate ids must be uppercase, distinct, and non-empty.
        Assert.Equal(["DECLARE", "RETURN"], added.Value!.Domain.PredicateIds);

        // Source ids must be uppercase, distinct, and non-empty.
        Assert.Equal(["DOC-1", "PLAN-A"], added.Value.Domain.SourceIds);
    }

    [Fact]
    public void LogicalTopologyRegistry_EditDomain_UpdatesValueAndRevision()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the initial domain definition.
        registry.AddOrUpdateDomain(BuildDomain("qa", "QA"));

        // Update the same domain with a clearer purpose.
        var updated = registry.AddOrUpdateDomain(new LogicalDomain(
            "qa",
            "Quality Assurance",
            "Owns manual release confidence checks.",
            ["VALIDATE_OP"],
            ["qa-plan"]));

        // The update operation must succeed.
        Assert.True(updated.IsSuccess, updated.ErrorMessage);

        // The display name must reflect the latest domain definition.
        Assert.Equal("Quality Assurance", updated.Value!.Domain.Name);

        // The revision must advance after the update.
        Assert.Equal(2, updated.Value.Revision);
    }

    [Fact]
    public void LogicalTopologyRegistry_ResolveDomain_ReturnsStoredDomain()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the UI/UX definition domain.
        registry.AddOrUpdateDomain(BuildDomain("ui_ux_definition", "UI UX Definition"));

        // Resolve the domain by id.
        var resolved = registry.ResolveDomain("ui_ux_definition");

        // The resolve operation must succeed.
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);

        // The resolved domain must carry the requested id.
        Assert.Equal("ui_ux_definition", resolved.Value!.Domain.DomainId);
    }

    [Fact]
    public void LogicalTopologyRegistry_MissingDomain_FailsCleanly()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Resolve a domain that was not added.
        var resolved = registry.ResolveDomain("missing");

        // Missing domains must fail through the runtime envelope.
        Assert.False(resolved.IsSuccess);

        // The error code must identify a missing domain.
        Assert.Equal("DOMAIN_NOT_FOUND", resolved.ErrorCode);
    }

    [Fact]
    public void LogicalTopologyRegistry_InvalidDomain_FailsCleanly()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Try to add a domain without a stable id.
        var added = registry.AddOrUpdateDomain(new LogicalDomain("", "Broken", "Missing id.", [], []));

        // Invalid domains must fail through the runtime envelope.
        Assert.False(added.IsSuccess);

        // The error code must identify the empty domain id.
        Assert.Equal("EMPTY_DOMAIN_ID", added.ErrorCode);
    }

    [Fact]
    public void LogicalTopologyRegistry_ListDomains_ReturnsRevisionOrder()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the first domain.
        registry.AddOrUpdateDomain(BuildDomain("functional_spec", "Functional Spec"));

        // Add the second domain.
        registry.AddOrUpdateDomain(BuildDomain("technical_design", "Technical Design"));

        // List all domains.
        var listed = registry.ListDomains();

        // The list operation must succeed.
        Assert.True(listed.IsSuccess, listed.ErrorMessage);

        // The domains must appear in mutation order.
        Assert.Equal(["functional_spec", "technical_design"], listed.Value!.Select(entry => entry.Domain.DomainId).ToArray());
    }

    [Fact]
    public void LogicalTopologyRegistry_AddPhase_RegistersPhase()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the technical design phase.
        var added = registry.AddOrUpdatePhase(BuildPhase("technical_design", "Technical Design"));

        // The add operation must succeed through the runtime envelope.
        Assert.True(added.IsSuccess, added.ErrorMessage);

        // The stored phase id must match the caller id.
        Assert.Equal("technical_design", added.Value!.Phase.PhaseId);

        // The first topology mutation must receive revision one.
        Assert.Equal(1, added.Value.Revision);
    }

    [Fact]
    public void LogicalTopologyRegistry_AddPhase_NormalizesIds()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add a phase with duplicated and differently-cased identifiers.
        var added = registry.AddOrUpdatePhase(new LogicalPhase(
            " implementation ",
            " Implementation ",
            " Owns code construction work. ",
            ["technical_design", "technical_design", " qa "],
            ["declare", "DECLARE", " return ", ""],
            ["doc-1", "DOC-1", " plan-a "]));

        // The add operation must succeed before normalization is checked.
        Assert.True(added.IsSuccess, added.ErrorMessage);

        // The phase id must be trimmed.
        Assert.Equal("implementation", added.Value!.Phase.PhaseId);

        // Domain ids must be trimmed, distinct, and preserve stable id casing.
        Assert.Equal(["technical_design", "qa"], added.Value.Phase.DomainIds);

        // Predicate ids must be uppercase, distinct, and non-empty.
        Assert.Equal(["DECLARE", "RETURN"], added.Value.Phase.PredicateIds);

        // Source ids must be uppercase, distinct, and non-empty.
        Assert.Equal(["DOC-1", "PLAN-A"], added.Value.Phase.SourceIds);
    }

    [Fact]
    public void LogicalTopologyRegistry_EditPhase_UpdatesValueAndRevision()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the initial phase definition.
        registry.AddOrUpdatePhase(BuildPhase("qa", "QA"));

        // Update the same phase with a clearer purpose.
        var updated = registry.AddOrUpdatePhase(new LogicalPhase(
            "qa",
            "Quality Assurance",
            "Owns release confidence checks.",
            ["testing"],
            ["VALIDATE_OP"],
            ["qa-plan"]));

        // The update operation must succeed.
        Assert.True(updated.IsSuccess, updated.ErrorMessage);

        // The display name must reflect the latest phase definition.
        Assert.Equal("Quality Assurance", updated.Value!.Phase.Name);

        // The revision must advance after the update.
        Assert.Equal(2, updated.Value.Revision);
    }

    [Fact]
    public void LogicalTopologyRegistry_ResolvePhase_ReturnsStoredPhase()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the implementation phase.
        registry.AddOrUpdatePhase(BuildPhase("implementation", "Implementation"));

        // Resolve the phase by id.
        var resolved = registry.ResolvePhase("implementation");

        // The resolve operation must succeed.
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);

        // The resolved phase must carry the requested id.
        Assert.Equal("implementation", resolved.Value!.Phase.PhaseId);
    }

    [Fact]
    public void LogicalTopologyRegistry_MissingPhase_FailsCleanly()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Resolve a phase that was not added.
        var resolved = registry.ResolvePhase("missing");

        // Missing phases must fail through the runtime envelope.
        Assert.False(resolved.IsSuccess);

        // The error code must identify a missing phase.
        Assert.Equal("PHASE_NOT_FOUND", resolved.ErrorCode);
    }

    [Fact]
    public void LogicalTopologyRegistry_InvalidPhase_FailsCleanly()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Try to add a phase without a stable id.
        var added = registry.AddOrUpdatePhase(new LogicalPhase("", "Broken", "Missing id.", ["domain"], [], []));

        // Invalid phases must fail through the runtime envelope.
        Assert.False(added.IsSuccess);

        // The error code must identify the empty phase id.
        Assert.Equal("EMPTY_PHASE_ID", added.ErrorCode);
    }

    [Fact]
    public void LogicalTopologyRegistry_PhaseWithoutDomains_FailsCleanly()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Try to add a phase with no domain ownership.
        var added = registry.AddOrUpdatePhase(new LogicalPhase("phase", "Phase", "No domains.", [], [], []));

        // Domainless phases must fail through the runtime envelope.
        Assert.False(added.IsSuccess);

        // The error code must identify missing phase domain ids.
        Assert.Equal("EMPTY_PHASE_DOMAIN_IDS", added.ErrorCode);
    }

    [Fact]
    public void LogicalTopologyRegistry_ListPhases_ReturnsRevisionOrder()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the first phase.
        registry.AddOrUpdatePhase(BuildPhase("functional_spec", "Functional Spec"));

        // Add the second phase.
        registry.AddOrUpdatePhase(BuildPhase("technical_design", "Technical Design"));

        // List all phases.
        var listed = registry.ListPhases();

        // The list operation must succeed.
        Assert.True(listed.IsSuccess, listed.ErrorMessage);

        // The phases must appear in mutation order.
        Assert.Equal(["functional_spec", "technical_design"], listed.Value!.Select(entry => entry.Phase.PhaseId).ToArray());
    }

    [Fact]
    public void LogicalTopologyRegistry_AddSubPhase_RegistersSubPhase()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the implementation planning sub-phase.
        var added = registry.AddOrUpdateSubPhase(BuildSubPhase("implementation_planning", "Implementation Planning"));

        // The add operation must succeed through the runtime envelope.
        Assert.True(added.IsSuccess, added.ErrorMessage);

        // The stored sub-phase id must match the caller id.
        Assert.Equal("implementation_planning", added.Value!.SubPhase.SubPhaseId);

        // The first topology mutation must receive revision one.
        Assert.Equal(1, added.Value.Revision);
    }

    [Fact]
    public void LogicalTopologyRegistry_AddSubPhase_NormalizesIds()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add a sub-phase with duplicated and differently-cased identifiers.
        var added = registry.AddOrUpdateSubPhase(new LogicalSubPhase(
            " implementation_planning ",
            " implementation ",
            " Implementation Planning ",
            " Plan concrete implementation work. ",
            ["technical_design", "technical_design", " implementation "],
            ["create", "CREATE", " assign ", ""],
            ["doc-1", "DOC-1", " plan-a "]));

        // The add operation must succeed before normalization is checked.
        Assert.True(added.IsSuccess, added.ErrorMessage);

        // The sub-phase id must be trimmed.
        Assert.Equal("implementation_planning", added.Value!.SubPhase.SubPhaseId);

        // The parent phase id must be trimmed.
        Assert.Equal("implementation", added.Value.SubPhase.PhaseId);

        // Domain ids must be trimmed, distinct, and preserve stable id casing.
        Assert.Equal(["technical_design", "implementation"], added.Value.SubPhase.DomainIds);

        // Predicate ids must be uppercase, distinct, and non-empty.
        Assert.Equal(["CREATE", "ASSIGN"], added.Value.SubPhase.PredicateIds);

        // Source ids must be uppercase, distinct, and non-empty.
        Assert.Equal(["DOC-1", "PLAN-A"], added.Value.SubPhase.SourceIds);
    }

    [Fact]
    public void LogicalTopologyRegistry_EditSubPhase_UpdatesValueAndRevision()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the initial sub-phase definition.
        registry.AddOrUpdateSubPhase(BuildSubPhase("qa_manual", "QA Manual"));

        // Update the same sub-phase with a clearer purpose.
        var updated = registry.AddOrUpdateSubPhase(new LogicalSubPhase(
            "qa_manual",
            "qa",
            "Manual Quality Assurance",
            "Owns manual confidence checks.",
            ["qa"],
            ["VALIDATE_OP"],
            ["qa-plan"]));

        // The update operation must succeed.
        Assert.True(updated.IsSuccess, updated.ErrorMessage);

        // The display name must reflect the latest sub-phase definition.
        Assert.Equal("Manual Quality Assurance", updated.Value!.SubPhase.Name);

        // The revision must advance after the update.
        Assert.Equal(2, updated.Value.Revision);
    }

    [Fact]
    public void LogicalTopologyRegistry_ResolveSubPhase_ReturnsStoredSubPhase()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the implementation planning sub-phase.
        registry.AddOrUpdateSubPhase(BuildSubPhase("implementation_planning", "Implementation Planning"));

        // Resolve the sub-phase by id.
        var resolved = registry.ResolveSubPhase("implementation_planning");

        // The resolve operation must succeed.
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);

        // The resolved sub-phase must carry the requested id.
        Assert.Equal("implementation_planning", resolved.Value!.SubPhase.SubPhaseId);
    }

    [Fact]
    public void LogicalTopologyRegistry_MissingSubPhase_FailsCleanly()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Resolve a sub-phase that was not added.
        var resolved = registry.ResolveSubPhase("missing");

        // Missing sub-phases must fail through the runtime envelope.
        Assert.False(resolved.IsSuccess);

        // The error code must identify a missing sub-phase.
        Assert.Equal("SUB_PHASE_NOT_FOUND", resolved.ErrorCode);
    }

    [Fact]
    public void LogicalTopologyRegistry_InvalidSubPhase_FailsCleanly()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Try to add a sub-phase without a stable id.
        var added = registry.AddOrUpdateSubPhase(new LogicalSubPhase("", "phase", "Broken", "Missing id.", ["domain"], [], []));

        // Invalid sub-phases must fail through the runtime envelope.
        Assert.False(added.IsSuccess);

        // The error code must identify the empty sub-phase id.
        Assert.Equal("EMPTY_SUB_PHASE_ID", added.ErrorCode);
    }

    [Fact]
    public void LogicalTopologyRegistry_SubPhaseWithoutParentPhase_FailsCleanly()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Try to add a sub-phase without a parent phase id.
        var added = registry.AddOrUpdateSubPhase(new LogicalSubPhase("sub", "", "Sub", "No parent.", ["domain"], [], []));

        // Parentless sub-phases must fail through the runtime envelope.
        Assert.False(added.IsSuccess);

        // The error code must identify the missing parent phase id.
        Assert.Equal("EMPTY_PARENT_PHASE_ID", added.ErrorCode);
    }

    [Fact]
    public void LogicalTopologyRegistry_SubPhaseWithoutDomains_FailsCleanly()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Try to add a sub-phase with no domain ownership.
        var added = registry.AddOrUpdateSubPhase(new LogicalSubPhase("sub", "phase", "Sub", "No domains.", [], [], []));

        // Domainless sub-phases must fail through the runtime envelope.
        Assert.False(added.IsSuccess);

        // The error code must identify missing sub-phase domain ids.
        Assert.Equal("EMPTY_SUB_PHASE_DOMAIN_IDS", added.ErrorCode);
    }

    [Fact]
    public void LogicalTopologyRegistry_ListSubPhases_ReturnsRevisionOrder()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the first sub-phase.
        registry.AddOrUpdateSubPhase(BuildSubPhase("design_tokens", "Design Tokens"));

        // Add the second sub-phase.
        registry.AddOrUpdateSubPhase(BuildSubPhase("interaction_model", "Interaction Model"));

        // List all sub-phases.
        var listed = registry.ListSubPhases();

        // The list operation must succeed.
        Assert.True(listed.IsSuccess, listed.ErrorMessage);

        // The sub-phases must appear in mutation order.
        Assert.Equal(["design_tokens", "interaction_model"], listed.Value!.Select(entry => entry.SubPhase.SubPhaseId).ToArray());
    }

    [Fact]
    public void LogicalTopologyRegistry_AddTransition_RegistersTransition()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the technical-design to implementation transition.
        var added = registry.AddOrUpdateTransition(BuildTransition("technical-to-implementation"));

        // The add operation must succeed through the runtime envelope.
        Assert.True(added.IsSuccess, added.ErrorMessage);

        // The stored transition id must match the caller id.
        Assert.Equal("technical-to-implementation", added.Value!.Transition.TransitionId);

        // The first topology mutation must receive revision one.
        Assert.Equal(1, added.Value.Revision);
    }

    [Fact]
    public void LogicalTopologyRegistry_AddTransition_NormalizesIds()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add a transition with whitespace and duplicated identifiers.
        var added = registry.AddOrUpdateTransition(new LogicalTransition(
            " technical-to-implementation ",
            " phase ",
            " technical_design ",
            " phase ",
            " implementation ",
            " Carry technical design decisions into implementation. ",
            ["declare", "DECLARE", " return "],
            ["preserve_constraints", "PRESERVE_CONSTRAINTS", ""],
            ["doc-1", "DOC-1"]));

        // The add operation must succeed before normalization is checked.
        Assert.True(added.IsSuccess, added.ErrorMessage);

        // The transition id must be trimmed.
        Assert.Equal("technical-to-implementation", added.Value!.Transition.TransitionId);

        // The source topology id must be trimmed.
        Assert.Equal("technical_design", added.Value.Transition.FromTopologyId);

        // The target topology id must be trimmed.
        Assert.Equal("implementation", added.Value.Transition.ToTopologyId);

        // Predicate ids must be uppercase, distinct, and non-empty.
        Assert.Equal(["DECLARE", "RETURN"], added.Value.Transition.PredicateIds);

        // Constraint ids must be uppercase, distinct, and non-empty.
        Assert.Equal(["PRESERVE_CONSTRAINTS"], added.Value.Transition.ConstraintIds);

        // Source ids must be uppercase, distinct, and non-empty.
        Assert.Equal(["DOC-1"], added.Value.Transition.SourceIds);
    }

    [Fact]
    public void LogicalTopologyRegistry_EditTransition_UpdatesValueAndRevision()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the initial transition definition.
        registry.AddOrUpdateTransition(BuildTransition("qa-to-automation"));

        // Update the same transition with a clearer purpose.
        var updated = registry.AddOrUpdateTransition(new LogicalTransition(
            "qa-to-automation",
            "phase",
            "qa",
            "phase",
            "qa_automation",
            "Carry manual QA findings into automation coverage.",
            ["VALIDATE_OP"],
            ["PRESERVE_REJECTIONS"],
            ["qa-plan"]));

        // The update operation must succeed.
        Assert.True(updated.IsSuccess, updated.ErrorMessage);

        // The purpose must reflect the latest transition definition.
        Assert.Equal("Carry manual QA findings into automation coverage.", updated.Value!.Transition.Purpose);

        // The revision must advance after the update.
        Assert.Equal(2, updated.Value.Revision);
    }

    [Fact]
    public void LogicalTopologyRegistry_ResolveTransition_ReturnsStoredTransition()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the technical-design to implementation transition.
        registry.AddOrUpdateTransition(BuildTransition("technical-to-implementation"));

        // Resolve the transition by id.
        var resolved = registry.ResolveTransition("technical-to-implementation");

        // The resolve operation must succeed.
        Assert.True(resolved.IsSuccess, resolved.ErrorMessage);

        // The resolved transition must carry the requested id.
        Assert.Equal("technical-to-implementation", resolved.Value!.Transition.TransitionId);
    }

    [Fact]
    public void LogicalTopologyRegistry_MissingTransition_FailsCleanly()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Resolve a transition that was not added.
        var resolved = registry.ResolveTransition("missing");

        // Missing transitions must fail through the runtime envelope.
        Assert.False(resolved.IsSuccess);

        // The error code must identify a missing transition.
        Assert.Equal("TRANSITION_NOT_FOUND", resolved.ErrorCode);
    }

    [Fact]
    public void LogicalTopologyRegistry_InvalidTransition_FailsCleanly()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Try to add a transition without a stable id.
        var added = registry.AddOrUpdateTransition(new LogicalTransition("", "phase", "a", "phase", "b", "Missing id.", [], [], []));

        // Invalid transitions must fail through the runtime envelope.
        Assert.False(added.IsSuccess);

        // The error code must identify the empty transition id.
        Assert.Equal("EMPTY_TRANSITION_ID", added.ErrorCode);
    }

    [Fact]
    public void LogicalTopologyRegistry_TransitionWithoutTarget_FailsCleanly()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Try to add a transition without a target id.
        var added = registry.AddOrUpdateTransition(new LogicalTransition("edge", "phase", "a", "phase", "", "Missing target.", [], [], []));

        // Targetless transitions must fail through the runtime envelope.
        Assert.False(added.IsSuccess);

        // The error code must identify the missing target id.
        Assert.Equal("EMPTY_TO_TOPOLOGY_ID", added.ErrorCode);
    }

    [Fact]
    public void LogicalTopologyRegistry_ListTransitions_ReturnsRevisionOrder()
    {
        // Create the runtime topology registry.
        var registry = new InMemoryLogicalTopologyRegistry();

        // Add the first transition.
        registry.AddOrUpdateTransition(BuildTransition("ui-to-functional"));

        // Add the second transition.
        registry.AddOrUpdateTransition(BuildTransition("functional-to-technical"));

        // List all transitions.
        var listed = registry.ListTransitions();

        // The list operation must succeed.
        Assert.True(listed.IsSuccess, listed.ErrorMessage);

        // The transitions must appear in mutation order.
        Assert.Equal(["ui-to-functional", "functional-to-technical"], listed.Value!.Select(entry => entry.Transition.TransitionId).ToArray());
    }

    private static LogicalDomain BuildDomain(string domainId, string name)
    {
        // Build a small valid logical domain for registry tests.
        return new LogicalDomain(
            domainId,
            name,
            $"Owns {name} orchestration.",
            ["DECLARE", "RETURN"],
            [$"{domainId}-source"]);
    }

    private static LogicalPhase BuildPhase(string phaseId, string name)
    {
        // Build a small valid logical phase for registry tests.
        return new LogicalPhase(
            phaseId,
            name,
            $"Owns {name} lifecycle orchestration.",
            [$"{phaseId}_domain"],
            ["DECLARE", "RETURN"],
            [$"{phaseId}-source"]);
    }

    private static LogicalSubPhase BuildSubPhase(string subPhaseId, string name)
    {
        // Build a small valid logical sub-phase for registry tests.
        return new LogicalSubPhase(
            subPhaseId,
            "implementation",
            name,
            $"Owns {name} sub-phase orchestration.",
            [$"{subPhaseId}_domain"],
            ["CREATE", "ASSIGN"],
            [$"{subPhaseId}-source"]);
    }

    private static LogicalTransition BuildTransition(string transitionId)
    {
        // Build a small valid logical transition for registry tests.
        return new LogicalTransition(
            transitionId,
            "phase",
            "technical_design",
            "phase",
            "implementation",
            "Carries upstream constraints into downstream orchestration.",
            ["DECLARE", "RETURN"],
            ["PRESERVE_CONSTRAINTS"],
            [$"{transitionId}-source"]);
    }
}
