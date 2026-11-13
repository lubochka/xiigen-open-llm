// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Projects validated identity-control and exact leakage-exclusion evidence already attached to an execution result.
/// This meter does not inspect candidate-order evidence or infer aggregate quality, trained state, promotion,
/// lifecycle, or training state.
/// </summary>
public static class DynamicGraphEdgeIdentityQualityMeter
{
    private const string InvalidMeasurementCode = "INVALID_EDGE_IDENTITY_QUALITY_MEASUREMENT";
    private const string InvalidMeasurementMessage = "Dynamic graph edge identity quality measurement is invalid.";

    /// <summary>
    /// Measures identity/exclusion evidence without rerunning a guard or invoking an edge authority dependency.
    /// </summary>
    public static OperationResult<DynamicGraphEdgeIdentityQualityMeasurement> Measure(
        EdgeAuthorityExecutionResult? result)
    {
        if (result is null)
            return InvalidMeasurement();

        try
        {
            var proposedCandidates = result.ProposedCandidates.ToArray();
            var normalizedInputSource = result.NormalizedInput;
            var decisionSource = result.Decision;
            var report = result.IdentityLeakageReport;
            var reportEdgeId = report.EdgeId;
            var resolvedAuthorityUnitId = report.ResolvedAuthorityUnitId;
            var isIdentityInvariant = report.IsIdentityInvariant;
            var probes = report.Probes.ToArray();
            var excludedLeakageSignals = report.ExcludedLeakageSignals.ToArray();

            if (!DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotNormalizedInput(
                    normalizedInputSource,
                    reportEdgeId,
                    proposedCandidates,
                    out var normalizedInput))
            {
                return InvalidMeasurement();
            }

            var normalizedTargetIds = normalizedInput.Candidates
                .Select(candidate => candidate.TargetSubPhaseId)
                .ToArray();
            if (!DynamicGraphEdgeAuthorityExecutionPipeline.TrySnapshotDecision(
                    decisionSource,
                    normalizedTargetIds,
                    normalizedInput.Candidates,
                    out var canonicalDecision)
                || !IsCanonicalValue(resolvedAuthorityUnitId)
                || !StringComparer.Ordinal.Equals(reportEdgeId, normalizedInput.EdgeId)
                || !isIdentityInvariant
                || !DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(excludedLeakageSignals)
                || probes.Length != 3)
            {
                return InvalidMeasurement();
            }

            var probeIds = new string[probes.Length];
            var controlledSignals = new string[probes.Length];
            var controlValues = new string[probes.Length];
            for (var probeIndex = 0; probeIndex < probes.Length; probeIndex++)
            {
                var probe = probes[probeIndex];
                var expectedProbeId = DynamicGraphEdgeIdentityLeakageGuardUnit.CreateProbeId(probeIndex);
                var expectedControlledSignal = DynamicGraphEdgeIdentityLeakageGuardUnit
                    .CreateControlledSignal(probeIndex);
                var expectedControlValue = DynamicGraphEdgeIdentityLeakageGuardUnit.CreateControlValue(probeIndex);
                if (probe is null
                    || !StringComparer.Ordinal.Equals(probe.ProbeId, expectedProbeId)
                    || !StringComparer.Ordinal.Equals(probe.ControlledSignal, expectedControlledSignal)
                    || !StringComparer.Ordinal.Equals(probe.ControlValue, expectedControlValue)
                    || !DynamicGraphEdgeCandidateOrderLeakageGuardUnit.DecisionsEqual(
                        canonicalDecision,
                        probe.Decision))
                {
                    return InvalidMeasurement();
                }

                probeIds[probeIndex] = expectedProbeId;
                controlledSignals[probeIndex] = expectedControlledSignal;
                controlValues[probeIndex] = expectedControlValue;
            }

            return OperationResult<DynamicGraphEdgeIdentityQualityMeasurement>.Ok(
                new DynamicGraphEdgeIdentityQualityMeasurement(
                    normalizedInput.EdgeId,
                    resolvedAuthorityUnitId,
                    probeIds.Length,
                    isIdentityInvariant,
                    probeIds,
                    controlledSignals,
                    controlValues,
                    excludedLeakageSignals));
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidMeasurement();
        }
    }

    private static bool IsCanonicalValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value.All(character => !char.IsControl(character))
           && value.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) < 0;

    private static OperationResult<DynamicGraphEdgeIdentityQualityMeasurement> InvalidMeasurement()
        => OperationResult<DynamicGraphEdgeIdentityQualityMeasurement>.Fail(
            InvalidMeasurementMessage,
            InvalidMeasurementCode);
}
