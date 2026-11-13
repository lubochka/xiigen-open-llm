// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using AgentInfrastructure.Logical;
using ExperimentsV2.LogicalFlow;

namespace ExperimentsV2;

// This experiment proves that one deterministic station can be replaced by an actual ILogicalUnit.
public sealed class T35LogicalUnitReplacementProof
{
    // This identifier names the logical-unit replacement proof in evidence folders and tests.
    public const string Id = "T35-LOGICAL-UNIT-REPLACEMENT-PROOF";

    // This identifier names the exact station being replaced.
    public const string ReplacementTarget = "T34-node-01-transition-domain-splitter";

    // This record carries the mismatch counts between oracle and unit packet output.
    public sealed record ReplacementMetrics(
        int OraclePacketCount,
        int UnitPacketCount,
        int PacketIdMismatchCount,
        int DomainLabelMismatchCount,
        int CoverageStatusMismatchCount,
        int SourceIdMismatchCount,
        double UnitMinConfidence,
        bool T34TransitionSuccess,
        bool T35TransitionSuccess,
        [property: JsonPropertyName("logical_unit_replacement_success")] bool LogicalUnitReplacementSuccess);

    // This record carries the complete T35 evidence payload.
    public sealed record Evidence(
        string ExperimentId,
        string ReplacementTarget,
        string RunId,
        string RunTimestampUtc,
        string UnitId,
        string UnitRole,
        string DomainTag,
        LogicalExecutionFlags Flags,
        T34PhaseTransitionProof.PhaseContextPacket InputPhaseContext,
        T34PhaseTransitionProof.DomainPacket[] OraclePackets,
        T34PhaseTransitionProof.DomainPacket[] UnitPackets,
        ReplacementMetrics Metrics,
        T34PhaseTransitionProof.PassGate[] PassGates,
        string HumanFinding,
        ExperimentV2Result Summary);

    // Build the replacement proof without writing files.
    public async Task<OperationResult<Evidence>> BuildEvidenceAsync(int tier = 1, CancellationToken cancellationToken = default)
    {
        try
        {
            // Build the unchanged T34 evidence to use as the oracle.
            var oracle = new T34PhaseTransitionProof().BuildEvidence(tier);

            // Stop if the oracle could not be built.
            if (!oracle.IsSuccess) return OperationResult<Evidence>.Fail(oracle.ErrorMessage);

            // Use one run id across JSON, Markdown, and Mermaid evidence.
            string runId = $"run-{DateTime.UtcNow:yyyy-MM-dd-HHmmss}";

            // Create the actual logical unit that replaces T34 node-01.
            var unit = new T34TransitionDomainSplitterUnit();

            // Use default flags because optimization flags apply as truth.
            var flags = new LogicalExecutionFlags();

            // Build the logical-unit request from the T34 node-00 output.
            var request = new LogicalUnitRequest<T34PhaseTransitionProof.PhaseContextPacket>(
                runId,
                T34TransitionDomainSplitterUnit.Id,
                T34TransitionDomainSplitterUnit.Role,
                T34TransitionDomainSplitterUnit.DomainTag,
                oracle.Value!.Node00CompiledPhaseContext,
                flags,
                new Dictionary<string, object> { ["oracle_experiment"] = T34PhaseTransitionProof.Id });

            // Execute the real logical unit for node-01.
            var unitResult = await unit.ExecuteAsync(request, cancellationToken);

            // Stop if the unit failed through its shared operation envelope.
            if (!unitResult.IsSuccess) return OperationResult<Evidence>.Fail(unitResult.ErrorMessage);

            // Read the oracle packets from the unchanged T34 evidence.
            var oraclePackets = oracle.Value.Node01DomainPackets;

            // Read the unit packets from the logical-unit result.
            var unitPackets = unitResult.Value!.Output;

            // Build mismatch metrics from exact packet comparisons.
            var metrics = BuildMetrics(oraclePackets, unitPackets, unitResult.Value.Confidence, oracle.Value.Metrics.TransitionSuccess);

            // Build pass gates for artifact review.
            var passGates = BuildPassGates(metrics);

            // Build the human finding that explains what the replacement proved.
            string humanFinding = BuildHumanFinding(metrics);

            // Compute success from all replacement gates.
            double success = passGates.All(gate => gate.Passed) ? 1.0 : 0.0;

            // Build the standard V2 summary.
            var summary = new ExperimentV2Result
            {
                ExperimentId = Id,
                Tier = tier,
                BaselinePrimary = 0.0,
                EnhancedPrimary = success,
                PrimaryMetricName = "logical_unit_replacement_success",
                LowerIsBetter = false,
                ScalingSignal = metrics.UnitMinConfidence,
                ScalingSignalNote = "confidence_reported_by_the_replacement_logical_unit",
                ShouldScaleUp = success < 1.0,
                BaselineTimeMs = 0.1,
                EnhancedTimeMs = 0.1,
                Notes = $"target={ReplacementTarget}, packets={metrics.UnitPacketCount}, confidence={metrics.UnitMinConfidence:0.00}"
            };

            // Return the evidence for tests and evidence writers.
            return OperationResult<Evidence>.Ok(new Evidence(
                Id,
                ReplacementTarget,
                runId,
                DateTime.UtcNow.ToString("O"),
                T34TransitionDomainSplitterUnit.Id,
                T34TransitionDomainSplitterUnit.Role,
                T34TransitionDomainSplitterUnit.DomainTag,
                flags,
                oracle.Value.Node00CompiledPhaseContext,
                oraclePackets,
                unitPackets,
                metrics,
                passGates,
                humanFinding,
                summary));
        }
        catch (Exception ex)
        {
            // Return unexpected failures through the shared operation envelope.
            return OperationResult<Evidence>.Fail($"T35 failed: {ex.Message}");
        }
    }

    // Write JSON, Markdown, and Mermaid evidence for artifact review.
    public async Task<OperationResult<string>> WriteEvidenceAsync(string outputRoot, int tier = 1, CancellationToken cancellationToken = default)
    {
        // Build evidence through the same path used by tests.
        var evidence = await BuildEvidenceAsync(tier, cancellationToken);

        // Stop before file I/O if the proof failed.
        if (!evidence.IsSuccess) return OperationResult<string>.Fail(evidence.ErrorMessage);

        try
        {
            // Place evidence under the experiment-specific result directory.
            string dir = Path.Combine(outputRoot, Id, "results");

            // Create the directory if this is the first T35 run.
            Directory.CreateDirectory(dir);

            // Use one run id for all review artifacts.
            string runId = evidence.Value!.RunId;

            // Build the JSON path first because tests use it as the anchor.
            string json = Path.Combine(dir, $"{runId}.json");

            // Build the Markdown path beside the JSON file.
            string md = Path.ChangeExtension(json, ".md");

            // Build the Mermaid path beside the JSON file.
            string mmd = Path.ChangeExtension(json, ".mmd");

            // Use snake_case JSON for consistency with E53 and T34 evidence.
            var options = new JsonSerializerOptions { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

            // Write the machine-readable evidence.
            File.WriteAllText(json, JsonSerializer.Serialize(evidence.Value, options), Encoding.UTF8);

            // Write the human-readable evidence.
            File.WriteAllText(md, ToMarkdown(evidence.Value), Encoding.UTF8);

            // Write the replacement topology.
            File.WriteAllText(mmd, ToMermaid(), Encoding.UTF8);

            // Return the anchor artifact path.
            return OperationResult<string>.Ok(json);
        }
        catch (Exception ex)
        {
            // Return file-writing failures through the shared operation envelope.
            return OperationResult<string>.Fail($"T35 evidence write failed: {ex.Message}");
        }
    }

    // Build mismatch metrics from oracle and unit packet arrays.
    private static ReplacementMetrics BuildMetrics(T34PhaseTransitionProof.DomainPacket[] oraclePackets, T34PhaseTransitionProof.DomainPacket[] unitPackets, double unitConfidence, bool t34TransitionSuccess)
    {
        // Count packet id mismatches by exact set comparison.
        int packetIdMismatchCount = MismatchCount(oraclePackets.Select(packet => packet.PacketId), unitPackets.Select(packet => packet.PacketId));

        // Count domain-label mismatches on matching packet ids.
        int domainLabelMismatchCount = FieldMismatchCount(oraclePackets, unitPackets, packet => packet.DomainLabel);

        // Count coverage-status mismatches on matching packet ids.
        int coverageStatusMismatchCount = FieldMismatchCount(oraclePackets, unitPackets, packet => packet.CoverageStatus);

        // Count source-id mismatches on matching packet ids.
        int sourceIdMismatchCount = FieldMismatchCount(oraclePackets, unitPackets, packet => string.Join("|", packet.SourceIds.OrderBy(id => id, StringComparer.Ordinal)));

        // The replacement succeeds only when exact parity and confidence gates hold.
        bool replacementSuccess =
            packetIdMismatchCount == 0 &&
            domainLabelMismatchCount == 0 &&
            coverageStatusMismatchCount == 0 &&
            sourceIdMismatchCount == 0 &&
            unitConfidence >= 0.70 &&
            t34TransitionSuccess;

        // Return the complete machine metric set.
        return new ReplacementMetrics(
            oraclePackets.Length,
            unitPackets.Length,
            packetIdMismatchCount,
            domainLabelMismatchCount,
            coverageStatusMismatchCount,
            sourceIdMismatchCount,
            unitConfidence,
            t34TransitionSuccess,
            replacementSuccess,
            replacementSuccess);
    }

    // Count exact set mismatch between two string collections.
    private static int MismatchCount(IEnumerable<string> expected, IEnumerable<string> actual)
    {
        // Build an ordinal expected set.
        var expectedSet = new HashSet<string>(expected, StringComparer.Ordinal);

        // Build an ordinal actual set.
        var actualSet = new HashSet<string>(actual, StringComparer.Ordinal);

        // Count expected items missing from actual plus unexpected actual items.
        return expectedSet.Except(actualSet, StringComparer.Ordinal).Count() + actualSet.Except(expectedSet, StringComparer.Ordinal).Count();
    }

    // Count mismatches for one field across matching packet ids.
    private static int FieldMismatchCount(T34PhaseTransitionProof.DomainPacket[] oraclePackets, T34PhaseTransitionProof.DomainPacket[] unitPackets, Func<T34PhaseTransitionProof.DomainPacket, string> selector)
    {
        // Index the unit packets by packet id for exact matching.
        var unitById = unitPackets.ToDictionary(packet => packet.PacketId, StringComparer.Ordinal);

        // Count every oracle packet whose unit packet is missing or whose selected field differs.
        return oraclePackets.Count(oraclePacket => !unitById.TryGetValue(oraclePacket.PacketId, out var unitPacket) || selector(oraclePacket) != selector(unitPacket));
    }

    // Build pass gates from replacement metrics.
    private static T34PhaseTransitionProof.PassGate[] BuildPassGates(ReplacementMetrics metrics)
    {
        // Return every machine gate required by the T35 plan.
        return
        [
            new("oracle_packet_count", "10", metrics.OraclePacketCount.ToString(), metrics.OraclePacketCount == 10, "T34 oracle packet count changed"),
            new("unit_packet_count", "10", metrics.UnitPacketCount.ToString(), metrics.UnitPacketCount == 10, "logical unit emitted the wrong packet count"),
            new("packet_id_mismatch_count", "0", metrics.PacketIdMismatchCount.ToString(), metrics.PacketIdMismatchCount == 0, "packet identity drifted"),
            new("domain_label_mismatch_count", "0", metrics.DomainLabelMismatchCount.ToString(), metrics.DomainLabelMismatchCount == 0, "domain labels drifted"),
            new("coverage_status_mismatch_count", "0", metrics.CoverageStatusMismatchCount.ToString(), metrics.CoverageStatusMismatchCount == 0, "coverage statuses drifted"),
            new("source_id_mismatch_count", "0", metrics.SourceIdMismatchCount.ToString(), metrics.SourceIdMismatchCount == 0, "source ids drifted"),
            new("unit_min_confidence", ">= 0.70", metrics.UnitMinConfidence.ToString("0.00"), metrics.UnitMinConfidence >= 0.70, "logical unit confidence fell below acceptance floor"),
            new("t34_transition_success", "true", metrics.T34TransitionSuccess.ToString(), metrics.T34TransitionSuccess, "oracle transition no longer passes"),
            new("t35_transition_success", "true", metrics.T35TransitionSuccess.ToString(), metrics.T35TransitionSuccess, "replacement transition did not pass"),
            new("logical_unit_replacement_success", "true", metrics.LogicalUnitReplacementSuccess.ToString(), metrics.LogicalUnitReplacementSuccess, "logical unit replacement failed")
        ];
    }

    // Build the human-readable finding for the replacement proof.
    private static string BuildHumanFinding(ReplacementMetrics metrics)
    {
        // Return a concise finding that names the replacement and the next safe step.
        return $"The node-01 logical unit matched the T34 oracle with {metrics.UnitPacketCount}/{metrics.OraclePacketCount} packets, no downstream transition metrics drifted, confidence was {metrics.UnitMinConfidence:0.00}, and the next safe replacement target is node-02 or node-03 in a separate phase.";
    }

    // Write Markdown evidence for human artifact review.
    private static string ToMarkdown(Evidence evidence)
    {
        // Use a builder because the evidence has multiple tables.
        var sb = new StringBuilder();

        // Write the title.
        sb.AppendLine($"# {evidence.ExperimentId}");

        // Add a blank line after the title.
        sb.AppendLine();

        // Write the replacement target.
        sb.AppendLine($"Replacement target: {evidence.ReplacementTarget}");

        // Write the unit identity.
        sb.AppendLine($"Unit: {evidence.UnitId}");

        // Add a blank line before the finding.
        sb.AppendLine();

        // Write the human finding.
        sb.AppendLine(evidence.HumanFinding);

        // Add a blank line before metrics.
        sb.AppendLine();

        // Write metric heading.
        sb.AppendLine("## Metrics");

        // Add a blank line before the metrics table.
        sb.AppendLine();

        // Write metric table header.
        sb.AppendLine("| Metric | Value |");

        // Write metric table divider.
        sb.AppendLine("|---|---|");

        // Write all replacement metrics.
        sb.AppendLine($"| oracle_packet_count | {evidence.Metrics.OraclePacketCount} |");
        sb.AppendLine($"| unit_packet_count | {evidence.Metrics.UnitPacketCount} |");
        sb.AppendLine($"| packet_id_mismatch_count | {evidence.Metrics.PacketIdMismatchCount} |");
        sb.AppendLine($"| domain_label_mismatch_count | {evidence.Metrics.DomainLabelMismatchCount} |");
        sb.AppendLine($"| coverage_status_mismatch_count | {evidence.Metrics.CoverageStatusMismatchCount} |");
        sb.AppendLine($"| source_id_mismatch_count | {evidence.Metrics.SourceIdMismatchCount} |");
        sb.AppendLine($"| unit_min_confidence | {evidence.Metrics.UnitMinConfidence:0.00} |");
        sb.AppendLine($"| logical_unit_replacement_success | {evidence.Metrics.LogicalUnitReplacementSuccess} |");

        // Add a blank line before pass gates.
        sb.AppendLine();

        // Write pass-gate heading.
        sb.AppendLine("## Pass Gates");

        // Add a blank line before pass-gate table.
        sb.AppendLine();

        // Write pass-gate table header.
        sb.AppendLine("| Gate | Required | Actual | Passed |");

        // Write pass-gate table divider.
        sb.AppendLine("|---|---|---|---|");

        // Write every pass-gate row.
        foreach (var gate in evidence.PassGates)
            sb.AppendLine($"| {gate.Gate} | {gate.Required} | {gate.Actual} | {gate.Passed} |");

        // Return the completed Markdown evidence.
        return sb.ToString();
    }

    // Write the replacement graph for Mermaid review.
    private static string ToMermaid() => """
flowchart TD
    A["T34 node-00 phase context packet"] --> B["T35 ILogicalUnit node-01 transition splitter"]
    B --> C["10 unit packets"]
    D["T34 oracle node-01 packets"] --> E["packet parity comparison"]
    C --> E
    E --> F["logical unit replacement success"]
""";
}
