# AdaptiveLogicalOrchestrator Implementation Plan
# Version: 2.0 - 2026-05-05
# Supersedes: ADAPTIVE-LOGICAL-ORCHESTRATOR-IMPLEMENTATION-PLAN-v1.md
# Authority: DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v2.md

---

## Purpose

This plan fixes the v1 implementation blockers and gives the executor a
buildable sequence for the first real adaptive orchestrator.

The target class remains:

```csharp
AdaptiveLogicalOrchestrator : ILogicalOrchestrator
```

Target location:

```text
C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\AdaptiveLogicalOrchestrator.cs
```

The orchestrator must eventually:

```text
raw prompt
-> transition/sub-phase routing
-> topology-aware flow selection
-> one or more flow branches
-> context carry-forward
-> knowledge-edge detection
-> learning event scheduling
```

---

## Corrections From v1

### Correction 1 - Registry ownership

v1 placed `AdaptiveLogicalOrchestrator` in project 21 but depended on
`InMemoryLogicalFlowRegistry` and `InMemoryLogicalUnitRegistry`, which currently
live in project 35. Project 21 cannot reference project 35.

v2 adds Phase 0 to move the runtime registries into project 21 first.

### Correction 2 - Router input type

v1 used:

```csharp
ILogicalUnit<string, SubPhaseRoutingDecision>
```

The actual router contract is:

```csharp
ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>
```

v2 adds a prompt-to-router-input compiler before the router call.

### Correction 3 - Flow route metadata

The current flow registry can resolve by scenario id, but topology-aware
orchestration needs resolution by phase, sub-phase, domain, and task type.

v2 extends the flow registry with route metadata and `ResolveBySubPhase`.

### Correction 4 - Multi-route output

v1 assumed the existing router could return multiple sub-phases. It cannot.

v2 adds a separate multi-sub-phase router for Phase B:

```csharp
ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
```

### Correction 5 - Learned unit swapping

v1 assumed the orchestrator can swap units inside an already constructed flow.
Current flows receive units through constructors, so the orchestrator cannot
replace them after construction.

v2 adds flow factories / unit-resolving flow construction in Phase C.

### Correction 6 - Learning scheduler threshold

v1 expected one missing example to increment `LearningEventScheduler.QueuedCount`.
The scheduler defaults to a minimum of five examples.

v2 makes the threshold explicit: production keeps the default threshold; tests
use a scheduler configured with threshold 1.

---

## Phase Sequence

```text
Phase 0 - Move runtime registries into project 21 and add route metadata
Phase A - Add adaptive orchestrator with single-route topology-aware selection
Phase B - Add multi-sub-phase routing and branch results
Phase C - Add learned unit selection through flow factories
Phase D - Add knowledge-edge to learning-event scheduling
```

No phase starts until the prior Gate B passes.

Test floor: 1172 passing, 0 failing.

---

## Phase 0 - Registry Ownership And Route Metadata

### Why Phase 0 Exists

`AdaptiveLogicalOrchestrator` belongs in project 21 because it is runtime
infrastructure. It cannot depend on registry classes in project 35.

### Files Moved

Move these files:

```text
from:
C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\InMemoryLogicalFlowRegistry.cs
C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\InMemoryLogicalUnitRegistry.cs

to:
C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\InMemoryLogicalFlowRegistry.cs
C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\InMemoryLogicalUnitRegistry.cs
```

Change namespace from:

```csharp
namespace ExperimentsV2.LogicalFlow;
```

to:

```csharp
namespace AgentInfrastructure.Logical;
```

Project 35 continues to reference project 21. Project 21 still does not reference
project 35.

### Route Metadata Added

Extend `LogicalFlowDescriptor` with route metadata:

```csharp
public sealed record LogicalFlowDescriptor(
    string FlowId,
    string ScenarioId,
    string Version,
    Type InputType,
    string PhaseId,
    string SubPhaseId,
    string TaskType,
    string[] DomainTags,
    long RegistrationOrder);
```

Keep the existing `Register<TInput>(string flowId, string scenarioId, string version, ILogicalFlow<TInput> flow)` overload.
It fills route metadata with empty values for backward compatibility.

Add a route-aware overload:

```csharp
public OperationResult<LogicalFlowDescriptor> Register<TInput>(
    string flowId,
    string scenarioId,
    string version,
    string phaseId,
    string subPhaseId,
    string taskType,
    string[] domainTags,
    ILogicalFlow<TInput> flow);
```

Add route lookup:

```csharp
public OperationResult<ILogicalFlow<TInput>> ResolveBySubPhase<TInput>(string subPhaseId);
public OperationResult<LogicalFlowBinding> ResolveBindingBySubPhase(string subPhaseId);
```

The first route-aware registration must bind the existing CB4 security flow to
sub-phase `3.4`.

### Gate B

```powershell
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "LogicalUnitRegistry|LogicalFlowRegistry"
# Expected: all pass

dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "LogicalOrchestratorAssistant|CB4"
# Expected: all pass

dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj
# Expected: >= 1172 passed, 0 failed
```

### Required Tests

1. Existing unit registry tests still pass after namespace move.
2. Existing flow registry tests still pass after namespace move.
3. Route-aware registration resolves the CB4 security flow by sub-phase `3.4`.
4. Project 21 builds without referencing project 35.

---

## Phase A - Single-Route Adaptive Orchestrator

### What Phase A Builds

Add:

```text
C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\PromptRoutingInputCompilerUnit.cs
C:\Projects\llm_mvp_core\21.AgentInfrastructure\Logical\AdaptiveLogicalOrchestrator.cs
```

The compiler unit converts `LogicalOrchestratorRequest` into the router input
shape the existing router already requires:

```csharp
PromptRoutingInputCompilerUnit
    : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
```

The router remains:

```csharp
SubPhaseRouterUnit
    : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision>
```

`AdaptiveLogicalOrchestrator` receives:

```csharp
ILogicalTopologyRegistry topology
ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument> promptCompiler
ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision> router
InMemoryLogicalFlowRegistry flows
InMemoryLogicalUnitRegistry units
```

Phase A runs one routed sub-phase only. Multi-branch routing is Phase B.

### Phase A Runtime

```text
LogicalOrchestratorRequest
-> PromptRoutingInputCompilerUnit
-> SubPhaseRouterUnit
-> InMemoryLogicalFlowRegistry.ResolveBindingBySubPhase
-> selected flow
-> LogicalOrchestratorResult
```

If no flow is registered for the routed sub-phase, the orchestrator returns a
synthetic knowledge-edge `LogicalFlowResult` instead of a generic failure.

Required metrics for that synthetic result:

```text
knowledge_edge = True
missing_sub_phase = <sub phase id>
missing_flow = True
```

### Assistant Wiring

`LogicalOrchestratorAssistant.CreateDefault()` wires `AdaptiveLogicalOrchestrator`
only after Phase A tests pass.

`InMemoryLogicalOrchestrator` remains in project 35 as deterministic reference.

### Gate B

```powershell
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "AdaptiveLogicalOrchestrator"
# Expected: all pass

dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "LogicalOrchestratorAssistant"
# Expected: all pass

dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj
# Expected: >= 1172 passed, 0 failed
```

### Required Tests

1. Prompt that routes to registered sub-phase `3.4` selects the CB4 security flow
   without caller-provided scenario id.
2. Prompt that routes to an unregistered sub-phase returns knowledge-edge result
   naming the missing sub-phase.
3. `LogicalOrchestratorAssistant.CreateDefault()` uses adaptive routing and no
   longer relies on hardcoded default scenario id for normal prompt execution.

---

## Phase B - Multi-Sub-Phase Routing And Branch Results

### What Phase B Builds

Add:

```csharp
MultiSubPhaseRouterUnit
    : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
```

This unit may wrap the existing single router internally, but its output contract
is an array.

Do not change the positional constructor of `LogicalOrchestratorResult`. That
would break existing callers.

Instead extend the record with init-only properties:

```csharp
public sealed record LogicalOrchestratorResult(
    string ScenarioId,
    string SelectedFlowId,
    string OverallVerdict,
    string FinalHumanResult,
    LogicalExecutionFlags AppliedFlags,
    LogicalFlowResult FlowResult)
{
    public LogicalFlowResult[] BranchResults { get; init; } = [];
}
```

Phase B sets:

```text
FlowResult = first branch result for backward compatibility
BranchResults = all branch results
```

### Execution Ordering

Use topology transitions to decide ordering:

```text
no transition dependency -> branches may run with Task.WhenAll
transition dependency -> upstream branch runs before downstream branch
```

If transition metadata is absent, run sequentially. Correctness beats parallelism.

### Verdict Aggregation

Aggregate branch verdicts:

```text
REJECT beats REPROCESS
REPROCESS beats PASS_WITH_GAPS
PASS_WITH_GAPS beats PASS_WITH_CONCERNS
PASS_WITH_CONCERNS beats PASS
```

Knowledge-edge branch results are included in `BranchResults`; they are not
dropped.

### Gate B

```powershell
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "AdaptiveLogicalOrchestrator"
# Expected: all pass

dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj
# Expected: >= 1172 passed, 0 failed
```

### Required Tests

1. Prompt touching two registered sub-phases returns `BranchResults.Length == 2`.
2. Prompt touching one registered and one unregistered sub-phase returns one real
   branch and one knowledge-edge branch.
3. `FlowResult` remains populated for old callers.
4. Independent test branches run concurrently, proven by measured elapsed time
   with deterministic delayed test flows.
5. Dependent branches run in transition order.

---

## Phase C - Learned Unit Selection Through Flow Factories

### Why v1 Was Not Buildable

Current flows receive their units through constructors. An orchestrator cannot
swap learned units inside an already constructed flow.

Phase C adds factories so the orchestrator can construct a flow with the right
units before execution.

### New Contracts

Add to project 21:

```csharp
public sealed record LogicalFlowFactoryRequest(
    string RunId,
    LogicalExecutionFlags Flags,
    InMemoryLogicalUnitRegistry Units,
    IReadOnlyDictionary<string, object> Metadata);

public interface ILogicalFlowFactory<TInput>
{
    OperationResult<ILogicalFlow<TInput>> Create(LogicalFlowFactoryRequest request);
}
```

Extend `InMemoryLogicalFlowRegistry` to register either:

```text
flow instance
flow factory
```

Factory resolution is preferred when the caller asks for learned-unit selection.

### First Factory

Add a factory for the existing security verification flow:

```text
C:\Projects\llm_mvp_core\35.ExperimentsV2\LogicalFlow\SecurityVerification34FlowFactory.cs
```

It resolves trusted learned units by role/domain when
`UseLearnedUnitWhenAvailable` is true. If no trusted learned unit exists, it
uses the deterministic security unit.

This keeps learned-unit selection inside flow construction, where constructor
injection already works.

### Context Carry-Forward

`AdaptiveLogicalOrchestrator` adds completed upstream branch results to
downstream branch metadata when `UseContextCarryForward` is true:

```text
metadata["context.branch_results"] = completed upstream results
metadata["context.source_branch_ids"] = upstream scenario ids
```

### Gate B

```powershell
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "AdaptiveLogicalOrchestrator"
# Expected: all pass

dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "SecurityVerification34Flow"
# Expected: all pass

dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj
# Expected: >= 1172 passed, 0 failed
```

### Required Tests

1. Trusted learned unit is selected when `UseLearnedUnitWhenAvailable = true`.
2. Deterministic fallback is selected when `UseLearnedUnitWhenAvailable = false`.
3. Rejected learned unit is never selected.
4. Probationary learned unit is selected only when minimum trust allows it.
5. Downstream branch metadata contains upstream context when
   `UseContextCarryForward = true`.
6. Downstream branch metadata does not contain upstream context when
   `UseContextCarryForward = false`.

---

## Phase D - Knowledge Edge To Learning Event

### What Phase D Builds

Inject `LearningEventScheduler` into `AdaptiveLogicalOrchestrator` as an optional
dependency.

When `UseKnowledgeEdgeDetection` is true, inspect each branch result for:

```text
Metrics["knowledge_edge"] == "True"
Warnings containing "knowledge_edge"
OverallVerdict == "KNOWLEDGE_EDGE"
OverallVerdict == "PASS_WITH_GAPS"
```

Extract:

```text
missing domain
missing sub-phase
missing flow
source prompt
```

Queue a learning event:

```csharp
LearningEventScheduler.QueueIfReady(domain, pendingExampleCount, reason)
```

Production keeps the scheduler default minimum of five examples. Tests use a
scheduler configured with `minimumExamples: 1`.

### Capability Generation Method

Add to `LogicalOrchestratorAssistant`:

```csharp
public async Task<OperationResult<LogicalAssistantRunResult>> RunWithCapabilityGenerationAsync(
    string userRequest,
    LogicalExecutionFlags? flags = null,
    CancellationToken cancellationToken = default)
```

This method:

```text
1. Calls RunAsync.
2. Detects knowledge-edge branches.
3. Schedules learning events.
4. If a teaching flow is registered for the missing sub-phase, runs it.
5. Does not retry unless the teaching flow returns a promoted capability.
6. Returns a human-visible result naming the gap and the learning action.
```

No fake learning is allowed. If no teaching flow exists, the result stays blocked
or gap-marked.

### Gate B

```powershell
dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "AdaptiveLogicalOrchestrator"
# Expected: all pass

dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj --filter "LearningEventScheduler"
# Expected: all pass

dotnet test C:\Projects\llm_mvp_core\Tests\LlmFromScratch.Tests.csproj
# Expected: >= 1172 passed, 0 failed
```

### Required Tests

1. Knowledge edge with scheduler threshold 1 increments `QueuedCount`.
2. Same knowledge edge with `UseKnowledgeEdgeDetection = false` does not queue.
3. Same knowledge edge with production threshold 5 and one example does not queue
   but records "below learning threshold".
4. Registered teaching flow is invoked by `RunWithCapabilityGenerationAsync`.
5. No teaching flow means no fake retry and no invented capability.

---

## What This Plan Does Not Build

This plan does not connect projects 00-14 to `ILogicalUnit`.

The LLM stack remains separate until the measured learning-cycle implementation:

```text
BPE tokenizer
embedding model
HNSW/KNN
LoRA
DPO
training metrics
```

This plan makes the orchestrator capable of routing, branching, carrying context,
selecting learned units when registered, and triggering learning events. The
learning-cycle plan makes those units genuinely learned.

---

## File Impact Summary

### Phase 0

Moved:

```text
35.ExperimentsV2\LogicalFlow\InMemoryLogicalFlowRegistry.cs
35.ExperimentsV2\LogicalFlow\InMemoryLogicalUnitRegistry.cs
```

To:

```text
21.AgentInfrastructure\Logical\InMemoryLogicalFlowRegistry.cs
21.AgentInfrastructure\Logical\InMemoryLogicalUnitRegistry.cs
```

Modified:

```text
15.OrchestratorModel\Logical\LogicalOrchestratorAssistant.cs
35.ExperimentsV2 tests and using statements as needed
```

### Phase A

Added:

```text
21.AgentInfrastructure\Logical\PromptRoutingInputCompilerUnit.cs
21.AgentInfrastructure\Logical\AdaptiveLogicalOrchestrator.cs
Tests\AdaptiveLogicalOrchestratorTests.cs
```

Modified:

```text
15.OrchestratorModel\Logical\LogicalOrchestratorAssistant.cs
```

### Phase B

Modified:

```text
21.AgentInfrastructure\Logical\LogicalContracts.cs
21.AgentInfrastructure\Logical\AdaptiveLogicalOrchestrator.cs
```

Added:

```text
21.AgentInfrastructure\Logical\MultiSubPhaseRouterUnit.cs
```

### Phase C

Modified:

```text
21.AgentInfrastructure\Logical\InMemoryLogicalFlowRegistry.cs
21.AgentInfrastructure\Logical\AdaptiveLogicalOrchestrator.cs
```

Added:

```text
21.AgentInfrastructure\Logical\LogicalFlowFactoryContracts.cs
35.ExperimentsV2\LogicalFlow\SecurityVerification34FlowFactory.cs
```

### Phase D

Modified:

```text
21.AgentInfrastructure\Logical\AdaptiveLogicalOrchestrator.cs
15.OrchestratorModel\Logical\LogicalOrchestratorAssistant.cs
```

---

## Executor Rule

Do not begin Phase A before Phase 0 passes.

Do not implement more than one phase per executor session.

Do not connect projects 00-14 in this plan.

Do not fake learned behavior. If no learned unit or teaching flow exists, return
a knowledge edge and schedule learning.

---

## Last Updated

2026-05-05.

