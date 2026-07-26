// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

namespace ProjectWikiDomain;

using System.Text.Json.Serialization;
using GraphRag.Models;

public enum ProjectWikiRetrievalMode
{
    Mode3LexicalEvidence,
    Mode2SemanticHybrid
}

public enum WikiReadinessStatus
{
    NotIndexed,
    Indexing,
    BasicWikiReady,
    TrainingMissing,
    ImportExportMissing,
    SystemMapMissing,
    AdvancedPromptSuiteMissing,
    AdvancedWikiReady,
    BlockedWithGaps
}

public enum ProjectWikiImportExportStatus
{
    ManifestPresent,
    ManifestDerivableButNotRegistryLinked,
    CheckpointReferenceOnly,
    NotPublishableBaselineOnly,
    Missing
}

public sealed record WikiRequest(
    string RequestId,
    string Prompt,
    string[] ProjectRoots,
    string DocumentationVersion,
    bool RequireAdvancedWiki,
    string[] RequestedEvidenceTypes);

public sealed record WikiAnswer(
    string RequestId,
    WikiReadinessStatus ReadinessStatus,
    string Answer,
    string[] EvidenceIds,
    string[] SourcePaths,
    string MissingEvidence,
    double Confidence,
    string[] Warnings);

public sealed record WikiRelevanceMap(
    string RequestId,
    string[] ProjectRoots,
    string[] RelevantProjects,
    string[] RelevantEvidenceIds,
    string[] MissingEvidenceTypes,
    double RelevanceScore);

public sealed record WikiContextSlice(
    string RequestId,
    string[] EvidenceIds,
    string[] FormattedSections,
    string[] SourcePaths,
    string MissingEvidence);

public sealed record ProjectSystemMap(
    string SystemId,
    string[] ProjectRoots,
    string[] ClientProjectIds,
    string[] ServerProjectIds,
    string[] DatabaseProjectIds,
    string[] CrossStackEdgeIds,
    string[] UnknownBoundaryIds);

public sealed record ProjectWikiUnitManifest(
    string UnitId,
    string UnitVersion,
    string ManifestRef,
    string LocatorKey,
    string RemoteUnitId,
    string CheckpointRef,
    string UpgradePolicy,
    string RollbackRef,
    ProjectWikiImportExportStatus ImportExportStatus);

public sealed record ProjectWikiQuestionRequest(
    string ProjectId,
    string Question,
    string WikiDomainRoot,
    string DocumentationVersion = "",
    int TopK = 5,
    bool AllowSemantic = true);

public enum ProjectWikiQueryIntent
{
    SimpleSymbolLookup,
    EntityImpact,
    FieldAddition,
    FieldRename,
    WorkflowTrace,
    ChangeLocation,
    RegressionRisk,
    ArchitectureBoundary,
    StoredProcedureImpact,
    VersionOrStackSpecificBehavior,
    MissingEvidenceAbstention,
    WrongProjectRejection,
    StaleDocumentationRejection
}

public enum ProjectWikiQueryStepType
{
    SymbolLookup,
    LocationLookup,
    ParameterLookup,
    ReturnLookup,
    CallerLookup,
    CalleeLookup,
    ValueFlowLookup,
    EntityDefinitionLookup,
    DtoLookup,
    MapperLookup,
    ValidatorLookup,
    PersistenceLookup,
    StoredProcedureLookup,
    SqlParameterLookup,
    SqlTableDependencyLookup,
    SqlResultShapeLookup,
    SqlTransactionLookup,
    SqlSideEffectLookup,
    ProjectionOrSerializerLookup,
    ApiContractLookup,
    UiBindingLookup,
    UiComponentLookup,
    UiServiceHttpCallLookup,
    ClientStateLookup,
    CrossRepositoryBoundaryLookup,
    WorkflowEntryLookup,
    WorkflowBranchLookup,
    AuthorizationLookup,
    ConfigurationLookup,
    TestLookup,
    MissingEvidenceCheck,
    WrongProjectCheck,
    StaleVersionCheck
}

public sealed record ProjectWikiUserQuestion(
    string QuestionId,
    string ProjectId,
    string RepositoryRoot,
    string DocumentationVersion,
    string RawUserText,
    string DetectedStack = "",
    string DetectedLanguageVersion = "",
    string[]? KnownProjectAliases = null,
    string[]? PriorContextIds = null);

public sealed record ProjectWikiQueryStep(
    string StepId,
    ProjectWikiQueryStepType StepType,
    string QueryText,
    string TargetIndexOrGraph,
    string[] DependsOnStepIds,
    string WhyNeeded,
    string EvidenceExpected,
    string FailureIfMissing);

public sealed record ProjectWikiQueryPlan(
    string PlanId,
    string ProjectId,
    ProjectWikiQueryIntent Intent,
    string[] Entities,
    string[] Symbols,
    ProjectWikiQueryStep[] RequiredQuerySteps,
    string[] RequiredEvidenceTypes,
    string[] SufficiencyChecks,
    string ExpectedAnswerShape,
    string MissingEvidencePolicy,
    string WrongProjectPolicy,
    string StaleVersionPolicy,
    double ConfidenceBeforeRetrieval,
    double PlanSufficiencyScore,
    double AnswerReadinessScore,
    string[] Warnings);

public sealed record ProjectWikiQueryPlanningDpoPair(
    string PairId,
    string QuestionType,
    ProjectWikiQueryIntent Intent,
    string A,
    string Chosen,
    string Rejected,
    string RejectedType,
    string[] RequiredQueryStepTypes,
    string[] EvidenceIds);

public sealed record ProjectWikiQueryPlanningDpoBatch(
    string BatchId,
    int RowCount,
    int DeveloperReasoningRowCount,
    double RequiredTouchpointRecall,
    double WrongAnnotationRate,
    ProjectWikiQueryPlanningDpoPair[] Pairs,
    string[] Warnings);

public sealed record ProjectWikiQueryPlanningTrainingCaseResult(
    string PairId,
    ProjectWikiQueryIntent ExpectedIntent,
    ProjectWikiQueryIntent ActualIntent,
    bool IntentCorrect,
    bool RequiredTouchpointsCovered,
    bool MissingEvidenceCheckPresent,
    double PlanSufficiencyScore,
    double AnswerReadinessScore);

public sealed record ProjectWikiQueryPlanningTrainingRunResult(
    int TrainingRowCount,
    int HeldOutRowCount,
    double QueryPlanAccuracy,
    double RequiredTouchpointRecall,
    double MultiHopSequenceAccuracy,
    double MissingEvidenceDetection,
    double WrongAnnotationRate,
    double AverageAnswerReadinessScore,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    string Verdict,
    ProjectWikiQueryPlanningTrainingCaseResult[] Cases,
    WikiUnitTrace UnitTrace);

public sealed record ProjectWikiQueryPlanningPublicationResult(
    string UnitId,
    string TrainingRunId,
    string PolicyCheckpointPath,
    string TokenizerPath,
    string GeneratedCapabilityManifestPath,
    string RemoteUnitEndpoint,
    string ManifestSha256,
    bool Registered,
    string[] Warnings);

public sealed record ProjectWikiQueryPlanningTrainingPipelineResult(
    string BatchId,
    ProjectWikiQueryPlanningDpoBatch TrainingBatch,
    ProjectWikiQueryPlanningDpoBatch HeldOutBatch,
    ProjectWikiQueryPlanningTrainingRunResult TrainingRun,
    ProjectWikiQueryPlanningPublicationResult? Publication,
    string NextAction,
    string[] Warnings,
    ProjectWikiDpoMetricRunResult? DpoMetrics = null);

public sealed record ProjectWikiAnswerComposerDpoPair(
    string PairId,
    string QuestionType,
    ProjectWikiQuestionRequest Request,
    ProjectWikiEvidenceRecord? Evidence,
    string A,
    string Chosen,
    string Rejected,
    string RejectedType,
    string[] RequiredEvidenceIds,
    string[] RequiredAnswerSections);

public sealed record ProjectWikiAnswerComposerDpoBatch(
    string BatchId,
    int RowCount,
    int DeveloperReasoningRowCount,
    double WrongAnnotationRate,
    double EvidenceGroundingRate,
    ProjectWikiAnswerComposerDpoPair[] Pairs,
    string[] Warnings);

public sealed record ProjectWikiAnswerComposerTrainingCaseResult(
    string PairId,
    string QuestionType,
    string[] ExpectedEvidenceIds,
    string[] ActualEvidenceIds,
    bool ExactEvidenceHit,
    bool CitationCorrect,
    bool AnswerShapeCorrect,
    bool MissingEvidenceHandled,
    string[] MissingAnswerSections);

public sealed record ProjectWikiAnswerComposerTrainingRunResult(
    int TrainingRowCount,
    int HeldOutRowCount,
    double ExactEvidenceAccuracy,
    double CitationAccuracy,
    double AnswerShapeAccuracy,
    double MissingEvidenceAbstentionAccuracy,
    double WrongAnnotationRate,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    string Verdict,
    ProjectWikiAnswerComposerTrainingCaseResult[] Cases,
    WikiUnitTrace UnitTrace);

public sealed record ProjectWikiAnswerComposerPublicationResult(
    string UnitId,
    string TrainingRunId,
    string PolicyCheckpointPath,
    string TokenizerPath,
    string GeneratedCapabilityManifestPath,
    string RemoteUnitEndpoint,
    string ManifestSha256,
    bool Registered,
    string[] Warnings);

public sealed record ProjectWikiAnswerComposerTrainingPipelineResult(
    string BatchId,
    ProjectWikiAnswerComposerDpoBatch TrainingBatch,
    ProjectWikiAnswerComposerDpoBatch HeldOutBatch,
    ProjectWikiAnswerComposerTrainingRunResult TrainingRun,
    ProjectWikiAnswerComposerPublicationResult? Publication,
    string NextAction,
    string[] Warnings,
    ProjectWikiDpoMetricRunResult? DpoMetrics = null);

public enum ProjectWikiContextRouteKind
{
    Mode3LexicalEvidence,
    Mode2SemanticHybrid,
    MissingEvidenceAbstention,
    WrongProjectRejection,
    StaleDocumentationRejection
}

public sealed record ProjectWikiContextRoutingDecision(
    ProjectWikiContextRouteKind RouteKind,
    string RouteReason,
    bool RequiresSemanticRetriever,
    bool AllowsLexicalFallback,
    double Confidence,
    string[] RequiredPreChecks,
    string[] Warnings);

public sealed record ProjectWikiContextRoutingDpoPair(
    string PairId,
    string QuestionType,
    ProjectWikiUserQuestion UserQuestion,
    ProjectWikiQueryPlan QueryPlan,
    ProjectWikiContextRouteKind ExpectedRouteKind,
    string A,
    string Chosen,
    string Rejected,
    string RejectedType);

public sealed record ProjectWikiContextRoutingDpoBatch(
    string BatchId,
    int RowCount,
    int DeveloperReasoningRowCount,
    double WrongAnnotationRate,
    ProjectWikiContextRoutingDpoPair[] Pairs,
    string[] Warnings);

public sealed record ProjectWikiContextRoutingTrainingCaseResult(
    string PairId,
    ProjectWikiContextRouteKind ExpectedRouteKind,
    ProjectWikiContextRouteKind ActualRouteKind,
    bool RouteCorrect,
    bool PreChecksPresent,
    double Confidence);

public sealed record ProjectWikiContextRoutingTrainingRunResult(
    int TrainingRowCount,
    int HeldOutRowCount,
    double RouteAccuracy,
    double PreCheckAccuracy,
    double WrongAnnotationRate,
    double AverageConfidence,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    string Verdict,
    ProjectWikiContextRoutingTrainingCaseResult[] Cases,
    WikiUnitTrace UnitTrace);

public sealed record ProjectWikiContextRoutingPublicationResult(
    string UnitId,
    string TrainingRunId,
    string PolicyCheckpointPath,
    string TokenizerPath,
    string GeneratedCapabilityManifestPath,
    string RemoteUnitEndpoint,
    string ManifestSha256,
    bool Registered,
    string[] Warnings);

public sealed record ProjectWikiContextRoutingTrainingPipelineResult(
    string BatchId,
    ProjectWikiContextRoutingDpoBatch TrainingBatch,
    ProjectWikiContextRoutingDpoBatch HeldOutBatch,
    ProjectWikiContextRoutingTrainingRunResult TrainingRun,
    ProjectWikiContextRoutingPublicationResult? Publication,
    string NextAction,
    string[] Warnings,
    ProjectWikiDpoMetricRunResult? DpoMetrics = null);

public sealed record NegativeDpoGeneratorBatch(
    string BatchId,
    int RowCount,
    double WrongAnnotationRate,
    double AcceptedCandidateRate,
    double NegativeTypeCoverageRate,
    ProjectDocumentationUnit[] DocumentationUnits,
    NegativeDtoDocumentationCandidate[] Candidates,
    NegativeDtoDocumentationValidation[] Validations,
    ProjectDocumentationDpoPair[] Pairs,
    string[] Warnings);

public sealed record NegativeDpoGeneratorTrainingCaseResult(
    string PairId,
    string NegativeType,
    bool ChosenRejectedDifferent,
    bool WrongAnnotationPresent,
    bool EvidenceIdsPresent,
    bool RejectedOmitsRequiredFact);

public sealed record NegativeDpoGeneratorTrainingRunResult(
    int TrainingRowCount,
    int HeldOutRowCount,
    double ChosenRejectedSeparationRate,
    double WrongAnnotationRate,
    double EvidenceGroundingRate,
    double RejectedDefectAccuracy,
    double NegativeTypeCoverageRate,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    string Verdict,
    NegativeDpoGeneratorTrainingCaseResult[] Cases,
    WikiUnitTrace UnitTrace);

public sealed record NegativeDpoGeneratorPublicationResult(
    string UnitId,
    string TrainingRunId,
    string PolicyCheckpointPath,
    string TokenizerPath,
    string GeneratedCapabilityManifestPath,
    string RemoteUnitEndpoint,
    string ManifestSha256,
    bool Registered,
    string[] Warnings);

public sealed record NegativeDpoGeneratorTrainingPipelineResult(
    string BatchId,
    NegativeDpoGeneratorBatch TrainingBatch,
    NegativeDpoGeneratorBatch HeldOutBatch,
    NegativeDpoGeneratorTrainingRunResult TrainingRun,
    NegativeDpoGeneratorPublicationResult? Publication,
    string NextAction,
    string[] Warnings,
    ProjectWikiDpoMetricRunResult? DpoMetrics = null);

public enum ProjectWikiRequestRouteKind
{
    DocumentationBuild,
    WikiQuestionAnswer,
    ProjectWikiRequestRouterTraining,
    StackVersionTraining,
    SystemBoundaryMapperTraining,
    CrossStackBusinessFlowGraphTraining,
    CodeTraceTraining,
    EntityImpactTraining,
    DocumentationGeneratorTraining,
    QueryPlanningTraining,
    AnswerComposerTraining,
    ContextRoutingTraining,
    NegativeDpoTraining,
    AnswerVerifierTraining,
    TraceAggregatorTraining,
    ProjectValidation,
    NotProjectWiki
}

public enum ProjectWikiRequestRoutingDecisionStatus
{
    Selected,
    NoRoute,
    NeedsSecondPass
}

public sealed record ProjectWikiRequestRoutingInput(
    string Prompt,
    ProjectWikiRequestRoutingModelSnapshot? ModelSnapshot = null);

public sealed record ProjectWikiRequestRoutingGateEvidence(
    string EvaluationId,
    int TrainingRowCount,
    int HeldOutRowCount,
    double RouteAccuracy,
    double WrongAnnotationRate,
    double AverageConfidence,
    bool SemanticQualityClaimed,
    bool HeldOutGatePassed,
    bool PublishAllowed,
    double MinimumRouteAccuracy,
    double MinimumWrongAnnotationRate,
    double MinimumAverageConfidence,
    double MinimumTopTwoMargin,
    double MinimumSelectionConfidence,
    string[] Warnings);

public sealed record ProjectWikiRequestRoutingDecision(
    ProjectWikiRequestRouteKind RouteKind,
    string TargetUnitId,
    string Reason,
    double Confidence,
    string[] RequiredInputs,
    string[] Warnings)
{
    public ProjectWikiRequestRoutingDecisionStatus DecisionStatus { get; init; } = ProjectWikiRequestRoutingDecisionStatus.Selected;
    public ProjectWikiRequestRoutingGateEvidence? GateEvidence { get; init; }
    public double TopTwoMargin { get; init; }
    public bool CanStartTargetRunner => DecisionStatus == ProjectWikiRequestRoutingDecisionStatus.Selected;
}

public sealed record ProjectWikiRequestRoutingDpoPair(
    string PairId,
    string PromptType,
    string Prompt,
    ProjectWikiRequestRouteKind ExpectedRouteKind,
    string A,
    string Chosen,
    string Rejected,
    string RejectedType);

public sealed record ProjectWikiRequestRoutingDpoBatch(
    string BatchId,
    int RowCount,
    double WrongAnnotationRate,
    ProjectWikiRequestRoutingDpoPair[] Pairs,
    string[] Warnings);

public sealed record ProjectWikiRequestRoutingTrainingCaseResult(
    string PairId,
    ProjectWikiRequestRouteKind ExpectedRouteKind,
    ProjectWikiRequestRouteKind ActualRouteKind,
    bool RouteCorrect,
    double Confidence);

public sealed record ProjectWikiRequestRoutingTrainingRunResult(
    int TrainingRowCount,
    int HeldOutRowCount,
    double RouteAccuracy,
    double WrongAnnotationRate,
    double AverageConfidence,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    string Verdict,
    ProjectWikiRequestRoutingTrainingCaseResult[] Cases,
    WikiUnitTrace UnitTrace,
    ProjectWikiRequestRoutingModelSnapshot? ModelSnapshot = null,
    ProjectWikiRequestRoutingGateEvidence? GateEvidence = null);

public sealed record ProjectWikiRequestRoutingModelSnapshot(
    string ModelKind,
    string[] Labels,
    Dictionary<string, double[]> TokenWeights,
    double[] Biases,
    int EpochCount,
    int VocabularySize,
    int TrainingRowCount,
    int HeldOutRowCount,
    string EvaluationId,
    double RouteAccuracy,
    double WrongAnnotationRate,
    double AverageConfidence,
    bool SemanticQualityClaimed,
    bool GatePassed,
    double MinimumRouteAccuracy,
    double MinimumWrongAnnotationRate,
    double MinimumAverageConfidence,
    double MinimumTopTwoMargin,
    double MinimumSelectionConfidence,
    bool PublishAllowed,
    string[] GateWarnings)
{
    public ProjectWikiRequestRoutingGateEvidence ToGateEvidence()
        => new(
            EvaluationId,
            TrainingRowCount,
            HeldOutRowCount,
            RouteAccuracy,
            WrongAnnotationRate,
            AverageConfidence,
            SemanticQualityClaimed,
            GatePassed,
            PublishAllowed,
            MinimumRouteAccuracy,
            MinimumWrongAnnotationRate,
            MinimumAverageConfidence,
            MinimumTopTwoMargin,
            MinimumSelectionConfidence,
            GateWarnings);
}

public sealed record ProjectWikiRequestRoutingPublicationResult(
    string UnitId,
    string TrainingRunId,
    string PolicyCheckpointPath,
    string TokenizerPath,
    string GeneratedCapabilityManifestPath,
    string RemoteUnitEndpoint,
    string ManifestSha256,
    bool Registered,
    string[] Warnings);

public sealed record ProjectWikiRequestRoutingTrainingPipelineResult(
    string BatchId,
    ProjectWikiRequestRoutingDpoBatch TrainingBatch,
    ProjectWikiRequestRoutingDpoBatch HeldOutBatch,
    ProjectWikiRequestRoutingTrainingRunResult TrainingRun,
    ProjectWikiRequestRoutingPublicationResult? Publication,
    string NextAction,
    string[] Warnings,
    ProjectWikiDpoMetricRunResult? DpoMetrics = null);

public sealed record ProjectWikiTrainingOrchestratorResult(
    ProjectWikiRequestRoutingDecision RoutingDecision,
    string BatchId,
    string TargetUnitId,
    string Verdict,
    int TrainingRowCount,
    int HeldOutRowCount,
    double PrimaryAccuracy,
    double SecondaryAccuracy,
    double WrongAnnotationRate,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    string? GeneratedCapabilityManifestPath,
    string NextAction,
    string[] Warnings,
    double? DpoLoss = null,
    double? DpoAccuracy = null,
    double? DpoRewardMargin = null,
    double? DpoHeldOutLoss = null,
    double? DpoHeldOutAccuracy = null,
    double? DpoHeldOutRewardMargin = null,
    string? DpoRunnerReportPath = null,
    double? DpoAverageCpuPercent = null,
    double? DpoCpuTimeMs = null,
    double? DpoPeakWorkingSetMb = null,
    double? DpoTotalElapsedMs = null,
    string? DpoPairDiagnosticsPath = null);

public sealed record ProjectWikiDpoMetricPair(
    string PairId,
    string Prompt,
    string Chosen,
    string Rejected);

public sealed record ProjectWikiDpoMetricBatchSummary(
    int BatchIndex,
    string FirstPairId,
    string LastPairId,
    int PairCount,
    double Loss,
    double Accuracy,
    double RewardMargin,
    double ElapsedMs,
    double CpuPercent,
    double WorkingSetDeltaMb);

public sealed record ProjectWikiDpoMetricPairDiagnostic(
    string PairId,
    string Split,
    string RejectedType,
    double Loss,
    double Accuracy,
    double RewardMargin,
    double ChosenRejectedOverlapRatio,
    int ChosenTokenCount,
    int RejectedTokenCount);

public sealed record ProjectWikiDpoMetricRunResult(
    string RunId,
    string UnitId,
    string BatchId,
    string ResultRoot,
    string SourcePath,
    string CheckpointRoot,
    int EpochCount,
    double LearningRate,
    string PolicyInitialization,
    string ReferenceInitialization,
    double InitialPolicyReferenceMaxAbsDelta,
    string PolicyCheckpointPath,
    string ReferenceCheckpointPath,
    int TrainingRowCount,
    int HeldOutRowCount,
    double FinalLoss,
    double FinalAccuracy,
    double FinalRewardMargin,
    double HeldOutLoss,
    double HeldOutAccuracy,
    double HeldOutRewardMargin,
    double AverageCpuPercent,
    double TotalCpuTimeMs,
    double PeakWorkingSetMb,
    double TotalElapsedMs,
    string PairDiagnosticsPath,
    ProjectWikiDpoMetricBatchSummary[] BatchSummaries);

public sealed record StackAndVersionClassification(
    string RuntimeFamily,
    string TargetFramework,
    string CSharpLanguageVersion,
    string ProjectStyle,
    double Confidence,
    string[] EvidenceMarkers,
    string[] Warnings,
    string ClientLanguage = "",
    string ClientFramework = "",
    string ClientFrameworkVersion = "",
    string ServerLanguage = "",
    string ServerFramework = "",
    string ServerFrameworkVersion = "",
    string DatabaseLanguage = "",
    string DatabaseDialect = "");

public sealed record StackAndVersionDpoPair(
    string PairId,
    string ProjectType,
    string CsprojText,
    string ExpectedRuntimeFamily,
    string ExpectedTargetFramework,
    string ExpectedCSharpLanguageVersion,
    string ExpectedProjectStyle,
    string A,
    string Chosen,
    string Rejected,
    string RejectedType);

public sealed record StackAndVersionDpoBatch(
    string BatchId,
    int RowCount,
    double WrongAnnotationRate,
    StackAndVersionDpoPair[] Pairs,
    string[] Warnings);

public sealed record StackAndVersionTrainingCaseResult(
    string PairId,
    bool RuntimeCorrect,
    bool TargetFrameworkCorrect,
    bool LanguageVersionCorrect,
    bool ProjectStyleCorrect,
    double Confidence);

public sealed record StackAndVersionTrainingRunResult(
    int TrainingRowCount,
    int HeldOutRowCount,
    double RuntimeAccuracy,
    double TargetFrameworkAccuracy,
    double LanguageVersionAccuracy,
    double ProjectStyleAccuracy,
    double WrongAnnotationRate,
    double AverageConfidence,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    string Verdict,
    StackAndVersionTrainingCaseResult[] Cases,
    WikiUnitTrace UnitTrace);

public sealed record StackAndVersionPublicationResult(
    string UnitId,
    string TrainingRunId,
    string PolicyPath,
    string TokenizerPath,
    string GeneratedCapabilityManifestPath,
    string RemoteUnitEndpoint,
    string ManifestSha256,
    bool Registered,
    string[] Warnings);

public sealed record StackAndVersionTrainingPipelineResult(
    string BatchId,
    StackAndVersionDpoBatch TrainingBatch,
    StackAndVersionDpoBatch HeldOutBatch,
    StackAndVersionTrainingRunResult TrainingRun,
    StackAndVersionPublicationResult? Publication,
    string NextAction,
    string[] Warnings);

public sealed record SystemBoundaryProjectEvidence(
    string ProjectId,
    string RepositoryRoot,
    string ProjectRole,
    string Stack,
    string Version,
    string[] RouteOrCallMarkers,
    string[] DataMarkers,
    string[] EvidenceIds);

public sealed record SystemBoundaryEdge(
    string EdgeId,
    string SourceProjectId,
    string TargetProjectId,
    string BoundaryKind,
    string[] SharedMarkers,
    string[] EvidenceIds,
    double Confidence,
    string Status);

public sealed record SystemBoundaryMapResult(
    string SystemId,
    string[] ProjectIds,
    SystemBoundaryEdge[] Edges,
    string[] UnknownBoundaries,
    string[] RequiredFollowUpEvidence,
    double Confidence,
    string[] Warnings);

public sealed record SystemBoundaryMapperDpoPair(
    string PairId,
    SystemBoundaryProjectEvidence[] ProjectEvidence,
    string A,
    string Chosen,
    string Rejected,
    string RejectedType,
    string[] RequiredEdgeIds,
    string[] EvidenceIds);

public sealed record SystemBoundaryMapperDpoBatch(
    string BatchId,
    int RowCount,
    double WrongAnnotationRate,
    SystemBoundaryMapperDpoPair[] Pairs,
    string[] Warnings);

public sealed record SystemBoundaryMapperTrainingCaseResult(
    string PairId,
    double BoundaryEdgeAccuracy,
    double UnknownBoundaryAccuracy,
    double EvidenceGroundingRate,
    double Confidence);

public sealed record SystemBoundaryMapperTrainingRunResult(
    int TrainingRowCount,
    int HeldOutRowCount,
    double BoundaryEdgeAccuracy,
    double UnknownBoundaryAccuracy,
    double EvidenceGroundingRate,
    double WrongAnnotationRate,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    string Verdict,
    SystemBoundaryMapperTrainingCaseResult[] Cases,
    WikiUnitTrace UnitTrace);

public sealed record SystemBoundaryMapperPublicationResult(
    string UnitId,
    string TrainingRunId,
    string PolicyPath,
    string TokenizerPath,
    string GeneratedCapabilityManifestPath,
    string RemoteUnitEndpoint,
    string ManifestSha256,
    bool Registered,
    string[] Warnings);

public sealed record SystemBoundaryMapperTrainingPipelineResult(
    string BatchId,
    SystemBoundaryMapperDpoBatch TrainingBatch,
    SystemBoundaryMapperDpoBatch HeldOutBatch,
    SystemBoundaryMapperTrainingRunResult TrainingRun,
    SystemBoundaryMapperPublicationResult? Publication,
    string NextAction,
    string[] Warnings,
    ProjectWikiDpoMetricRunResult? DpoMetrics = null);

public enum CrossStackBusinessFlowEdgeKind
{
    ClientToServerApi,
    ServerToDatabase,
    ServerToStoredProcedure,
    StoredProcedureToResultShape,
    ServerResponseToClientState,
    UnknownBoundary
}

public sealed record CrossStackBusinessFlowNode(
    string NodeId,
    string ProjectId,
    string NodeKind,
    string Stack,
    string[] EvidenceIds);

public sealed record CrossStackBusinessFlowEdge(
    string EdgeId,
    string SourceNodeId,
    string TargetNodeId,
    CrossStackBusinessFlowEdgeKind EdgeKind,
    string[] EvidenceIds,
    double Confidence,
    string Status);

public sealed record CrossStackBusinessFlowGraph(
    string GraphId,
    string SystemId,
    CrossStackBusinessFlowNode[] Nodes,
    CrossStackBusinessFlowEdge[] Edges,
    string[] ClientToStoredProcedurePaths,
    string[] UnknownPaths,
    double ClientServerEdgeRecall,
    double RepositoryToStoredProcedureEdgeRecall,
    double ClientToStoredProcedurePathRecall,
    string[] Warnings);

public sealed record CrossStackBusinessFlowGraphDpoPair(
    string PairId,
    SystemBoundaryMapResult BoundaryMap,
    string A,
    string Chosen,
    string Rejected,
    string RejectedType,
    string[] RequiredPathIds,
    string[] EvidenceIds);

public sealed record CrossStackBusinessFlowGraphDpoBatch(
    string BatchId,
    int RowCount,
    double WrongAnnotationRate,
    CrossStackBusinessFlowGraphDpoPair[] Pairs,
    string[] Warnings);

public sealed record CrossStackBusinessFlowGraphTrainingCaseResult(
    string PairId,
    double ClientToStoredProcedurePathRecall,
    double ClientServerEdgeRecall,
    double StoredProcedureEdgeRecall,
    double EvidenceGroundingRate,
    double Confidence);

public sealed record CrossStackBusinessFlowGraphTrainingRunResult(
    int TrainingRowCount,
    int HeldOutRowCount,
    double ClientToStoredProcedurePathRecall,
    double ClientServerEdgeRecall,
    double StoredProcedureEdgeRecall,
    double EvidenceGroundingRate,
    double WrongAnnotationRate,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    string Verdict,
    CrossStackBusinessFlowGraphTrainingCaseResult[] Cases,
    WikiUnitTrace UnitTrace);

public sealed record CrossStackBusinessFlowGraphPublicationResult(
    string UnitId,
    string TrainingRunId,
    string PolicyPath,
    string TokenizerPath,
    string GeneratedCapabilityManifestPath,
    string RemoteUnitEndpoint,
    string ManifestSha256,
    bool Registered,
    string[] Warnings);

public sealed record CrossStackBusinessFlowGraphTrainingPipelineResult(
    string BatchId,
    CrossStackBusinessFlowGraphDpoBatch TrainingBatch,
    CrossStackBusinessFlowGraphDpoBatch HeldOutBatch,
    CrossStackBusinessFlowGraphTrainingRunResult TrainingRun,
    CrossStackBusinessFlowGraphPublicationResult? Publication,
    string NextAction,
    string[] Warnings,
    ProjectWikiDpoMetricRunResult? DpoMetrics = null);

public sealed record CodeTraceBuilderAnalysis(
    string ProjectId,
    string SourceId,
    string[] Methods,
    string[] CallEdges,
    string[] ValueFlows,
    double Confidence,
    string[] Warnings);

public sealed record CodeTraceBuilderDpoPair(
    string PairId,
    string ProjectId,
    string SourceId,
    string SourceText,
    string[] ExpectedMethods,
    string[] ExpectedCallEdges,
    string[] ExpectedValueFlows,
    string A,
    string Chosen,
    string Rejected,
    string RejectedType);

public sealed record CodeTraceBuilderDpoBatch(
    string BatchId,
    int RowCount,
    double WrongAnnotationRate,
    CodeTraceBuilderDpoPair[] Pairs,
    string[] Warnings);

public sealed record CodeTraceBuilderTrainingCaseResult(
    string PairId,
    double MethodRecall,
    double CallEdgeRecall,
    double ValueFlowRecall,
    double Confidence);

public sealed record CodeTraceBuilderTrainingRunResult(
    int TrainingRowCount,
    int HeldOutRowCount,
    double MethodRecall,
    double CallEdgeRecall,
    double ValueFlowRecall,
    double WrongAnnotationRate,
    double AverageConfidence,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    string Verdict,
    CodeTraceBuilderTrainingCaseResult[] Cases,
    WikiUnitTrace UnitTrace);

public sealed record CodeTraceBuilderPublicationResult(
    string UnitId,
    string TrainingRunId,
    string PolicyPath,
    string TokenizerPath,
    string GeneratedCapabilityManifestPath,
    string RemoteUnitEndpoint,
    string ManifestSha256,
    bool Registered,
    string[] Warnings);

public sealed record CodeTraceBuilderTrainingPipelineResult(
    string BatchId,
    CodeTraceBuilderDpoBatch TrainingBatch,
    CodeTraceBuilderDpoBatch HeldOutBatch,
    CodeTraceBuilderTrainingRunResult TrainingRun,
    CodeTraceBuilderPublicationResult? Publication,
    string NextAction,
    string[] Warnings,
    ProjectWikiDpoMetricRunResult? DpoMetrics = null);

public sealed record EntityImpactMapResult(
    string ProjectId,
    string EntityName,
    string[] InputTouchpoints,
    string[] ValidationTouchpoints,
    string[] MappingTouchpoints,
    string[] CalculationTouchpoints,
    string[] PersistenceTouchpoints,
    string[] OutputTouchpoints,
    string[] TestTouchpoints,
    string[] UnknownTouchpoints,
    string[] EvidenceIds,
    double Confidence,
    string[] Warnings);

public sealed record EntityImpactMapperDpoPair(
    string PairId,
    string ProjectId,
    string EntityName,
    ProjectWikiEvidenceRecord[] EvidenceRecords,
    string[] ExpectedEvidenceIds,
    string[] ExpectedTouchpoints,
    string A,
    string Chosen,
    string Rejected,
    string RejectedType);

public sealed record EntityImpactMapperDpoBatch(
    string BatchId,
    int RowCount,
    double WrongAnnotationRate,
    EntityImpactMapperDpoPair[] Pairs,
    string[] Warnings);

public sealed record EntityImpactMapperTrainingCaseResult(
    string PairId,
    double EvidenceRecall,
    double TouchpointRecall,
    double Confidence);

public sealed record EntityImpactMapperTrainingRunResult(
    int TrainingRowCount,
    int HeldOutRowCount,
    double EvidenceRecall,
    double TouchpointRecall,
    double WrongAnnotationRate,
    double AverageConfidence,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    string Verdict,
    EntityImpactMapperTrainingCaseResult[] Cases,
    WikiUnitTrace UnitTrace);

public sealed record EntityImpactMapperPublicationResult(
    string UnitId,
    string TrainingRunId,
    string PolicyPath,
    string TokenizerPath,
    string GeneratedCapabilityManifestPath,
    string RemoteUnitEndpoint,
    string ManifestSha256,
    bool Registered,
    string[] Warnings);

public sealed record EntityImpactMapperTrainingPipelineResult(
    string BatchId,
    EntityImpactMapperDpoBatch TrainingBatch,
    EntityImpactMapperDpoBatch HeldOutBatch,
    EntityImpactMapperTrainingRunResult TrainingRun,
    EntityImpactMapperPublicationResult? Publication,
    string NextAction,
    string[] Warnings,
    ProjectWikiDpoMetricRunResult? DpoMetrics = null);

public sealed record DocumentationGeneratorOutput(
    string DocumentationUnitId,
    string Text,
    bool HasMeaning,
    bool HasLocation,
    bool HasParameters,
    bool HasReturnValue,
    bool HasCallers,
    bool HasCallees,
    bool HasStackVersion,
    bool HasEvidenceIds,
    double Confidence,
    string[] Warnings);

public sealed record DocumentationGeneratorDpoPair(
    string PairId,
    ProjectDocumentationUnit Unit,
    string A,
    string Chosen,
    string Rejected,
    string RejectedType);

public sealed record DocumentationGeneratorDpoBatch(
    string BatchId,
    int RowCount,
    double WrongAnnotationRate,
    DocumentationGeneratorDpoPair[] Pairs,
    string[] Warnings);

public sealed record DocumentationGeneratorTrainingCaseResult(
    string PairId,
    bool RequiredFieldsPresent,
    double FieldCoverage,
    double Confidence);

public sealed record DocumentationGeneratorTrainingRunResult(
    int TrainingRowCount,
    int HeldOutRowCount,
    double RequiredFieldCoverage,
    double WrongAnnotationRate,
    double AverageConfidence,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    string Verdict,
    DocumentationGeneratorTrainingCaseResult[] Cases,
    WikiUnitTrace UnitTrace);

public sealed record DocumentationGeneratorPublicationResult(
    string UnitId,
    string TrainingRunId,
    string PolicyPath,
    string TokenizerPath,
    string GeneratedCapabilityManifestPath,
    string RemoteUnitEndpoint,
    string ManifestSha256,
    bool Registered,
    string[] Warnings);

public sealed record DocumentationGeneratorTrainingPipelineResult(
    string BatchId,
    DocumentationGeneratorDpoBatch TrainingBatch,
    DocumentationGeneratorDpoBatch HeldOutBatch,
    DocumentationGeneratorTrainingRunResult TrainingRun,
    DocumentationGeneratorPublicationResult? Publication,
    string NextAction,
    string[] Warnings,
    ProjectWikiDpoMetricRunResult? DpoMetrics = null);

public sealed record ProjectAnswerVerifierDpoPair(
    string PairId,
    ProjectWikiAnswer Answer,
    ProjectWikiEvidenceRecord[] EvidenceRecords,
    string ExpectedProjectId,
    bool ExpectedPassed,
    string[] ExpectedWarnings,
    string A,
    string Chosen,
    string Rejected,
    string RejectedType);

public sealed record ProjectAnswerVerifierDpoBatch(
    string BatchId,
    int RowCount,
    double WrongAnnotationRate,
    ProjectAnswerVerifierDpoPair[] Pairs,
    string[] Warnings);

public sealed record ProjectAnswerVerifierTrainingCaseResult(
    string PairId,
    bool PassFailCorrect,
    bool WarningSetCorrect,
    double CitationAccuracy);

public sealed record ProjectAnswerVerifierTrainingRunResult(
    int TrainingRowCount,
    int HeldOutRowCount,
    double PassFailAccuracy,
    double WarningSetAccuracy,
    double AverageCitationAccuracy,
    double WrongAnnotationRate,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    string Verdict,
    ProjectAnswerVerifierTrainingCaseResult[] Cases,
    WikiUnitTrace UnitTrace);

public sealed record ProjectAnswerVerifierPublicationResult(
    string UnitId,
    string TrainingRunId,
    string PolicyPath,
    string TokenizerPath,
    string GeneratedCapabilityManifestPath,
    string RemoteUnitEndpoint,
    string ManifestSha256,
    bool Registered,
    string[] Warnings);

public sealed record ProjectAnswerVerifierTrainingPipelineResult(
    string BatchId,
    ProjectAnswerVerifierDpoBatch TrainingBatch,
    ProjectAnswerVerifierDpoBatch HeldOutBatch,
    ProjectAnswerVerifierTrainingRunResult TrainingRun,
    ProjectAnswerVerifierPublicationResult? Publication,
    string NextAction,
    string[] Warnings,
    ProjectWikiDpoMetricRunResult? DpoMetrics = null);

public sealed record WikiTraceAggregationResult(
    string BatchId,
    int QueryTraceCount,
    int UnitTraceCount,
    double QueryPassRate,
    string[] FailedUnits,
    string[] RemediationActions,
    bool StopBlindTraining,
    double Confidence,
    string[] Warnings);

public sealed record WikiTraceAggregatorDpoPair(
    string PairId,
    WikiQueryTrace[] QueryTraces,
    WikiUnitTrace[] UnitTraces,
    string[] ExpectedFailedUnits,
    string[] ExpectedRemediationActions,
    bool ExpectedStopBlindTraining,
    string A,
    string Chosen,
    string Rejected,
    string RejectedType);

public sealed record WikiTraceAggregatorDpoBatch(
    string BatchId,
    int RowCount,
    double WrongAnnotationRate,
    WikiTraceAggregatorDpoPair[] Pairs,
    string[] Warnings);

public sealed record WikiTraceAggregatorTrainingCaseResult(
    string PairId,
    bool FailedUnitsCorrect,
    bool RemediationCorrect,
    bool StopDecisionCorrect,
    double Confidence);

public sealed record WikiTraceAggregatorTrainingRunResult(
    int TrainingRowCount,
    int HeldOutRowCount,
    double FailedUnitAccuracy,
    double RemediationAccuracy,
    double StopDecisionAccuracy,
    double WrongAnnotationRate,
    double AverageConfidence,
    double CpuMilliseconds,
    double PeakWorkingSetMb,
    double ElapsedMilliseconds,
    string Verdict,
    WikiTraceAggregatorTrainingCaseResult[] Cases,
    WikiUnitTrace UnitTrace);

public sealed record WikiTraceAggregatorPublicationResult(
    string UnitId,
    string TrainingRunId,
    string PolicyPath,
    string TokenizerPath,
    string GeneratedCapabilityManifestPath,
    string RemoteUnitEndpoint,
    string ManifestSha256,
    bool Registered,
    string[] Warnings);

public sealed record WikiTraceAggregatorTrainingPipelineResult(
    string BatchId,
    WikiTraceAggregatorDpoBatch TrainingBatch,
    WikiTraceAggregatorDpoBatch HeldOutBatch,
    WikiTraceAggregatorTrainingRunResult TrainingRun,
    WikiTraceAggregatorPublicationResult? Publication,
    string NextAction,
    string[] Warnings,
    ProjectWikiDpoMetricRunResult? DpoMetrics = null);

public sealed record ProjectWikiQuestionSeed(
    string QuestionType,
    string Question,
    string ExpectedEvidenceId,
    string ExpectedProjectId,
    string DocumentationLevel,
    string SourcePath);

public sealed record ProjectWikiEvidenceRecord
{
    [JsonPropertyName("evidence_id")]
    public string EvidenceId { get; init; } = "";

    [JsonPropertyName("project_id")]
    public string ProjectId { get; init; } = "";

    [JsonPropertyName("documentation_version")]
    public string DocumentationVersion { get; init; } = "";

    [JsonPropertyName("symbol_id")]
    public string SymbolId { get; init; } = "";

    [JsonPropertyName("relation_symbol_id")]
    public string RelationSymbolId { get; init; } = "";

    [JsonPropertyName("doc_path")]
    public string DocPath { get; init; } = "";

    [JsonPropertyName("source_path")]
    public string SourcePath { get; init; } = "";

    [JsonPropertyName("source_line")]
    public int SourceLine { get; init; }

    [JsonPropertyName("documentation_level")]
    public string DocumentationLevel { get; init; } = "";

    [JsonPropertyName("target_framework")]
    public string TargetFramework { get; init; } = "";

    [JsonPropertyName("csharp_language_version")]
    public string CSharpLanguageVersion { get; init; } = "";

    [JsonPropertyName("meaning")]
    public string Meaning { get; init; } = "";

    [JsonPropertyName("parameters")]
    public string[] Parameters { get; init; } = [];

    [JsonPropertyName("returns")]
    public string Returns { get; init; } = "";

    [JsonPropertyName("called_functions")]
    public string[] CalledFunctions { get; init; } = [];

    [JsonPropertyName("called_by")]
    public string[] CalledBy { get; init; } = [];

    [JsonPropertyName("known_callees")]
    public string[] KnownCallees { get; init; } = [];

    [JsonPropertyName("stored_procedure_name")]
    public string StoredProcedureName { get; init; } = "";

    [JsonPropertyName("sql_parameters")]
    public string[] SqlParameters { get; init; } = [];

    [JsonPropertyName("sql_tables_read")]
    public string[] SqlTablesRead { get; init; } = [];

    [JsonPropertyName("sql_tables_written")]
    public string[] SqlTablesWritten { get; init; } = [];

    [JsonPropertyName("sql_result_columns")]
    public string[] SqlResultColumns { get; init; } = [];

    [JsonPropertyName("sql_transaction_behavior")]
    public string SqlTransactionBehavior { get; init; } = "";

    [JsonPropertyName("sql_side_effects")]
    public string[] SqlSideEffects { get; init; } = [];
}

public sealed record ProjectWikiEvidenceHit(
    ProjectWikiEvidenceRecord Evidence,
    double Score,
    string Source);

public sealed record ProjectWikiRetrievedContext(
    ProjectWikiRetrievalMode Mode,
    IReadOnlyList<ProjectWikiEvidenceHit> EvidenceHits,
    RetrievalContext? SemanticContext,
    bool SemanticUnavailable,
    string[] Warnings);

public sealed record ProjectWikiAnswer(
    string ProjectId,
    string DocumentationVersion,
    string Answer,
    string[] EvidenceIds,
    string[] SourcePaths,
    double Confidence,
    string MissingEvidence,
    ProjectWikiRetrievalMode RetrievalMode,
    string[] Warnings);

public sealed record WikiUnitTraceResult(
    string UnitId,
    string UnitVersion,
    string TrainedCheckpointId,
    string InputSummary,
    string OutputSummary,
    double QualityScore,
    string PassOrFail,
    string FailureType,
    string RemediationAction);

public sealed record WikiQueryTrace(
    string QueryId,
    string ProjectId,
    string DocumentationVersion,
    string QuestionType,
    string ExpectedLevel,
    bool QueryPlanningFlowInvoked,
    string QueryPlanId,
    string PlannedIntent,
    string[] PlannedEntities,
    string[] PlannedQuerySteps,
    string[] ExecutedQuerySteps,
    string[] SkippedQuerySteps,
    string[] MissingEvidenceFromPlan,
    double PlanSufficiencyScore,
    double AnswerReadinessScore,
    string[] RoutedUnits,
    WikiUnitTraceResult[] UnitResults,
    string[] RetrievedEvidenceIds,
    string[] SelectedAnswerEvidenceIds,
    string[] MissingEvidence,
    double Confidence,
    string VerifierResult,
    double CpuMs,
    double ElapsedMs,
    double PeakRamMb,
    string FinalStatus);

public sealed record WikiUnitTrace(
    string BatchId,
    int RowsTrained,
    int HeldOutRows,
    WikiUnitTraceResult[] UnitMetrics,
    string[] FailedUnits,
    string[] RemediationBatchesRequired,
    double CoverageDelta,
    bool StopBlindTraining);

public sealed record ProjectWikiAnswerTraceResult(
    ProjectWikiAnswer Answer,
    ProjectWikiQueryPlan QueryPlan,
    WikiQueryTrace Trace);

public sealed record ProjectWikiVerificationResult(
    bool IsPassed,
    string[] InvalidEvidenceIds,
    string[] Warnings,
    double CitationAccuracy,
    bool IsProjectBoundaryPassed);

public sealed record ProjectDocumentationRequest(
    string ProjectRoot,
    string? WikiDomainRoot = null,
    string DocumentationVersion = "",
    bool PersistArtifacts = true);

public sealed record ProjectDocumentationResult(
    string ProjectId,
    string WikiDomainRoot,
    string DocumentationVersion,
    int MethodCount,
    int CallEdgeCount,
    int ValueFlowEdgeCount,
    int QuestionSeedCount,
    int NegativeDpoPairCount,
    double NegativeDtoAcceptanceRate,
    string[] Warnings,
    ProjectDocumentationKnowledgeGraph KnowledgeGraph);

public sealed record ProjectDocumentationKnowledgeGraph(
    string ProjectId,
    string RepositoryRoot,
    string DocumentationVersion,
    ProjectCodeTrace CodeTrace,
    IReadOnlyList<ProjectDocumentationUnit> DocumentationUnits,
    IReadOnlyList<ProjectDocumentationCallEdge> CallEdges,
    IReadOnlyList<ProjectDocumentationValueFlow> ValueFlows,
    IReadOnlyList<ProjectWikiEvidenceRecord> EvidenceRecords,
    IReadOnlyList<ProjectWikiQuestionSeed> QuestionSeeds,
    NegativeDtoDocumentationBundle NegativeDpo);

public sealed record ProjectCodeTrace(
    string ProjectId,
    string RepositoryRoot,
    string DocumentationVersion,
    string Status,
    string[] TargetFrameworks,
    string[] CSharpLanguageVersions,
    int SourceFileCount,
    int MethodCount,
    int CallEdgeCount,
    int ValueFlowEdgeCount,
    int UnresolvedCallCount,
    int ExternalCallCount,
    string[] Warnings);

public sealed record ProjectDocumentationPipelineRequest(
    string ProjectRoot,
    string? WikiDomainRoot = null,
    string DocumentationVersion = "",
    bool PersistArtifacts = false,
    bool VerifyCoverage = true);

public sealed record ProjectDocumentationPipelineResult(
    string ProjectId,
    string DocumentationVersion,
    int MethodCount,
    int CallEdgeCount,
    int ValueFlowEdgeCount,
    int NegativeDpoPairCount,
    double NegativeDtoAcceptanceRate,
    ProjectWikiCoverageReport? CoverageReport,
    ProjectWikiValidationProjectReport? ValidationReport,
    ProjectDocumentationKnowledgeGraph KnowledgeGraph,
    string[] Warnings);

public sealed record ProjectDocumentationUnit(
    string DocumentationUnitId,
    string ProjectId,
    string RepositoryRoot,
    string SourcePath,
    int SourceLine,
    string SymbolName,
    string DocumentationLevel,
    string TargetFramework,
    string CSharpLanguageVersion,
    string Meaning,
    string[] Parameters,
    string ReturnValue,
    string[] Callees,
    string[] Callers,
    string[] EvidenceIds);

public sealed record ProjectDocumentationCallEdge(
    string ProjectId,
    string CallerId,
    string CalleeId,
    string CallerSymbol,
    string CalleeSymbol,
    string CallExpression);

public sealed record ProjectDocumentationValueFlow(
    string ProjectId,
    string CallerId,
    string CalleeId,
    string[] ArgumentExpressions,
    string ReturnUsage);

public sealed record NegativeDtoDocumentationCandidate(
    string CandidateId,
    string DocumentationUnitId,
    string NegativeType,
    string ChosenDocumentation,
    string RejectedDocumentation,
    string[] EvidenceIds);

public sealed record NegativeDtoDocumentationValidation(
    string CandidateId,
    string DocumentationUnitId,
    string NegativeType,
    bool IsAccepted,
    string[] Errors);

public sealed record ProjectDocumentationDpoPair(
    string PairId,
    string DocumentationUnitId,
    string A,
    string Chosen,
    string Rejected,
    string NegativeType,
    string[] EvidenceIds);

public sealed record ProjectWikiCoverageReport(
    string ProjectId,
    string WikiDomainRoot,
    int EvidenceRecordCount,
    int QuestionCount,
    int AnsweredQuestionCount,
    int ExactEvidenceHitCount,
    double ExactEvidenceAccuracy,
    string[] CoveredQuestionTypes,
    string[] MissingQuestionTypes,
    bool IsPassed,
    string[] Failures);

public sealed record ProjectWikiValidationQuestionResult(
    string ProjectId,
    string QuestionType,
    string Question,
    string ExpectedEvidenceId,
    string[] ActualEvidenceIds,
    bool ExactEvidenceHit,
    bool QueryPlanningFlowInvoked,
    string PlannedIntent,
    int PlannedQueryStepCount,
    string TraceStatus,
    double CpuMs,
    double ElapsedMs,
    double PeakRamMb,
    string[] Failures);

public sealed record ProjectWikiValidationProjectReport(
    string ProjectId,
    string WikiDomainRoot,
    int EvidenceRecordCount,
    int BasicQuestionCount,
    int DeveloperReasoningQuestionCount,
    int AnsweredQuestionCount,
    int ExactEvidenceHitCount,
    int QueryPlanningInvokedCount,
    int EntityImpactTracePassCount,
    double ExactEvidenceAccuracy,
    double QueryPlanningInvocationRate,
    double EntityImpactTracePassRate,
    double CpuMs,
    double ElapsedMs,
    double PeakRamMb,
    bool IsPassed,
    ProjectWikiValidationQuestionResult[] QuestionResults,
    string[] Failures);

public sealed record ProjectWikiValidationRunReport(
    int ProjectCount,
    int EvidenceRecordCount,
    int TotalQuestionCount,
    int ExactEvidenceHitCount,
    double ExactEvidenceAccuracy,
    double QueryPlanningInvocationRate,
    double EntityImpactTracePassRate,
    double CpuMs,
    double ElapsedMs,
    double PeakRamMb,
    bool IsPassed,
    ProjectWikiValidationProjectReport[] Projects,
    string[] Failures);
