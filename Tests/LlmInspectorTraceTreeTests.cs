// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;
using LlmInspector.Inspector;

namespace LlmFromScratch.Tests;

public sealed class LlmInspectorTraceTreeTests
{
    [Fact]
    public async Task StationTraceTree_Phase64Events_BuildsDepthChainAndUpsertsCompletions()
    {
        // Create a session that emits the live Phase 64 trace stream.
        var session = new InspectorSession();

        // Capture trace events in emission order.
        var traces = new List<StationTrace>();

        // Subscribe before command execution so root events are captured.
        session.StationTraced += (_, trace) => traces.Add(trace);

        // Run one valid add-domain topology mutation.
        var result = await session.RunTopologyCommandAsync(CreateDomainCommand());

        // The traced command must succeed before tree assertions are meaningful.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Create the trace tree under test.
        var tree = new StationTraceTree();

        // Feed all start and completion events through the upsert path.
        foreach (var trace in traces)
            tree.Upsert(trace);

        // The tree should have one root orchestrator.
        var root = Assert.Single(tree.Roots);

        // Completion events must update the root instead of adding a duplicate.
        Assert.False(root.IsRunning);

        // The root should have one child wave containing the selected flow.
        var flow = SingleChild(root);

        // The flow should have one child wave containing the mutation unit.
        var unit = SingleChild(flow);

        // The unit should have one child wave containing the storage boundary.
        var storage = SingleChild(unit);

        // The root card should represent the orchestrator station.
        Assert.Equal(StationKind.Orchestrator, root.Kind);

        // The flow card should represent the logical flow station.
        Assert.Equal(StationKind.Flow, flow.Kind);

        // The unit card should represent the logical unit station.
        Assert.Equal(StationKind.Unit, unit.Kind);

        // The storage card should represent the concrete storage station.
        Assert.Equal(StationKind.Storage, storage.Kind);

        // The flow trace should remain attached to the root trace id.
        Assert.Equal(root.Trace.TraceId, flow.Trace.ParentTraceId);

        // The unit trace should remain attached to the flow trace id.
        Assert.Equal(flow.Trace.TraceId, unit.Trace.ParentTraceId);

        // The storage trace should remain attached to the unit trace id.
        Assert.Equal(unit.Trace.TraceId, storage.Trace.ParentTraceId);

        // The storage completion should expose output after upsert.
        Assert.NotNull(storage.Output);
    }

    [Fact]
    public void StationCardViewModel_AddChild_RaisesHasChildrenNotification()
    {
        // Create a root station card without children.
        var root = new StationCardViewModel(CreateTrace("root", null, 0, StationKind.Orchestrator));

        // Capture property notifications emitted by the card.
        var changed = new List<string?>();

        // Subscribe to property change notifications.
        root.PropertyChanged += (_, args) => changed.Add(args.PropertyName);

        // Create a child card in the first wave.
        var child = new StationCardViewModel(CreateTrace("child", "root", 1, StationKind.Flow));

        // Add the child to the root card.
        root.AddChild(child);

        // The computed HasChildren property should now be true.
        Assert.True(root.HasChildren);

        // The card must notify HasChildren so the recursive children panel can become visible.
        Assert.Contains(nameof(StationCardViewModel.HasChildren), changed);
    }

    [Fact]
    public void StationCardViewModel_StationTypeText_MapsRuntimeKindsForInspectorBadges()
    {
        // Create an orchestrator card so the UI can identify top-level selectors.
        var orchestrator = new StationCardViewModel(CreateTrace("orchestrator", null, 0, StationKind.Orchestrator, "logical_orchestrator"));

        // Create a flow card so the UI can identify graph runners.
        var flow = new StationCardViewModel(CreateTrace("flow", "orchestrator", 1, StationKind.Flow, "logical_flow"));

        // Create a unit card with a normal role so the UI can identify node-style units.
        var node = new StationCardViewModel(CreateTrace("node", "flow", 2, StationKind.Unit, "project21_permission_gate"));

        // Create a unit card with an arbiter role so the UI can identify arbiter-style units.
        var arbiter = new StationCardViewModel(CreateTrace("arbiter", "flow", 2, StationKind.Unit, "identity_arbiter_unit"));

        // Orchestrator stations must show the orchestrator badge.
        Assert.Equal("ORCHESTRATOR", orchestrator.StationTypeText);

        // Flow stations must show the flow badge.
        Assert.Equal("FLOW", flow.StationTypeText);

        // Normal unit stations must show the node badge.
        Assert.Equal("NODE", node.StationTypeText);

        // Arbiter-role unit stations must show the arbiter badge.
        Assert.Equal("ARBITER", arbiter.StationTypeText);
    }

    private static AddDomainCommand CreateDomainCommand()
    {
        // Return a valid add-domain command used by the live trace-tree test.
        return new AddDomainCommand(
            new LogicalDomain(
                "technical_design",
                "Technical Design",
                "Owns technical design orchestration.",
                ["DECLARE", "RETURN"],
                ["PHASE-65"]),
            "Phase 65 trace tree integration test.");
    }

    private static StationCardViewModel SingleChild(StationCardViewModel parent)
    {
        // Read the single wave group expected for this sequential command path.
        var wave = Assert.Single(parent.WaveGroups);

        // Read the single child card inside that wave.
        return Assert.Single(wave.Cards);
    }

    private static StationTrace CreateTrace(
        string traceId,
        string? parentTraceId,
        int depth,
        StationKind kind,
        string stationRole = "test-role")
    {
        // Return a minimal start trace for view-model notification tests.
        return new StationTrace(
            traceId,
            parentTraceId,
            "run-1",
            depth,
            0,
            kind,
            kind.ToString(),
            stationRole,
            [],
            null,
            null,
            null,
            DateTimeOffset.UtcNow,
            null);
    }
}
