> **STATUS: SUPERSEDED — Phase 37 (2026-06)**
> The distributed model architecture described here was integrated and superseded by ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.
> Phase 37 implements OrchestrationPlanMachine and PromptFlowGroundingModel in 42.ProjectWikiDomain.
> 25.ModelServer and 26.TrainableRouter from this plan remain planned but are not yet built as independent subprojects.
> See current architecture: `C:\Projects\llm_mvp_core\docs\ARCHITECTURE.md`

---

# Distributed Model Architecture Plan

Version: 1.1

Date: 2026-05-12

Project: `llm_mvp_core`

Status: reviewed and corrected against the required architecture.

## Required Architecture

The system must not become one large monolithic model.

Each trained capability is a separately loadable model unit. The unit is trained
on its own branch or stream, promoted independently, uploaded to a model server,
and invoked through a common orchestration contract.

The model itself becomes a design pattern:

`IOrchestrator -> ILogicalFlow -> ILogicalUnit`

Routing is not a static table. Routing is a trainable logical flow. The router
learns which model unit should receive which prompt, sub-prompt, or task slice.
For project-local wiki questions, routing is followed by another trainable
flow: `ProjectWikiQueryPlanningFlow`. It does not choose the project; it turns a
developer question into a sequence of evidence queries before retrieval and
answer composition.

Projects are allowed to train separately on different branches. The shared
system does not merge their weights into one universal checkpoint. It registers
their manifests and trains the routing/orchestration layer to use them.

## Review Verdict

The original plan was directionally correct, but needed four corrections:

1. It was too tied to `language_translate`. The architecture must support
   orchestration, translation, code documentation, and future project-specific
   branches equally.
2. It described a trainable router, but did not state strongly enough that
   routing itself is a first-class `ILogicalFlow`.
3. It treated remote model loading mostly as repository plumbing. The corrected
   plan makes "server-loaded trained model unit" the core abstraction.
4. It hard-coded a test floor and branch strategy. Those must be recorded from
   the current branch at phase start, not frozen in this architecture document.

## Non-Negotiable Design Rules

1. One capability equals one independently trained model unit.
2. Each model unit has a manifest.
3. A model unit can be loaded from a model server by unit id.
4. The orchestrator does not know model file paths.
5. Static routing is only a bootstrap fallback, not the final design.
6. Learned routing is implemented as an `ILogicalFlow`.
7. The routing model is trained with DPO pairs just like other learned units.
8. Different project domains may train on different branches.
9. Promotion merges manifests and router evidence, not unreviewed model outputs.
10. A model unit is never promoted without standalone and integration gates.

## Conceptual Shape

```text
User prompt
  |
  v
IOrchestrator
  |
  | decomposes or preserves the prompt as one or more routable task slices
  v
TrainableRoutingFlow : ILogicalFlow<RoutingRequest>
  |
  | scores candidate model units
  | chooses best unit per slice
  | records chosen/rejected routing evidence
  v
RemoteLogicalUnitProxy : ILogicalUnit<UnitRequest, UnitResult>
  |
  | loads or calls the trained model unit from server
  v
ModelServer
  |
  | serves unit manifest, tokenizer, adapter, checkpoint, and metadata
  v
Trained capability model
```

The same pattern can nest:

```text
TopOrchestrator
  -> PlanningRouterFlow
      -> PlanModelUnit
      -> SkillModelUnit
      -> OrchestrationModelUnit
  -> ProjectWikiRoutingFlow
      -> ProjectWikiQueryPlanningFlow
      -> ProjectLocalWikiOrchestrator
  -> TranslationRouterFlow
      -> RuEnGeneralUnit
      -> EnRuGeneralUnit
      -> HeEnLegalUnit
  -> DocumentationRouterFlow
      -> FunctionSummaryUnit
      -> CallerPropagationUnit
      -> LegacyMvcDocUnit
```

## Current Project Numbering Check

Current `llm_mvp_core` tree already contains numbered projects up to `36`.
There is no `25.*` or `26.*` project in the current tree. Therefore the plan can
use the following proposed project slots, but implementation must verify the
tree again before creating files:

- `25.ModelServer`
- `26.TrainableRouter`

If those numbers become occupied before implementation, choose the next free
numbers and update this plan before coding.

## What Changes And What Stays The Same

### Stays The Same

- `ILogicalUnit<TInput, TOutput>` remains the unit execution contract.
- `ILogicalFlow<TInput>` remains the flow-level contract.
- The top-level orchestrator remains the entry point for complex tasks.
- Individual capability DPO training keeps the same 50-pair batch concept and
  standalone/combo acceptance gates.
- `GeneratedCapabilityCheckpointManifest` is extended, not replaced.
- `TeachingRegistryPersistence` is extended with server publishing metadata.

### Changes

- Model weights are no longer assumed to be local files.
- Model resolution goes through `IModelRepository`.
- A trained unit can be served by URL and loaded by unit id.
- Unit registry becomes lazy: it stores descriptors and resolves units on demand.
- Static routing becomes a bootstrap fallback only.
- Production routing is handled by `TrainableRoutingFlow`.
- Router decisions are logged as DPO evidence.
- Router training has its own metrics: routing accuracy, reward margin, loss,
  CPU, RAM, and latency.

## New Project: `25.ModelServer`

Responsibility: publish, locate, and serve independently trained model units.

```text
25.ModelServer/
  Contracts/
    IModelRepository.cs
    IModelServer.cs
    IRemoteUnitLocator.cs
  Http/
    ModelServerHost.cs
    ModelEndpointRegistry.cs
  Local/
    LocalModelRepository.cs
  Remote/
    RemoteModelRepository.cs
    RemoteLogicalUnitProxy.cs
    RemoteUnitManifest.cs
```

### `IModelRepository`

Purpose: resolve a model unit manifest into usable model bytes or an inference
endpoint. The caller does not know whether the unit is local or remote.

```csharp
public interface IModelRepository
{
    Task<OperationResult<Stream>> LoadWeightsAsync(
        string unitId,
        string? serverBaseUrl,
        string localFallbackPath,
        CancellationToken ct = default);

    Task<OperationResult<string>> UploadAsync(
        GeneratedCapabilityCheckpointManifest manifest,
        string serverBaseUrl,
        CancellationToken ct = default);
}
```

### `IRemoteUnitLocator`

Purpose: map a unit id to the latest promoted manifest.

```csharp
public interface IRemoteUnitLocator
{
    Task<OperationResult<RemoteUnitManifest>> LocateAsync(
        string unitId,
        CancellationToken ct = default);

    Task<OperationResult> RegisterAsync(
        RemoteUnitManifest manifest,
        CancellationToken ct = default);
}
```

### `RemoteUnitManifest`

Purpose: describe one promoted model unit.

```csharp
public sealed record RemoteUnitManifest(
    string UnitId,
    string UnitRole,
    string DomainTag,
    string Version,
    string ServerBaseUrl,
    string UnitEndpoint,
    string ManifestSha256,
    string TrainingBranch,
    string TrainingRunId,
    string SourceDatasetSha256,
    ModelConfig ModelConfig,
    LoraConfig LoraConfig,
    string TokenizerPath,
    DateTimeOffset RegisteredAtUtc);
```

Required manifest fields:

- unit id;
- unit role;
- domain tag;
- version;
- server endpoint;
- training branch;
- training run id;
- source dataset hash;
- model config;
- LoRA config;
- tokenizer reference;
- registration timestamp.

## New Project: `26.TrainableRouter`

Responsibility: implement learned routing as an `ILogicalFlow`.

This project does not replace the orchestrator. It gives the orchestrator a
learned routing collaborator.

```text
26.TrainableRouter/
  Contracts/
    ITrainableRoutingFlow.cs
    IUnitScorer.cs
    IRoutingDecisionLog.cs
  Models/
    RoutingRequest.cs
    RoutingDecision.cs
    UnitScoringResult.cs
    RoutingDecisionRecord.cs
  Training/
    RoutingDpoPairBuilder.cs
    RoutingTrainingRunner.cs
```

### `RoutingRequest`

Purpose: represent the prompt or prompt slice that must be routed.

```csharp
public sealed record RoutingRequest(
    string RequestId,
    string OriginalPrompt,
    string TaskSlice,
    string? RequiredDirection,
    string? DomainHint,
    string[] CandidateUnitIds,
    IReadOnlyDictionary<string, string> Context);
```

### `ITrainableRoutingFlow`

Purpose: a trainable flow that chooses a model unit.

```csharp
public interface ITrainableRoutingFlow : ILogicalFlow<RoutingRequest>
{
    Task<OperationResult<UnitScoringResult[]>> ScoreAsync(
        RoutingRequest request,
        CancellationToken ct = default);

    Task<OperationResult<RoutingDecision>> RouteAsync(
        RoutingRequest request,
        CancellationToken ct = default);

    Task<OperationResult> RecordDecisionAsync(
        RoutingDecisionRecord decision,
        CancellationToken ct = default);
}
```

Important: `RouteAsync` is not a lookup table. It is model-backed scoring over
candidate units. The initial static router is only allowed as bootstrap data for
building early DPO pairs.

### `RoutingDecisionRecord`

Purpose: capture chosen and rejected unit decisions for router DPO training.

```csharp
public sealed record RoutingDecisionRecord(
    string RunId,
    string RequestId,
    string Prompt,
    string TaskSlice,
    string ChosenUnitId,
    double ChosenScore,
    UnitScoringResult[] RejectedCandidates,
    string CorrectReason,
    string WrongRouteConsequence,
    DateTimeOffset DecidedAtUtc);
```

Every routing decision can become a DPO pair:

- A-side: prompt or prompt slice plus available candidate units.
- Chosen: correct unit id and reason.
- Rejected: plausible wrong unit id plus `[WRONG: ...]` consequence.

## Existing Contract Extensions

### `GeneratedCapabilityCheckpointManifest`

Add server and provenance fields. Keep local fields as fallback.

```csharp
public string PolicyCheckpointPath { get; init; }
public string TokenizerPath { get; init; }

public string? ServerBaseUrl { get; init; }
public string? ServerUnitEndpoint { get; init; }
public string? ManifestSha256 { get; init; }
public string? TrainingBranch { get; init; }
public string? TrainingRunId { get; init; }
public string? SourceDatasetSha256 { get; init; }
public DateTimeOffset? UploadedAtUtc { get; init; }
```

Resolution rule:

1. If `ServerBaseUrl` and `ServerUnitEndpoint` are present, resolve remote first.
2. If remote resolution fails, use local fallback only if the manifest permits it.
3. If both fail, return `OperationResult` failure. Do not silently route to a
   different model unit.

### `TeachingRegistryPersistence`

After a model unit passes training gates:

1. save local checkpoint;
2. write local manifest;
3. upload model unit to server when configured;
4. rewrite manifest with server fields;
5. register the remote manifest in `IRemoteUnitLocator`;
6. log the promotion event.

Training is not blocked if server upload is disabled. Promotion to distributed
orchestration is blocked until server registration succeeds.

### Unit Registry

Current registry should become descriptor-first:

- register unit descriptor;
- do not load model at startup;
- resolve through locator when first used;
- cache loaded unit by manifest hash;
- invalidate cache when manifest hash changes.

## Orchestrator Behavior

The top-level orchestrator does four steps:

1. receive original prompt;
2. optionally split prompt into task slices;
3. call `ITrainableRoutingFlow` for each slice;
4. call the selected model unit through `RemoteLogicalUnitProxy`.

The orchestrator records:

- prompt;
- task slice;
- candidate units;
- chosen unit;
- rejected candidates;
- confidence;
- final user-visible result;
- correction signal when user or evaluator rejects the route.

This recorded data becomes router training material.

## Branch Strategy

The original plan incorrectly anchored every phase to `language_translate`.
Correct rule:

Each domain can train independently on its own branch family.

Example branch families:

```text
orchestration/*
translation/*
documentation/*
legacy-code-documentation/*
model-server/*
trainable-router/*
```

Examples:

```text
orchestration/v309-plan-routing
translation/ru-en-general-l1
documentation/dotnet-legacy-function-summary-l1
model-server/unit-manifest-registry
trainable-router/distributed-router-r1
```

Merge rule:

- Model branches produce trained model artifacts and manifests.
- Router branches produce routing DPO pairs and router checkpoints.
- Model server branches produce serving infrastructure.
- The integration branch merges only reviewed manifests, evidence reports, and
  passing infrastructure changes.
- One project branch must not rewrite another project's training artifacts.

## Implementation Phases

### Phase 0: Baseline Capture

Before any code change:

- record current test count from the branch;
- record current accepted training baseline;
- record existing logical contracts;
- record current branch name;
- record target domain branches.

Gate:

- no implementation until baseline evidence exists.

### Phase 1: Model Manifest And Repository

Files to create:

- `25.ModelServer/Contracts/IModelRepository.cs`
- `25.ModelServer/Contracts/IRemoteUnitLocator.cs`
- `25.ModelServer/Remote/RemoteUnitManifest.cs`
- `25.ModelServer/Local/LocalModelRepository.cs`

Files to extend:

- capability checkpoint manifest;
- teaching registry persistence.

Gate:

- local manifest can resolve a local checkpoint;
- remote manifest can be registered without loading weights;
- no existing training flow breaks.

### Phase 2: Model Server And Remote Unit Proxy

Files to create:

- `25.ModelServer/Http/ModelServerHost.cs`
- `25.ModelServer/Http/ModelEndpointRegistry.cs`
- `25.ModelServer/Remote/RemoteModelRepository.cs`
- `25.ModelServer/Remote/RemoteLogicalUnitProxy.cs`

Gate:

- remote proxy can locate a manifest;
- remote proxy can execute a stub model unit;
- manifest hash controls cache invalidation;
- failure returns `OperationResult` failure, not exception leakage.

### Phase 3: Trainable Routing Flow

Files to create:

- `26.TrainableRouter/Contracts/ITrainableRoutingFlow.cs`
- `26.TrainableRouter/Contracts/IUnitScorer.cs`
- `26.TrainableRouter/Contracts/IRoutingDecisionLog.cs`
- `26.TrainableRouter/Models/RoutingRequest.cs`
- `26.TrainableRouter/Models/RoutingDecision.cs`
- `26.TrainableRouter/Training/RoutingDpoPairBuilder.cs`

Gate:

- router receives candidate model units;
- router returns ranked unit scores;
- router records chosen/rejected decision evidence;
- router can build DPO pairs from decisions.

### Phase 4: Orchestrator Integration

Files to extend:

- adaptive orchestrator;
- logical flow registry;
- logical unit registry;
- teaching registry persistence.

Gate:

- static routing can still run as bootstrap fallback;
- trainable routing flow can be selected when available;
- orchestrator can route one prompt slice to one remote model unit;
- orchestrator can route multiple slices to different model units.

### Phase 5: Router Training Loop

Files to create:

- `26.TrainableRouter/Training/RoutingTrainingRunner.cs`
- router evaluation report format;
- router batch summary format.

Gate:

- 50 routing DPO pairs per batch;
- held-out routing set per batch;
- DPO accuracy reported;
- reward margin reported;
- CPU/RAM/time reported from runner;
- accepted router checkpoint promoted only after standalone and combo gates.

## Training Data For The Router

Router DPO pairs must teach:

- prompt to domain routing;
- prompt slice to unit routing;
- wrong-domain rejection;
- wrong-direction rejection;
- wrong-level rejection;
- stale model rejection;
- unregistered unit rejection;
- fallback vs learned-router boundary.

Example routing pair shape:

```json
{
  "a_side": "Prompt slice: document this C# legacy Page_Load method. Candidate units: ru-en-general, dotnet-function-summary, orchestration-plan-router.",
  "chosen": "Route to dotnet-function-summary. The request asks for code documentation, not language translation or orchestration planning.",
  "rejected": "Route to ru-en-general. [WRONG: the prompt is already English/C# and asks for legacy code documentation. Sending it to a translation unit loses caller/callee semantics and cannot produce bottom-up code docs.]"
}
```

Router training is separate from unit training. A translation model can improve
translation while the router learns when to call it.

## Acceptance Metrics

Each phase must report:

- rows trained;
- DPO accuracy;
- reward margin;
- loss;
- elapsed time;
- average CPU;
- peak CPU;
- average working set delta;
- peak working set delta;
- accepted/rejected batch decision;
- branch name;
- model unit id or router checkpoint id.

Do not call structural validation "accuracy". Accuracy is only from the runner.

## What This Unlocks

After this architecture is implemented:

1. A translation unit can be trained on one branch and uploaded to the server.
2. A legacy documentation unit can be trained on another branch and uploaded to
   the same server.
3. An orchestration/planning unit can remain focused on complex task structure.
4. The trainable router learns which unit to call for each prompt or prompt
   slice.
5. Adding a model does not require hard-coding a new router branch.
6. The system improves by training both units and the router that chooses among
   them.

## What This Does Not Allow

- No hidden monolithic "all tasks" checkpoint as the main design.
- No permanent static routing table as production routing.
- No unregistered model unit calls.
- No promotion without manifest provenance.
- No branch overwriting another domain's training artifacts.
- No invented CPU/RAM/accuracy metrics.

## Corrected Next Step

The next executor should not start by implementing arbitrary server code.

The correct order is:

1. write the exact contracts for `RemoteUnitManifest`, `IModelRepository`,
   `IRemoteUnitLocator`, `RoutingRequest`, and `ITrainableRoutingFlow`;
2. write tests for manifest registration and learned-routing decision records;
3. create a stub model server that can serve a fake model unit by manifest;
4. create one remote proxy test;
5. build the first 50 routing DPO pairs for choosing among model units;
6. train the router batch;
7. report DPO accuracy, margin, loss, CPU, RAM, and elapsed time;
8. only then connect real trained model units.

This preserves the required architecture: separate trained units, server-loaded
models, and learned routing as an orchestrator flow.
