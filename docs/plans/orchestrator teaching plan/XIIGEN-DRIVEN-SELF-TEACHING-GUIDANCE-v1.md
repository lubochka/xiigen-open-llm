# XIIGen-Driven Orchestrator Self-Teaching Guidance
# Version: 1.0 — 2026-05-05
# Authority: DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v2.md
# Examination method: EXAMINATION-METHOD-ORCHESTRATION-v7.md
# Teaching plan: ORCHESTRATOR-TEACHING-PLAN-v2.md
# Registry: EXTERNAL-SOURCE-REGISTRY-v6.md

---

## What this guidance is

This guidance tells an executor how to take one XIIGen session document —
a real architect or developer conversation with full scope across phases and
sub-phases — and turn it into learning set rows, teach each `ILogicalUnit`
from those rows, and then run the whole `AdaptiveLogicalOrchestrator` as a
live end-to-end verification.

The cycle repeats for each session document. Each repetition adds coverage
across more sub-phases. Over time the machine learns to orchestrate the full
development lifecycle from real XIIGen history.

**One cycle produces:**
- Learning set rows for each layer the document covers
- One DPO step per row per unit
- One verified live orchestration run per unit per cycle
- One ledger record per taught row with full resource measurements

---

## Why XIIGen documents first

XIIGen session documents are the highest-quality first-party source. They contain:

- Real developer prompts exactly as a human typed them — correct A-side language
  for the routing compiler and transition orchestrator
- Real architect corrections — the clearest signal of what the wrong B looks like
- Real multi-phase scope — a single session often spans architecture, security,
  implementation, and testing decisions in the correct dependency order
- Real conflict detection — sessions where Luba corrected a wrong decision are
  the best examples of what the orchestrator must detect and surface

No external source provides all four of these simultaneously. XIIGen documents
are processed first and exhausted before any external source is used.

---

## What a XIIGen session document contains

Each session document holds one complete development work session. It contains:

```text
one or more developer prompts (the real A-side language)
one or more architect responses covering multiple sub-phases
correction events where a wrong answer was rejected
decisions that must be carried forward to later phases
dependency statements between sub-phases
arbiter events where a result was questioned
```

The document maps to the orchestrator pipeline as follows:

```text
developer prompt         → Layer 0-A (compiler pair A-side)
                         → Layer 1 (transition orchestrator A-side)

architect response       → Layer 0-B (router pair B-side: which sub-phases)
                         → Layer 2 Node 01 B-side (domain packets)
                         → Layer 2 Node 03 B-side (work branch plan with dependencies)

correction event         → Layer 3 (correction learner pair)
                         → Corruption case for held-out set

dependency statement     → Layer 2 Node 03 branch depends_on field
                         → Topology transition registration if not yet present

arbiter event            → Layer 2 Node 07 B-side (arbiter selection)
                         → Layer 2 Node 08 B-side (pass/concern/reprocess verdict)

decision carry-forward   → Layer 0-B parallel group and ordering
                         → context.packet content for the next phase's branch
```

---

## Cycle structure — one session document, one full cycle

Each cycle has five stages. Run them in order. Do not skip a stage.

```text
Stage 1 — Read and map the session document
Stage 2 — Register any new topology entries
Stage 3 — Produce learning set rows
Stage 4 — Teach each ILogicalUnit one row at a time
Stage 5 — Run the full orchestration verification
```

---

## Stage 1 — Read and map the session document

**What to do:**

Read the entire session document before doing anything else. While reading,
build a session map with these four sections:

```text
PROMPTS:
  For each distinct developer prompt found in the document:
    - prompt_text: the exact words the developer typed
    - sub_phases_mentioned: which sub-phases the response covered
    - transition_type: feature_addition / repair / architecture / testing / review
    - correction: yes/no — was the first response corrected?

DEPENDENCIES:
  For each pair of sub-phases where one must precede the other:
    - from_sub_phase: the prerequisite
    - to_sub_phase: the dependent
    - reason: why (e.g. "security model must be defined before API contract")

DECISIONS:
  For each decision that must carry forward to later phases:
    - decision_text: the locked decision in plain language
    - source_sub_phase: where it was decided
    - target_sub_phases: which later sub-phases must respect it

ARBITERS:
  For each case where a result was questioned or corrected:
    - what_was_wrong: plain language description of the incorrect answer
    - what_is_correct: the accepted answer
    - sub_phase: which sub-phase the arbiter applies to
```

**Gate before Stage 2:** the session map must have at least one entry in
PROMPTS. A document with no recoverable developer prompts cannot produce
valid A-side rows and must be skipped.

---

## Stage 2 — Register any new topology entries

**What to do:**

For each sub-phase in the session map that is not yet in the topology
registry, add it using `AddOrUpdateSubPhase`. For each dependency in the
session map that is not yet a `LogicalTransition` entry, add it using
`AddOrUpdateTransition`.

Use the seeder as the reference pattern:

```csharp
// Add the sub-phase if it is not yet registered.
var result = topologyRegistry.ResolveSubPhase(subPhaseId);
if (!result.IsSuccess)
{
    var registration = topologyRegistry.AddOrUpdateSubPhase(new LogicalSubPhase(
        subPhaseId,
        phaseId,
        name,
        purpose,          // use vocabulary from the session document
        [domainLabel],
        [predicateIds],   // from the nine-predicate compiler lexicon
        []));
    if (!registration.IsSuccess)
        return fail(registration.ErrorMessage);
}

// Add the transition if it is not yet registered.
var transResult = topologyRegistry.AddOrUpdateTransition(new LogicalTransition(
    $"transition-{fromSubPhaseId}-to-{toSubPhaseId}",
    "sub-phase",
    fromSubPhaseId,
    "sub-phase",
    toSubPhaseId,
    reason,
    [],
    [],
    []));
```

`DevelopmentLifecycleTopologySeeder` already registers all 31 standard
sub-phases and 36 standard transitions. Only add entries that are not already
there. Never duplicate an existing entry — `AddOrUpdateSubPhase` is idempotent
but adding a transition that already exists will create a duplicate edge in
`OrderByTransitions`.

**Gate before Stage 3:** run the full test suite. All must pass.

```bash
dotnet test Tests/LlmFromScratch.Tests.csproj --no-build 2>&1 | tail -3
# Expected: >= 1205 passed, 0 failed
```

---

## Stage 3 — Produce learning set rows

**What to do:**

For each prompt in the session map, produce the rows specified by
`EXAMINATION-METHOD-ORCHESTRATION-v7.md`. Follow the layer ordering:
Layer 0-A first, then Layer 0-B, then Layer 1, then Layer 2 Nodes 01-09,
then Layer 3. Never write a downstream row before the upstream row it
depends on exists and is accepted.

**Layer 0-A — Compiler pair (one per prompt):**

A is the `LogicalOrchestratorRequest` shape. The `user_request` field is the
exact developer prompt text from the session document. Do not paraphrase.

B is the `SubPhaseRouterSourceDocument`. The `CompiledPredicateIds` array
contains the predicate IDs from the compiler's nine-predicate lexicon that
match the prompt text. Compute this by applying the lexicon manually:

```text
For each predicate (SECURITY, AUTHORIZATION, API_CONTRACT, ARCHITECTURE,
IMPLEMENT, TEST, QA, UI, DOCUMENT), check whether any of its trigger terms
appear in the prompt text (case-insensitive). If yes, include the predicate ID.
```

If no predicates match, the `CompiledPredicateIds` is empty, `Confidence` is
0.70, and `SourceStatus` is `"knowledge_edge: no high-signal development predicate found"`.

**Layer 0-B — Router pair (one per prompt, A from Layer 0-A B-side):**

B lists one `SubPhaseRoutingDecision` per sub-phase the architect response
covered. Fill in `PredictedSubPhaseId`, `PredictedPhaseId`, `Confidence`
(estimate from how central the sub-phase is in the response — primary topic
0.85-0.95, secondary 0.55-0.75), `EvidencePredicates` from the matched
predicates, and `KnowledgeEdge: false` for covered sub-phases.

For any sub-phase mentioned in the session that has no registered flow
(all except 3.4 in the current state), the orchestrator will produce a
knowledge-edge branch result automatically. You do not need to write a router
entry for unregistered sub-phases — but you do need a Layer 0-C pair showing
the expected knowledge-edge branch output.

**Layer 1 — Transition orchestrator pair (one per prompt):**

The A-side is the same developer prompt. The B-side is the selected transition
type and the rejected alternatives. Use the `transition_type` from the session
map. The rejected alternatives must each have a `rejection_reason` that matches
why the session document did not take that path.

**Layer 2 Node 01 — Domain splitter pair (one per transition):**

A is the selected transition B-side from Layer 1. B is the domain packet array.
One packet per sub-phase the architect response covered. Fill in all required
fields: `packet_id`, `domain_label`, `phase`, `sub_phase`, `task_type`,
`prompt_fragment` (the fragment of the prompt that triggered this territory),
`coverage_status` (`covered` / `concern` / `boundary`), `reason`, `handoff_marker`,
`source_evidence`.

**Layer 2 Node 03 — Scheme planner pair (one per transition):**

A is the complete set of translated domain statements from Node 02. B is the
ordered work branch array. Use the DEPENDENCIES section of the session map to
fill in `depends_on` correctly. Branches whose sub-phases were in a
`depends_on` relationship must have the correct ordering.

Use the DECISIONS section to fill in `boundary_or_knowledge_edge_status` for
any branch that references a decision that must be preserved as context.

**Layer 2 Nodes 07-08 — Arbiter pair (one per arbiter event):**

Use the ARBITERS section. The arbiter selector B-side names the arbiters that
fire for this scenario. The arbiter unit B-side gives the verdict: `concern`
for things that were questioned, `reprocess` for corrections, `pass` for clean
results.

**Layer 3 — Correction learner pair (one per correction event):**

A is the original wrong response as `Rejected` and the corrected response as
`Chosen`. B is the training result with `LoraAdapterCount > 0` and
`DpoStepCount == 1`. Write this row only after the correction event pair files
exist.

**Row file format:**

Each row is one YAML file in this directory structure:

```text
C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\
  xiigen-sessions\
    {session-date}-{session-title}\
      layer-0-a\  — compiler pairs
      layer-0-b\  — router pairs
      layer-0-c\  — knowledge-edge pairs
      layer-1\    — transition orchestrator pairs
      layer-2\    — node 01 through node 09 pairs
      layer-3\    — correction learner pairs
      held-out\   — rephrasing and corruption pairs
      manifest.yaml
```

**Required coverage labels** on every row (from examination method v7):

```yaml
source_id: "xiigen-sessions-{date}"
source_title: "{session document filename}"
source_license_state: "first_party"
contract_signature: "{exact ILogicalUnit<TInput, TOutput> or orchestrator signature}"
transition_type: "{feature_addition | repair | ...}"
phase: "{phase id}"
sub_phase: "{sub-phase id}"
task_type: "{task description}"
node_role: "{domain-splitter | scheme-planner | ...}"
coverage_status: "covered | concern | boundary | knowledge_edge"
pair_kind: "{adaptive_routing_compiler | flow_node | teaching_unit | ...}"
```

**Held-out rows (required per batch):**

For every prompt, produce at least one rephrase pair where the A-side uses
different vocabulary but the B-side is identical. Rephrase the developer
prompt, not the architect response.

For every batch, produce at least one corruption case where the B-side is
deliberately wrong in a specific way: wrong sub-phase assignment, wrong
predicate set, or wrong dependency order. Label it `pair_kind: corruption_case`.

---

## Stage 4 — Teach each ILogicalUnit one row at a time

Follow `ORCHESTRATOR-TEACHING-PLAN-v2.md` exactly. The sub-cycle runs once
per row per unit. The teaching order within each cycle is:

```text
1. PromptRoutingInputCompilerUnit  — Layer 0-A rows
2. MultiSubPhaseRouterUnit         — Layer 0-B rows (A from compiler B-side)
3. DomainSplitterUnit              — Layer 2 Node 01 rows
4. SchemePlannerUnit               — Layer 2 Node 03 rows
5. ArbiterUnit                     — Layer 2 Node 08 rows
6. LogicalDpoTrainingUnit          — Layer 3 correction rows (run last)
```

For each unit, for each row:

**Step T1 — Baseline snapshot.** Run the verification prompt through the
current orchestrator. Record `baseline_verdict` and `baseline_confidence`.

**Step T2 — Build the DPO preference batch.** The pair's A-side becomes
`Prompt`. The pair's B-side becomes `Chosen`. Produce `Rejected` by taking
the nearest wrong example from the held-out corruption cases or by using the
deterministic unit's output on the same prompt when that output differs from B.

**Step T3 — Apply LoRA and train one DPO step.** Use `Alpha = 2f, Rank = 4`
(Scaling = 0.5). Measure all resource fields as specified in the teaching plan.

**Step T4 — Register supersession sentinel.** Before registering the new
trained unit, register a `Candidate`-state sentinel for the same role to
prevent the prior probationary version from remaining active:

```csharp
unitRegistry.Register<TInput, TOutput>(
    $"unit-{role}-sentinel-row-{rowIndex:D4}",
    role, subPhaseId, $"1.0.row{rowIndex}.sentinel",
    LogicalUnitTrustState.Candidate, false, deterministicFallback);
```

**Step T5 — Wrap the verification run with tracing.** Register a
`TracingFlowDecorator`-wrapped version of the flow for the verification run.
Collect `StationTrace` objects.

**Step T6 — Run verification.** Run the adaptive orchestrator with
`UseLearnedUnitWhenAvailable: true` and the same verification prompt used
in T1.

**Step T7 — Decide and write the ledger record.** Apply the decision rules
from the teaching plan. Write one `TeachingRowRecord` to the session ledger
JSONL file before moving to the next row.

**Stopping rules (from teaching plan v2):**

- Three consecutive `rejected` decisions for the same unit: stop that unit's
  teaching for this batch. The pair files have a systematic problem. Review
  the session map before the next cycle.
- `LoraAdapterBytes` exceeds 2MB: reduce `Rank` to 2 and restart from the
  last promoted row.

---

## Stage 5 — Run the full orchestration verification

After all rows for this session document have been taught, run one end-to-end
verification using the original developer prompt from the session document.

```csharp
var result = await assistant.RunAsync(
    userRequest: originalPromptFromSessionDocument,
    scenarioId: null,       // adaptive routing
    flags: new LogicalExecutionFlags
    {
        UseLearnedUnitWhenAvailable = true,
        UseContextCarryForward      = true,
        UseKnowledgeEdgeDetection   = true
    });
```

**What to check:**

```text
1. Routing correctness: result.Value.BranchResults contains one branch per
   sub-phase that appeared in the session map PROMPTS section.

2. Knowledge-edge honesty: every sub-phase that has no registered flow appears
   as a branch with OverallVerdict == "KNOWLEDGE_EDGE" and the six required
   metrics.

3. Context carry-forward: branches that depend on earlier branches (from the
   session map DEPENDENCIES section) have context.packet in their metadata
   containing the upstream branch results.

4. Arbiter correctness: if the session map ARBITERS section has entries, the
   corresponding branch verdicts must not be "PASS" — they must be "CONCERN"
   or "REPROCESS" for the same scenarios.

5. Confidence direction: for branches corresponding to taught rows, the
   confidence value in the branch result must be >= the baseline_confidence
   recorded in Step T1.
```

**Write the cycle summary** to the session ledger file before starting the
next session document:

```json
{
  "cycle_id": "cycle-{session-date}-{session-title}",
  "session_document": "{filename}",
  "rows_produced": {count},
  "rows_taught": {count},
  "rows_promoted": {count},
  "rows_rejected": {count},
  "units_taught": ["{unit roles}"],
  "sub_phases_covered": ["{sub-phase ids}"],
  "verification_result": {
    "overall_verdict": "{PASS | PASS_WITH_GAPS | ...}",
    "branch_count": {count},
    "knowledge_edge_count": {count},
    "context_carry_forward_present": true,
    "confidence_improved": true
  }
}
```

---

## Session document selection order

Process session documents in this order. The order matters because later
documents depend on concepts established by earlier ones.

**Priority 1 — Multi-phase architecture and security sessions.**
These contain the richest dependency graphs and the most important carry-forward
decisions. They produce rows for sub-phases 3.1, 3.3, 3.4, 4.1, and 5.2 —
the sub-phases required for Orchestration Scenario O1 (multi-tenant JWT auth).
Look for sessions with titles containing: architecture, security, design,
tenant, authentication, API, integration.

**Priority 2 — Plan and repair sessions.**
These produce rows for the transition orchestrator's `feature_addition` and
`repair` transition types, and for the scheme planner's dependency ordering.
Look for sessions containing explicit "fix", "incorrect", or "wrong" correction
events.

**Priority 3 — Testing and QA sessions.**
These produce rows for sub-phases 5.1, 5.2, 5.3, 6.1, 6.2, 6.3. They are
processed third because good testing pairs depend on having correct design and
implementation pairs to reference.

**Priority 4 — Review and release sessions.**
These produce arbiter rows and formatter rows. Processed last because they
summarize work that earlier sessions established.

---

## Acceptance gate for each cycle

A cycle is accepted and the next session document can begin when all of the
following are true:

```text
□ Stage 1 session map written and present in the batch folder
□ Stage 2 topology registrations applied; test suite still green
□ Stage 3 row files present with all required coverage labels
□ Stage 3 manifest.yaml present in the batch folder
□ Stage 3 held-out rows present (at least one rephrase per prompt)
□ Stage 3 corruption rows present (at least one per batch)
□ Stage 4 teaching ledger JSONL has one entry per taught row
□ Stage 4 no consecutive rejected decisions > 3 for any unit
□ Stage 4 LoraAdapterBytes <= 2MB for each LoRA application
□ Stage 5 cycle summary JSON written
□ Stage 5 verification overall verdict is not REJECT
□ Stage 5 branch count >= sub-phase count in session map
```

If any gate item fails, do not start the next cycle. Fix the failing item
in the current cycle first.

---

## What the machine learns from each cycle

After one cycle on one XIIGen session document, the orchestrator has been
taught on real evidence. What changes:

The **compiler** has seen real XIIGen developer vocabulary. Its predicate
extraction for similar prompts will produce more accurate `CompiledPredicateIds`.

The **router** has seen real multi-phase session scope. It will produce routing
decisions that match the sub-phases a XIIGen session actually touches, not just
the highest-confidence single sub-phase.

The **domain splitter** has seen real transition-to-packet mappings. It will
produce domain packets grounded in real architectural decisions rather than
generic templates.

The **scheme planner** has seen real dependency ordering from actual design
sessions. It will produce work branch plans where dependencies match the order
real architects establish.

The **arbiters** have seen real correction events. They will flag the same
kinds of errors that real architect sessions corrected.

The **knowledge-edge detection** becomes calibrated: after several cycles,
the orchestrator knows which sub-phases commonly appear together in XIIGen
sessions and will expect to route to multiple sub-phases simultaneously for
the same class of prompts.

---

## File locations

Session documents to process:
`C:\Users\lubao\Documents\xiigen\`

Learning set output:
`C:\Users\lubao\Documents\xiigen\llm mvp\external-sources-training-sets\xiigen-sessions\`

Teaching ledger:
`C:\Users\lubao\Documents\xiigen\llm mvp\teaching-ledger\xiigen-sessions\`

Topology registration file (update `CreateDefault` after each cycle):
`C:\Projects\llm_mvp_core\15.OrchestratorModel\Logical\LogicalOrchestratorAssistant.cs`

---

## Rules for this guidance

Read the entire session document before producing any row. One fact from the
last paragraph of the document must be recorded in the session map as proof
of reading.

Never write a downstream row before the upstream row it depends on is accepted.
Node 02 A-side comes from Node 01 B-side. Node 03 A-side is the set of Node 02
B-sides. Node 04 A-side comes from Node 03 B-side. This chain is inviolable.

Never use session document language as a prompt A-side. Session documents are
written in architect language. The A-side must be developer language — what a
developer would type, not what an architect would write in a design review.

Never produce a B-side that describes the output. B must be the actual runtime
object, written in full, as it would appear in a `LogicalUnitResult.Output` field.

Test floor: 1205. The test suite must be green after every Stage 2 topology
change. Never commit a topology change that breaks a test.

---

# Last updated: 2026-05-05
