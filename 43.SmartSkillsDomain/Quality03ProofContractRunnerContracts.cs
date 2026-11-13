// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

public sealed record Quality03ProofContractRunnerOptions
{
    public string EvidencePackDirectory { get; init; } = string.Empty;
    public string ProofContractPackDirectory { get; init; } = string.Empty;
    public string? SecondRaterPacketPath { get; init; }
    public string OutputDirectory { get; init; } = string.Empty;
    public string RunId { get; init; } = string.Empty;
    public string WorkOrderId { get; init; } = "QUALITY-03-PROOF-CONTRACT-RUNNER-CODE-IMPLEMENTATION-09";
    public DateTimeOffset GeneratedAtUtc { get; init; } = DateTimeOffset.UtcNow;
    public bool WriteReports { get; init; } = true;
}

public sealed record Quality03ProofContractRunnerValidation(
    bool Passed,
    IReadOnlyList<string> Errors,
    IReadOnlyList<string> Warnings);

public sealed record Quality03ProofContractClaimLocks(
    bool TokenSavingsClaimed = false,
    bool TokenSavingsClaimAllowed = false,
    bool QualityImprovementClaimed = false,
    bool QualityImprovementClaimAllowed = false,
    bool SemanticQualityClaimed = false,
    bool SemanticReadinessClaimed = false,
    bool LearnabilityClaimed = false,
    bool RunnerReadinessClaimed = false,
    bool RuntimeReadinessClaimed = false,
    bool ProductReadinessClaimed = false,
    bool FinalReadinessClaimed = false,
    bool TrainingReadyClaimed = false,
    bool TargetPromotionClaimed = false,
    bool LocatorUpdateClaimed = false);

public sealed record Quality03ProofContractInputRow(
    string RowId,
    string PairSequenceId,
    string PairId,
    string ScenarioId,
    string SourceScenarioId,
    string Host,
    string Method,
    string EvidenceClass,
    string PromptSha256);

public sealed record Quality03ProofContractCategoryDecision(
    string CategoryId,
    string RequiredProofRef,
    string PassOrRisk,
    string ProofStatus,
    bool BlockingRefResolved,
    bool Deferred,
    string? ConcreteProofRef,
    string EvidenceRef,
    IReadOnlyList<string> RiskCodes,
    string DecisionReason);

public sealed record Quality03ProofContractRowDecision(
    string Schema,
    string WorkOrderId,
    string RunId,
    string GeneratedAtUtc,
    string RowId,
    string PairSequenceId,
    string PairId,
    string ScenarioId,
    string SourceScenarioId,
    string Host,
    string Method,
    string EvidenceClass,
    string PromptSha256,
    IReadOnlyList<Quality03ProofContractCategoryDecision> CategoryResults,
    int ResolvedBlockingRefCount,
    int MissingOrDeferredBlockingRefCount,
    bool RunnerReadyCandidate,
    bool QualityPassRow,
    bool RunnerScorerRunAllowed,
    string Decision);

public sealed record Quality03ProofContractRunnerReadyInputRow(
    string Schema,
    string WorkOrderId,
    string RunId,
    string GeneratedAtUtc,
    string RowId,
    string PairSequenceId,
    string PairId,
    string ScenarioId,
    string Host,
    string Method,
    string PromptSha256,
    IReadOnlyList<string> BlockingProofRefs,
    bool QualityImprovementClaimed,
    bool SemanticQualityClaimed,
    bool LearnabilityClaimed,
    bool ProductReadinessClaimed,
    bool ExternalAiModelCalled,
    bool TrainingDpoCheckpointModelLocatorMutation);

public sealed record Quality03ProofContractRunnerReport(
    string Schema,
    string WorkOrderId,
    string RunId,
    string GeneratedAtUtc,
    string EvidencePackDirectory,
    string ProofContractPackDirectory,
    string SecondRaterPacketPath,
    string OutputDirectory,
    Quality03ProofContractRunnerValidation Validation,
    int QualityRowsSeen,
    int RequiredQualityRows,
    int ProofCategoriesRequired,
    int BlockingProofRefsTotal,
    int BlockingProofRefsPass,
    int BlockingProofRefsRiskOrDeferred,
    int RunnerReadyRows,
    int QualityPassRows,
    bool FullProofPass,
    bool RunnerReadyEvalInputsCreated,
    bool RunnerScorerRun,
    bool ExternalAiModelCalled,
    bool TrainingDpoCheckpointModelLocatorMutation,
    Quality03ProofContractClaimLocks ClaimLocks,
    IReadOnlyList<Quality03ProofContractRowDecision> RowDecisions,
    IReadOnlyList<string> MissingOrDeferredRefs,
    IReadOnlyList<string> OutputRefs,
    string DeterministicRunnerLabel,
    string WhatThisProves,
    string WhatThisDoesNotProve,
    string NextBoundedWorkOrder);

public sealed record Quality03ProofContractArtifactRow
{
    public string RowId { get; init; } = string.Empty;
    public string PairSequenceId { get; init; } = string.Empty;
    public string PairId { get; init; } = string.Empty;
    public string ScenarioId { get; init; } = string.Empty;
    public string SourceScenarioId { get; init; } = string.Empty;
    public string Host { get; init; } = string.Empty;
    public string Method { get; init; } = string.Empty;
    public string EvidenceClass { get; init; } = string.Empty;
    public string PromptSha256 { get; init; } = string.Empty;
    public string CategoryId { get; init; } = string.Empty;
    public string ArtifactRef { get; init; } = string.Empty;
    public bool Pass { get; init; }
    public string PassOrRisk { get; init; } = string.Empty;
    public string ProofStatus { get; init; } = string.Empty;
    public string RiskStatus { get; init; } = string.Empty;
    public bool BlockingRefResolved { get; init; }
    public bool Deferred { get; init; }
    public string? ConcreteProofRef { get; init; }
    public bool ExternalAiModelCalled { get; init; }
    public bool TrainingDpoCheckpointModelLocatorMutation { get; init; }
    public bool IndexContractPresent { get; init; }
    public int TopKResultsCount { get; init; }
    public bool RunnerExecuted { get; init; }
    public bool RunnerContractAvailableForCurrentRows { get; init; }
    public bool ShortcutOnly { get; init; }
    public string RunnerStatus { get; init; } = string.Empty;
    public IReadOnlyList<string> ControlsRequired { get; init; } = [];
    public IReadOnlyList<string> ControlsRun { get; init; } = [];
    public bool ReplayProbeExecuted { get; init; }
    public bool PassBoundaryFlagsOnly { get; init; }
    public string ExactMatchScanScope { get; init; } = string.Empty;
    public int? LongestCommonFragmentTokenCount { get; init; }
    public IReadOnlyList<string> ApprovedRawPrivateRefs { get; init; } = [];
    public bool IndependentSecondRaterAvailable { get; init; }
    public string ReviewerId1 { get; init; } = string.Empty;
    public string ReviewerId2 { get; init; } = string.Empty;
    public string AdjudicationStatus { get; init; } = string.Empty;
    public bool FakedIndependence { get; init; }
    public string PassScope { get; init; } = string.Empty;
    public bool CanSatisfyInterRaterAdjudicationProof { get; init; }
    public bool RunnerReadyAfterThisItem { get; init; }
    public bool QualityPassAfterThisItem { get; init; }
    public IReadOnlyList<string> RiskCodes { get; init; } = [];
}

public sealed record Quality03ProofContractApprovedCorpusAuthority(
    bool FilePresent,
    bool Approved,
    string ManifestStatus,
    int RowCount,
    bool HasApprovedTrainSourceCorpusManifestRef,
    bool HasApprovedHeldOutSplitManifestRef,
    bool HasApprovedRawPrivateCorpusRef,
    IReadOnlyDictionary<string, Quality03ProofContractApprovedCorpusRow> RowsById);

public sealed record Quality03ProofContractApprovedCorpusRow(
    string RowId,
    string PairId,
    string Host,
    string Method,
    string PromptSha256,
    string ApprovedAssignment,
    bool SourceDisjointPass);

public sealed record Quality03ProofContractNearestNeighbourIndexAuthority(
    bool FilePresent,
    bool IndexBuilt,
    bool Pass,
    string ManifestStatus,
    string CorpusManifestRef,
    string IndexAlgorithm,
    double? SimilarityThreshold,
    int? TopK);
