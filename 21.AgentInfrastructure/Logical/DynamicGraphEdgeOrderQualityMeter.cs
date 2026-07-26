// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Projects validated candidate-order permutation and exact exclusion evidence.
/// This meter does not infer novelty, ablation, source-id selection, training,
/// promotion, lifecycle state, or any alternate quality-gate verdict.
/// </summary>
public static class DynamicGraphEdgeOrderQualityMeter
{
    private const string InvalidMeasurementCode = "INVALID_EDGE_ORDER_QUALITY_MEASUREMENT";
    private const string InvalidMeasurementMessage = "Dynamic graph edge order quality measurement is invalid.";

    /// <summary>
    /// Measures order/exclusion evidence already attached to one edge-authority execution result.
    /// </summary>
    public static OperationResult<DynamicGraphEdgeOrderQualityMeasurement> Measure(
        EdgeAuthorityExecutionResult? result)
    {
        if (result is null)
            return InvalidMeasurement();

        EdgeCandidate[] proposedCandidates;
        EdgeAuthorityInput normalizedInputSource;
        EdgeAuthorityDecision decisionSource;
        string reportEdgeId;
        string resolvedAuthorityUnitId;
        bool isOrderInvariant;
        DynamicGraphEdgeCandidateOrderPermutationProbe[] probes;
        string[] excludedLeakageSignals;

        try
        {
            proposedCandidates = result.ProposedCandidates.ToArray();
            normalizedInputSource = result.NormalizedInput;
            decisionSource = result.Decision;
            var report = result.CandidateOrderLeakageReport;
            reportEdgeId = report.EdgeId;
            resolvedAuthorityUnitId = report.ResolvedAuthorityUnitId;
            isOrderInvariant = report.IsOrderInvariant;
            probes = report.Probes.ToArray();
            excludedLeakageSignals = report.ExcludedLeakageSignals.ToArray();
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidMeasurement();
        }

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
            || !isOrderInvariant
            || !DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(excludedLeakageSignals)
            || probes.Length != 5)
        {
            return InvalidMeasurement();
        }

        var probeIds = new string[probes.Length];
        for (var probeIndex = 0; probeIndex < probes.Length; probeIndex++)
        {
            var probe = probes[probeIndex];
            var expectedProbeId = DynamicGraphEdgeCandidateOrderLeakageGuardUnit.CreateProbeId(probeIndex);
            var expectedCandidates = DynamicGraphEdgeCandidateOrderLeakageGuardUnit.CreateExpectedPermutation(
                normalizedInput.Candidates,
                probeIndex);
            if (probe is null
                || !StringComparer.Ordinal.Equals(probe.ProbeId, expectedProbeId)
                || !CandidateOrdersEqual(probe.CandidateOrder, expectedCandidates)
                || !DynamicGraphEdgeCandidateOrderLeakageGuardUnit.DecisionsEqual(
                    canonicalDecision,
                    probe.Decision))
            {
                return InvalidMeasurement();
            }

            probeIds[probeIndex] = expectedProbeId;
        }

        return OperationResult<DynamicGraphEdgeOrderQualityMeasurement>.Ok(
            new DynamicGraphEdgeOrderQualityMeasurement(
                normalizedInput.EdgeId,
                resolvedAuthorityUnitId,
                probeIds.Length,
                isOrderInvariant,
                probeIds,
                excludedLeakageSignals));
    }

    private static bool CandidateOrdersEqual(
        IReadOnlyList<EdgeCandidate>? actual,
        IReadOnlyList<EdgeCandidate> expected)
    {
        if (actual is null || actual.Count != expected.Count)
            return false;

        for (var index = 0; index < expected.Count; index++)
        {
            var actualCandidate = actual[index];
            var expectedCandidate = expected[index];
            if (actualCandidate is null
                || !StringComparer.Ordinal.Equals(
                    actualCandidate.TargetSubPhaseId,
                    expectedCandidate.TargetSubPhaseId)
                || !double.IsFinite(actualCandidate.BaselineScore)
                || !actualCandidate.BaselineScore.Equals(expectedCandidate.BaselineScore)
                || !StringComparer.Ordinal.Equals(
                    actualCandidate.AuthorityRole,
                    expectedCandidate.AuthorityRole)
                || actualCandidate.EvidencePredicates is null
                || !actualCandidate.EvidencePredicates.SequenceEqual(
                    expectedCandidate.EvidencePredicates,
                    StringComparer.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsCanonicalValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value.All(character => !char.IsControl(character))
           && value.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) < 0;

    private static OperationResult<DynamicGraphEdgeOrderQualityMeasurement> InvalidMeasurement()
        => OperationResult<DynamicGraphEdgeOrderQualityMeasurement>.Fail(
            InvalidMeasurementMessage,
            InvalidMeasurementCode);
}
