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
/// Carries one independently owned authority-control measurement, the atomic novelty evaluation
/// that produced its own novelty measurement, and the selection-ablation adapter result. This is
/// correlated evidence only: it has no overall verdict and does not bind an authority
/// <c>AblationProofRef</c> to an evaluation identifier or infer target execution, generic product
/// accuracy, training, DPO, promotion, lifecycle state, or graph mutation.
/// </summary>
public sealed record DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement
{
    private readonly DynamicGraphEdgeAuthorityControlMeasurement _edgeAuthorityControl;
    private readonly DynamicGraphEdgeAuthorityNoveltyEvaluationResult _noveltyEvaluation;
    private readonly DynamicGraphAblationMeasurement _selectionAblation;

    public DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement(
        DynamicGraphEdgeAuthorityControlMeasurement EdgeAuthorityControl,
        DynamicGraphEdgeAuthorityNoveltyEvaluationResult NoveltyEvaluation,
        DynamicGraphAblationMeasurement SelectionAblation)
    {
        ArgumentNullException.ThrowIfNull(EdgeAuthorityControl);
        ArgumentNullException.ThrowIfNull(NoveltyEvaluation);
        ArgumentNullException.ThrowIfNull(SelectionAblation);

        _edgeAuthorityControl = DynamicGraphEdgeQualityEvidenceSnapshots.CopyAuthorityControl(
            EdgeAuthorityControl);
        _noveltyEvaluation = DynamicGraphEdgeQualityEvidenceSnapshots.CopyNoveltyEvaluation(
            NoveltyEvaluation);
        _selectionAblation = DynamicGraphEdgeQualityEvidenceSnapshots.CopyAblation(
            SelectionAblation);
    }

    public DynamicGraphEdgeAuthorityControlMeasurement EdgeAuthorityControl
        => DynamicGraphEdgeQualityEvidenceSnapshots.CopyAuthorityControl(_edgeAuthorityControl);

    public DynamicGraphEdgeAuthorityNoveltyEvaluationResult NoveltyEvaluation
        => DynamicGraphEdgeQualityEvidenceSnapshots.CopyNoveltyEvaluation(_noveltyEvaluation);

    public DynamicGraphAblationMeasurement SelectionAblation
        => DynamicGraphEdgeQualityEvidenceSnapshots.CopyAblation(_selectionAblation);
}

/// <summary>
/// Associates one admitted L3 logical-flow target with independently owned edge-authority
/// selection quality evidence. This evidence-only association does not execute the target or make
/// a generic quality, aggregate verdict, training, DPO, checkpoint, promotion, or lifecycle claim.
/// </summary>
public sealed record DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement
{
    private readonly DynamicGraphMeasurementTarget _target;
    private readonly DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement
        _edgeAuthoritySelectionQualityEvidence;
    private readonly string _authorityEdgeId;

    public DynamicGraphTargetEdgeAuthoritySelectionQualityEvidenceMeasurement(
        DynamicGraphMeasurementTarget Target,
        DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement
            EdgeAuthoritySelectionQualityEvidence)
    {
        ArgumentNullException.ThrowIfNull(Target);
        ArgumentNullException.ThrowIfNull(EdgeAuthoritySelectionQualityEvidence);

        _target = DynamicGraphEdgeQualityEvidenceSnapshots.CopyTarget(Target);
        _edgeAuthoritySelectionQualityEvidence =
            DynamicGraphEdgeQualityEvidenceSnapshots.CopySelectionQualityEvidence(
                EdgeAuthoritySelectionQualityEvidence);
        _authorityEdgeId = _edgeAuthoritySelectionQualityEvidence
            .EdgeAuthorityControl.AuthorityEvidence.EdgeId;
    }

    public DynamicGraphMeasurementTarget Target
        => DynamicGraphEdgeQualityEvidenceSnapshots.CopyTarget(_target);

    public DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement
        EdgeAuthoritySelectionQualityEvidence
        => DynamicGraphEdgeQualityEvidenceSnapshots.CopySelectionQualityEvidence(
            _edgeAuthoritySelectionQualityEvidence);

    internal (DynamicGraphMeasurementTarget Target, string AuthorityEdgeId)
        SnapshotTargetAuthorityEdgeBinding()
        => (DynamicGraphEdgeQualityEvidenceSnapshots.CopyTarget(_target), _authorityEdgeId);
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
    private const string SelectionEvidenceMismatchCode =
        "EDGE_AUTHORITY_SELECTION_QUALITY_EVIDENCE_MISMATCH";
    private const string SelectionEvidenceMismatchMessage =
        "Dynamic graph edge authority selection quality evidence is not correlated.";

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

    /// <summary>
    /// Composes only existing authority-control, atomic novelty, and paired-selection ablation
    /// boundaries. Each component gate may be false while the evidence composition succeeds. The
    /// method makes no overall quality verdict and does not interpret <c>AblationProofRef</c> as an
    /// evaluation identifier.
    /// </summary>
    public static OperationResult<DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement>
        MeasureEdgeAuthoritySelection(
            EdgeAuthorityExecutionResult? authorityResult,
            DynamicGraphEdgeAuthorityNoveltyEvaluationRequest? noveltyRequest)
    {
        var authority = DynamicGraphEdgeAuthorityControlMeter.Measure(authorityResult);
        if (!authority.IsSuccess)
        {
            return authority.MapError<
                DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement>();
        }

        var novelty = DynamicGraphEdgeAuthorityNoveltyEvaluationProducer.Produce(noveltyRequest);
        if (!novelty.IsSuccess)
        {
            return novelty.MapError<
                DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement>();
        }

        var noveltyEvidence = novelty.Value!;
        var pairedSelection = noveltyEvidence.PairedSelection;
        var selectionAblation = DynamicGraphAblationMeter.MeasureEdgeAuthoritySelection(
            pairedSelection);
        if (!selectionAblation.IsSuccess)
        {
            return selectionAblation.MapError<
                DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement>();
        }

        var authorityEvidence = authority.Value!.AuthorityEvidence;
        var noveltyObservation = noveltyEvidence.Observation;
        var noveltyMeasurement = noveltyEvidence.Measurement;
        var ablationEvidence = selectionAblation.Value!;
        if (!StringComparer.Ordinal.Equals(authorityEvidence.EdgeId, pairedSelection.EdgeId)
            || !StringComparer.Ordinal.Equals(
                authorityEvidence.AuthorityUnitId,
                pairedSelection.AuthorityUnitId)
            || !StringComparer.Ordinal.Equals(
                pairedSelection.EvaluationId,
                noveltyObservation.EvaluationId)
            || !StringComparer.Ordinal.Equals(
                pairedSelection.EvaluationId,
                noveltyMeasurement.EvaluationId)
            || !StringComparer.Ordinal.Equals(
                pairedSelection.EvaluationId,
                ablationEvidence.EvaluationId)
            || pairedSelection.RowsEvaluated != noveltyObservation.RowsEvaluated
            || pairedSelection.RowsEvaluated != noveltyMeasurement.RowsEvaluated
            || pairedSelection.RowsEvaluated != ablationEvidence.RowsEvaluated
            || !SplitProofsMatch(
                pairedSelection.SplitProof,
                noveltyObservation.SplitProof)
            || !SplitProofsMatch(
                pairedSelection.SplitProof,
                noveltyMeasurement.SplitProof)
            || !SplitProofsMatch(
                pairedSelection.SplitProof,
                ablationEvidence.SplitProof))
        {
            return OperationResult<
                DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement>.Fail(
                SelectionEvidenceMismatchMessage,
                SelectionEvidenceMismatchCode);
        }

        return OperationResult<
            DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement>.Ok(
            new DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement(
                authority.Value,
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

    internal static DynamicGraphEdgeAuthorityNoveltyEvaluationResult CopyNoveltyEvaluation(
        DynamicGraphEdgeAuthorityNoveltyEvaluationResult source)
        => new(
            source.CallerAttestedFingerprintNormalizationId,
            source.CallerAttestedTokenizationId,
            source.RequestedRowIds,
            source.MissingRowIds,
            source.TrainingEvidence,
            source.Rows,
            source.PairedSelection,
            source.Observation,
            source.Measurement);

    internal static DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement
        CopySelectionQualityEvidence(
            DynamicGraphEdgeAuthoritySelectionQualityEvidenceMeasurement source)
        => new(
            source.EdgeAuthorityControl,
            source.NoveltyEvaluation,
            source.SelectionAblation);

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
