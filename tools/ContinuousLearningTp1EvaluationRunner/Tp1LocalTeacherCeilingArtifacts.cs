// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearningTp1EvaluationRunner;

internal sealed record Tp1LocalTeacherCeilingPolicyArtifact(
    string PolicyId,
    int WindowSize,
    double MinimumBestImprovement,
    int BeatNumeratorMultiplier,
    int BeatDenominatorThreshold,
    string PlateauOperator,
    string LowBeatAuthorityOperator);

internal sealed record Tp1LocalTeacherCeilingSourcePacketHashes(
    string ReviewRequestSha256,
    string ReviewMarkdownSha256,
    string ReviewRunSha256);

internal sealed record Tp1LocalTeacherCeilingSelectedRun(
    int Ordinal,
    string RunId,
    string WindowRef,
    int CaseCount,
    int StrictTeacherWins,
    double HeldOutAccuracyYgoal);

internal sealed record Tp1LocalTeacherCeilingEvaluationArtifact(
    string Status,
    string EvaluationId,
    Tp1LocalTeacherCeilingPolicyArtifact Policy,
    string SourceReviewPacketId,
    string SourceReviewStatus,
    Tp1LocalTeacherCeilingSourcePacketHashes SourcePacketHashes,
    Tp1ReviewInvariantSet InvariantIdentity,
    IReadOnlyList<Tp1LocalTeacherCeilingSelectedRun> SelectedRuns,
    int SelectedCaseCount,
    long SelectedStrictTeacherWins,
    double FirstHeldOutAccuracyYgoal,
    double BestSubsequentHeldOutAccuracyYgoal,
    double BestImprovement,
    double PooledTeacherBeatRateDescriptive,
    bool PlateauPredicate,
    bool LowBeatAuthorityPredicate,
    bool LocalTeacherCeilingEvaluated,
    bool LocalTeacherCeilingCandidate,
    bool SourceOrderingVerified,
    bool IndependentAcceptance,
    bool Passed,
    bool Tp1ExitGateEvaluated,
    bool Tp2Authorized,
    bool EvalOnly,
    bool DpoTrainReady,
    bool TeacherOutputUsedForTraining,
    bool TrainingStarted,
    int DpoStepsExecuted,
    bool CheckpointPromoted,
    bool HumanDecisionRequired,
    IReadOnlyList<string> ReasonCodes,
    IReadOnlyList<string> Limitations);

internal sealed record Tp1LocalTeacherCeilingEvaluationRunArtifact(
    string Status,
    string EvaluationId,
    string PolicyId,
    string SourceReviewPacketId,
    string EvaluationSha256,
    string MarkdownSha256,
    string ReviewRequestSha256,
    string ReviewMarkdownSha256,
    string ReviewRunSha256,
    IReadOnlyList<string> PublishedArtifactLeafNames,
    string PublishedArtifactRosterSha256,
    bool LocalTeacherCeilingEvaluated,
    bool LocalTeacherCeilingCandidate,
    bool SourceOrderingVerified,
    bool IndependentAcceptance,
    bool Passed,
    bool Tp1ExitGateEvaluated,
    bool Tp2Authorized,
    bool EvalOnly,
    bool DpoTrainReady,
    bool TeacherOutputUsedForTraining,
    bool TrainingStarted,
    int DpoStepsExecuted,
    bool CheckpointPromoted,
    bool HumanDecisionRequired);

internal sealed record Tp1LocalTeacherCeilingSuccess(
    bool Ok,
    string Status,
    string EvaluationId,
    bool LocalTeacherCeilingCandidate,
    int SelectedRunCount,
    int SelectedCaseCount);
