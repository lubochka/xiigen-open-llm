// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

using AgentInfrastructure;

/// <summary>
/// Carries independently measured edge-authority, novelty, and numeric ablation evidence.
/// This carrier does not claim a full quality axis, aggregate graph quality, physical edge
/// disablement, lifecycle state, DPO state, training state, promotion, or graph mutation.
/// </summary>
public sealed record DynamicGraphEdgeQualityEvidenceMeasurement
{
    private readonly DynamicGraphEdgeAuthorityControlMeasurement _edgeAuthorityControl;
    private readonly DynamicGraphNoveltyMeasurement _novelty;
    private readonly DynamicGraphAblationMeasurement _ablation;

    public DynamicGraphEdgeQualityEvidenceMeasurement(
        DynamicGraphEdgeAuthorityControlMeasurement EdgeAuthorityControl,
        DynamicGraphNoveltyMeasurement Novelty,
        DynamicGraphAblationMeasurement Ablation)
    {
        ArgumentNullException.ThrowIfNull(EdgeAuthorityControl);
        ArgumentNullException.ThrowIfNull(Novelty);
        ArgumentNullException.ThrowIfNull(Ablation);

        _edgeAuthorityControl = DynamicGraphEdgeQualityEvidenceSnapshots.CopyAuthorityControl(
            EdgeAuthorityControl);
        _novelty = DynamicGraphEdgeQualityEvidenceSnapshots.CopyNovelty(Novelty);
        _ablation = DynamicGraphEdgeQualityEvidenceSnapshots.CopyAblation(Ablation);
    }

    public DynamicGraphEdgeAuthorityControlMeasurement EdgeAuthorityControl
        => DynamicGraphEdgeQualityEvidenceSnapshots.CopyAuthorityControl(_edgeAuthorityControl);

    public DynamicGraphNoveltyMeasurement Novelty
        => DynamicGraphEdgeQualityEvidenceSnapshots.CopyNovelty(_novelty);

    public DynamicGraphAblationMeasurement Ablation
        => DynamicGraphEdgeQualityEvidenceSnapshots.CopyAblation(_ablation);
}

/// <summary>
/// Associates one admitted L3 logical-flow target with owned edge-quality evidence.
/// The association is evidence-only and is not a full graph-quality, lifecycle, DPO,
/// training, promotion, aggregate, or physical-disablement decision.
/// </summary>
public sealed record DynamicGraphTargetEdgeQualityEvidenceMeasurement
{
    private readonly DynamicGraphMeasurementTarget _target;
    private readonly DynamicGraphEdgeQualityEvidenceMeasurement _edgeQualityEvidence;

    public DynamicGraphTargetEdgeQualityEvidenceMeasurement(
        DynamicGraphMeasurementTarget Target,
        DynamicGraphEdgeQualityEvidenceMeasurement EdgeQualityEvidence)
    {
        ArgumentNullException.ThrowIfNull(Target);
        ArgumentNullException.ThrowIfNull(EdgeQualityEvidence);

        _target = DynamicGraphEdgeQualityEvidenceSnapshots.CopyTarget(Target);
        _edgeQualityEvidence = DynamicGraphEdgeQualityEvidenceSnapshots.CopyMeasurement(
            EdgeQualityEvidence);
    }

    public DynamicGraphMeasurementTarget Target
        => DynamicGraphEdgeQualityEvidenceSnapshots.CopyTarget(_target);

    public DynamicGraphEdgeQualityEvidenceMeasurement EdgeQualityEvidence
        => DynamicGraphEdgeQualityEvidenceSnapshots.CopyMeasurement(_edgeQualityEvidence);
}

/// <summary>
/// Composes existing edge-authority, held-out novelty, and numeric ablation meters in a
/// fixed order. It only proves that the three admitted evidence carriers describe the
/// same positive held-out evaluation and split; it does not calculate an aggregate score.
/// </summary>
public static class DynamicGraphEdgeQualityEvidenceMeter
{
    private const string EvaluationMismatchCode = "EDGE_QUALITY_EVALUATION_MISMATCH";
    private const string EvaluationMismatchMessage =
        "Dynamic graph edge quality evidence does not describe the same held-out evaluation.";

    public static OperationResult<DynamicGraphEdgeQualityEvidenceMeasurement> Measure(
        EdgeAuthorityExecutionResult? authorityResult,
        DynamicGraphNoveltyObservation? noveltyObservation,
        DynamicGraphAccuracyMeasurement? subject,
        DynamicGraphAccuracyMeasurement? ablated)
    {
        var authority = DynamicGraphEdgeAuthorityControlMeter.Measure(authorityResult);
        if (!authority.IsSuccess)
            return authority.MapError<DynamicGraphEdgeQualityEvidenceMeasurement>();

        var novelty = DynamicGraphNoveltyMeter.Measure(noveltyObservation);
        if (!novelty.IsSuccess)
            return novelty.MapError<DynamicGraphEdgeQualityEvidenceMeasurement>();

        var ablation = DynamicGraphAblationMeter.Measure(subject, ablated);
        if (!ablation.IsSuccess)
            return ablation.MapError<DynamicGraphEdgeQualityEvidenceMeasurement>();

        var noveltyEvidence = novelty.Value!;
        var ablationEvidence = ablation.Value!;
        if (!StringComparer.Ordinal.Equals(
                noveltyEvidence.EvaluationId,
                ablationEvidence.EvaluationId)
            || noveltyEvidence.RowsEvaluated <= 0
            || noveltyEvidence.RowsEvaluated != ablationEvidence.RowsEvaluated
            || !SplitProofsMatch(noveltyEvidence.SplitProof, ablationEvidence.SplitProof))
        {
            return OperationResult<DynamicGraphEdgeQualityEvidenceMeasurement>.Fail(
                EvaluationMismatchMessage,
                EvaluationMismatchCode);
        }

        return OperationResult<DynamicGraphEdgeQualityEvidenceMeasurement>.Ok(
            new DynamicGraphEdgeQualityEvidenceMeasurement(
                authority.Value!,
                noveltyEvidence,
                ablationEvidence));
    }

    private static bool SplitProofsMatch(
        DynamicGraphMeasurementSplitProof novelty,
        DynamicGraphMeasurementSplitProof ablation)
        => novelty.TrainSourceRoots.SequenceEqual(
               ablation.TrainSourceRoots,
               StringComparer.Ordinal)
           && novelty.EvalSourceRoots.SequenceEqual(
               ablation.EvalSourceRoots,
               StringComparer.Ordinal)
           && novelty.SourceHashes.SequenceEqual(
               ablation.SourceHashes,
               StringComparer.Ordinal)
           && novelty.CrossSplitNearDuplicateCount == ablation.CrossSplitNearDuplicateCount
           && StringComparer.Ordinal.Equals(
               novelty.SimhashOrMinhash,
               ablation.SimhashOrMinhash)
           && novelty.NearDuplicateThreshold.Equals(ablation.NearDuplicateThreshold);
}

internal static class DynamicGraphEdgeQualityEvidenceSnapshots
{
    internal static DynamicGraphEdgeAuthorityControlMeasurement CopyAuthorityControl(
        DynamicGraphEdgeAuthorityControlMeasurement source)
        => DynamicGraphEdgeAuthorityControlSnapshots.CopyMeasurement(source);

    internal static DynamicGraphNoveltyMeasurement CopyNovelty(
        DynamicGraphNoveltyMeasurement source)
        => new(
            source.EvaluationId,
            source.RequestedRows,
            source.RowsEvaluated,
            source.SelectionShortfall,
            source.NewPatternRate,
            source.SourceTokenSeenRate,
            source.MaxAccuracy,
            source.MinimumNewPatternRate,
            source.MaximumSeenTokenRate,
            source.MeetsNoveltyGate,
            CopySplitProof(source.SplitProof));

    internal static DynamicGraphAblationMeasurement CopyAblation(
        DynamicGraphAblationMeasurement source)
        => new(
            source.EvaluationId,
            source.RowsEvaluated,
            source.SubjectHeldOutAccuracy,
            source.AblatedHeldOutAccuracy,
            source.AccuracyDrop,
            source.MeetsQ11NumericalGate,
            CopySplitProof(source.SplitProof));

    internal static DynamicGraphEdgeQualityEvidenceMeasurement CopyMeasurement(
        DynamicGraphEdgeQualityEvidenceMeasurement source)
        => new(source.EdgeAuthorityControl, source.Novelty, source.Ablation);

    internal static DynamicGraphMeasurementTarget CopyTarget(
        DynamicGraphMeasurementTarget source)
        => new(
            source.TargetId,
            source.Level,
            source.MechanismKind,
            source.DeterministicFinalAuthority);

    private static DynamicGraphMeasurementSplitProof CopySplitProof(
        DynamicGraphMeasurementSplitProof source)
        => new(
            Array.AsReadOnly(source.TrainSourceRoots.ToArray()),
            Array.AsReadOnly(source.EvalSourceRoots.ToArray()),
            Array.AsReadOnly(source.SourceHashes.ToArray()),
            source.CrossSplitNearDuplicateCount,
            source.SimhashOrMinhash,
            source.NearDuplicateThreshold);
}
