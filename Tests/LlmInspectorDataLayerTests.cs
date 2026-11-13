// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;
using LlmInspector.Inspector;

namespace LlmFromScratch.Tests;

public sealed class LlmInspectorDataLayerTests
{
    [Fact]
    public void PropertyNodeSerialiser_LogicalDomainEntry_EmitsDomainFields()
    {
        // Create a domain entry with nested predicate and source arrays.
        var entry = new LogicalDomainEntry(
            new LogicalDomain(
                "technical_design",
                "Technical Design",
                "Owns technical design orchestration.",
                ["DECLARE", "RETURN"],
                ["TECHNICAL-DESIGN-PLAN"]),
            7,
            DateTimeOffset.Parse("2026-05-04T20:00:00.0000000+00:00"));

        // Serialise the entry through the inspector data layer.
        var nodes = PropertyNodeSerialiser.Serialise(entry);

        // The nested domain branch must be visible to the inspector.
        var domain = Required(nodes, "Domain");

        // The domain id must be inspectable as a leaf value.
        Assert.Equal("technical_design", Required(domain.Children, "DomainId").Value);

        // The display name must be inspectable as a leaf value.
        Assert.Equal("Technical Design", Required(domain.Children, "Name").Value);

        // The predicate array must keep indexed child values.
        Assert.Equal("DECLARE", Required(Required(domain.Children, "PredicateIds").Children, "[0]").Value);

        // The source array must keep indexed child values.
        Assert.Equal("TECHNICAL-DESIGN-PLAN", Required(Required(domain.Children, "SourceIds").Children, "[0]").Value);

        // The revision must remain visible at the entry root.
        Assert.Equal("7", Required(nodes, "Revision").Value);
    }

    [Fact]
    public void PropertyNodeSerialiser_LogicalPhaseEntry_EmitsPhaseDomainIds()
    {
        // Create a phase entry with multiple domain ownership ids.
        var entry = new LogicalPhaseEntry(
            new LogicalPhase(
                "implementation",
                "Implementation",
                "Owns implementation orchestration.",
                ["technical_design", "testing"],
                ["CREATE", "ASSIGN"],
                ["IMPLEMENTATION-PLAN"]),
            11,
            DateTimeOffset.Parse("2026-05-04T20:01:00.0000000+00:00"));

        // Serialise the phase entry through the inspector data layer.
        var nodes = PropertyNodeSerialiser.Serialise(entry);

        // The nested phase branch must be visible to the inspector.
        var phase = Required(nodes, "Phase");

        // The phase id must be inspectable as a leaf value.
        Assert.Equal("implementation", Required(phase.Children, "PhaseId").Value);

        // The first domain id must remain in its indexed array position.
        Assert.Equal("technical_design", Required(Required(phase.Children, "DomainIds").Children, "[0]").Value);

        // The second domain id must remain in its indexed array position.
        Assert.Equal("testing", Required(Required(phase.Children, "DomainIds").Children, "[1]").Value);

        // The revision must remain visible at the entry root.
        Assert.Equal("11", Required(nodes, "Revision").Value);
    }

    [Fact]
    public void PropertyNodeSerialiser_LogicalTransitionEntry_EmitsEndpointAndConstraintFields()
    {
        // Create a transition entry with source, target, predicate, and constraint ids.
        var entry = new LogicalTransitionEntry(
            new LogicalTransition(
                "technical-to-implementation",
                "phase",
                "technical_design",
                "phase",
                "implementation",
                "Carry technical design decisions into implementation.",
                ["DECLARE", "RETURN"],
                ["PRESERVE_CONSTRAINTS"],
                ["TRANSITION-PLAN"]),
            13,
            DateTimeOffset.Parse("2026-05-04T20:02:00.0000000+00:00"));

        // Serialise the transition entry through the inspector data layer.
        var nodes = PropertyNodeSerialiser.Serialise(entry);

        // The nested transition branch must be visible to the inspector.
        var transition = Required(nodes, "Transition");

        // The transition id must be inspectable as a leaf value.
        Assert.Equal("technical-to-implementation", Required(transition.Children, "TransitionId").Value);

        // The source endpoint kind must be inspectable.
        Assert.Equal("phase", Required(transition.Children, "FromTopologyKind").Value);

        // The source endpoint id must be inspectable.
        Assert.Equal("technical_design", Required(transition.Children, "FromTopologyId").Value);

        // The target endpoint id must be inspectable.
        Assert.Equal("implementation", Required(transition.Children, "ToTopologyId").Value);

        // The first constraint id must remain in its indexed array position.
        Assert.Equal("PRESERVE_CONSTRAINTS", Required(Required(transition.Children, "ConstraintIds").Children, "[0]").Value);

        // The revision must remain visible at the entry root.
        Assert.Equal("13", Required(nodes, "Revision").Value);
    }

    [Fact]
    public void StationTrace_StartAndCompletionEvents_CarrySharedTraceIdentity()
    {
        // Create a start trace without output to represent an in-flight station.
        var start = new StationTrace(
            "trace-1",
            "parent-1",
            "run-1",
            2,
            0,
            StationKind.Unit,
            "AddDomainTopologyMutationUnit",
            "topology-domain-mutation",
            [new PropertyNode("Command", null, "AddDomainCommand", [])],
            null,
            null,
            null,
            DateTimeOffset.Parse("2026-05-04T20:03:00.0000000+00:00"),
            null);

        // Create a completion trace by preserving identity and adding output.
        var completed = start with
        {
            Output = [new PropertyNode("Result", "ok", "string", [])],
            CompletedAt = DateTimeOffset.Parse("2026-05-04T20:03:01.0000000+00:00")
        };

        // Start and completion events must share the same trace id for tree upsert.
        Assert.Equal(start.TraceId, completed.TraceId);

        // Start and completion events must share the same parent id for stable tree placement.
        Assert.Equal(start.ParentTraceId, completed.ParentTraceId);

        // The start event must represent an in-flight station.
        Assert.Null(start.Output);

        // The completion event must carry the completed output tree.
        Assert.Equal("ok", completed.Output![0].Value);
    }

    private static PropertyNode Required(IReadOnlyList<PropertyNode> nodes, string name)
    {
        // Find the node by name so each assertion can read the exact property branch it cares about.
        var node = nodes.FirstOrDefault(candidate => candidate.Name == name);

        // Fail clearly if the serializer did not emit the expected property.
        Assert.NotNull(node);

        // Return the found node after the assertion establishes it is present.
        return node!;
    }
}
