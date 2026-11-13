// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Carries structural authority, order-permutation, and identity-control evidence for one edge.
/// This carrier does not claim aggregate quality, accuracy, novelty, numeric ablation, trained state,
/// lifecycle, DPO, promotion, persistence, or graph mutation.
/// </summary>
public sealed record DynamicGraphEdgeAuthorityControlMeasurement
{
    private readonly DynamicGraphEdgeAuthorityEvidence _authorityEvidence;
    private readonly DynamicGraphEdgeOrderQualityMeasurement _edgeOrderQuality;
    private readonly DynamicGraphEdgeIdentityQualityMeasurement _edgeIdentityQuality;

    public DynamicGraphEdgeAuthorityControlMeasurement(
        DynamicGraphEdgeAuthorityEvidence AuthorityEvidence,
        DynamicGraphEdgeOrderQualityMeasurement EdgeOrderQuality,
        DynamicGraphEdgeIdentityQualityMeasurement EdgeIdentityQuality)
    {
        ArgumentNullException.ThrowIfNull(AuthorityEvidence);
        ArgumentNullException.ThrowIfNull(EdgeOrderQuality);
        ArgumentNullException.ThrowIfNull(EdgeIdentityQuality);

        _authorityEvidence = DynamicGraphEdgeAuthorityControlSnapshots.CopyAuthorityEvidence(
            AuthorityEvidence);
        _edgeOrderQuality = DynamicGraphEdgeAuthorityControlSnapshots.CopyOrderQuality(
            EdgeOrderQuality);
        _edgeIdentityQuality = DynamicGraphEdgeAuthorityControlSnapshots.CopyIdentityQuality(
            EdgeIdentityQuality);
    }

    public DynamicGraphEdgeAuthorityEvidence AuthorityEvidence
        => DynamicGraphEdgeAuthorityControlSnapshots.CopyAuthorityEvidence(_authorityEvidence);

    public DynamicGraphEdgeOrderQualityMeasurement EdgeOrderQuality
        => DynamicGraphEdgeAuthorityControlSnapshots.CopyOrderQuality(_edgeOrderQuality);

    public DynamicGraphEdgeIdentityQualityMeasurement EdgeIdentityQuality
        => DynamicGraphEdgeAuthorityControlSnapshots.CopyIdentityQuality(_edgeIdentityQuality);
}

/// <summary>
/// Associates one admitted L3 logical-flow target with owned per-edge authority-control evidence.
/// This association is not a full graph-quality, lifecycle, training, DPO, or promotion decision.
/// </summary>
public sealed record DynamicGraphTargetEdgeAuthorityControlMeasurement
{
    private readonly DynamicGraphMeasurementTarget _target;
    private readonly DynamicGraphEdgeAuthorityControlMeasurement _edgeAuthorityControl;

    public DynamicGraphTargetEdgeAuthorityControlMeasurement(
        DynamicGraphMeasurementTarget Target,
        DynamicGraphEdgeAuthorityControlMeasurement EdgeAuthorityControl)
    {
        ArgumentNullException.ThrowIfNull(Target);
        ArgumentNullException.ThrowIfNull(EdgeAuthorityControl);

        _target = DynamicGraphEdgeAuthorityControlSnapshots.CopyTarget(Target);
        _edgeAuthorityControl = DynamicGraphEdgeAuthorityControlSnapshots.CopyMeasurement(
            EdgeAuthorityControl);
    }

    public DynamicGraphMeasurementTarget Target
        => DynamicGraphEdgeAuthorityControlSnapshots.CopyTarget(_target);

    public DynamicGraphEdgeAuthorityControlMeasurement EdgeAuthorityControl
        => DynamicGraphEdgeAuthorityControlSnapshots.CopyMeasurement(_edgeAuthorityControl);
}

/// <summary>
/// Composes already-validated structural, order-permutation, and identity-control evidence.
/// Baseline and ablation references are carried as structural provenance only; this meter does not
/// execute or infer an ablation, aggregate a graph, or make a training or lifecycle decision.
/// </summary>
public static class DynamicGraphEdgeAuthorityControlMeter
{
    private const string InvalidMeasurementCode = "INVALID_EDGE_AUTHORITY_CONTROL_MEASUREMENT";
    private const string InvalidMeasurementMessage =
        "Dynamic graph edge authority control measurement is invalid.";

    /// <summary>
    /// Measures one execution result without rerunning authority, order, or identity guards.
    /// </summary>
    public static OperationResult<DynamicGraphEdgeAuthorityControlMeasurement> Measure(
        EdgeAuthorityExecutionResult? result)
    {
        if (result is null)
            return InvalidMeasurement();

        DynamicGraphEdgeAuthorityEvidence authorityEvidence;
        try
        {
            var source = result.AuthorityEvidence;
            var excludedLeakageSignals = source.ExcludedLeakageSignals.ToArray();
            if (!IsCanonicalValue(source.EdgeId)
                || !IsCanonicalValue(source.AuthorityUnitId)
                || !StringComparer.Ordinal.Equals(source.AuthorityUnitLevel, "L2")
                || source.DeterministicFinalAuthority
                || !IsCanonicalValue(source.BaselineUnitId)
                || !IsCanonicalValue(source.BaselineRole)
                || !DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(excludedLeakageSignals)
                || !IsCanonicalValue(source.SourceProof)
                || !IsCanonicalValue(source.CutoverProof)
                || !IsCanonicalValue(source.AblationProofRef))
            {
                return InvalidMeasurement();
            }

            authorityEvidence = new DynamicGraphEdgeAuthorityEvidence(
                source.EdgeId,
                source.AuthorityUnitId,
                source.AuthorityUnitLevel,
                source.DeterministicFinalAuthority,
                source.BaselineUnitId,
                source.BaselineRole,
                excludedLeakageSignals,
                source.SourceProof,
                source.CutoverProof,
                source.AblationProofRef);
        }
        catch (Exception exception) when (SnapshotExceptionGraphClassifier.IsNonFatal(exception))
        {
            return InvalidMeasurement();
        }

        var edgeOrderQuality = DynamicGraphEdgeOrderQualityMeter.Measure(result);
        if (!edgeOrderQuality.IsSuccess)
            return edgeOrderQuality.MapError<DynamicGraphEdgeAuthorityControlMeasurement>();

        var edgeIdentityQuality = DynamicGraphEdgeIdentityQualityMeter.Measure(result);
        if (!edgeIdentityQuality.IsSuccess)
            return edgeIdentityQuality.MapError<DynamicGraphEdgeAuthorityControlMeasurement>();

        var orderQuality = edgeOrderQuality.Value!;
        var identityQuality = edgeIdentityQuality.Value!;
        if (!StringComparer.Ordinal.Equals(authorityEvidence.EdgeId, orderQuality.EdgeId)
            || !StringComparer.Ordinal.Equals(authorityEvidence.EdgeId, identityQuality.EdgeId)
            || !StringComparer.Ordinal.Equals(orderQuality.EdgeId, identityQuality.EdgeId)
            || !StringComparer.Ordinal.Equals(
                authorityEvidence.AuthorityUnitId,
                orderQuality.ResolvedAuthorityUnitId)
            || !StringComparer.Ordinal.Equals(
                authorityEvidence.AuthorityUnitId,
                identityQuality.ResolvedAuthorityUnitId)
            || !DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(
                authorityEvidence.ExcludedLeakageSignals)
            || !DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(
                orderQuality.ExcludedLeakageSignals)
            || !DynamicGraphEdgeAuthorityLeakageBoundary.IsExact(
                identityQuality.ExcludedLeakageSignals))
        {
            return InvalidMeasurement();
        }

        return OperationResult<DynamicGraphEdgeAuthorityControlMeasurement>.Ok(
            new DynamicGraphEdgeAuthorityControlMeasurement(
                authorityEvidence,
                orderQuality,
                identityQuality));
    }

    private static bool IsCanonicalValue(string? value)
        => !string.IsNullOrWhiteSpace(value)
           && StringComparer.Ordinal.Equals(value, value.Trim())
           && value.All(character => !char.IsControl(character))
           && value.IndexOfAny(['\r', '\n', '\u0085', '\u2028', '\u2029']) < 0;

    private static OperationResult<DynamicGraphEdgeAuthorityControlMeasurement> InvalidMeasurement()
        => OperationResult<DynamicGraphEdgeAuthorityControlMeasurement>.Fail(
            InvalidMeasurementMessage,
            InvalidMeasurementCode);
}

internal static class DynamicGraphEdgeAuthorityControlSnapshots
{
    internal static DynamicGraphEdgeAuthorityEvidence CopyAuthorityEvidence(
        DynamicGraphEdgeAuthorityEvidence source)
        => new(
            source.EdgeId,
            source.AuthorityUnitId,
            source.AuthorityUnitLevel,
            source.DeterministicFinalAuthority,
            source.BaselineUnitId,
            source.BaselineRole,
            source.ExcludedLeakageSignals.ToArray(),
            source.SourceProof,
            source.CutoverProof,
            source.AblationProofRef);

    internal static DynamicGraphEdgeOrderQualityMeasurement CopyOrderQuality(
        DynamicGraphEdgeOrderQualityMeasurement source)
        => new(
            source.EdgeId,
            source.ResolvedAuthorityUnitId,
            source.PermutationProbeCount,
            source.PermutationInvariant,
            source.ProbeIds.ToArray(),
            source.ExcludedLeakageSignals.ToArray());

    internal static DynamicGraphEdgeIdentityQualityMeasurement CopyIdentityQuality(
        DynamicGraphEdgeIdentityQualityMeasurement source)
        => new(
            source.EdgeId,
            source.ResolvedAuthorityUnitId,
            source.IdentityControlProbeCount,
            source.IdentityInvariant,
            source.ProbeIds.ToArray(),
            source.ControlledSignals.ToArray(),
            source.ControlValues.ToArray(),
            source.ExcludedLeakageSignals.ToArray());

    internal static DynamicGraphEdgeAuthorityControlMeasurement CopyMeasurement(
        DynamicGraphEdgeAuthorityControlMeasurement source)
        => new(source.AuthorityEvidence, source.EdgeOrderQuality, source.EdgeIdentityQuality);

    internal static DynamicGraphMeasurementTarget CopyTarget(DynamicGraphMeasurementTarget source)
        => new(
            source.TargetId,
            source.Level,
            source.MechanismKind,
            source.DeterministicFinalAuthority);
}
