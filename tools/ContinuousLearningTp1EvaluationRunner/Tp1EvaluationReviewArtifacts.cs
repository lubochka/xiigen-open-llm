// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ContinuousLearning;

namespace ContinuousLearningTp1EvaluationRunner;

internal sealed record Tp1ReviewIndexInput(
    string ReviewPacketId,
    string OrderingAuthorityRef,
    IReadOnlyList<Tp1ReviewRunIndexInput> Runs);

internal sealed record Tp1ReviewRunIndexInput(
    int SequenceOrdinal,
    string ScoringRunDirectory);

internal sealed record Tp1ReviewRunSummary(
    int Ordinal,
    string RunId,
    string SourceRunId,
    string WindowRef,
    int CaseCount,
    int SemanticCandidateCount,
    double HeldOutAccuracyYgoal,
    double HeldOutAccuracyYbest,
    double TeacherGapToGold,
    double TeacherBeatRate,
    int StrictTeacherWins,
    string EvaluationRosterSha256,
    Tp1ReviewSourceArtifactHashes SourceArtifactHashes)
{
    // Паспорт шума появился позже TP1-формата, поэтому держим его init-полями:
    // старые конструкторы и сохранённые review-пакеты не должны ломаться только из-за новой честной пометки.
    public double? MeasurementNoiseFloor { get; init; }

    public string MeasurementNoiseStatus { get; init; } = MeasurementNoisePolicy.DifferenceNotProvenNoCalibrationStatus;
}

internal sealed record Tp1ReviewSourceArtifactHashes(
    int Ordinal,
    string CandidateScoresSha256,
    string CaseComparisonsSha256,
    string TeacherStageGapReportSha256,
    string ScoringRunManifestSha256);

internal sealed record Tp1ReviewAdjacentDelta(
    int FromOrdinal,
    int ToOrdinal,
    double DeltaHeldOutAccuracyYgoal,
    double DeltaHeldOutAccuracyYbest,
    double DeltaTeacherGapToGold,
    double DeltaTeacherBeatRate);

internal sealed record Tp1ReviewAggregate(
    int RunCount,
    int TotalCaseObservations,
    int TotalSemanticCandidates,
    int PooledStrictTeacherWins,
    double MeanHeldOutAccuracyYgoal,
    double MeanHeldOutAccuracyYbest,
    double AggregateTeacherGapToGold,
    double PooledTeacherBeatRate);

internal sealed record Tp1ReviewInvariantSet(
    string Stage,
    string EvaluationSetId,
    string DomainId,
    string ScorerId,
    string ScorerVersion,
    string ThresholdSetId,
    int InferredK,
    int CaseCount,
    string EvaluationRosterSha256,
    string SplitProofArtifactRef,
    string SplitProofSha256,
    string ModelId,
    string TeacherArtifactDigestKind,
    string OllamaManifestDigest,
    string OllamaModelLayerDigest,
    string LocalRetentionPolicyRef,
    string OutputLicenseRef,
    string TeacherLicenseEvidenceRef,
    string DeploymentEvidenceRef);

internal sealed record Tp1EvaluationReviewRequestArtifact(
    string Status,
    string ReviewPacketId,
    string OrderingAuthorityRef,
    bool OrderingVerified,
    Tp1ReviewInvariantSet Invariants,
    IReadOnlyList<Tp1ReviewRunSummary> Runs,
    IReadOnlyList<Tp1ReviewAdjacentDelta> AdjacentDeltas,
    Tp1ReviewAggregate Aggregate,
    bool EvalOnly,
    bool IndependentAcceptance,
    bool Tp1ExitGateEvaluated,
    bool LocalTeacherCeilingEvaluated,
    bool DpoTrainReady,
    bool TeacherOutputUsedForTraining,
    bool TrainingStarted,
    int DpoStepsExecuted,
    bool CheckpointPromoted,
    bool HumanDecisionRequired);

internal sealed record Tp1EvaluationReviewRunArtifact(
    string Status,
    string ReviewPacketId,
    string OrderingAuthorityRef,
    bool OrderingVerified,
    string ReviewIndexSha256,
    string ReviewRequestSha256,
    string ReviewMarkdownSha256,
    IReadOnlyList<string> PublishedArtifactLeafNames,
    IReadOnlyList<Tp1ReviewSourceArtifactHashes> SourceArtifacts,
    bool EvalOnly,
    bool IndependentAcceptance,
    bool Tp1ExitGateEvaluated,
    bool LocalTeacherCeilingEvaluated,
    bool DpoTrainReady,
    bool TeacherOutputUsedForTraining,
    bool TrainingStarted,
    int DpoStepsExecuted,
    bool CheckpointPromoted,
    bool HumanDecisionRequired);

internal sealed record Tp1EvaluationReviewSuccess(
    bool Ok,
    string Status,
    string ReviewPacketId,
    int RunCount,
    int TotalCaseObservations);
