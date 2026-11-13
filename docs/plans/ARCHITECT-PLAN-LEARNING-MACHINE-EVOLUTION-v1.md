# ARCHITECT-PLAN-LEARNING-MACHINE-EVOLUTION-v1.md
# Version: 1.0 — 2026-05-28
# Purpose: Design the ILearningMachine pattern and document the evolution from
#          deterministic C# orchestration to a fully trainable topology.
# Verified against: codebase read 2026-05-28 (see evidence references below)
# Governing pattern: DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v3-6.md

---

## 1. Current Architecture — What the Code Actually Does

### 1.1 Foundation interfaces (verified in 00.Core/LogicalContracts.cs, 230 lines)

The project already has a complete runtime interface hierarchy:

```
ILogicalUnit<TIn, TOut>       — smallest executable cell (line 97)
ILogicalFlow<TInput>          — topology of units (line 166)
ILogicalOrchestrator          — coordinator over flows (line 202)
ILogicalCapabilityTrainer     — training bridge from knowledge edge to learned unit
                                (21.AgentInfrastructure/Logical/LogicalCapabilityTrainingContracts.cs)
TrainableRoutingFlow          — first learning-capable flow (26.TrainableRouter/Training/
                                TrainableRoutingFlow.cs, implements ITrainableRoutingFlow)
```

`ILogicalUnit.ExecuteAsync()` is execution-only. Training happens through a separate
path: `ILogicalCapabilityTrainer.TrainAndRegisterAsync()`. This separation is correct and
must be preserved in all new designs.

### 1.2 Static decision logic (verified in
    42.ProjectWikiDomain/ProjectTaskOrchestrationPipeline.cs, 392 lines)

Six components make decisions through hardcoded C# that cannot be trained:

| Component | Mechanism | Lines (approx) |
|---|---|---|
| BuildIntent | Regex.Replace + Regex.Split on comma/and/then/-> | ~130–160 |
| ProjectUnderstandingUnit | File-system scan, hardcoded folder/extension rules | ~160–260 |
| EvidencePlanner | Fixed list of generic questions, keyword matching | ~260–300 |
| UnitSelector | if (projects.Length > 1), if (prompt.Contains("rag")) | ~300–340 |
| HumanRouteComposer | StringBuilder templates, same structure for all prompts | ~340–380 |
| OrchestrationVerifier | Checks that strings are non-empty | ~380–392 |

These components are not connected to `ILogicalUnit`. They are called as plain C# methods
inside `TaskOrchestrationUnit.OrchestrateAsync()`.

### 1.3 Disconnected pipeline (verified by reading both files)

`ProjectWikiPluginOrchestrationAdapter.cs` calls `SubmitUserTaskApi` directly.
`SubmitUserTaskApi` calls `TaskOrchestrationUnit.OrchestrateAsync()` directly.
Neither call passes through `ILogicalOrchestrator`.

`15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs` registers
`T34LogicalFlowAdapter` and `SecurityVerification34FlowAdapter` — the wiki domain is
not registered here.

This means `TrainableRoutingFlow` in project 26 never receives wiki task requests.
Routing decisions from DPO training in project 26 cannot influence wiki orchestration.

### 1.4 Existing training infrastructure (verified by listing 42.ProjectWikiDomain)

Each wiki unit already has a complete training stack:

```
*DpoGenerator.cs        — builds DPO preference pairs
*TrainingPipeline.cs    — manages the training run
*TrainingRunner.cs      — executes the DPO runner
*ManifestPublisher.cs   — publishes accepted checkpoints to 25.ModelServer
```

Examples confirmed on disk: `WikiTraceAggregatorDpoGenerator.cs`,
`WikiContextRoutingDpoGenerator.cs`, `CodeTraceBuilderDpoGenerator.cs`.

The training machinery exists. The static decision logic has not yet been converted
to use it.

### 1.5 Missing top-level concept

No interface in `00.Core` or `21.AgentInfrastructure` represents a complete trainable
domain machine — an object that:
- owns a domain of knowledge
- trains itself in incremental batches
- evaluates on held-out sets
- exports and imports its state as a portable checkpoint
- returns a runtime orchestrator backed by its trained state
- pairs with other machines for knowledge sharing

`ILogicalCapabilityTrainer` is a single training step, not a machine.
`ILogicalOrchestrator` is a runtime coordinator, not a learner.
`IModelRepository` in 25.ModelServer is a storage layer, not a machine interface.

---

## 2. ILearningMachine Design Pattern

### 2.1 Design rule

`ILearningMachine<TDomain>` sits above `ILogicalOrchestrator` in the hierarchy.
It does not replace any existing interface. It adds a new outer ring:

```
ILearningMachine<TDomain>           ← NEW top layer (declared in 00.Core/LearningMachineContracts.cs,
                                       namespace AgentInfrastructure.Logical)
  └── GetOrchestratorAsync()
        └── ILogicalOrchestrator    ← existing (00.Core)
              └── ILogicalFlow<T>   ← existing (00.Core)
                    └── ILogicalUnit<TIn, TOut>  ← existing (00.Core)

Training path (separate from execution):
ILearningMachine.TrainBatchAsync()
  └── internally calls ILogicalCapabilityTrainer.TrainAndRegisterAsync()
        └── uses existing *DpoGenerator + *TrainingRunner per unit
```

Train() is NOT on ILogicalUnit. It is NOT on ILogicalFlow. It is NOT on
ILogicalOrchestrator. The only place training is initiated is
`ILearningMachine.TrainBatchAsync()`. This preserves Rule 2 and the existing
DPO infrastructure.

### 2.2 Interface shape (defined in 00.Core/LearningMachineContracts.cs)

```csharp
namespace AgentInfrastructure.Logical;

public interface ILearningMachine<TDomain>
{
    // TrainBatchAsync — one incremental DPO training step, returns before/after metrics
    Task<OperationResult<LearningBatchResult>> TrainBatchAsync(
        LearningBatch<TDomain> batch, CancellationToken ct = default);

    // EvaluateAsync — held-out set measurement, returns accuracy + gate pass/fail
    Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
        LearningEvaluationSet<TDomain> evalSet, CancellationToken ct = default);

    // ExportAsync — serialise checkpoint into a portable transfer record
    Task<OperationResult<LearningMachineExport>> ExportAsync(
        CancellationToken ct = default);

    // ImportAsync — merge an external checkpoint into this machine
    Task<OperationResult<bool>> ImportAsync(
        LearningMachineExport export, CancellationToken ct = default);

    // GetOrchestratorAsync — runtime orchestrator backed by the current trained state
    Task<OperationResult<ILogicalOrchestrator>> GetOrchestratorAsync(
        CancellationToken ct = default);
}

public interface ILearningMachinePair<TPersonal, TGlobal>
{
    ILearningMachine<TPersonal> Personal { get; }
    ILearningMachine<TGlobal>   Global   { get; }

    // SyncPersonalToGlobalAsync — anonymised signals only, no private data leaves
    Task<OperationResult<bool>> SyncPersonalToGlobalAsync(CancellationToken ct = default);

    // PullGlobalToPersonalAsync — apply latest global checkpoint to personal machine
    Task<OperationResult<bool>> PullGlobalToPersonalAsync(CancellationToken ct = default);
}
```

### 2.3 Supporting records

```csharp
LearningBatch<TDomain>        — batch id, domain tag, training items, source ids
LearningBatchResult           — rows trained, accuracy before/after, reward margin,
                                 CPU ms, peak RAM mb, elapsed ms, promoted unit ids
LearningEvaluationSet<TDomain> — evaluation id, domain tag, evaluation items
LearningEvaluationResult      — rows evaluated, accuracy, reward margin,
                                 passes gate (bool), gate note
LearningMachineExport         — machine id, domain tag, version, checkpoint ref,
                                 manifest ref, accuracy at export, exported at UTC
```

All fields that measure performance (accuracy, reward margin, CPU ms, RAM mb, elapsed ms)
are required. No field may be set to a string placeholder or approximate value in a
production export. The gate in `LearningEvaluationResult.PassesGate` is the trainable
equivalent of the existing runner gate in `ORCHESTRATION-TRAINING-PROTOCOL-v1.md`.

### 2.4 Relationship to 25.ModelServer

`LearningMachineExport` carries a `CheckpointRef` and `ManifestRef` that point into
`25.ModelServer`. `IModelRepository` (already in 25.ModelServer) becomes the storage
backend for exports. `IRemoteUnitLocator` (already in 25.ModelServer) discovers exports
by machine id.

No changes are required in 25.ModelServer to support TD3. The export format references
existing server contracts.

---

## 3. Evolution Path — From Now to Fully Trainable Topology

### Phase 0 (current plan — TECHNICAL-DEBT-CLOSURE-PHASE-0.md)
- TD3: add `ILearningMachine<TDomain>` to `00.Core`
- TD1: wrap static units in `ILogicalUnit` interfaces + bootstrap implementations
- TD2: register `ProjectWikiOrchestrationFlow` in `ILogicalOrchestrator`

### Phase 1 (after Phase 0 — Phase 0 complete as of 2026-05-29)
- Continue WikiTraceAggregator DPO training (next action from PROJECT-LOCAL-WIKI-MODEL-STATE.json; QUERY_PLANNING_BATCH_030 rejected — train cross-unit trace rows proving `ProjectWikiQueryPlanningFlow → WikiContextRoutingUnit → WikiAnswerComposer` order)
- Translation domain is active: 27.TranslationDomain exists, 44 tests passing (phase 24, commit `a5e74154`, 2026-05-29); current work is the 100+100 base loop targeting runtimeProductAccuracy ≥ 0.95; phase 25 code repairs pending
- Implement `CodeWikiMachine : ILearningMachine<CodeWikiDomain>` in 42.ProjectWikiDomain

### Phase 2 (after first wiki batches accepted)
- Replace BootstrapIntentBuilderUnit with trained implementation from 25.ModelServer
- Replace BootstrapEvidencePlannerUnit with trained implementation
- Verify: answers to same prompts differ between bootstrap and trained implementations

### Phase 3 (after phase 2 validated)
- Scaffold 28.AffiliateDomain with `ILearningMachinePair<AffiliatePersonal, AffiliateGlobal>`
- Scaffold 30.DesignConversionDomain (note: 29 is already SemanticCompiler)
- Build `ILearningMachine<DesignConversionDomain>` on top of existing CodeWikiMachine analysis

---

## 4. Decision Register

| Decision | What | Rationale |
|---|---|---|
| A-LM-01 | ILearningMachine sits above ILogicalOrchestrator | Separates domain ownership from runtime coordination |
| A-LM-02 | Train() is only on ILearningMachine, not ILogicalUnit | Preserves existing DPO infrastructure and Rule 2 |
| A-LM-03 | Bootstrap implementations keep existing static logic | No regression in current behaviour during transition |
| A-LM-04 | 27.TranslationDomain is a separate subproject | Translation is an independent domain, not a routing concern |
| A-LM-05 | 30.DesignConversionDomain (not 29, which is SemanticCompiler) | Conflict avoided by checking existing project list |
| A-LM-06 | BOOTSTRAP_FALLBACK logged as warning in TD2 | Tracks residual use of old path during transition |
| A-LM-07 | AffiliateGlobal server = 25.ModelServer + new endpoint in 15.OrchestratorModel | Single model server for all domains; personal machine is client library |
| A-LM-08 | SyncPersonalToGlobalAsync sends only anonymised signals | Privacy separation between personal and global domains |
