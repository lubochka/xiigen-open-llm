# 43.SmartSkillsDomain

Smart skills and AI extension domain. Phase 39/40 of ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.

## Purpose

This subproject provides trainable skill selection for AI extension hosts (Claude, Codex, Cursor). It is the core library ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â all trainable logic lives here; three external thin-client host repos consume it via a stable API boundary.

**Namespace:** `SmartSkillsDomain`
**Target framework:** .NET 9
**Dependency:** `00.Core` (AgentInfrastructure, AgentInfrastructure.Logical)

## Machines

### SkillSelectionMachine (Phase 39 ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â trained state on demand)

`SkillSelectionMachine : ILearningMachine<SkillSelectionDomain>, IMachineTraining<SkillSelectionDomain>`

Selects skills to inject into an AI session based on the user's prompt and task context. Uses 11 trainable semantic units (see `SmartSkillsSemanticUnitCatalog.cs`). Training produces DPO (Direct Preference Optimization) pairs via `TrainMachineBatchAsync`; candidate-position weights are scaffold evidence only. SS-02 added the first checkpoint-backed semantic unit slice, `prompt_understanding`; SS-03 added `task_understanding`; SS-04 added `task_type`; SS-05 added `routing`; SS-06 added `skill_candidates`; SS-07 added `skill_selection`; SS-08 added `prompt_context`; SS-09 added `result_gap`; SS-10 added `output_validation`; SS-11 added `task_completion`; SS-12 adds `intermediate_checks`. Full SkillSelection semantic quality still requires SS-13 asset lifecycle.

Training corpus: `training-data/skill-selection/skill-selection-v1/corpus-snapshot.jsonl` + `corpus-pointer.json`

Checkpoint: `models/skill-selection/skill-selection-v1/skill-selection-v1-checkpoint.json`

Key methods:
- `TrainMachineBatchAsync(IReadOnlyList<SkillSelectionMachinePair>)` - updates scaffold candidate-position evidence plus the `prompt_understanding`, `task_understanding`, `task_type`, `routing`, `skill_candidates`, `skill_selection`, `prompt_context`, `result_gap`, `output_validation`, `task_completion`, and `intermediate_checks` checkpoint sections from DPO pairs; semantic-unit routing comes from `TargetTrainingAdapter`, not runtime prompt keyword gates
- `DecideFromTrainedState(SkillSelectionRequest)` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â returns a `SkillSelectionDecision` using persisted weights
- `ExportCheckpoint()` / `LoadCheckpoint(SkillSelectionCheckpoint)` - checkpoint import/export
- `GetOrchestratorAsync()` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â returns a `SkillSelectionOrchestrator` implementing `ILogicalOrchestrator`

> **Implementation note**: `ExportAsync` and `ImportAsync` (ILearningMachine interface) return `*_NOT_READY`
> ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â the full ILearningMachine export/import path is not yet implemented.
> Use the machine-specific `ExportCheckpoint` / `LoadCheckpoint` methods for checkpoint persistence.
> See ILearningMachine hierarchy in docs/ARCHITECTURE.md Ãƒâ€šÃ‚Â§10 for the planned full interface compliance.

### SkillSelectionOrchestrator

`SkillSelectionOrchestrator : ILogicalOrchestrator`

Phase 39 S.1 contract scaffold. `RunAsync` maps a `LogicalOrchestratorRequest` to a `SkillSelectionRequest` and returns a `LogicalOrchestratorResult` with verdict `CONTRACT_ONLY` until trained semantic selection gates pass.

### OrchestrationReasoningMachine (SKELETON ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â NOT TRAINED)

Status constant: `not_trained_no_learned_guide_state`

Skeleton implementation. No orchestration inference is implemented. Returns risk flags and next-granularization-action guidance only. Accepts an optional `IOrchestrationReasoningLearnedGuideStateLoader`; when a guide state is loaded the status becomes `loaded_guide_state_ready_no_inference_implemented`. Governed by `ORCHESTRATION-AND-PROMPT-TO-TASK-EXTENSIONS-STATE.json` (separate plan, paused for commit-boundary reconciliation). NOT part of ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.

### PromptToTaskMachine (SKELETON ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â NOT TRAINED)

Status constant: `not_trained_no_loaded_state`

Skeleton implementation. No semantic classification or inference is implemented. Accepts an optional `IPromptToTaskLearnedStateLoader`; when state is loaded the status becomes `loaded_state_ready_no_inference_implemented`. Translates a raw prompt into a `StructuredTaskContract` for handoff to `OrchestrationReasoningMachine`. Same governing plan as `OrchestrationReasoningMachine`.

## Semantic Unit Catalog (SmartSkillsSemanticUnitCatalog.cs)

11 trainable semantic units. All have `DeterministicFinalAuthority = false` and `Trainable = true`:

| ID | Machine name | Training adapter | DPO row shape |
|---|---|---|---|
| `prompt_understanding` | `PromptUnderstandingMachine` | `IPromptUnderstandingTraining` | `PromptUnderstandingDpoRow` |
| `task_understanding` | `TaskUnderstandingMachine` | `ITaskUnderstandingTraining` | `TaskUnderstandingDpoRow` |
| `task_type` | `TaskTypeMachine` | `ITaskTypeTraining` | `TaskTypeDpoRow` |
| `routing` | `RoutingMachine` | `IRoutingTraining` | `RoutingDpoRow` |
| `skill_candidates` | `SkillCandidateMachine` | `ISkillCandidateTraining` | `SkillCandidateDpoRow` |
| `skill_selection` | `SkillSelectionMachine` | `ISkillSelectionTraining; IMachineTraining<SkillSelectionDomain>` | `SkillSelectionMachinePair` |
| `prompt_context` | `PromptContextMachine` | `IPromptContextTraining` | `PromptContextDpoRow` |
| `result_gap` | `ResultGapMachine` | `IResultGapTraining` | `ResultGapDpoRow` |
| `output_validation` | `OutputValidationMachine` | `IOutputValidationTraining` | `OutputValidationDpoRow` |
| `task_completion` | `TaskCompletionMachine` | `ITaskCompletionTraining` | `TaskCompletionDpoRow` |
| `intermediate_checks` | `IntermediateCheckMachine` | `IIntermediateCheckTraining` | `IntermediateCheckDpoRow` |

`ExpectedDomainIds` is the canonical ordered list used for gate assertions.

## API Boundary (XiigenExtensionApiContracts.cs)

Thin-client API consumed by all three host extension repos:

```csharp
// Request from host to xiigen core
XiigenExtensionRequest(
    HostName, HostType, Method, UserPromptOrTaskText,
    HostFacts, DebugEnabled, RequestedSkills, AvailableSkills,
    CheckSetInputs, Provenance, CorrelationId)

// Response from xiigen core to host
XiigenExtensionResponse(
    Success, Status, ChatDisplayText, SelectedSkills,
    CheckSetResults, StructuredTaskRefs, ScenarioRefs,
    Warnings, Errors, DebugTrace?)
```

Static helpers on `XiigenExtensionApiContract`:
- `ValidateRequest` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â returns `XiigenExtensionRequestValidationResult` (required fields: HostName, HostType, Method, CorrelationId)
- `FormatDebugTraceForChat` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â redacts traces that contain raw private content or secrets
- `BuildChatDisplayText` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â merges chat text and optional debug trace

`XiigenDebugTrace` carries `Method`, `InputPayloadText`, `OutputPayloadText`, plus `ContainsRawPrivateContent` and `ContainsSecrets` safety flags.

Host repos (external, NOT in this repository):
- `claude-xiigen-smart-skills` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â `C:\Users\lubao\Documents\xiigen\extensions reasearch\`
- `codex-xiigen-smart-skills`
- `cursor-xiigen-smart-skills`

Host repos are thin API / check-set clients. They must NOT contain training logic, corpus, or semantic selection code (A-LM-11).

## Phase 40 Evidence Contracts (Phase40ExtensionContracts.cs)

Enumerations and records that carry evidence scope across the three host kinds:

```csharp
Phase40ExtensionHostKind  { Unknown, Claude, Codex, Cursor }
Phase40EvidenceScope      { Unknown, HostRuntimeUserFacingEntrypoint,
                            PackageStaticOrAdapterContractOnly, ProductDecisionRequired }
Phase40CoreDecisionSourceKind { Unknown, CoreTrainableSkillSelection,
                                CoreCheckSetContract, ExactOrStaticEvidenceOnly,
                                HostProvidedEvidenceOnly }
```

Key records:
- `Phase40ExtensionApiRequest` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â full request including prompt-suite request, asset references, and trace log references
- `SkillSelectionResponse` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â wraps `SkillSelectionDecision` with `Phase40CoreDecisionEvidence`, check-set results, and contract status
- `Phase40ContractStatus` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â tracks whether host runtime is unavailable, whether full Phase 40 proof exists, and what the next required action is
- `Phase40CoreDecisionEvidence` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â includes `MasqueradeProhibited` flag and `DeterministicEvidenceUsedAsLearning` guard

## Key Files

| File | Purpose |
|---|---|
| `SkillSelectionMachine.cs` | Main machine - `ILearningMachine + IMachineTraining`; candidate-position scaffold plus `prompt_understanding`, `task_understanding`, `task_type`, `routing`, `skill_candidates`, `skill_selection`, `prompt_context`, `result_gap`, `output_validation`, `task_completion`, and `intermediate_checks` checkpoint sections |
| `SkillSelectionOrchestrator.cs` | `ILogicalOrchestrator` contract scaffold; returned by `GetOrchestratorAsync` |
| `SkillSelectionDomain.cs` | Domain marker class (`SkillSelectionDomain`) |
| `SkillSelectionContracts.cs` | All contracts: `SkillSelectionRequest`, `SkillSelectionDecision`, `SkillSelectionMachinePair`, `ServerSkillLibraryRegistry`, `ProjectLocalSkillState`, `SkillLibraryUpgradePlanner`, `ResultGapMachine`, `TaskCompletionMachine` |
| `SmartSkillsSemanticUnitCatalog.cs` | Registry of all 11 trainable semantic unit definitions |
| `XiigenExtensionApiContracts.cs` | Thin-client API boundary records and validation |
| `Phase40ExtensionContracts.cs` | Phase 40 evidence types, host request/response, and contract status |
| `OrchestrationReasoningContracts.cs` | `OrchestrationReasoningRequest`, `OrchestrationScenarioPlan`, `OrchestrationReasoningResponse` |
| `OrchestrationReasoningMachine.cs` | Skeleton orchestration reasoning machine (NOT trained) |
| `PromptToTaskContracts.cs` | `PromptToTaskRequest`, `StructuredTaskContract`, `PromptToTaskResponse` |
| `PromptToTaskMachine.cs` | Skeleton prompt-to-task machine (NOT trained) |

## SS-02 Through SS-12 Scope

`prompt_understanding`, `task_understanding`, `task_type`, `routing`, `skill_candidates`, `skill_selection`, `prompt_context`, `result_gap`, `output_validation`, `task_completion`, and `intermediate_checks` are represented in the checkpoint as the first eleven SkillSelection semantic unit slices. They store learned signal profiles from training pairs and can drive held-out decisions after export/import or fresh load. `task_understanding`, `task_type`, `routing`, `skill_candidates`, `skill_selection`, `prompt_context`, `result_gap`, `output_validation`, `task_completion`, and `intermediate_checks` are trained only from rows routed to their explicit `TargetTrainingAdapter` values; runtime scoring does not use hardcoded task, task-type, routing, skill-candidate, final skill-selection, prompt-context, result-gap, output-validation, task-completion, or intermediate-check keyword whitelists. `prompt_context` learns from prompt tokens plus redacted-safe host fact key/value tokens, generic project-root presence/type signals, and provenance-reference presence/type signals; it does not store raw project paths or exact prompt-source ids as learned authority. `result_gap` is checkpoint-backed for the SS-09 semantic unit only; the older `ResultGapMachine` bootstrap fixture remains contract support and is not adaptive DPO writing or final semantic authority. `output_validation` is checkpoint-backed for the SS-10 semantic unit only; deterministic API/request validators remain infrastructure and are not substituted for the learned unit. `task_completion` is checkpoint-backed for the SS-11 semantic unit only; the older `TaskCompletionMachine` bootstrap false-done guard remains contract support and is not substituted for the learned unit. `intermediate_checks` is checkpoint-backed for the SS-12 semantic unit only and supports mid-step evidence checks from learned training-pair profiles. Candidate-position weights remain scaffold evidence only, and these slices do not claim full SkillSelection semantic quality until SS-13 asset lifecycle passes.

## Adaptive State Boundary (A-LM-10)

- Common assets (skill library baseline): `models/skill-selection/` and `training-data/skill-selection/`
- Adaptive user state (per-project): `<project root>\.xiigen\smart-skills\` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â never overwritten by upgrades
- Project-local DPO signals: `.xiigen/smart-skills/dpo-signals.jsonl`
- Project-local preference patches: `.xiigen/smart-skills/preference-patches.json`

`SkillSelectionFeedbackWriter` writes project-local feedback signals and guards against private absolute paths in signal payloads.

`SkillLibraryUpgradePlanner` upgrades the universal skill library while preserving all project-local state and generating `SkillLibraryPreferencePatchProposal` entries for any skill identity changes that require a user decision.

## Tests

Test files live in `C:\Projects\llm_mvp_core\Tests\`:
- `SkillSelectionMachineTests.cs`
- `XiigenExtensionApiContractsTests.cs`
- `OrchestrationReasoningContractsTests.cs`
- `OrchestrationReasoningMachineSkeletonTests.cs`
- `OrchestrationReasoningStateLoadingTests.cs`
- `PromptToTaskContractsTests.cs`
- `PromptToTaskMachineSkeletonTests.cs`
- `PromptToTaskMachineStateLoadingTests.cs`
- `PromptToTaskToOrchestrationReasoningHandoffTests.cs`

## Gate Scripts (tools/)

7 Phase 39/40 gate scripts in `C:\Projects\llm_mvp_core\tools\`:

| Script | Purpose |
|---|---|
| `phase40-extension-core-api.ps1` | Phase 40 core API contract gate |
| `phase40-extension-prompt-suite-gate.ps1` | Phase 40 prompt suite gate |
| `skill-selection-core-adaptation-gate.ps1` | Skill selection core adaptation gate |
| `skill-selection-core-extension-boundary-gate.ps1` | Skill selection core/extension boundary gate |
| `skill-selection-plan-coverage-gate.ps1` | Skill selection plan coverage gate |
| `xiigen-extension-host-gate.ps1` | XIIGen extension host gate |
| `xiigen-extension-plan-gate.ps1` | XIIGen extension plan and governance gate |

## Decisions

- **A-LM-09**: All 11 semantic units use trained inference as final authority; deterministic helpers may support parsing, serialization, and reporting only.
- **A-LM-10**: Adaptive user state in `.xiigen\smart-skills\` ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â not overwritten by upgrades
- **A-LM-11**: Host repos are thin clients ÃƒÂ¢Ã¢â€šÂ¬Ã¢â‚¬Â no training logic inside
- **D-16** (related, in 42.ProjectWikiDomain): PromptFlowGroundingModel (Direct Preference Optimization-trained) is sole production flow grounding; deterministic helpers permitted only for corpus labeling, audit, and evaluation setup
