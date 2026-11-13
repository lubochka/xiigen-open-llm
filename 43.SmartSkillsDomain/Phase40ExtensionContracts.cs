// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

public enum Phase40ExtensionHostKind
{
    Unknown = 0,
    Claude = 1,
    Codex = 2,
    Cursor = 3
}

public enum Phase40EvidenceScope
{
    Unknown = 0,
    HostRuntimeUserFacingEntrypoint = 1,
    PackageStaticOrAdapterContractOnly = 2,
    ProductDecisionRequired = 3
}

public enum Phase40CoreDecisionSourceKind
{
    Unknown = 0,
    CoreTrainableSkillSelection = 1,
    CoreCheckSetContract = 2,
    ExactOrStaticEvidenceOnly = 3,
    HostProvidedEvidenceOnly = 4
}

public sealed record Phase40ExtensionApiRequest(
    string RequestId,
    Phase40ExtensionHostKind HostKind,
    string HostRepositoryRoot,
    string UserFacingEntrypoint,
    string ConcreteTask,
    SkillSelectionRequest SkillSelectionRequest,
    Phase40PromptSuiteRequest PromptSuiteRequest,
    Phase40AssetReferenceSet AssetReferences,
    IReadOnlyList<Phase40TraceLogReference> TraceLogReferences);

public sealed record SkillSelectionResponse(
    string ResponseId,
    SkillSelectionDecision Decision,
    Phase40CoreDecisionEvidence DecisionEvidence,
    IReadOnlyList<Phase40CheckSetResult> CheckSetResults,
    Phase40ContractStatus ContractStatus,
    Phase40ExternalToolUsageEvidence ExternalToolUsageEvidence,
    Phase40AssetReferenceSet AssetReferences,
    IReadOnlyList<Phase40TraceLogReference> TraceLogReferences);

public sealed record Phase40PromptSuiteRequest(
    string PromptSuiteId,
    string PromptSuitePath,
    IReadOnlyList<Phase40CheckSetItem> CheckSetItems);

public sealed record Phase40PromptSuiteResult(
    string PromptSuiteId,
    Phase40ExtensionHostKind HostKind,
    IReadOnlyList<Phase40CheckSetResult> Results,
    Phase40ContractStatus ContractStatus,
    IReadOnlyList<Phase40TraceLogReference> TraceLogReferences);

public sealed record Phase40CheckSetItem(
    string CheckId,
    string Prompt,
    string ConcreteTask,
    IReadOnlyList<string> RequiredSkillIds,
    IReadOnlyDictionary<string, string> ExpectedAnswerFacts);

public sealed record Phase40CheckSetResult(
    string CheckId,
    bool Passed,
    string ConcreteAnswer,
    IReadOnlyList<string> SelectedSkillIds,
    string ResultGapDecision,
    string CompletionDecision,
    Phase40CoreDecisionEvidence DecisionEvidence,
    IReadOnlyList<Phase40TraceLogReference> TraceLogReferences);

public sealed record Phase40TraceLogReference(
    string TraceId,
    string RelativePath,
    string Description,
    bool ContainsPrivateAbsolutePath,
    bool ContainsInternalKitchenState);

public sealed record Phase40ContractStatus(
    Phase40EvidenceScope EvidenceScope,
    bool HostRuntimeUnavailable,
    bool FullPhase40ImplementationProven,
    bool ContractOnlyDowngradeRequired,
    string DowngradeReason,
    string NextRequiredAction);

public sealed record Phase40ExternalToolUsageEvidence(
    bool ExternalAiOrUnrelatedToolUsed,
    IReadOnlyList<string> ToolNames,
    string ExplicitApprovalReference,
    string ProhibitionEvidence);

public sealed record Phase40AssetReferenceSet(
    string CommonModelManifestRef,
    string CommonTrainingDataManifestRef,
    string CommonModelLocatorRef,
    string AdaptiveStateRootRef,
    string ProjectLocalStateRef,
    bool CommonAssetsRepoOwned,
    bool AdaptiveStateSeparatedFromHostPackage);

public sealed record Phase40CoreDecisionEvidence(
    Phase40CoreDecisionSourceKind DecisionSourceKind,
    string CoreDecisionReference,
    string ExactOrStaticEvidenceReference,
    bool DeterministicEvidenceUsedAsLearning,
    bool HostProvidedSemanticDecision,
    bool MasqueradeProhibited,
    string ReviewNote);
