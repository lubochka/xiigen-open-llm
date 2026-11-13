// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

public sealed record ExtensionEvalOnlyQualityRunnerOptions
{
    public string InputJsonlPath { get; init; } = string.Empty;
    public string OutputDirectory { get; init; } = string.Empty;
    public string RunId { get; init; } = string.Empty;
    public string PhaseId { get; init; } = "QUALITY-02-EVAL-ONLY-RUNNER-IMPLEMENTATION-NO-EVAL-EXECUTION";
    public string EvidenceScope { get; init; } = "local_private_eval_only";
    public DateTimeOffset GeneratedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public bool WriteReports { get; init; } = true;
    public bool SyntheticFixtureOnly { get; init; } = true;
    public bool ActualApprovedEvalExecuted { get; init; }
}

public sealed record ExtensionEvalOnlyQualityRunnerInputRow
{
    public string ScenarioId { get; init; } = string.Empty;
    public string PairId { get; init; } = string.Empty;
    public string Route { get; init; } = string.Empty;
    public string EvidenceScope { get; init; } = string.Empty;
    public string ApprovalRef { get; init; } = string.Empty;
    public string UserLocalRootRef { get; init; } = string.Empty;
    public string RawPrivateInputRef { get; init; } = string.Empty;
    public string RedactedSharedEvidenceRef { get; init; } = string.Empty;
    public string SourceRef { get; init; } = string.Empty;
    public string DocId { get; init; } = string.Empty;
    public string PromptSha256 { get; init; } = string.Empty;
    public string HostName { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
    public string BaselineOutputRef { get; init; } = string.Empty;
    public string AdaptiveOutputRef { get; init; } = string.Empty;
    public string PrivacyRedactionStatus { get; init; } = string.Empty;
    public bool NoCommonMutationAttestation { get; init; }
    public bool NoTrainingMutationAttestation { get; init; }
    public bool NoModelCheckpointLocatorMutationAttestation { get; init; }
    public IReadOnlyList<ExtensionEvalOnlyQualityRunnerLayerInput> LayerInputs { get; init; } = [];
    public IReadOnlyList<string> FailureCodes { get; init; } = [];
}

public sealed record ExtensionEvalOnlyQualityRunnerLayerInput
{
    public string LayerId { get; init; } = string.Empty;
    public int BaselineScore { get; init; }
    public int AdaptiveScore { get; init; }
    public string ReviewerNote { get; init; } = string.Empty;
    public IReadOnlyList<string> EvidenceRefs { get; init; } = [];
    public IReadOnlyList<string> FailureCodes { get; init; } = [];
}

public sealed record ExtensionEvalOnlyQualityRunnerLayerScoreRow(
    string LayerId,
    int BaselineScore,
    int AdaptiveScore,
    string PassFail,
    bool QualityRegressed,
    IReadOnlyList<string> FailureCodes,
    IReadOnlyList<string> EvidenceRefs,
    string ReviewerNote,
    string DeterministicScorerLabel);

public sealed record ExtensionEvalOnlyQualityRunnerOutputRow(
    string Schema,
    string RecordType,
    string PhaseId,
    string RunId,
    string GeneratedAtUtc,
    string ScenarioId,
    string PairId,
    string Route,
    string ScoreStatus,
    string EvidenceScope,
    string Mode,
    string HostName,
    string Method,
    string PromptSha256,
    string BaselineOutputRef,
    string AdaptiveOutputRef,
    IReadOnlyList<ExtensionEvalOnlyQualityRunnerLayerScoreRow> PerLayerScores,
    IReadOnlyList<string> FailureCodes,
    string HumanReviewerNote,
    string InterRaterStatus,
    bool QualityRegression,
    bool QualityImprovementClaimed,
    bool SemanticQualityClaimAllowed,
    string DeterministicScorerLabel,
    ExtensionEvalOnlyQualityRunnerClaimLocks ClaimLocks,
    bool RawPrivateTextCopied,
    bool SyntheticFixtureOnly,
    bool ActualApprovedEvalExecuted,
    bool NoModelHostTrainingExecution,
    int TotalInputRows,
    int ScoredOutputRows,
    int EvidenceOnlyExcludedRows,
    int CommonBlockedExcludedRows);

public sealed record ExtensionEvalOnlyQualityRunnerPairResult(
    string ScenarioId,
    string PairId,
    string Route,
    bool Scored,
    string ScoreStatus,
    string EvidenceScope,
    bool SyntheticFixtureOnly,
    bool ActualApprovedEvalExecuted,
    ExtensionQualityVerdict? Verdict,
    double? AverageAdaptiveScore,
    bool QualityRegression,
    IReadOnlyList<string> FailureCodes,
    string HumanReviewerNote,
    string DeterministicScorerLabel,
    ExtensionEvalOnlyQualityRunnerClaimLocks ClaimLocks,
    ExtensionEvalOnlyQualityRunnerOutputRow? OutputRow);

public sealed record ExtensionEvalOnlyQualityRunnerReport(
    string Schema,
    string PhaseId,
    string RunId,
    string GeneratedAtUtc,
    string EvidenceScope,
    bool SyntheticFixtureOnly,
    bool ActualApprovedEvalExecuted,
    int TotalInputRows,
    int UserAdaptiveEvalOnlyRows,
    int ScoredOutputRows,
    int EvidenceOnlyExcludedRows,
    int CommonBlockedExcludedRows,
    int ValidationErrorCount,
    IReadOnlyList<ExtensionEvalOnlyQualityRunnerPairResult> PairResults,
    IReadOnlyList<ExtensionEvalOnlyQualityRunnerOutputRow> OutputRows,
    ExtensionEvalOnlyQualityRunnerValidation Validation,
    ExtensionEvalOnlyQualityRunnerClaimLocks ClaimLocks,
    IReadOnlyList<string> OutputRefs,
    string HumanBoardRu,
    string WhatThisProves,
    string WhatThisDoesNotProve,
    string NextIndependentTrack,
    bool EvalRunnerExecuted,
    bool HostRuntimeStarted,
    bool TrainingStarted,
    bool ModelCheckpointLocatorMutated);

public sealed record ExtensionEvalOnlyQualityRunnerValidation(
    bool Passed,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings);

public sealed record ExtensionEvalOnlyQualityRunnerClaimLocks(
    bool TokenSavingsClaimed = false,
    bool QualityImprovementClaimed = false,
    bool SemanticQualityClaimed = false,
    bool SemanticReadinessClaimed = false,
    bool LearnabilityClaimed = false,
    bool LiveHostReadinessClaimed = false,
    bool CommonReadinessClaimed = false,
    bool StrictQuality03Ready = false,
    bool TrainingReadyClaimed = false,
    bool TargetPromotionClaimed = false,
    bool LocatorUpdateClaimed = false,
    bool FinalReadinessClaimed = false);
