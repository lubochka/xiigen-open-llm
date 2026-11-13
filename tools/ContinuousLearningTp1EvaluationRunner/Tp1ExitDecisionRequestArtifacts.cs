// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ContinuousLearningTp1EvaluationRunner;

internal sealed record Tp1ExitDecisionSourceHashes(
    string CeilingEvaluationSha256,
    string CeilingMarkdownSha256,
    string CeilingRunSha256);

internal sealed record Tp1ExitIndependentReviewPrerequisite(
    string AuthorityRole,
    bool Required,
    IReadOnlyList<string> RequiredReceiptFields,
    bool AuthorityEvidencePresent);

internal sealed record Tp1ExitLubaDecisionPrerequisite(
    string AuthorityRole,
    bool Required,
    IReadOnlyList<string> AllowedDecisions,
    bool DecisionEvidencePresent);

internal sealed record Tp1ExitFutureTp2StartRequirements(
    bool SeparateRequestRequired,
    string ProviderTermsProofRequirement,
    string HostedTeacherIdentityRequirement,
    string HumanApprovalEvidenceRequirement);

internal sealed record Tp1ExitDecisionRequestArtifact(
    string Status,
    string DecisionRequestId,
    string SourceCeilingEvaluationId,
    string SourceCeilingStatus,
    string SourcePolicyId,
    Tp1ExitDecisionSourceHashes SourcePacketHashes,
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
    bool SourceLocalTeacherCeilingEvaluated,
    bool SourceLocalTeacherCeilingCandidate,
    IReadOnlyList<string> SourceReasonCodes,
    Tp1ExitIndependentReviewPrerequisite IndependentReviewerPrerequisite,
    Tp1ExitLubaDecisionPrerequisite LubaTp1DecisionPrerequisite,
    Tp1ExitFutureTp2StartRequirements FutureTp2StartRequirements,
    bool HumanDecisionPresent,
    bool SourceRunOrderingVerified,
    bool SourceAuthenticityVerified,
    bool NumericEvidenceVerified,
    bool IndependentAcceptance,
    bool LocalTeacherCeilingAccepted,
    bool Passed,
    bool Tp1ExitGateEvaluated,
    bool Tp2Authorized,
    bool EvalOnly,
    bool DpoTrainReady,
    bool TeacherOutputUsedForTraining,
    bool TrainingStarted,
    int DpoStepsExecuted,
    bool CheckpointPromoted,
    bool PromotionStarted,
    bool ActivationStarted,
    bool HumanDecisionRequired,
    IReadOnlyList<string> Limitations);

internal sealed record Tp1ExitDecisionRequestRunArtifact(
    string Status,
    string DecisionRequestId,
    string SourceCeilingEvaluationId,
    string SourcePolicyId,
    string DecisionRequestSha256,
    string MarkdownSha256,
    string HumanDecisionSubmissionSchemaSha256,
    string CeilingEvaluationSha256,
    string CeilingMarkdownSha256,
    string CeilingRunSha256,
    IReadOnlyList<string> PublishedArtifactLeafNames,
    string PublishedArtifactRosterSha256,
    bool HumanDecisionPresent,
    bool SourceRunOrderingVerified,
    bool SourceAuthenticityVerified,
    bool NumericEvidenceVerified,
    bool IndependentAcceptance,
    bool LocalTeacherCeilingAccepted,
    bool Passed,
    bool Tp1ExitGateEvaluated,
    bool Tp2Authorized,
    bool EvalOnly,
    bool DpoTrainReady,
    bool TeacherOutputUsedForTraining,
    bool TrainingStarted,
    int DpoStepsExecuted,
    bool CheckpointPromoted,
    bool PromotionStarted,
    bool ActivationStarted,
    bool HumanDecisionRequired);

internal sealed record Tp1ExitDecisionRequestSuccess(
    bool Ok,
    string Status,
    string DecisionRequestId,
    string SourceCeilingEvaluationId,
    bool SourceLocalTeacherCeilingCandidate);
