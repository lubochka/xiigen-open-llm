# EXTENSION-REALITY-MAP-v1.md
# Generated: 2026-06-04 | Branch: codex/chat-gpt-5.5
# Scope: 43.SmartSkillsDomain — three capabilities read-only recon

---

## 1. PromptToTask

**Main class:** `SkillSelectionContracts.cs` (domain marker `PromptToTaskDomain`), `PromptToTaskMachine.cs` (implementation)

**Key public method:**
- `PromptToTaskMachine.Translate(PromptToTaskRequest) -> OperationResult<PromptToTaskResponse>`
  - File: `43.SmartSkillsDomain/PromptToTaskMachine.cs` line ~37
  - Synchronous (not async)

**Input -> Output:**
- In: `PromptToTaskRequest` (PromptId, RawPrompt, RawPromptSha256, PromptProvenance, UserLanguage, ExplicitConstraints, ContextFields)
- Out: `OperationResult<PromptToTaskResponse>` wrapping `StructuredTaskContract`

**Verdict: DETERMINISTIC (stub)**
- Machine is a plain `sealed class` — does NOT implement ILearningMachine, ILogicalUnit, ILogicalFlow, or any hierarchy interface.
- Always returns `StructuredTaskContract` with `TaskType = "not_ready_no_semantic_task"`.
- Status constant: `LoadedStateReadyStatus = "loaded_state_ready_no_inference_implemented"`.
- Optional `IPromptToTaskLearnedStateLoader` injection accepted but inference path is not implemented even when state is loaded.

**DPO signal builder:** `PromptToTaskTaskPatternDpoSignalBuilder.Build(PromptToTaskTaskPatternDpoRequest)` exists in `PromptToTaskContracts.cs`. Signal is marked "contract-only; not semantic inference and not train-ready output."

**Wired into orchestrator:** No. No registration in `15.OrchestratorModel` found for PromptToTask machine.

**Training runner:** MISSING — no `PromptToTask.*DpoGenerator` or `PromptToTask.*TrainingRunner` found in `43.SmartSkillsDomain`.

---

## 2. OrchestrationReasoning (Find Orchestration Plan)

**Main class:** `OrchestrationReasoningContracts.cs` (domain marker `OrchestrationReasoningDomain`), `OrchestrationReasoningMachine.cs` (implementation)

**Key public method:**
- `OrchestrationReasoningMachine.Plan(OrchestrationReasoningRequest) -> OperationResult<OrchestrationReasoningResponse>`
  - File: `43.SmartSkillsDomain/OrchestrationReasoningMachine.cs` line ~37
  - Synchronous (not async)

**Input -> Output:**
- In: `OrchestrationReasoningRequest` (RequestId, StructuredTaskContract from PromptToTask, GuideDerivedStateRefs, CorpusBundleIds, AdaptiveContextRefs, RuntimePolicyRefs)
- Out: `OperationResult<OrchestrationReasoningResponse>` wrapping `OrchestrationScenarioPlan`

**Verdict: DETERMINISTIC (stub)**
- Machine is a plain `sealed class` — does NOT implement ILearningMachine, ILogicalUnit, ILogicalFlow, or any hierarchy interface.
- Always returns `OrchestrationScenarioPlan` with `ScenarioId = "not-ready-no-orchestration-inference"`.
- Status constants: `NotTrainedStatus = "not_trained_no_learned_guide_state"`, `LoadedGuideStateReadyStatus = "loaded_guide_state_ready_no_inference_implemented"`.
- Optional `IOrchestrationReasoningLearnedGuideStateLoader` injection accepted but inference path is not implemented even when guide state is loaded.
- RiskFlags always include `"NO_ORCHESTRATION_INFERENCE_IMPLEMENTED"`.

**DPO signal builder:** `OrchestrationReasoningPlanPatternDpoSignalBuilder.Build(OrchestrationReasoningPlanPatternDpoRequest)` exists in `OrchestrationReasoningContracts.cs`. Signal is marked "contract-only signal; not orchestration inference and not train-ready output."

**Wired into orchestrator:** No. No registration in `15.OrchestratorModel` found for OrchestrationReasoning machine.

**Training runner:** MISSING — no `OrchestrationReasoning.*DpoGenerator` or `OrchestrationReasoning.*TrainingRunner` found in `43.SmartSkillsDomain`.

---

## 3. SkillSelection (Find Needed Skills)

**Main class:** `SkillSelectionContracts.cs` (domain marker, contracts), `SkillSelectionMachine.cs` (implementation), `SkillSelectionOrchestrator.cs` (orchestrator)

**Key public methods:**
- `SkillSelectionMachine.TrainMachineBatchAsync(IReadOnlyList<SkillSelectionMachinePair>, CancellationToken) -> Task<OperationResult<SkillSelectionTrainingResult>>`
  - File: `43.SmartSkillsDomain/SkillSelectionMachine.cs` — calls `LearnPair()` internally with real weight updates
- `SkillSelectionMachine.DecideFromTrainedState(SkillSelectionRequest) -> OperationResult<SkillSelectionDecision>`
  - File: `43.SmartSkillsDomain/SkillSelectionMachine.cs`
- `SkillSelectionMachine.GetOrchestratorAsync() -> Task<ILogicalOrchestrator>`
  - Returns `new SkillSelectionOrchestrator()` (implements `ILogicalOrchestrator` — Level 4)

**Input -> Output:**
- Train in: `IReadOnlyList<SkillSelectionMachinePair>` (chosen/rejected DPO pairs)
- Infer in: `SkillSelectionRequest` (Prompt, ProjectRoot, AvailableSkillIds, HostFacts, UserPromptSourceEntryId)
- Infer out: `OperationResult<SkillSelectionDecision>` (SelectedSkillIds, RejectedSkillIds, EvidenceFeatures, Confidence, RequiresSecondPass, DecisionText)

**Verdict: TRAINABLE (partially)**
- Machine implements `ILearningMachine<SkillSelectionDomain>` + `IMachineTraining<SkillSelectionDomain>` (IMachineTraining extends ILearningMachine, both defined in `SkillSelectionContracts.cs`).
- Real learned state: `_candidatePositionWeights = new Dictionary<int, int>()` updated by `LearnPair()`.
- Training result: `ContractOnly: false` — real training, not a stub.
- Checkpoint record: `SkillSelectionCheckpoint` with `IReadOnlyDictionary<int, int> CandidatePositionWeights`.
- Manifest: `SkillSelectionCheckpointManifest` with `AccuracyAtExport`, `SemanticQualityClaimed`.
- ExportAsync/ImportAsync: return `*_NOT_READY` — not yet implemented (noted in README comments).

**Wired into orchestrator:** Partially. `GetOrchestratorAsync()` returns `SkillSelectionOrchestrator : ILogicalOrchestrator`. No registration in `15.OrchestratorModel` confirmed from recon scope.

**Training runner:** MISSING — no `SkillSelection.*DpoGenerator` or `SkillSelection.*TrainingRunner` found in `43.SmartSkillsDomain`.

---

## ILearningMachine Hierarchy Reference (from 00.Core/LearningMachineContracts.cs)

- Level 5: `ILearningMachine<TDomain>` — TrainBatchAsync, EvaluateAsync, ExportAsync, ImportAsync, GetOrchestratorAsync
- Level 4: `ILogicalOrchestrator` — RunAsync(LogicalOrchestratorRequest) -> LogicalOrchestratorResult
- Level 3: `ILogicalFlow<TInput>` — ExecuteAsync(LogicalFlowRequest<TInput>) -> LogicalFlowResult
- Level 2: `ILogicalUnit<TInput,TOutput>` — ExecuteAsync(LogicalUnitRequest<TInput>) -> LogicalUnitResult<TOutput>
- Level 1: `ILearningMachinePair<TP,TG>` — Personal + Global + anonymised sync
- Rule A-LM-02: Train() methods ONLY on ILearningMachine. Never on ILogicalUnit or ILogicalFlow.

---

## GAP LIST

GAP-1: PromptToTask — not on ILearningMachine hierarchy. Inference not implemented. Training runner missing. Cannot be trained or export/import state. Status: runner_missing, contract-only.

GAP-2: OrchestrationReasoning — not on ILearningMachine hierarchy. Inference not implemented. Training runner missing. Cannot be trained or export/import state. Status: runner_missing, contract-only.

GAP-3: SkillSelection — on ILearningMachine hierarchy with real weight training. However: ExportAsync/ImportAsync return NOT_READY (no persistence). Training runner missing. No 15.OrchestratorModel registration confirmed. Status: train-capable in memory only, runner_missing, not persistable.

GAP-4: All three capabilities — DPO signal builders are contract-only stubs. No actual chosen/rejected pairs flow to a training pipeline. Signal StorageScope fields reference `.xiigen/` paths but no writer or persister exists in the scanned files.

GAP-5: PromptToTask and OrchestrationReasoning — no ILogicalUnit or ILogicalFlow wrapping. These are plain classes with no integration point into the ILearningMachine hierarchy at any level. Implementing trainable inference requires: (a) add ILearningMachine<TDomain> interface, (b) implement inference method, (c) implement ExportAsync/ImportAsync, (d) add training runner, (e) register in 15.OrchestratorModel.

GAP-6: SkillSelection — ExportAsync/ImportAsync NOT_READY means checkpoints cannot survive process restart. A training runner and checkpoint persistence path must be implemented before any trained state is durable.
