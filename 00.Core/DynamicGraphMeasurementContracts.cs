// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace AgentInfrastructure.Logical;

/// <summary>
/// Identifies the DynamicGraph layer whose behavior is being measured.
/// </summary>
public enum DynamicGraphMeasurementTargetLevel
{
    Pair,
    L2LogicalUnit,
    L3LogicalFlow,
    L4LogicalOrchestrator,
    L5LearningMachine
}

/// <summary>
/// Identifies the mechanism behind a DynamicGraph measurement target.
/// Only a learning subject may be admitted as a target; the other values are
/// explicit forbidden-shortcut evidence for the admission boundary.
/// </summary>
public enum DynamicGraphMeasurementMechanismKind
{
    LearningSubject,
    Knn,
    Scorer,
    Counter,
    Regex
}

/// <summary>
/// Describes the subject whose behavior a DynamicGraph measurement observes.
/// It carries identity and classification only; it is not a lifecycle verdict.
/// </summary>
public sealed record DynamicGraphMeasurementTarget(
    string TargetId,
    DynamicGraphMeasurementTargetLevel Level,
    DynamicGraphMeasurementMechanismKind MechanismKind,
    bool DeterministicFinalAuthority);

/// <summary>
/// Carries the source-disjoint evidence that must be admitted before a DynamicGraph
/// held-out measurement can be used. The roots are exact source identities for the
/// training and evaluation partitions; source hashes are opaque, caller-produced
/// identities and this contract does not prescribe or infer a hashing algorithm.
/// </summary>
/// <param name="TrainSourceRoots">Canonical, unique source roots used by training.</param>
/// <param name="EvalSourceRoots">Canonical, unique source roots reserved for held-out evaluation.</param>
/// <param name="SourceHashes">Canonical, unique opaque source hashes supporting the split proof.</param>
/// <param name="CrossSplitNearDuplicateCount">Number of detected train-to-held-out near-duplicate pairs.</param>
/// <param name="SimhashOrMinhash">Exact near-duplicate method name: <c>simhash</c> or <c>minhash</c>.</param>
/// <param name="NearDuplicateThreshold">Finite non-negative similarity threshold used to produce the count.</param>
public sealed record DynamicGraphMeasurementSplitProof(
    IReadOnlyList<string> TrainSourceRoots,
    IReadOnlyList<string> EvalSourceRoots,
    IReadOnlyList<string> SourceHashes,
    int CrossSplitNearDuplicateCount,
    string SimhashOrMinhash,
    double NearDuplicateThreshold);

/// <summary>
/// Carries already-produced held-out selection evidence for one novelty measurement.
/// The rates and counts are supplied observations; this contract does not select rows,
/// discover patterns, inspect tokens, execute training, or decide lifecycle state.
/// </summary>
public sealed record DynamicGraphNoveltyObservation(
    string EvaluationId,
    int RequestedRows,
    int RowsEvaluated,
    int SelectionShortfall,
    double NewPatternRate,
    double SourceTokenSeenRate,
    double MaxAccuracy,
    DynamicGraphMeasurementSplitProof SplitProof);

/// <summary>
/// Numeric novelty evidence projected from one admitted held-out split. The gate reports
/// only the locked numeric predicates; it is not full quality, training, promotion,
/// ablation, source-id selection, named-control, or lifecycle evidence.
/// </summary>
public sealed record DynamicGraphNoveltyMeasurement(
    string EvaluationId,
    int RequestedRows,
    int RowsEvaluated,
    int SelectionShortfall,
    double NewPatternRate,
    double SourceTokenSeenRate,
    double MaxAccuracy,
    double MinimumNewPatternRate,
    double MaximumSeenTokenRate,
    bool MeetsNoveltyGate,
    DynamicGraphMeasurementSplitProof SplitProof);

/// <summary>
/// Associates one admitted L2 logical-unit or L3 logical-flow target with numeric novelty evidence.
/// The association does not prove a physical origin path from observation to target, and the target
/// identity may differ from the evaluation identity. It is not full quality, trained-state, lifecycle,
/// training, or promotion evidence.
/// </summary>
public sealed record DynamicGraphTargetNoveltyMeasurement(
    DynamicGraphMeasurementTarget Target,
    DynamicGraphNoveltyMeasurement Novelty);

/// <summary>
/// Separates the live margin signal from source-disjoint held-out accuracy.
/// The delta is the held-out after value minus the held-out before value; it is never
/// derived from <see cref="LiveMarginAccuracy"/>.
/// </summary>
public sealed record DynamicGraphAccuracyAxis(
    double LiveMarginAccuracy,
    double HeldOutAccuracy,
    double AccuracyDelta);

/// <summary>
/// Numeric DynamicGraph accuracy evidence projected from one admitted held-out split.
/// <see cref="MeetsQ30NumericalGate"/> reports only the locked numeric predicates; it
/// does not assert any state transition or lifecycle decision.
/// </summary>
public sealed record DynamicGraphAccuracyMeasurement(
    string EvaluationId,
    int RowsEvaluated,
    DynamicGraphAccuracyAxis Accuracy,
    double HeldOutRewardMargin,
    bool MeetsQ30NumericalGate,
    DynamicGraphMeasurementSplitProof SplitProof);

/// <summary>
/// Numeric held-out comparison between one subject and its ablated counterpart.
/// This evidence does not prove the ablated runtime state, novelty, source-id
/// independence, named controls, trained state, promotion, or lifecycle state.
/// </summary>
public sealed record DynamicGraphAblationMeasurement(
    string EvaluationId,
    int RowsEvaluated,
    double SubjectHeldOutAccuracy,
    double AblatedHeldOutAccuracy,
    double AccuracyDrop,
    bool MeetsQ11NumericalGate,
    DynamicGraphMeasurementSplitProof SplitProof);

/// <summary>
/// Associates one admitted L2 logical-unit or L3 logical-flow target with numeric ablation evidence.
/// This association does not prove physical target disablement, absence of trained state, full quality,
/// lifecycle, training, or promotion, and it does not bind the target identity to the evaluation identity.
/// </summary>
public sealed record DynamicGraphTargetAblationMeasurement(
    DynamicGraphMeasurementTarget Target,
    DynamicGraphAblationMeasurement Ablation);

/// <summary>
/// Associates one admitted DynamicGraph measurement target with its numeric accuracy evidence.
/// This record carries measurement evidence only; it is not a lifecycle or training decision.
/// </summary>
public sealed record DynamicGraphTargetAccuracyMeasurement(
    DynamicGraphMeasurementTarget Target,
    DynamicGraphAccuracyMeasurement Accuracy);

/// <summary>
/// Resource cost observed for one completed training batch.
/// </summary>
public sealed record DynamicGraphTrainingSpeedAxis(
    long CpuMs,
    double PeakRamMb,
    long ElapsedMs,
    int TrainRows);

/// <summary>
/// Pure training-cost projection for one already-produced batch result.
/// </summary>
public sealed record DynamicGraphTrainingSpeedMeasurement(
    string BatchId,
    DynamicGraphTrainingSpeedAxis Speed);

/// <summary>
/// Associates one admitted L5 learning-machine target with its training-cost evidence.
/// This record describes an already-produced batch result; it does not execute training.
/// </summary>
public sealed record DynamicGraphTargetTrainingSpeedMeasurement(
    DynamicGraphMeasurementTarget Target,
    DynamicGraphTrainingSpeedMeasurement TrainingSpeed);

/// <summary>
/// Raw timing samples observed for one completed DynamicGraph run.
/// </summary>
public sealed record DynamicGraphRunSpeedObservation(
    int EvaluatedRowCount,
    long InferenceElapsedMs,
    long RoutingElapsedMs,
    IReadOnlyList<long> CandidateElapsedMsSamples,
    IReadOnlyList<long> BatchElapsedMsSamples);

/// <summary>
/// Pure run-speed projection. Optional per-candidate and per-batch values are
/// absent when the corresponding observation contains no samples.
/// </summary>
public sealed record DynamicGraphRunSpeedAxis(
    long InferenceElapsedMs,
    double ThroughputRowsPerSec,
    long RoutingElapsedMs,
    long? PerCandidateElapsedMs,
    long? PerBatchElapsedMs);

/// <summary>
/// Run-speed evidence produced from an independently snapshotted observation.
/// </summary>
public sealed record DynamicGraphRunSpeedMeasurement(
    int EvaluatedRowCount,
    int CandidateSampleCount,
    int BatchSampleCount,
    DynamicGraphRunSpeedAxis RunSpeed);

/// <summary>
/// Associates one admitted DynamicGraph measurement target with its run-speed evidence.
/// This record carries measurement evidence only; it is not a lifecycle or training decision.
/// </summary>
public sealed record DynamicGraphTargetRunSpeedMeasurement(
    DynamicGraphMeasurementTarget Target,
    DynamicGraphRunSpeedMeasurement RunSpeed);

/// <summary>
/// Associates one admitted L3 logical-flow target with its edge-order evidence.
/// This record carries association evidence only; it is not a full quality,
/// lifecycle, training, or promotion decision.
/// </summary>
public sealed record DynamicGraphTargetEdgeOrderQualityMeasurement(
    DynamicGraphMeasurementTarget Target,
    DynamicGraphEdgeOrderQualityMeasurement EdgeOrderQuality);

/// <summary>
/// Associates one admitted L3 logical-flow target with its edge-identity evidence.
/// This record carries association evidence only; it is not aggregate quality,
/// trained-state, lifecycle, training, DPO, promotion, or candidate-order evidence.
/// </summary>
public sealed record DynamicGraphTargetEdgeIdentityQualityMeasurement(
    DynamicGraphMeasurementTarget Target,
    DynamicGraphEdgeIdentityQualityMeasurement EdgeIdentityQuality);

/// <summary>
/// Carries order-permutation and exact leakage-exclusion evidence for one learned edge authority.
/// This is order/exclusion evidence only: it is not novelty, ablation, source-id selection,
/// trained-state, promotion, or lifecycle evidence.
/// </summary>
public sealed record DynamicGraphEdgeOrderQualityMeasurement
{
    private readonly string[] _probeIds;
    private readonly string[] _excludedLeakageSignals;

    public DynamicGraphEdgeOrderQualityMeasurement(
        string EdgeId,
        string ResolvedAuthorityUnitId,
        int PermutationProbeCount,
        bool PermutationInvariant,
        IReadOnlyList<string> ProbeIds,
        IReadOnlyList<string> ExcludedLeakageSignals)
    {
        ArgumentNullException.ThrowIfNull(ProbeIds);
        ArgumentNullException.ThrowIfNull(ExcludedLeakageSignals);

        this.EdgeId = EdgeId;
        this.ResolvedAuthorityUnitId = ResolvedAuthorityUnitId;
        this.PermutationProbeCount = PermutationProbeCount;
        this.PermutationInvariant = PermutationInvariant;
        _probeIds = ProbeIds.ToArray();
        _excludedLeakageSignals = ExcludedLeakageSignals.ToArray();
    }

    public string EdgeId { get; }

    public string ResolvedAuthorityUnitId { get; }

    public int PermutationProbeCount { get; }

    public bool PermutationInvariant { get; }

    public IReadOnlyList<string> ProbeIds => Array.AsReadOnly(_probeIds.ToArray());

    public IReadOnlyList<string> ExcludedLeakageSignals
        => Array.AsReadOnly(_excludedLeakageSignals.ToArray());
}

/// <summary>
/// Carries validated identity-control and exact leakage-exclusion evidence for one learned edge authority.
/// This is identity/exclusion evidence only: it is not candidate-order evidence, aggregate quality,
/// trained state, promotion, lifecycle, or training evidence.
/// </summary>
public sealed record DynamicGraphEdgeIdentityQualityMeasurement
{
    private readonly string[] _probeIds;
    private readonly string[] _controlledSignals;
    private readonly string[] _controlValues;
    private readonly string[] _excludedLeakageSignals;

    public DynamicGraphEdgeIdentityQualityMeasurement(
        string EdgeId,
        string ResolvedAuthorityUnitId,
        int IdentityControlProbeCount,
        bool IdentityInvariant,
        IReadOnlyList<string> ProbeIds,
        IReadOnlyList<string> ControlledSignals,
        IReadOnlyList<string> ControlValues,
        IReadOnlyList<string> ExcludedLeakageSignals)
    {
        ArgumentNullException.ThrowIfNull(ProbeIds);
        ArgumentNullException.ThrowIfNull(ControlledSignals);
        ArgumentNullException.ThrowIfNull(ControlValues);
        ArgumentNullException.ThrowIfNull(ExcludedLeakageSignals);

        this.EdgeId = EdgeId;
        this.ResolvedAuthorityUnitId = ResolvedAuthorityUnitId;
        this.IdentityControlProbeCount = IdentityControlProbeCount;
        this.IdentityInvariant = IdentityInvariant;
        _probeIds = ProbeIds.ToArray();
        _controlledSignals = ControlledSignals.ToArray();
        _controlValues = ControlValues.ToArray();
        _excludedLeakageSignals = ExcludedLeakageSignals.ToArray();
    }

    public string EdgeId { get; }

    public string ResolvedAuthorityUnitId { get; }

    public int IdentityControlProbeCount { get; }

    public bool IdentityInvariant { get; }

    public IReadOnlyList<string> ProbeIds => Array.AsReadOnly(_probeIds.ToArray());

    public IReadOnlyList<string> ControlledSignals
        => Array.AsReadOnly(_controlledSignals.ToArray());

    public IReadOnlyList<string> ControlValues
        => Array.AsReadOnly(_controlValues.ToArray());

    public IReadOnlyList<string> ExcludedLeakageSignals
        => Array.AsReadOnly(_excludedLeakageSignals.ToArray());
}

/// <summary>
/// Carries an already-produced runtime result together with the concrete boundary types
/// that physically produced it. The meter reflects only the specified types; it neither
/// executes a target nor infers checkpoint absence, trained-feature quality, accuracy,
/// lifecycle, or promotion state.
/// </summary>
public sealed record DynamicGraphUntrainedHonestyObservation(
    string EvaluationId,
    bool RuntimeReportedNotReady,
    IReadOnlyList<string> RuntimeEvidence,
    IReadOnlyList<Type> InspectedTypes);

/// <summary>
/// Physical L2 untrained-honesty evidence for the specified runtime boundary types.
/// <see cref="MeetsUntrainedHonestyGate"/> is only the locked F5.1.3 predicate carrier;
/// it is not a final PASS, trained-feature, accuracy, aggregate-quality, lifecycle, or
/// promotion decision. This measurement does not execute the inspected target.
/// </summary>
public sealed record DynamicGraphUntrainedHonestyMeasurement(
    string EvaluationId,
    bool RuntimeReportedNotReady,
    bool HasNoTrainedStateEvidence,
    bool HasNoSemanticClassificationEvidence,
    IReadOnlyList<string> RuntimeEvidence,
    IReadOnlyList<string> InspectedTypeNames,
    IReadOnlyList<string> ForbiddenShortcutMatches,
    bool MeetsUntrainedHonestyGate);

/// <summary>
/// Registry-backed physical provenance for the exact L2 logical-unit producer that was
/// executed by an untrained-honesty probe. This evidence carries producer identity and
/// typed-boundary metadata only; it is not a target association, final PASS, trained-state,
/// lifecycle, or promotion decision.
/// </summary>
public sealed record DynamicGraphUntrainedHonestyL2ProducerEvidence(
    string UnitId,
    string UnitRole,
    string DomainTag,
    string Version,
    long RegistrationOrder,
    string ProducerTypeName,
    string InputTypeName,
    string OutputTypeName);

/// <summary>
/// Couples registry-backed physical L2 producer provenance to untrained-honesty evidence.
/// It reports provenance and honesty only; it does not associate a measurement target or
/// assert final PASS, trained state, lifecycle state, or promotion readiness.
/// </summary>
public sealed record DynamicGraphUntrainedHonestyL2ProbeResult(
    DynamicGraphUntrainedHonestyL2ProducerEvidence Producer,
    DynamicGraphUntrainedHonestyMeasurement Honesty);

/// <summary>
/// Associates one admitted L2 logical-unit target with an already-produced,
/// registry-backed untrained-honesty probe result. This association is not a final PASS,
/// trained-state, aggregate-quality, lifecycle, or promotion decision.
/// </summary>
public sealed record DynamicGraphTargetUntrainedHonestyMeasurement(
    DynamicGraphMeasurementTarget Target,
    DynamicGraphUntrainedHonestyL2ProbeResult UntrainedHonesty);
