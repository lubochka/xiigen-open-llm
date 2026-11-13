# AdaptiveLogicalOrchestrator — Implementation Plan
# Version: 1.0 — 2026-05-05
# SUPERSEDED: use ADAPTIVE-LOGICAL-ORCHESTRATOR-IMPLEMENTATION-PLAN-v2.md.
# v1 is not executor-ready. It places project-35 registries behind a project-21
# orchestrator, uses the wrong router input type, lacks flow route metadata,
# assumes multi-sub-phase router output that does not exist, and assumes learned
# units can be swapped inside already constructed flows.
# Authority: DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v1.md
# Precondition: 1172/1172 tests passing. No behavior changes until Gate B passes.

---

## What this plan builds

One new class:

```csharp
AdaptiveLogicalOrchestrator : ILogicalOrchestrator
```

Located in: `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs`

It replaces `InMemoryLogicalOrchestrator` as the runtime orchestrator in
`LogicalOrchestratorAssistant.CreateDefault()`. The interface is identical.
The behavior is completely different.

`InMemoryLogicalOrchestrator` stays in the codebase as the deterministic
reference. Tests that use it remain unchanged. Only `CreateDefault()` is updated
to wire the new orchestrator.

---

## What the four phases build in sequence

Phase A — topology-aware flow selection.
Phase B — multi-flow execution with result array.
Phase C — learning flag honour (UseLearnedUnitWhenAvailable + UseContextCarryForward).
Phase D — knowledge edge detection and learning cycle trigger.

Each phase adds one capability. Each phase has one Gate B. No phase begins until
the prior gate passes. Test count must not drop below 1172 at any phase boundary.

---

## Phase A — Topology-Aware Flow Selection

### What is missing

`InMemoryLogicalOrchestrator` selects flows by matching `request.ScenarioId`
against a fixed dictionary built at construction time. The caller must already
know the correct scenario ID. The orchestrator does not read the user request
and does not consult `ILogicalTopologyRegistry`.

### What Phase A builds

`AdaptiveLogicalOrchestrator` holds a reference to `ILogicalTopologyRegistry`.
When `RunAsync` receives a request, it reads the registry to find which
sub-phases and domains are relevant to the request, then selects the appropriate
registered flow.

The selection in Phase A uses the `SubPhaseRouterUnit` that already exists in
`21.AgentInfrastructure/Logical/SubPhaseRouterUnit.cs`. The orchestrator calls
the router unit, takes its output (a `SubPhaseRoutingDecision`), and maps the
routing decision to a registered flow binding.

If no flow is registered for the routed sub-phase, the orchestrator returns a
clean knowledge-edge result rather than an error. The knowledge-edge result
names the missing sub-phase and the missing flow.

### New code

```csharp
// 21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs

public sealed class AdaptiveLogicalOrchestrator : ILogicalOrchestrator
{
    private readonly ILogicalTopologyRegistry _topology;
    private readonly ILogicalUnit<string, SubPhaseRoutingDecision> _router;
    private readonly InMemoryLogicalFlowRegistry _flows;
    private readonly InMemoryLogicalUnitRegistry _units;

    public AdaptiveLogicalOrchestrator(
        ILogicalTopologyRegistry topology,
        ILogicalUnit<string, SubPhaseRoutingDecision> router,
        InMemoryLogicalFlowRegistry flows,
        InMemoryLogicalUnitRegistry units)
    {
        _topology = topology;
        _router   = router;
        _flows    = flows;
        _units    = units;
    }

    public async Task<OperationResult<LogicalOrchestratorResult>> RunAsync(
        LogicalOrchestratorRequest request,
        CancellationToken cancellationToken = default)
    {
        // Phase A: route the request to the correct sub-phase flow.
        // Phase B will extend this to multiple sub-phases.
        // Phase C will add learned unit selection.
        // Phase D will add knowledge edge detection and learning trigger.
    }
}
```

`LogicalOrchestratorResult` is unchanged in Phase A. The orchestrator still
returns one result. Multi-flow execution is Phase B.

### Gate B

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: >= 1172 passed, 0 failed

dotnet test --filter "AdaptiveLogicalOrchestrator" Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: all pass

dotnet test --filter "LogicalOrchestratorAssistant" Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: all pass (prior tests must still pass)
```

Three new tests for Phase A:

Test 1: given a request whose content matches a registered sub-phase,
`AdaptiveLogicalOrchestrator` selects the correct flow without the caller
providing a scenario ID.

Test 2: given a request whose content matches no registered flow,
`AdaptiveLogicalOrchestrator` returns a knowledge-edge result naming the missing
sub-phase. It does not return a generic failure.

Test 3: given the same request routed through `LogicalOrchestratorAssistant.CreateDefault()`
after Phase A wiring, the result scenario ID matches the routed sub-phase, not a
hardcoded default.

---

## Phase B — Multi-Flow Execution with Result Array

### What is missing

`LogicalOrchestratorResult` contains one `LogicalFlowResult`. A request that
spans multiple sub-phases — for example, an O1 request touching sub-phases 3.1,
3.3, 3.4, 4.1, 5.2, and 6.3 — cannot be handled. The orchestrator stops after
the first flow.

### What Phase B builds

`LogicalOrchestratorResult` gains a second field:

```csharp
public sealed record LogicalOrchestratorResult(
    string ScenarioId,
    string SelectedFlowId,
    string OverallVerdict,
    string FinalHumanResult,
    LogicalExecutionFlags AppliedFlags,
    LogicalFlowResult FlowResult,           // kept for backward compat
    LogicalFlowResult[] BranchResults);     // new in Phase B
```

`BranchResults` contains one entry per sub-phase the router identified.
`FlowResult` is the first branch result. Existing tests that read `FlowResult`
still pass without changes.

`AdaptiveLogicalOrchestrator` extends its routing step: after identifying the
primary sub-phase, it asks the router whether the request spans additional
sub-phases. For each identified sub-phase that has a registered flow, it
executes that flow and collects the result into `BranchResults`. Flows that can
run in parallel (determined by the topology registry's transition records) run
concurrently using `Task.WhenAll`. Flows with ordering dependencies run in
sequence.

If any branch returns a knowledge-edge result, the orchestrator does not fail.
It continues the remaining branches and includes the knowledge-edge result in
`BranchResults`.

`OverallVerdict` is computed from the branch results:
- All branches pass → `PASS`
- Any branch returns a concern → `PASS_WITH_CONCERNS`
- Any branch returns a knowledge edge → `PASS_WITH_GAPS`
- Any branch returns reprocess → `REPROCESS`
- Any branch returns reject → `REJECT`

### Gate B

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: >= 1172 passed, 0 failed
```

Three new tests for Phase B:

Test 1: a request touching two registered sub-phases produces
`BranchResults.Length == 2` and `OverallVerdict` derived from both.

Test 2: a request where one sub-phase has a registered flow and one does not
produces `BranchResults.Length == 2`, one a real result and one a knowledge-edge
result, overall verdict `PASS_WITH_GAPS`.

Test 3: a request with two independent sub-phases executes both flows via
`Task.WhenAll` and the elapsed time is less than the sum of sequential
execution times (concurrency is real).

---

## Phase C — Learning Flag Honour

### What is missing

`UseLearnedUnitWhenAvailable` defaults to true across the entire system. Nothing
reads it. `InMemoryLogicalUnitRegistry` has `TrustState` and `IsLearned` fields
on every registered unit. Nothing selects based on them at runtime. A learned
unit registered with `TrustState.Trusted` is indistinguishable from a
deterministic stub during execution.

`UseContextCarryForward` defaults to true. Only one unit in the codebase checks
it (`T34TransitionDomainSplitterUnit`) and only to reject requests where it is
false. No component carries context from one flow's result into the next flow's
input.

### What Phase C builds

**Learned unit selection.** When `AdaptiveLogicalOrchestrator` calls a flow, it
first checks whether a learned unit is registered for each node role in that flow
at `Trusted` or `Probationary` trust state. If yes and
`flags.UseLearnedUnitWhenAvailable` is true, it resolves the learned unit through
`InMemoryLogicalUnitRegistry.ResolveLatest`. The flow uses the learned unit for
that role. If no learned unit exists, the deterministic unit is used. The
selection is transparent to the caller.

The flow interface stays unchanged. The unit selection happens inside the
orchestrator before constructing the flow request. This is the connection between
the unit registry's `TrustState` machinery and the execution path — the first
time `UseLearnedUnitWhenAvailable` is actually honoured.

**Context carry-forward.** When `flags.UseContextCarryForward` is true,
`AdaptiveLogicalOrchestrator` passes the `BranchResults` of completed flows into
the `Metadata` dictionary of subsequent flow requests. Flows that depend on a
prior sub-phase (determined by the topology registry's transition records) receive
the prior flow's key results as context. The downstream flow's units can read
this context through `request.Metadata`.

This is the implementation of the cross-phase context package described in the
orchestration plan. It does not require a new interface — `Metadata` on
`LogicalFlowRequest` already exists and accepts arbitrary objects.

### Gate B

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: >= 1172 passed, 0 failed
```

Three new tests for Phase C:

Test 1: register a learned unit at `Trusted` trust state for a specific role.
Run a request with `UseLearnedUnitWhenAvailable = true`. Verify the learned unit
was called (via evidence ID in the result) rather than the deterministic unit.

Test 2: run the same request with `UseLearnedUnitWhenAvailable = false`. Verify
the deterministic unit was called instead.

Test 3: run a two-branch request where branch 2 depends on branch 1. With
`UseContextCarryForward = true`, verify that branch 2's flow request metadata
contains the key result from branch 1's flow result.

---

## Phase D — Knowledge Edge Detection and Learning Cycle Trigger

### What is missing

When a unit cannot handle its input — because it lacks training, because the
input falls outside its covered territory, or because confidence falls below the
floor — it should produce a knowledge-edge result. `UseKnowledgeEdgeDetection`
defaults to true. Nothing in the orchestrator reads a knowledge-edge result and
responds to it. The gap is silently passed through.

`LearningEventScheduler` exists and correctly queues learning events and drains
them at idle. Nothing in the orchestrator calls it when a knowledge edge is
detected.

### What Phase D builds

**Knowledge edge detection.** After each branch flow completes,
`AdaptiveLogicalOrchestrator` inspects `BranchResults` for knowledge-edge
verdicts. When `flags.UseKnowledgeEdgeDetection` is true and a knowledge-edge
result is present, the orchestrator extracts the named missing competence from
the result's warnings or metrics.

**Learning cycle trigger.** When a knowledge edge is detected,
`AdaptiveLogicalOrchestrator` calls `LearningEventScheduler.QueueIfReady` with
the domain and a count of pending examples (initially 1 — the failed input
itself). `LearningEventScheduler` queues it when the threshold is met. Nothing
else changes in Phase D — the scheduler's `DrainIfIdle` is already built and
handles the rest.

**Missing capability generation path.** Phase D also adds one new method to
`LogicalOrchestratorAssistant`:

```csharp
public async Task<OperationResult<LogicalAssistantRunResult>> RunWithCapabilityGenerationAsync(
    string userRequest,
    LogicalExecutionFlags? flags = null,
    CancellationToken cancellationToken = default)
```

This method calls `RunAsync`, inspects the result for knowledge-edge branches,
and if any exist, attempts to register a teaching flow for the missing sub-phase
and re-runs the branch. The teaching flow is the `TeachingFlow` registered in
the flow registry (if present). If no teaching flow is registered, the method
returns the knowledge-edge result as-is and does not retry.

This is the first implementation of the loop: detect knowledge edge → schedule
learning → if teaching flow exists, run it → retry branch.

### Gate B

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
# Expected: >= 1172 passed, 0 failed
```

Three new tests for Phase D:

Test 1: a request routed to a sub-phase with no registered flow produces a
knowledge-edge result. With `UseKnowledgeEdgeDetection = true`, verify that
`LearningEventScheduler.QueuedCount` increases by 1 after the run.

Test 2: with `UseKnowledgeEdgeDetection = false`, verify that the same request
does not increment `QueuedCount`.

Test 3: call `RunWithCapabilityGenerationAsync` with a request that produces a
knowledge-edge branch. Register a teaching flow before the call. Verify that the
teaching flow was invoked and the result shows an attempted retry rather than a
plain knowledge-edge pass-through.

---

## What this does not build

Phase A through D do not connect the LLM infrastructure (projects 00-14) to
`ILogicalUnit`. That connection — BPE tokenizer to embedding to HNSW to
confidence scoring — is a separate plan (`LEARNING-CYCLE-IMPLEMENTATION-PLAN-v1.md`).

Phases A through D make the orchestrator behave correctly with whatever units are
registered — deterministic or learned. The learning cycle plan makes those units
genuinely learned. Both are needed. Neither replaces the other.

Phase A through D do not touch the nine-node security flow or any experiment in
project 35. They add a new orchestrator that the assistant can use in parallel
with the existing system. The existing tests all continue to pass.

---

## File locations

New file:
`21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs`

Modified files:
`21.AgentInfrastructure/Logical/LogicalContracts.cs` — add `BranchResults` to `LogicalOrchestratorResult`
`15.OrchestratorModel/Logical/LogicalOrchestratorAssistant.cs` — `CreateDefault()` wires `AdaptiveLogicalOrchestrator`

New test file:
`Tests/AdaptiveLogicalOrchestratorTests.cs`

No files in projects 00-14, 22, 23, or 35 are touched.

---

## Execution order for the executor

Phase A first. Gate B must pass before Phase B begins.
Phase B second. Gate B must pass before Phase C begins.
Phase C third. Gate B must pass before Phase D begins.
Phase D last.

Each phase is one executor session. Four sessions total.
Test floor: 1172. Never drops below this number at any phase boundary.

---

# Last updated: 2026-05-05
