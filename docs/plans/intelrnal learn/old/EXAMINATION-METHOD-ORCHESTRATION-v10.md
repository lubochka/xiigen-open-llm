# Document Examination to Training Sets — Orchestration Edition
# Version: 10.0 — 2026-05-08
# Supersedes: EXAMINATION-METHOD-ORCHESTRATION-v9.md

---

## What changed from v9

**Source families section added.** v9 described how to produce pairs but not
which source documents to use and what rules govern each family. v10 adds a
complete source families registry covering 13 document families identified in
the XIIGen corpus. Each family states: what DPO pairs it produces, which pair
type (A/B/C), typical overlap ratio, what is excluded, and failure modes specific
to that family.

**Four new source types formalised:** migration plans (U03), portability protocols
(U07), agent tooling instructions (U11), and skill library documents (U05). Each
produces a distinct class of pairs not covered by any existing source type.

**Two source families added to excluded list:** source archives and snapshots
(U10, ZIP files — must be unpacked and reclassified), and raw merged text dumps
(U13 — must be segmented before any pair extraction).

**Three source families clarified as A-side-only:** design conversion documents
(U08), plugin/extension docs (U04), and capability matrix RAG indexes (U12 partial).
These provide Type 1 developer prompts but must not be used as B-side authority
for XIIGen-specific decisions.

**Pair kind values extended.** `migration_plan`, `portability_invariant`,
`tooling_contract`, `skill_boundary`, `research_fact` added to the allowed list.

**Failure modes EM-18 through EM-22 added** covering the new families.

**Acceptance checklist extended** with source-family-specific checks.

---

## What changed from v8

**DPO accuracy diagnosis added from calibration run data.** Run 2026-05-06
produced 40 batches. 29 of 40 had negative reward margin — the model assigned
higher probability to rejected than to chosen. This section explains exactly
why that happens and how each pair type prevents it.

**Reward margin mechanics explained.** The relationship between pair structure
and reward margin is now documented with the actual numbers from the run. Every
author of pair files needs to understand this to produce pairs that will move
the margin positive.

**Prompt quality rules for PLAN added from batch analysis.** Batches 16 and 17
(acc=0.618, 0.771) had specific prompt characteristics that the worst batches
lacked. These characteristics are now explicit rules, not observations.

**TrainingText compiled-prompt requirement added.** Pairs where TrainingText
uses the raw prompt instead of the compiled form are rejected at the measurement
gate. This is now a hard rule in the acceptance checklist.

**Correction events from XIIGen history added as a pair source.** Every Luba
correction in a session document is a pre-made chosen/rejected pair. Extraction
rules are now part of this document.

---

## Canonical Pattern Dependency

This document uses `DYNAMIC-FLOW-CANONICAL-DESIGN-PATTERN-v2.md` as the authority
for `ILogicalUnit`, `ILogicalFlow`, `ILogicalOrchestrator`, recursive composition,
and array output.

---

## Full Ordered Path

```
human prompt
-> PromptRoutingInputCompilerUnit     [Layer 0-A]
-> MultiSubPhaseRouterUnit            [Layer 0-B]
-> knowledge-edge detection           [Layer 0-C]
-> flow factory selection             [Layer 0-D]
-> LogicalTransitionOrchestrator      [Layer 1]
-> selected A/B/X transition
-> DomainSplitterUnit                 [Layer 2, Node 01]
-> DomainTranslatorUnit               [Layer 2, Node 02]
-> SchemePlannerUnit                  [Layer 2, Node 03]
-> DomainProcessorUnit                [Layer 2, Node 04]
-> ApplierUnit                        [Layer 2, Node 05]
-> ResultTranslatorUnit               [Layer 2, Node 06]
-> ArbiterSelectorUnit                [Layer 2, Node 07]
-> ArbiterUnit                        [Layer 2, Node 08]
-> FormatterUnit                      [Layer 2, Node 09]
-> teaching and retraining flows      [Layer 3]
```

Layer 0 must be produced before Layer 1 for any batch targeting the adaptive
orchestrator. For batches targeting only the nine-node flow or teaching flow,
Layer 0 is not required — but if a scenario has been used in any adaptive routing
batch, Layer 0 pairs must exist for all subsequent batches using that scenario.

---

## Universal Pair Rule

Each pair has this shape:

```
Contract:
  <interface signature>

A:
  the exact runtime input object

B:
  the exact runtime output object
```

B must be the output itself, written in full. B must not say what the output
should contain.

---

## DPO Accuracy — How Reward Margin Works

Before writing any pair, understand the mechanism that determines whether a
batch improves the model or degrades it.

### What reward margin is

DPO training runs one optimizer step per pair. The step computes:

```
chosen_reward  = log P(chosen | prompt)   under the policy model
rejected_reward = log P(rejected | prompt) under the policy model
reward_margin  = chosen_reward - rejected_reward
```

When `reward_margin > 0`: the model assigns higher probability to chosen than
to rejected. The optimizer step reinforces this preference.

When `reward_margin < 0`: the model assigns higher probability to rejected than
to chosen. The optimizer step runs but pushes in the wrong direction — it makes
the model more uncertain about a distinction it was already getting wrong.

DPO accuracy for the batch is the fraction of pairs where `reward_margin > 0`.

### What the calibration run showed

Run 2026-05-06, 40 batches total:

```
Negative reward margin: 29 of 40 batches
Mean accuracy: 0.422
Worst batch:   acc=0.259, margin=-0.003108 (BUG domain)
Best batch:    acc=0.771, margin=+0.002569 (PLAN domain, batch 17)
Loss across all batches: ~0.693 = ln(2) = random-guess level
```

Loss staying at ln(2) across all batches means the model is not learning.
The chosen and rejected responses were too similar for the model to distinguish.

### Why the current rejected template fails

The rejected template used in the codebase:

```csharp
$"Do not generate a capability for sub-phase {subPhase.Value.SubPhase.SubPhaseId}."
```

This shares high-frequency tokens with the chosen response:
- chosen: `"Generated capability for sub-phase 3.4: Security and Authorization Model."`
- rejected: `"Do not generate a capability for sub-phase 3.4."`

Both contain `capability`, `sub-phase`, `3.4`. The model has no token-level signal
to distinguish them. `reward_margin` stays near zero or goes negative because the
token probability distributions overlap almost completely.

### What makes reward margin positive

The model can distinguish chosen from rejected when:

1. **Different sub-phase content.** Chosen discusses boundary decisions;
   rejected discusses class implementation. Different vocabulary, different
   structure, different token distribution.

2. **Different dependency structure.** Chosen shows `branch_2.depends_on=[branch_1]`;
   rejected shows `branch_2.depends_on=[]`. The presence or absence of the
   dependency field creates a measurable distributional difference.

3. **Different domain framing.** Chosen says "write tests for the auth guard"
   routes to 5.1 (Unit Tests); rejected says the same prompt routes to 3.4
   (Security Model). The sub-phase name and purpose create different token
   contexts.

All three cases require that rejected has real content — not a negation of chosen.

### The two-token-overlap test

Before adding a pair to a batch, apply this test:

Count the tokens shared between `chosen` and `rejected` that are not also in the
`prompt`. If more than 60% of non-prompt tokens are shared between chosen and
rejected, the pair will likely produce negative reward margin.

The negation template always fails this test because it shares every meaningful
token with chosen except "Do not".

A Type A rejected passes this test because it discusses a different sub-phase's
responsibility using that sub-phase's vocabulary — architecture vocabulary for a
3.1 rejected versus implementation vocabulary for a 4.1 chosen.

---

## DPO Pair Quality Rules — Universal

These rules apply to every pair produced from every layer. They override any
layer-specific guidance where there is a conflict.

### The forbidden rejected template

This template is forbidden in all batches:

```
"Do not generate a capability for sub-phase X."
```

And all variants:

```
"Skip sub-phase X."
"Sub-phase X is not applicable here."
"This prompt does not require sub-phase X."
```

All of these share too many tokens with the chosen response. They produced
negative reward margin in 29 of 40 batches in calibration run 2026-05-06.

### Three valid rejected types

**Type A — Plausible adjacent sub-phase.**

The rejected response addresses a different sub-phase that is semantically close
to the correct one. It contains real technical content and a coherent argument
but addresses the wrong responsibility.

Token distribution: Type A rejected uses the vocabulary of the wrong sub-phase
(e.g., class names and method signatures for a 4.1 rejected) rather than the
vocabulary of the correct sub-phase (boundary rules and ownership for a 3.1
chosen). The distributions are different enough for the model to detect.

**Type B — Correct sub-phases, wrong dependency order.**

The rejected response names the same sub-phases as the chosen response but
sequences them in the wrong order or omits a dependency constraint.

Token distribution: Type B rejected contains the same sub-phase names as chosen,
so the distinguishing signal is the `depends_on` field content. Chosen has
`depends_on=["branch_1"]`; rejected has `depends_on=[]`. This is a smaller
distribution difference than Type A, so the [WRONG: ...] annotation is especially
important — it provides additional token context that helps the model learn the
dependency direction.

**Type C — Wrong domain for the prompt's verb.**

The rejected response addresses the wrong domain based on a misreading of the
prompt's intent signal. The intent verb determines the domain:

```
"explain / how does X work / what is"  → CODE_Q
"X throws / X fails / error in / bug"  → BUG
"implement / add / build / create"     → PLAN or IMPLEMENT
"design / define boundaries"           → ARCH
"review / audit / verify"              → QA or ARCH review
"plan / sequence / order of"           → PLAN (SchemePlannerUnit)
```

Token distribution: Type C rejected uses the vocabulary of the wrong domain
(e.g., explanation vocabulary for CODE_Q versus planning vocabulary for PLAN).
This produces strong distributional differences and typically achieves positive
reward margin.

### Required annotation

Every rejected response must end with a [WRONG: ...] block naming the specific
confusion. The annotation serves two purposes: it documents the confusion for
the pair author, and it adds tokens to the rejected response that describe why
the answer is wrong — giving the model additional signal beyond the content alone.

```
[WRONG: "define boundaries" asks who owns what and where dependencies flow,
not what the class body contains. Writing the class body is implementation.
Implementation decisions follow boundary decisions, they do not replace them.]
```

A generic annotation ("this is incorrect", "wrong approach") is not acceptable.
The annotation must name the specific confusion — which sub-phase the rejected
incorrectly addresses and why that sub-phase does not own this responsibility.

### Annotation quality examples

**Weak annotation — does not name the confusion:**

```
[WRONG: This is the wrong sub-phase for this prompt.]
```

**Strong annotation — names the specific confusion:**

```
[WRONG: "design the observability strategy" asks what signals to measure and
what thresholds to alert on — not how to configure a Grafana dashboard.
Configuring the dashboard (7.3) follows from the strategy (3.5).
The strategy must exist before the configuration can be meaningful.]
```

### One prompt per pair per batch

A prompt must not appear as chosen in one pair and as rejected in another within
the same batch. When one prompt maps to multiple sub-phases, each sub-phase gets
a separate pair — each pair has a different chosen (the correct response for that
sub-phase) and a different rejected (a plausible adjacent sub-phase confusion for
that sub-phase).

### TrainingText must use compiled form

Every pair where `TrainingText` uses the raw prompt string instead of the compiled
form is rejected at the measurement gate regardless of accuracy.

The compiled form is:

```
"semantic_prompt: {compiledText} branch_result: {branchResult}
 sub_phase: {subPhaseId} {subPhaseName} {subPhasePurpose} {predicateIds}
 semantic_compressed: {compiledText} teaching_vector_60: {vector}"
```

Where `compiledText` is the normalized prompt with predicate tokens appended —
not the raw user input. This is the gate from PLAN-COMPILED-PROMPT-IN-TRAINING-v1.md.

### Batch composition minimums

| Pair type | Minimum fraction | When required |
|---|---|---|
| Type A (adjacent sub-phase) | 40% | Always |
| Type B (wrong dependency order) | 20% | When any prompt in the batch covers multiple tasks |
| Type C (wrong domain) | 30% | For every domain present in the batch |
| Stage 9 corruption rows | 1 of each kind | Every batch |

### Pre-commit measurement gate

Before a batch is committed to the training ledger, compute:

```
reward_margin_mean = mean(ChosenReward - RejectedReward) across all DPO steps
```

A batch is rejected when `reward_margin_mean < 0`.

Diagnosis table when margin is negative:

| Symptom | Cause | Fix |
|---|---|---|
| All pairs negative | Rejected uses forbidden template | Rewrite all rejected as Type A, B, or C |
| Specific sub-phase pairs negative | Chosen and rejected share too many tokens | Ensure rejected addresses a visibly different sub-phase |
| Type B pairs negative | Dependency order in rejected is actually correct | Verify dependency table; swap branches |
| Corruption rows positive but real pairs negative | Real pairs are weaker than corruption pairs | Strengthen chosen/rejected distinction in real pairs |
| All pairs near zero margin | Prompts are too short or abstract | Use prompts with named artifacts and technical specifics |

---

## ARCH Sub-Phase Confusion Reference Table

Use this table to select a plausible rejected for every ARCH pair. The "Why it
is wrong" column provides the basis for the [WRONG: ...] annotation.

| Chosen sub-phase | What it owns | Most plausible rejected | Why it is wrong |
|---|---|---|---|
| 3.1 Architecture and Component Topology | Boundary decisions, dependency direction | 4.1 Backend Services | Writes the class instead of deciding the boundary |
| 3.1 Architecture and Component Topology | Boundary decisions, dependency direction | 4.4 Refactoring | Moves code instead of deciding ownership |
| 3.2 Data Model and Persistence Design | Entity shape, storage rules, consistency | 4.1 Backend Services | Writes the repository instead of defining the model |
| 3.2 Data Model and Persistence Design | Entity shape, storage rules, consistency | 4.5 Integration | Writes the adapter instead of deciding the boundary |
| 3.5 Observability and Operational Design | What to measure, alert thresholds, what not to log | 4.1 Backend Services | Wires the logger instead of defining the strategy |
| 3.5 Observability and Operational Design | What to measure, alert thresholds, what not to log | 7.3 Monitoring and Feedback Loop | Configures the dashboard before signals are designed |
| 6.1 Manual QA and Acceptance Review | Human review criteria, release judgment | 5.1 Unit Test Design | Automates instead of defining human acceptance |
| 6.1 Manual QA and Acceptance Review | Human review criteria, release judgment | 5.3 End-to-End Scenario Testing | Automates the user journey instead of a human checklist |

### ARCH annotation templates

For 3.1 → 4.1 confusion:

```
[WRONG: "{prompt verb}" asks who owns what and where dependencies flow —
not what the class body contains. The class is the implementation of the boundary
decision. The boundary decision must be made first.]
```

For 3.5 → 7.3 confusion:

```
[WRONG: "design the observability strategy" asks what signals to capture and what
thresholds matter — not how to configure monitoring tooling. 7.3 Monitoring sets
up the infrastructure to collect what 3.5 defines. 3.5 must come first.]
```

For 6.1 → 5.x confusion:

```
[WRONG: acceptance review requires human judgment on the release as a whole.
Automated tests verify specific paths but cannot make the release judgment call.
6.1 owns the human checklist; 5.x owns the automated suite.]
```

---

## PLAN Dependency Ordering Reference Table

Use this table to construct Type B rejected responses. The wrong order must be
plausible — someone testing-first would reasonably put 5.2 before 3.4.

| Correct sequence | Natural wrong order for rejected | Why the wrong order fails |
|---|---|---|
| 3.1 Architecture → 4.1 Backend Services | 4.1 first, 3.1 as afterthought | Implementing without boundaries creates coupling |
| 3.2 Data Model → 3.3 API Contract | 3.3 first, model derived from contract | Contract shape depends on model shape, not the reverse |
| 3.4 Security → 5.2 Integration Tests | 5.2 first with mock tokens | Mock tokens cannot test real tenant isolation |
| 3.1 + 3.4 parallel → 4.1 after both | All three in parallel | 4.1 cannot implement what 3.1 has not decided |
| 2.2 Acceptance Criteria → 5.1 Unit Tests | 5.1 first, criteria inferred | Tests define what passes; criteria define what should pass |
| 3.5 Observability Design → 7.3 Monitoring | 7.3 first, design retroactively | Cannot configure what has not been defined |
| 3.3 API Contract → 4.5 Integration Connectors | 4.5 first, contract from implementation | Implementation detail leaks into public contract |

### PLAN Type B annotation template

```
[WRONG: {branch_2.action} requires {what branch_1 produces}. Without {branch_1}
complete, {branch_2} either uses mocks that cannot test the real behavior, or
it implements against assumptions that the prior decision has not yet validated.
{branch_1} must be complete before {branch_2} can begin meaningfully.]
```

### What makes a good PLAN prompt

From calibration run analysis: the two best PLAN batches (acc=0.618 and 0.771)
shared specific prompt characteristics. The worst PLAN batch (acc=0.306) lacked them.

**Characteristics of prompts that produced positive reward margin:**

```
Named artifact present:
  "Flow 10", "Phase 3", "Factory Interface", "AuthGuard", "TenantResolver"
  Good: "implement Phase 1 — Factory Interfaces and Engine P..."
  Bad:  "let's work on the next steps"

Dependency language present:
  "before", "after", "once X is complete", "depends on", "when X is in place"
  Good: "add JWT auth and then write integration tests for tenant isolation"
  Bad:  "add JWT auth and write integration tests"

Technical specificity:
  Component names, sub-phase references, predicate vocabulary
  Good: "define the component boundaries for the notification service"
  Bad:  "Option A is clearly the right path"
```

**Prompts that must not be used for PLAN training:**

```
Meta-commentary: "this approach is clearly better"
Abstract correctness: "it's grounded in the existing documents"
Status updates: "we're making good progress on this"
```

These prompts contain no predicate vocabulary and produce near-zero compiled
predicate sets. The TrainingText carries no signal, and reward margin stays near
zero regardless of rejected quality.

---

## Correction Events as Pre-Made Pairs

Every Luba correction in a XIIGen session document is a pre-made chosen/rejected
pair. These require no invented rejected content and no annotation invention.

### Extraction pattern

```
What existed in the session:
  Claude's wrong answer  → becomes rejected
                           (add [WRONG: {the correction itself}] as annotation)
  Luba's correction      → becomes chosen
  The original prompt    → becomes A side

Example from session document:
  Prompt:   "design the data model for multi-tenant user preferences"
  Wrong:    "Create a UserPreferencesRepository with GetByUserId..."
  Correct:  "Entity: UserPreference {tenantId (partition key), userId...}"

Extracted pair:
  A: "design the data model for multi-tenant user preferences"
  Chosen: "Entity: UserPreference {tenantId (partition key), userId, preferenceKey,
           preferenceValue, lastModified}. Partition by tenantId..."
  Rejected: "Create a UserPreferencesRepository with GetByUserId(tenantId, userId),
             Set(tenantId, userId, key, value)..."
  [WRONG: "design the data model" asks for entity shape and partition rules,
  not the repository class. The repository is built after the model is decided.]
```

### Why correction events produce better pairs than constructed pairs

1. The wrong answer was genuinely produced — it is not a plausible wrong answer
   written by a human trying to imagine what wrong looks like. It is an actual
   wrong answer that a capable system produced.

2. The correction is the ground truth — not an approximation or inference.

3. The prompt is real developer language — not a synthetic prompt written to
   match a category.

4. Reward margin for correction-derived pairs is almost always positive because
   the chosen (correction) and rejected (wrong answer) come from different
   reasoning paths and have genuinely different token distributions.

### Correction event inventory

For each session document, record:

```
session_id
correction_count       — number of Luba corrections found
sub_phases_covered     — which sub-phases the corrections address
pair_quality_estimate  — High / Medium / Low based on:
  High: chosen and rejected have clearly different vocabulary
  Medium: chosen and rejected address different responsibilities
  Low: correction is only phrasing, not substance
```

Only High and Medium quality corrections enter the training ledger.

---

## Layer 0 — Adaptive Orchestrator Infrastructure Pairs

### Layer 0-A — Prompt Routing Input Compiler

```
PromptRoutingInputCompilerUnit
    : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
```

**B field names** (use these exactly — no invented fields):

```
DocumentId             — "{run_id}-prompt"
SourcePath             — "logical-orchestrator://user-prompt"
SourceTier             — "runtime_prompt"
LicenseOrAuthority     — "user_request"
DocumentType           — "user_prompt"
DocumentTitle          — "Runtime user prompt"
CompiledText           — user_request string trimmed
CompiledPredicateIds   — matched predicate IDs from the lexicon
SourceStatus           — "ready" when predicates found; "knowledge_edge: ..." when none
KnownFailureMode       — "runtime_prompt_compiled"
```

**Predicate lexicon:**

```
SECURITY      — jwt, authentication, auth, oauth, oidc, token, session, asvs, mfa, rbac, security, authorization
AUTHORIZATION — authorization, authorisation, permission, rbac, scope, tenant, ownership
API_CONTRACT  — api, endpoint, contract, schema, request, response, openapi
ARCHITECTURE  — architecture, component, topology, boundary, dependency, service
IMPLEMENT     — "implement the", "implementation plan", connector, provider, "integration layer", "code change"
TEST          — test, testing, "integration test", "unit test", coverage, fixture
QA            — qa, quality, release, acceptance, verification
UI            — ui, ux, screen, layout, design, interaction, accessibility
DOCUMENT      — document, documentation, spec, specification, requirements
```

Confidence: 0.92 when at least one predicate matches. 0.70 when none match.

**Example — multi-predicate prompt:**

```
Contract:
  PromptRoutingInputCompilerUnit
      : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>

A:
  run_id: "O1-router-001"
  scenario_id: "O1"
  user_request: "We need to add multi-tenant JWT authentication to the API
                 and make sure tenant isolation is tested."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  DocumentId: "O1-router-001-prompt"
  SourcePath: "logical-orchestrator://user-prompt"
  SourceTier: "runtime_prompt"
  LicenseOrAuthority: "user_request"
  DocumentType: "user_prompt"
  DocumentTitle: "Runtime user prompt"
  CompiledText: "We need to add multi-tenant JWT authentication to the API
                 and make sure tenant isolation is tested."
  CompiledPredicateIds: ["API_CONTRACT", "AUTHORIZATION", "SECURITY", "TEST"]
  SourceStatus: "ready"
  KnownFailureMode: "runtime_prompt_compiled"
  [unit result envelope]
    Confidence: 0.92
    PredicateIds: ["API_CONTRACT", "AUTHORIZATION", "SECURITY", "TEST"]
```

**Example — no-predicate prompt:**

```
A:
  run_id: "O1-router-002"
  user_request: "The 403 is wrong on the admin endpoint."

B:
  DocumentId: "O1-router-002-prompt"
  CompiledText: "The 403 is wrong on the admin endpoint."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge: no high-signal development predicate found"
  KnownFailureMode: "runtime_prompt_compiled"
  [unit result envelope]
    Confidence: 0.70
    PredicateIds: []
    Warnings: ["no_high_signal_predicate"]
```

**DPO pair — Type A rejected:**

```
Prompt: "test coverage shows 42% on the authentication module"

CHOSEN:
  CompiledPredicateIds: ["TEST", "SECURITY"]
  SourceStatus: "ready"
  Confidence: 0.92

REJECTED (wrong-predicate-assignment):
  CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
  SourceStatus: "ready"
  Confidence: 0.92
[WRONG: "test coverage" triggers TEST as the primary predicate. "authentication
module" is the object being measured, not the action being taken. SECURITY is
secondary to TEST when the primary verb is coverage measurement. Assigning only
SECURITY and AUTHORIZATION ignores the dominant signal in the prompt.]
```

---

### Layer 0-B — Multi-Sub-Phase Router

```
MultiSubPhaseRouterUnit
    : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
```

The A for every router row comes directly from the B of the corresponding
compiler row. Do not invent router A shapes.

**B field names per decision** (use these exactly):

```
DocumentId                — carried from the source document
SourceStatus              — "predicted" or "knowledge_edge"
PredictedPhaseId          — phase identifier e.g. "3"
PredictedSubPhaseId       — sub-phase identifier e.g. "3.4"
Confidence                — routing confidence 0.0 to 1.0
NearestWrongSubPhaseScore — score of closest non-selected sub-phase
EvidencePredicates        — predicate IDs that grounded this decision
KnowledgeEdge             — true when confidence below floor
FailureClassification     — "none" for clean routes
```

Do not add `dependency_order`, `parallel_group`, `domain_label`, or `is_primary`.

**Example — clean multi-route:**

```
Contract:
  MultiSubPhaseRouterUnit
      : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>

A:
  DocumentId: "O1-router-001-prompt"
  CompiledText: "We need to add multi-tenant JWT authentication to the API
                 and make sure tenant isolation is tested."
  CompiledPredicateIds: ["API_CONTRACT", "AUTHORIZATION", "SECURITY", "TEST"]
  SourceStatus: "ready"

B:
  routing_decisions:
    - DocumentId: "O1-router-001-prompt"
      SourceStatus: "predicted"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.4"
      Confidence: 0.91
      NearestWrongSubPhaseScore: 0.52
      EvidencePredicates: ["AUTHORIZATION", "SECURITY"]
      KnowledgeEdge: false
      FailureClassification: "none"
    - DocumentId: "O1-router-001-prompt"
      SourceStatus: "predicted"
      PredictedPhaseId: "3"
      PredictedSubPhaseId: "3.3"
      Confidence: 0.78
      NearestWrongSubPhaseScore: 0.41
      EvidencePredicates: ["API_CONTRACT"]
      KnowledgeEdge: false
      FailureClassification: "none"
    - DocumentId: "O1-router-001-prompt"
      SourceStatus: "predicted"
      PredictedPhaseId: "5"
      PredictedSubPhaseId: "5.2"
      Confidence: 0.84
      NearestWrongSubPhaseScore: 0.48
      EvidencePredicates: ["TEST"]
      KnowledgeEdge: false
      FailureClassification: "none"
```

**DPO pair — ambiguous-predicate-case:**

```
CHOSEN: routes to 3.4 AND 3.3 — two decisions (shown above)

REJECTED:
  routing_decisions:
    - PredictedSubPhaseId: "3.4"
      Confidence: 0.91
      EvidencePredicates: ["AUTHORIZATION", "SECURITY"]
      KnowledgeEdge: false
    (only one decision — 3.3 omitted)
[WRONG: API_CONTRACT predicate is present from "API" in the prompt. When multiple
predicates match different sub-phases, all sub-phases must be routed. Returning
only the highest-confidence decision suppresses routing signal for API_CONTRACT
and leaves 3.3 as a knowledge edge when it should be a predicted route.]
```

---

### Layer 0-C — Knowledge Edge Detection

**The gap branch shape** (all six metrics required):

```
FlowId:          "knowledge-edge-missing-flow"
ScenarioId:      <the request ScenarioId>
OverallVerdict:  "KNOWLEDGE_EDGE"
FinalHumanResult: "Knowledge edge: routed to sub-phase '<id>', but <reason>."
Metrics:
  knowledge_edge:        "True"
  missing_sub_phase:     <PredictedSubPhaseId>
  missing_flow:          "True"
  routed_phase_id:       <PredictedPhaseId>
  routing_confidence:    <Confidence as 2-decimal string>
  knowledge_edge_reason: "no logical flow registered for routed sub-phase"
EvidenceIds: <EvidencePredicates from routing decision>
Warnings:    ["knowledge_edge"]
```

Every knowledge-edge pair must be accompanied in the same batch by at least one
comparison pair where a registered sub-phase (3.4) produces a PASS result.

**DPO pair — knowledge-edge-suppressed:**

```
CHOSEN:
  OverallVerdict: "KNOWLEDGE_EDGE"
  Metrics: knowledge_edge=True, missing_sub_phase=3.3, missing_flow=True,
           routed_phase_id=3, routing_confidence="0.83",
           knowledge_edge_reason="no logical flow registered for routed sub-phase"

REJECTED:
  OverallVerdict: "PASS"
  (no knowledge edge markers)
[WRONG: when no flow is registered for the routed sub-phase, the system cannot
produce a real result. Returning PASS fabricates a result. The correct output
is KNOWLEDGE_EDGE with all six metrics. The model must learn to detect this
case and produce the knowledge-edge structure rather than a silent pass.]
```

---

### Layer 0-D — Flow Factory Selection

Every batch must include all four trust-state cases:

1. `UseLearnedUnitWhenAvailable: true` + Trusted learned unit → learned unit selected
2. `UseLearnedUnitWhenAvailable: false` + Trusted learned unit → deterministic selected
3. `UseLearnedUnitWhenAvailable: true` + only Rejected learned unit → deterministic fallback
4. `UseLearnedUnitWhenAvailable: true` + Probationary + minimum trust Probationary → Probationary selected

**DPO pair — Rejected-unit case:**

```
CHOSEN: deterministic unit selected (Rejected learned unit excluded)
REJECTED: Rejected learned unit selected
[WRONG: Rejected units are excluded from selection regardless of the
UseLearnedUnitWhenAvailable flag. A unit at Rejected trust state was assessed
as harmful or incorrect. Selecting it would bypass the trust evaluation system.
The deterministic fallback must be used when the only learned unit is Rejected.]
```

---

## Layer 1 — Transition Orchestrator Pairs

```
LogicalTransitionOrchestrator : ILogicalOrchestrator
```

The A side is the real user prompt. The B side includes the full candidate set
with selected and rejected candidates.

**DPO pair:**

```
CHOSEN: feature_addition transition selected, confidence=0.88
        candidate_set shows repair and external_source_integration rejected with reasons

REJECTED: repair transition selected for a prompt that adds new capability
[WRONG: repair applies when existing behavior is broken or incorrect. This prompt
adds JWT authentication that does not exist yet — it is an addition, not a fix.
Selecting repair would set the wrong A/B/X structure for all downstream nodes.]
```

---

## Layer 2 — Nine-Node Flow Pairs

Downstream node pairs must be written from real upstream outputs — never from
invented shapes.

### Node 01 — Domain Splitter

```
DomainSplitterUnit : ILogicalUnit<SelectedLogicalTransition, DomainPacket[]>
```

B contains one packet per territory actually present in the transition.

Required fields per packet:

```
packet_id, domain_label, phase, sub_phase, task_type,
prompt_fragment, coverage_status, reason, handoff_marker, source_evidence
```

**DPO pair:**

```
CHOSEN: packets for 3.4 and 5.2 only — both present in the transition A/B/X

REJECTED: packets for 3.4, 5.2, and 3.1 — 3.1 added because it usually appears
          alongside auth work
[WRONG: the domain splitter adds only territories present in the transition.
Architecture and component topology (3.1) was not stated in the prompt or in
the A/B/X statement. Adding it inflates the branch count and sends the planner
a false scope signal.]
```

### Node 02 — Domain Translator

```
DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
```

A is exactly one domain packet — array-node, never the full packet array.

### Node 03 — Scheme Planner

```
SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
```

B is the ordered work branch array with explicit dependency relationships.

Required fields per branch:

```
branch_id, domain_packet_ids, action, produces, depends_on,
parallel_group, wait_state, recovery_path, boundary_or_knowledge_edge_status
```

**DPO pair — highest priority for PLAN training:**

```
CHOSEN:
  branch_1: sub_phase=3.4, action="Define JWT model and bypass paths",
            depends_on=[], parallel_group=1
  branch_2: sub_phase=5.2, action="Write tenant isolation integration tests",
            depends_on=["branch_1"], parallel_group=2

REJECTED (Type B — reversed dependency):
  branch_1: sub_phase=5.2, action="Write tenant isolation tests with mock tokens",
            depends_on=[], parallel_group=1
  branch_2: sub_phase=3.4, action="Define JWT model after tests specify it",
            depends_on=["branch_1"], parallel_group=2
[WRONG: integration tests for tenant isolation require a working JWT mechanism to
issue and validate real tokens. Mock tokens cannot test real tenant isolation
because they bypass the actual validation logic. The security model must be
defined and deployed before integration tests can run meaningfully.]
```

### Nodes 04–06 — Branch Execution

Node 04 `DomainProcessorUnit`, Node 05 `ApplierUnit`, Node 06 `ResultTranslatorUnit`
are array-node positions. Write one branch at a time.

Node 06 may fold applied results into fewer fragments but every status class
(accepted, blocked, rejected, knowledge-edge) must remain visible in B.

### Node 07 — Arbiter Selector

```
ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>
```

B names each selected arbiter and each considered-but-not-selected arbiter with
the reason. This teaches arbiter routing, not arbiter judgment.

### Node 08 — Arbiter Unit

```
ArbiterUnit : ILogicalUnit<ArbiterReviewRequest, ArbiterVerdict>
```

For clean pass cases B may be exactly `verdict: "pass"`.

**DPO pair:**

```
CHOSEN: verdict=pass (implementation satisfies the stated control)

REJECTED: verdict=concern, violated_constraint="missing @UseGuards decorator"
          (when the implementation actually has the guard)
[WRONG: a concern verdict requires an identified gap in the implementation.
The @UseGuards decorator is present on the controller. Returning a concern
verdict when the control is satisfied teaches the arbiter to fire false positives,
which degrades the quality gate signal for all subsequent batches.]
```

### Node 09 — Formatter

```
FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
```

B names accepted, blocked, rejected, knowledge-edge, and next-action sections
where those statuses exist.

---

## Layer 3 — Teaching Unit Pairs

### Model Trainer

```
ModelTrainingUnit : ILogicalUnit<FeatureVectorDataset, TrainingResult>
```

B must include all resource fields:

```
model_artifact_path, model_artifact_bytes,
training_started_at, training_completed_at, teaching_time_ms,
cpu_time_ms, working_set_peak_bytes, managed_allocation_delta_bytes,
disk_read_bytes, disk_write_bytes,
training_row_count, held_out_row_count,
training_accuracy, held_out_accuracy, warnings
```

### Held-Out Evaluator and Correction Learner

```
HeldOutEvaluationUnit : ILogicalUnit<ModelEvaluationInput, HeldOutEvaluationResult>
CorrectionLearningUnit : ILogicalUnit<CorrectionTrainingInput, TrainingResult>
```

Held-out B includes correct count, failed pair ids, confusion matrix, and latency.
Correction learner B includes before/after verdict for the corrected input.

---

## Stage 9 — Corruption Cases

Every batch must include at least one of each.

### wrong-predicate-assignment

```
Prompt: "test coverage shows 42% on the authentication module"
Correct:   CompiledPredicateIds: ["TEST", "SECURITY"]
Corrupted: CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
[WRONG: "test coverage" triggers TEST as the primary verb signal.
"authentication module" is the object being measured. TEST takes priority.
Assigning only SECURITY ignores the dominant predicate in the prompt.]
```

### ambiguous-predicate-case

```
Prompt: "add authorization to the API endpoint"
Correct:   Routes to 3.4 (SECURITY) AND 3.3 (API_CONTRACT) — two decisions.
Corrupted: Routes only to 3.4, ignores API_CONTRACT.
[WRONG: API_CONTRACT predicate is present from "API endpoint". When multiple
predicates match different sub-phases, all sub-phases must appear in the routing
output. Returning only one suppresses valid routing signal.]
```

### knowledge-edge-suppressed

```
Prompt routes to 5.3 — no flow registered.
Correct:   OverallVerdict: "KNOWLEDGE_EDGE"
           Metrics: knowledge_edge=True, missing_sub_phase=5.3, missing_flow=True,
                    routed_phase_id=5, routing_confidence="0.84",
                    knowledge_edge_reason="no logical flow registered for routed sub-phase"
Corrupted: OverallVerdict: "PASS" — knowledge edge suppressed.
[WRONG: PASS fabricates a result the system cannot produce for an unregistered
sub-phase. The required output is KNOWLEDGE_EDGE with all six metrics present.]
```

---

## Source Families

Every source document entering the training pipeline belongs to one of the
families below. The family determines which pair types are permitted, what the
typical overlap ratio is, and what is excluded.

The rule that applies to all families without exception: **B must be the actual
runtime output object, not a description of it.** This is EM-2 and it fires
more often from external source families than from session documents.

---

### U01 — Product and business flow specifications

**What it is:** Business flow analysis documents, task catalogs, engine
architecture documents per business module.

**DPO output:** These documents contain three zones that produce different pair
types. Do not treat the whole document as one type.

Zone A — acceptance criteria sections → ArbiterUnit pairs (Node 08).
The criteria state what must be true before a result is accepted. Chosen = pass
verdict when criterion is met. Rejected = pass verdict when criterion is absent
from the implementation.

Zone B — task type contracts → Layer 0 routing pairs.
The task type name, sub-phase, and predicate vocabulary become routing evidence
for MultiSubPhaseRouterUnit pairs and for `CompiledPredicateIds` construction.

Zone C — architectural decisions → ARCH Type A pairs.
Ownership rules and component responsibilities in BFA documents are B-side
authority for 3.1 and 3.2 chosen responses.

**Zone mapping rule:** Before extracting any pair, label each paragraph as Zone
A, B, or C. Do not mix zones in a single pair.

**Excluded:** Do not use BFA document prose as A-side developer prompts. The
language is formal and domain-specific — it does not match real developer input.

**Typical overlap:** 0.10–0.25 for Zone A/C pairs. 0.15–0.30 for Zone B pairs.

---

### U02 — Deep research reports

**What it is:** External domain research documents, marketplace research,
technology landscape surveys. These describe the external world, not XIIGen
internal decisions.

**DPO output:** Permitted for CODE_Q pairs only — prompts that ask about
external standards, protocols, or technology behaviour. The research document
provides authority evidence for the B side when it cites a named external
standard (RFC number, specification name, version).

```
Permitted use:
  Prompt:  "how does GraphQL federation handle cross-service type merging?"
  Chosen:  "[from research document citing Apollo Federation spec v2]
            Each subgraph defines its own types. The gateway merges them using
            the @key directive — fields marked @key in one subgraph can be
            resolved from another. The merge happens at query planning time,
            not at runtime."
  Rejected: "[outdated version — pre-federation v2]
             Each service must republish its full schema to the gateway.
             Cross-service references are handled by stitching at the gateway."
  [WRONG: Schema stitching was the pre-federation approach. Federation v2 uses
  subgraph-owned @key directives, not gateway-side stitching. The outdated
  description teaches the wrong runtime behaviour.]
```

**Excluded:** Do not use research documents as B-side authority for any ARCH,
PLAN, or BUG pair. Research documents describe the external world — they cannot
confirm what XIIGen has decided. Do not use as PLAN ordering authority.

**Failure mode:** Using a research document to justify a PLAN branch order
or an ARCH decision produces pairs that may become wrong when the external
landscape changes. The training set cannot be easily corrected when the
underlying source is invalidated by external events.

**Typical overlap:** 0.20–0.35 for CODE_Q version-comparison pairs.

---

### U03 — Translation and migration plans

**What it is:** Step-by-step migration plans (NestJS → Python, Figma → React
Native, platform to platform), translation state JSON files recording what has
been completed and what is pending.

**DPO output:** Primary source for PLAN Type B pairs. Migration plans contain
the clearest dependency ordering in the corpus because they were written to be
executed in sequence — the ordering rationale is already in the document.

```
Example from a NestJS → Python migration plan:

Chosen (PLAN — correct migration order):
  Branch 1 — 3.1 Architecture: map all NestJS module boundaries to Python
    service boundaries. Produce the dependency graph before any code moves.
    depends_on: []
  Branch 2 — 4.1 Backend: migrate core fabric interfaces to Python first —
    these are the only dependency that every other service imports.
    depends_on: ["branch_1"]
  Branch 3 — 4.1 Backend: migrate individual services against the new
    Python interfaces.
    depends_on: ["branch_2"]

Rejected (PLAN — migrate services before interfaces):
  Branch 1 — migrate individual services first.
  Branch 2 — migrate fabric interfaces after all services are moved.
  depends_on: [] for both.
[WRONG: individual services import the fabric interfaces. Migrating services
before the interfaces means services are rewritten against TypeScript interfaces
that will change again when the interfaces are migrated. The interfaces must
be established first — they are the stable contract that all services depend on.]
```

**TRANSLATION_STATE.json files** record completed steps, pending steps, and
blocked steps. Use as Layer 3 measurement evidence (parallel to Type 4 in the
registry) — the state file shows what a correct workflow execution record looks
like. Rejected = state file with missing required fields or incorrect status values.

**Excluded:** Do not use migration step text as A-side developer prompts without
verifying the prompt is in developer language ("fix the rxjs pipe", not "execute
migration step 3.2.1").

**Typical overlap:** 0.30–0.45 for Type B ordering pairs. Annotation is critical —
the rationale from the plan document becomes the annotation text directly.

**Pair kind:** `migration_plan`

---

### U04 — Plugin and extension marketplace documents

**A-side prompts only.** These documents describe external platforms (WordPress,
Adobe Express, plugin ecosystems) that XIIGen does not control. The platform
specifications may change and cannot serve as B-side authority for XIIGen decisions.

Real developer prompts from plugin integration sessions ("how do I connect a
WordPress webhook to a XIIGen flow?", "the Adobe Express plugin errors on export")
are valid Type 1 A-sides — they carry UI, IMPLEMENT, or BUG predicates.

**Excluded as B-side authority:** Do not use plugin documentation to justify ARCH,
PLAN, or any XIIGen-internal decision. Platform documentation is evidence for
the external context, not for internal contracts.

---

### U05 — Skill library documents

**What it is:** SKILL.md files, skill manifests, prompt engineering templates
defining how the agent should behave on specific task types.

**DPO output:** Type C pairs only — wrong domain for the prompt's verb.
A skill document defines the domain boundary for a specific task type. The
correct response stays within that boundary. The rejected response crosses it
by applying a different skill's logic to the same prompt.

```
Example from an architecture skill document:

Prompt:  "define the component boundaries for the payment service"

Chosen (ARCH — within skill boundary):
  PaymentGateway owns external provider calls. PaymentProcessor owns transaction
  logic. PaymentStore owns persistence. Dependencies: Processor → Store → Gateway
  direction only. Forbidden: Gateway must not call Processor.

Rejected (wrong skill — applies implementation logic):
  Create a PaymentService class with ProcessPayment(), RecordTransaction(), and
  CallProvider() methods. Wire the dependencies through DI.
[WRONG: the ARCH skill defines boundaries and dependency rules, not class bodies.
Applying the implementation skill here produces code instead of a boundary
decision. The class cannot be written until the boundary is decided.]
```

**Type C only — not Type A or B.** The skill library defines which domain owns
a task, not the implementation within that domain. A Type A rejected (adjacent
sub-phase) can still be used, but the chosen must satisfy the skill's domain
contract exactly.

**Excluded:** Do not use SKILL.md procedural steps as A-sides. Do not use
prompt templates as developer prompts — they are authority evidence for B,
not natural user language.

**Typical overlap:** 0.10–0.25 for Type C pairs (different skill vocabulary
sets are largely non-overlapping).

**Pair kind:** `skill_boundary`

---

### U06 — Delivery test packages

**What it is:** End-to-end test plans, GATE-B-FINDINGS.md files, learning
pipeline test plans, delivery verification packages.

**Split into two zones:**

Zone A — GATE-B-FINDINGS files → Node 08 ArbiterUnit pairs. These contain
real production findings: "GATE B: pattern X absent on line Y" is a real
concern or reprocess verdict produced by an actual delivery. Use directly
as ArbiterUnit B-side content — these are the highest-quality arbiter pairs
available because the finding is real, not constructed.

```
GATE-B-FINDINGS entry:
  finding: "E34V2 fusion absent — AstRelevanceRanker has BM25 and KNN
            separately but no fusion step. EXPERIMENT-IMPLEMENTATION-STATUS
            shows NOT IMPLEMENTED."
  verdict: concern

→ ArbiterUnit pair:
  A: ArbiterReviewRequest for retrieval path review
  Chosen: verdict=concern, finding=[content from GATE-B-FINDINGS]
  Rejected: verdict=pass, finding="BM25 and KNN are present in AstRelevanceRanker"
  [WRONG: present is not the same as fused. The fusion is not implemented —
  GATE-B-FINDINGS confirms this with a direct code check. Pass verdict requires
  the control to be satisfied, not just the components to exist.]
```

Zone B — E2E test plans → 5.2 and 5.3 sub-phase pairs. Test plans describe
what integration tests must verify. Use as B-side authority for TEST-predicate
routing decisions and for Node 03 branch content covering 5.2/5.3.

**Excluded:** Do not use delivery package pass/fail summaries as PLAN A-sides
without verifying the summary contains developer-language prompts.

**Typical overlap:** 0.10–0.20 for Zone A arbiter pairs.

---

### U07 — Flow portability protocols

**What it is:** FLOW-PORTABILITY-TEST-PROTOCOL documents, FORK-EXECUTION-DIRECTIVE
specifications, portability invariant checklists.

**DPO output:** The only source of invariant-based pairs in the corpus. Portability
protocols define what must be true for any flow regardless of tenant or environment.
An invariant violation is always a reprocess verdict — not a concern.

**Rejected construction rule for portability pairs:** Take a correct flow
description, introduce exactly one tenant-specific hardcode, mark it as rejected.
The annotation must name the specific invariant that fails.

```
Invariant: "no flow may embed a literal tenant ID in any route, selector,
            or configuration value."

Chosen (portability invariant satisfied):
  TenantAwareRouter resolves the tenant context from the request header at
  runtime. Route pattern: /api/{tenantId}/resource — tenantId from the resolved
  context, not from configuration.

Rejected (invariant violated):
  Route: /api/acme-corp/resource — tenant ID hardcoded in the route definition.
  The ACME Corp tenant is the primary customer so this route is stable.
[WRONG: portability invariant 3 (no literal tenant ID in route definitions)
is violated. A flow with a hardcoded tenant ID cannot be deployed for other
tenants without code changes. The portability test protocol requires
routing_confidence > 0 for any registered tenant, which this flow cannot
satisfy for tenants other than acme-corp.]
```

**FORK-EXECUTION-DIRECTIVE** documents define the contract for fork/join semantics
in multi-branch execution. Use as B-side authority for Node 03 SchemePlannerUnit
pairs that include parallel branches with join conditions.

**Typical overlap:** 0.10–0.20. Invariant vocabulary (tenant-specific, hardcoded,
portability test, routing_confidence) does not overlap with correct flow vocabulary
(resolves at runtime, from context, tenant-agnostic).

**Pair kind:** `portability_invariant`

---

### U08 — Design conversion documents

**A-side prompts only.** Figma-to-code conversion maps, Adobe Express design
specs, UI asset conversion instructions. These are source material for UI and
frontend sub-phase (1.1, 1.2, 4.2) developer prompts.

Real developer prompts from conversion sessions ("convert this Figma artboard to
a React component", "the Adobe export has the wrong font weight") are valid Type 1
A-sides carrying UI predicate.

**Excluded as B-side authority:** Design specs describe visual intent, not XIIGen
runtime contracts. Do not use as B-side for any non-UI sub-phase pair.

---

### U09 — Machine state JSON files

**What it is:** STATE.json, TRANSLATION_STATE.json, xiigen-file-state.json files
recording workflow execution state: completed steps, pending steps, blocked items,
artifact paths.

**Distinct from calibration metrics (Type 4 / Layer 3):** Type 4 records DPO
training metrics (loss, accuracy, checkpoint bytes). State JSON files record
workflow progress (step X complete, step Y pending, artifact Z at path W).

**DPO output:** Missing-field pairs for workflow state records. Chosen = complete
state record with all required status fields. Rejected = state record missing
required fields.

```
Required fields for a workflow state record:
  step_id, status (complete/pending/blocked),
  artifact_path (when status=complete),
  blocked_by (when status=blocked),
  completed_at (when status=complete)

Chosen:
  { step_id: "2B", status: "complete", artifact_path: "outputs/step-2b.json",
    completed_at: "2026-05-07T14:23:11Z" }

Rejected:
  { step_id: "2B", status: "complete" }
[WRONG: a complete state record must include artifact_path and completed_at.
Without artifact_path, the next step cannot locate the artifact it depends on.
Without completed_at, the timing audit cannot verify the ordering invariant
that step 2C must start after 2B completes.]
```

**Typical overlap:** < 0.15. Chosen has fields rejected lacks entirely.

---

### U10 — Source archives and snapshots

**Excluded.** ZIP archives and snapshots are delivery artifacts, not source
documents. A ZIP file as a unit has no DPO pair role.

**Required preprocessing:** Unpack the archive. Classify each contained file
by its document family (U01–U09, U11–U13). Apply the corresponding family rules
to each file individually.

Do not extract pairs from a file discovered inside a ZIP without first classifying
it. A session state document found inside a ZIP is a Type 13 document in the
registry — treat it as such, not as "a file from the ZIP".

---

### U11 — Agent tooling instructions

**What it is:** AGENTS.md, CLAUDE.md, copilot-instructions.md, tool-calling
instruction sets. These are machine-facing executable contracts — they specify
when to call which tool, how to interpret tool output, and what ordering is
required. Distinct from Type 14 behavioral rules (human-facing "what not to do").

**DPO output:** Executable-contract pairs. Chosen = agent follows the tool-calling
sequence as specified. Rejected = agent skips a required step or calls tools in
the wrong order.

```
AGENTS.md instruction:
  "Before writing any code: (1) call project_knowledge_search,
   (2) read the SKILL.md for the relevant domain,
   (3) only then produce output."

Chosen:
  Step 1: project_knowledge_search("component boundaries notification service")
  Step 2: read /mnt/skills/public/frontend-design/SKILL.md
  Step 3: [produces output using skill vocabulary]

Rejected:
  [produces output directly without reading skill or searching knowledge]
[WRONG: AGENTS.md requires project_knowledge_search and SKILL.md read before
any output. Producing output without these steps means the output may contradict
project-specific constraints that the knowledge search and skill file would have
surfaced. The two steps are not optional — they are the gate before output.]
```

**What distinguishes this from Type 14 (behavioral rules):**
- Type 14: rules state what a human author must not do when writing pairs ("do not use the negation template")
- Type 11 (agent tooling): contracts state what the agent must do at runtime ("call tool X before producing output")

Type 14 pairs test whether a plan or document follows the rules. Type U11 pairs
test whether the agent's runtime tool-calling behaviour follows its instructions.

**Excluded:** Do not use AGENTS.md rule text as developer A-side prompts. Do not
use tool-calling instructions as B-side authority for sub-phase content decisions.

**Typical overlap:** 0.10–0.20. Correct tool sequence vocabulary (search, read,
then produce) does not overlap with skipped-step vocabulary.

**Pair kind:** `tooling_contract`

---

### U12 — Capability catalogs and matrices

**Split handling — three zones:**

Zone A — capability matrices showing flow-to-subphase coverage → extend
corruption case `knowledge-edge-suppressed`. A matrix entry that shows "flow X
does NOT cover sub-phase W" is direct evidence that routing to W should produce
KNOWLEDGE_EDGE, not PASS.

Zone B — UNIFIED_SOURCE_INDEX and RAG indexes → vocabulary calibration only.
These are reference indexes, not authority sources. Use to verify that predicate
terms are consistent across documents. Do not build pairs directly from index text.

Zone C — SKILLS_FACTORY_RAG → excluded as direct pair source. RAG indexes
contain fragments from many documents mixed together. Unattributable fragments
cannot be used as B-side authority.

**Excluded as standalone B-side source:** Do not attribute a B-side claim to
a capability matrix unless the matrix cites a specific flow ID and sub-phase ID
with status evidence.

---

### U13 — Raw merged text dumps

**Excluded until preprocessed.** Files like `extracted_merged.txt` and
`planning_session.txt` are unsegmented concatenations of multiple conversations
or documents. A-side language from these files will contain fragments from
different contexts that were never connected in any real developer session.

**Required preprocessing protocol:**
1. Split on segment delimiters: `---`, timestamps, `Human:`, `Assistant:`,
   conversation boundary markers.
2. Classify each segment individually against U01–U12 family rules.
3. Extract pairs only from segments that pass the individual family gate.

Do not extract any pair from a raw merged dump without segment classification.
Segments that cannot be attributed to a single conversation or document are
excluded regardless of content quality.

---

### Source family decision table

| Family | Pair type | Use as A-side | Use as B-side authority | Typical overlap | Pair kind |
|--------|-----------|--------------|------------------------|-----------------|-----------|
| U01 — business flow specs | A, B, C (zone-dependent) | ✗ | ✓ (per zone) | 0.10–0.30 | flow_node, arbiter |
| U02 — research reports | C only | ✗ | ✓ CODE_Q only | 0.20–0.35 | research_fact |
| U03 — migration plans | B primary | ✗ | ✓ PLAN ordering | 0.30–0.45 | migration_plan |
| U04 — plugin docs | none | ✓ prompts only | ✗ | n/a | — |
| U05 — skill library | C only | ✗ | ✓ domain boundary | 0.10–0.25 | skill_boundary |
| U06 — delivery test packages | A (Zone A), C (Zone B) | ✗ | ✓ (per zone) | 0.10–0.25 | arbiter, flow_node |
| U07 — portability protocols | invariant-based | ✗ | ✓ invariant check | 0.10–0.20 | portability_invariant |
| U08 — design conversion | none | ✓ UI prompts only | ✗ | n/a | — |
| U09 — machine state JSON | missing-field | ✗ | ✓ state records | < 0.15 | teaching_unit |
| U10 — source archives | excluded | ✗ | ✗ | — | unpack first |
| U11 — agent tooling | contract-compliance | ✗ | ✓ tool sequence | 0.10–0.20 | tooling_contract |
| U12 — capability catalogs | Zone A: corruption only | ✗ | Zone A only | 0.05–0.15 | corruption_case |
| U13 — raw merged dumps | excluded | ✗ | ✗ | — | segment first |

---



1. Write `PromptRoutingInputCompilerUnit` pairs. No upstream dependency.
2. Write `MultiSubPhaseRouterUnit` pairs from compiler B outputs.
3. Write knowledge-edge detection pairs. Include at least one registered-flow comparison.
4. Write flow factory selection pairs. Include all four trust-state cases.
5. Write `LogicalTransitionOrchestrator` pairs.
6. Write Node 01 rows from selected transitions.
7. Write Nodes 02–09 from real upstream outputs only.
8. Write teaching unit rows after pair files exist.

Do not write downstream node pairs against invented upstream shapes.

---

## Required Coverage Labels

Every pair row must carry:

```
source_id, source_title, source_license_state, contract_signature,
transition_type, phase, sub_phase, task_type, node_role,
coverage_status, pair_kind, rejected_type (A / B / C),
compiled_prompt_in_training (true / false)
```

`compiled_prompt_in_training: false` causes the pair to be rejected at the
measurement gate regardless of accuracy.

Allowed `pair_kind` values:

```
adaptive_routing_compiler
adaptive_routing_multi_phase
knowledge_edge_detection
flow_factory_selection
transition_orchestrator
transition_internal_unit
flow_node
teaching_unit
held_out
corruption_case
rephrasing
correction_event
migration_plan          ← U03 — migration/translation plans
portability_invariant   ← U07 — portability protocol checks
tooling_contract        ← U11 — agent tool-calling contracts
skill_boundary          ← U05 — skill library domain boundary
research_fact           ← U02 — external research CODE_Q pairs
```

`correction_event` is a new kind for pairs extracted from Luba correction events
in XIIGen session documents.

---

## Failure Modes

**EM-1 — Contract Not Named.** Correction: add the interface signature before A.

**EM-2 — B Describes Output.** Correction: rewrite B as the runtime output object.

**EM-3 — Transition Replaces Flow Nodes.** Correction: restore nine-node rows.

**EM-4 — Downstream Shape Invented.** Correction: generate from real upstream outputs.

**EM-5 — Corpus Teaches Topic But Not Orchestration.** Correction: map to contract
signature at the point where the fact affects orchestration.

**EM-6 — Adaptive Routing Layer Missing.** Correction: produce Layer 0-A and 0-B pairs.

**EM-7 — Knowledge Edge Has No Registered Comparison.** Correction: add registered-flow
comparison pair per batch.

**EM-8 — Factory Selection Missing Rejected-Unit Case.** Correction: add the
rejected-unit case to every batch with factory selection pairs.

**EM-9 — Router B Uses Invented Fields.** Correction: use only actual
`SubPhaseRoutingDecision` fields.

**EM-10 — Rejected Uses Forbidden Template.** Correction: rewrite as Type A, B,
or C with [WRONG: ...].

**EM-11 — Rejected Has No Annotation.** Correction: add annotation naming the
specific confusion.

**EM-12 — Stage 9 Corruption Cases Missing.** Correction: add all three kinds.

**EM-13 — TrainingText Uses Raw Prompt.** Correction: rebuild TrainingText using
compiledText as promptSide. Raw prompt in TrainingText = rejected at measurement gate.

**EM-14 — Annotation Is Generic.** The [WRONG: ...] block says only "this is
incorrect" or "wrong approach" without naming the confusion. Correction: name
which sub-phase the rejected incorrectly addresses and why that sub-phase does
not own this responsibility.

**EM-15 — Reward Margin Negative Without Diagnosis.** Batch committed with
negative reward_margin_mean and no diagnosis entry in the ledger. Correction:
run the diagnosis table, identify the cause, fix the pairs, resubmit.

**EM-16 — PLAN Prompt Is Abstract Meta-Commentary.** The PLAN prompt contains
no named artifacts, no dependency language, and no technical specifics. Correction:
replace with a prompt from a XIIGen session that contains at least one named
component and one explicit dependency statement.

**EM-17 — Correction Event Quality Not Assessed.** Pairs extracted from correction
events were not rated High/Medium/Low before inclusion. Correction: rate each
correction event and exclude Low quality events (phrasing-only corrections where
chosen and rejected share the same technical content).

**EM-18 — Research Document Used as ARCH or PLAN Authority.** A pair uses a
deep research report (U02) as the B-side authority for an ARCH decision or PLAN
ordering. Research documents describe the external world and may become outdated.
Correction: use research documents only for CODE_Q external-standard pairs. For
ARCH/PLAN, use first-party decision documents (DECISIONS-LOCKED.md, session state).

**EM-19 — Migration Plan Used Without Zone Classification.** Pairs extracted from
a translation or migration plan (U03) without first labelling each section as
ordering content, state evidence, or A-side prompts. Correction: apply the three-zone
rule before extraction. Ordering sections → PLAN Type B. State JSON → Layer 3
evidence. Developer prompts → A-side only if in natural developer language.

**EM-20 — Source Archive or Merged Dump Used Without Preprocessing.** Pairs
extracted directly from a ZIP file (U10) or unsegmented text dump (U13) without
unpacking and segment classification. These sources mix contexts from multiple
conversations. Correction: apply the preprocessing protocol — unpack, segment,
classify each segment against its family rules, then extract.

**EM-21 — Portability Invariant Pairs Missing Invariant Number.** A portability
protocol pair (U07) names a violated constraint but does not cite the invariant
by number or name from the protocol document. The annotation says "portability
violated" without naming the specific check. Correction: every portability pair
annotation must name the specific invariant: "portability invariant 3 (no literal
tenant ID in route definitions) is violated."

**EM-22 — Agent Tooling Pair Tested as Behavioral Rule.** A pair from an agent
tooling document (U11 — AGENTS.md, CLAUDE.md) was written as a behavioral rule
pair (Type 14 pattern) rather than as a tool-sequence pair. The rejected describes
a human-authored document without Gate B rather than an agent skipping a required
tool call. Correction: U11 pairs test agent runtime behavior — chosen = correct
tool sequence, rejected = missing or wrong-order tool call. Use Type 14 pattern
only for human-authored output rules.

---

## Acceptance Checklist

- [ ] Every row names a contract signature.
- [ ] Every B side is an actual runtime output object.
- [ ] Layer 0-A B sides use actual SubPhaseRouterSourceDocument field names.
- [ ] Layer 0-A B sides show Confidence 0.92 when predicates matched, 0.70 when none.
- [ ] Layer 0-B B sides use actual SubPhaseRoutingDecision field names only.
- [ ] Layer 0-B A sides come from real compiler B outputs.
- [ ] Knowledge-edge Metrics include all six keys.
- [ ] Knowledge-edge pairs include at least one registered-flow comparison.
- [ ] Flow factory selection includes all four trust-state cases.
- [ ] Transition rows include selected and rejected candidates.
- [ ] Node 02 rows are one packet at a time.
- [ ] Node 03 rows express `depends_on` relationships — not just a step list.
- [ ] Node 03 pairs use Type B rejected with annotation from the dependency template.
- [ ] Array-node rows do not group multiple items into one A.
- [ ] Node 08 pass rows are silent if runtime pass is silent.
- [ ] Teaching rows include all resource measurement fields.
- [ ] No rejected response uses the forbidden negation template or any variant.
- [ ] Every rejected response includes a [WRONG: ...] annotation.
- [ ] Annotation names the specific sub-phase confusion — not a generic statement.
- [ ] Rejected type (A, B, or C) is recorded in the coverage label.
- [ ] compiled_prompt_in_training=true recorded for every pair.
- [ ] Batch reward_margin_mean >= 0 verified before commit.
- [ ] If reward_margin_mean < 0, diagnosis entry present in ledger before resubmit.
- [ ] PLAN prompts contain named artifact OR dependency language OR technical specifics.
- [ ] Correction events rated High or Medium before inclusion.
- [ ] Batch contains at least one of each Stage 9 corruption kind.
- [ ] Batch composition meets minimum pair type fractions.
- [ ] Every pair carries a `pair_kind` value from the allowed list.
- [ ] Source family identified for every source document used.
- [ ] U02 (research) sources used only for CODE_Q pairs — not for ARCH or PLAN.
- [ ] U03 (migration) sources zone-classified before extraction — ordering / state / prompts.
- [ ] U07 (portability) pairs cite the specific invariant number in the annotation.
- [ ] U10 (archives) and U13 (merged dumps) unpacked/segmented before any extraction.
- [ ] U11 (agent tooling) pairs test tool-call sequence — not human-authored rule compliance.
- [ ] U04 and U08 sources used as A-side prompts only — never as B-side authority.

---

## Last Updated

2026-05-08. Version 10.0.
