# TECHNICAL-DEBT-CLOSURE-PHASE-0.md
# Version: 1.0 — 2026-05-28
# Purpose: Close three architectural debts that block parallel domain training.
# Authority: ARCHITECT-PLAN-LEARNING-MACHINE-EVOLUTION-v1.md
# Dependency: must complete before executing PARALLEL-DOMAIN-TRAINING-ROADMAP-v1.md

---

## Context

Three debts prevent the five parallel training directions from running independently:

1. Static units in 42.ProjectWikiDomain make decisions through hardcoded C# logic that
   cannot be trained.
2. ProjectTaskOrchestrationPipeline bypasses ILogicalOrchestrator, so TrainableRoutingFlow
   in 26.TrainableRouter never receives wiki task requests.
3. No ILearningMachine<TDomain> interface exists in 00.Core, so each training direction
   cannot be expressed in a common form.

Closing order: TD3 first (defines the shape everything else uses), then TD1 and TD1 in
parallel (TD1 wraps static units, TD2 connects them to the orchestrator), then each of the
five domain projects can start from a clean interface.

---

## TD3 — ILearningMachine<TDomain> in 00.Core

### What this adds

New file: `00.Core/LearningMachineContracts.cs`

This file defines the top-level abstraction for any trainable, exportable,
orchestrator-backed domain machine. It adds no implementations — only contracts.

Interfaces and records added:

- `LearningBatch<TDomain>` — one incremental training batch.
- `LearningBatchResult` — accuracy before/after, reward margin, CPU/RAM/elapsed metrics.
- `LearningEvaluationSet<TDomain>` — held-out evaluation input.
- `LearningEvaluationResult` — accuracy, reward margin, gate pass/fail.
- `LearningMachineExport` — portable checkpoint transfer record for import/export.
- `ILearningMachine<TDomain>` — top-level trainable machine contract.
- `ILearningMachinePair<TPersonal, TGlobal>` — personal + global machine pair for affiliate use case.

### Separation rule preserved

`ILearningMachine<TDomain>` does NOT add `Train()` to any execution interface.
Training is performed by `ILearningMachine.TrainBatchAsync()`, which internally calls
existing `ILogicalCapabilityTrainer.TrainAndRegisterAsync()` in the implementation.
`ILogicalUnit.ExecuteAsync()` remains execution-only. This preserves Rule 2 and the
existing DPO infrastructure in 42.ProjectWikiDomain.

### ILearningMachine<TDomain> contract (summary)

```csharp
// In namespace AgentInfrastructure.Logical — same as LogicalContracts.cs
public interface ILearningMachine<TDomain>
{
    // One training batch — returns metrics before and after
    Task<OperationResult<LearningBatchResult>> TrainBatchAsync(
        LearningBatch<TDomain> batch, CancellationToken ct = default);

    // Held-out evaluation — returns accuracy and gate result
    Task<OperationResult<LearningEvaluationResult>> EvaluateAsync(
        LearningEvaluationSet<TDomain> evalSet, CancellationToken ct = default);

    // Export current checkpoint for sharing or deployment
    Task<OperationResult<LearningMachineExport>> ExportAsync(
        CancellationToken ct = default);

    // Import and merge an external checkpoint
    Task<OperationResult<bool>> ImportAsync(
        LearningMachineExport export, CancellationToken ct = default);

    // Return a runtime orchestrator backed by the trained state
    Task<OperationResult<ILogicalOrchestrator>> GetOrchestratorAsync(
        CancellationToken ct = default);
}

public interface ILearningMachinePair<TPersonal, TGlobal>
{
    ILearningMachine<TPersonal> Personal { get; }
    ILearningMachine<TGlobal>   Global   { get; }

    // Send anonymised training signals from personal to global — no private data leaves
    Task<OperationResult<bool>> SyncPersonalToGlobalAsync(CancellationToken ct = default);

    // Pull latest global checkpoint into the personal machine
    Task<OperationResult<bool>> PullGlobalToPersonalAsync(CancellationToken ct = default);
}
```

### New test class

New file: `Tests/LearningMachineContractsTests.cs`

Verifies that the interfaces compile, that a stub implementation satisfies the contracts,
and that `OperationResult<T>` wraps all return types correctly.

### Gate B TD3

```bash
# Interface and record presence
grep -rn "ILearningMachine" C:/Projects/llm_mvp_core/00.Core/LearningMachineContracts.cs
# Expected: finds ILearningMachine, ILearningMachinePair, LearningBatch,
#           LearningBatchResult, LearningEvaluationSet, LearningEvaluationResult,
#           LearningMachineExport — 7 names minimum

# Separation rule verification — Train() must NOT appear on ILogicalUnit
grep -n "Train" C:/Projects/llm_mvp_core/00.Core/LogicalContracts.cs
# Expected: 0 matches (Train belongs to ILearningMachine, not ILogicalUnit)

# Build gate
dotnet build C:/Projects/llm_mvp_core/00.Core/Core.csproj 2>&1 | tail -3
# Expected: 0 Error(s)

# Test gate
dotnet test C:/Projects/llm_mvp_core/Tests/LlmFromScratch.Tests.csproj \
  --filter "LearningMachineContractsTests" 2>&1 | tail -5
# Expected: 3+ tests pass, 0 fail
```

---

## TD1 — Static Units Wrapped as ILogicalUnit Implementations

### What this changes

File: `42.ProjectWikiDomain/ProjectTaskOrchestrationPipeline.cs`

Six components currently contain hardcoded decision logic. Each is extracted into an
interface and a bootstrap implementation. The bootstrap implementation contains the same
logic that exists today — it is not a regression, it is the starting point for training.

New interfaces added to 42.ProjectWikiDomain:

```csharp
IIntentBuilderUnit     : ILogicalUnit<string, TaskOrchestrationIntent>
IProjectScannerUnit    : ILogicalUnit<SubmitUserTaskRequest, ProjectUnderstanding>
IEvidencePlannerUnit   : ILogicalUnit<EvidencePlannerInput, TaskEvidencePlan>
IUnitSelectorUnit      : ILogicalUnit<UnitSelectorInput, TaskUnitSelection>
IRouteComposerUnit     : ILogicalUnit<RouteComposerInput, HumanTaskRoute>
IOrchestrationVerifierUnit : ILogicalUnit<VerifierInput, VerifierResult>
```

New bootstrap implementations:

```csharp
BootstrapIntentBuilderUnit     : IIntentBuilderUnit
BootstrapProjectScannerUnit    : IProjectScannerUnit
BootstrapEvidencePlannerUnit   : IEvidencePlannerUnit
BootstrapUnitSelectorUnit      : IUnitSelectorUnit
BootstrapRouteComposerUnit     : IRouteComposerUnit
BootstrapOrchestrationVerifierUnit : IOrchestrationVerifierUnit
```

The bootstrap implementations contain the exact regex/file-scan/template logic that
currently lives inside `TaskOrchestrationUnit`. No behaviour changes. All existing tests
must still pass.

`TaskOrchestrationUnit` constructor accepts the six interfaces. When called without
arguments, it creates bootstrap instances (same as today). When called with trained
implementations from 25.ModelServer, it uses those.

### DPO generators added

For each new interface, a DPO generator and training runner follow the pattern already
established by `WikiTraceAggregatorDpoGenerator.cs` and
`WikiTraceAggregatorTrainingRunner.cs` in the same project.

Added files (per interface):

- `IntentBuilderDpoGenerator.cs`
- `IntentBuilderTrainingRunner.cs`
- (same pattern for the other five interfaces)

### Gate B TD1

```bash
# Interface presence
grep -rn "IIntentBuilderUnit\|IProjectScannerUnit\|IEvidencePlannerUnit\|IUnitSelectorUnit\|IRouteComposerUnit\|IOrchestrationVerifierUnit" \
  C:/Projects/llm_mvp_core/42.ProjectWikiDomain/
# Expected: 6 interface declarations found

# Bootstrap presence
grep -rn "BootstrapIntentBuilderUnit\|BootstrapProjectScannerUnit\|BootstrapEvidencePlannerUnit\|BootstrapUnitSelectorUnit\|BootstrapRouteComposerUnit\|BootstrapOrchestrationVerifierUnit" \
  C:/Projects/llm_mvp_core/42.ProjectWikiDomain/
# Expected: 6 class declarations found

# TaskOrchestrationUnit injection point
grep -n "IIntentBuilderUnit\|IProjectScannerUnit" \
  C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectTaskOrchestrationPipeline.cs
# Expected: constructor parameter lines found

# Full build and test suite
dotnet build C:/Projects/llm_mvp_core/LlmFromScratch.sln 2>&1 | tail -3
# Expected: 0 Error(s)

dotnet test C:/Projects/llm_mvp_core/Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: N passed (N >= floor from before TD1), 0 failed
```

---

## TD2 — ProjectWikiFlow Registered in ILogicalOrchestrator

### What this changes

New file: `15.OrchestratorModel/Logical/ProjectWikiOrchestrationFlow.cs`

This flow implements `ILogicalFlow<ProjectWikiOrchestratorRequest>` and wraps the six
`ILogicalUnit` implementations from TD1 into the topology that `ILogicalOrchestrator`
expects.

File modified: `15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs`

`ProjectWikiOrchestrationFlow` is registered alongside `T34LogicalFlowAdapter` and
`SecurityVerification34FlowAdapter`.

File modified: `42.ProjectWikiDomain/ProjectWikiPluginOrchestrationAdapter.cs`

The call to `SubmitUserTaskApi` is replaced with a call to
`ILogicalOrchestrator.RunAsync(...)` using flow id `project-wiki-orchestration-flow`.

A `BOOTSTRAP_FALLBACK` path is preserved and logged as a warning when the flow is not
registered:

```csharp
// BOOTSTRAP_FALLBACK: flow not registered — falling back to direct SubmitUserTaskApi.
// This path should not be active after TD2 completes. Log a warning to track usage.
_logger.LogWarning("BOOTSTRAP_FALLBACK active for project-wiki-orchestration-flow. " +
    "This indicates the flow is not registered in the orchestrator.");
```

### Gate B TD2

```bash
# Flow registration in LogicalOrchestratorAssistant
grep -n "project-wiki-orchestration-flow" \
  C:/Projects/llm_mvp_core/15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs
# Expected: at least 1 line with the flow id

# Adapter uses orchestrator, not direct API
grep -n "ILogicalOrchestrator\|RunAsync" \
  C:/Projects/llm_mvp_core/42.ProjectWikiDomain/ProjectWikiPluginOrchestrationAdapter.cs
# Expected: RunAsync call found

# Test: RunAsync with project-wiki-orchestration-flow returns non-empty FinalHumanResult
dotnet test C:/Projects/llm_mvp_core/Tests/LlmFromScratch.Tests.csproj \
  --filter "ProjectWikiOrchestrationFlowTests" 2>&1 | tail -5
# Expected: 2+ tests pass, 0 failed
```

---

## Execution order

```
TD3 (00.Core — no dependencies)        ← start here
  ↓ unblocks
TD1 (42.ProjectWikiDomain)             ← depends on TD3 interfaces
TD2 (15.OrchestratorModel + 42)        ← depends on TD1 interfaces

TD3 and Direction 1 (Translation)      ← can run in parallel
```

TD3 is the minimum required step before any domain project can be scaffolded.
TD1 and TD2 are required before training the orchestration units in Direction 2.
