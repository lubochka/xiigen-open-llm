# Document Examination to Training Sets — Orchestration Edition
# Version: 9.0 — 2026-05-08
# Supersedes: EXAMINATION-METHOD-ORCHESTRATION-v8.md

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

## Source Examination Order

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

---

## Last Updated

2026-05-08.
