# Plan — Flow-Driven Phase Transitions via DomainClassifierUnit Re-Entry
# Version: 1.0 — 2026-05-06
# Session type: ARCHITECT
# Test floor entering this plan: 1224
# Companion to: PLAN-CONTEXT-ENRICHED-ROUTING-v1.md
#               PLAN-DOMAIN-CLASSIFIER-SINGLE-SOURCE-v1.md

---

## What this plan is

Any logical unit at any position in the nine-node flow can determine that the
work it is processing belongs to a different domain and must transition there.
A planner at node 03 might determine that a security review is required before
implementation can proceed. A processor at node 04 might discover a bug that
must be fixed before the plan continues. A result translator at node 06 might
produce output that is clearly QA work. An arbiter at node 08 might conclude
that development is complete and QA must begin.

In the current implementation, none of these signals exist. Every node produces
its result and returns. The orchestrator aggregates all branch results and hands
them to the caller. There is no mechanism for any node to say "the next domain
is QA" and have the orchestrator act on that.

This plan adds that mechanism. It is intentionally simple:

- Any `LogicalUnitResult` can carry an optional `PhaseTransitionSignal`.
- The signal carries the next domain label and a synthesised prompt that
  describes why the transition is needed.
- After all branches complete, the orchestrator reads transition signals,
  feeds each `TransitionPrompt` back into `DomainClassifierUnit` (full
  classification cycle — not a sub-phase shortcut), and executes the
  resulting new branches.
- The new branches' results are appended to the aggregate result.

The domain classifier re-classification is not decorative. If a node says
"next domain is QA" but the transition prompt classifies as BUG, that
mismatch surfaces as a concern in the new branch rather than routing
blindly to QA. The classifier's evidence is authoritative. The node's
signal is advisory.

---

## The transition signal contract

### New record: `PhaseTransitionSignal`

```csharp
// This record carries a phase transition request from any logical unit in a flow.
// Any node at any position may emit one. The orchestrator reads it after all
// branches complete and re-enters DomainClassifierUnit with the TransitionPrompt.
public sealed record PhaseTransitionSignal(
    // The domain this node believes the work should transition to.
    // Used as a hint — DomainClassifierUnit re-classifies independently.
    Domain SuggestedDomain,

    // The synthesised text that DomainClassifierUnit receives as its input.
    // Must be self-contained: it describes what was produced and why it needs
    // the suggested domain. Do not include raw source code or long lists.
    // Example: "Security review required: JWT validation implemented in
    //   UserService but tenant isolation constraint is unverified. QA must
    //   confirm cross-tenant isolation before this branch is accepted."
    string TransitionPrompt,

    // The node that emitted this signal. Used for tracing and evidence.
    string SourceNodeId,

    // The sub-phase of the branch that produced this signal.
    string SourceSubPhaseId,

    // Evidence IDs from the producing node that justify the transition.
    string[] EvidenceIds);
```

### Change to `LogicalUnitResult<TOutput>`

Add one optional field:

```csharp
public sealed record LogicalUnitResult<TOutput>(
    string UnitId,
    string UnitRole,
    string DomainTag,
    TOutput Output,
    double Confidence,
    string[] PredicateIds,
    string[] DomainTags,
    string[] EvidenceIds,
    string[] Warnings,
    PhaseTransitionSignal? TransitionSignal = null);  // NEW — null means no transition
```

Adding a nullable field with a default of `null` is backward-compatible. Every
existing unit that does not set this field continues to work without change.

### Change to `LogicalFlowResult`

Add one field to carry transition signals from all nodes in the flow:

```csharp
public sealed record LogicalFlowResult(
    string FlowId,
    string ScenarioId,
    string OverallVerdict,
    string FinalHumanResult,
    LogicalFlowSummary Summary,
    IReadOnlyDictionary<string, string> Metrics,
    string[] EvidenceIds,
    string[] Warnings,
    PhaseTransitionSignal[] TransitionSignals = []);  // NEW — empty means no transitions
```

The nine-node flow collects `TransitionSignal` from every node result that
is non-null and surfaces them on `LogicalFlowResult.TransitionSignals`.
The orchestrator reads them after branch execution completes.

---

## The orchestrator re-entry loop

### Change to `AdaptiveLogicalOrchestrator.RunAsync`

After `ExecuteBranchesAsync` returns and before `BuildAggregateResult`,
add the transition loop:

```
Algorithm — transition loop:

1. Collect all PhaseTransitionSignal values from all branch results.
   signals = branches.SelectMany(b => b.TransitionSignals).ToList()

2. If signals is empty: proceed to BuildAggregateResult as today.

3. For each signal in signals:

   a. Build a new DomainClassifierRequest:
      Prompt       = signal.TransitionPrompt
      Predicates   = null  (re-compile from prompt)
      PriorContext = current run's LogicalContextPacket (carries completed branches)

   b. Resolve DomainClassifierUnit from registry by role "domain_classifier".
      Call ExecuteAsync(DomainClassifierRequest).
      → DomainClassification (Label, Confidence, Evidence, UsedContext)

   c. If Confidence < 0.40:
      Add a warning to the aggregate result:
        "Transition signal from {signal.SourceNodeId} classified with low
         confidence ({Confidence:P0}). Suggested domain: {signal.SuggestedDomain}.
         Classified domain: {Label}. Transition skipped."
      Continue to next signal. Do not route.

   d. If Label != signal.SuggestedDomain:
      Add a concern to the aggregate result:
        "Domain mismatch on transition from {signal.SourceNodeId}.
         Suggested: {signal.SuggestedDomain}. Classified: {Label}.
         Routing to classified domain {Label}."
      Proceed with classified domain, not suggested domain.
      (The classifier's evidence is authoritative.)

   e. Build a new LogicalOrchestratorRequest:
      UserRequest = signal.TransitionPrompt
      Metadata    = current context packet + source signal evidence
      RunId       = current RunId + "-transition-" + signalIndex

   f. Call RunAsync recursively on the new request.
      Depth guard: if transition depth > 3, emit a concern and stop.
      This prevents runaway chains.

   g. Append the recursive result's BranchResults to the current aggregate.

4. BuildAggregateResult from all original branches + all transition branches.
```

The recursive call to `RunAsync` means the transition branch goes through the
full cycle: domain classifier → sub-phase router → domain adapter → nine-node
flow → its own arbiters. If that flow emits its own transition signals, those
are also followed, up to the depth limit of 3.

---

## How any node emits a transition signal

A node emits a transition signal by returning a non-null `TransitionSignal`
on its `LogicalUnitResult`. This requires no new interface, no new base class,
no new registry entry. The unit simply populates the field.

### Example — node 03 scheme planner detecting a security gap

```csharp
// Scheme planner determines that a security review is required before
// implementation can proceed. It emits a transition signal to ARCH domain.
return new LogicalUnitResult<WorkBranch[]>(
    UnitId:    SchemePlannerUnit.Id,
    UnitRole:  SchemePlannerUnit.Role,
    DomainTag: SchemePlannerUnit.DomainTag,
    Output:    workBranches,
    Confidence: 0.88,
    PredicateIds: ["SECURITY", "ARCHITECTURE"],
    DomainTags: ["3.4"],
    EvidenceIds: ["security-gap-unverified-jwt"],
    Warnings:  [],
    TransitionSignal: new PhaseTransitionSignal(
        SuggestedDomain:  Domain.ARCH,
        TransitionPrompt: "Security review required before implementation: " +
            "JWT validation is planned but tenant isolation constraint is unverified. " +
            "Architecture must confirm the isolation boundary before Phase 4 begins.",
        SourceNodeId:     "node-03-scheme-planner",
        SourceSubPhaseId: "4.1",
        EvidenceIds:      ["security-gap-unverified-jwt"]));
```

### Example — node 08 arbiter signalling QA readiness

```csharp
// Arbiter confirms development is complete. Signals transition to QA domain.
return new LogicalUnitResult<ArbiterVerdict>(
    ...
    TransitionSignal: new PhaseTransitionSignal(
        SuggestedDomain:  Domain.CODE_Q,  // QA maps to CODE_Q in D-04
        TransitionPrompt: "Development complete for UserService async fix. " +
            "Unit tests pass. Integration tests not yet run. " +
            "QA review required for regression and compatibility coverage.",
        SourceNodeId:     "node-08-arbiter",
        SourceSubPhaseId: "4.1",
        EvidenceIds:      ["unit-tests-pass", "integration-tests-pending"]));
```

The `TransitionPrompt` is always human-readable, self-contained text. It is
what `DomainClassifierUnit` classifies. It must describe the situation clearly
enough that the classifier can independently confirm the domain.

---

## The nine-node flow collects transition signals

### Change to `UniversalNineNodeDevelopmentFlow`

After each node executes, check for a transition signal and accumulate:

```
Algorithm — collect transition signals:

transitionSignals = new List<PhaseTransitionSignal>()

after each node result:
  if result.TransitionSignal is not null:
    transitionSignals.Add(result.TransitionSignal)

When building LogicalFlowResult:
  TransitionSignals = transitionSignals.ToArray()
```

This is the only change to the nine-node flow. Each node still executes
in the same order. Transition signals do not interrupt the current flow —
they are collected and surfaced. The orchestrator acts on them after the
full flow completes.

This is intentional. A node should not halt the current branch to trigger
a transition. It should complete its own work, signal what it observed,
and let the orchestrator coordinate the next cycle. This preserves the
clean separation between flow execution and orchestration.

---

## The depth guard

```
MaxTransitionDepth = 3

The orchestrator tracks transition depth via metadata:
  metadata["transition.depth"] = current depth (0 if not set)

Before following a transition signal:
  if metadata["transition.depth"] >= MaxTransitionDepth:
    emit concern: "Transition depth limit reached. Signal from {node} skipped."
    continue to next signal.

Each recursive RunAsync call increments the depth in its metadata.
```

A depth of 3 allows:
- development → security review → architecture fix → QA

A depth of 4 or more is almost certainly a cycle and must be blocked.

---

## Baseline gate

```bash
cd C:\Projects\llm_mvp_core
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Record the count. Every gate must match or exceed it.

---

## Phase 1 — Add PhaseTransitionSignal record and extend contracts

**File:** `21.AgentInfrastructure/Logical/LogicalContracts.cs`

Add `PhaseTransitionSignal` record. Add nullable `TransitionSignal` to
`LogicalUnitResult<TOutput>`. Add `TransitionSignals` array to `LogicalFlowResult`.

All new fields have defaults (`null` and `[]` respectively). No existing
code breaks. The change is purely additive.

### Gate B — Phase 1

```bash
dotnet build LlmFromScratch.sln 2>&1 | grep "error" | head -10
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: zero errors, floor unchanged.

New tests:
- `PhaseTransitionSignal` can be constructed with all required fields
- `LogicalUnitResult` with null `TransitionSignal` is valid (backward compat)
- `LogicalFlowResult` with empty `TransitionSignals` is valid (backward compat)

---

## Phase 2 — Collect transition signals in UniversalNineNodeDevelopmentFlow

**File:** `21.AgentInfrastructure/Logical/UniversalNineNodeDevelopmentFlow.cs`

After each node executes, accumulate non-null `TransitionSignal` values.
Pass the collected array to `LogicalFlowResult` as `TransitionSignals`.

No node behaviour changes. No node is added or removed. Existing scaffold
nodes return null `TransitionSignal` implicitly — no update needed.

### Gate B — Phase 2

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: floor unchanged.

New tests:
- A flow where no node emits a signal produces empty `TransitionSignals`
- A flow where one node emits a signal surfaces it on `LogicalFlowResult`
- Multiple signals from multiple nodes are all collected

---

## Phase 3 — Transition loop in AdaptiveLogicalOrchestrator

**File:** `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs`

Add the transition loop after `ExecuteBranchesAsync` returns. The loop:

1. Collects all `PhaseTransitionSignal` values from all branch results.
2. For each signal, builds a `DomainClassifierRequest` using the
   `TransitionPrompt` as the prompt and the current context packet as
   prior context.
3. Resolves `DomainClassifierUnit` from the unit registry by role
   `"domain_classifier"`.
4. Calls `ExecuteAsync`. Checks confidence threshold (0.40 floor).
5. Checks domain match. Logs mismatch as concern if it differs.
6. Checks depth guard. Stops if depth ≥ 3.
7. Calls `RunAsync` recursively with the transition prompt.
8. Appends results to the current aggregate.

### Gate B — Phase 3

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New tests:
- No transition signals → orchestrator returns result unchanged (regression)
- One signal with high confidence → transition branch executes and appends
- One signal with confidence below 0.40 → warning emitted, no branch executes
- Signal domain differs from classified domain → concern emitted, classified
  domain used (not suggested domain)
- Transition depth > 3 → concern emitted, further transitions blocked
- Transition branch itself emits a signal → followed up to depth limit
- Two signals from different nodes → two transition branches execute

---

## Phase 4 — Full suite verification and commit

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New test count:
- Phase 1 (contracts): 3 tests
- Phase 2 (nine-node collection): 3 tests
- Phase 3 (orchestrator loop): 7 tests
- Total new: 13 tests
- New floor: 1224 + 13 = 1237

Commit message:
```
flow-driven phase transitions via DomainClassifierUnit re-entry

Any logical unit at any node can now signal a phase transition.

New contract:
  PhaseTransitionSignal — SuggestedDomain, TransitionPrompt, SourceNodeId,
    SourceSubPhaseId, EvidenceIds
  LogicalUnitResult.TransitionSignal — nullable, defaults to null (backward compat)
  LogicalFlowResult.TransitionSignals — array, defaults to [] (backward compat)

Nine-node flow:
  Collects non-null TransitionSignal from every node after it executes.
  Surfaces all on LogicalFlowResult.TransitionSignals.
  Does not interrupt the current flow — collection only.

Orchestrator transition loop (after ExecuteBranchesAsync):
  For each TransitionSignal:
    Re-enters DomainClassifierUnit with TransitionPrompt (full cycle)
    Confidence floor 0.40 — low-confidence signals emit warning, skip
    Domain mismatch → concern emitted, classifier's domain used (not suggested)
    Depth guard MaxTransitionDepth=3 — prevents runaway chains
    Recursive RunAsync appends transition branches to aggregate result

No existing tests broken — all new fields are additive with defaults.
Floor raises from 1224 to 1237.
```

---

## Execution order and stop conditions

Phases in strict order: baseline gate → 1 → 2 → 3 → 4.

Stop immediately if:
- Phase 1 Gate B shows any existing test failing (contract change broke something)
- Phase 3 regression test fails (no-signal path changed)
- A test is modified to suppress a genuine regression

---

## What this plan does NOT change

| Component | Unchanged |
|---|---|
| `DomainClassifierUnit` contract | Unchanged — receives `DomainClassifierRequest`, returns `DomainClassification` |
| `MultiSubPhaseRouterUnit` | Unchanged — transition branches go through full routing cycle |
| Sub-phase resolution | Unchanged — stays in router, not in signal |
| Domain adapters | Unchanged — transition branch uses the adapter for its classified domain |
| Existing unit implementations | Unchanged — null `TransitionSignal` is the default |
| Knowledge edge retry | Unchanged — separate mechanism, unaffected |
| Learning path | Unchanged — transition branches train the same domain adapters |

---

## Relationship to other plans

**PLAN-DOMAIN-CLASSIFIER-SINGLE-SOURCE-v1.md** must execute first.
The transition loop resolves `DomainClassifierUnit` from the unit registry
by role `"domain_classifier"`. That registration happens in the domain
classifier plan. Without it, the registry resolve fails.

**PLAN-CONTEXT-ENRICHED-ROUTING-v1.md** is a companion.
The transition loop passes the current context packet as `PriorContext` to
`DomainClassifierUnit`. The context biasing from the enriched routing plan
means the classifier sees not only the transition prompt but also what the
completed branches concluded. This makes the domain classification of a
transition prompt more accurate than classifying the same text in isolation.

**PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v4.md** is independent.
This plan does not depend on v4 phases. The transition mechanism works with
the current hardcoded keyword classifier. When v4 wires the full
`SemanticCompilerPipeline`, the transition prompt benefits automatically —
the same `DomainClassifierUnit` call now runs the full pipeline.

Safe execution order:
  v4 Phase 0 → domain classifier plan → this plan → context routing plan → v4 phases 1-8

---

# Plan version: 1.0
# Author: ARCHITECT session 2026-05-06
# Phases: 4
# New records: PhaseTransitionSignal
# Modified records: LogicalUnitResult (additive), LogicalFlowResult (additive)
# Modified files: LogicalContracts.cs, UniversalNineNodeDevelopmentFlow.cs,
#                 AdaptiveLogicalOrchestrator.cs
# New tests: 13
# New floor: 1237
# Key property: any node at any position can signal a phase transition
# Key safety: depth guard 3, confidence floor 0.40, classifier authority over suggestion
