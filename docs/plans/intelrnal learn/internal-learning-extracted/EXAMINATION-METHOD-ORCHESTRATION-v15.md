# Document Examination to Training Sets — Orchestration Edition
# Version: 15.0 — 2026-05-08
# Supersedes: EXAMINATION-METHOD-ORCHESTRATION-v14.md

---

## What changed from v14

**Additional registry DTO gate added** for A01-A51, B13-B43, C01-C12, Z41-Z56,
D01-D19, E01-E16, F01-F17, G01-G13, H01-H08, I01-I14, J01-J09, K01-K13,
L01-L08, M01-M21, N01-N09, O01-O19, P01-P12, Q01-Q05, R01-R13,
S01-S14, T01-T10, U01-U20, V01-V08, W01-W16, X01-X16, Y01-Y10,
AA01-AA17, AB01-AB07, AC01-AC20, AD01-AD08, AE01-AE10, AF01-AF13,
and AG01-AG06 from
`XIIGEN-INTERNAL-SOURCE-REGISTRY-ADDITIONAL-TYPES-v1.md`. These families must be
examined by DTO shape, not filename alone. New failure modes EM-42 through EM-49
cover missing family DTO fields, external structural sources used as XIIGen
authority, missing recovered SKILL.md content, snapshot bundles used without
part-level classification, and session-analysis artifacts merged across rounds.
EM-71 and EM-72 add guards for AF planning/artifact verdict families and AG
renumbering/raw-intent families.

**A25–A36 source families added** from XIIGEN-INTERNAL-SOURCE-REGISTRY-ADDITIONAL-TYPES-v1.md
(companion to registry v11).

**Full sections for A25 (IaC) and A26 (DB migrations)** — both are `full_type_candidate`
with worked examples: A25 resource graph / deployment dependency pairs; A26 migration
order / schema constraint pairs. Both require security gates (no credentials, state files,
or production data). Pair kinds: `flow_node` and `arbiter`.

**Limited-authority sections for A31, A33, A36** — web server configs (routing/rewrite
only), IDE metadata (tooling setup only), repo governance (ownership/process only).

**Excluded-direct rules for A27, A29, A30, A34** — publish profiles (secure_operational),
native binaries, git internals, browser sidecars.

**Preprocess rules for A28, A35** — coverage binaries (convert then Type 4); non-ZIP
archives (extract + reclassify, same as U10/U19).

**A21 status corrected** to `limited_authority` per addendum ruling — synthetic catalogs
inherit A07 self-training risk.

**Source family decision table extended** to 48 rows (A25–A36 added).

**Failure modes EM-39 through EM-41 added** covering IaC credential exposure, reversed
DB migration order, and browser sidecar misclassification.

**Acceptance checklist extended** with per-family checks for A25–A36.

---

## What changed from v13

**Review findings applied.** Nine findings from the cross-document review:

**B-series families placeholder added** to source family decision table and
acceptance checklist. B01–B12 (serialized delivery parts, skills factory registries,
naming tables, validation reports, conflict analysis, DoD documents, prompt pattern
libraries, DNA patterns) are pending registry v11. Provisional classification
mapping provided: B05 → `naming_translation`, B06 → `arbiter`, B07 → `flow_node`,
B03 → `knowledge_edge_detection`. Field `pending_b_series: true` marks pairs for
retroactive reclassification when v11 is published.

**Batch composition cross-reference note added** to primary quota table: v6/v7 DPO
pair guidance subdivisions apply *within* these primary allocations.

**Canonicality gate upgraded to explicit stop rule** in acceptance checklist: now
reads "STOP HERE IF FAILED, do not proceed to any other check" with instruction to
record rejection as "canonicality: {class}" for accurate batch diagnostics.

**`naming_translation` pair_kind added** to allowed list. For B05 naming tables,
ROSETTA_STONE, and mapping matrices. Lowest overlap class (0.05–0.15). EM-37 now
covers misuse of wrong pair_kind for these pairs.

**EM-38 added** covering B-series documents used without `pending_b_series: true` flag.

**Cross-document navigation note added** to Required Coverage Labels section.

---

## What changed from v9

**Canonicality framework added as a section before Source Families.** v11 had
individual canonicality mentions per family but no unified framework. v12 adds the
seven canonicality classes from registry v7 (canonical, mirror, extracted_copy,
stale, external, archive_container, secure_operational), the required JSON fields
(`source_canonicality`, `canonical_confirmed_via`), and explicit gate failure rules:
mirror/stale/archive_container = gate failure; external + mixed_with_internal: true
= hard gate failure.

**`source_canonicality` added to Required Coverage Labels.** Every pair row now
requires this field. Valid at submission: `canonical` or `extracted_copy` only.

**U14–U24 zone markers and exclusion rules upgraded to v7 precision.** Zone A/B/C
markers for U15/U16/U20/U21 now include the exact text markers from registry v7.
U22 exclusion rule now includes the `mixed_with_internal` JSON requirement and hard
gate failure statement. U14 now includes the three-part extraction rule (gap /
wrong-fix / correct-fix) with the explicit fallback rule when Part 2 is absent.
U23 now includes the overlap threshold (0.25–0.50), the quality gate checklist,
and the verification signal requirement.

**Failure modes EM-29 and EM-30 added** covering missing canonicality fields and
external pairs mixed into internal batches.

**Acceptance checklist extended** with canonicality gate checks at the top.

---



**Two new source types:** U14 (gap remediation and fix documents) and U23
(generated code and debug correction histories). Both produce pre-made DPO pairs
from an issue→wrong-fix→correct-fix structure. U14 is the highest-value source
for BUG domain pairs after correction events. U23 extends this to generated-code
repair with implementation-vocabulary pairs.

**Seven zone-mapped families:** U15 (visual UI quality), U16 (flow guide library),
U20 (benchmark/dry-run), U21 (prompt handoff) — each contains zones requiring
different pair construction rules. Treating them as uniform Type 2 produces wrong
pair types. Zone mapping rules documented per family.

**Five excluded families with rules:** U17 (worktree mirrors — canonicalize first),
U18 (certificates/env — sensitive, exclude), U19 (code repo snapshots — unpack and
reclassify), U22 (external training sets — external registry only, never mix silently),
U24 (marketplace legal — not training material).

**pair_kind `gap_remediation` added.** For U14 pairs.

**Failure modes EM-23 through EM-28 added** covering the new families.

**Acceptance checklist extended** with per-family checks for U14–U24.

**Source family decision table extended to 24 rows.**

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

These are the primary batch quotas. The DPO pair authoring guidance (v6) provides
ARCH and PLAN subdivisions that apply *within* these allocations — for example, how
to distribute the 40% Type A quota across ARCH subtypes. When both documents are
in use, apply the primary quotas here first, then apply v6 subdivisions within each quota.

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

## Canonicality Framework

Every source document must be assigned a canonicality class before any pair is
constructed from it. The source family rules govern what pairs a document can
produce. The canonicality class governs whether this particular copy is a valid
source at all. These are independent axes — a document can be the right family
but the wrong copy.

```
canonical          The authoritative live version. Pair construction is permitted
                   subject to family rules.

mirror             An exact or near-exact copy of a canonical document in a
                   different location (.claude/worktrees, extracted subdirectory,
                   old folder, business-flows vs business flows duplicate).
                   Do not use as training source. Use the canonical copy.
                   If the canonical source is missing, promote the mirror to
                   extracted_copy and record the gap before use.

extracted_copy     A file extracted from a ZIP or repository snapshot placed in
                   a working directory for inspection. Valid for pair construction
                   only if the extraction is dated and the canonical archive path
                   is also recorded in the pair file.

stale              A versioned document where a newer version exists, confirmed
                   via the document state register or the document's own
                   "What changed" header. Use only as rejected-vocabulary source
                   (Type 16 pairs). Never as chosen source.

external           Originates outside XIIGen — external research, open-source
                   documentation, external DPO pair batches. Governed by
                   EXTERNAL-SOURCE-REGISTRY, not this document. Must never
                   silently mix into internal teaching batches.

archive_container  A ZIP, repository snapshot, or merged text dump that bundles
                   other documents. Not a training source itself. Must be unpacked
                   and each extracted file classified and canonicality-checked
                   separately.

secure_operational Certificates, private keys, environment credentials, and
                   deployment secrets. Excluded from all training without exception.
                   Do not log paths from these files in any pair manifest.
```

**Required fields in every pair file:**

```json
{
  "source_type": "Type 23",
  "source_canonicality": "canonical",
  "source_path": "GAP-IMPLEMENTATION-PLAN-flow21.md",
  "source_version": "v1",
  "canonical_confirmed_via": "DOCUMENT-STATE-REGISTER-2026-05-06-v8.md"
}
```

**Gate failures from canonicality:**

- `source_canonicality: mirror` → pair rejected at submission gate regardless of quality
- `source_canonicality: stale` → pair rejected unless the pair is a Type 16 (superseded vocabulary) pair
- `source_canonicality: archive_container` → pair rejected; unpack and reclassify first
- `source_canonicality: secure_operational` → pair rejected; do not log path
- `source_canonicality: external` and `mixed_with_internal: true` → hard gate failure; external and internal pairs must be in separate batch files

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

### U14 — Gap remediation and fix documents

**What it is:** Documents that record a confirmed gap, the wrong fix that was
attempted, and the correct fix with evidence. Examples: `GAP-IMPLEMENTATION-PLAN`,
`FIX-PLAN`, `REMEDIATION-PLAN`, `PER-FLOW-FIX-MAP`, `Missing gaps` documents,
and the gap findings section of `GATE-B-FINDINGS.md`.

**Why this is distinct from Types 7 and 8:** Type 7 records implementation status
(a status read). Type 8 records arbiter verdicts on artifacts. U14 records the full
repair arc: gap identification → wrong-fix attempt → correct fix with evidence. No
other type has all three parts in one document.

**Three-part extraction rule — mandatory before building any pair:**

```
Part 1 — gap description:   context for the A-side. NOT the A-side itself.
Part 2 — wrong-fix attempt: what was tried and failed → becomes rejected
Part 3 — correct fix + evidence → becomes chosen
```

If the document has only Part 1 + Part 3 (no wrong-fix attempt recorded): construct
rejected using the ARCH or BUG confusion tables. Do not invent a wrong fix.
Correct fix must have evidence (test result, run output, or verification statement).
A fix without evidence is a claim, not ground truth.

**DPO pair — BUG repair:**

```
A (Type 1 — developer language, NOT gap description text):
  "fix the domain classifier KNN path — it never activates"

Chosen (Part 3 — correct fix with evidence):
  "BuildSeededClassifier() at static construction. Seed with one
   representative predicate profile per domain. _trained=true before
   the first prompt reaches the guard check.
   Evidence: REMEDIATION-PLAN §3 confirms KNN path activates on first call."

Rejected (Part 2 — wrong-fix attempt):
  "Call Train() at the top of ClassifyBaseDomain()."
[WRONG: Train() inside ClassifyBaseDomain() executes after the _trained guard
check — the guard fires first and falls to ClassifyByKeywords before Train()
runs. The fix must run before the guard, not after it. Only static construction
executes before the guard.]
```

**False-pass entries** — the highest-value form. Gate claimed PASS but
implementation was still wrong. The false pass is the rejected; the
correctly-failing verdict is chosen for an ArbiterUnit pair.

```
A (ArbiterReviewRequest):
  artifact_type: "flow_service_file"
  artifact_content: "tenantId = config.GetValue('default_tenant') ?? 'tenant-abc'"
  review_criteria: "portability check P-3: no hardcoded tenant identifiers"
  source_rule: "FLOW-PORTABILITY-TEST-PROTOCOL check P-3"

Chosen (correct detection — reprocess):
  verdict: reprocess
  finding: "config constant 'acme-corp' violates P-3. Config constants that
            resolve to a fixed tenant at startup behave identically to hardcoded
            literals under the portability protocol. PER-FLOW-FIX-MAP records
            this as a confirmed false-pass pattern."

Rejected (what the gate incorrectly produced):
  verdict: pass
  finding: "no hardcoded tenant identifiers found"
[WRONG: the gate scanned for string literals but not config constants that resolve
to tenant identifiers. P-3 covers both. A config constant 'acme-corp' fails the
same portability test as a literal — the flow cannot serve any tenant but acme-corp.]
```

**Annotation rule:** Name the mechanism of failure — which call site, layer, or
detection gap makes the wrong fix fail. Not just "this is wrong".

**Typical overlap:** 0.15–0.30 for BUG/ARCH repair pairs. 0.10–0.20 for
false-pass arbiter pairs.

**Pair kind:** `gap_remediation`

**Excluded use:**
```
Do not use gap description text (Part 1) as A-side.
A-side is always a Type 1 developer-language prompt.
Do not invent a wrong fix if the document does not record one — use confusion tables.
Do not use a correct fix as chosen without evidence attached.
```

---

### U15 — Visual UI quality evidence

**Zone mapping — three distinct zones. Identify before building any pair.**

Documents: VIZ-QA files, UI-STATE-MAP, ROLE-SCREEN-MATRIX, GALLERY-DESIGN-SYSTEM,
UML_COVERAGE_ANALYSIS.

```
Zone A — Visual findings and defect reports
  Markers: "defect", "visual regression", "wrong color", "layout break",
           "accessibility violation", "fails visual check"
  Apply: Type 8 ArbiterUnit rules.
  Chosen: concern or reprocess verdict with specific visual finding.
  Rejected: pass verdict without checking the visual relationship.

Zone B — Role/screen maps and design system references
  Markers: "role", "screen", "layout", "design token", "component spec"
  Apply: Type 2 (ARCH/UI) B-side rules for sub-phases 1.1 and 4.2.
  Use as chosen content for UI ARCH boundary pairs.

Zone C — UML and coverage analysis
  Markers: "UML", "coverage", "class diagram", "sequence diagram",
           "which screens are covered"
  Apply: Type 7 extension rules — coverage as implementation status.
  Use for knowledge-edge detection pairs (what is covered vs not).
```

**Excluded:** Do not use screenshot captions or layout annotations as developer
A-sides — design-tool language is not developer-intent language.

---

### U16 — Flow generator guidance library

**Zone mapping — three distinct zones. Identify before building any pair.**

Documents: GUIDE-B01 through GUIDE-B50, LIST-A-COVERAGE-REPORT, LIBRARY-MASTER-INDEX.

```
Zone A — GUIDE-B procedure steps
  Markers: "Step N", "required before", "gate", "prerequisite"
  Apply: Type 10 PLAN ordering rules.
  Chosen: correct step order with explicit dependency chain.
  Rejected: steps in wrong order or gate skipped.

Zone B — Coverage reports
  Markers: "covered", "missing", "gap", "LIST-A"
  Apply: Type 8 arbiter rules (coverage = verdict on completeness)
         or Type 13 document register rules (which guide is current).

Zone C — Source maps and library index
  Markers: "maps to", "source", "LIBRARY-MASTER-INDEX"
  Provenance only. Not a direct pair source.
  Use for vocabulary calibration and canonical source tracing only.
```

**Excluded:** Do not use guide step text as A-side developer prompts.

---

### U17 — Worktree mirrors and duplicate copies

**Excluded — `mirror` canonicality. Use canonical copy only.**

Locations: `.claude/worktrees/`, `business-flows/` when duplicating `business flows/`,
`auth_extracted/`, `old copies/` subdirectories.

Mirrors must not be used as training sources. If both canonical and mirror exist,
use canonical only. If canonical is missing and only mirror exists, promote mirror
to `extracted_copy` canonicality, record the source gap, then proceed.

Never include both the canonical and a mirror of the same document in the same batch
— this creates contradictory training signal with near-identical token distributions.

---

### U18 — Certificates and environment material

**Excluded — `secure_operational` canonicality. No exception.**

Files: `certificates/*.crt`, `certificates/*.csr`, `environment.zip`, `env*.zip`,
`.env` files.

Do not inspect, read, or extract any content from these files. Do not log these
paths in any pair manifest. Treat as `secure_operational` and stop.

---

### U19 — Code repository snapshots with embedded documents

**Excluded as a unit — `archive_container` canonicality. Unpack and reclassify.**

Examples: `xiigen_v18_complete.zip`, `llm_mvp_core-main.zip`, `orchestrator-mvp-main.zip`.

The snapshot itself has no DPO value. After unpacking, deduplicate against canonical
copies in the document state register, then classify each extracted file:

```
Source code files (.cs, .ts, .py) → vocabulary calibration only; not direct pair source
CLAUDE.md / AGENTS.md             → U11 (agent tooling) — family rules apply
session state docs                → Type 13 (document state register)
DECISIONS-LOCKED.md               → Type 9 (decision documents)
test files with annotations       → check for correction events (Type 3 pattern)
WHAT-TO-DO docs                   → Type 14 (behavioral rules)
Markdown docs                     → classify by content against all family rules
```

Never use the snapshot filename as a document title in an A-side prompt.
Set `source_canonicality: extracted_copy` and record the archive path.

---

### U20 — Benchmark, stress, and dry-run logs

**Zone mapping — three distinct zones. Identify before building any pair.**

Documents: V62_BFA_STRESS_TEST, REFERENCE-RAG-BENCHMARK-PLAN, DRY-RUN files,
EXECUTION-LOG, EXPECTED-TRACE.

```
Zone A — Actual run metrics (measured output)
  Markers: "elapsed_ms", "cpu_percent", "accuracy", "loss", recorded numbers
  Apply: Type 4 measurement evidence rules.
  Use as B-side content for ModelTrainingUnit pairs.

Zone B — Expected traces and reference plans
  Markers: "expected", "should produce", "reference output", "plan"
  Apply: Type 10/14 authority rules.
  Ground truth for what a correct run looks like — use as chosen.
  A run that diverges from expected becomes the rejected.

Zone C — Dry-run and stress reports (planned, not yet measured)
  Markers: "dry run", "stress test plan", "hypothetical", "expected to"
  Apply: B-side authority with explicit pair-file flag:
         "evidence_type": "expected_not_measured"
  Do not use as Type 4 measurement evidence.
  Use only as test plan authority under Type 10 rules.
```

**Excluded:** Do not treat planned metrics as actual metrics. A DRY-RUN document
that says "expected accuracy 0.82" is not a run that measured 0.82.

---

### U21 — Prompt handoff documents and session briefs

**Zone mapping — three distinct zones. Distinct from Type 15.**

Type 15 covers session-start gate prompts. U21 documents have different structure:

Documents: PROMPTS.md, basic_prompt.txt, CLAUDE-CODE-SESSION-PROMPT, HANDOFF-CONTRACT,
SESSION-BRIEF.

```
Zone A — Session start gates (pre-work and post-work conditions)
  Markers: "before any work", "live test count", "manifest required",
           "gate", "must verify"
  Apply: Type 15 rules exactly.

Zone B — Prompt templates (reusable instruction patterns)
  Markers: "template", "insert here", "{{placeholder}}", "use this format"
  Apply: Type 20 (skill library) rules for domain boundary pairs.
  Prompt templates define what a task should look like — rejected is
  a prompt that violates the template's required structure.
  Do not use as A-side input.

Zone C — Handoff contracts and session briefs
  Markers: "handoff to", "next session", "carry forward", "context for"
  Apply: Type 19 (agent tooling instructions) rules for ordering pairs.
  Handoff contracts define what the receiving agent must do and in what
  order — rejected skips a required handoff step.
```

**Excluded:** "Please read the following documents and confirm your understanding"
is Type 6 regardless of which document it appears in.

---

### U22 — External training sets and DPO pair batches

**Excluded from internal registry — `external` canonicality. Never silently mixed.**

Locations: `external-sources/`, `external-sources-training-sets/`,
`external-sources-training-sets 2/`, `dpo-pairs-*/`.

External DPO pair batches must not mix into internal teaching batches. The critical
failure mode: an external batch that contains XIIGen session prompts as A-sides looks
like internal material but was constructed with external B-side authority. Mixed
silently, it produces contradictory signal — the same A-side prompt produces different
chosen responses in different batches.

Required fields for any external pair appearing in any ledger:
```json
{
  "source_canonicality": "external",
  "source_registry": "EXTERNAL-SOURCE-REGISTRY-v8.md",
  "external_source_id": "wave-3-arch-adr",
  "mixed_with_internal": false
}
```

`mixed_with_internal: true` is a hard gate failure. External and internal pairs
must be in separate batch files.

---

### U23 — Generated code and debug correction histories

**What it is:** Debug traces and repair histories from design-to-code or code
generation sessions: `css-debug-history.json`, `generated_code.txt`,
`CORRECTED-DEFECT-CATALOG`, Figma-to-code conversion repair logs.

**Why distinct from Type 3 (correction events):** Type 3 corrections have overlap
< 0.25 because the wrong and correct answers come from different reasoning paths.
Type 24 (U23) corrections often have higher overlap (0.25–0.50) because the wrong
and correct code share most of their structure — only specific lines differ.
The annotation must carry more of the signal.

**DPO pair — BUG repair from debug history:**

```
A (Type 1 — the generation request, NOT the generated code):
  "generate the CSS layout for the tenant dashboard —
   two-column grid with sidebar collapsing below 768px"

Chosen (after debug correction — from css-debug-history.json):
  .tenant-dashboard {
    display: grid;
    grid-template-columns: 240px 1fr;
    gap: 16px;
  }
  @media (max-width: 768px) {
    .tenant-dashboard { grid-template-columns: 1fr; }
    .sidebar { order: 2; }
  }

Rejected (original generated code — before debug):
  .tenant-dashboard { display: flex; flex-direction: row; }
  .sidebar { width: 240px; flex-shrink: 0; }
  @media (max-width: 768px) {
    .tenant-dashboard { flex-direction: column; }
  }
[WRONG: flex collapse does not reorder sidebar — it appears above main content on
mobile. Grid with order: 2 pushes sidebar below content. The debug history records
the flex version as the defect causing visual regression on mobile devices.]
```

**Quality gate — apply before including any U23 pair:**

```
□ Overlap ratio computed — if > 0.45: annotation must introduce ≥ 5 non-overlapping
  tokens describing why the wrong code fails functionally (not just which lines differ)
□ Debug history confirms the wrong code was actually generated (not constructed) —
  rejected must come from a real generation event
□ The correct code has a verification signal in the debug history
  (build passed, test passed, visual confirmed)
□ defect class is a reasoning error, not a typo or missing import
  — Low quality (typo): exclude
  — Medium (missing edge case): include with strong annotation
  — High (reasoning error, wrong operator, wrong layer): include unconditionally
```

**Typical overlap:** 0.25–0.50. Higher than Type 3 because generated code shares
structure. Annotation weight is higher than Type 3.

**Pair kind:** `correction_event`

**Excluded:** Do not use debug trace text (stack traces, console output) as A-side.
A-side is always the original generation request. Do not include pairs where the
"wrong" version is a deliberate placeholder — rejected must be genuinely wrong.

---

### U24 — Marketplace, legal, and store assets

**Excluded — not core training material.**

Files like `STORE_LISTING.md`, `LICENSE`, `NOTICE.md`, `privacy.html`,
`sitemap.xml`, `marketplace.json` are distribution and compliance artifacts.

Exception: if a session document contains a developer task prompt about preparing
marketplace or legal documentation, that prompt is a valid Type 1 A-side for a
DOCUMENT-predicate pair (sub-phase 8.x). The marketplace document itself remains
excluded as direct pair source.

---

### A13 — Diagram and topology source files

**What it is:** Machine-readable diagram files that encode component relationships,
dependency graphs, and architecture topology as structured data.
Examples: `*.mmd`, `*.mermaid`, `*.drawio`, `*.puml`, `DEPENDENCY-GRAPH.mmd`.

**Limited authority — topology and graph evidence only.**

These files are executable topology evidence: a Mermaid diagram of the nine-node
flow encodes the same node and edge relationships as a prose description, but more
precisely and without ambiguity. Use as Type 5 extension for topology vocabulary
and node/edge pair content.

**Valid DPO use — two scopes:**

Scope 1: Topology boundary pairs (Type 5 extension). A diagram that shows
`CompilerUnit → RouterUnit → SchemePlannerUnit` is authoritative for which unit
receives which unit's output. Rejected = wrong dependency direction.

```
A: "which unit receives the compiler's output?"

Chosen (from DEPENDENCY-GRAPH.mmd):
  "MultiSubPhaseRouterUnit receives SubPhaseRouterSourceDocument from
   PromptRoutingInputCompilerUnit. The graph shows a directed edge:
   CompilerUnit → RouterUnit. RouterUnit is the only consumer of the
   compiler's output in the nine-node flow."

Rejected:
  "The compiled prompt is passed directly to SchemePlannerUnit for branch ordering."
[WRONG: DEPENDENCY-GRAPH.mmd shows no direct edge from CompilerUnit to
SchemePlannerUnit. The edge goes CompilerUnit → RouterUnit → SchemePlannerUnit.
SchemePlannerUnit receives translated domain statements, not the compiled prompt.]
```

Scope 2: Architecture review pairs (Type 8 extension). A diagram that shows a
circular dependency is direct evidence for a reprocess verdict.

**Preprocessing required:** Parse as graph — preserve nodes and edges before
flattening to prose. Do not use diagram source text (Mermaid DSL, PlantUML syntax)
as A-side input.

**Excluded:** Do not derive sub-phase Purpose fields from diagrams — use Type 5
(topology seeder code) for that. Diagrams show structure; seeder code defines meaning.

**Pair kind:** `flow_node` (topology pairs), `arbiter` (review pairs)

---

### A14 — Runtime debug and tool logs

**Excluded as direct pair source — preprocessing and normalization required.**

Files: `debug.log`, `firebase-debug.log`, tool execution logs, build/test console
output.

Raw execution logs contain mixed content: timestamps, command invocations,
environment details, error messages. The error messages may be valuable evidence;
the surrounding context is noise.

**Limited use — failure evidence only, after normalization:**

Extract the command that was run, the error output, and the resolution if present.
This is Type 23 extension material (gap/fix evidence) when the log shows a defect
that was subsequently resolved.

```
Normalization protocol:
  1. Identify the failing command line
  2. Extract the error message (not the full stack trace)
  3. Identify the fix applied (if present in subsequent log entries)
  4. Classify as Type 23 (gap remediation) if gap + fix present,
     or Type 4 (measurement evidence) if the log records timing/metrics

Do not include raw log lines as pair content.
Do not use timestamps, environment paths, or process IDs in pairs.
```

**Excluded:** All personally identifiable paths, environment variable values that
may contain credentials, and machine-specific configuration values.

---

### A15 — Test suite source files

**What it is:** Executable test files that encode expected behavior as assertions.
Examples: `*.test.ts`, `*.spec.ts`, `*Tests.cs`, `mock-figma.ts`, `test-settings.ts`.

**Why this is valuable:** A passing test is stronger evidence than a prose
description. The test file shows exactly what inputs produce what outputs, what
edge cases are expected, and what constitutes a pass. This is the most precise
available source for ArbiterUnit pairs — the assertion is the acceptance criterion.

**Two-part structure for extraction:**

Before extracting any pair from a test file:
```
Part 1 — test intent: what behavior is being verified (from describe/it block name)
Part 2 — assertion: what the test actually checks (from expect/assert calls)
```

Separate these two parts. The test intent is the A-side context; the assertion
is the B-side authority. Never use test scaffolding (setup, teardown, mocking
infrastructure) as pair content.

**How to use for ArbiterUnit pairs:**

```
Test file assertion:
  it("returns KNOWLEDGE_EDGE when no flow is registered", () => {
    const result = orchestrator.route(prompt);
    expect(result.OverallVerdict).toBe("KNOWLEDGE_EDGE");
    expect(result.Metrics.knowledge_edge).toBe("True");
    expect(result.Metrics.missing_flow).toBe("True");
  });

A (ArbiterReviewRequest):
  artifact_type: "routing_result"
  artifact_content: { OverallVerdict: "PASS", Metrics: {} }
  review_criteria: "verify knowledge edge detection per test suite contract"
  source_rule: "OrchestratorTests.cs — knowledge edge assertion"

Chosen (ArbiterUnit — reprocess: assertion fails):
  verdict: reprocess
  finding: "OverallVerdict is PASS but test suite requires KNOWLEDGE_EDGE when
            no flow is registered. Metrics.knowledge_edge and missing_flow fields
            are absent. The test assertion is the acceptance criterion — a result
            that does not satisfy it cannot be accepted."

Rejected (ArbiterUnit — pass: ignores test assertion):
  verdict: pass
  finding: "the routing result was produced without errors"
[WRONG: producing a result without errors is not the acceptance criterion.
The test file asserts OverallVerdict = KNOWLEDGE_EDGE with both metric flags.
A pass verdict on a result that fails the stated assertion teaches the arbiter
to ignore the test contract entirely.]
```

**How to use for CODE_Q pairs:**

Test files with descriptive names document expected behavior. A test named
`"returns Probationary trust state after first successful training"` is a
CODE_Q pair source: chosen = what the test confirms, rejected = wrong state.

**Quality gate:**

```
□ Test intent (Part 1) is clear from describe/it block name
□ Assertion (Part 2) is in the actual expect/assert call — not in comments
□ Test is passing in the current codebase (confirmed via document state register
  or live test run) — a failing test is a gap, not ground truth
□ Test file is the canonical current version (three-check rule applies)
□ Scaffolding (beforeEach, afterEach, mock setup) excluded from pair content
```

**Pair kind:** `arbiter` (assertion-based pairs), `flow_node` (behavior pairs)

**Excluded use:**
```
Do not use test file text as A-side developer prompts.
Do not use failing tests as chosen content — failing tests are Type 23 material.
Do not include mock scaffolding in pair content.
```

---

### A16 — Plugin and extension manifest files

**Limited authority — runtime capability boundary only.**

Files: `manifest.json` (browser extensions, Figma plugins), extension manifest
JSON, plugin descriptor files.

These differ from `package.json` (A02): package.json defines build dependencies.
Plugin manifests define runtime permissions and entry points — what the plugin
is allowed to do, which APIs it can call, which UI surfaces it can modify.

**Valid DPO use — one scope only:**

Capability boundary pairs: the manifest defines what the plugin is permitted to
access. A claim that the plugin does something not in its declared permissions is
a constraint violation — the manifest is the authority.

```
A: "can the Figma plugin read clipboard data without user permission?"

Chosen (from manifest permissions):
  "No — the manifest declares 'permissions': ['currentUser', 'activeUsers'].
   Clipboard access is not in the declared permissions list. Any clipboard
   read attempt will be blocked by the plugin runtime."

Rejected:
  "Yes — plugins can read clipboard data using the standard browser Clipboard API."
[WRONG: manifest permissions define what this plugin is allowed to do.
The 'clipboard-read' permission is not declared. Asserting clipboard access
is available contradicts the manifest contract.]
```

**Excluded:** Do not use manifest files as architecture authority for XIIGen internal
decisions. Manifests define plugin boundaries for external platforms, not XIIGen
component boundaries.

---

### A17 — Office, PDF, and exported document files

**Excluded as direct pair source — render and reclassify.**

Files: `*.docx`, `*.pdf`, exported planning documents, rendered specification packets.

Binary office and PDF files may duplicate or supersede markdown sources. They cannot
be read as text without preprocessing.

**Required preprocessing:**

```
1. Extract visible text content (strip formatting, headers, footers)
2. Deduplicate against canonical markdown sources via document state register
3. Classify extracted content by content type (Types 1–26)
4. Set source_canonicality appropriately:
   — If content matches a canonical markdown → use the markdown as canonical
   — If content has no markdown equivalent → extracted_copy canonicality
   — If content is older than the markdown → stale canonicality (rejected vocab only)
```

Do not treat the `.docx` or `.pdf` filename as a document title in any A-side.

---

### A18 — Vendored third-party codebases

**Excluded from internal registry — external boundary applies.**

Files and folders: `claude-useful-skills-master/`, `vibecoder-master/`,
`get-shit-done-main/`, `uiux-lib/`.

These are extracted third-party repositories embedded in the workspace. They are
`external` canonicality under the same rules as U22. They must not become internal
XIIGen authority without an explicit adoption record that names which decision
(A-series) incorporated the external material.

**If a decision record adopts specific content from a vendored codebase:**
Extract only the adopted portion, classify it by content type under the adoption
decision's Type 9 entry, and record `external_source_id` with the vendor name.

**Never:**
```
Do not use vendored codebase source code as B-side authority for XIIGen contracts.
Do not treat presence in the workspace as implied adoption.
A skill file from claude-useful-skills-master is Type 20 material only if an
adoption decision records it — otherwise it is external.
```

---

### A19 — Generated UI code outputs

**What it is:** Generated HTML, React, TSX, and CSS files produced by conversion
experiments — not source-of-truth specifications.
Examples: `*_generated.html`, generated React/TSX files from Figma conversion,
generated CSS from design system exports.

**This is Type 24 source material.** Generated UI code outputs are the starting
point of a debug correction cycle. When a generated file has a documented defect
and a corrected version, the pair follows Type 24 (generated code debug) rules
exactly: A = the generation request, chosen = corrected output, rejected = original
generated output.

**Excluded as B-side authority:** Generated code is not architecture truth. Do not
use generated HTML or CSS as the authoritative specification for a UI component —
use the design spec (U08 Zone B) or the Type 26 source contract instead.

---

### A20 — Before/after visual comparison sets

**Zone mapping — visual QA evidence under U15 Zone A rules.**

Files: `dashboard.before.webp`, `dashboard.after.webp`, `form.before.webp`,
`form.after.webp`.

These are paired visual examples showing an incorrect state and an improved state.
They are the most direct evidence available for visual QA arbiter pairs.

**Required handling:**

The before/after direction must be preserved in the pair file. `before` = the
defective state = rejected verdict content. `after` = the corrected state = pass
or improvement evidence.

```
For ArbiterUnit pairs (U15 Zone A):
  A: ArbiterReviewRequest for the "before" visual state
  Chosen: concern or reprocess verdict citing the specific visual defect
  Rejected: pass verdict without examining the visual relationship

Required metadata per pair:
  { "before_artifact": "dashboard.before.webp",
    "after_artifact": "dashboard.after.webp",
    "defect_description": "sidebar order inverted on mobile",
    "correction_confirmed": true }
```

**Binary files excluded from DPO text.** The `.webp` images themselves are not
pair content — the pair content is the verbal description of the visual defect
and its correction. Use image content as evidence for what the description says,
not as direct pair material.

---

### A21 — Positive/negative example catalogs

**Limited authority — verify labels, provenance, and canonicality before use.**

Registry status: `limited_authority`. High-value when provenance and human authorship
are confirmed. Synthetic catalogs inherit A07 self-training risk.

Files: `POSITIVE-NEGATIVE-EXAMPLES.md`, `CORRECTED-DEFECT-CATALOG`,
example catalogs, failure/success example lists.

**Why this is the highest-priority new family:** These documents have the chosen
and rejected already labeled. No pair construction is required — only extraction,
provenance verification, and quality rating. The expected overlap ratio is
typically < 0.30 because the example catalog was created precisely to show
contrast between good and bad outputs.

**Three-gate quality check before extraction:**

```
Gate 1 — Label verification:
  Confirm that "positive" / "pass" / "correct" examples are actually correct
  against current document state register and Type 9 locked decisions.
  A positive example that was correct when written but is superseded by a
  later decision is now stale — use as Type 16 rejected-vocabulary source only.

Gate 2 — Provenance verification:
  Confirm each example came from a real session or real artifact.
  Synthetic examples (written to fill a coverage gap) must be labeled
  source_canonicality: synthetic and treated with extra scrutiny.
  Synthetic examples have not been tested against the actual system.

Gate 3 — Overlap test:
  Compute overlap_ratio for each pair.
  If > 0.45: annotation must introduce ≥ 5 non-overlapping tokens.
  If > 0.60: exclude.
```

**Extraction pattern:**

```
Catalog entry:
  positive: "CompiledPredicateIds: ['TEST', 'SECURITY'] — correct when the
             prompt mentions 'test coverage' for the 'auth module'"
  negative: "CompiledPredicateIds: ['SECURITY', 'AUTHORIZATION'] — wrong;
             'test coverage' is the primary verb signal, not the object"

Extracted pair:
  A: "test coverage shows 42% on the authentication module"
  Chosen: CompiledPredicateIds: ["TEST", "SECURITY"] — extracted from positive example
  Rejected: CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] — from negative example
  [WRONG: "test coverage" is the primary verb. "authentication" is the object
  being measured. TEST takes priority when the action is coverage measurement.
  Assigning SECURITY and AUTHORIZATION ignores the dominant predicate signal.]
```

**Overlap ratio:** typically < 0.30 — example catalogs are designed to show contrast.

**Pair kind:** depends on content — `flow_node`, `arbiter`, `adaptive_routing_compiler`,
or `correction_event` based on what the examples demonstrate.

**Excluded use:**
```
Never use catalog examples without Gate 1 (label verification).
Stale positive examples must not become chosen content.
Synthetic examples require extra scrutiny and explicit source_canonicality: synthetic.
```

---

### A22 — Algorithm reference and math notes

**Limited authority — CODE_Q only, same rules as Type 21.**

Files: `ml_algorithms_implementations_en.md`, `ML_SIMULATIONS.md`, algorithm
notebooks, math reference notes.

This is external algorithmic reference material. It describes mathematical and
algorithmic concepts but does not define XIIGen decisions.

**Rule:** Apply Type 21 (deep research / external domain reports) rules exactly.
CODE_Q pairs only. A specific algorithm description can be chosen content if:
- A Type 9 locked decision explicitly adopted the algorithm (cite the decision)
- Or the pair is teaching algorithm understanding, not XIIGen architecture

**Never** use algorithm reference material as B-side authority for ARCH or PLAN
decisions. The fact that an algorithm exists in a reference document does not mean
XIIGen uses it or should use it.

---

### A23 — Localized user-facing guides

**Apply A11 rules exactly — mirrors, not independent truth.**

Files: `README-HE.md`, Hebrew guides, translated onboarding docs, localized user
manuals.

The handling is identical to A11 (localization and language variants):
- A-side prompts in the developer's original language are valid Type 1 material
- B-side must use the canonical English source only
- Translated content is `source_canonicality: mirror` — never independent authority

If a localized guide contains content that differs from the canonical English
source, the canonical source wins. The discrepancy is a mirror drift issue, not
a new decision.

---

### A24 — Template and boilerplate libraries

**Limited authority — generation contract evidence after placeholder separation.**

Files: `templates/infra/*`, skill templates, component templates, boilerplate
project folders.

Templates define the expected shape of a generated output — what fields must be
present, what structure is required, what placeholders must be replaced. This is
the accepted contract for what the generated artifact should look like.

**Valid DPO use — structural completeness pairs:**

A template that defines required fields is authority for what a complete artifact
looks like. An artifact that omits a required template field is incomplete — this
is the same logic as Type 11a (missing layer) or Type 22a (missing field).

```
Template defines required fields for a flow manifest:
  {
    "flow_id": "{{FLOW_ID}}",
    "sub_phase": "{{SUB_PHASE}}",
    "tenant_cert_target": "{{TIER}}",
    "portability_status": "{{STATUS}}"
  }

A: "write the manifest for flow 21"

Chosen: complete manifest with all four fields populated
Rejected: manifest with flow_id and sub_phase only — tenant_cert_target and
          portability_status absent
[WRONG: the template defines four required fields. A manifest without
tenant_cert_target and portability_status cannot be validated against the
portability protocol — the protocol checks these fields explicitly.]
```

**Required: separate placeholders from concrete content before any pair.**
A template `{{FLOW_ID}}` is not chosen content — the resolved value `flow-21`
is chosen content. Never include unreplaced placeholder tokens in any pair.

**Excluded:** Boilerplate folder structures and project scaffolding that contain
no field-level contracts have no DPO pair value — they define project layout, not
content correctness.

---



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
| U14 — gap remediation | wrong-fix / false-pass | ✗ | ✓ correct fix | 0.10–0.30 | gap_remediation |
| U15 — visual UI quality | zone-dependent | ✗ | ✓ (per zone) | 0.10–0.30 | arbiter, flow_node |
| U16 — flow guide library | zone-dependent | ✗ | ✓ Zone A/B only | 0.15–0.45 | migration_plan, corruption_case |
| U17 — worktree mirrors | excluded | ✗ | ✗ | — | canonicalize first |
| U18 — certificates / env | excluded | ✗ | ✗ | — | sensitive, exclude |
| U19 — code repo snapshots | excluded as unit | ✗ | ✗ | — | unpack + reclassify |
| U20 — benchmark / dry-run | zone-dependent | ✗ | ✓ Zone A/B only | see zones | teaching_unit, flow_node |
| U21 — prompt handoff | zone-dependent | ✓ Zone C only | ✓ Zone A/B | 0.10–0.30 | tooling_contract, flow_node |
| U22 — external DPO batches | excluded from internal | ✗ | ✗ | — | external registry only |
| U23 — generated code debug | correction_event | ✗ | ✓ correct repair | 0.10–0.30 | correction_event |
| U24 — marketplace / legal | excluded | ✗ | ✗ | — | not training material |
| A13 — diagram / topology files | topology + review | ✗ | ✓ graph structure | Wrong dependency direction | 0.10–0.20 |
| A14 — runtime debug logs | excluded direct; normalize first | ✗ | ✓ failure evidence only | — | gap_remediation |
| A15 — test suite source files | excluded as A-side | ✗ | ✓ assertion as acceptance criterion | Assertion-failing variant | 0.10–0.25 |
| A16 — plugin manifests | capability boundary only | ✗ | ✓ permission boundary | Undeclared permission claim | 0.10–0.20 |
| A17 — office / PDF exports | excluded direct; render first | ✗ | ✗ | — | reclassify after extract |
| A18 — vendored third-party code | excluded (external boundary) | ✗ | ✗ | — | external registry only |
| A19 — generated UI code | Type 24 source material | ✗ | ✗ | — | correction_event |
| A20 — before/after visuals | U15 Zone A extension | ✗ | ✓ visual defect evidence | Pass without checking | 0.15–0.30 |
| A21 — positive/negative catalogs | limited authority; verify labels + provenance | ✗ | ✓ verified positive example | Labeled negative example | 0.10–0.35 |
| A22 — algorithm / math reference | CODE_Q only (Type 21 rules) | ✗ | ✓ CODE_Q algorithm pairs | — | research_fact |
| A23 — localized user guides | A-side prompts in original language | ✓ | ✗ | — | mirror rules |
| A24 — template / boilerplate | structural completeness | ✗ | ✓ required field contract | Missing field variant | 0.15–0.30 |
| A25 — infrastructure as code | full_type_candidate; resource graph | ✗ | ✓ resource/dependency graph | Wrong resource / missing dependency | 0.15–0.30 |
| A26 — DB schema / migration files | full_type_candidate; schema contract | ✗ | ✓ table/constraint/migration order | Wrong type / reversed order | 0.10–0.25 |
| A27 — publish profiles / credentials | excluded; sanitized sample only | ✗ | ✗ | — | secure_operational |
| A28 — coverage binary artifacts | preprocess_then_reclassify | ✗ | ✓ converted metric only | Wrong coverage claim | inherited Type 4 |
| A29 — native runtime binaries | excluded | ✗ | ✗ | — | binary, exclude |
| A30 — git internal metadata | excluded | ✗ | ✗ | — | use git commands instead |
| A31 — web server / hosting configs | limited; routing + rewrite only | ✗ | ✓ route/rewrite rule | Wrong route order | 0.15–0.30 |
| A32 — lock / cache / partial downloads | excluded; lock version only | ✗ | ✗ | — | lock → inherit A02 |
| A33 — IDE project metadata | limited; tooling setup only | ✗ | ✓ tooling rule | Wrong setting | 0.10–0.25 |
| A34 — browser export sidecars | excluded | ✗ | ✗ | — | belongs to parent HTML |
| A35 — non-ZIP compressed archives | preprocess_then_reclassify | ✗ | ✗ | — | extract + reclassify |
| A36 — repo governance files | limited; ownership + process only | ✗ | ✓ owner/hook/package rule | Wrong owner/gate | 0.10–0.25 |

---

### A25 — Infrastructure as code files

**Registry status: full_type_candidate.** Terraform, HCL, and infrastructure
topology files that declare cloud, container, or runtime resources.
Examples: `*.tf`, `*.hcl`, `terraform-main.tf`.

**Authority boundary:** IaC is authoritative for declared resources and
dependencies, not for product rationale. A resource that exists in the IaC file
is declared; a resource that is missing is absent by design.

**Valid DPO use — resource graph and unsafe-default pairs:**

```
A: "which services must be deployed before the API gateway starts?"

Chosen (from terraform-main.tf resource graph):
  "The database module and the identity service must both be deployed first.
   The API gateway declares depends_on = [module.database, module.identity].
   These two dependencies are explicit in the IaC file — startup before
   them will fail at health check."

Rejected:
  "The API gateway can start in any order; services register themselves
   after startup."
[WRONG: the IaC file declares explicit depends_on relationships for the
gateway resource. Ignoring declared dependencies violates the resource graph
and produces startup failures that the IaC prevents by design.]
```

**Security gate — mandatory before any A25 pair:**

```
□ No provider credentials in file (API keys, access tokens, private endpoints)
□ No terraform.tfstate or .tfvars files with real variable values
□ If state file is present: treat as secure_operational, exclude
□ IaC alternatives (multiple resource options) not used as chosen unless
  a Type 9 decision explicitly selects that configuration
```

**Typical overlap:** 0.15–0.30 (resource vocabulary vs deployment-order prose).
**Pair kind:** `flow_node` (deployment ordering), `arbiter` (unsafe-default pairs).

---

### A26 — Database schema and migration files

**Registry status: full_type_candidate.** Database schema definitions, migration
SQL, and seed data files that define storage contracts.
Examples: `*.sql`, `supabase/migrations/*.sql`, `initial_schema.sql`.

**Authority boundary:** Migration order and schema definitions are storage-contract
authority — stronger than prose for exact column types, constraints, and nullability.
Not general product architecture.

**Valid DPO use — schema constraint and migration order pairs:**

```
A: "what constraint prevents a cart item from being added without a tenant key?"

Chosen (from schema.sql):
  "The cart_items table declares tenant_id VARCHAR(64) NOT NULL with a foreign
   key to tenants(id). A row without tenant_id fails the NOT NULL constraint
   before it reaches application logic. The schema enforces tenant isolation
   at the persistence layer regardless of application-level validation."

Rejected:
  "Tenant isolation in cart items is enforced by the service layer which
   checks for a tenant context before inserting."
[WRONG: the schema defines NOT NULL on tenant_id at the database level —
enforcement is structural, not application-level. Application checks are
additional; the schema constraint is the floor. A row without tenant_id
cannot exist in the database.]
```

**Migration order pairs:**

```
A: "can the add_tenant_constraints migration run before the create_tenants migration?"

Chosen: "No — the foreign key in add_tenant_constraints references tenants(id).
         The tenants table must exist before the constraint can be added.
         Reversing migration order will fail at the ALTER TABLE step."

Rejected: "Migration order is configurable — they can run in parallel."
[WRONG: a foreign key constraint cannot reference a table that does not yet exist.
The create_tenants migration is a structural prerequisite of add_tenant_constraints.]
```

**Excluded use:**
```
Do not train on real production data dumps or demo seed rows with customer data.
Do not mix schema facts with runtime application state.
Do not use tenant-specific seed data as general product behavior truth.
```

**Typical overlap:** 0.10–0.25.
**Pair kind:** `flow_node` (migration ordering), `arbiter` (constraint conformance).

---

### A27 — Publish profiles and credential-adjacent files

**Excluded — `secure_operational` canonicality. No DPO use without sanitization.**

Files: `*.PublishSettings`, `.env` (with real values), publish profiles, deploy
credential files.

Real credentials, endpoints, tokens, and private keys cannot appear in any pair
content. Even sanitized sample files (`.env.sample`) require screening before use.

**Narrow exception — sanitized sample files only:**

If the file uses placeholder values only (`DATABASE_URL=<your-url>`,
`API_KEY=replace-this`), it follows A12 deployment template rules for
required-variable completeness pairs. Verify that no real value exists anywhere
in the file before use.

---

### A28 — Coverage binary and test result artifacts

**Preprocess then reclassify — binary content not DPO text.**

Files: `.coverage`, coverage cache files, binary test-result outputs.

These require the owning tool (e.g., `dotnet-coverage`, `coverage.py`) to convert
to a human-readable summary before classification. After conversion, the summary
follows Type 4 (calibration run data) rules for measurement evidence.

**Required:**
```
□ Convert with owning tool to text summary
□ Record tool name and version in pair metadata
□ Use converted summary as Type 4 measurement evidence only
□ Do not cite coverage without the converted report
```

---

### A29 — Native runtime binary dependencies

**Excluded — binary identity only. No text DPO.**

Files: `*.dll`, `*.so`, `*.dylib`, `*.exe`, `*.a`.

These are compiled artifacts. They carry no tokenizable text content usable in
DPO pairs. Binary presence can support provenance metadata only.

---

### A30 — Git repository internal metadata

**Excluded — use git commands, not raw `.git` internals.**

Files: `.git/config`, `.git/logs/*`, `.git/objects/*`, `.git/index`, `COMMIT_EDITMSG`.

Raw git object storage and index files are not training sources. When commit history
is needed as evidence (Type 12 extension), use normalized `git log` or `git show`
output against a canonical repository — not the internal object files.

---

### A31 — Web server and hosting config files

**Limited authority — deployment routing only. Not application API contract.**

Files: `web.config`, `.htaccess`, `*.conf`, IIS / Apache / Nginx config files.

These define host-level routing, rewrite rules, static file serving, and server
runtime parameters. Authority boundary: routing and rewrite rules are authoritative
for what paths the server handles. Application API contracts come from Type 26
(source code contracts) or Type 2 (architecture documents).

**Valid DPO use — routing order and rewrite rule pairs:**

```
A: "will a request to /api/search be rewritten before reaching the application?"

Chosen (from web.config RewriteRule):
  "Yes — web.config declares a RewriteRule that matches ^/api/(.*)$ and
   rewrites to /backend/api/$1 before the request reaches the application router.
   The application sees /backend/api/search, not /api/search."

Rejected:
  "No — /api/ routes go directly to the application."
[WRONG: the RewriteRule in web.config executes at the server layer before
application routing. The application router never sees the original /api/ path.]
```

**Security gate:** Screen for embedded credentials, private server names, and
environment-specific internal IPs before use.

**Pair kind:** `flow_node` (routing order).

---

### A32 — Lock, cache, and partial download artifacts

**Excluded direct — lock files handled as A02 dependency evidence.**

Files: `*.lock`, `*.cache`, `*.download`, `eslint cache`.

Cache and partial-download files contain generated noise — no DPO pair value.
Lock files (`package-lock.json`, `pnpm-lock.yaml`) record pinned dependency versions
and are handled as A02 (dependency and build manifests) for version-pinning pairs.
Do not use lock files to infer architecture decisions.

---

### A33 — IDE project metadata files

**Limited authority — tooling setup only. Not product behavior.**

Files: `.project`, `.buildpath`, `.prefs`, `.editorconfig`.

These define local developer tooling configuration: formatting rules, build paths,
editor preferences. They can produce tooling-setup pairs but must not override
AGENTS.md or Type 14 behavioral rules.

**Valid narrow scope:** `.editorconfig` defines formatting rules that a tool-compliance
pair can reference. `.buildpath` defines source roots that a process-order pair can
reference. Nothing in this family is product behavior authority.

---

### A34 — Browser export resource sidecars

**Excluded — belongs to parent HTML capture (A09).**

Files: `Attached text prompt - Claude_files/*`, hashed extensionless resource blobs,
HTML export sidecar folders.

Sidecar resources are fragments of the parent browser capture. They have no
independent identity and must not be classified separately. Process the parent
HTML (A09 rules) if the capture is needed; the sidecars follow automatically.

---

### A35 — Non-ZIP compressed archives

**Preprocess then reclassify — same rules as U10/U19 for ZIP files.**

Files: `*.tar.gz`, `*.gz`, `benchmark_complete.tar.gz`.

These are `archive_container` canonicality. Extract, deduplicate against canonical
copies in the document state register, classify each extracted file by content type
(Types 1–28), then apply the corresponding family rules. Do not use the archive
filename as a document title in any A-side prompt.

---

### A36 — Code owner and repository governance files

**Limited authority — process and ownership evidence only.**

Files: `CODEOWNERS`, `.npmignore`, pre-commit hooks, repository ownership files.

These define who owns which paths in the repository, what files are excluded from
npm packages, and what local hooks run before commits. Authority boundary: process
and ownership evidence only — not product architecture.

**Valid DPO use — ownership routing and packaging exclusion pairs:**

```
A: "which team owns changes to the authentication module?"

Chosen (from CODEOWNERS):
  "The security-team owns /src/auth/* per CODEOWNERS.
   Any PR modifying authentication files automatically requires a review
   from a member of @security-team before merge."

Rejected:
  "Any developer can merge authentication changes without additional review."
[WRONG: CODEOWNERS declares security-team as a required reviewer for /src/auth/*.
The repository's branch protection enforces this — PRs without security-team
approval are blocked from merge.]
```

**Pair kind:** `flow_node` (process ordering), `arbiter` (policy compliance).

---



The extended scan identified twelve additional artifact families (serialized delivery
parts, skills factory registries, naming/mapping tables, validation reports, conflict
analysis, DoD documents, prompt pattern libraries, DNA patterns, README indexes,
and miscellaneous state files) that are not yet explicitly covered. These will be
added when registry v11 is available. Minimum expected entries:

| Family | Expected pair type | Expected pair_kind |
|---|---|---|
| B03 — skills factory registry | knowledge-edge detection | `knowledge_edge_detection` |
| B05 — naming / mapping tables | naming translation pairs | `naming_translation` |
| B06 — validation reports | arbiter verdict pairs | `arbiter` |
| B07 — conflict analysis | ARCH Type D trade-off | `flow_node` |

Until registry v11 is published: B-series documents encountered during extraction
should be classified provisionally under the closest U-series or A-series family,
with a `pending_b_series: true` field in the pair file to enable retroactive
reclassification when the rules are published.

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
source_canonicality, canonical_confirmed_via,
transition_type, phase, sub_phase, task_type, node_role,
coverage_status, pair_kind, rejected_type (A / B / C),
compiled_prompt_in_training (true / false)
```

`compiled_prompt_in_training: false` causes the pair to be rejected at the
measurement gate regardless of accuracy.

`source_canonicality` must be one of: `canonical`, `extracted_copy`.
Any other value is a gate failure (see Canonicality Framework).

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
gap_remediation         ← U14 — gap remediation and fix documents
topology_graph          ← A13 — diagram / topology source files
example_catalog         ← A21 — positive/negative example catalogs
naming_translation      ← naming/mapping tables such as ROSETTA_STONE and Figma-to-CSS
                          mapping matrices. chosen = current name/term, rejected = old or
                          wrong name. Lowest overlap class in the corpus: 0.05–0.15.
design_simulation       ← E01/F09 design simulation and graph-extension artifacts
session_round_contract  ← D01-D04/E03/F16 per-session round/load contracts
patch_addendum          ← D06/E16/F05 patch, delta, and replan artifacts
state_reconciliation    ← F08 reconciliation state documents
package_verification    ← F15/G06-G11 package and archive verification artifacts
requirements_traceability ← F12/G05 validation matrix rows
skill_recovery          ← F01 recovered missing skill files
external_structural_pattern ← Z50-Z56 external structural sources only
runtime_evidence        ← G01-G04 runtime output and handoff artifacts
archive_manifest        ← G06-G11 archive bundle manifests
named_session_artifact  ← H01-H08 named session document families
data_catalog_evolution  ← I01 versioned task-type catalogs
rule_stress_test        ← I02/I03 CF and ST rule families
merged_canonical_document ← I04 merged canonical document family
session_type_taxonomy   ← I05/I13/I14 session classification outputs
role_execution_plan     ← I08 architect behavior execution plans
visual_proof_plan       ← I10 visual proof plans
run_instruction_input   ← K01 master run instruction files
active_experiment_result ← J05 active experiment result JSON
teaching_cycle_package  ← J09 teaching cycle package structure
bash_view_path_source   ← K01-K13 bash/view-discovered file families
drawio_topology_graph   ← K07 DrawIO XML topology graphs
component_debug_history ← K08 component-scoped CSS debug histories
repository_reference_authority ← L01-L08 repository reference families
training_run_telemetry ← M01 batch-level training telemetry
dpo_checkpoint_metric ← M02 DPO checkpoints
training_pair_yaml ← M05 machine-readable training pair YAML
holdout_corruption_eval ← M06 corruption holdouts
holdout_rephrase_eval ← M07 rephrase holdouts
teaching_ledger_audit ← M08 per-unit teaching ledger
flow_examination_result ← M13 flow examination result pairs
arbiter_analysis_pass ← M15 multi-pass arbiter analysis
flow_live_run_evidence ← M17 live run result plus execution log
goal_gap_report ← M18 goal-element gap reports
flow_registry_reference ← N01 flow registries
engine_contract_template ← N02 engine contract templates
canonical_merge_protocol ← N03 merge protocols
visual_screenshot_evidence ← O13 screenshot evidence
ux_review_finding ← O14 per-PNG UX findings
typescript_service_interface ← P01 TypeScript service interfaces
machine_constant_rule ← P02 named runtime constants
round_gap_matrix ← P03 round output gap matrices
round_root_cause_map ← P04 round output root cause maps
skill_gap_remediation ← Q01 architectural gap to skill remediation
planning_startup_skill ← Q02 planning-session startup skill
architectural_decision_testing ← Q03 architecture decision testing skill
claude_code_integration_instruction ← Q04 Claude Code integration instructions
verification_query_section ← Q05 executable verification query sections
session_process_template ← R01-R13 docs.zip session process families
per_task_why_teaching ← R09 SESSION-TEACH-TNN reasoning documents
cross_flow_edge_spec ← R10 cross-flow edge specification tests
llm_subproject_registry ← S01 llm_mvp_core subproject maps
llm_experiment_plan ← S04/W01-W06 experiment plan family
llm_capability_plan ← S06/W16 capability execution plans
live_portability_evidence ← T07 phase-N evidence JSON
nestjs_canonical_source ← V01-V08 xiigen-COMPLETE canonical families
experiment_runtime_state ← W07-W09 session/schema/runtime state
gate_d_capability_format ← W16 Gate D plan completeness format
```

`correction_event` is a new kind for pairs extracted from Luba correction events
in XIIGen session documents. It is also used for U23 generated-code debug correction
histories and A19 generated UI code outputs — the extraction pattern is identical,
the domain differs.

**Cross-document navigation:** This document covers layer structure, required fields,
source family rules, and batch acceptance gates. For worked examples, confusion tables,
annotation rules, and pair type guidance (Types A–K), use the DPO pair authoring
guidance alongside this document. The navigation table at the start of that document
maps pair types to sections in both documents.

---

## Additional Registry DTO Examination Gate

The companion registry addendum covers `A01-A51`, `B13-B43`, `C01-C12`,
`Z41-Z56`, `D01-D19`, `E01-E16`, `F01-F17`, `G01-G13`, `H01-H08`,
`I01-I14`, `J01-J09`, `K01-K13`, `L01-L08`, `M01-M21`, `N01-N09`,
`O01-O19`, `P01-P12`, `Q01-Q05`, `R01-R13`, `S01-S14`, `T01-T10`,
`U01-U20`, `V01-V08`, `W01-W16`, `X01-X16`, `Y01-Y10`, `AA01-AA17`,
`AB01-AB07`, `AC01-AC20`, `AD01-AD08`, `AE01-AE10`, `AF01-AF13`,
and `AG01-AG06`. These
families are accepted only when the examiner records the DTO shape that makes the
classification true. Filename-only classification is a failure.

Required examination DTO for every additional-family source:

```json
{
  "source_family": "G05",
  "source_document_family": "VALIDATION-MATRIX",
  "source_path": "...",
  "source_locator": {
    "section_id": "...",
    "line_start": null,
    "json_pointer": null,
    "archive_member_path": null
  },
  "source_canonicality": "canonical",
  "canonical_confirmed_via": "...",
  "target_registry_type": "Type 29 Zone D",
  "source_role": "b_side_chosen",
  "pair_kind": "requirements_traceability",
  "family_required_fields_complete": true
}
```

Family-specific DTO gates:

| Family group | Required fields | Reject when |
|---|---|---|
| `D01-D04`, `E01-E06`, `F11-F17` | `session_id`, `flow_id`, `phase_id`, `round_id`, `artifact_id`, `gate_or_verdict_id` | several rounds are merged into one row |
| `D06`, `E16`, `F05` | `target_document_id`, `base_version`, `delta_id`, `before_state`, `after_state`, `supersedes` | delta has no base document |
| `D09-D10`, `E07`, `E13`, `F04`, `F07-F08` | `state_scope`, `state_status`, `flow_id_or_phase_id`, `schema_or_version` | in-progress/snapshot state is used as current completion |
| `E02`, `E05`, `F02`, `F09`, `G12-G13` | `flow_id`, `decision_id` or `contract_file_path`, `node_or_edge_id`, `field_names` | field/edge/decision is applied to the wrong flow |
| `Z41-Z49`, `G06-G11` | `archive_path`, `archive_kind`, `content_manifest_path`, `extraction_path`, `archive_member_path` | archive filename is used as authority |
| `Z50-Z56` | `source_canonicality=external`, `structural_pattern_only=true`, `external_source_family`, `archive_member_path` | external content becomes XIIGen-specific B-side |
| `F01` | `skill_id`, `skill_source_path`, `recovered_skill_version`, `activation_rule_id`, `skill_boundary_quote` | skill behavior inferred from session mention |
| `G01-G05` | `runtime_source_id` or `matrix_row_id`, `flow_id`, `phase_id`, `evidence_locator` | DPO metric or plan prose is used as runtime evidence |
| `H01-H08` | `named_artifact_kind`, artifact-specific id, `target_type`, `scope_or_version`, `evidence_locator` | broad Type 29/32 label is used without the named DTO shape |
| `I01-I14` | `message_source_kind`, `session_type`, `thinking_or_tool_locator`, artifact-specific ids, version/rule ids | thinking/tool-call output is used without locator or concrete family |
| `J01-J09` | `project_id`, `archive_path`, `archive_member_path`, experiment/training DTO fields, `cycle_id` when applicable | active project artifact treated as stale archive or raw dump |
| `K01-K13` | exact loaded/opened path, `runtime_use`, family-specific ids, parsed graph/debug fields when applicable | path mention used as authority without DTO |
| `L01-L08` | repository-era canonicality, target tool/stack/pattern id, prompt/template/config-specific fields | historical repo file used without era/tool/pattern DTO |
| `M01-M21` | parsed JSON/YAML/JSONL fields, run/cycle/flow ids, verdict/metric/evidence locators, eval/train split | machine-readable training/examination artifact flattened into prose |
| `N01-N09` | tool_result/view/create_file locator, reference family id, protocol/template/interface/registry-specific fields | reference family inferred from path only |
| `O01-O19` | flow id, step/phase/tenant/role/screenshot locators, paired MD/JSON or PNG/review links | live docs artifact treated as archive or screenshot filename used as evidence |
| `P01-P12` | session type, matrix/tool_result origin, interface/constant/round-output fields | statistical count used as source authority without concrete artifact |
| `Q01-Q05` | parent document id, H1/header locator, recovered skill/remediation/query fields | H1 text or skill filename used as complete source |
| `R01-R13` | ZIP member path, flow id, round/session id, template-vs-instance role, gate/action ids | template instance confused with canonical template or session prompt used as output |
| `S01-S14` | `project_id=llm_mvp_core`, subproject/experiment/capability ids, project adoption status for skills | llm_mvp_core artifact used as XIIGen authority without adoption |
| `T01-T10` | version, flow id, machine-readable evidence fields, latest-version status | stale governance/report version used as current |
| `U01-U20` | exact skill id and skill-specific DTO fields | broad skill-file label used without skill-specific rule fields |
| `V01-V08` | `archive_id=xiigen-COMPLETE.zip`, `canonical_nestjs=true`, module/page/test-specific fields | Python/FLOW33 pattern used as chosen when NestJS canonical conflicts |
| `W01-W16` | experiment id, subtype, mandatory plan/schema/result/progress fields | broad experiment-plan label used without subtype DTO |
| `X01-X16` | `project_id=genie_skill_library`, archive/member path, stack/tool/template/recipe/pattern fields | Genie stack/template/source used as XIIGen authority or collapsed to Type 20 |
| `Y01-Y10` | module-architecture or phases_done archive id, composition/translation state fields | matrix, module reference, translated parts, state, and final declaration collapsed |
| `AA01-AA17` | xiigen_v18_complete archive id plus law/module/matrix/manifest/site-config/flow/skill fields | V18 composition layer collapsed into generic skill file label |
| `AB01-AB07` | canonical V18 migration/architect/schema/config/manifest anchor and role fields | stale migration plan or schema/config/manifest role swap |
| `AC01-AC20` | compliance requirement, flow extension, renumbering, validation, example polarity, or source-code mapping fields | broad flow/state/test/skill-index label without AC DTO |
| `AD01-AD08` | web-session handoff, per-flow metrics, SDK audit, artifact range, validation matrix, or consolidation pair fields | analysis doc as execution, fabricated metrics, draft IDs, or false all-pass |
| `AE01-AE10` | flow-boundary snapshot, append target, citation type, V37 subphase, approach-library, or decision-log fields | snapshot ZIP as authority or append/research/RAG families swapped |
| `AF01-AF13` | supreme skill load-order, governance version, skeleton state, transcript zone, goal anchor, simulation/TEACH-QA round, HTML verdict extraction, or confirmation target fields | raw transcript/HTML used whole, simulation/TEACH roles swapped, or covered family duplicated |
| `AG01-AG06` | standalone ID map, applied merge offset, raw intent/response boundary, citation type, PromptOps schema/loop, or early engine extension fields | draft IDs used despite map, plan used as completion, or raw intent separated from response |

Examination order for these families:

1. Identify exact family ID.
2. Confirm canonicality.
3. Fill the family-specific DTO fields.
4. Only then select pair kind and chosen/rejected text.
5. If any family-specific field is missing, record `family_dto_missing` and stop.

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

**EM-23 — Gap Remediation Pair Uses Tracking Label as A-side.** The A-side for a
U14 gap remediation pair contains a gap tracking label (`"GAP-007: E18 KNN
unreachable"`) rather than a developer-intent prompt. Correction: convert to
developer-language before use — `"fix the E18 KNN path — DomainClassifierUnit._trained
is always false"`. Gap IDs and tracking codes are B-side provenance, not A-side input.

**EM-24 — Mirror Document Used Without Canonicality Check.** A pair was built from
a document found in a worktree mirror path (U17 — `.claude/worktrees`, duplicate
folder) without verifying it is the canonical version. Correction: check the document
state register before using any document. If the register marks this path as a mirror
or extracted copy, use the canonical source instead. Pairs from non-canonical copies
may contradict canonical decisions silently.

**EM-25 — External Training Batch Mixed Into Internal Registry.** Pairs from an
external source batch (U22 — `external-sources-training-sets`, wave folders) were
registered as internal pairs without the `source_family: external` label. External
source pairs carry external authority — a Wave 1 security pair is authoritative for
protocol facts but not for XIIGen architecture decisions. Correction: all pairs from
U22 must remain in the external registry under EXTERNAL-SOURCE-REGISTRY rules. Never
re-register as internal without explicit authority transfer.

**EM-26 — Generated Code Used as A-side.** A pair from U23 (generated code debug
history) uses the generated code itself as the A-side prompt. The generated code is
the artifact, not the task that requested it. Correction: A-side must be the task
description or design spec that produced the generation request — in developer-language.

**EM-27 — Dry-Run Expected Trace Treated as Actual Measurement.** A pair from U20
(benchmark/dry-run) uses a `DRY-RUN` or `EXPECTED-TRACE` document as if it were
a real run result. Correction: mark pairs from Zone B (expected traces, plan documents)
with `coverage_status: expected_not_measured`. Do not use expected metrics as chosen
content for ModelTrainingUnit pairs — use only actual measured values from Zone A.

**EM-28 — Prompt Template Used as Developer A-side.** A U21 prompt template
(Zone B — `PROMPTS.md`, `CLAUDE-CODE-SESSION-PROMPT`) was used directly as a
developer-intent A-side. Prompt templates are vocabulary authority for compiler
pairs, not developer language. Correction: use Zone C (agent execution prompts
written in developer-task framing) as A-sides, subject to Type 1 eligibility.
Zone B prompt templates go to B-side vocabulary for Layer 0-A compiler pairs only.

**EM-29 — Canonicality Not Recorded.** A pair file is missing
`source_canonicality` or `canonical_confirmed_via` fields. Without canonicality
metadata the submission gate cannot verify the source is valid. Correction: add
both fields to every pair file. Valid values for `source_canonicality` at
submission: `canonical` or `extracted_copy`. All other values are gate failures.

**EM-30 — External Pairs Mixed Into Internal Batch.** A batch file contains pairs
with `source_canonicality: external` alongside internal pairs. Correction: external
and internal pairs must be in separate batch files. Set `mixed_with_internal: false`
in all external pair files. Any batch where `mixed_with_internal: true` is a hard
gate failure that requires the batch to be split before resubmission.

**EM-31 — Test File Failing Test Used as Chosen Content.** A pair from A15 (test
suite source files) uses a test that is currently failing in the codebase as the
acceptance criterion for chosen content. Correction: a failing test is a gap (Type 23
material), not ground truth. Only passing tests are valid A15 chosen sources.
Verify against live test run or document state register before use.

**EM-32 — Example Catalog Label Not Verified.** A pair from A21 (positive/negative
example catalogs) uses a "positive" example that was correct when written but is
now superseded by a later locked decision. Correction: Gate 1 (label verification)
is mandatory — check positive examples against current Type 9 decisions and document
state register. Stale positive examples are Type 16 rejected-vocabulary sources only.

**EM-33 — Diagram Source Text Used as A-Side.** A13 Mermaid or PlantUML diagram
source syntax was included in the A-side prompt. Correction: diagram source files
are topology evidence for B-sides. The A-side must be a developer-language question
("which unit receives the compiler's output?"). The diagram DSL is the B-side
authority, not the input.

**EM-34 — Generated Code Output Used as Architecture Authority.** An A19 generated
HTML or CSS file was used as the B-side specification for a UI component boundary
pair. Correction: generated code is not architecture truth — it is a Type 24 source
(before-debug artifact). Use the design spec (U08 Zone B) or Type 26 source contract
for architecture authority.

**EM-35 — Template Placeholder Included in Pair Content.** A pair from A24 (template
libraries) includes an unreplaced placeholder token (`{{FLOW_ID}}`, `{{TIER}}`) in
chosen or rejected content. Correction: resolve all placeholders before extraction.
Placeholder tokens produce nonsense tokens in the compiled training text.

**EM-36 — Vendored Codebase Used as Internal Authority Without Adoption Record.**
A pair uses content from an A18 vendored third-party codebase (e.g.,
`claude-useful-skills-master`) as B-side authority for a XIIGen ARCH or PLAN pair
without citing a Type 9 locked decision that explicitly adopted the content.
Correction: vendored codebases are external canonicality. An adoption decision is
mandatory before any vendored content becomes internal B-side authority.

**EM-37 — Naming/Mapping Table Pair Uses Wrong pair_kind.** A pair from a naming
table, ROSETTA_STONE, or mapping matrix (the source for `naming_translation` pairs)
was submitted with `pair_kind: migration_plan` or `correction_event`. These are
different pair classes: naming_translation pairs have the lowest overlap in the corpus
(0.05–0.15) because old and new names are entirely non-overlapping tokens. Incorrect
pair_kind makes batch diagnostics misleading — abnormally low loss from these pairs
will be attributed to the wrong class. Correction: use `pair_kind: naming_translation`
for all pairs where chosen = current name/term and rejected = deprecated name/term.

**EM-38 — B-Series Document Used Without Provisional Classification Flag.** A document
from a B-series family (B01–B12: serialized parts, skills factory registry, naming tables,
validation reports, conflict analysis, DoD documents, prompt pattern libraries, DNA
patterns) was extracted into a pair without the `pending_b_series: true` flag. Until
registry v11 is published with explicit B-series rules, pairs from these families must
carry this flag to enable retroactive reclassification. Correction: add
`pending_b_series: true` to the pair file and classify provisionally under the closest
U-series or A-series family (B05 naming tables → provisional `naming_translation`;
B06 validation reports → provisional `arbiter`; B07 conflict analysis → provisional
`flow_node`; B03 skills factory → provisional `knowledge_edge_detection`).

**EM-39 — IaC File Used With Credentials or State File Present.** A pair from an A25
IaC file (Terraform, HCL) was submitted without applying the security gate. Credentials,
API tokens, or provider secrets were present in the file and may have entered the pair
content. Correction: treat any IaC file containing real credential values as
`secure_operational`. Apply the A25 security gate before any pair construction. If the
security gate was skipped, the batch must be reviewed and pairs containing credential-adjacent
tokens withdrawn before submission.

**EM-40 — DB Migration Order Reversed Without Schema Dependency Check.** A pair from an
A26 database migration file claims that two migrations can run in either order when one
declares a foreign key or constraint that depends on the other's table. Correction: foreign
key constraints cannot reference tables that do not yet exist. The migration that creates
the referenced table is a structural prerequisite. Verify the dependency chain from the
actual SQL `CREATE TABLE` and `ALTER TABLE` statements before writing the pair.

**EM-41 — Browser Sidecar Classified as Independent Document.** A file from an A34
sidecar folder (`Attached text prompt - Claude_files/*`, hashed extensionless blob) was
classified as an independent pair source. Correction: sidecar resources belong to the
parent HTML capture and have no independent identity. Process the parent A09 HTML file;
the sidecars are not separate training sources under any circumstances.

**EM-42 — Additional Family Classified By Filename Only.** A row from A01-A51,
B13-B43, C01-C12, Z41-Z56, D01-D19, E01-E16, F01-F17, G01-G13, or H01-H08 lacks
the family-specific DTO fields. Correction: stop extraction, fill source_locator
and required family fields, then rebuild the row.

**EM-43 — External Structural Source Used As XIIGen Authority.** A Z50-Z56 row uses
third-party content as XIIGen-specific B-side authority. Correction: set
`source_canonicality=external`, `structural_pattern_only=true`, or exclude.

**EM-44 — Missing Skill Used Before Recovery.** An F01 missing-skill row infers
behavior from a session mention or skill name. Correction: recover the SKILL.md
content and cite activation rule plus skill boundary before authoring.

**EM-45 — Archive Bundle Used Without Member Classification.** A Z41-Z49 or G06-G11
archive is treated as a document. Correction: extract, verify manifest, classify
each member, and use member-level source paths.

**EM-46 — Session Analysis Artifact Merges Rounds.** A D/E/F session artifact row
combines several rounds or phases. Correction: split by `session_id`, `flow_id`,
`phase_id`, and `round_id`.

**EM-47 — Runtime Evidence Confused With DPO Metrics.** A G01-G04 runtime artifact
is measured as if it were DPO training output, or DPO metrics are used as product
runtime evidence. Correction: record runtime source id, command id, flow/phase id.

**EM-48 — Validation Matrix Row Missing Covered-By Evidence.** A G05 validation
matrix row claims PASS without a `Covered By` artifact. Correction: use
`requirements_traceability` with `requirement_id`, `covered_by_artifact`, and
coverage status; otherwise rejected is PASS-without-evidence.

**EM-49 — Named Session Document Collapsed Into Broad Type.** An H01-H08 artifact is
classified as generic Type 29/32/34 without named artifact kind, scope, or version.
Correction: record `named_artifact_kind`, target type, and artifact-specific ids.

**EM-50 — Session Message Output Used Without Locator.** An I01-I14 source was found
inside thinking blocks, tool calls, or assistant-generated structured output, but
the row records only a filename-like label. Correction: record `message_source_kind`,
`thinking_or_tool_locator`, and concrete artifact family before extraction.

**EM-51 — CF And ST Rule Families Mixed.** A BFA CF rule and a stress-test ST rule
are treated as the same family. Correction: record `rule_family=CF` for I02 and
`rule_family=ST` for I03; link ST rows to their CF rule where applicable.

**EM-52 — Merged Canonical Document Used Without Merge Provenance.** An I04 MERGED
document is used as canonical without `source_versions` or continuity check.
Correction: record merge sources, merge phase, and `continuity_check_passed=true`.

**EM-53 — Active Experiment Result Missing Measurement DTO.** A J05 experiment run
result row omits baseline/enhanced/improvement/next_action fields. Correction:
rebuild from the JSON result, not from a prose summary.

**EM-54 — Teaching Cycle Package Flattened Into Text.** A J09 teaching cycle package
is treated as plain documentation. Correction: preserve cycle id, layer id, ledger
row, held-out kind, and checkpoint path.

**EM-55 — Bash/View Path Mention Used As Authority.** A K01-K13 artifact was only
seen in a bash command or view path, but the row lacks exact path and runtime use.
Correction: record the path, loaded/opened context, and family-specific DTO fields.

**EM-56 — DrawIO Diagram Not Parsed As Graph.** A K07 DrawIO XML file is treated as
visual reference only. Correction: parse nodes and edges, then use
`drawio_topology_graph`.

**EM-57 — Component CSS Debug History Missing Component Scope.** A K08 row omits
component id or corrected/wrong revision ids. Correction: record component-scoped
debug DTO before creating Type 24 pair.

**EM-58 - Repository Reference Used Without Era Or Tool Scope.** An L01-L08 source
is used without repository-era canonicality or target tool/stack/pattern identity.
Correction: record repository context and family-specific DTO before extraction.

**EM-59 - Machine-Readable Artifact Flattened.** An M/T/W JSON, YAML, or JSONL source
is paraphrased from filename or prose instead of parsed into its DTO. Correction:
parse the structured fields directly and preserve run/cycle/flow/experiment ids.

**EM-60 - Tool-Result Reference Inferred From Path Only.** An N01-N09 source is
classified from a path or H1 mention without returned content. Correction: cite the
tool_result/view/create_file locator and extract reference fields.

**EM-61 - Visual Evidence Missing Link.** An O13/O14 visual row uses a PNG filename
or UX finding without linking screenshot, tenant/role context, and finding row.
Correction: record both screenshot evidence and UX review row.

**EM-62 - Session Statistics Used As Evidence.** A P01-P12 family is justified by
frequency counts alone. Correction: locate the concrete artifact path or tool_result
source and extract family-specific fields.

**EM-63 - Tool-Result Header Treated As Full Source.** A Q01-Q05 source is created
from H1/header text only. Correction: record parent document id, H1/header locator,
and full recovered section fields.

**EM-64 - Session Process Template And Instance Swapped.** An R01-R13 source treats a
flow-specific instantiated session as the canonical process template, or vice versa.
Correction: record `template_id`, `instantiated_from_template`, and placeholder values.

**EM-65 - llm_mvp_core Source Used As XIIGen Authority.** An S/U skill or plan is used
as XIIGen authority without adoption status. Correction: record project id and
adoption boundary, or keep it llm_mvp_core-scoped.

**EM-66 - NestJS Canonicality Anchor Ignored.** A V01-V08 conflict uses Python/FLOW33
patterns as chosen after `xiigen-COMPLETE.zip` exists. Correction: use V-family
NestJS canonical source and mark older snapshot stale.

**EM-67 - Experiment Family Collapsed.** A W01-W16 source is labeled only
"experiment plan" and misses subtype fields. Correction: classify as single plan,
batch plan, index, infra plan, strategy, promotion, schema, runtime state, or Gate D
format before extraction.

**EM-68 - Genie Or V18 Composition Source Collapsed.** An X/Y/AA/AB source is
labeled only "skill", "architecture", or "config". Correction: record the exact
project/archive and family role first: stack alternative, recipe, law, matrix,
module reference, translated part, schema, site config instance, deployment
manifest, architect package, or reference protocol.

**EM-69 - Compliance Or Flow Extension Evidence Flattened.** An AC/AD source is
labeled only "flow doc", "state", "test", or "skill index". Correction: record
the AC/AD role first: compliance requirement, append-to patch, renumber map,
validation matrix, positive/negative example, web-session governance skill,
per-flow completion state, SDK audit, or consolidation before/after record.

**EM-70 - Canonical Extension Or V37 Navigation Used As Source.** An AE source is
labeled only "merged files", "research", or "RAG index". Correction: distinguish
snapshot provenance, per-canonical append content, cited deep research,
engine-specific research, Q&A chat log, V37 adaptive plan, sub-phase state,
approach-library index, and session decision log before extraction.

**EM-71 - Flow Planning Or Artifact Verdict Collapsed.** An AF source is labeled
only "planning", "QA", "simulation", or "HTML". Correction: distinguish supreme
skill load-order, examination governance, skeleton fix plan, goal anchor,
planning simulation, deep design simulation, TEACH-QA-R0, TEACH-QA-R1 correction,
simulation patch, and extracted verdict HTML before pair construction.

**EM-72 - Draft Flow IDs Or Intent Boundaries Lost.** An AG source uses pre-
renumber draft IDs, merge plans as completion evidence, or an AI design response
without its raw user-intent boundary. Correction: prefer standalone renumber maps
and merge completion reports, and preserve raw-intent and response locators.

---

## Acceptance Checklist

- [ ] Every row names a contract signature.
- [ ] Every B side is an actual runtime output object.
- [ ] CANONICALITY GATE — check this first; if failed, stop and do not apply any other check.
  source_canonicality field present in every pair file: canonical or extracted_copy only.
  mirror / stale / archive_container / secure_operational → STOP. Exclude entirely.
    Record rejection reason as "canonicality: {class}" — not "failed overlap" or other.
    This ensures accurate diagnosis: a mirror source and a high-overlap pair are different problems.
  external → STOP. Must be in a separate external-only batch file; mixed_with_internal: false required.
- [ ] canonical_confirmed_via field present — names the document state register or
  "What changed" header that confirms this is the current version.
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
- [ ] For U14 (gap remediation): A-side is developer-language task prompt — not a gap
  tracking label. Wrong-fix entries rated High/Medium/Low before use (same as Type 3).
  False-pass entries used as ArbiterUnit pairs — chosen = correctly-failing output.
- [ ] For U15 (visual UI quality): zone identified before pair construction.
  Zone A (visual findings) → Type 8 ArbiterUnit rules.
  Zone B (role/screen maps) → Type 5 topology rules for UI sub-phases.
  Zone C (design-system analysis) → Type 2 ARCH rules for 1.1 sub-phase.
- [ ] For U16 (flow guide library): zone identified before pair construction.
  Zone A (guide steps) → Type 10 PLAN Type B rules.
  Zone C (index files) → provenance only, not a pair source.
- [ ] For U17 (worktree mirrors): canonicality verified against document state register
  before use. Pairs from non-canonical copies excluded.
- [ ] For U18 (certificates/env): excluded entirely — not inspected or extracted.
- [ ] For U19 (repo snapshots): unpacked and reclassified by content type before use.
  Snapshot filename not used as document title in any A-side.
- [ ] For U20 (benchmark/dry-run): zone identified.
  Zone A (actual metrics) → Type 4 rules.
  Zone B (expected traces) → marked expected_not_measured, not actual measurement.
  Zone C (dry-run discrepancies) → discrepancy pair with expected vs actual.
- [ ] For U21 (prompt handoff): zone identified.
  Zone A (handoff contracts) → Type 19 agent tooling rules.
  Zone B (prompt templates) → B-side vocabulary for Layer 0-A only, not A-sides.
  Zone C (agent execution prompts) → Type 1 eligibility test applied first.
- [ ] For U22 (external training sets): not mixed into internal registry.
  source_family: external label present. External registry authority applies.
- [ ] For U23 (generated code debug): A-side is task description or design spec —
  not the generated code artifact itself. Correction entries rated High/Medium/Low.
- [ ] For U24 (marketplace/legal): excluded unless session contains a DOCUMENT-predicate
  task prompt about preparing marketplace material (then A-side only).
- [ ] For A13 (diagram/topology files): pair uses node/edge vocabulary from parsed graph —
  not raw DSL syntax. A-side is developer-language question. Three-check rule confirms current version.
- [ ] For A14 (debug logs): normalization applied — command, error, resolution extracted.
  Raw timestamps, paths, and process IDs excluded from pair content.
- [ ] For A15 (test suite files): test confirmed passing in current codebase (not failing).
  Scaffolding excluded. Three-check rule confirms current version.
- [ ] For A16 (plugin manifests): B-side scope is capability boundary pairs only.
  No core XIIGen ARCH or PLAN authority from manifest content.
- [ ] For A17 (office/PDF): text extracted and deduplicated against canonical markdown.
  File not processed as raw binary.
- [ ] For A18 (vendored codebases): source_canonicality = external. No internal B-side
  authority without a Type 9 adoption decision record.
- [ ] For A19 (generated UI outputs): treated as Type 24 source material.
  A-side is the generation request — not the generated file.
- [ ] For A20 (before/after visuals): before/after direction preserved in pair metadata.
  Pair content is verbal defect description — binary images not in DPO text.
- [ ] For A21 (positive/negative catalogs): all three gates applied —
  Gate 1: label verified against current Type 9 decisions and document state register
  Gate 2: provenance checked — synthetic examples flagged
  Gate 3: overlap test computed; stale positive examples excluded as chosen.
- [ ] For A22 (algorithm/math reference): CODE_Q only. Type 21 rules applied exactly.
  No ARCH or PLAN authority claimed. XIIGen adoption decision cited if applicable.
- [ ] For A23 (localized guides): A-side prompts valid in original language.
  B-side uses canonical English source only. source_canonicality = mirror.
- [ ] For A24 (templates): all placeholders resolved before extraction.
  Required fields verified against template — missing-field logic as Type 11a/22a.
- [ ] For B-series families (B01–B12 — pending registry v11): if encountered, classify
  provisionally under closest U/A-series family and add `pending_b_series: true` to pair file.
  B05 naming tables → `naming_translation`; B06 validation reports → `arbiter`;
  B07 conflict analysis → `flow_node`; B03 skills factory → `knowledge_edge_detection`.
- [ ] For A25 (IaC files): security gate passed — no credentials, state files, or .tfvars with real values.
  IaC alternatives not used as chosen without Type 9 decision selecting that configuration.
- [ ] For A26 (DB schema/migrations): no production data or customer seed rows in pair content.
  Migration order in chosen derived from actual dependency chain, not assumed.
- [ ] For A27 (publish profiles): treated as secure_operational unless confirmed to be
  a sanitized sample with placeholder values only. Real credentials → excluded entirely.
- [ ] For A28 (coverage binaries): conversion to text summary completed by owning tool.
  Tool name and version recorded. Inherits Type 4 rules after conversion.
- [ ] For A29 (native binaries): excluded entirely — not inspected as text.
- [ ] For A30 (git internals): excluded. Git history evidence uses normalized `git log`
  or `git show` output, not raw `.git` object files.
- [ ] For A31 (web server configs): scope confirmed as routing/rewrite pairs only.
  Application API contracts come from Type 26 or Type 2, not web.config.
  Secret screening applied — no embedded credentials or private server names.
- [ ] For A32 (locks/caches): cache and download files excluded. Lock files handled
  as A02 dependency evidence for version-pinning pairs only.
- [ ] For A33 (IDE metadata): scope confirmed as tooling setup pairs only.
  AGENTS.md and Type 14 rules take precedence over IDE preferences.
- [ ] For A34 (browser sidecars): excluded as standalone documents.
  Processed only as part of parent A09 HTML capture.
- [ ] For A35 (non-ZIP archives): archive_container canonicality. Extracted, deduplicated,
  then each file classified by content type. Archive filename not used as document title.
- [ ] For A36 (repo governance): scope confirmed as ownership routing, packaging exclusion,
  or pre-commit process pairs only. Not product architecture authority.
- [ ] For additional registry families A01-A51 / B13-B43 / C01-C12 / Z41-Z56 /
  D01-D19 / E01-E16 / F01-F17 / G01-G13 / H01-H08: family-specific DTO fields
  are complete before pair construction.
- [ ] For Z50-Z56: source_canonicality = external and structural_pattern_only = true.
  No XIIGen-specific B-side authority from nested external archives.
- [ ] For F01: recovered SKILL.md exists and activation rule / skill boundary are cited.
- [ ] For G06-G11: archive manifest verified and every member has its own target family.
- [ ] For G05: validation matrix row includes requirement_id, covered_by_artifact,
  and coverage_status.
- [ ] For H01-H08: named_artifact_kind, scope/version, target_type, and evidence_locator
  recorded; no broad Type 29/32/34 label without subtype.
- [ ] For I01-I14: message_source_kind and thinking_or_tool_locator recorded when
  the source came from thinking/tool-call/message content.
- [ ] For I02/I03: CF and ST rule families kept separate; rule_id and version recorded.
- [ ] For I04: source_versions and continuity_check_passed recorded before canonical use.
- [ ] For I13/I14: session type label routes artifacts only; produced artifacts are
  classified to concrete families.
- [ ] For J05: experiment result JSON fields baseline/enhanced/improvement_pct/passed/
  next_action present.
- [ ] For J09: teaching cycle package keeps manifest, layer, ledger, held-out, and
  checkpoint DTO fields.
- [ ] For K01-K13: exact bash/view path and runtime use recorded before pair construction.
- [ ] For K07: DrawIO XML parsed into nodes/edges, not treated as a screenshot.
- [ ] For K08: component_id, wrong revision, corrected revision, and verification signal present.
- [ ] For L01-L08: repository-era canonicality and target tool/stack/pattern identity recorded.
- [ ] For M01-M21: JSON/YAML/JSONL DTO fields parsed directly; training/eval/measurement roles are not mixed.
- [ ] For N01-N09: tool_result/view/create_file locator recorded before using a reference/protocol as authority.
- [ ] For O01-O19: flow id plus step/phase/tenant/role/screenshot locators recorded where applicable.
- [ ] For O13/O14: screenshot evidence is linked to UX review finding row.
- [ ] For P01-P12: concrete artifact path or tool_result locator present; count alone is not evidence.
- [ ] For Q01-Q05: parent document id and H1/header locator recorded; full skill/remediation/query content recovered.
- [ ] For R01-R13: ZIP member path, flow id, round/session id, and template-vs-instance role recorded.
- [ ] For S01-S14: project_id=llm_mvp_core recorded; project-specific skills require adoption before XIIGen use.
- [ ] For T01-T10: latest version wins for versioned governance/evidence docs; machine-readable fields parsed.
- [ ] For U01-U20: exact skill id and skill-specific DTO fields recorded; broad skill-file label rejected.
- [ ] For V01-V08: archive_id=xiigen-COMPLETE.zip and canonical_nestjs=true recorded when used.
- [ ] For W01-W16: experiment subtype and mandatory fields recorded; broad experiment-plan label rejected.
- [ ] For X01-X16: project_id=genie_skill_library plus stack/tool/template/recipe fields recorded.
- [ ] For Y01-Y10: module-architecture/phases_done archive and composition or translation DTO fields recorded.
- [ ] For AA01-AA17: xiigen_v18_complete V18 law/module/matrix/manifest/config/flow/skill role recorded.
- [ ] For AB01-AB07: canonical V18 migration/architect/schema/config/manifest anchor and role recorded.
- [ ] For AC01-AC20: compliance/flow-extension/plugin-factory DTO fields recorded before using evidence.
- [ ] For AD01-AD08: web-session handoff, state, renumbering, validation, or consolidation role recorded.
- [ ] For AE01-AE10: canonical extension, research, V37 state, or RAG decision/index role recorded.
- [ ] For AF01-AF13: planning/examination/simulation/TEACH/HTML verdict role recorded; transcripts and HTML are zone-extracted.
- [ ] For AG01-AG06: renumber map, applied offset, raw intent/response boundary, or early engine-extension role recorded; draft IDs rejected.

---

## Last Updated

2026-05-08. Version 14.0.
