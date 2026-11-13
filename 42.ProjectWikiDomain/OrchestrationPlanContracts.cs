// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed record FlowDetectionResult(
    IReadOnlyList<string> AllRefs,
    IReadOnlyList<string> ForbiddenCandidates);

public enum PairMode
{
    Mode1LearnedFlowGrounding,
    Mode2Generalization,
    Mode3HumanExplanation
}

public enum GenerationRouteMode
{
    LearnedFlowGrounding,
    Mode2Generalization,
    Mode3HumanExplanation,
    Passthrough
}

public static class GenerationRouteModeContract
{
    public static bool RequiresMlGenerated(this GenerationRouteMode routeMode) =>
        routeMode is GenerationRouteMode.LearnedFlowGrounding
            or GenerationRouteMode.Mode2Generalization
            or GenerationRouteMode.Mode3HumanExplanation;
}

public enum BoundarySource
{
    Classifier
}

public enum IntentKind
{
    ImplementationExecutionRequest,
    KnownFlowPlanning,
    SkillLoadSessionInit,
    HumanOrchestrationExplanation,
    PhaseContinuation,
    DocumentationMigration,
    ArchitectureReview,
    TestingPlan
}

public sealed record HumanIntentContract(
    IntentKind Kind,
    IReadOnlyList<string> AllowedFlowScope,
    IReadOnlyList<string> ForbiddenFlowScope,
    IReadOnlyList<string> PhaseScope,
    bool RequiresApprovalGate,
    bool RequiresSaveState,
    IReadOnlyList<string> HumanConstraints,
    IReadOnlyList<string> RequestedFlowTextSpans)
{
    public static HumanIntentContract Empty { get; } = new(
        IntentKind.ImplementationExecutionRequest,
        [],
        [],
        [],
        false,
        false,
        [],
        []);
}

public sealed record FlowGroundingResult(
    string FlowRef,
    double Confidence,
    IReadOnlyList<string> EvidenceFeatures,
    IReadOnlyList<string> PredictedPhaseSequence,
    IReadOnlyList<string> PredictedArtifactNames,
    IReadOnlyList<string> PredictedGateSequence);

public sealed record OrchestrationGenerationResult(
    string PlanText,
    GenerationRouteMode RouteMode,
    BoundarySource BoundarySource,
    bool MlGenerated,
    IntentKind IntentKind,
    FlowGroundingResult? FlowGrounding,
    IReadOnlyList<string> Warnings)
{
    public bool ExpectedMlGeneratedByRoute => RouteMode.RequiresMlGenerated();

    public bool SatisfiesMlGeneratedContract => !ExpectedMlGeneratedByRoute || MlGenerated;
}

public sealed record OrchestrationFlowGroundingWeights(
    IReadOnlyDictionary<string, float> InputFeatureWeights,
    IReadOnlyDictionary<string, float> AllowedScopeFeatures,
    IReadOnlyDictionary<string, float> LabelDescriptorFeatures,
    IReadOnlyDictionary<string, float> EvidenceOnlyTargetFeatures,
    IReadOnlyDictionary<string, float> PhaseWeights,
    IReadOnlyDictionary<string, float> ArtifactWeights,
    IReadOnlyDictionary<string, float> GateWeights,
    IReadOnlyDictionary<string, float> StackWeights);

public sealed record OrchestrationPlanChosen(
    string FlowRef,
    string Title,
    string SourcePath,
    string PlanText);

public sealed record OrchestrationTrainingPair(
    string PairId,
    PairMode Mode,
    string Prompt,
    OrchestrationPlanChosen Chosen,
    string RejectedText,
    GenerationRouteMode ExpectedRouteMode,
    BoundarySource ExpectedBoundarySource,
    bool ExpectedMlGenerated,
    IntentKind ExpectedIntentKind,
    string? FlowRef,
    string SourceBundleId,
    bool IsHeldOut);

public sealed record OrchestrationTrainingBatch(
    string BatchId,
    IReadOnlyList<OrchestrationTrainingPair> Pairs);

public sealed record OrchestrationTrainingBatchResult(
    string BatchId,
    int RowsTrained,
    int Mode1RowsTrained,
    int Mode2RowsTrained,
    int Mode3RowsTrained,
    IReadOnlyList<string> TrainedPairIds,
    long CpuMs,
    long PeakRamMb,
    long ElapsedMs);

public sealed record OrchestrationPlanEvaluationGate(
    string EvaluationId,
    int RowsEvaluated,
    double Accuracy,
    bool PassesGate,
    string GateNote,
    DateTimeOffset EvaluatedAtUtc,
    bool SemanticQualityClaimed = false);

public sealed record OrchestrationPlanMachineCheckpoint(
    int SchemaVersion,
    string MachineId,
    string DomainTag,
    string Version,
    DateTimeOffset ExportedAtUtc,
    IReadOnlyList<OrchestrationTrainingPair> FlowGroundingKnn,
    IReadOnlyDictionary<string, OrchestrationFlowGroundingWeights> FlowGroundingWeights,
    IReadOnlyList<OrchestrationTrainingPair> KnnStore,
    IReadOnlyDictionary<string, IReadOnlyList<string>> PatternMap,
    IReadOnlyList<OrchestrationTrainingPair> HumanExplanationKnn,
    IReadOnlyDictionary<string, IReadOnlyList<string>> HumanExplanationPatternMap,
    IReadOnlyList<string> TrainedPairIds,
    IReadOnlyList<string> Mode1PromotedPairIds,
    IReadOnlyList<string> Mode2PromotedPairIds,
    IReadOnlyList<string> Mode3PromotedPairIds,
    OrchestrationLoopCheckpoint? OrchestrationLoop,
    double TrainingProgressScore = 0.0,
    OrchestrationPlanEvaluationGate? LastEvaluationGate = null);

public sealed record OrchestrationPlanMachineLoadResult(
    OrchestrationPlanMachine Machine,
    OrchestrationLoopCheckpoint? Loop);

public sealed record OrchestrationCorpusSources(
    IReadOnlyDictionary<string, OrchestrationPlanChosen> FlowDocs);

public sealed record OrchestrationCorpusAuditReport(
    string SourceDir,
    int TrainingReadyCount,
    int ExplanationReadyCount,
    bool PrivateRawContentWrittenToCommittedArtifacts,
    bool MeetsAuditGate);

public sealed record OrchestrationCorpusBuildReport(
    string SourceDir,
    string OutputPath,
    int TotalPairs,
    int Mode1TrainingPairs,
    int Mode1HeldOutEvalPairs,
    int Mode2TrainingPairs,
    int Mode2HeldOutEvalPairs,
    int Mode3TrainingPairs,
    int Mode3HeldOutEvalPairs,
    int Mode3ExplanationPairs,
    bool NoMode1ExactPromptOverlap,
    bool NoMode2ExactPromptOverlap,
    bool NoMode3ExactPromptOverlap,
    bool NoMode3SourceBundleOverlap,
    bool NoMode3FlowRefOverlap,
    bool MeetsCorpusGate);

public sealed record OrchestrationLoopRunOptions(
    string PrivatePairsPath,
    int TrainSetSize,
    int TestSetSize,
    int Mode3TrainSetSize,
    int MaxCycles,
    double FlowGroundingTarget,
    double StyleTarget,
    double ExplanationTarget,
    string? CheckpointPath,
    bool RequireCheckpointResume,
    string? ReportPath);

public sealed record OrchestrationLoopCheckpoint(
    string TrainingBundleRef,
    int LastCompletedCycle,
    int Cursor,
    IReadOnlyList<string> TrainedPairIds,
    IReadOnlyList<string> Mode1PromotedPairIds,
    IReadOnlyList<string> Mode2PromotedPairIds,
    IReadOnlyList<string> Mode3PromotedPairIds);

public sealed record OrchestrationLoopReport(
    string PrivatePairsPath,
    int TotalPairs,
    int TrainSetSize,
    int TestSetSize,
    int Mode3TrainSetSize,
    int MaxCycles,
    double FlowGroundingTarget,
    double StyleTarget,
    double ExplanationTarget,
    bool CheckpointLoaded,
    bool CheckpointWritten,
    bool TrainedStateLoaded,
    string ResumeMode,
    int ReplayedCycles,
    bool InitialTrainingSkippedDueCheckpoint,
    int ResumeTrainBatchCalls,
    bool TrainedPairIdsUsedForExclusionOnly,
    int TrainedPairIdsRestoredCount,
    int Mode1TrainedPairsRestored,
    int Mode2TrainedPairsRestored,
    int Mode3TrainedPairsRestored,
    int Cursor,
    int LastCompletedCycle,
    int Mode1FlowGroundingKnnSize,
    int Mode1FlowGroundingWeightsSize,
    int Mode2KnnStoreSize,
    int Mode2PatternMapSize,
    int Mode3KnnStoreSize,
    int Mode3HumanExplanationPatternMapSize,
    int Mode3TrainRows,
    double LearnedFlowGroundingAccuracy,
    double OverallTrainingAccuracy,
    double UnseenStyleAccuracy,
    double HumanExplanationAccuracy,
    double ExactPromptLeakageRate,
    IReadOnlyList<string> ExactPromptLeakagePairs,
    double ForbiddenScopeViolationRate,
    double PhaseOnlyCompliance,
    double ApprovalGateCompliance,
    bool PassesGate,
    bool ReachedTarget,
    IReadOnlyList<OrchestrationLoopCycleReport> Cycles,
    IReadOnlyList<OrchestrationFixtureResult> FixtureResults,
    IReadOnlyList<OrchestrationEvalPurityResult> Mode1EvalPurity,
    IReadOnlyList<OrchestrationEvalPurityResult> Mode2EvalPurity,
    IReadOnlyList<OrchestrationEvalPurityResult> Mode3EvalPurity);

public sealed record OrchestrationLoopCycleReport(
    int Cycle,
    int Mode1TrainRows,
    int Mode2TrainRows,
    int Mode3TrainRows,
    int CombinedTrainBatchCalls,
    bool CheckpointWritten,
    double LearnedFlowGroundingAccuracy,
    double OverallTrainingAccuracy,
    double UnseenStyleAccuracy,
    double HumanExplanationAccuracy,
    double ExactPromptLeakageRate,
    double ForbiddenScopeViolationRate,
    double PhaseOnlyCompliance,
    double ApprovalGateCompliance,
    bool PassesTarget);

public sealed record OrchestrationFixtureResult(
    string Fixture,
    GenerationRouteMode RouteMode,
    BoundarySource BoundarySource,
    bool MlGenerated,
    IntentKind IntentKind,
    string? PredictedFlowRef,
    IReadOnlyList<string> Warnings,
    bool Passed);

public sealed record OrchestrationEvalPurityResult(
    string PairId,
    BoundarySource BoundarySource,
    bool MlGenerated,
    GenerationRouteMode RouteMode,
    string? PredictedFlowRef,
    bool Passed);

public static class OrchestrationPlanJsonOptions
{
    public static readonly JsonSerializerOptions Standard = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        Converters = { new JsonStringEnumConverter() }
    };
}
