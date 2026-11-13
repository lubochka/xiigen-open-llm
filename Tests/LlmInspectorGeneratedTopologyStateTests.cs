// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;
using LlmInspector.Inspector;

namespace LlmFromScratch.Tests;

public sealed class LlmInspectorGeneratedTopologyStateTests
{
    [Fact]
    public void GeneratedTopologyState_DomainOutput_AddsDomainNode()
    {
        // Create the generated topology state used by the lightweight viewport.
        var state = new GeneratedTopologyState();

        // Apply a completed domain output tree.
        var changed = state.ApplyOutput(Output(DomainEntry("technical_design", "Technical Design")));

        // The output must change generated topology state.
        Assert.True(changed);

        // Read the immutable topology snapshot.
        var snapshot = state.Snapshot();

        // The domain output must create exactly one visible node.
        var node = Assert.Single(snapshot.Nodes);

        // The generated key must use the normalized domain kind.
        Assert.Equal("domain:technical_design", node.Key);

        // The generated node must be a domain.
        Assert.Equal("domain", node.Kind);

        // The domain node must not have a parent.
        Assert.Null(node.ParentKey);
    }

    [Fact]
    public void GeneratedTopologyState_PhaseBeforeDomain_BuffersThenAttaches()
    {
        // Create the generated topology state used by the lightweight viewport.
        var state = new GeneratedTopologyState();

        // Apply a phase before its parent domain output has arrived.
        var buffered = state.ApplyOutput(Output(PhaseEntry("implementation", "Implementation", "technical_design")));

        // Buffering is a meaningful state change.
        Assert.True(buffered);

        // Read the snapshot after the out-of-order phase.
        var beforeParent = state.Snapshot();

        // The phase must wait because its parent domain is missing.
        Assert.Equal(1, beforeParent.PendingPhaseCount);

        // No visible node can appear before the parent exists.
        Assert.Empty(beforeParent.Nodes);

        // Apply the parent domain output.
        var attached = state.ApplyOutput(Output(DomainEntry("technical_design", "Technical Design")));

        // Adding the parent must attach the pending phase.
        Assert.True(attached);

        // Read the snapshot after the parent arrives.
        var afterParent = state.Snapshot();

        // The pending phase buffer must be empty after attachment.
        Assert.Equal(0, afterParent.PendingPhaseCount);

        // The domain and phase must both be visible.
        Assert.Equal(2, afterParent.Nodes.Count);

        // The phase must attach under the generated domain key.
        Assert.Equal("domain:technical_design", RequiredNode(afterParent, "phase:implementation").ParentKey);
    }

    [Fact]
    public void GeneratedTopologyState_SubPhaseBeforePhase_BuffersThenAttaches()
    {
        // Create the generated topology state used by the lightweight viewport.
        var state = new GeneratedTopologyState();

        // Apply the domain so the later phase has a valid parent.
        state.ApplyOutput(Output(DomainEntry("technical_design", "Technical Design")));

        // Apply a sub-phase before its parent phase output has arrived.
        var buffered = state.ApplyOutput(Output(SubPhaseEntry("architecture_decisions", "technical_design_phase", "Architecture Decisions")));

        // Buffering is a meaningful state change.
        Assert.True(buffered);

        // Read the snapshot after the out-of-order sub-phase.
        var beforeParent = state.Snapshot();

        // The sub-phase must wait because its parent phase is missing.
        Assert.Equal(1, beforeParent.PendingSubPhaseCount);

        // Only the domain should be visible before the phase arrives.
        Assert.Single(beforeParent.Nodes);

        // Apply the missing parent phase output.
        var attached = state.ApplyOutput(Output(PhaseEntry("technical_design_phase", "Technical Design Phase", "technical_design")));

        // Adding the parent phase must attach the pending sub-phase.
        Assert.True(attached);

        // Read the snapshot after the parent phase arrives.
        var afterParent = state.Snapshot();

        // The pending sub-phase buffer must be empty after attachment.
        Assert.Equal(0, afterParent.PendingSubPhaseCount);

        // The domain, phase, and sub-phase must all be visible.
        Assert.Equal(3, afterParent.Nodes.Count);

        // The sub-phase must attach under the generated phase key.
        Assert.Equal("phase:technical_design_phase", RequiredNode(afterParent, "subphase:architecture_decisions").ParentKey);
    }

    [Fact]
    public void GeneratedTopologyState_TransitionBeforeTarget_BuffersThenAttaches()
    {
        // Create the generated topology state used by the lightweight viewport.
        var state = new GeneratedTopologyState();

        // Apply the source domain output first.
        state.ApplyOutput(Output(DomainEntry("technical_design", "Technical Design")));

        // Apply a transition before the target domain output has arrived.
        var buffered = state.ApplyOutput(Output(TransitionEntry("technical-to-implementation", "domain", "technical_design", "domain", "implementation")));

        // Buffering is a meaningful state change.
        Assert.True(buffered);

        // Read the snapshot after the out-of-order transition.
        var beforeTarget = state.Snapshot();

        // The transition must wait because its target endpoint is missing.
        Assert.Equal(1, beforeTarget.PendingTransitionCount);

        // No transition edge can appear before both endpoints exist.
        Assert.Empty(beforeTarget.Transitions);

        // Apply the target domain output.
        var attached = state.ApplyOutput(Output(DomainEntry("implementation", "Implementation")));

        // Adding the target must attach the pending transition.
        Assert.True(attached);

        // Read the snapshot after the target arrives.
        var afterTarget = state.Snapshot();

        // The pending transition buffer must be empty after attachment.
        Assert.Equal(0, afterTarget.PendingTransitionCount);

        // The transition must now be visible.
        var transition = Assert.Single(afterTarget.Transitions);

        // The transition source key must point at the generated source domain.
        Assert.Equal("domain:technical_design", transition.FromKey);

        // The transition target key must point at the generated target domain.
        Assert.Equal("domain:implementation", transition.ToKey);
    }

    private static IReadOnlyList<PropertyNode> Output(object value)
    {
        // Serialise a runtime output object exactly as station traces do.
        return PropertyNodeSerialiser.Serialise(value);
    }

    private static GeneratedTopologyNode RequiredNode(GeneratedTopologySnapshot snapshot, string key)
    {
        // Find the requested generated node by stable key.
        var node = snapshot.Nodes.FirstOrDefault(candidate => string.Equals(candidate.Key, key, StringComparison.Ordinal));

        // Fail clearly if the node did not appear.
        Assert.NotNull(node);

        // Return the node after the assertion proves it exists.
        return node!;
    }

    private static LogicalDomainEntry DomainEntry(string id, string name)
    {
        // Build a domain entry with the exact property shape produced by storage output.
        return new LogicalDomainEntry(
            new LogicalDomain(id, name, "Generated domain for inspector topology test.", ["DECLARE"], ["TEST"]),
            1,
            DateTimeOffset.Parse("2026-05-05T00:00:00.0000000+00:00"));
    }

    private static LogicalPhaseEntry PhaseEntry(string id, string name, string domainId)
    {
        // Build a phase entry with the exact property shape produced by storage output.
        return new LogicalPhaseEntry(
            new LogicalPhase(id, name, "Generated phase for inspector topology test.", [domainId], ["PLAN"], ["TEST"]),
            2,
            DateTimeOffset.Parse("2026-05-05T00:00:01.0000000+00:00"));
    }

    private static LogicalSubPhaseEntry SubPhaseEntry(string id, string phaseId, string name)
    {
        // Build a sub-phase entry with the exact property shape produced by storage output.
        return new LogicalSubPhaseEntry(
            new LogicalSubPhase(id, phaseId, name, "Generated sub-phase for inspector topology test.", ["technical_design"], ["PLAN"], ["TEST"]),
            3,
            DateTimeOffset.Parse("2026-05-05T00:00:02.0000000+00:00"));
    }

    private static LogicalTransitionEntry TransitionEntry(string id, string fromKind, string fromId, string toKind, string toId)
    {
        // Build a transition entry with the exact property shape produced by storage output.
        return new LogicalTransitionEntry(
            new LogicalTransition(id, fromKind, fromId, toKind, toId, "Generated transition for inspector topology test.", ["HANDOFF"], ["PRESERVE_CONTEXT"], ["TEST"]),
            4,
            DateTimeOffset.Parse("2026-05-05T00:00:03.0000000+00:00"));
    }
}
