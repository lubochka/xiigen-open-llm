// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ExperimentsV2;
using ExperimentsV2.LogicalFlow;
using System.Text.Json;

namespace LlmFromScratch.Tests;

public sealed class DynamicFlowT35Tests
{
    [Fact]
    public async Task T35_Run_WritesJsonMarkdownAndMermaidEvidence()
    {
        // Build the experiment through the public writer so artifact paths use the production evidence path.
        var experiment = new T35LogicalUnitReplacementProof();

        // Write evidence into the shared experiment folder.
        var result = await experiment.WriteEvidenceAsync(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "experiments-v2"));

        // Prove the writer succeeded before checking individual files.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Keep JSON as the anchor artifact.
        string json = result.Value!;

        // The Markdown artifact must share the same run id.
        string md = Path.ChangeExtension(json, ".md");

        // The Mermaid artifact must share the same run id.
        string mmd = Path.ChangeExtension(json, ".mmd");

        // The machine-readable artifact must exist.
        Assert.True(File.Exists(json), json);

        // The human-readable artifact must exist.
        Assert.True(File.Exists(md), md);

        // The topology artifact must exist.
        Assert.True(File.Exists(mmd), mmd);
    }

    [Fact]
    public async Task T35_EvidenceJson_UsesRequiredMetricKeys()
    {
        // Write evidence so the JSON serialization path is under test.
        var experiment = new T35LogicalUnitReplacementProof();

        // Place evidence in the shared experiment folder.
        var result = await experiment.WriteEvidenceAsync(Path.Combine(Environment.CurrentDirectory, "..", "..", "..", "..", "experiments-v2"));

        // Stop on writer failure so the JSON assertion reports the real cause.
        Assert.True(result.IsSuccess, result.ErrorMessage);

        // Parse the emitted JSON exactly as artifact reviewers will read it.
        using var document = JsonDocument.Parse(File.ReadAllText(result.Value!));

        // Metrics must be present as a machine gate collection.
        var metrics = document.RootElement.GetProperty("metrics");

        // The replacement-success key must use the canonical spelling.
        Assert.True(metrics.TryGetProperty("logical_unit_replacement_success", out var success), "Missing logical_unit_replacement_success");

        // The replacement proof must report success after all gates pass.
        Assert.True(success.GetBoolean());

        // Confidence must be visible as a machine-readable gate.
        Assert.True(metrics.TryGetProperty("unit_min_confidence", out var confidence), "Missing unit_min_confidence");

        // The unit confidence must meet the T35 threshold.
        Assert.True(confidence.GetDouble() >= 0.70);
    }

    [Fact]
    public async Task T35_LogicalUnitReplacement_PassesAllGates()
    {
        // Build evidence through the replacement proof.
        var evidence = await BuildEvidenceAsync();

        // Every pass gate defined by T35 must pass.
        Assert.All(evidence.PassGates, gate => Assert.True(gate.Passed, gate.Gate));

        // The top-level replacement metric must pass.
        Assert.True(evidence.Metrics.LogicalUnitReplacementSuccess);

        // The T34 oracle transition must still pass.
        Assert.True(evidence.Metrics.T34TransitionSuccess);

        // The T35 replacement transition must pass.
        Assert.True(evidence.Metrics.T35TransitionSuccess);
    }

    [Fact]
    public async Task T35_Node01LogicalUnit_EmitsExactOraclePacketIds()
    {
        // Build evidence so oracle and unit packets can be compared directly.
        var evidence = await BuildEvidenceAsync();

        // The unit must emit the same number of packets as the oracle.
        Assert.Equal(evidence.Metrics.OraclePacketCount, evidence.Metrics.UnitPacketCount);

        // Packet id mismatch count is the strongest edge-preservation gate.
        Assert.Equal(0, evidence.Metrics.PacketIdMismatchCount);

        // Compare the exact packet id sets.
        Assert.Equal(
            evidence.OraclePackets.Select(packet => packet.PacketId).OrderBy(id => id),
            evidence.UnitPackets.Select(packet => packet.PacketId).OrderBy(id => id));
    }

    [Fact]
    public async Task T35_Node01LogicalUnit_PreservesDomainLabelsCoverageAndSourceIds()
    {
        // Build evidence so all packet fields are available.
        var evidence = await BuildEvidenceAsync();

        // Domain labels must match the T34 oracle.
        Assert.Equal(0, evidence.Metrics.DomainLabelMismatchCount);

        // Coverage statuses must match the T34 oracle.
        Assert.Equal(0, evidence.Metrics.CoverageStatusMismatchCount);

        // Source ids must match the T34 oracle.
        Assert.Equal(0, evidence.Metrics.SourceIdMismatchCount);
    }

    [Fact]
    public async Task T35_Node01LogicalUnit_MeetsConfidenceGate()
    {
        // Build evidence so confidence can be checked from the logical-unit envelope.
        var evidence = await BuildEvidenceAsync();

        // T35 requires confidence at or above the correct-domain floor.
        Assert.True(evidence.Metrics.UnitMinConfidence >= 0.70);

        // The deterministic splitter reports a calibrated confidence above the floor.
        Assert.Equal(T34TransitionDomainSplitterUnit.Confidence, evidence.Metrics.UnitMinConfidence);
    }

    [Fact]
    public async Task T35_DefaultFlags_AreAppliedToLogicalUnitRun()
    {
        // Build evidence so the unit flags are recorded in the artifact.
        var evidence = await BuildEvidenceAsync();

        // The compiled predicate representation flag must be on for the unit.
        Assert.True(evidence.Flags.UseCompiledPredicateRepresentation);

        // Context carry-forward must be on because the unit consumes a phase context packet.
        Assert.True(evidence.Flags.UseContextCarryForward);

        // Oracle comparison must be on because this is a replacement proof.
        Assert.True(evidence.Flags.CompareAgainstOracle);

        // Metric drift must fail the proof by default.
        Assert.True(evidence.Flags.FailOnMetricDrift);
    }

    [Fact]
    public async Task T35_HumanFinding_NamesReplacementAndNextSafeTarget()
    {
        // Build evidence so the human finding can be checked against the review contract.
        var evidence = await BuildEvidenceAsync();

        // The finding must name the replaced unit.
        Assert.Contains("node-01 logical unit matched the T34 oracle", evidence.HumanFinding);

        // The finding must explicitly state that downstream metrics did not drift.
        Assert.Contains("no downstream transition metrics drifted", evidence.HumanFinding);

        // The finding must name the next replacement candidates and keep them in a separate phase.
        Assert.Contains("node-02 or node-03 in a separate phase", evidence.HumanFinding);
    }

    private static async Task<T35LogicalUnitReplacementProof.Evidence> BuildEvidenceAsync()
    {
        // Use the public evidence builder so tests share the same path as evidence writers.
        var experiment = new T35LogicalUnitReplacementProof();

        // Build the evidence without file I/O for most tests.
        var evidence = await experiment.BuildEvidenceAsync();

        // Fail immediately with the experiment diagnostic if the evidence could not be built.
        Assert.True(evidence.IsSuccess, evidence.ErrorMessage);

        // The core replacement gate must pass before relationship checks run.
        Assert.True(evidence.Value!.Metrics.LogicalUnitReplacementSuccess);

        // Return the evidence payload for the calling test.
        return evidence.Value;
    }
}
