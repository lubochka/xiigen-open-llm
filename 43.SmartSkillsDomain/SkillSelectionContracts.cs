// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace SmartSkillsDomain;

using System.Text;
using System.Text.Json;
using System.Security.Cryptography;
using AgentInfrastructure;
using AgentInfrastructure.Logical;

public sealed record SkillSelectionRequest(
    string Prompt,
    string ProjectRoot,
    IReadOnlyList<string> AvailableSkillIds,
    IReadOnlyDictionary<string, string> HostFacts,
    string UserPromptSourceEntryId);

public sealed record SkillSelectionDecision(
    IReadOnlyList<string> SelectedSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    IReadOnlyList<string> EvidenceFeatures,
    double Confidence,
    bool RequiresSecondPass,
    string DecisionText);

public sealed record CandidateOrderLeakageGuardState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyDictionary<int, int> CandidatePositionWeights,
    bool SemanticQualityClaimed);

public sealed record CandidateOrderLeakageReport(
    string UnitId,
    bool SemanticQualityClaimed,
    bool CanSelectSkill,
    bool OrderSensitivityDetected,
    bool PermutationSelectionInvariant,
    int PermutationCount,
    int CandidatePositionProfileCount,
    int CandidatePositionWeightTotal,
    int RequestCandidateCount,
    int CoveredRequestPositions,
    IReadOnlyList<CandidateOrderPermutationProbe> PermutationProbes,
    IReadOnlyList<string> BlockedFeatureIds,
    IReadOnlyList<string> EvidenceFeatures);

public sealed record CandidateOrderPermutationProbe(
    string ProbeId,
    string OrderLabel,
    int CandidateCount,
    IReadOnlyList<int> SourceCandidatePositions,
    IReadOnlyList<string> SelectedSkillIds,
    bool RequiresSecondPass,
    double Confidence);

public sealed record SkillSelectionMachinePair(
    string PairId,
    SkillSelectionRequest Request,
    SkillSelectionDecision Chosen,
    SkillSelectionDecision Rejected,
    string RejectedType,
    string SourcePromptEntryId,
    string TargetTrainingAdapter);

public sealed record SkillSelectionTrainingResult(
    string BatchId,
    int PairCount,
    bool ContractOnly,
    string GateNote);

public sealed record SkillSelectionCheckpoint(
    int SchemaVersion,
    IReadOnlyDictionary<int, int> CandidatePositionWeights,
    IReadOnlyList<string> TrainedPairIds,
    IReadOnlyList<string> TrainedPromptKeys,
    PromptUnderstandingUnitState PromptUnderstanding,
    TaskUnderstandingUnitState TaskUnderstanding,
    TaskTypeUnitState TaskType,
    RoutingUnitState Routing,
    SkillCandidatesUnitState SkillCandidates,
    SkillSelectionUnitState SkillSelection,
    PromptContextUnitState PromptContext,
    string RouteMode,
    DateTimeOffset SavedAtUtc)
{
    public ResultGapUnitState ResultGap { get; init; } = new(
        UnitId: "result_gap",
        SchemaVersion: 1,
        SkillProfiles: [],
        TrainingPairIds: [],
        SemanticQualityClaimed: false);

    public OutputValidationUnitState OutputValidation { get; init; } = new(
        UnitId: "output_validation",
        SchemaVersion: 1,
        SkillProfiles: [],
        TrainingPairIds: [],
        SemanticQualityClaimed: false);

    public TaskCompletionUnitState TaskCompletion { get; init; } = new(
        UnitId: "task_completion",
        SchemaVersion: 1,
        SkillProfiles: [],
        TrainingPairIds: [],
        SemanticQualityClaimed: false);

    public IntermediateChecksUnitState IntermediateChecks { get; init; } = new(
        UnitId: "intermediate_checks",
        SchemaVersion: 1,
        SkillProfiles: [],
        TrainingPairIds: [],
        SemanticQualityClaimed: false);
}

public sealed record PromptUnderstandingUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<PromptUnderstandingSkillProfile> SkillProfiles,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record PromptUnderstandingSkillProfile(
    string SkillId,
    IReadOnlyDictionary<string, int> LearnedSignals,
    IReadOnlyList<string> SourcePairIds);

public sealed record PromptUnderstandingSkillUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<string> PromptFeatureVocabulary,
    IReadOnlyList<string> SkillVocabulary,
    IReadOnlyList<PromptUnderstandingSkillFeatureProfile> SkillProfiles,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record PromptUnderstandingSkillFeatureProfile(
    string SkillId,
    IReadOnlyDictionary<string, int> PromptFeatureWeights,
    IReadOnlyList<string> SourcePairIds);

public sealed record PromptUnderstandingSkillInput(
    SkillSelectionRequest Request,
    PromptUnderstandingSkillUnitState State);

public sealed record PromptUnderstandingSkillEvidence(
    string UnitId,
    bool HasDecision,
    bool SemanticQualityClaimed,
    bool BaselineCounterOnly,
    IReadOnlyList<string> SelectedSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    IReadOnlyList<string> EvidenceFeatures,
    int LearnedSignalScore,
    double Confidence,
    bool RequiresSecondPass,
    string DecisionText);

public sealed record TaskUnderstandingUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<PromptUnderstandingSkillProfile> SkillProfiles,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record TaskUnderstandingSkillUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<string> TaskFeatureVocabulary,
    IReadOnlyList<string> TaskKindVocabulary,
    IReadOnlyList<string> DeliverableVocabulary,
    IReadOnlyList<string> SkillVocabulary,
    IReadOnlyList<TaskUnderstandingSkillFeatureProfile> SkillProfiles,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record TaskUnderstandingSkillFeatureProfile(
    string SkillId,
    IReadOnlyDictionary<string, int> TaskFeatureWeights,
    IReadOnlyList<string> SourcePairIds);

public sealed record TaskUnderstandingSkillInput(
    SkillSelectionRequest Request,
    TaskUnderstandingSkillUnitState State);

public sealed record TaskUnderstandingSkillEvidence(
    string UnitId,
    bool HasDecision,
    bool SemanticQualityClaimed,
    bool BaselineCounterOnly,
    IReadOnlyList<string> SelectedSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    IReadOnlyList<string> EvidenceFeatures,
    int LearnedTaskSignalScore,
    double Confidence,
    bool RequiresSecondPass,
    string DecisionText);

public sealed record TaskTypeUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<PromptUnderstandingSkillProfile> SkillProfiles,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record TaskTypeSkillUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<string> TaskTypeFeatureVocabulary,
    IReadOnlyList<string> SkillVocabulary,
    IReadOnlyList<TaskTypeSkillProfile> SkillProfiles,
    IReadOnlyList<TaskTypeKnnExample> KnnMemory,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record TaskTypeSkillProfile(
    string SkillId,
    IReadOnlyDictionary<string, int> TaskTypeFeatureWeights,
    IReadOnlyList<string> SourcePairIds);

public sealed record TaskTypeSkillInput(
    SkillSelectionRequest Request,
    TaskTypeSkillUnitState State);

public sealed record TaskTypeKnnExample(
    string ExampleId,
    IReadOnlyList<string> TaskTypeFeatures,
    string ChosenSkillId,
    double Weight);

public sealed record TaskTypeSupport(
    string SkillId,
    double SupportScore,
    int MatchedFeatureCount,
    int NearestExampleCount);

public sealed record TaskTypeSkillEvidence(
    string UnitId,
    bool HasDecision,
    bool SemanticQualityClaimed,
    bool BaselineCounterOnly,
    IReadOnlyList<string> SelectedSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    IReadOnlyList<string> EvidenceFeatures,
    int LearnedTaskTypeSignalScore,
    double Confidence,
    bool RequiresSecondPass,
    string DecisionText);

public sealed record RoutingUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<PromptUnderstandingSkillProfile> SkillProfiles,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record RoutingSkillUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<string> RoutingFeatureVocabulary,
    IReadOnlyList<string> SkillVocabulary,
    IReadOnlyList<RoutingSkillProfile> SkillProfiles,
    IReadOnlyList<RoutingKnnExample> KnnMemory,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record RoutingSkillProfile(
    string SkillId,
    IReadOnlyDictionary<string, int> RoutingFeatureWeights,
    IReadOnlyList<string> SourcePairIds);

public sealed record RoutingSkillInput(
    SkillSelectionRequest Request,
    RoutingSkillUnitState State);

public sealed record RoutingKnnExample(
    string ExampleId,
    IReadOnlyList<string> RoutingFeatures,
    string ChosenSkillId,
    double Weight);

public sealed record RoutingSupport(
    string SkillId,
    double SupportScore,
    int MatchedFeatureCount,
    int NearestExampleCount);

public sealed record RoutingSkillEvidence(
    string UnitId,
    bool HasDecision,
    bool SemanticQualityClaimed,
    bool BaselineCounterOnly,
    IReadOnlyList<string> SelectedSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    IReadOnlyList<string> EvidenceFeatures,
    int LearnedRoutingSignalScore,
    double Confidence,
    bool RequiresSecondPass,
    string DecisionText);

public sealed record SkillCandidatesUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<PromptUnderstandingSkillProfile> SkillProfiles,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record SkillCandidatesSkillUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<string> CandidateFeatureVocabulary,
    IReadOnlyList<string> SkillVocabulary,
    IReadOnlyList<SkillCandidatesSkillProfile> SkillProfiles,
    IReadOnlyList<SkillCandidatesKnnExample> KnnMemory,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record SkillCandidatesSkillProfile(
    string SkillId,
    IReadOnlyDictionary<string, int> CandidateFeatureWeights,
    IReadOnlyList<string> SourcePairIds);

public sealed record SkillCandidatesSkillInput(
    SkillSelectionRequest Request,
    SkillCandidatesSkillUnitState State);

public sealed record SkillCandidatesKnnExample(
    string ExampleId,
    IReadOnlyList<string> CandidateFeatures,
    string ChosenSkillId,
    double Weight);

public sealed record SkillCandidatesSupport(
    string SkillId,
    double SupportScore,
    int MatchedFeatureCount,
    int NearestExampleCount);

public sealed record SkillCandidatesSkillEvidence(
    string UnitId,
    bool HasDecision,
    bool SemanticQualityClaimed,
    bool BaselineCounterOnly,
    IReadOnlyList<string> SelectedSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    IReadOnlyList<string> EvidenceFeatures,
    int LearnedCandidateSignalScore,
    double Confidence,
    bool RequiresSecondPass,
    string DecisionText);

public sealed record SkillSelectionUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<PromptUnderstandingSkillProfile> SkillProfiles,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record SkillSelectionSkillUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<string> FinalSelectionFeatureVocabulary,
    IReadOnlyList<string> SkillVocabulary,
    IReadOnlyList<SkillSelectionSkillProfile> SkillProfiles,
    IReadOnlyList<SkillSelectionKnnExample> KnnMemory,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record SkillSelectionSkillProfile(
    string SkillId,
    IReadOnlyDictionary<string, int> FinalSelectionFeatureWeights,
    IReadOnlyList<string> SourcePairIds);

public sealed record SkillSelectionSkillInput(
    SkillSelectionRequest Request,
    SkillSelectionSkillUnitState State);

public sealed record SkillSelectionKnnExample(
    string ExampleId,
    IReadOnlyList<string> FinalSelectionFeatures,
    string ChosenSkillId,
    double Weight);

public sealed record SkillSelectionSupport(
    string SkillId,
    double SupportScore,
    int MatchedFeatureCount,
    int NearestExampleCount);

public sealed record SkillSelectionSkillEvidence(
    string UnitId,
    bool HasDecision,
    bool SemanticQualityClaimed,
    bool BaselineCounterOnly,
    IReadOnlyList<string> SelectedSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    IReadOnlyList<string> EvidenceFeatures,
    int LearnedFinalSelectionSignalScore,
    double Confidence,
    bool RequiresSecondPass,
    string DecisionText);

public sealed record PromptContextUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<PromptUnderstandingSkillProfile> SkillProfiles,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record PromptContextSkillSelectionUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<string> PromptFeatureVocabulary,
    IReadOnlyList<string> ContextFeatureVocabulary,
    IReadOnlyList<string> SkillVocabulary,
    IReadOnlyList<PromptContextSkillProfile> SkillProfiles,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record PromptContextSkillProfile(
    string SkillId,
    IReadOnlyDictionary<string, int> PromptFeatureWeights,
    IReadOnlyDictionary<string, int> ContextFeatureWeights,
    IReadOnlyList<string> SourcePairIds);

public sealed record PromptContextSkillInput(
    SkillSelectionRequest Request,
    PromptContextSkillSelectionUnitState State);

public sealed record PromptContextSkillEvidence(
    string UnitId,
    bool HasDecision,
    bool SemanticQualityClaimed,
    bool BaselineCounterOnly,
    IReadOnlyList<string> SelectedSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    IReadOnlyList<string> EvidenceFeatures,
    int PromptScore,
    int ContextScore,
    int TotalScore,
    double Confidence,
    bool RequiresSecondPass,
    string DecisionText);

public sealed record ResultGapUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<PromptUnderstandingSkillProfile> SkillProfiles,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record ResultGapSkillUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<string> ResultGapFeatureVocabulary,
    IReadOnlyList<string> SkillVocabulary,
    IReadOnlyList<ResultGapSkillProfile> SkillProfiles,
    IReadOnlyList<ResultGapKnnExample> KnnMemory,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record ResultGapSkillProfile(
    string SkillId,
    IReadOnlyDictionary<string, int> ResultGapFeatureWeights,
    IReadOnlyList<string> SourcePairIds);

public sealed record ResultGapSkillInput(
    SkillSelectionRequest Request,
    ResultGapSkillUnitState State);

public sealed record ResultGapKnnExample(
    string ExampleId,
    IReadOnlyList<string> ResultGapFeatures,
    string ChosenSkillId,
    double Weight);

public sealed record ResultGapSupport(
    string SkillId,
    double SupportScore,
    int MatchedFeatureCount,
    int NearestExampleCount);

public sealed record ResultGapSkillEvidence(
    string UnitId,
    bool HasDecision,
    bool SemanticQualityClaimed,
    bool BaselineCounterOnly,
    IReadOnlyList<string> SelectedSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    IReadOnlyList<string> EvidenceFeatures,
    int LearnedResultGapSignalScore,
    double Confidence,
    bool RequiresSecondPass,
    string DecisionText);

public sealed record OutputValidationUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<PromptUnderstandingSkillProfile> SkillProfiles,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record OutputValidationSkillUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<string> ValidationFeatureVocabulary,
    IReadOnlyList<string> SkillVocabulary,
    IReadOnlyList<OutputValidationSkillProfile> SkillProfiles,
    IReadOnlyList<OutputValidationKnnExample> KnnMemory,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record OutputValidationSkillProfile(
    string SkillId,
    IReadOnlyDictionary<string, int> ValidationFeatureWeights,
    IReadOnlyList<string> SourcePairIds);

public sealed record OutputValidationSkillInput(
    SkillSelectionRequest Request,
    OutputValidationSkillUnitState State);

public sealed record OutputValidationKnnExample(
    string ExampleId,
    IReadOnlyList<string> ValidationFeatures,
    string ChosenSkillId,
    double Weight);

public sealed record OutputValidationSupport(
    string SkillId,
    double SupportScore,
    int MatchedFeatureCount,
    int NearestExampleCount);

public sealed record OutputValidationSkillEvidence(
    string UnitId,
    bool HasDecision,
    bool SemanticQualityClaimed,
    bool BaselineCounterOnly,
    IReadOnlyList<string> SelectedSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    IReadOnlyList<string> EvidenceFeatures,
    int LearnedOutputValidationSignalScore,
    double Confidence,
    bool RequiresSecondPass,
    string DecisionText);

public sealed record TaskCompletionUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<PromptUnderstandingSkillProfile> SkillProfiles,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record TaskCompletionSkillUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<string> CompletionFeatureVocabulary,
    IReadOnlyList<string> SkillVocabulary,
    IReadOnlyList<TaskCompletionSkillProfile> SkillProfiles,
    IReadOnlyList<TaskCompletionKnnExample> KnnMemory,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record TaskCompletionSkillProfile(
    string SkillId,
    IReadOnlyDictionary<string, int> CompletionFeatureWeights,
    IReadOnlyList<string> SourcePairIds);

public sealed record TaskCompletionSkillInput(
    SkillSelectionRequest Request,
    TaskCompletionSkillUnitState State);

public sealed record TaskCompletionKnnExample(
    string ExampleId,
    IReadOnlyList<string> CompletionFeatures,
    string ChosenSkillId,
    double Weight);

public sealed record TaskCompletionSupport(
    string SkillId,
    double SupportScore,
    int MatchedFeatureCount,
    int NearestExampleCount);

public sealed record TaskCompletionSkillEvidence(
    string UnitId,
    bool HasDecision,
    bool SemanticQualityClaimed,
    bool BaselineCounterOnly,
    IReadOnlyList<string> SelectedSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    IReadOnlyList<string> EvidenceFeatures,
    int LearnedTaskCompletionSignalScore,
    double Confidence,
    bool RequiresSecondPass,
    string DecisionText);

public sealed record IntermediateChecksUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<PromptUnderstandingSkillProfile> SkillProfiles,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record IntermediateChecksSkillUnitState(
    string UnitId,
    int SchemaVersion,
    IReadOnlyList<string> CheckFeatureVocabulary,
    IReadOnlyList<string> SkillVocabulary,
    IReadOnlyList<IntermediateChecksSkillProfile> SkillProfiles,
    IReadOnlyList<IntermediateChecksKnnExample> KnnMemory,
    IReadOnlyList<string> TrainingPairIds,
    bool SemanticQualityClaimed);

public sealed record IntermediateChecksSkillProfile(
    string SkillId,
    IReadOnlyDictionary<string, int> CheckFeatureWeights,
    IReadOnlyList<string> SourcePairIds);

public sealed record IntermediateChecksSkillInput(
    SkillSelectionRequest Request,
    IntermediateChecksSkillUnitState State);

public sealed record IntermediateChecksKnnExample(
    string ExampleId,
    IReadOnlyList<string> CheckFeatures,
    string ChosenSkillId,
    double Weight);

public sealed record IntermediateChecksSupport(
    string SkillId,
    double SupportScore,
    int MatchedFeatureCount,
    int NearestExampleCount);

public sealed record IntermediateChecksSkillEvidence(
    string UnitId,
    bool HasDecision,
    bool SemanticQualityClaimed,
    bool BaselineCounterOnly,
    IReadOnlyList<string> SelectedSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    IReadOnlyList<string> EvidenceFeatures,
    int LearnedCheckSignalScore,
    double Confidence,
    bool RequiresSecondPass,
    string DecisionText);

public sealed record SkillSelectionCheckpointManifest(
    int SchemaVersion,
    string MachineId,
    string DomainTag,
    string Version,
    string CheckpointRef,
    string ManifestRef,
    string CheckpointSha256,
    int CheckpointSchemaVersion,
    string RouteMode,
    double AccuracyAtExport,
    bool SemanticQualityClaimed,
    DateTimeOffset ExportedAtUtc);

public sealed record SkillSelectionMachineLoadResult(
    SkillSelectionMachine Machine,
    SkillSelectionCheckpoint Checkpoint,
    string CheckpointRef);

public sealed record ServerSkillLibraryEntry(
    string SkillId,
    string Version,
    string Summary,
    double DefaultWeight,
    string PortabilityLabel = "universal");

public sealed record ProjectLocalSkillState(
    IReadOnlyDictionary<string, string> WikiNotes,
    IReadOnlyDictionary<string, string> Preferences,
    IReadOnlyList<string> Feedback,
    IReadOnlyDictionary<string, double> CustomSkillWeights,
    IReadOnlyList<ServerSkillLibraryEntry> ServerLibraryEntries);

public sealed record SkillSelectionFeedbackEvent(
    string FeedbackId,
    string Prompt,
    IReadOnlyList<string> OriginallySelectedSkillIds,
    IReadOnlyList<string> CorrectedSkillIds,
    string FeedbackText,
    string SourcePromptLedgerId);

public sealed record ProjectLocalSkillFeedbackSignal(
    string PairId,
    string SourceId,
    string Prompt,
    IReadOnlyList<string> ChosenSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    string Reason,
    string TargetTrainingAdapter,
    string SourcePromptLedgerId,
    string StorageScope,
    string RouteMode,
    bool SemanticQualityClaimed,
    bool ContainsPrivateAbsolutePath);

public sealed record SkillSelectionFeedbackWriteResult(
    ProjectLocalSkillState UpdatedProjectLocalState,
    ProjectLocalSkillFeedbackSignal Signal,
    bool UniversalLibraryMutated);

public sealed record SkillLibraryUpgradeRequest(
    IReadOnlyList<ServerSkillLibraryEntry> OldUniversalLibrary,
    IReadOnlyList<ServerSkillLibraryEntry> NewUniversalLibrary,
    ProjectLocalSkillState ProjectLocalState);

public sealed record SkillLibraryPreferencePatchProposal(
    string PatchId,
    string ExistingProjectLocalKey,
    string CandidateUniversalSkillId,
    string Reason,
    string StorageScope,
    bool RequiresUserDecision,
    bool SemanticQualityClaimed);

public sealed record SkillLibraryUpgradeResult(
    ProjectLocalSkillState UpgradedProjectLocalState,
    IReadOnlyList<ServerSkillLibraryEntry> AppliedUniversalLibrary,
    IReadOnlyList<SkillLibraryPreferencePatchProposal> PreferencePatchProposals,
    bool ProjectLocalStatePreserved,
    bool UniversalLibraryMutatedProjectLocalState,
    string StorageScope,
    string EvidenceMode,
    bool SemanticQualityClaimed);

public sealed record ResultGapRequest(
    string Prompt,
    IReadOnlyList<string> SelectedSkillIds,
    string AiOutput,
    string ExpectedMissingSkillId,
    string ObservedMissingSkillId,
    string UserPromptSourceEntryId);

public sealed record ResultGapDpoRow(
    string PairId,
    string Prompt,
    IReadOnlyList<string> ChosenSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    string MissingSkillId,
    string Reason,
    string TargetTrainingAdapter,
    string SourcePromptLedgerId);

public sealed record SecondPassContextUpdate(
    string SourcePromptLedgerId,
    IReadOnlyList<string> RequiredSkillIds,
    string AddedSkillId,
    string Reason);

public sealed record ResultGapResult(
    bool SkillGapDetected,
    string MissingSkillId,
    bool RequiresSecondPass,
    SecondPassContextUpdate? ContextUpdate,
    ResultGapDpoRow? DpoRow,
    IReadOnlyList<string> EvidenceFeatures,
    string EvidenceMode);

public sealed record ResultGapAdaptiveSignalWriteRequest(
    string UserLocalRoot,
    ResultGapResult ResultGap,
    string PromptRef,
    string OutputRef,
    string SourcePromptLedgerId);

public sealed record ResultGapAdaptiveDpoSignal(
    string SignalId,
    string PairId,
    string ChosenRef,
    string RejectedRef,
    IReadOnlyList<string> ChosenSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    string MissingSkillId,
    string ErrorClass,
    string PromptRef,
    string OutputRef,
    string SourcePromptLedgerId,
    string TargetTrainingAdapter,
    string StorageScope,
    string RouteMode,
    bool SemanticQualityClaimed,
    bool ContainsRawPrivateText,
    bool CommonCheckpointMutationClaimed,
    IReadOnlyDictionary<string, string> ProvenanceRefs);

public sealed record ResultGapAdaptiveSignalWriteResult(
    string SignalPath,
    ResultGapAdaptiveDpoSignal Signal,
    bool Written,
    bool CommonCheckpointMutated);

public sealed record ResultGapAdaptiveTrainingRequest(
    string UserLocalRoot,
    string SignalPath);

public sealed record ResultGapAdaptiveSkillProfile(
    string MissingSkillId,
    int SignalCount,
    IReadOnlyList<string> SourceSignalIds,
    IReadOnlyList<string> ChosenRefs,
    IReadOnlyList<string> RejectedRefs);

public sealed record ResultGapAdaptiveCheckpoint(
    int SchemaVersion,
    string Scope,
    IReadOnlyList<ResultGapAdaptiveSkillProfile> MissingPatternProfiles,
    IReadOnlyList<string> TrainedSignalIds,
    bool SemanticQualityClaimed,
    bool CommonCheckpointMutationClaimed,
    DateTimeOffset SavedAtUtc);

public sealed record ResultGapAdaptiveTrainingReport(
    bool Passed,
    string Verdict,
    string CheckpointPath,
    string ReportPath,
    string CheckpointSha256,
    int EligibleSignalCount,
    int TrainedSignalCount,
    int MetricBefore,
    int MetricAfter,
    bool MetricImproved,
    string NoOpReason,
    bool CommonCheckpointMutated,
    bool FreshLoadPassed,
    IReadOnlyList<string> TrainedSignalIds);

public sealed record ResultGapAdaptiveTrainingResult(
    ResultGapAdaptiveCheckpoint Checkpoint,
    ResultGapAdaptiveTrainingReport Report);

public sealed record SkillSelectionCorrectionDpoSignalWriteRequest(
    string UserLocalRoot,
    string CorrectionId,
    IReadOnlyList<string> ChosenSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    string CorrectionClass,
    string PromptRef,
    string OutputRef,
    string AiResponseRef,
    string SourcePromptLedgerId,
    IReadOnlyDictionary<string, string> ProvenanceRefs);

public sealed record SkillSelectionCorrectionAdaptiveDpoSignal(
    string SignalId,
    string PairId,
    string ChosenRef,
    string RejectedRef,
    IReadOnlyList<string> ChosenSkillIds,
    IReadOnlyList<string> RejectedSkillIds,
    string CorrectionClass,
    string PromptRef,
    string OutputRef,
    string AiResponseRef,
    string SourcePromptLedgerId,
    string TargetTrainingAdapter,
    string StorageScope,
    string RouteMode,
    bool SemanticQualityClaimed,
    bool ContainsRawPrivateText,
    bool CommonCheckpointMutationClaimed,
    IReadOnlyDictionary<string, string> ProvenanceRefs);

public sealed record SkillSelectionCorrectionSignalWriteResult(
    string SignalPath,
    SkillSelectionCorrectionAdaptiveDpoSignal Signal,
    bool Written,
    bool CommonCheckpointMutated);

public sealed record SkillSelectionCorrectionAdaptiveTrainingRequest(
    string UserLocalRoot,
    string SignalPath);

public sealed record SkillSelectionCorrectionAdaptiveSkillProfile(
    string CorrectionClass,
    int SignalCount,
    IReadOnlyList<string> SourceSignalIds,
    IReadOnlyList<string> ChosenRefs,
    IReadOnlyList<string> RejectedRefs);

public sealed record SkillSelectionCorrectionAdaptiveCheckpoint(
    int SchemaVersion,
    string Scope,
    IReadOnlyList<SkillSelectionCorrectionAdaptiveSkillProfile> CorrectionProfiles,
    IReadOnlyList<string> TrainedSignalIds,
    bool SemanticQualityClaimed,
    bool CommonCheckpointMutationClaimed,
    DateTimeOffset SavedAtUtc);

public sealed record SkillSelectionCorrectionAdaptiveTrainingReport(
    bool Passed,
    string Verdict,
    string CheckpointPath,
    string ReportPath,
    string CheckpointSha256,
    string FreshLoadCheckpointSha256,
    string ContinueCheckpointSha256,
    int EligibleSignalCount,
    int TrainedSignalCount,
    int MetricBefore,
    int MetricAfter,
    bool MetricImproved,
    string NoOpReason,
    bool CommonCheckpointMutated,
    bool FreshLoadPassed,
    bool ContinueProbePassed,
    IReadOnlyList<string> TrainedSignalIds);

public sealed record SkillSelectionCorrectionAdaptiveTrainingResult(
    SkillSelectionCorrectionAdaptiveCheckpoint Checkpoint,
    SkillSelectionCorrectionAdaptiveTrainingReport Report);

public sealed record TaskCompletionRequest(
    string Prompt,
    string AiOutput,
    ResultGapResult ResultGap);

public sealed record TaskCompletionDecision(
    bool IsComplete,
    bool BlocksFalseDone,
    string Verdict,
    IReadOnlyList<string> EvidenceFeatures);

public sealed class ResultGapMachine
{
    public ResultGapResult Evaluate(ResultGapRequest request)
    {
        var missingSkill = string.IsNullOrWhiteSpace(request.ObservedMissingSkillId)
            ? request.ExpectedMissingSkillId
            : request.ObservedMissingSkillId;
        var gapDetected = !string.IsNullOrWhiteSpace(missingSkill)
            && !request.SelectedSkillIds.Contains(missingSkill, StringComparer.Ordinal);
        if (!gapDetected)
        {
            return new ResultGapResult(
                SkillGapDetected: false,
                MissingSkillId: "",
                RequiresSecondPass: false,
                ContextUpdate: null,
                DpoRow: null,
                EvidenceFeatures: ["BOOTSTRAP_RESULT_GAP_FIXTURE", "NO_SKILL_GAP_DETECTED"],
                EvidenceMode: "bootstrap_test_harness_not_semantic_authority");
        }

        var requiredSkillIds = request.SelectedSkillIds
            .Append(missingSkill)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var contextUpdate = new SecondPassContextUpdate(
            SourcePromptLedgerId: request.UserPromptSourceEntryId,
            RequiredSkillIds: requiredSkillIds,
            AddedSkillId: missingSkill,
            Reason: "Post-output review found a missing skill; second pass must add it before completion.");
        var dpoRow = new ResultGapDpoRow(
            PairId: $"{request.UserPromptSourceEntryId}-missing-{missingSkill}",
            Prompt: request.Prompt,
            ChosenSkillIds: requiredSkillIds,
            RejectedSkillIds: request.SelectedSkillIds.ToArray(),
            MissingSkillId: missingSkill,
            Reason: "Missing skill after output creates adaptive second-pass DPO evidence.",
            TargetTrainingAdapter: "IResultGapTraining",
            SourcePromptLedgerId: request.UserPromptSourceEntryId);

        return new ResultGapResult(
            SkillGapDetected: true,
            MissingSkillId: missingSkill,
            RequiresSecondPass: true,
            ContextUpdate: contextUpdate,
            DpoRow: dpoRow,
            EvidenceFeatures:
            [
                "BOOTSTRAP_RESULT_GAP_FIXTURE",
                "SECOND_PASS_CONTEXT_UPDATE",
                "RESULT_GAP_DPO_ROW",
                "TRAINABLE_REPLACEMENT_REQUIRED_BY_S6_S14"
            ],
            EvidenceMode: "bootstrap_test_harness_not_semantic_authority");
    }
}

public sealed class TaskCompletionMachine
{
    public TaskCompletionDecision Decide(TaskCompletionRequest request)
    {
        if (request.ResultGap.SkillGapDetected || request.ResultGap.RequiresSecondPass)
        {
            return new TaskCompletionDecision(
                IsComplete: false,
                BlocksFalseDone: true,
                Verdict: "BLOCK_FALSE_DONE_SKILL_GAP_REMAINS",
                EvidenceFeatures:
                [
                    "RESULT_GAP_REMAINS",
                    $"missing_skill={request.ResultGap.MissingSkillId}",
                    "SECOND_PASS_REQUIRED"
                ]);
        }

        return new TaskCompletionDecision(
            IsComplete: true,
            BlocksFalseDone: false,
            Verdict: "COMPLETE_NO_SKILL_GAP_REPORTED",
            EvidenceFeatures: ["NO_RESULT_GAP"]);
    }
}

public sealed class ResultGapAdaptiveSignalWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public OperationResult<ResultGapAdaptiveSignalWriteResult> WriteUserLocalSignal(
        ResultGapAdaptiveSignalWriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserLocalRoot))
            return OperationResult<ResultGapAdaptiveSignalWriteResult>.Fail(
                "User-local root is required.",
                "RESULT_GAP_USER_LOCAL_ROOT_REQUIRED");

        if (request.ResultGap?.DpoRow is null || !request.ResultGap.SkillGapDetected)
            return OperationResult<ResultGapAdaptiveSignalWriteResult>.Fail(
                "A missing-pattern ResultGap DPO row is required.",
                "RESULT_GAP_DPO_ROW_REQUIRED");

        if (!IsSafeRef(request.PromptRef) || !IsSafeRef(request.OutputRef))
            return OperationResult<ResultGapAdaptiveSignalWriteResult>.Fail(
                "Redacted prompt and output refs are required; raw private text is not written.",
                "RESULT_GAP_REDACTED_REFS_REQUIRED");

        var signalPath = Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "smart-skills",
            "result-gap",
            "adaptive-dpo-signals.jsonl");
        var signalDirectory = Path.GetDirectoryName(signalPath);
        if (signalDirectory is null)
            return OperationResult<ResultGapAdaptiveSignalWriteResult>.Fail(
                "Could not resolve user-local signal directory.",
                "RESULT_GAP_SIGNAL_DIRECTORY_INVALID");

        var dpo = request.ResultGap.DpoRow;
        var sourceLedger = string.IsNullOrWhiteSpace(request.SourcePromptLedgerId)
            ? dpo.SourcePromptLedgerId
            : request.SourcePromptLedgerId;
        var chosenRef = $"result-gap:chosen:{sourceLedger}:{dpo.MissingSkillId}";
        var rejectedRef = $"result-gap:rejected:{sourceLedger}:{dpo.MissingSkillId}";
        var signal = new ResultGapAdaptiveDpoSignal(
            SignalId: $"result-gap-adaptive-{sourceLedger}-{dpo.MissingSkillId}",
            PairId: dpo.PairId,
            ChosenRef: chosenRef,
            RejectedRef: rejectedRef,
            ChosenSkillIds: dpo.ChosenSkillIds.ToArray(),
            RejectedSkillIds: dpo.RejectedSkillIds.ToArray(),
            MissingSkillId: dpo.MissingSkillId,
            ErrorClass: "missing_pattern_result_gap",
            PromptRef: request.PromptRef,
            OutputRef: request.OutputRef,
            SourcePromptLedgerId: sourceLedger,
            TargetTrainingAdapter: dpo.TargetTrainingAdapter,
            StorageScope: "project-local:.xiigen/smart-skills/result-gap/adaptive-dpo-signals.jsonl",
            RouteMode: "user_local_missing_pattern_result_gap_signal",
            SemanticQualityClaimed: false,
            ContainsRawPrivateText: false,
            CommonCheckpointMutationClaimed: false,
            ProvenanceRefs: new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["prompt_ref"] = request.PromptRef,
                ["output_ref"] = request.OutputRef,
                ["source_prompt_ledger_id"] = sourceLedger,
                ["chosen_ref"] = chosenRef,
                ["rejected_ref"] = rejectedRef
            });

        Directory.CreateDirectory(signalDirectory);
        File.AppendAllText(
            signalPath,
            JsonSerializer.Serialize(signal, JsonOptions) + Environment.NewLine,
            Encoding.UTF8);

        return OperationResult<ResultGapAdaptiveSignalWriteResult>.Ok(
            new ResultGapAdaptiveSignalWriteResult(
                SignalPath: signalPath,
                Signal: signal,
                Written: true,
                CommonCheckpointMutated: false));
    }

    private static bool IsSafeRef(string value)
        => !string.IsNullOrWhiteSpace(value)
            && value.Length <= 128
            && !value.Any(char.IsWhiteSpace)
            && !value.Contains(@":\", StringComparison.Ordinal)
            && !value.Contains("C:/Users/", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("/Users/", StringComparison.OrdinalIgnoreCase)
            && value.Contains("-ref:", StringComparison.Ordinal);
}

public sealed class ResultGapAdaptiveTrainer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public OperationResult<ResultGapAdaptiveTrainingResult> TrainUserLocalAsync(
        ResultGapAdaptiveTrainingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserLocalRoot))
            return OperationResult<ResultGapAdaptiveTrainingResult>.Fail(
                "User-local root is required.",
                "RESULT_GAP_ADAPTIVE_ROOT_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.SignalPath) || !File.Exists(request.SignalPath))
            return OperationResult<ResultGapAdaptiveTrainingResult>.Fail(
                "ResultGap adaptive signal JSONL is required.",
                "RESULT_GAP_ADAPTIVE_SIGNAL_FILE_REQUIRED");

        var signals = File.ReadLines(request.SignalPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<ResultGapAdaptiveDpoSignal>(
                line,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)))
            .Where(signal => signal is not null
                && string.Equals(signal.TargetTrainingAdapter, "IResultGapTraining", StringComparison.Ordinal)
                && !signal.ContainsRawPrivateText
                && !signal.CommonCheckpointMutationClaimed)
            .Select(signal => signal!)
            .ToArray();
        var checkpointPath = Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "smart-skills",
            "result-gap",
            "adaptive-checkpoint.json");
        var reportPath = Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "smart-skills",
            "result-gap",
            "adaptive-training-report.json");
        Directory.CreateDirectory(Path.GetDirectoryName(checkpointPath)!);
        var previousCount = File.Exists(checkpointPath)
            ? LoadCheckpoint(checkpointPath).Value?.TrainedSignalIds.Count ?? 0
            : 0;
        var profiles = signals
            .GroupBy(signal => signal.MissingSkillId, StringComparer.Ordinal)
            .Select(group => new ResultGapAdaptiveSkillProfile(
                MissingSkillId: group.Key,
                SignalCount: group.Count(),
                SourceSignalIds: group.Select(signal => signal.SignalId).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToArray(),
                ChosenRefs: group.Select(signal => signal.ChosenRef).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToArray(),
                RejectedRefs: group.Select(signal => signal.RejectedRef).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToArray()))
            .OrderBy(profile => profile.MissingSkillId, StringComparer.Ordinal)
            .ToArray();
        var checkpoint = new ResultGapAdaptiveCheckpoint(
            SchemaVersion: 1,
            Scope: "project-local:.xiigen/smart-skills/result-gap/adaptive-checkpoint.json",
            MissingPatternProfiles: profiles,
            TrainedSignalIds: signals.Select(signal => signal.SignalId).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToArray(),
            SemanticQualityClaimed: false,
            CommonCheckpointMutationClaimed: false,
            SavedAtUtc: DateTimeOffset.UtcNow);

        File.WriteAllText(checkpointPath, JsonSerializer.Serialize(checkpoint, JsonOptions), Encoding.UTF8);
        var checkpointSha256 = Sha256(checkpointPath);
        var freshLoad = LoadCheckpoint(checkpointPath);
        var metricAfter = checkpoint.TrainedSignalIds.Count;
        var report = new ResultGapAdaptiveTrainingReport(
            Passed: freshLoad.IsSuccess && metricAfter >= previousCount,
            Verdict: metricAfter > previousCount ? "ADAPTIVE_SIGNAL_COUNT_IMPROVED" : "NO_OP_NO_NEW_ELIGIBLE_SIGNALS",
            CheckpointPath: checkpointPath,
            ReportPath: reportPath,
            CheckpointSha256: checkpointSha256,
            EligibleSignalCount: signals.Length,
            TrainedSignalCount: metricAfter,
            MetricBefore: previousCount,
            MetricAfter: metricAfter,
            MetricImproved: metricAfter > previousCount,
            NoOpReason: metricAfter > previousCount ? "" : "No new eligible ResultGap adaptive DPO signals were available.",
            CommonCheckpointMutated: false,
            FreshLoadPassed: freshLoad.IsSuccess,
            TrainedSignalIds: checkpoint.TrainedSignalIds);
        File.WriteAllText(reportPath, JsonSerializer.Serialize(report, JsonOptions), Encoding.UTF8);

        return OperationResult<ResultGapAdaptiveTrainingResult>.Ok(
            new ResultGapAdaptiveTrainingResult(checkpoint, report));
    }

    public OperationResult<ResultGapAdaptiveCheckpoint> LoadCheckpoint(string checkpointPath)
    {
        if (string.IsNullOrWhiteSpace(checkpointPath) || !File.Exists(checkpointPath))
            return OperationResult<ResultGapAdaptiveCheckpoint>.Fail(
                "ResultGap adaptive checkpoint is required.",
                "RESULT_GAP_ADAPTIVE_CHECKPOINT_REQUIRED");

        var checkpoint = JsonSerializer.Deserialize<ResultGapAdaptiveCheckpoint>(
            File.ReadAllText(checkpointPath),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        if (checkpoint is null)
            return OperationResult<ResultGapAdaptiveCheckpoint>.Fail(
                "ResultGap adaptive checkpoint could not be parsed.",
                "RESULT_GAP_ADAPTIVE_CHECKPOINT_INVALID");

        return OperationResult<ResultGapAdaptiveCheckpoint>.Ok(checkpoint);
    }

    private static string Sha256(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
}

public sealed class SkillSelectionCorrectionDpoSignalWriter
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public OperationResult<SkillSelectionCorrectionSignalWriteResult> WriteUserLocalSignal(
        SkillSelectionCorrectionDpoSignalWriteRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserLocalRoot))
            return OperationResult<SkillSelectionCorrectionSignalWriteResult>.Fail(
                "User-local root is required.",
                "CORRECTION_DPO_USER_LOCAL_ROOT_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.CorrectionId)
            || string.IsNullOrWhiteSpace(request.CorrectionClass)
            || string.IsNullOrWhiteSpace(request.SourcePromptLedgerId))
            return OperationResult<SkillSelectionCorrectionSignalWriteResult>.Fail(
                "Correction id, correction class, and source prompt ledger id are required.",
                "CORRECTION_DPO_IDENTITY_REQUIRED");

        if (request.ChosenSkillIds.Count == 0 || request.RejectedSkillIds.Count == 0)
            return OperationResult<SkillSelectionCorrectionSignalWriteResult>.Fail(
                "Chosen and rejected skill ids are required for correction DPO.",
                "CORRECTION_DPO_CHOSEN_REJECTED_REQUIRED");

        if (!IsSafeRef(request.PromptRef) || !IsSafeRef(request.OutputRef) || !IsSafeRef(request.AiResponseRef))
            return OperationResult<SkillSelectionCorrectionSignalWriteResult>.Fail(
                "Prompt, output, and AI response refs must be redacted refs, not raw private text.",
                "CORRECTION_DPO_REDACTED_REFS_REQUIRED");

        var signalPath = Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "smart-skills",
            "corrections",
            "adaptive-dpo-signals.jsonl");
        var signalDirectory = Path.GetDirectoryName(signalPath);
        if (signalDirectory is null)
            return OperationResult<SkillSelectionCorrectionSignalWriteResult>.Fail(
                "Could not resolve user-local correction signal directory.",
                "CORRECTION_DPO_SIGNAL_DIRECTORY_INVALID");

        var chosenRef = $"skill-correction:chosen:{request.SourcePromptLedgerId}:{request.CorrectionId}";
        var rejectedRef = $"skill-correction:rejected:{request.SourcePromptLedgerId}:{request.CorrectionId}";
        var provenance = new Dictionary<string, string>(request.ProvenanceRefs, StringComparer.Ordinal)
        {
            ["prompt_ref"] = request.PromptRef,
            ["output_ref"] = request.OutputRef,
            ["ai_response_ref"] = request.AiResponseRef,
            ["source_prompt_ledger_id"] = request.SourcePromptLedgerId,
            ["chosen_ref"] = chosenRef,
            ["rejected_ref"] = rejectedRef
        };
        var signal = new SkillSelectionCorrectionAdaptiveDpoSignal(
            SignalId: $"skill-correction-adaptive-{request.SourcePromptLedgerId}-{request.CorrectionId}",
            PairId: $"skill-correction-pair-{request.CorrectionId}",
            ChosenRef: chosenRef,
            RejectedRef: rejectedRef,
            ChosenSkillIds: request.ChosenSkillIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
            RejectedSkillIds: request.RejectedSkillIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray(),
            CorrectionClass: request.CorrectionClass,
            PromptRef: request.PromptRef,
            OutputRef: request.OutputRef,
            AiResponseRef: request.AiResponseRef,
            SourcePromptLedgerId: request.SourcePromptLedgerId,
            TargetTrainingAdapter: "IProjectLocalSkillSelectionFeedbackTraining",
            StorageScope: "project-local:.xiigen/smart-skills/corrections/adaptive-dpo-signals.jsonl",
            RouteMode: "user_local_correction_change_signal",
            SemanticQualityClaimed: false,
            ContainsRawPrivateText: false,
            CommonCheckpointMutationClaimed: false,
            ProvenanceRefs: provenance);

        Directory.CreateDirectory(signalDirectory);
        File.AppendAllText(
            signalPath,
            JsonSerializer.Serialize(signal, JsonOptions) + Environment.NewLine,
            Encoding.UTF8);

        return OperationResult<SkillSelectionCorrectionSignalWriteResult>.Ok(
            new SkillSelectionCorrectionSignalWriteResult(
                SignalPath: signalPath,
                Signal: signal,
                Written: true,
                CommonCheckpointMutated: false));
    }

    private static bool IsSafeRef(string value)
        => !string.IsNullOrWhiteSpace(value)
            && value.Length <= 128
            && !value.Any(char.IsWhiteSpace)
            && !value.Contains(@":\", StringComparison.Ordinal)
            && !value.Contains("C:/Users/", StringComparison.OrdinalIgnoreCase)
            && !value.Contains("/Users/", StringComparison.OrdinalIgnoreCase)
            && value.Contains("-ref:", StringComparison.Ordinal);
}

public sealed class SkillSelectionCorrectionAdaptiveTrainer
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    public OperationResult<SkillSelectionCorrectionAdaptiveTrainingResult> TrainUserLocalAsync(
        SkillSelectionCorrectionAdaptiveTrainingRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.UserLocalRoot))
            return OperationResult<SkillSelectionCorrectionAdaptiveTrainingResult>.Fail(
                "User-local root is required.",
                "CORRECTION_ADAPTIVE_ROOT_REQUIRED");

        if (string.IsNullOrWhiteSpace(request.SignalPath) || !File.Exists(request.SignalPath))
            return OperationResult<SkillSelectionCorrectionAdaptiveTrainingResult>.Fail(
                "Correction adaptive signal JSONL is required.",
                "CORRECTION_ADAPTIVE_SIGNAL_FILE_REQUIRED");

        var checkpointPath = Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "smart-skills",
            "corrections",
            "adaptive-checkpoint.json");
        var reportPath = Path.Combine(
            request.UserLocalRoot,
            ".xiigen",
            "smart-skills",
            "corrections",
            "adaptive-training-report.json");
        Directory.CreateDirectory(Path.GetDirectoryName(checkpointPath)!);
        var existingLoad = File.Exists(checkpointPath)
            ? LoadCheckpoint(checkpointPath)
            : OperationResult<SkillSelectionCorrectionAdaptiveCheckpoint>.Fail(
                "No existing correction adaptive checkpoint found; starting from empty user-local state.",
                "CORRECTION_ADAPTIVE_CHECKPOINT_NOT_FOUND");
        var previousCount = existingLoad.IsSuccess
            ? existingLoad.Value!.TrainedSignalIds.Count
            : 0;
        var signals = File.ReadLines(request.SignalPath)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .Select(line => JsonSerializer.Deserialize<SkillSelectionCorrectionAdaptiveDpoSignal>(
                line,
                new JsonSerializerOptions(JsonSerializerDefaults.Web)))
            .Where(signal => signal is not null
                && string.Equals(signal.TargetTrainingAdapter, "IProjectLocalSkillSelectionFeedbackTraining", StringComparison.Ordinal)
                && !signal.ContainsRawPrivateText
                && !signal.CommonCheckpointMutationClaimed)
            .Select(signal => signal!)
            .ToArray();
        var profiles = signals
            .GroupBy(signal => signal.CorrectionClass, StringComparer.Ordinal)
            .Select(group => new SkillSelectionCorrectionAdaptiveSkillProfile(
                CorrectionClass: group.Key,
                SignalCount: group.Count(),
                SourceSignalIds: group.Select(signal => signal.SignalId).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToArray(),
                ChosenRefs: group.Select(signal => signal.ChosenRef).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToArray(),
                RejectedRefs: group.Select(signal => signal.RejectedRef).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToArray()))
            .OrderBy(profile => profile.CorrectionClass, StringComparer.Ordinal)
            .ToArray();
        var checkpoint = new SkillSelectionCorrectionAdaptiveCheckpoint(
            SchemaVersion: 1,
            Scope: "project-local:.xiigen/smart-skills/corrections/adaptive-checkpoint.json",
            CorrectionProfiles: profiles,
            TrainedSignalIds: signals.Select(signal => signal.SignalId).Distinct(StringComparer.Ordinal).OrderBy(id => id, StringComparer.Ordinal).ToArray(),
            SemanticQualityClaimed: false,
            CommonCheckpointMutationClaimed: false,
            SavedAtUtc: DateTimeOffset.UtcNow);

        File.WriteAllText(checkpointPath, JsonSerializer.Serialize(checkpoint, JsonOptions), Encoding.UTF8);
        var checkpointSha256 = Sha256(checkpointPath);
        var freshLoad = LoadCheckpoint(checkpointPath);
        var metricAfter = checkpoint.TrainedSignalIds.Count;
        var report = new SkillSelectionCorrectionAdaptiveTrainingReport(
            Passed: freshLoad.IsSuccess && metricAfter >= previousCount,
            Verdict: metricAfter > previousCount ? "CORRECTION_ADAPTIVE_SIGNAL_COUNT_IMPROVED" : "NO_OP_NO_NEW_ELIGIBLE_CORRECTION_SIGNALS",
            CheckpointPath: checkpointPath,
            ReportPath: reportPath,
            CheckpointSha256: checkpointSha256,
            FreshLoadCheckpointSha256: freshLoad.IsSuccess ? Sha256(checkpointPath) : "",
            ContinueCheckpointSha256: checkpointSha256,
            EligibleSignalCount: signals.Length,
            TrainedSignalCount: metricAfter,
            MetricBefore: previousCount,
            MetricAfter: metricAfter,
            MetricImproved: metricAfter > previousCount,
            NoOpReason: metricAfter > previousCount ? "" : "No new eligible correction adaptive DPO signals were available.",
            CommonCheckpointMutated: false,
            FreshLoadPassed: freshLoad.IsSuccess,
            ContinueProbePassed: freshLoad.IsSuccess && metricAfter >= previousCount,
            TrainedSignalIds: checkpoint.TrainedSignalIds);
        File.WriteAllText(reportPath, JsonSerializer.Serialize(report, JsonOptions), Encoding.UTF8);

        return OperationResult<SkillSelectionCorrectionAdaptiveTrainingResult>.Ok(
            new SkillSelectionCorrectionAdaptiveTrainingResult(checkpoint, report));
    }

    public OperationResult<SkillSelectionCorrectionAdaptiveCheckpoint> LoadCheckpoint(string checkpointPath)
    {
        if (string.IsNullOrWhiteSpace(checkpointPath) || !File.Exists(checkpointPath))
            return OperationResult<SkillSelectionCorrectionAdaptiveCheckpoint>.Fail(
                "Correction adaptive checkpoint is required.",
                "CORRECTION_ADAPTIVE_CHECKPOINT_REQUIRED");

        var checkpoint = JsonSerializer.Deserialize<SkillSelectionCorrectionAdaptiveCheckpoint>(
            File.ReadAllText(checkpointPath),
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        if (checkpoint is null)
            return OperationResult<SkillSelectionCorrectionAdaptiveCheckpoint>.Fail(
                "Correction adaptive checkpoint could not be parsed.",
                "CORRECTION_ADAPTIVE_CHECKPOINT_INVALID");

        return OperationResult<SkillSelectionCorrectionAdaptiveCheckpoint>.Ok(checkpoint);
    }

    private static string Sha256(string path)
        => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path))).ToLowerInvariant();
}

public sealed class ServerSkillLibraryRegistry
{
    public IReadOnlyList<ServerSkillLibraryEntry> LoadUniversalSkills()
        =>
        [
            new ServerSkillLibraryEntry(
                SkillId: "planning-session-startup",
                Version: "1.0.0",
                Summary: "Creates bounded planning work orders from authority requirements.",
                DefaultWeight: 1.0,
                PortabilityLabel: "universal"),
            new ServerSkillLibraryEntry(
                SkillId: "plan-review",
                Version: "1.0.0",
                Summary: "Reviews plans against coverage, contradiction, and OP-45 anti-masquerade gates.",
                DefaultWeight: 1.0,
                PortabilityLabel: "universal"),
            new ServerSkillLibraryEntry(
                SkillId: "human-language-gate",
                Version: "1.0.0",
                Summary: "Keeps user-facing work in the operator's language and with explicit evidence.",
                DefaultWeight: 0.8,
                PortabilityLabel: "universal")
        ];
}

public sealed class SkillSelectionFeedbackWriter
{
    public SkillSelectionFeedbackWriteResult WriteProjectLocalSignal(
        ProjectLocalSkillState currentState,
        SkillSelectionFeedbackEvent feedback)
    {
        var chosen = feedback.CorrectedSkillIds
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var rejected = feedback.OriginallySelectedSkillIds
            .Where(skillId => !chosen.Contains(skillId, StringComparer.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var sourceId = $"{feedback.SourcePromptLedgerId}:{feedback.FeedbackId}";
        var reason = $"Project-local feedback: {feedback.FeedbackText}";
        var signal = new ProjectLocalSkillFeedbackSignal(
            PairId: $"project-local-feedback-{feedback.FeedbackId}",
            SourceId: sourceId,
            Prompt: feedback.Prompt,
            ChosenSkillIds: chosen,
            RejectedSkillIds: rejected,
            Reason: reason,
            TargetTrainingAdapter: "IProjectLocalSkillSelectionFeedbackTraining",
            SourcePromptLedgerId: feedback.SourcePromptLedgerId,
            StorageScope: "project-local:.xiigen/smart-skills/dpo-signals.jsonl",
            RouteMode: "project_local_feedback_signal",
            SemanticQualityClaimed: false,
            ContainsPrivateAbsolutePath: ContainsPrivateAbsolutePath(feedback.Prompt)
                || ContainsPrivateAbsolutePath(reason)
                || ContainsPrivateAbsolutePath(sourceId));
        var feedbackRows = currentState.Feedback
            .Append(feedback.FeedbackText)
            .ToArray();

        return new SkillSelectionFeedbackWriteResult(
            UpdatedProjectLocalState: currentState with { Feedback = feedbackRows },
            Signal: signal,
            UniversalLibraryMutated: false);
    }

    private static bool ContainsPrivateAbsolutePath(string value)
        => value.Contains(@":\", StringComparison.Ordinal)
            || value.Contains("C:/Users/", StringComparison.OrdinalIgnoreCase)
            || value.Contains("/Users/", StringComparison.OrdinalIgnoreCase);
}

public sealed class SkillLibraryUpgradePlanner
{
    public SkillLibraryUpgradeResult PlanUpgrade(SkillLibraryUpgradeRequest request)
    {
        var appliedLibrary = request.NewUniversalLibrary
            .OrderBy(entry => entry.SkillId, StringComparer.Ordinal)
            .ThenBy(entry => entry.Version, StringComparer.Ordinal)
            .ToArray();
        var upgradedState = request.ProjectLocalState with
        {
            ServerLibraryEntries = appliedLibrary
        };
        var patchProposals = BuildPreferencePatchProposals(request).ToArray();

        return new SkillLibraryUpgradeResult(
            UpgradedProjectLocalState: upgradedState,
            AppliedUniversalLibrary: appliedLibrary,
            PreferencePatchProposals: patchProposals,
            ProjectLocalStatePreserved: ProjectLocalStatePreserved(request.ProjectLocalState, upgradedState),
            UniversalLibraryMutatedProjectLocalState: false,
            StorageScope: "project-local:.xiigen/smart-skills/preference-patches.json",
            EvidenceMode: "schema_upgrade_boundary_not_semantic_authority",
            SemanticQualityClaimed: false);
    }

    private static IEnumerable<SkillLibraryPreferencePatchProposal> BuildPreferencePatchProposals(
        SkillLibraryUpgradeRequest request)
    {
        var oldById = request.OldUniversalLibrary
            .GroupBy(entry => entry.SkillId, StringComparer.Ordinal)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.Ordinal);
        var newIds = request.NewUniversalLibrary
            .Select(entry => entry.SkillId)
            .ToHashSet(StringComparer.Ordinal);
        var localKeys = request.ProjectLocalState.WikiNotes.Keys
            .Concat(request.ProjectLocalState.Preferences.Keys)
            .Concat(request.ProjectLocalState.CustomSkillWeights.Keys)
            .Distinct(StringComparer.Ordinal)
            .OrderBy(key => key, StringComparer.Ordinal);

        foreach (var localKey in localKeys)
        {
            if (newIds.Contains(localKey) || !oldById.TryGetValue(localKey, out var oldEntry))
                continue;

            var candidates = request.NewUniversalLibrary
                .Where(entry => SharesMigrationToken(localKey, entry.SkillId)
                    || SharesMigrationToken(oldEntry.Summary, entry.Summary)
                    || SharesMigrationToken(oldEntry.SkillId, entry.Summary))
                .OrderBy(entry => entry.SkillId, StringComparer.Ordinal)
                .ToArray();

            foreach (var candidate in candidates)
            {
                yield return new SkillLibraryPreferencePatchProposal(
                    PatchId: $"project-local-patch-{localKey}-to-{candidate.SkillId}",
                    ExistingProjectLocalKey: localKey,
                    CandidateUniversalSkillId: candidate.SkillId,
                    Reason: "Universal skill identity changed; project-local preference requires an explicit patch proposal instead of overwrite.",
                    StorageScope: "project-local:.xiigen/smart-skills/preference-patches.json",
                    RequiresUserDecision: true,
                    SemanticQualityClaimed: false);
            }
        }
    }

    private static bool ProjectLocalStatePreserved(
        ProjectLocalSkillState original,
        ProjectLocalSkillState upgraded)
        => DictionaryEquals(original.WikiNotes, upgraded.WikiNotes)
            && DictionaryEquals(original.Preferences, upgraded.Preferences)
            && original.Feedback.SequenceEqual(upgraded.Feedback, StringComparer.Ordinal)
            && DictionaryEquals(original.CustomSkillWeights, upgraded.CustomSkillWeights);

    private static bool DictionaryEquals<TValue>(
        IReadOnlyDictionary<string, TValue> left,
        IReadOnlyDictionary<string, TValue> right)
        => left.Count == right.Count
            && left.All(pair => right.TryGetValue(pair.Key, out var value)
                && EqualityComparer<TValue>.Default.Equals(pair.Value, value));

    private static bool SharesMigrationToken(string left, string right)
    {
        var rightTokens = MigrationTokens(right).ToHashSet(StringComparer.OrdinalIgnoreCase);
        return MigrationTokens(left).Any(rightTokens.Contains);
    }

    private static IEnumerable<string> MigrationTokens(string value)
        => value.Split(
                ['-', '_', ' ', '.', ':', ';', '/', '\\', '(', ')', ','],
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(token => token.Length >= 4);
}

public static class ProjectLocalSkillStateRefresh
{
    public static ProjectLocalSkillState ApplyServerLibraryRefresh(
        ProjectLocalSkillState localState,
        IReadOnlyList<ServerSkillLibraryEntry> refreshedServerEntries)
        => localState with
        {
            ServerLibraryEntries = refreshedServerEntries
                .OrderBy(entry => entry.SkillId, StringComparer.Ordinal)
                .ThenBy(entry => entry.Version, StringComparer.Ordinal)
                .ToArray()
        };
}

public interface IMachineTraining<TDomain> : ILearningMachine<TDomain>
{
    Task<OperationResult<SkillSelectionTrainingResult>> TrainMachineBatchAsync(
        IReadOnlyList<SkillSelectionMachinePair> pairs,
        CancellationToken ct = default);
}
