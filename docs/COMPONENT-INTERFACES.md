# COMPONENT-INTERFACES.md — Cross-Project Interface Reference

Last updated: 2026-05-28

RAG (Retrieval-Augmented Generation) scanning reference: all public interfaces that cross subproject boundaries.
Each entry lists the declaring assembly, implementing class, and consuming projects.

---

## 00.Core — `AgentInfrastructure.Logical` namespace (declared in 00.Core)

### ILearningMachine\<TDomain\>
```csharp
Task<OperationResult<LearningBatchResult>>     TrainBatchAsync(LearningBatch<TDomain> batch, CancellationToken ct = default)
Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(LearningEvaluationSet<TDomain> evalSet, CancellationToken ct = default)
Task<OperationResult<LearningMachineExport>>   ExportAsync(CancellationToken ct = default)
Task<OperationResult<bool>>                    ImportAsync(LearningMachineExport export, CancellationToken ct = default)
Task<OperationResult<ILogicalOrchestrator>>    GetOrchestratorAsync(CancellationToken ct = default)
```
Implemented by: domain machine classes (one per domain project — 27.TranslationDomain, OrchestrationPlanMachine (42.ProjectWikiDomain, Phase 37), SkillSelectionMachine (43.SmartSkillsDomain, Phase 39), etc.)
Decision A-LM-02: `Train*` methods live **only** here — never on `ILogicalUnit` or `ILogicalFlow`.

### IMachineTraining\<TDomain\> (00.Core — LearningMachineContracts.cs, Phase 39)
Product-facing training adapter that extends `ILearningMachine<TDomain>`. Implemented by `SkillSelectionMachine` (43.SmartSkillsDomain).
```csharp
public interface IMachineTraining<TDomain> : ILearningMachine<TDomain>
{
    Task<OperationResult<SkillSelectionTrainingResult>> TrainMachineBatchAsync(
        IReadOnlyList<SkillSelectionMachinePair> pairs,
        CancellationToken ct = default);
}
```

### ILearningMachinePair\<TPersonal, TGlobal\>
```csharp
ILearningMachine<TPersonal> Personal { get; }
ILearningMachine<TGlobal>   Global   { get; }
Task<OperationResult<bool>>  SyncPersonalToGlobalAsync(CancellationToken ct = default)
Task<OperationResult<bool>>  PullGlobalToPersonalAsync(CancellationToken ct = default)
```
`SyncPersonalToGlobalAsync` sends only anonymised signals — decision A-LM-08.

### ILogicalOrchestrator
```csharp
Task<OperationResult<LogicalOrchestratorResult>> RunAsync(LogicalOrchestratorRequest request, CancellationToken ct = default)
```
Implemented by: `AdaptiveLogicalOrchestrator` (15)
Consumed by: `ILearningMachine.GetOrchestratorAsync`, `ProjectWikiPluginOrchestrationAdapter` (42)

### ILogicalFlow\<TInput\>
```csharp
Task<OperationResult<LogicalFlowResult>> ExecuteAsync(LogicalFlowRequest<TInput> request, CancellationToken ct = default)
```
Implemented by: `ProjectDocumentationPipelineFlowAdapter` (42), `ProjectWikiOrchestrationFlow` (15)

### ILogicalUnit\<TInput, TOutput\>
```csharp
Task<OperationResult<LogicalUnitResult<TOutput>>> ExecuteAsync(LogicalUnitRequest<TInput> request, CancellationToken ct = default)
```
Implemented by: 6 Bootstrap classes in `42.ProjectWikiDomain/BootstrapOrchestrationUnits.cs`
Note: `ILogicalUnit` has **no** Train method — decision A-LM-02.

### Supporting records (LearningMachine family)
```csharp
record LearningBatch<TDomain>(string BatchId, string DomainTag, IReadOnlyList<LearningTrainingItem> TrainingItems, IReadOnlyList<string> SourceIds)
record LearningBatchResult(string BatchId, int RowsTrained, double AccuracyBefore, double AccuracyAfter, double RewardMargin, long CpuMs, long PeakRamMb, long ElapsedMs, IReadOnlyList<string> PromotedUnitIds)
record LearningEvaluationSet<TDomain>(string EvaluationId, string DomainTag, IReadOnlyList<LearningEvaluationItem> EvaluationItems)
record LearningEvaluationResult(string EvaluationId, int RowsEvaluated, double Accuracy, double RewardMargin, bool PassesGate, string GateNote)
record LearningMachineExport(string MachineId, string DomainTag, string Version, string CheckpointRef, string ManifestRef, double AccuracyAtExport, DateTimeOffset ExportedAtUtc)
```

---

## 21.AgentInfrastructure — `AgentInfrastructure.dll`

### ISessionStorage
```csharp
Task<OperationResult<bool>>               SaveAsync(string sessionId, AgentState state)
Task<OperationResult<AgentState>>         LoadAsync(string sessionId)
Task<OperationResult<IReadOnlyList<string>>> ListSessionsAsync()
```
Implemented by: `SessionStateStore` (21)
Consumed by: 22 (AgentOrchestrator), 14 (all services via BaseService), 15 (OrchestratorFacade)

### ICodeReader
```csharp
Task<OperationResult<string>>                    ReadFileAsync(string path)
Task<OperationResult<IReadOnlyList<string>>>     ListFilesAsync(string directory, string searchPattern)
```
Implemented by: `CodebaseIndexer` (21)
Consumed by: 14 (ProjectIndexer), 22 (RepoMapBuilder)

### ICodeWriter
```csharp
Task<OperationResult<bool>> WriteFileAsync(string path, string content)
```
Implemented by: `FileChangeApplier` (21)
Consumed by: 14 (DomainDatasetCache.FlushAsync, DomainModelRegistry)

### ILlmClient
```csharp
Task<OperationResult<LlmResponse>> SendAsync(
    IReadOnlyList<ConversationMessage> messageHistory,
    IReadOnlyList<ToolDefinition>? availableTools,
    CancellationToken cancellationToken)
```
Implemented by: (external adapter)
Consumed by: 21 (AgentLoop), 22 (ReactAgentLoop)

### IConfigurationProvider
```csharp
Task<OperationResult<Dictionary<string, object>>> ReadJsonAsync(string path)
Task<OperationResult<string>>                     ReadTextAsync(string path)
```
Implemented by: `ConfigurationLoader` (21)
Consumed by: 22 (RulesFileParser)

### SubPhaseRouterUnit / MultiSubPhaseRouterUnit
```csharp
// Implements ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>
// Implements ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
```
Input: `SubPhaseRouterSourceDocument` with `DocumentTitle`, `DocumentType`, `CompiledText`, `CompiledPredicateIds`, and `SourcePath`.
Scoring contract: routing scores use only `DocumentTitle`, `DocumentType`, `CompiledText`, and `CompiledPredicateIds`; `SourcePath` is never part of token scoring, exact sub-phase mention scoring, or hybrid rank input.
Provenance contract: `SourcePath` is preserved only in `LogicalUnitResult.EvidenceIds` as `source_path_provenance:<path>` and warning evidence includes `source_path_used_for_scoring=false`.
Leakage warning: when path-only vocabulary points to a different sub-phase than path-free semantic evidence, warnings include `source_path_semantic_conflict:<path_sub_phase_id>-><semantic_sub_phase_id>`. This warning must not raise confidence or change the selected route.
Fallback consistency contract: when `MultiSubPhaseRouterUnit` delegates to a single router and that router returns a `PredictedSubPhaseId` absent from the active topology, the multi-router fails with error code `FALLBACK_SUBPHASE_ID_NOT_REGISTERED`. The error message includes the safe single-line missing id and says the single-router fallback id is not registered in the active topology. Unknown fallback ids must never be converted to the first registered sub-phase or to an empty successful route result.

---

## 22.AgentAlgorithms — `AgentAlgorithms.dll`

### ITaskDecomposer
```csharp
OperationResult<IReadOnlyList<SubTask>> Decompose(string taskDescription)
```
Implemented by: `ComplexTaskDecomposer`, `NgramTaskDecomposer`, `RoleBasedTaskDecomposer` (22)
Consumed by: 15 (EpisodeDecomposer)

### IAgentMessageBus
```csharp
Task<OperationResult<string>> AssignTaskAsync(string subAgentName, string task)
```
Implemented by: `AgentOrchestrator` (22)
Consumed by: 15 (AgentLauncher)

### ITokenBudgetCalculator
```csharp
int Estimate(string content, ContentType type)
```
Implemented by: `AstTokenBudgetCalculator` (22)
Consumed by: 22 (ContextWindowManager), 21 (TokenCounter)

### ICodeIndex
```csharp
void IndexFile(string fileId, string csharpSource)
IReadOnlyList<(string FileId, double Score)> Search(CodeQuery query, int topK)
```
Implemented by: `AstPackedContextBuilder` (22)
Consumed by: 22 (AstRelevanceRanker, ContextWindowManager)

### ISkillFilter
```csharp
Task<OperationResult<IReadOnlyList<TextChunk>>> FilterAsync(
    IReadOnlyList<TextChunk> skillCandidates,
    string taskDescription,
    int topK = 5,
    double confidenceThreshold = 0.40)
```
Implemented by: `KnnSkillFilter` (22)
Consumed by: 22 (AgentPipeline), 21 (AgentLoop skill selection)

### ILlmGateway
```csharp
Task<OperationResult<string>> SendAsync(
    IReadOnlyList<ConversationMessage> history,
    CancellationToken cancellationToken = default)
```
Implemented by: adapter wrapping ILlmClient
Consumed by: 22 (ReactAgentLoop, LlmJudge)

### ISymbolGraphProvider
```csharp
Task<OperationResult<List<Dictionary<string, object>>>> GetAllSymbolsAsync()
Task<OperationResult<List<Dictionary<string, object>>>> GetReferencedByAsync(string symbolId)
```
Implemented by: `RepoMapBuilder` (22) via 07.GraphEngine
Consumed by: 22 (AstRelevanceRanker)

### ISemanticSearchProvider
```csharp
Task<OperationResult<List<(string ChunkId, double Score)>>> SearchAsync(string query, int topK)
```
Implemented by: adapter over 06.VectorSearch / 08.GraphRag
Consumed by: 22 (SemanticRanker, ContextCompactor)

### DependencyGraph (class, not interface — reused directly)
```csharp
// Declared in 22.AgentAlgorithms/MultiAgent/DependencyGraph.cs
void AddTask(SubTask task)
IReadOnlyList<IReadOnlyList<SubTask>> GetSortedLevels()
```
Reused by: 15 (OrchestratorFacade), 29 (Planning/DependencyGraph.cs — local copy from E35)

---

## 29.SemanticCompiler — `SemanticCompiler.dll`

### ISemanticCompiler29
```csharp
OperationResult<PipelineOutput> Compile(PipelineInput input)
```
Implemented by: `SemanticCompilerPipeline` (29)
Consumed by: 14 (ProjectIndexer), 15 (EpisodeClassifier)
Alias required: `using SC29 = SemanticCompiler29;` to avoid OperationResult ambiguity

### IStackDetector29
```csharp
string Detect(string rawText)   // returns "csharp" | "typescript" | "python" | "unknown"
```
Implemented by: `StackDetector` (29/Routing)
Note: must run BEFORE normalization — E20 Critical Rule A

### IRouteClassifier29
```csharp
Domain Classify(string[] tokens)
```
Implemented by: `RouteClassifier` (29/Routing)
Domain enum values: `BUG, PLAN, ARCH, REFACTOR, CODE_Q`

### ISemanticNormalizer29
```csharp
string[] Normalize(string text)
```
Implemented by: `SemanticNormalizer` (29/Compression)
E15: +9.7% accuracy improvement

### IFamilyCollapser29
```csharp
string[] Collapse(string[] tokens)
```
Implemented by: `FamilyCollapser` (29/Compression)
E16: +8.4% accuracy improvement

### ILogicExtractor29
```csharp
string[] Extract(string[] tokens)
```
Implemented by: `LogicExtractor` (29/Compression)
E17: +97% predicate density improvement

---

## 15.OrchestratorModel — `OrchestratorModel.dll`

### ITemplateBeautifier
```csharp
OperationResult<string> Beautify(string text)
```
Implemented by: `TemplateBeautifierAdapter` (15/Adapters) — wraps `SC29.TemplateBeautifier`
Consumed by: `ResultMerger` (15)
Error code on miss: `TEMPLATE_NOT_FOUND` — caller falls back to raw text

### ProjectWikiOrchestrationFlow
```csharp
// Implements ILogicalFlow<string>; FlowId = ScenarioId = "project-wiki-orchestration-flow"
// Registered in LogicalOrchestratorAssistant.CreateDefault()
Task<OperationResult<LogicalFlowResult>> ExecuteAsync(LogicalFlowRequest<string> request, CancellationToken ct = default)
```
Input: user prompt string; metadata keys: `active_repo_path`, `linked_repo_paths` (CSV), `user_facing_entrypoint`
Output: `LogicalFlowResult` with `FinalHumanResult` = verified wiki answer; `OverallVerdict` = `"ORCHESTRATED"` or `"NEEDS_MORE_EVIDENCE"`
Returns `BLOCKED_MISSING_PROJECT_ROOT` verdict when `active_repo_path` is absent from metadata.

---

## 42.ProjectWikiDomain — `ProjectWikiDomain.dll` (orchestration unit interfaces, TD1)

Six `ILogicalUnit` specialisations wrapping the six static orchestration steps.
All declared in `42.ProjectWikiDomain/OrchestrationUnitContracts.cs`.
Bootstrap implementations are in `42.ProjectWikiDomain/BootstrapOrchestrationUnits.cs`.

### IIntentBuilderUnit
```csharp
// Extends ILogicalUnit<string, TaskOrchestrationIntent>
// Input: user prompt string
// Output: TaskOrchestrationIntent (Goal, RequestedWorkItems[], Intent)
```
Implemented by: `BootstrapIntentBuilderUnit` — regex-based intent extraction from static logic

### IProjectScannerUnit
```csharp
// Extends ILogicalUnit<SubmitUserTaskRequest, ProjectUnderstanding>
// Input: SubmitUserTaskRequest (prompt, active root, linked roots, entrypoint)
// Output: ProjectUnderstanding (ProjectRoots[], StackFacts[], ScanSummary)
```
Implemented by: `BootstrapProjectScannerUnit` — delegates to `ProjectUnderstandingUnit.UnderstandAsync`

### IEvidencePlannerUnit
```csharp
// Extends ILogicalUnit<EvidencePlannerInput, TaskEvidencePlan>
// Input: EvidencePlannerInput(Intent, Project)
// Output: TaskEvidencePlan (EvidenceQuestions[], EvidenceGaps[])
```
Implemented by: `BootstrapEvidencePlannerUnit` — delegates to `EvidencePlanner.PlanAsync`

### IUnitSelectorUnit
```csharp
// Extends ILogicalUnit<UnitSelectorInput, TaskUnitSelection>
// Input: UnitSelectorInput(Intent, Project, EvidencePlan)
// Output: TaskUnitSelection (Units[])
```
Implemented by: `BootstrapUnitSelectorUnit` — delegates to `UnitSelector.SelectAsync`

### IRouteComposerUnit
```csharp
// Extends ILogicalUnit<RouteComposerInput, HumanTaskRoute>
// Input: RouteComposerInput(Request, Intent, Project, EvidencePlan, Units)
// Output: HumanTaskRoute (UserAnswer, HasEnoughStructure, Warnings[])
```
Implemented by: `BootstrapRouteComposerUnit` — delegates to `HumanRouteComposer.ComposeAsync`

### IOrchestrationVerifierUnit
```csharp
// Extends ILogicalUnit<VerifierInput, VerifierResult>
// Input: VerifierInput(Route, Intent, Project, EvidencePlan)
// Output: VerifierResult (Passes, Verdict)
```
Implemented by: `BootstrapOrchestrationVerifierUnit` — delegates to `OrchestrationVerifier.VerifyAsync`

### Aggregate input types
```csharp
record EvidencePlannerInput(TaskOrchestrationIntent Intent, ProjectUnderstanding Project)
record UnitSelectorInput(TaskOrchestrationIntent Intent, ProjectUnderstanding Project, TaskEvidencePlan EvidencePlan)
record RouteComposerInput(SubmitUserTaskRequest Request, TaskOrchestrationIntent Intent, ProjectUnderstanding Project, TaskEvidencePlan EvidencePlan, TaskUnitSelection Units)
record VerifierInput(HumanTaskRoute Route, TaskOrchestrationIntent Intent, ProjectUnderstanding Project, TaskEvidencePlan EvidencePlan)
record VerifierResult(bool Passes, string Verdict)
```

### OrchestrationPlanMachine (42.ProjectWikiDomain — Phase 37)
Trained orchestration plan generator. Implements `ILearningMachine<OrchestrationPlanDomain>`.
```csharp
public sealed class OrchestrationPlanMachine : ILearningMachine<OrchestrationPlanDomain>
{
    public const string MachineId = "orchestration-plan-machine";
    public const string DomainTag = "ProjectWikiDomain.OrchestrationPlan";
    // Named subcomponent: PromptFlowGroundingModel (_promptFlowGroundingModel)
    // Three generation modes: LearnedFlowGrounding, Mode2Generalization, Mode3HumanExplanation
    Task<OperationResult<OrchestrationGenerationResult>> GeneratePlanAsync(string prompt, CancellationToken ct = default)
    Task<OperationResult<OrchestrationTrainingBatchResult>> TrainOrchestrationBatchAsync(OrchestrationTrainingBatch batch, CancellationToken ct = default)
    OperationResult<bool> ImportCheckpoint(OrchestrationPlanMachineCheckpoint checkpoint)  // Current checkpoint schema is 3; import accepts schema 2 or 3
    OrchestrationPlanMachineCheckpoint ExportCheckpoint(OrchestrationLoopCheckpoint? loop = null)
    Task<OperationResult<LearningMachineExport>> SaveCheckpointAsync(string checkpointPath, OrchestrationLoopCheckpoint? loop = null, CancellationToken ct = default)
    static Task<OperationResult<OrchestrationPlanMachineLoadResult>> LoadCheckpointAsync(string checkpointPath, CancellationToken ct = default)
}
```
Current `OrchestrationPlanMachineCheckpoint` schema is 3. Schema 3 adds `TrainingProgressScore` and `LastEvaluationGate`; import remains backward-compatible with schema 2 checkpoints. `LearningMachineExport.AccuracyAtExport` must come from a passed `LastEvaluationGate`; training progress is not export accuracy.

### PromptFlowGroundingModel (42.ProjectWikiDomain — Phase 37)
Named subcomponent of `OrchestrationPlanMachine`. Owns `_flowGroundingKnn` (via `OrchestrationKnnStore`) and `_flowGroundingWeights` (Dictionary keyed by flowRef).
Three generation modes: `Mode1LearnedFlowGrounding`, `Mode2Generalization`, `Mode3HumanExplanation`.
Runtime confidence uses only input-side evidence: prompt tokens plus allowed-scope tokens. `LabelDescriptorFeatures` and `EvidenceOnlyTargetFeatures` are retained for evidence/checkpoint inspection only; they must not increase confidence and are not included in learned-pair K-NN query text. `BuildLearnedPairQueryTextWithoutPredictedFlow` must not add the predicted `FlowRef` as a nearest-neighbour feature.
CONSTRAINT: `exactPromptLeakageRate` must be `0.0` — any eval prompt exactly matching a training prompt causes `PassesGate=false`.
FORBIDDEN: `PromptFlowGroundingModel` must not use deterministic lookup as flow grounding. `FlowReferenceDetector` is build-time corpus/audit only.

### FlowGroundingResult (42.ProjectWikiDomain — OrchestrationPlanContracts.cs, Phase 37)
```csharp
public sealed record FlowGroundingResult(
    string FlowRef,
    double Confidence,
    IReadOnlyList<string> EvidenceFeatures,
    IReadOnlyList<string> PredictedPhaseSequence,
    IReadOnlyList<string> PredictedArtifactNames,
    IReadOnlyList<string> PredictedGateSequence);
```

### OrchestrationFlowGroundingWeights (42.ProjectWikiDomain — OrchestrationPlanContracts.cs, Phase 37)
```csharp
public sealed record OrchestrationFlowGroundingWeights(
    IReadOnlyDictionary<string, float> InputFeatureWeights,
    IReadOnlyDictionary<string, float> AllowedScopeFeatures,
    IReadOnlyDictionary<string, float> LabelDescriptorFeatures,
    IReadOnlyDictionary<string, float> EvidenceOnlyTargetFeatures,
    IReadOnlyDictionary<string, float> PhaseWeights,
    IReadOnlyDictionary<string, float> ArtifactWeights,
    IReadOnlyDictionary<string, float> GateWeights,
    IReadOnlyDictionary<string, float> StackWeights);
```

### OrchestrationGenerationResult (42.ProjectWikiDomain — OrchestrationPlanContracts.cs, Phase 37)
```csharp
public sealed record OrchestrationGenerationResult(
    string PlanText,
    GenerationRouteMode RouteMode,
    BoundarySource BoundarySource,
    bool MlGenerated,
    IntentKind IntentKind,
    FlowGroundingResult? FlowGrounding,
    IReadOnlyList<string> Warnings)
// Computed: bool ExpectedMlGeneratedByRoute => RouteMode.RequiresMlGenerated()
// Computed: bool SatisfiesMlGeneratedContract => !ExpectedMlGeneratedByRoute || MlGenerated
```
`GenerationRouteMode` values: `LearnedFlowGrounding`, `Mode2Generalization`, `Mode3HumanExplanation`, `Passthrough`

### REMOVED contracts (Phase 37 — must not be re-added)
- `PromptFlowIndex` — removed; replaced by `PromptFlowGroundingModel`
- `GenerationRouteMode.KnownFlowRetrieval` — removed
- `RetrievalSource` — removed
- `_flowIndex` field — removed

`FlowReferenceDetector` is NOT a runtime component — build-time corpus/audit tool only.

---

## 26.TrainableRouter — `TrainableRouter.dll`

### TrainableRoutingFlow
```csharp
// Implements ILogicalFlow<RoutingRequest>; FlowId = "trainable-routing-flow"
Task<OperationResult<UnitScoringResult[]>> ScoreAsync(RoutingRequest request, CancellationToken ct = default)
Task<OperationResult<RoutingDecision>> RouteAsync(RoutingRequest request, CancellationToken ct = default)
Task<OperationResult<LogicalFlowResult>> ExecuteAsync(LogicalFlowRequest<RoutingRequest> request, CancellationToken ct = default)
```
Input: `RoutingRequest` with prompt, task slice, optional direction/domain hint, candidate unit IDs, and context.
Route gate: `RoutingFlowThresholds.MinimumRoutingConfidence` and `MinimumScoreGap` must both pass before a scored unit becomes `ChosenUnitId`.
Knowledge edge: low confidence or ambiguous top-two score gap returns `RoutingDecision.KnowledgeEdge=true`, `ChosenUnitId=null`, preserved `RankedCandidates`, `RoutingScoreGapEvidence`, and `RoutingKnowledgeEdge` reason evidence. `ExecuteAsync` surfaces `OverallVerdict="ROUTING_KNOWLEDGE_EDGE"` and does not claim target-unit success.
Locator boundary: `DistributedRoutingBootstrap.RouteAndLocateAsync` returns `OperationResult.Fail(..., "ROUTING_KNOWLEDGE_EDGE")` before calling `IRemoteUnitLocator.LocateAsync` when the routing decision is a knowledge edge.
Training runner behavior: `RoutingTrainingRunner` records knowledge-edge cases and decision evidence, but does not count them as accepted/correct routes and does not turn them into accepted DPO pairs.
Profile metadata boundary: `RoutingProfileTrainer` learns keywords from `RoutingTrainingExample` rows, but role, domain, baseline prior, allowed inputs, confidence floor, and provenance must come from `RoutingUnitMetadataRegistry`. Unknown unit IDs fail with `ROUTING_UNIT_METADATA_REQUIRED` unless explicit general metadata is registered. The default orchestration curriculum supplies explicit registry entries for orchestration, translation, and documentation units.

### Routing profile metadata records
```csharp
record UnitRoutingMetadata(string UnitId, string Role, string Domain, string[] AllowedInputs, double ConfidenceFloor, string MetadataSourceRef, double BaselinePrior = 0.0)
record UnitScoringProfile(string UnitId, string UnitRole, string DomainTag, string[] Keywords, string[] AllowedInputs, double ConfidenceFloor, string MetadataSourceRef, double BaseScore = 0.0)
```
`UnitScoringProfile` carries metadata provenance so a published profile snapshot does not hide why a role/domain/floor was assigned. Unit-id substring inference is not a runtime metadata source.

### Routing decision evidence records
```csharp
record RoutingFlowThresholds(double MinimumRoutingConfidence, double MinimumScoreGap, string ThresholdsRef)
record RoutingScoreGapEvidence(string TopUnitId, string? SecondUnitId, double TopScore, double? SecondScore, double ScoreGap, bool MeetsMinimumScoreGap)
record RoutingKnowledgeEdge(bool KnowledgeEdge, string ReasonCode, string Explanation)
record RoutingDecision(string RequestId, string? ChosenUnitId, UnitScoringResult[] RankedCandidates, double Confidence, string Reason)
record RoutingDecisionRecord(string RunId, string RequestId, string Prompt, string TaskSlice, string ChosenUnitId, double ChosenScore, UnitScoringResult[] RejectedCandidates, string CorrectReason, string WrongRouteConsequence, DateTimeOffset DecidedAtUtc)
```
`RoutingDecision` and `RoutingDecisionRecord` carry `KnowledgeEdge`, `ScoreGap`, and `ThresholdsRef` properties so later DPO repair can tell why a route was not accepted.

---

## 27.TranslationDomain — `TranslationDomain.dll`

Active training domain for RU↔EN and HE↔EN translation. Declared in `27.TranslationDomain/`.
Implements `ILearningMachine<TranslationDomain>` (from `00.Core/LearningMachineContracts.cs`, namespace `AgentInfrastructure.Logical`).

### TranslationMachine
```csharp
// Implements ILearningMachine<TranslationDomain>
// Three trainable units: SemanticTokenizerUnit, ExecutorUnit, QualityVerifierUnit
Task<OperationResult<LearningBatchResult>>      TrainBatchAsync(LearningBatch<TranslationDomain> batch, CancellationToken ct = default)
Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(LearningEvaluationSet<TranslationDomain> evalSet, CancellationToken ct = default)
Task<OperationResult<LearningMachineExport>>    ExportAsync(CancellationToken ct = default)
Task<OperationResult<bool>>                     ImportAsync(LearningMachineExport export, CancellationToken ct = default)
Task<OperationResult<ILogicalOrchestrator>>     GetOrchestratorAsync(CancellationToken ct = default)
```
Implemented by: `TranslationMachine` (`27.TranslationDomain/TranslationMachine.cs`)
Test status: 44 tests passing as of phase 24 (2026-05-29, commit `a5e74154`)

### TranslationRoutingFlow
```csharp
// Implements ILogicalFlow<TranslationRoutingRequest>
// FlowId: "translation-routing-flow"
// Registered in LogicalOrchestratorAssistant.CreateDefault()
Task<OperationResult<LogicalFlowResult>> ExecuteAsync(LogicalFlowRequest<TranslationRoutingRequest> request, CancellationToken ct = default)
```
Input: `TranslationRoutingRequest(SourceLanguage, TargetLanguage, TopicDomain, InputText)`
Adapters: RU_EN, EN_RU, HE_EN, EN_HE (in `TranslationDirectionAdapters.cs`)

### TranslationKnnStore — FindNearestCascade
```csharp
// KNN (K-Nearest Neighbours) lookup cascade for translation unit inference
// Full → Prefix → Structure → BootstrapFallback
OperationResult<TranslationKnnMatch> FindNearestCascade(string structuralSignature, string directionTag)
```
Error codes on similarity guard violation: `TRAIN_HELDOUT_OVERLAP`, `TRAIN_HELDOUT_SIMILARITY_OVERLAP` (≥70% Jaccard token overlap)
Declared in: `TranslationTrainedUnits.cs`

### TranslationSemanticTokenizer — language rule tags
```
RU:SUBJECT:FEM, RU:SUBJECT:MASC
RU:TENSE:PAST
RU:VERB_GENDER:FEM, RU:VERB_GENDER:MASC, RU:VERB_GENDER:PLUR
RU:AGREEMENT:SUBJECT_FEM_PAST_FEM
RU:CASE:DATIVE
```
Emitted by `TranslationSemanticTokenizer` (`TranslationSemanticTokenizer.cs`).
UTF-8 Cyrillic methods called first; mojibake fallback (phase 25 repair pending).

---

## 14.ContinuousLearning — `ContinuousLearning.dll`

### IProjectIndexer
```csharp
OperationResult<IReadOnlyList<TrainingExample>> Index(string rootPath)
```
Implemented by: `ProjectIndexer` (14/Indexer)
Injection: requires `ISemanticCompiler29` — never reimplements StackDetector (Principle 4)

### IDomainDatasetCache
```csharp
OperationResult<bool>                       Add(TrainingExample example)
OperationResult<IReadOnlyList<TrainingExample>> GetByDomain(string domain)
OperationResult<int>                        TotalCount()
Task<OperationResult<bool>>                 FlushAsync()
```
Implemented by: `DomainDatasetCache` (14/Cache)

### IFeedbackRouter
```csharp
OperationResult<bool>                     Route(FeedbackEvent feedback)
OperationResult<IReadOnlyList<DpoPair>>   GetDpoPairs()
```
Implemented by: `FeedbackRouter` (14/Feedback)
Weight formula: `D-09: maxWeight = total / (domains × rareExamples)`

### IIncrementalTrainer
```csharp
Task<OperationResult<string>> TrainAsync(
    string domain,
    IReadOnlyList<TrainingExample> examples,
    IReadOnlyList<DpoPair> dpoPairs)
```
Implemented by: `IncrementalTrainer` (14/Training)
Delegates to: `LoraApplicator.Apply` (10.LoRA), `DpoTrainer.Step` (11.DPO)
Error code: `EMPTY_EXAMPLES` when examples.Count == 0

### IDomainModelRegistry
```csharp
OperationResult<bool>          Register(ModelAdapter adapter)
OperationResult<ModelAdapter?> GetAdapter(string domain)   // Value null = not found, still IsSuccess
OperationResult<IReadOnlyList<ModelAdapter>> ListAll()
```
Implemented by: `DomainModelRegistry` (14/Registry)

### IContinuousLearningPipeline
```csharp
Task<OperationResult<IReadOnlyList<ModelAdapter>>> RunCycleAsync(string rootPath)
```
Implemented by: `ContinuousLearningPipeline` (14/Pipeline)
For injection by: 16.AutonomousLearning — must use this interface, not the concrete class

---

## Core Data Types (cross-project)

### AgentInfrastructure types
```csharp
record ConversationMessage(string Role, string Content)
record AgentState(string SessionId, IReadOnlyList<ConversationMessage> History, string CurrentTask, IReadOnlyList<string> TodoItems)
record LlmResponse(string Content, ToolCall? ToolCall)
record ToolDefinition(string Name, string Description, object Schema)
```

### SemanticCompiler29 types
```csharp
record PipelineInput(string Text)
record PipelineOutput(Domain Domain, string[] Predicates, string Stack, double CompressionRatio)
enum Domain { BUG, PLAN, ARCH, REFACTOR, CODE_Q }
```

### AgentAlgorithms types
```csharp
record SubTask(string Id, string Description, string Role, IReadOnlyList<string> DependsOn)
record TextChunk(string Id, string Content, double Score = 0)
enum ContentType { CSharpCode, Documentation, NaturalLanguage, Json, SkillDocument }
```

### ContinuousLearning types
```csharp
record TrainingExample(string Prompt, string Response, string Domain, double Weight, DateTimeOffset CapturedAt)
record DpoPair(string Prompt, string Chosen, string Rejected, string Domain, DateTimeOffset CapturedAt)
record FeedbackEvent(string Prompt, string ModelResponse, string? UserCorrection, FeedbackSignal Signal, string Domain)
record ModelAdapter(string Domain, string AdapterPath, int TrainingExamples, DateTimeOffset TrainedAt)
enum FeedbackSignal { Accepted, Edited, Rejected }
```

---

## §43 — 43.SmartSkillsDomain (Smart Skills Domain — Phase 39/40)

Namespace: `SmartSkillsDomain`. All types declared in `43.SmartSkillsDomain/`.

### XiigenExtensionRequest / XiigenExtensionResponse (XiigenExtensionApiContracts.cs — Phase 40)
```csharp
public sealed record XiigenExtensionRequest(
    string HostName,
    string HostType,
    string Method,
    string UserPromptOrTaskText,
    IReadOnlyDictionary<string, string> HostFacts,
    bool DebugEnabled,
    IReadOnlyList<string> RequestedSkills,
    IReadOnlyList<string> AvailableSkills,
    IReadOnlyDictionary<string, string> CheckSetInputs,
    IReadOnlyDictionary<string, string> Provenance,
    string CorrelationId);

public sealed record XiigenExtensionResponse(
    bool Success,
    string Status,
    string ChatDisplayText,
    IReadOnlyList<string> SelectedSkills,
    IReadOnlyDictionary<string, string> CheckSetResults,
    IReadOnlyList<string> StructuredTaskRefs,
    IReadOnlyList<string> ScenarioRefs,
    IReadOnlyList<string> Warnings,
    IReadOnlyList<string> Errors,
    XiigenDebugTrace? DebugTrace);

public sealed record XiigenDebugTrace(
    string Method,
    string InputPayloadText,
    string OutputPayloadText,
    bool ContainsRawPrivateContent = false,
    bool ContainsSecrets = false);

public sealed record XiigenExtensionRequestValidationResult(
    bool IsValid,
    IReadOnlyList<string> Errors);
```

### XiigenExtensionApiContract (XiigenExtensionApiContracts.cs — Phase 40)
Static class providing request validation and display-text helpers.
```csharp
public static class XiigenExtensionApiContract
{
    public static XiigenExtensionRequestValidationResult ValidateRequest(XiigenExtensionRequest request)
    public static string FormatDebugTraceForChat(bool debugEnabled, XiigenDebugTrace? debugTrace)
    public static string BuildChatDisplayText(string chatDisplayText, bool debugEnabled, XiigenDebugTrace? debugTrace)
}
```
`ValidateRequest` requires non-empty: `HostName`, `HostType`, `Method`, `CorrelationId`.
`FormatDebugTraceForChat` returns empty string when `ContainsRawPrivateContent` or `ContainsSecrets` is true.

### Phase40 enums (Phase40ExtensionContracts.cs — Phase 40)
```csharp
public enum Phase40ExtensionHostKind { Unknown = 0, Claude = 1, Codex = 2, Cursor = 3 }

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
```

### Phase40 contract records (Phase40ExtensionContracts.cs — Phase 40)
Key records for extension API request/response cycle and evidence tracking:
```csharp
record Phase40ExtensionApiRequest(string RequestId, Phase40ExtensionHostKind HostKind, string HostRepositoryRoot,
    string UserFacingEntrypoint, string ConcreteTask, SkillSelectionRequest SkillSelectionRequest,
    Phase40PromptSuiteRequest PromptSuiteRequest, Phase40AssetReferenceSet AssetReferences,
    IReadOnlyList<Phase40TraceLogReference> TraceLogReferences)

record SkillSelectionResponse(string ResponseId, SkillSelectionDecision Decision,
    Phase40CoreDecisionEvidence DecisionEvidence, IReadOnlyList<Phase40CheckSetResult> CheckSetResults,
    Phase40ContractStatus ContractStatus, Phase40ExternalToolUsageEvidence ExternalToolUsageEvidence,
    Phase40AssetReferenceSet AssetReferences, IReadOnlyList<Phase40TraceLogReference> TraceLogReferences)

record Phase40ContractStatus(Phase40EvidenceScope EvidenceScope, bool HostRuntimeUnavailable,
    bool FullPhase40ImplementationProven, bool ContractOnlyDowngradeRequired,
    string DowngradeReason, string NextRequiredAction)

record Phase40CoreDecisionEvidence(Phase40CoreDecisionSourceKind DecisionSourceKind,
    string CoreDecisionReference, string ExactOrStaticEvidenceReference,
    bool DeterministicEvidenceUsedAsLearning, bool HostProvidedSemanticDecision,
    bool MasqueradeProhibited, string ReviewNote)

record Phase40AssetReferenceSet(string CommonModelManifestRef, string CommonTrainingDataManifestRef,
    string CommonModelLocatorRef, string AdaptiveStateRootRef, string ProjectLocalStateRef,
    bool CommonAssetsRepoOwned, bool AdaptiveStateSeparatedFromHostPackage)
```

### SmartSkillsSemanticUnitDefinition / SmartSkillsSemanticUnitCatalog (SmartSkillsSemanticUnitCatalog.cs — Phase 39)
```csharp
public sealed record SmartSkillsSemanticUnitDefinition(
    string DomainId,
    string MachineName,
    string TrainingAdapterName,
    string DpoRowShapeName,
    string CheckpointSectionName,
    string HeldOutFixtureId,
    bool Trainable,
    bool DeterministicFinalAuthority,
    bool RuntimeSemanticQualityClaimed);
```
`SmartSkillsSemanticUnitCatalog.ExpectedDomainIds` (11 units, all `Trainable=true`, `DeterministicFinalAuthority=false`, `RuntimeSemanticQualityClaimed=false`):
- `prompt_understanding` → `PromptUnderstandingMachine`
- `task_understanding` → `TaskUnderstandingMachine`
- `task_type` → `TaskTypeMachine`
- `routing` → `RoutingMachine`
- `skill_candidates` → `SkillCandidateMachine`
- `skill_selection` → `SkillSelectionMachine` (training adapter: `ISkillSelectionTraining;IMachineTraining<SkillSelectionDomain>`)
- `prompt_context` → `PromptContextMachine`
- `result_gap` → `ResultGapMachine`
- `output_validation` → `OutputValidationMachine`
- `task_completion` → `TaskCompletionMachine`
- `intermediate_checks` → `IntermediateCheckMachine`

### SkillSelectionMachine (SkillSelectionContracts.cs + SkillSelectionMachine.cs — Phase 39)
Implements both `ILearningMachine<SkillSelectionDomain>` and `IMachineTraining<SkillSelectionDomain>`.
```csharp
public sealed class SkillSelectionMachine : ILearningMachine<SkillSelectionDomain>, IMachineTraining<SkillSelectionDomain>
{
    IMachineTraining<SkillSelectionDomain> TrainingAdapter { get; }
    Task<OperationResult<SkillSelectionTrainingResult>> TrainMachineBatchAsync(IReadOnlyList<SkillSelectionMachinePair> pairs, CancellationToken ct = default)
    OperationResult<SkillSelectionDecision> DecideFromTrainedState(SkillSelectionRequest request)
    SkillSelectionCheckpoint ExportCheckpoint()
    void LoadCheckpoint(SkillSelectionCheckpoint checkpoint)
    static string NormalizePromptKey(string prompt)
    // + full ILearningMachine<SkillSelectionDomain> implementation
}
```
Key contract records:
```csharp
record SkillSelectionRequest(string Prompt, string ProjectRoot, IReadOnlyList<string> AvailableSkillIds,
    IReadOnlyDictionary<string, string> HostFacts, string UserPromptSourceEntryId)

record SkillSelectionDecision(IReadOnlyList<string> SelectedSkillIds, IReadOnlyList<string> RejectedSkillIds,
    IReadOnlyList<string> EvidenceFeatures, double Confidence, bool RequiresSecondPass, string DecisionText)

record SkillSelectionMachinePair(string PairId, SkillSelectionRequest Request,
    SkillSelectionDecision Chosen, SkillSelectionDecision Rejected, string RejectedType,
    string SourcePromptEntryId, string TargetTrainingAdapter)

record SkillSelectionTrainingResult(string BatchId, int PairCount, bool ContractOnly, string GateNote)

record SkillSelectionCheckpoint(int SchemaVersion, IReadOnlyDictionary<int, int> CandidatePositionWeights,
    IReadOnlyList<string> TrainedPairIds, IReadOnlyList<string> TrainedPromptKeys,
    string RouteMode, DateTimeOffset SavedAtUtc)
```

### ServerSkillLibraryRegistry (SkillSelectionContracts.cs — Phase 39)
```csharp
public sealed class ServerSkillLibraryRegistry
{
    public IReadOnlyList<ServerSkillLibraryEntry> LoadUniversalSkills()
}
```
Returns 3 universal skills: `planning-session-startup`, `plan-review`, `human-language-gate`.
```csharp
record ServerSkillLibraryEntry(string SkillId, string Version, string Summary, double DefaultWeight, string PortabilityLabel = "universal")
record ProjectLocalSkillState(IReadOnlyDictionary<string, string> WikiNotes, IReadOnlyDictionary<string, string> Preferences,
    IReadOnlyList<string> Feedback, IReadOnlyDictionary<string, double> CustomSkillWeights,
    IReadOnlyList<ServerSkillLibraryEntry> ServerLibraryEntries)
```

### SkillLibraryUpgradePlanner (SkillSelectionContracts.cs — Phase 39)
```csharp
public sealed class SkillLibraryUpgradePlanner
{
    public SkillLibraryUpgradeResult PlanUpgrade(SkillLibraryUpgradeRequest request)
}
```
Builds preference-patch proposals when old universal skill IDs are not present in the new library. Sets `SemanticQualityClaimed=false`, `EvidenceMode="schema_upgrade_boundary_not_semantic_authority"`.

### OrchestrationReasoningMachine (OrchestrationReasoningMachine.cs — Phase 40)
Skeleton machine. NOT trained — no orchestration inference implemented.
```csharp
public sealed class OrchestrationReasoningMachine
{
    public const string NotTrainedStatus = "not_trained_no_learned_guide_state";
    public const string LoadedGuideStateReadyStatus = "loaded_guide_state_ready_no_inference_implemented";
    public const string NoInferenceScenarioId = "not-ready-no-orchestration-inference";

    public OrchestrationReasoningMachine()
    public OrchestrationReasoningMachine(IOrchestrationReasoningLearnedGuideStateLoader learnedGuideStateLoader)
    public OperationResult<OrchestrationReasoningResponse> Plan(OrchestrationReasoningRequest request)
}

public interface IOrchestrationReasoningLearnedGuideStateLoader
{
    OperationResult<OrchestrationReasoningLearnedGuideStateDescriptor> Load();
}

public sealed record OrchestrationReasoningLearnedGuideStateDescriptor(
    string StateId, string GuideStateRef, string ManifestId, string ManifestSha256,
    string MachineType, string SchemaVersion, string SourceBaseCommit, string StateSha256);
```
When no learned guide state is loaded: `ContractStatus = NotTrainedStatus`.
When guide state descriptor is loaded but inference not yet implemented: `ContractStatus = LoadedGuideStateReadyStatus`.

### ExtensionEvalOnlyQualityRunner (ExtensionEvalOnlyQualityRunner.cs - QUALITY-02)
Eval-only helper runner for QUALITY-01/QUALITY-02 score evidence. It validates
approved local/private eval-only rows, scores only `USER_ADAPTIVE_EVAL_ONLY`
rows through `ExtensionQualityScoring.Score`, excludes `EVIDENCE_ONLY` and
`COMMON_BLOCKED` rows from scoring, and writes JSON, JSONL, and Markdown
reports. This deterministic rubric runner is labelled eval-only/helper/internal
baseline evidence only; it is not semantic, trainable, runtime, token-savings,
learnability, target-promotion, locator-update, common-readiness, or final
readiness authority. Scored rows must declare `prompt_sha256`,
`mode=baseline_vs_adaptive_pair`, and
`evidence_scope=local_private_eval_only` before scoring. Generated reports carry
first-class `synthetic_fixture_only` and `actual_approved_eval_executed`
boundary fields so synthetic fixture evidence cannot be confused with an
approved eval execution.
```csharp
public sealed class ExtensionEvalOnlyQualityRunner
{
    public OperationResult<ExtensionEvalOnlyQualityRunnerReport> Run(ExtensionEvalOnlyQualityRunnerOptions options)
    public OperationResult<IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow>> ReadInputRows(string inputJsonlPath)
    public OperationResult<ExtensionEvalOnlyQualityRunnerValidation> ValidateRows(
        IReadOnlyList<ExtensionEvalOnlyQualityRunnerInputRow> rows)
    public OperationResult<IReadOnlyList<string>> WriteReportFiles(
        ExtensionEvalOnlyQualityRunnerOptions options,
        ExtensionEvalOnlyQualityRunnerReport report)
}
```

### PromptToTaskMachine (PromptToTaskMachine.cs — Phase 40)
Skeleton machine. NOT trained — no semantic task inference implemented.
```csharp
public sealed class PromptToTaskMachine
{
    public const string NotTrainedStatus = "not_trained_no_loaded_state";
    public const string LoadedStateReadyStatus = "loaded_state_ready_no_inference_implemented";
    public const string NotReadyTaskType = "not_ready_no_semantic_task";

    public PromptToTaskMachine()
    public PromptToTaskMachine(IPromptToTaskLearnedStateLoader learnedStateLoader)
    public OperationResult<PromptToTaskResponse> Translate(PromptToTaskRequest request)
}

public interface IPromptToTaskLearnedStateLoader
{
    OperationResult<PromptToTaskLearnedStateDescriptor> Load();
}

public sealed record PromptToTaskLearnedStateDescriptor(
    string StateId, string ManifestId, string ManifestSha256, string MachineType,
    string SchemaVersion, string SourceBaseCommit, string StateSha256);
```

---

## Quick Lookup: "Which interface do I need?"

| I want to… | Interface | Assembly |
|---|---|---|
| Train a domain learning machine | `ILearningMachine<TDomain>` | AgentInfrastructure.Logical (00.Core) |
| Pair personal + global machines | `ILearningMachinePair<TP, TG>` | AgentInfrastructure.Logical (00.Core) |
| Run the orchestrator | `ILogicalOrchestrator` | AgentInfrastructure.Logical (00.Core) |
| Implement a logical flow | `ILogicalFlow<TInput>` | AgentInfrastructure.Logical (00.Core) |
| Implement a single executable cell | `ILogicalUnit<TIn, TOut>` | AgentInfrastructure.Logical (00.Core) |
| Save/load agent session | `ISessionStorage` | AgentInfrastructure |
| Read files from disk | `ICodeReader` | AgentInfrastructure |
| Write files to disk | `ICodeWriter` | AgentInfrastructure |
| Call the LLM | `ILlmClient` | AgentInfrastructure |
| Decompose a task | `ITaskDecomposer` | AgentAlgorithms |
| Sort tasks by dependency | `DependencyGraph` (class) | AgentAlgorithms |
| Launch sub-agents | `IAgentMessageBus` | AgentAlgorithms |
| Filter skill list | `ISkillFilter` | AgentAlgorithms |
| Classify text → domain | `ISemanticCompiler29` | SemanticCompiler29 |
| Detect programming language | `IStackDetector29` | SemanticCompiler29 |
| Run full orchestration | `OrchestratorFacade` | OrchestratorModel |
| Run project-wiki plugin prompt | `ProjectWikiOrchestrationFlow` | OrchestratorModel |
| Quality-check output | `QualityOrchestrator` | OrchestratorModel |
| Run project-local documentation/wiki flow | `ProjectDocumentationPipelineFlowAdapter` | ProjectWikiDomain |
| Answer project-local wiki questions | `ProjectLocalWikiOrchestrator` | ProjectWikiDomain |
| Build wiki intent from user prompt | `IIntentBuilderUnit` | ProjectWikiDomain |
| Scan project file system | `IProjectScannerUnit` | ProjectWikiDomain |
| Plan wiki evidence requirements | `IEvidencePlannerUnit` | ProjectWikiDomain |
| Select orchestration units | `IUnitSelectorUnit` | ProjectWikiDomain |
| Compose human answer route | `IRouteComposerUnit` | ProjectWikiDomain |
| Verify orchestration result | `IOrchestrationVerifierUnit` | ProjectWikiDomain |
| Index codebase for training | `IProjectIndexer` | ContinuousLearning |
| Route user feedback | `IFeedbackRouter` | ContinuousLearning |
| Run LoRA adaptation cycle | `IContinuousLearningPipeline` | ContinuousLearning |
| Generate orchestration plan (trained only after passed eval gate) | `OrchestrationPlanMachine` | ProjectWikiDomain |
| Learn flow grounding from DPO pairs | `PromptFlowGroundingModel` | ProjectWikiDomain |
| Select skills for a prompt (typed baseline/contract; semantic quality unclaimed) | `SkillSelectionMachine` / `IMachineTraining<TDomain>` | SmartSkillsDomain |
| Load universal skill library | `ServerSkillLibraryRegistry` | SmartSkillsDomain |
| Plan skill library upgrade | `SkillLibraryUpgradePlanner` | SmartSkillsDomain |
| Validate xiigen extension request | `XiigenExtensionApiContract.ValidateRequest` | SmartSkillsDomain |
| Run QUALITY-02 eval-only score rows | `ExtensionEvalOnlyQualityRunner` | SmartSkillsDomain |
| Orchestration reasoning (skeleton) | `OrchestrationReasoningMachine` | SmartSkillsDomain |
| Prompt-to-task translation (skeleton) | `PromptToTaskMachine` | SmartSkillsDomain |
