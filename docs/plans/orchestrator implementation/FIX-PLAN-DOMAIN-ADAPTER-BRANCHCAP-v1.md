# Fix Plan — Domain Layer, Adapter Architecture, Branch Cap
# Version: 1.0 — 2026-05-06
# Grounded in: code review of commit 191bf8b + 20-row bulk measurement

---

## What is wrong and why it matters

Three things are broken in the teaching path, in dependency order.

**Problem 1 — No domain classification before routing.**
The orchestrator routes every prompt directly to sub-phases using predicate
matching. The D-04 domain classifier (RouteClassifier in 12.SemanticCompiler,
proven at 95% accuracy by E18) is never called. Every prompt is treated as if
domain does not exist. As a result the adapter architecture in D-05 cannot
work — there is nothing to hang a domain adapter on.

**Problem 2 — 114 independent tiny models instead of 5 domain adapters.**
The capability trainer creates one fresh tiny model per sub-phase per row.
Sub-phases 4.4 and 5.1 have no shared weights even though both belong to
CODE_Q / BUG work. D-05 requires one shared base model with five LoRA
adapters — one per domain. The adapter accumulates all learning for that
domain regardless of which sub-phases were touched. ResolveLatest for a
domain returns the adapter that has seen every prompt in that domain, not
an isolated single-sub-phase model.

**Problem 3 — E49 branch expansion has no cap.**
Rows 1 and 16 expanded to 26 branches because the relevance floor
(bestScore × 0.80) is too low for broad predicates like ARCHITECTURE +
DOCUMENT + IMPLEMENT + LEARNING. A documentation prompt correctly touches
5-6 sub-phases, not 26. The missing cap means slow rows and diluted
training signal — 26 branches trained on one sentence each produce 26
weakly-specialized models.

These three must be fixed in order. Domain classification must exist before
the adapter architecture makes sense. The adapter architecture must be
restructured before the branch cap matters for learning quality.

---

## Fix 1 — Domain classification step

### What to add

A domain classification call inside `AdaptiveLogicalOrchestrator`, between
prompt compilation and sub-phase routing. The classifier already exists in
`12.SemanticCompiler/Routing/RouteClassifier.cs`. It needs to be callable
from project 21.

Two options for wiring:

**Option A — Add project reference.**
Add `12.SemanticCompiler` as a project reference in
`AgentInfrastructure.csproj`. Call `RouteClassifier.ClassifyByKeywords`
directly on the compiled prompt text. This is the simplest change.
Risk: project 21 grows a dependency on project 12.

**Option B — Inline keyword classifier.**
Copy the keyword table from `RouteClassifier` into a new lightweight class
`PromptDomainClassifier` inside `21.AgentInfrastructure/Logical/`. Same
logic, no new project reference. Stays self-contained.

Recommended: Option B. Project 21 already avoids depending on 12, 14, 16,
22. Adding project 12 opens the door to circular dependencies later.
The keyword table is four lines — it does not need to import the whole
semantic compiler.

### Where to add it

In `AdaptiveLogicalOrchestrator.RunWithCapabilityGenerationAsync`, after
the prompt compiler call and before `RouteAsync`:

```
compiled prompt
  → PromptDomainClassifier.Classify(compiledText)   ← NEW
  → RouteAsync (sub-phase decisions)
  → ExecuteBranchesAsync
```

The classified domain label (CODE_Q / BUG / PLAN / ARCH / REFACTOR) is
passed through to `LogicalCapabilityTrainingRequest` so the trainer knows
which domain adapter to update.

### Contract change

Add `DomainLabel Domain` to `LogicalCapabilityTrainingRequest`. The trainer
uses this to select and update the correct domain adapter instead of creating
a per-sub-phase model.

### Files to change

- `21.AgentInfrastructure/Logical/AdaptiveLogicalOrchestrator.cs`
  — add domain classification call after prompt compilation.
- `21.AgentInfrastructure/Logical/LogicalCapabilityTrainingContracts.cs`
  — add `DomainLabel Domain` field to `LogicalCapabilityTrainingRequest`.
- `21.AgentInfrastructure/Logical/PromptDomainClassifier.cs` ← NEW FILE
  — inline keyword classifier, same logic as RouteClassifier.ClassifyByKeywords,
  no project 12 dependency.
- `21.AgentInfrastructure/Logical/DomainLabel.cs` ← NEW FILE (if not shared)
  — enum CODE_Q / BUG / PLAN / ARCH / REFACTOR mirroring D-04.

### Test to add

`PromptDomainClassifierTests.cs`:
- "I have a null reference error" → BUG
- "Refactor the session storage layer" → REFACTOR
- "How does the topology seeder work" → CODE_Q
- "Design the flow for multi-tenant routing" → ARCH
- "What phases does the deployment pipeline cover" → PLAN

---

## Fix 2 — Domain adapter architecture

### What to change

`DefaultLogicalCapabilityTrainer` currently keys everything on `subPhaseId`.
It must key the model and checkpoint on `domainLabel` instead.

The teaching cache `_teachingCache` changes from:
```
Dictionary<string subPhaseId, CachedTeachingStack>
```
to:
```
Dictionary<DomainLabel, CachedTeachingStack>
```

Five live models maximum — one per domain — instead of one per sub-phase.

The checkpoint directory changes from:
```
checkpointRoot / runId / subPhaseId /
```
to:
```
checkpointRoot / domainLabel /          ← shared across all rows for this domain
```

The unit ID and flow ID change from sub-phase keyed to domain keyed:
```
generated-capability-{domainLabel}-{runId}
generated-capability-flow-{subPhaseId}   ← flow ID stays sub-phase keyed
```

The flow registration stays sub-phase keyed — the orchestrator still routes
to specific sub-phases. Only the underlying trained model (the adapter) is
domain keyed. This is the correct separation: routing is fine-grained,
learning is domain-grained.

### What the training loop looks like after the change

For each branch in the row:
1. Extract sub-phase ID from branch (unchanged).
2. Look up domain label from the request (new — comes from Fix 1).
3. Check warm cache for that domain label.
4. If cached: use the live domain model directly, no disk read.
5. If not cached: load domain checkpoint from disk, or init fresh if first row.
6. Run one DPO step on the domain model using this sub-phase's training text.
7. Save domain checkpoint to disk.
8. Register the learned unit under (domain, sub-phase) combined key.
9. Register the flow under sub-phase ID (unchanged).
10. Update warm cache for this domain.

Multiple branches in the same row that belong to the same domain share one
model. For row 1 with 26 branches: if all 26 sub-phases are classified as
CODE_Q, they all train the same CODE_Q model in sequence, each DPO step
building on the previous one. The result is one model that has seen all 26
sub-phases in one pass, not 26 disconnected models.

### Replay buffer wiring (D-06)

The `ReplayBuffer` from project 16 cannot be referenced directly (project 21
does not reference project 16). Implement the 70/30 logic inline:

```csharp
// In DefaultLogicalCapabilityTrainer:
private readonly Dictionary<DomainLabel, List<string>> _replayTexts = new();

// Before each DPO step for domain D:
// 1. Add current trainingText to _replayTexts[D]
// 2. If _replayTexts[D].Count >= 4, sample 30% from prior texts
// 3. Build DPO batch: [currentExample] + [sampledPriorExamples]
// 4. Run DPO on the mixed batch
```

This is the E43 algorithm inline without the project 16 dependency. The
replay list per domain grows across rows in the same session (warm cache
lifetime). Between sessions it starts fresh — the checkpoint carries the
weights, the replay list is rebuilt as rows are re-taught.

### Reference model caching

The reference model is currently constructed fresh per branch:
```csharp
var referenceModel = new LlmModel(BuildTinyConfig());  // line 94 — wrong
```

Change to one reference model per domain, cached at session level alongside
the policy model:

```csharp
private sealed record CachedTeachingStack(
    LlmModel PolicyModel,
    LlmModel ReferenceModel,    ← ADD THIS
    BpeTokenizer Tokenizer,
    GeneratedCapabilityCheckpointManifest Manifest);
```

The reference model is frozen at domain initialization and never updated.
It provides the baseline for DPO loss. Constructing it once per domain
instead of once per branch eliminates 26 redundant model constructions for
a 26-branch row.

### Unit registry key change

`InMemoryLogicalUnitRegistry.ResolveLatest` currently resolves by
(role, subPhaseId). After this change it resolves by (role, domainLabel)
for the trained model, but the flow registry still resolves by sub-phase.
The flow factory uses the domain-level unit to execute sub-phase work.

`GeneratedCapabilityFlowFactory.Create` changes to:
```
resolve domain label for this sub-phase
→ load domain unit from registry
→ create GeneratedCapabilityFlow backed by domain unit
```

### Files to change

- `21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs`
  — change cache key, checkpoint path, unit ID to domain-keyed.
  — add inline 70/30 replay buffer per domain.
  — add reference model to CachedTeachingStack.
- `21.AgentInfrastructure/Logical/GeneratedCapabilityFlowFactory.cs`
  — resolve domain unit instead of sub-phase unit.
- `21.AgentInfrastructure/Logical/TeachingRegistrySnapshot.cs`
  — update UnitRegistryEntry to carry DomainLabel alongside SubPhaseId.
- `21.AgentInfrastructure/Logical/LogicalCapabilityTrainingContracts.cs`
  — add sub-phase-to-domain mapping helper.
- `Tests/DefaultLogicalCapabilityTrainerTests.cs`
  — update tests to verify domain-keyed caching and 70/30 replay.

### Tests to add

- Two rows for the same domain (BUG) touching different sub-phases
  (4.1 and 5.4) must produce one domain model, not two.
- A third row for the same domain must load the cached domain model and
  show TrainingStepCount = 3, not 1.
- The 70/30 replay: after 4 rows for BUG, the DPO batch for row 5 must
  contain the current example plus at least one prior BUG example.
- A row for a different domain (ARCH) must produce a separate model and
  not affect the BUG model weights.

---

## Fix 3 — Branch cap for E49 expansion

### The problem precisely

`MultiSubPhaseRouterUnit` keeps all candidates above `bestScore × 0.80`.
For a prompt with score 0.25 across all sub-phases (broad, weak signal),
the floor is `0.20` and everything above 0.20 survives. That is why
26 branches pass for a documentation prompt.

### The fix

After the relevance floor filter, add a hard cap:

```csharp
// After: scored = scored.Where(candidate => candidate.Score >= relevanceFloor).ToArray();

// Cap at MaxBranches to prevent slow rows and diluted training signal.
// Take the highest-scoring candidates only when the cap is hit.
const int MaxBranches = 8;
if (scored.Length > MaxBranches)
    scored = scored.Take(MaxBranches).ToArray();
```

Cap value: 8. Rationale: the 20-row bulk shows accepted rows have 1-6
branches. The review rows had 6-26. A cap of 8 keeps all accepted rows
within budget and cuts rows 1 and 16 from 26 branches to 8, reducing
their training time from 16-40s to approximately 5-10s.

### Where to add it

`21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs`, line ~148,
after the relevance floor filter and before building decisions.

### Files to change

- `21.AgentInfrastructure/Logical/MultiSubPhaseRouterUnit.cs`
  — add `const int MaxBranches = 8` and `Take(MaxBranches)` after floor filter.
- `Tests/MultiSubPhaseRouterUnitTests.cs`
  — add test: broad prompt that scores all 31 sub-phases must return at most 8.

---

## Execution order

Fix 3 (branch cap) can be done independently and immediately — it is one
line of code and one test. It stops the bleeding on slow rows without
touching the learning architecture.

Fix 1 (domain classification) is a prerequisite for Fix 2. The domain label
must flow through the request before the trainer can key on it.

Fix 2 (domain adapter) depends on Fix 1. Do not start Fix 2 until Fix 1
is in and tested.

Sequence:
1. Fix 3 — branch cap — one sitting, one commit.
2. Fix 1 — domain classifier — one sitting, one commit.
3. Fix 2 — domain adapter restructure — two sittings, two commits
   (first: restructure trainer + cache; second: wire replay buffer and
   reference model caching).

Test floor: 1220 must not drop at any commit.

---

## Expected result after all three fixes

| Metric                  | Before fixes    | After fixes     |
|-------------------------|-----------------|-----------------|
| Max branches per row    | 26              | 8               |
| Max row time            | 40.15s          | ~10s            |
| Learned models          | 1 per sub-phase | 1 per domain (5 total) |
| Training step count     | per sub-phase   | per domain (accumulated across all sub-phases in domain) |
| Forgetting prevention   | none            | 70/30 replay per domain |
| Reference model cost    | 1 per branch    | 1 per domain    |
| Domain classification   | missing         | before routing  |

---

# Last updated: 2026-05-06
