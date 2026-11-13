# XIIGen Document-Driven DPO Teaching Plan
# Version: 1.0 — 2026-05-08
# Authority: ORCHESTRATOR-TEACHING-PLAN-v2.md, EXAMINATION-METHOD-ORCHESTRATION-v8.md
# Experiment basis: Calibration run 2026-05-06 — PLAN mean acc=0.507, ARCH=0 batches

---

## What this plan is

This plan teaches the adaptive orchestrator using XIIGen session history as the
primary data source, with an explicit focus on improving DPO pair quality for
ARCH and PLAN domains. It is the internal-first counterpart to the external source
registry: external sources provide authority evidence for B sides and for rejected
content; XIIGen documents provide the only authentic A-side language.

The calibration run showed that batch 16 (acc=0.618) and batch 17 (acc=0.771)
were the two strongest results. Both came from XIIGen sessions with named phases,
named components, and explicit dependency language. Batch 30 (acc=0.306) came
from sessions with meta-commentary and no technical specificity. The teaching
plan encodes this finding: every pair written from a XIIGen document must contain
a technically specific prompt with named artifacts and dependency language.

One difference from the orchestrator teaching plan v2: this plan does not teach
one row at a time through live LoRA steps. It produces DPO pair files that feed
the calibration runner's existing `DefaultLogicalCapabilityTrainer`. The pair
structure matches the `LogicalDpoPreferenceExample` contract — prompt, chosen,
rejected — and the rejected follows the Type A / B / C rules from the examination
method.

---

## Hard gates before any pair is written

| Gate | Rule | Source |
|---|---|---|
| Rejected template forbidden | "Do not generate a capability for sub-phase X" is not an acceptable rejected | Calibration run diagnosis |
| Rejected must have annotation | Every rejected ends with [WRONG: ...] naming the specific confusion | DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md |
| One prompt per pair per batch | Same prompt cannot be chosen in one pair and rejected in another in the same batch | Calibration run batch 9 failure analysis |
| ARCH first | Sub-phases 3.1, 3.2, 3.5, 6.1 had zero training batches. These are processed before PLAN | Run analysis |
| Compiled text in training | TrainingText must use compiled form (predicate IDs + normalized text), not raw prompt | PLAN-COMPILED-PROMPT-IN-TRAINING-v1.md |
| Type B required for multi-task prompts | Any XIIGen prompt covering two or more sub-phases gets a Type B rejected (wrong dependency order) | Examination method v8 |

---

## Architecture: 6-step pair production cycle per session document

```
1. Session scan         → identify all prompts, corrections, dependency statements
2. Prompt classification → tag each prompt with intent verb → domain → sub-phase
3. Compiler simulation  → derive CompiledPredicateIds for each prompt
4. Pair construction    → chosen = correct sub-phase response
                          rejected = Type A / B / C based on prompt and sub-phase
5. Batch assembly       → group into batches respecting composition minimums
6. Measurement gate     → verify batch reward margin before committing to ledger
```

---

## Phase 1 — Session document scan and classification

### What this phase produces

For each XIIGen session document processed:

```
session_id              — derived from filename
prompt_count            — number of developer prompts extracted
correction_count        — number of Luba corrections (wrong answer + right answer)
sub_phase_coverage      — which sub-phases appear in the session
dependency_statements   — explicit before/after, depends-on statements
multi_phase_prompts     — prompts that span two or more sub-phases
```

### Classification rules

Tag each prompt with an intent verb category before routing it to a sub-phase:

```
"explain / how does / what is / why does"    → CODE_Q
"X throws / X fails / bug in / error in"     → BUG  
"implement / add / build / create"           → PLAN or IMPLEMENT
"design / define boundaries / architect"     → ARCH (Phase 3 sub-phases)
"review / audit / verify compliance"         → QA (Phase 6 sub-phases)
"plan the / sequence / order of"             → PLAN (Node 03 SchemePlannerUnit)
```

A prompt that contains both "design" and "implement" in separate sentences goes
to ARCH for the design sentence and PLAN for the implementation sentence. These
become two pairs, not one.

### Quality signal from correction events

Every Luba correction in a session document is a pre-made Type A or Type C
rejected example. The pattern is:

```
Claude's wrong answer  → rejected (it was corrected because it was wrong)
Luba's correction      → chosen (it is the ground truth)
The original prompt    → A side
```

These correction-derived pairs are the highest-quality pairs in the entire
dataset because the wrong answer was genuinely produced and genuinely rejected
by a human expert.

### Gate

Each session document produces a session scan record before pair construction begins.
The scan record states how many prompts, corrections, multi-phase prompts, and
dependency statements were found. A document that produces zero of all four is
not processed further.

---

## Phase 2 — Compiler simulation

Before writing any pair, derive the `CompiledPredicateIds` for each prompt using
the nine-predicate lexicon. This simulates what `PromptRoutingInputCompilerUnit`
would produce at runtime.

```
SECURITY      — jwt, authentication, auth, oauth, oidc, token, session, asvs, mfa, rbac
AUTHORIZATION — authorization, permission, rbac, scope, tenant, ownership
API_CONTRACT  — api, endpoint, contract, schema, request, response, openapi
ARCHITECTURE  — architecture, component, topology, boundary, dependency, service
IMPLEMENT     — "implement the", connector, provider, "integration layer", "code change"
TEST          — test, testing, "integration test", "unit test", coverage, fixture
QA            — qa, quality, release, acceptance, verification
UI            — ui, ux, screen, layout, design, interaction, accessibility
DOCUMENT      — document, documentation, spec, specification, requirements
```

A prompt with zero matched predicates is still used — it produces a
knowledge-edge pair (correct result = KNOWLEDGE_EDGE, rejected = a fabricated
PASS result that suppresses the knowledge edge).

The compiled predicate set is recorded as `CompiledPredicateIds` in the pair file
and used in the `TrainingText` field alongside the normalized prompt text.

### TrainingText construction

```
TrainingText = "semantic_prompt: {compiledText} branch_result: {branchResult}
                sub_phase: {subPhaseId} {subPhaseName} {subPhasePurpose} {predicateIds}
                semantic_compressed: {compiledText} teaching_vector_60: {vector}"
```

The `compiledText` is the normalized prompt text with predicate tokens appended —
not the raw prompt. This is the gate from PLAN-COMPILED-PROMPT-IN-TRAINING-v1.md:
training uses the compiled form, never the raw form.

---

## Phase 3 — Pair construction by domain

### ARCH pairs (primary target — zero batches in calibration run)

Process sub-phases 3.1, 3.2, 3.5, 6.1 first.

**Chosen content:** The architect response from the session document that addresses
component boundaries, entity shape, observability strategy, or acceptance criteria.
This is the B side — what the system should produce.

**Rejected selection:** Use the ARCH confusion table from the examination method.

| Chosen sub-phase | Rejected sub-phase | Pattern |
|---|---|---|
| 3.1 Architecture and Component Topology | 4.1 Backend Services | Writes class body instead of boundary rule |
| 3.1 Architecture and Component Topology | 4.4 Refactoring | Moves code instead of deciding ownership |
| 3.2 Data Model and Persistence Design | 4.1 Backend Services | Writes repository instead of entity shape |
| 3.2 Data Model and Persistence Design | 4.5 Integration | Writes adapter instead of storage boundary |
| 3.5 Observability and Operational Design | 4.1 Backend Services | Wires logger instead of defining strategy |
| 3.5 Observability and Operational Design | 7.3 Monitoring and Feedback Loop | Configures dashboard before signals are designed |
| 6.1 Manual QA and Acceptance Review | 5.1 Unit Test Design | Automates instead of defining human acceptance |
| 6.1 Manual QA and Acceptance Review | 5.3 End-to-End Scenario Testing | Automates user journey instead of human checklist |

**Annotation template:**

```
[WRONG: "{chosen sub-phase}" owns {decision type}. {Rejected sub-phase} addresses
{wrong action}. {Decision type} must exist before {wrong action} can be taken.]
```

Example:

```
[WRONG: "define boundaries" asks who owns what and where dependencies flow,
not what the class body contains. Writing the class body is implementation.
Implementation decisions follow boundary decisions, they do not replace them.]
```

### PLAN pairs (secondary target — 4 of 6 batches with negative margin)

Process Node 03 SchemePlannerUnit pairs — dependency ordering is the primary signal.

**Chosen content:** The dependency-ordered plan from the session document.
Branches with explicit `depends_on` relationships, named artifacts, and specific
sub-phase assignments.

**Rejected selection (Type B — required for all multi-task PLAN prompts):**
Use the dependency ordering table from the examination method.

| Correct sequence | Natural wrong order for rejected |
|---|---|
| 3.1 Architecture → 4.1 Backend Services | 4.1 first, 3.1 as documentation |
| 3.2 Data Model → 3.3 API Contract | 3.3 first, model derived from contract |
| 3.4 Security → 5.2 Integration Tests | 5.2 first with mocks, 3.4 after |
| 3.1 + 3.4 parallel → 4.1 after both | All three in parallel, no dependency |

**What makes a good PLAN prompt (from batch 16/17 analysis):**
The prompt must contain at least one of:
- A named artifact: "Flow 10", "Phase 3", "Factory Interface", component name
- A dependency verb: "before", "after", "once X is complete", "depends on"
- A specific sub-phase reference by name or number

Prompts that contain only abstract planning language ("Option A is clearly right")
must not be used for PLAN training — they produced the worst batches.

**PLAN Type C rejected (domain boundary teaching):**

For prompts where the intent verb is "plan" or "design" but the content is
technical implementation, the Type C rejected is a CODE_Q explanation:

```
Prompt: "how should we structure the authentication flow for multi-tenant SaaS"

CHOSEN — PLAN, branches 3.4 and 3.1:
Branch 1 (3.4, order=1): Define JWT structure with tenant_id claim...
Branch 2 (3.1, order=2): Component topology — AuthGateway → TenantResolver...

REJECTED — CODE_Q:
Multi-tenant authentication can be structured several ways: shared auth server
with tenant claims, separate auth server per tenant, or OIDC federation...
[WRONG: "how should we structure" asks for a plan with ordered decisions.
A conceptual survey of options is CODE_Q. This is PLAN scope.]
```

### BUG pairs (performing adequately — fill only gaps)

Only process BUG pairs where the session document contains a specific error
message or stack trace. Generic complaint prompts ("this is broken") do not
produce useful BUG pairs.

**Type C rejected for BUG:** The most common BUG confusion is between a
security enforcement error (SECURITY predicate, 3.4) and a frontend component
error (UI predicate, 4.2). Use this pattern for any prompt containing
"permissions", "403", "unauthorized", or "CSP":

```
Prompt: "camera is not allowed in this document"

CHOSEN — 3.4 Security and Authorization Model:
Set Content-Security-Policy headers to allow camera access from the required
origin. The violation indicates a missing CSP policy, not a broken component.

REJECTED — 4.2 Frontend and Client Integration:
Add explicit camera permission request in the Angular component...
[WRONG: this is a CSP document policy error, not a JavaScript permission request.
The error fires before any component code runs.]
```

---

## Phase 4 — Batch assembly

Each batch is assembled from the pairs produced in Phase 3. The batch must satisfy
the composition minimums before it is accepted.

### Composition minimums

| Pair type | Minimum fraction | Required when |
|---|---|---|
| Type A (adjacent sub-phase) | 40% | Always |
| Type B (wrong dependency order) | 20% | Any multi-task prompt in the batch |
| Type C (wrong domain) | 30% | At least one of each domain in the batch |
| Stage 9 corruption rows | 1 of each kind | Every batch |

### Stage 9 corruption rows — three required per batch

**wrong-predicate-assignment:** Take a prompt from the session where a secondary
vocabulary word could be confused with the primary predicate. Correct assigns the
right predicate; corrupted assigns the secondary word's predicate only.

```
Example from XIIGen session: "test coverage shows 42% on the authentication module"
Correct:  CompiledPredicateIds: ["TEST", "SECURITY"]
Corrupted: CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
[WRONG: "test coverage" triggers TEST. "authentication module" is the measured
object. TEST predicate takes priority when the primary verb is coverage measurement.]
```

**ambiguous-predicate-case:** Take a prompt where two predicates support two
different sub-phases. Correct routes both; corrupted routes only one.

```
Example from XIIGen session: "add authorization to the API endpoint"
Correct:  Routes to 3.4 (SECURITY) AND 3.3 (API_CONTRACT)
Corrupted: Routes only to 3.4
[WRONG: API_CONTRACT from "API endpoint" is present. Both sub-phases must route.]
```

**knowledge-edge-suppressed:** Take any prompt from the session that routes to
an unregistered sub-phase. Correct returns KNOWLEDGE_EDGE with all six metrics;
corrupted returns PASS.

```
Example: prompt routes to 5.2 Integration Tests — no flow registered
Correct:  KNOWLEDGE_EDGE with knowledge_edge=True, missing_sub_phase=5.2, etc.
Corrupted: PASS result without knowledge-edge markers
[WRONG: PASS fabricates a result the system cannot produce for this sub-phase.]
```

### One prompt per pair per batch rule

Before finalising the batch: scan all pairs and verify that no prompt string
appears as the A side of more than one pair in the batch. If duplicates exist,
remove all but the pair with the strongest chosen/rejected distinction.

---

## Phase 5 — Pre-commit measurement

Before a batch is committed to the training ledger, run a verification pass
using the current model state. Record these fields for every batch:

```
batch_id
domain
row_count
branch_count
distinct_sub_phases
chosen_reward_mean      — mean ChosenReward across DPO steps in this batch
rejected_reward_mean    — mean RejectedReward across DPO steps
reward_margin_mean      — mean (ChosenReward - RejectedReward)
                          MUST be >= 0 for the batch to be accepted
accuracy                — DPO accuracy for this batch
loss                    — DPO loss (should be moving below 0.693)
verdict                 — ACCEPT_BATCH or REJECT_BATCH
```

A batch is rejected when `reward_margin_mean < 0`. This means the model assigns
higher probability to rejected than to chosen — the pair structure is wrong.
Before resubmitting, inspect which pairs had the largest negative margin and
check whether:

1. The chosen and rejected texts are too similar in token space
2. The [WRONG: ...] annotation is missing or generic
3. The rejected is using the forbidden negation template

### Rejection diagnosis table

| Symptom | Likely cause | Fix |
|---|---|---|
| All pairs in batch have negative margin | Rejected template is negation ("Do not...") | Rewrite all rejected as Type A, B, or C |
| Specific sub-phase pairs have negative margin | Chosen and rejected are the same sub-phase template | Check that rejected addresses a different sub-phase |
| Type B pairs have negative margin | Dependency order in rejected is actually correct | Verify the dependency table and swap |
| Corruption rows have positive margin but other pairs do not | Real pairs are weaker than corruption pairs | The real pairs need stronger chosen/rejected distinction |

---

## Phase 6 — Session ledger

Every accepted batch is recorded in the session ledger before the next session
document is processed.

Ledger file location:

```
C:\Users\lubao\Documents\xiigen\llm mvp\teaching-ledger\
  dpo-internal-{date}.jsonl
```

One line per batch:

```json
{
  "batch_id": "xiigen-dpo-001-arch-3.1",
  "source_document": "last product session.txt",
  "session_excerpt_lines": "402-451",
  "domain": "ARCH",
  "sub_phases": ["3.1"],
  "row_count": 8,
  "pair_types": {"A": 4, "B": 2, "C": 2},
  "stage9_corruption": {"wrong_predicate": 1, "ambiguous_predicate": 1, "knowledge_edge_suppressed": 1},
  "reward_margin_mean": 0.0014,
  "accuracy": 0.581,
  "verdict": "ACCEPT_BATCH",
  "compiler_used": true,
  "compiled_prompt_in_training": true
}
```

`compiled_prompt_in_training: true` is a required field. A batch where `TrainingText`
used the raw prompt instead of the compiled form must be rejected regardless of
accuracy.

---

## Processing order across session documents

### Priority 1 — ARCH sub-phases (zero training signal)

Process all XIIGen session documents that contain prompts matching the ARCHITECTURE
predicate or the intent verbs "design", "define boundaries", "architect".

Target sub-phases: 3.1, 3.2, 3.5, 6.1.

Minimum pairs before moving to Priority 2: 20 ARCH pairs across at least 3 of
the 4 ARCH sub-phases.

### Priority 2 — PLAN multi-task prompts (positive margin in only 2 of 6 batches)

Process all XIIGen session documents containing prompts with two or more sub-phase
signals. These are the source of Type B (wrong dependency order) pairs.

Focus on sessions where Luba explicitly stated an ordering constraint: "first X
then Y", "after X is done", "X must be in place before Y".

Minimum pairs before moving to Priority 3: 15 Type B PLAN pairs with verified
positive reward margin.

### Priority 3 — Correction events (highest-quality rejected material)

Process all correction events from all session documents regardless of domain.
Every correction is a pre-made pair:

```
Prompt before correction  → A side
Claude's wrong answer     → rejected (with [WRONG: ...] annotation added)
Luba's correction         → chosen
```

These require no invented rejected content — the wrong answer already exists.
They also require no annotation invention — the correction itself is the annotation.

### Priority 4 — Fill remaining sub-phases

After ARCH, PLAN multi-task, and corrections are processed, fill remaining
sub-phases (5.1, 5.2, 5.3, 6.3) using session documents where those predicates
appear.

---

## What this plan does NOT do

**Does not teach live LoRA.** Live LoRA teaching (one-row-at-a-time with
TeachingRowRecord) is covered by ORCHESTRATOR-TEACHING-PLAN-v2.md. This plan
produces DPO pair files for the batch calibration runner.

**Does not invent prompts.** Every A side comes from a real XIIGen session
document. Invented prompts are not permitted.

**Does not process session documents without technical content.** A session
document that contains only meta-commentary, status updates, or abstract
planning language with no named artifacts is skipped.

**Does not skip the [WRONG: ...] annotation.** Every rejected response requires
an annotation. A pair file submitted without annotations is rejected at the
batch gate.

---

## Pre-condition check

Before processing any session document:

```
PC-1: PLAN-COMPILED-PROMPT-IN-TRAINING-v1.md implemented
      grep -n "BuildTrainingTextCore\|compiledText\|CompiledText" \
        21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs
      Expected: compiledText used as promptSide when not null

PC-2: Forbidden rejected template not present in any existing pair file
      grep -r "Do not generate a capability" \
        external-sources-training-sets/
      Expected: 0 matches

PC-3: Session ledger directory exists
      ls "C:\Users\lubao\Documents\xiigen\llm mvp\teaching-ledger\"
      Expected: directory exists (create if not)

PC-4: DPO-PAIR-AUTHORING-GUIDANCE-ARCH-PLAN-v1.md accessible
      Expected: document available as reference during pair construction
```

---

## Test count summary

This plan produces pair files, not unit tests. The verification gate is:

| Metric | Minimum acceptable | Target |
|---|---|---|
| Batches with positive reward margin | 60% | 80% |
| ARCH sub-phases with at least one batch | 3 of 4 | 4 of 4 |
| PLAN batches with positive margin | 4 of 6 | 5 of 6 |
| Correction-derived pairs | 10 | 25 |
| Stage 9 corruption rows per batch | 1 of each kind | 1 of each kind |
| Batches with compiled_prompt_in_training=true | 100% | 100% |

---

## Commit sequence

| Phase | Ledger entry prefix |
|---|---|
| Phase 1: session scan | scan-{session-id} |
| Phase 3 ARCH: first 20 pairs | arch-{sub-phase}-{batch-n} |
| Phase 3 PLAN: first 15 Type B pairs | plan-typeb-{batch-n} |
| Phase 3 corrections: all | correction-{session-id}-{n} |
| Phase 4: assembled batches | batch-{domain}-{date}-{n} |

---

## Last updated: 2026-05-08
