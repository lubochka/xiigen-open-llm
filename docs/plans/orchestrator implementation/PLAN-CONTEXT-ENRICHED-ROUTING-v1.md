# Plan — Context-Enriched Routing and Attributed Segment Routing
# Version: 1.0 — 2026-05-06
# Session type: ARCHITECT
# Builds on: PLAN-WIRE-SEMANTIC-COMPILER-INTO-ORCHESTRATION-v4.md (in execution)
# Test floor entering this plan: 1224

---

## What this plan is

Version 4 is in execution. This is a separate, independent plan covering two
architectural decisions reached in the session of 2026-05-06:

**Decision 1 — No Layer 1 transition orchestrator.**
The topology transition concept was rejected. A separate Layer 1 orchestrator with
its own transition type classifier would create two independent learning paths for
what is fundamentally the same domain knowledge. A developer specifying a product
page and a developer implementing a product page from a spec are both doing ARCH or
PLAN work — same domain, same adapter. The difference is prior context, not a
different model.

The correct design is: one domain classifier, one router, one set of adapters.
Prior context from a completed run is carried as a `LogicalContextPacket` into the
next run and used to bias scoring — it enriches the existing classifier and router
rather than forking them.

**Decision 2 — E24 attributed routing is safe in the new architecture.**
E24 failed in its original experiment (-37%) because it was routing prompt slices
to six simultaneous abstract datasets in one batch. A false conjunction split gave
each dataset a shorter garbled fragment, which was noisier than the full prompt.

In the new architecture that failure condition cannot arise. Each segment from E21's
Decomposer goes to exactly one domain adapter — its own independent branch. There
is no simultaneous multi-dataset distribution. A false split produces a mild
redundant branch, not garbled input. E24's attributed routing is therefore safe
and should be added alongside E21.

---

## The two changes this plan makes

### Change A — Context-enriched domain classification and routing

```
Session N result:
  LogicalOrchestratorResult
    → packaged as LogicalContextPacket
    → caller stores and passes on next call (optional)

Session N+1 with prior context:

raw prompt + LogicalContextPacket (optional)
  → DomainClassifierUnit
      reads:  prompt text
      reads:  prior context fragments (domains, verdicts, evidence IDs)
      biases: toward domains that appear in prior accepted fragments
      output: DomainClassification with context-boosted confidence

  → MultiSubPhaseRouterUnit
      reads:  compiled predicates from segment
      reads:  prior context sub-phase IDs
      biases: sub-phases that appear in prior context score higher
              sub-phases already PASS in prior context score lower
              (avoid re-doing completed work)
      output: SubPhaseRoutingDecision[] reweighted by context

  → domain adapter executes (same model, no fork)
  → result packaged as new LogicalContextPacket for next call
```

The key properties:
- No prior context → system behaves exactly as today (stateless, no regression)
- Prior context present → classifier and router are biased, not replaced
- Same domain adapter regardless of whether context was passed
- Learning path is single: all BUG work trains the BUG adapter whether or not
  a context packet was present at routing time

### Change B — E24 attributed routing per segment per domain

```
raw prompt
  → E21 Decomposer.Split → [segment 1, segment 2, segment 3]

  For each segment independently:
    → DomainClassifierUnit(segment text + context packet)
        → domain label for THIS segment only
    → SemanticCompilerAdapterUnit(segment text)
        → predicates for THIS segment only
    → MultiSubPhaseRouterUnit(segment predicates + context)
        → sub-phases for THIS segment only
    → one branch: domain adapter for this segment's domain
                  sub-phase flow for this segment's sub-phase

  → aggregate all segment branches into one result
  → package as LogicalContextPacket for next call
```

The one-segment-to-one-domain contract is what makes E24 safe here.
Each domain adapter sees only the text that belongs to it. There is no
noise from other segments because other segments never reach this adapter.

---

## What exists and what must be built

### Already exists

- `LogicalContextPacket` — record in `LogicalContracts.cs`, fully defined
- `BuildContextPacket` — method in `AdaptiveLogicalOrchestrator`, already builds
  the packet from completed branches and stores it in metadata under `"context.packet"`
- `UseContextCarryForward` — flag in `LogicalExecutionFlags`, defaults to true
- `LogicalOrchestratorRequest.Metadata` — carries the context packet already
- `PromptSegmenterUnit` — built in Phase 1 of v4 plan
- `DomainClassifierUnit` — built in Phase 1 of v4 plan
- `SemanticCompilerAdapterUnit` — built in Phase 1 of v4 plan

### Must be built

**`ContextPacketReader`** — static helper in project 23
Reads `LogicalContextPacket` from request metadata and extracts signals for
the classifier and router. Isolates the reading logic so classifier and router
do not couple to the metadata key string directly.

**`AttributedSegmentRouter`** — new class in project 23
Implements the per-segment-per-domain attributed routing from E24. Receives
`PromptSegment[]` from the Decomposer and returns one
`SubPhaseRouterSourceDocument` per segment, each compiled independently with
its own domain label.

Changes to `PromptRoutingInputCompilerUnit` — reads context packet,
passes it to domain classifier and router.

Changes to `MultiSubPhaseRouterUnit` — accepts optional context packet,
applies bias to scoring when present.

Changes to `AdaptiveLogicalOrchestrator` — exposes the context packet
on the outgoing `LogicalOrchestratorResult` so callers can store and pass it.

---

## Baseline gate

```bash
cd C:\Projects\llm_mvp_core
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

Expected: current floor passing, 0 failing.
Run this live. Do not use a remembered count.

---

## Phase 1 — ContextPacketReader and context-biased DomainClassifierUnit

### File: `23.SemanticCompilerAdapter/ContextPacketReader.cs`

```
Algorithm — ContextPacketReader.Read(metadata):

1. Look up "context.packet" key in metadata dictionary.
2. If absent or null: return ContextSignals.Empty.
3. Cast to LogicalContextPacket.
4. Extract signals:

   PriorDomains: distinct Domain values from all Fragments that have
     OverallVerdict == "PASS" or "CONCERN"
     (PASS = known good, CONCERN = known partial — both inform routing)

   CompletedSubPhases: SubPhaseId values from Fragments with verdict PASS
     (these score lower in routing — work already done)

   ActiveKnowledgeEdges: EvidenceIds containing "knowledge_edge:" prefix
     (these score higher — system knows they need attention)

   PriorConfidenceMap: Domain → average confidence across prior fragments
     (used to weight domain biasing)

5. Return ContextSignals(PriorDomains, CompletedSubPhases,
     ActiveKnowledgeEdges, PriorConfidenceMap).
```

### Changes to `DomainClassifierUnit` (project 23)

Add optional `ContextSignals` parameter to the unit's execution path:

```
Algorithm — context-biased domain classification:

1. Run existing RouteClassifier.ClassifyByKeywords(promptText).
   → BaseClassification (label, confidence, evidence)

2. If ContextSignals.IsEmpty: return BaseClassification unchanged.

3. If BaseClassification.Label appears in PriorDomains:
   confidence boost = +0.10 × (PriorConfidenceMap[label] / 1.0)
   (reinforces domain continuity — same session is likely same domain)

4. If BaseClassification.Label does NOT appear in PriorDomains
   and PriorDomains is non-empty:
   confidence reduction = -0.05
   (mild penalty — unexpected domain shift may be intentional, do not block)

5. Clamp final confidence to [0.20, 0.99].
6. Return adjusted DomainClassification.

The boost is small by design. The domain classifier's own evidence is primary.
Context is a hint, not an override.
```

### Gate B — Phase 1

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New tests:
- ContextPacketReader returns Empty when metadata has no context key
- ContextPacketReader extracts prior domains from PASS fragments
- ContextPacketReader marks completed sub-phases from PASS verdicts
- DomainClassifierUnit boosts confidence for domain matching prior context
- DomainClassifierUnit without context produces identical result to today
- Confidence is clamped and never exceeds 0.99

---

## Phase 2 — Context-biased MultiSubPhaseRouterUnit

### Changes to `MultiSubPhaseRouterUnit` (project 21)

The router's `Score` method currently computes predicate overlap only.
Add context signal as a third scoring component:

```
New scoring formula:

combined_score =
    0.60 × predicate_overlap_score     (was 0.70 — reduced to make room)
  + 0.25 × retrieval_score             (BM25+KNN from HybridRanker, v4 Phase 3)
  + 0.15 × context_signal_score        (new — from ContextPacketReader)

context_signal_score for sub-phase SP:
  +1.0  if SP.SubPhaseId is in ActiveKnowledgeEdges
         (known gap — system should address it)
  +0.5  if SP.Domain matches a PriorDomain
         (domain continuity — same session direction)
  -0.5  if SP.SubPhaseId is in CompletedSubPhases
         (already done — deprioritise unless re-triggered by strong predicate match)
   0.0  otherwise

When ContextSignals.IsEmpty: context_signal_score = 0 for all sub-phases.
Combined score degrades to 0.60 × predicate + 0.25 × retrieval + 0.15 × 0
= weights renormalise to 0.71 predicate + 0.29 retrieval, approximately
matching the prior 0.70/0.30 split.

This means: no context packet → router behaves as in v4 plan. Exact regression
safety. Prior context → routing is biased toward known gaps and session direction.
```

### Gate B — Phase 2

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New tests:
- Router without context packet produces same ordering as before (regression safety)
- Router with prior PASS fragment deprioritises completed sub-phase
- Router with knowledge edge fragment boosts that sub-phase
- Router with prior domain context boosts sub-phases in that domain
- Completed sub-phase can still be selected if predicate overlap is very strong
  (the penalty is not a block, just a weight reduction)

---

## Phase 3 — AttributedSegmentRouter and E24 per-segment-per-domain routing

### File: `23.SemanticCompilerAdapter/AttributedSegmentRouter.cs`

```
Algorithm — AttributedSegmentRouter.Route(segments[], contextSignals):

Input:  PromptSegment[] from E21 Decomposer (one or many)
        ContextSignals from prior session (may be empty)

For each segment:

  1. Call SemanticCompilerAdapterUnit.ExecuteAsync(segment.Text)
     → PipelineOutput (domain, predicates[], stack)

  2. Call DomainClassifierUnit.ExecuteAsync(segment.Text, contextSignals)
     → DomainClassification (label, confidence, evidence)

  3. Construct SubPhaseRouterSourceDocument:
     - CompiledText    = segment.Text (only this segment, not the full prompt)
     - CompiledPredicateIds = PipelineOutput.Predicates
     - DomainLabel     = DomainClassification.Label.ToString()
     - DocumentId      = segment-scoped ID (RunId + segment index)

Output: SubPhaseRouterSourceDocument[] — one per segment

Key property: each document contains only the text of its segment.
The domain adapter for ARCH never sees the BUG segment text.
The domain adapter for BUG never sees the ARCH segment text.
This is the E24 safety guarantee in this architecture.
```

### Changes to `PromptRoutingInputCompilerUnit` (project 21)

Replace current single-document compilation with attributed segment routing:

```
New ExecuteAsync algorithm:

1. Read context packet from request.Metadata via ContextPacketReader.
   → ContextSignals (may be empty if first session)

2. Call ComplexityClassifierUnit.ExecuteAsync(prompt)
   → SINGLE_STEP or MULTI_STEP (E19)

3. If SINGLE_STEP:
   Call SemanticCompilerAdapterUnit + DomainClassifierUnit on full prompt.
   Return one SubPhaseRouterSourceDocument.
   (Fast path — no segmentation overhead for simple prompts)

4. If MULTI_STEP:
   Call PromptSegmenterUnit.ExecuteAsync(prompt) → PromptSegment[]
   If one segment returned: treat as SINGLE_STEP (Decomposer found no boundary).
   If multiple segments: call AttributedSegmentRouter.Route(segments, contextSignals)
   Return SubPhaseRouterSourceDocument[] — one per segment.

5. Each returned document carries:
   - its segment text only
   - its own domain label
   - its own predicate set
   - the context signals so the router can apply bias
```

### Gate B — Phase 3

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New tests:
- Single-task prompt returns one source document (no unnecessary split)
- Multi-task prompt returns N documents, one per detected segment
- Each document contains only its segment's text, not the full prompt
- Domain labels on different segments can differ (ARCH segment ≠ BUG segment)
- Predicate sets on different segments are independently computed
- False split (two very similar segments) produces mild redundancy, not garbled input
  (both documents should route to same sub-phase, one becomes knowledge edge — acceptable)

---

## Phase 4 — Expose LogicalContextPacket on LogicalOrchestratorResult

Currently `BuildContextPacket` builds the packet and stores it inside the
metadata of in-flight branch requests. The caller never receives it.

### Changes to `LogicalContracts.cs`

Add one field to `LogicalOrchestratorResult`:

```csharp
// OutputContextPacket carries the completed run as a context source for the
// next orchestrator call. Pass it as Metadata["context.packet"] on the next
// LogicalOrchestratorRequest to enable context-enriched routing.
// Null when UseContextCarryForward is false or no branches completed.
public LogicalContextPacket? OutputContextPacket { get; init; }
```

### Changes to `AdaptiveLogicalOrchestrator`

In `BuildAggregateResult`, set `OutputContextPacket` to the packet already
built by `BuildContextPacket`:

```
OutputContextPacket = flags.UseContextCarryForward
    ? BuildContextPacket(request.RunId, completedBranches)
    : null
```

This is a one-line addition. The packet was already being built — it is now
surfaced on the result rather than buried in metadata.

Convenience method on the orchestrator:

```
public static LogicalOrchestratorRequest ContinueFrom(
    LogicalOrchestratorResult prior,
    string newUserRequest,
    string newRunId)
{
    var metadata = new Dictionary<string, object>(StringComparer.Ordinal);
    if (prior.OutputContextPacket is not null)
        metadata["context.packet"] = prior.OutputContextPacket;
    return new LogicalOrchestratorRequest(newRunId, prior.ScenarioId,
        newUserRequest, null, metadata);
}
```

Callers can pass `ContinueFrom(priorResult, newPrompt, newRunId)` to chain
sessions without manually handling the context packet.

### Gate B — Phase 4

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New tests:
- OutputContextPacket is populated on a successful run
- OutputContextPacket is null when UseContextCarryForward is false
- ContinueFrom produces a request with the prior context in metadata
- Running ContinueFrom request routes differently than a fresh request
  on the same prompt (context biases the result)

---

## Phase 5 — Full suite verification and commit

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj 2>&1 | tail -5
```

New test count:
- Phase 1 (ContextPacketReader + biased classifier): 6 tests
- Phase 2 (biased router): 5 tests
- Phase 3 (AttributedSegmentRouter + E24): 6 tests
- Phase 4 (OutputContextPacket exposure): 4 tests
- Total new: 21 tests
- New floor: prior floor + 21

Commit message:
```
context-enriched routing and attributed segment routing per session decision 2026-05-06

Context enrichment (no Layer 1 orchestrator — single learning path):
  ContextPacketReader: extracts prior domain, sub-phase, and knowledge edge signals
  DomainClassifierUnit: context-biased confidence (+0.10 boost for domain continuity)
  MultiSubPhaseRouterUnit: context signal as 0.15 weight component in scoring
    prior PASS sub-phases deprioritised, knowledge edges boosted, domain continuity biased
    zero context → weights degrade to prior 0.70/0.30 split (zero regression)
  LogicalOrchestratorResult: OutputContextPacket exposed for caller use
  AdaptiveLogicalOrchestrator: ContinueFrom convenience method for session chaining

Attributed segment routing (E24 rehabilitated for per-domain architecture):
  AttributedSegmentRouter: one SubPhaseRouterSourceDocument per E21 segment
    each document contains only its segment's text — adapter never sees other domains
    per-segment domain classification with context biasing
  PromptRoutingInputCompilerUnit: SINGLE_STEP fast path (no segmentation overhead)
    MULTI_STEP path delegates to AttributedSegmentRouter
```

---

## Execution order and stop conditions

Execute phases in strict order: baseline gate, 1, 2, 3, 4, 5.

Stop immediately if:
- Baseline gate shows a count different from the expected floor
- Any Gate B build step produces errors
- Any phase causes a previously passing test to fail
- A test is modified to suppress a genuine regression

---

## What this plan does NOT change

| Component | Unchanged |
|---|---|
| Domain adapters | One per D-04 domain. No forking. |
| Learning path | Single path. BUG rows train BUG adapter regardless of context. |
| LogicalTransitionOrchestrator | Not built. Not needed. |
| Phase transition topology | Ordering edges remain. No transition type classifier. |
| DpoTrainer.StepVector | Covered in v4 plan. |
| OrchestratorExpander / DomainGraphClosure | Covered in v4 plan. |
| Nine-node flow node wiring | Covered in v4 plan. |

---

## Relationship to v4 plan

This plan is additive. It can execute before or after v4 phases because it touches
different files. The safe order is:

v4 Phase 0 (consolidation) → this plan → v4 Phase 1 onwards

Phase 0 of v4 must run first because it unifies the Domain enum and RouteClassifier.
`ContextPacketReader` and `AttributedSegmentRouter` use the unified `Domain` type.
If this plan runs before v4 Phase 0 completes, it will build against the old
`DomainLabel` enum and require updating when v4 Phase 0 runs.

---

# Plan version: 1.0
# Author: ARCHITECT session 2026-05-06
# Phases: 5
# New tests: 21
# New classes: ContextPacketReader, AttributedSegmentRouter
# New fields: LogicalOrchestratorResult.OutputContextPacket
# New methods: AdaptiveLogicalOrchestrator.ContinueFrom
# Rehabilitates: E24 (attributed routing, per-segment-per-domain)
# Supersedes: Layer 1 transition orchestrator approach (rejected 2026-05-06)
