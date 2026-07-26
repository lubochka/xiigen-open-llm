// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using AgentInfrastructure.Logical;
using LlmInspector.Inspector;

namespace LlmFromScratch.Tests;

public sealed class LlmInspectorParallelWaveTests
{
    [Fact]
    public async Task InspectorSession_ParallelDomainCommands_RenderAsSameWaveSiblings()
    {
        // Create the traced inspector session.
        var session = new InspectorSession();

        // Capture all trace events emitted during the run.
        var traces = new List<StationTrace>();

        // Subscribe before execution so root and flow starts are captured.
        session.StationTraced += (_, trace) => traces.Add(trace);

        // Run two add-domain commands through one parallel flow wave.
        var result = await session.RunParallelDomainCommandsAsync(
            [CreateCommand("technical_design", "Technical Design"), CreateCommand("implementation", "Implementation")]);

        // The parallel topology mutation flow must succeed.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Extract unit start traces to prove the same parent and same wave.
        var unitStarts = traces
            .Where(trace => trace.CompletedAt is null && trace.Kind == StationKind.Unit)
            .ToArray();

        // The run should emit two unit start traces.
        Assert.Equal(2, unitStarts.Length);

        // Both unit starts should have the same flow parent trace id.
        Assert.Single(unitStarts.Select(trace => trace.ParentTraceId).Distinct(StringComparer.Ordinal));

        // Both unit starts should belong to the same wave for side-by-side rendering.
        Assert.All(unitStarts, trace => Assert.Equal(0, trace.WaveIndex));

        // Build the recursive trace tree from the raw start and completion events.
        var tree = new StationTraceTree();

        // Apply every event through the upsert path.
        foreach (var trace in traces)
            tree.Upsert(trace);

        // Read the root orchestrator card.
        var root = Assert.Single(tree.Roots);

        // Read the single flow child under the root.
        var flow = Assert.Single(Assert.Single(root.WaveGroups).Cards);

        // Read the unit wave under the flow.
        var unitWave = Assert.Single(flow.WaveGroups);

        // The unit wave should contain both unit siblings side by side.
        Assert.Equal(2, unitWave.Cards.Count);

        // The wave index should be zero for both parallel unit cards.
        Assert.Equal(0, unitWave.WaveIndex);

        // Every unit sibling should have its own storage child.
        Assert.All(unitWave.Cards, unit => Assert.Single(Assert.Single(unit.WaveGroups).Cards));
    }

    private static AddDomainCommand CreateCommand(string domainId, string name)
    {
        // Return one valid add-domain command for the parallel wave test.
        return new AddDomainCommand(
            new LogicalDomain(
                domainId,
                name,
                $"Owns {name} orchestration.",
                ["DECLARE"],
                ["PHASE-67"]),
            "Phase 67 parallel wave test.");
    }
}
