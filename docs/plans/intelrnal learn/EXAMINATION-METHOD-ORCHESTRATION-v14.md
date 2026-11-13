# Document Examination to Training Sets â€” Orchestration Edition
# Version: 14.0 â€” 2026-05-08
# Supersedes: EXAMINATION-METHOD-ORCHESTRATION-v13.md

---
## 2026-05-11 Routing-Flow Supersession Notice

For routing acceptance, this v14 method is superseded by
`EXAMINATION-METHOD-ORCHESTRATION-v15.md`.

Layer 0-A and Layer 0-B unit rows remain useful only as diagnostic pair
ingredients. They do not prove that routing is learned. Accepted routing must
target the full flow/orchestrator surface:

```csharp
OrchestrationRoutingFlow : ILogicalFlow<LogicalOrchestratorRequest>
```

or equivalent `AdaptiveLogicalOrchestrator` behavior.

If a batch contains only compiler/router unit rows, record:

```text
PAIRSET_READY_ROUTING_FLOW_PENDING
```

not `ACCEPT_BATCH`.

---
## 2026-05-11 Internal Run-Experience Fix

Operational runbook:
`C:\Users\lubao\Documents\xiigen\llm mvp\external sources learning 2\documentation\TRAINING-SET-BUILD-AND-TESTING-RUNBOOK-v1.md`

This guide must be read with the current run evidence in:
`C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1\RUN-STATUS.json`

Current protected baseline: RUN-416 / V246, 672 rows, weighted accuracy 0.7068,
weighted reward margin 0.016892, domains orchestration 257 / plans 341 / skills 74.
Future additions must protect orchestration, plans, and skills first.

Hard fixes from the live run:

1. Do not use deterministic filename/path code to decide semantic authority.
   Scripts may inventory, validate, split, hash, and train. They may not decide
   that a user request, `basic_prompt.txt`, session-start prompt, genesis prompt,
   or flow initiation document is low-authority without model-led registry
   adjudication.
2. Root prompt and user-intent documents are potential prompt authority. They
   belong in model-led authority adjudication, not generic review-required, when
   the script cannot type them.
3. Train-ready pairs must satisfy: `chosen != rejected`, rejected contains a
   specific `[WRONG: ...]`, no mojibake in prompt/a_side/chosen/rejected, source
   span recorded, canonicality/version state explicit.
4. Ledger, status, audit, inventory, and progress rows are evidence surfaces.
   They may justify pairs but must not be treated as DPO rows themselves.
5. New or repaired sets must run as 16-64 row standalone ablations before combo
   promotion. Promote only if reward margin is positive and weighted accuracy
   does not damage the protected baseline.
6. External code-documentation rows must not be mixed directly into internal
   orchestration/plans/skills training; they require their own code-doc gates.
7. If a document is a new type vs a new version, decide by structure, consumer
   action, A-side question, and lifecycle/canonicality. Do not decide by name
   alone.

Script references updated for this rule:

- `prepare_internal_learning_sets.py` now writes `model-led-authority-candidates.jsonl`
  for root prompt/user-intent candidates and `training-pairs-repair-required.jsonl`
  for rows blocked by hard DPO gates.
- `augment_internal_learning_sets_tier3.py` now routes B162/B165 prompt-authority
  candidates to model-led authority adjudication instead of treating them as
  ordinary low-confidence review rows.

---

## What changed from v13

**2026-05-08 additional registry DTO gate appended** for A01-A51, B13-B43,
C01-C12, Z41-Z56, D01-D19, E01-E16, F01-F17, G01-G13, H01-H08, I01-I14,
J01-J09, K01-K13, L01-L08, M01-M21, N01-N09, O01-O19, P01-P12, Q01-Q05,
R01-R13, S01-S14, T01-T10, U01-U20, V01-V08, W01-W16, X01-X16,
Y01-Y10, AA01-AA17, AB01-AB07, AC01-AC20, AD01-AD08, AE01-AE10,
AF01-AF13, and AG01-AG06 from the
additional registry companion. This keeps v14 usable for sessions that still
reference it while preserving the same DTO quality gates as v15.

**Review findings applied.** Nine findings from the cross-document review:

**B-series families placeholder added** to source family decision table and
acceptance checklist. B01â€“B12 (serialized delivery parts, skills factory registries,
naming tables, validation reports, conflict analysis, DoD documents, prompt pattern
libraries, DNA patterns) are pending registry v11. Provisional classification
mapping provided: B05 â†’ `naming_translation`, B06 â†’ `arbiter`, B07 â†’ `flow_node`,
B03 â†’ `knowledge_edge_detection`. Field `pending_b_series: true` marks pairs for
retroactive reclassification when v11 is published.

**Batch composition cross-reference note added** to primary quota table: v6/v7 DPO
pair guidance subdivisions apply *within* these primary allocations.

**Canonicality gate upgraded to explicit stop rule** in acceptance checklist: now
reads "STOP HERE IF FAILED, do not proceed to any other check" with instruction to
record rejection as "canonicality: {class}" for accurate batch diagnostics.

**`naming_translation` pair_kind added** to allowed list. For B05 naming tables,
ROSETTA_STONE, and mapping matrices. Lowest overlap class (0.05â€“0.15). EM-37 now
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

**U14â€“U24 zone markers and exclusion rules upgraded to v7 precision.** Zone A/B/C
markers for U15/U16/U20/U21 now include the exact text markers from registry v7.
U22 exclusion rule now includes the `mixed_with_internal` JSON requirement and hard
gate failure statement. U14 now includes the three-part extraction rule (gap /
wrong-fix / correct-fix) with the explicit fallback rule when Part 2 is absent.
U23 now includes the overlap threshold (0.25â€“0.50), the quality gate checklist,
and the verification signal requirement.

**Failure modes EM-29 and EM-30 added** covering missing canonicality fields and
external pairs mixed into internal batches.

**Acceptance checklist extended** with canonicality gate checks at the top.

---



**Two new source types:** U14 (gap remediation and fix documents) and U23
(generated code and debug correction histories). Both produce pre-made DPO pairs
from an issueâ†’wrong-fixâ†’correct-fix structure. U14 is the highest-value source
for BUG domain pairs after correction events. U23 extends this to generated-code
repair with implementation-vocabulary pairs.

**Seven zone-mapped families:** U15 (visual UI quality), U16 (flow guide library),
U20 (benchmark/dry-run), U21 (prompt handoff) â€” each contains zones requiring
different pair construction rules. Treating them as uniform Type 2 produces wrong
pair types. Zone mapping rules documented per family.

**Five excluded families with rules:** U17 (worktree mirrors â€” canonicalize first),
U18 (certificates/env â€” sensitive, exclude), U19 (code repo snapshots â€” unpack and
reclassify), U22 (external training sets â€” external registry only, never mix silently),
U24 (marketplace legal â€” not training material).

**pair_kind `gap_remediation` added.** For U14 pairs.

**Failure modes EM-23 through EM-28 added** covering the new families.

**Acceptance checklist extended** with per-family checks for U14â€“U24.

**Source family decision table extended to 24 rows.**

---

## What changed from v8

**DPO accuracy diagnosis added from calibration run data.** Run 2026-05-06
produced 40 batches. 29 of 40 had negative reward margin â€” the model assigned
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
Layer 0 is not required â€” but if a scenario has been used in any adaptive routing
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

## DPO Accuracy â€” How Reward Margin Works

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
to chosen. The optimizer step runs but pushes in the wrong direction â€” it makes
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

All three cases require that rejected has real content â€” not a negation of chosen.

### The two-token-overlap test

Before adding a pair to a batch, apply this test:

Count the tokens shared between `chosen` and `rejected` that are not also in the
`prompt`. If more than 60% of non-prompt tokens are shared between chosen and
rejected, the pair will likely produce negative reward margin.

The negation template always fails this test because it shares every meaningful
token with chosen except "Do not".

A Type A rejected passes this test because it discusses a different sub-phase's
responsibility using that sub-phase's vocabulary â€” architecture vocabulary for a
3.1 rejected versus implementation vocabulary for a 4.1 chosen.

---

## DPO Pair Quality Rules â€” Universal

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

**Type A â€” Plausible adjacent sub-phase.**

The rejected response addresses a different sub-phase that is semantically close
to the correct one. It contains real technical content and a coherent argument
but addresses the wrong responsibility.

Token distribution: Type A rejected uses the vocabulary of the wrong sub-phase
(e.g., class names and method signatures for a 4.1 rejected) rather than the
vocabulary of the correct sub-phase (boundary rules and ownership for a 3.1
chosen). The distributions are different enough for the model to detect.

**Type B â€” Correct sub-phases, wrong dependency order.**

The rejected response names the same sub-phases as the chosen response but
sequences them in the wrong order or omits a dependency constraint.

Token distribution: Type B rejected contains the same sub-phase names as chosen,
so the distinguishing signal is the `depends_on` field content. Chosen has
`depends_on=["branch_1"]`; rejected has `depends_on=[]`. This is a smaller
distribution difference than Type A, so the [WRONG: ...] annotation is especially
important â€” it provides additional token context that helps the model learn the
dependency direction.

**Type C â€” Wrong domain for the prompt's verb.**

The rejected response addresses the wrong domain based on a misreading of the
prompt's intent signal. The intent verb determines the domain:

```
"explain / how does X work / what is"  â†’ CODE_Q
"X throws / X fails / error in / bug"  â†’ BUG
"implement / add / build / create"     â†’ PLAN or IMPLEMENT
"design / define boundaries"           â†’ ARCH
"review / audit / verify"              â†’ QA or ARCH review
"plan / sequence / order of"           â†’ PLAN (SchemePlannerUnit)
```

Token distribution: Type C rejected uses the vocabulary of the wrong domain
(e.g., explanation vocabulary for CODE_Q versus planning vocabulary for PLAN).
This produces strong distributional differences and typically achieves positive
reward margin.

### Required annotation

Every rejected response must end with a [WRONG: ...] block naming the specific
confusion. The annotation serves two purposes: it documents the confusion for
the pair author, and it adds tokens to the rejected response that describe why
the answer is wrong â€” giving the model additional signal beyond the content alone.

```
[WRONG: "define boundaries" asks who owns what and where dependencies flow,
not what the class body contains. Writing the class body is implementation.
Implementation decisions follow boundary decisions, they do not replace them.]
```

A generic annotation ("this is incorrect", "wrong approach") is not acceptable.
The annotation must name the specific confusion â€” which sub-phase the rejected
incorrectly addresses and why that sub-phase does not own this responsibility.

### Annotation quality examples

**Weak annotation â€” does not name the confusion:**

```
[WRONG: This is the wrong sub-phase for this prompt.]
```

**Strong annotation â€” names the specific confusion:**

```
[WRONG: "design the observability strategy" asks what signals to measure and
what thresholds to alert on â€” not how to configure a Grafana dashboard.
Configuring the dashboard (7.3) follows from the strategy (3.5).
The strategy must exist before the configuration can be meaningful.]
```

### One prompt per pair per batch

A prompt must not appear as chosen in one pair and as rejected in another within
the same batch. When one prompt maps to multiple sub-phases, each sub-phase gets
a separate pair â€” each pair has a different chosen (the correct response for that
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

Where `compiledText` is the normalized prompt with predicate tokens appended â€”
not the raw user input. This is the gate from PLAN-COMPILED-PROMPT-IN-TRAINING-v1.md.

### Batch composition minimums

These are the primary batch quotas. The DPO pair authoring guidance (v6) provides
ARCH and PLAN subdivisions that apply *within* these allocations â€” for example, how
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

For 3.1 â†’ 4.1 confusion:

```
[WRONG: "{prompt verb}" asks who owns what and where dependencies flow â€”
not what the class body contains. The class is the implementation of the boundary
decision. The boundary decision must be made first.]
```

For 3.5 â†’ 7.3 confusion:

```
[WRONG: "design the observability strategy" asks what signals to capture and what
thresholds matter â€” not how to configure monitoring tooling. 7.3 Monitoring sets
up the infrastructure to collect what 3.5 defines. 3.5 must come first.]
```

For 6.1 â†’ 5.x confusion:

```
[WRONG: acceptance review requires human judgment on the release as a whole.
Automated tests verify specific paths but cannot make the release judgment call.
6.1 owns the human checklist; 5.x owns the automated suite.]
```

---

## PLAN Dependency Ordering Reference Table

Use this table to construct Type B rejected responses. The wrong order must be
plausible â€” someone testing-first would reasonably put 5.2 before 3.4.

| Correct sequence | Natural wrong order for rejected | Why the wrong order fails |
|---|---|---|
| 3.1 Architecture â†’ 4.1 Backend Services | 4.1 first, 3.1 as afterthought | Implementing without boundaries creates coupling |
| 3.2 Data Model â†’ 3.3 API Contract | 3.3 first, model derived from contract | Contract shape depends on model shape, not the reverse |
| 3.4 Security â†’ 5.2 Integration Tests | 5.2 first with mock tokens | Mock tokens cannot test real tenant isolation |
| 3.1 + 3.4 parallel â†’ 4.1 after both | All three in parallel | 4.1 cannot implement what 3.1 has not decided |
| 2.2 Acceptance Criteria â†’ 5.1 Unit Tests | 5.1 first, criteria inferred | Tests define what passes; criteria define what should pass |
| 3.5 Observability Design â†’ 7.3 Monitoring | 7.3 first, design retroactively | Cannot configure what has not been defined |
| 3.3 API Contract â†’ 4.5 Integration Connectors | 4.5 first, contract from implementation | Implementation detail leaks into public contract |

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
  Good: "implement Phase 1 â€” Factory Interfaces and Engine P..."
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
  Claude's wrong answer  â†’ becomes rejected
                           (add [WRONG: {the correction itself}] as annotation)
  Luba's correction      â†’ becomes chosen
  The original prompt    â†’ becomes A side

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

1. The wrong answer was genuinely produced â€” it is not a plausible wrong answer
   written by a human trying to imagine what wrong looks like. It is an actual
   wrong answer that a capable system produced.

2. The correction is the ground truth â€” not an approximation or inference.

3. The prompt is real developer language â€” not a synthetic prompt written to
   match a category.

4. Reward margin for correction-derived pairs is almost always positive because
   the chosen (correction) and rejected (wrong answer) come from different
   reasoning paths and have genuinely different token distributions.

### Correction event inventory

For each session document, record:

```
session_id
correction_count       â€” number of Luba corrections found
sub_phases_covered     â€” which sub-phases the corrections address
pair_quality_estimate  â€” High / Medium / Low based on:
  High: chosen and rejected have clearly different vocabulary
  Medium: chosen and rejected address different responsibilities
  Low: correction is only phrasing, not substance
```

Only High and Medium quality corrections enter the training ledger.

---

## Layer 0 â€” Adaptive Orchestrator Infrastructure Pairs

### Layer 0-A â€” Prompt Routing Input Compiler

```
PromptRoutingInputCompilerUnit
    : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
```

**B field names** (use these exactly â€” no invented fields):

```
DocumentId             â€” "{run_id}-prompt"
SourcePath             â€” "logical-orchestrator://user-prompt"
SourceTier             â€” "runtime_prompt"
LicenseOrAuthority     â€” "user_request"
DocumentType           â€” "user_prompt"
DocumentTitle          â€” "Runtime user prompt"
CompiledText           â€” user_request string trimmed
CompiledPredicateIds   â€” matched predicate IDs from the lexicon
SourceStatus           â€” "ready" when predicates found; "knowledge_edge: ..." when none
KnownFailureMode       â€” "runtime_prompt_compiled"
```

**Predicate lexicon (code-backed as of 2026-05-11):**

```
SECURITY      - jwt, authentication, auth, oauth, oidc, token, session, asvs, mfa, rbac, security, authorization
AUTHORIZATION - authorization, authorisation, permission, rbac, scope, tenant, ownership
API_CONTRACT  - api, endpoint, contract, schema, request, response, openapi
ARCHITECTURE  - architecture, architect, component topology, system boundary, dependency direction, project, codebase, code base, existing code, old code, current code
IMPLEMENT     - "implement the", "implementation plan", connector, provider, "integration layer", "code change", code, fix, bug, issue, async, null, generated frame, error, failure, failed, no data, returned no data, log
TEST          - test, testing, "integration test", "unit test", coverage, fixture, verify, validation, automation test, fix, bug, issue, regression, error, failure, failed, no data, returned no data, log, try again
QA            - qa, quality, release, acceptance, verification
UI            - ui, ux, screen, layout, design, interaction, accessibility
DOCUMENT      - document, documentation, spec, specification, requirements
AUTOMATION    - automation, automate, automated, ci, pipeline, quality gate, quality gates, build gate, build gates, monitoring feedback, coverage reporting, reporting dashboard, repeatable validation
LEARNING      - skill, skills, claude code, learning, teach, training, reusable guidance, knowledge, model feedback
```

Current compiler emits eleven predicates. Older text that says nine predicates is stale: it omits LEARNING and the 2026-05-11 AUTOMATION fix. Sub-phase `3.6` is not registered in current topology; any rows targeting `3.6` must be held or remapped before training.
Confidence: 0.92 when at least one predicate matches. 0.70 when none match.

**Example â€” multi-predicate prompt:**

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

**Example â€” no-predicate prompt:**

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

**DPO pair â€” Type A rejected:**

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

### Layer 0-B â€” Multi-Sub-Phase Router

```
MultiSubPhaseRouterUnit
    : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRoutingDecision[]>
```

The A for every router row comes directly from the B of the corresponding
compiler row. Do not invent router A shapes.

**B field names per decision** (use these exactly):

```
DocumentId                â€” carried from the source document
SourceStatus              â€” "predicted" or "knowledge_edge"
PredictedPhaseId          â€” phase identifier e.g. "3"
PredictedSubPhaseId       â€” sub-phase identifier e.g. "3.4"
Confidence                â€” routing confidence 0.0 to 1.0
NearestWrongSubPhaseScore â€” score of closest non-selected sub-phase
EvidencePredicates        â€” predicate IDs that grounded this decision
KnowledgeEdge             â€” true when confidence below floor
FailureClassification     â€” "none" for clean routes
```

Do not add `dependency_order`, `parallel_group`, `domain_label`, or `is_primary`.

**Example â€” clean multi-route:**

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

**DPO pair â€” ambiguous-predicate-case:**

```
CHOSEN: routes to 3.4 AND 3.3 â€” two decisions (shown above)

REJECTED:
  routing_decisions:
    - PredictedSubPhaseId: "3.4"
      Confidence: 0.91
      EvidencePredicates: ["AUTHORIZATION", "SECURITY"]
      KnowledgeEdge: false
    (only one decision â€” 3.3 omitted)
[WRONG: API_CONTRACT predicate is present from "API" in the prompt. When multiple
predicates match different sub-phases, all sub-phases must be routed. Returning
only the highest-confidence decision suppresses routing signal for API_CONTRACT
and leaves 3.3 as a knowledge edge when it should be a predicted route.]
```

---

### Layer 0-C â€” Knowledge Edge Detection

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

**DPO pair â€” knowledge-edge-suppressed:**

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

### Layer 0-D â€” Flow Factory Selection

Every batch must include all four trust-state cases:

1. `UseLearnedUnitWhenAvailable: true` + Trusted learned unit â†’ learned unit selected
2. `UseLearnedUnitWhenAvailable: false` + Trusted learned unit â†’ deterministic selected
3. `UseLearnedUnitWhenAvailable: true` + only Rejected learned unit â†’ deterministic fallback
4. `UseLearnedUnitWhenAvailable: true` + Probationary + minimum trust Probationary â†’ Probationary selected

**DPO pair â€” Rejected-unit case:**

```
CHOSEN: deterministic unit selected (Rejected learned unit excluded)
REJECTED: Rejected learned unit selected
[WRONG: Rejected units are excluded from selection regardless of the
UseLearnedUnitWhenAvailable flag. A unit at Rejected trust state was assessed
as harmful or incorrect. Selecting it would bypass the trust evaluation system.
The deterministic fallback must be used when the only learned unit is Rejected.]
```

---

## Layer 1 â€” Transition Orchestrator Pairs

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
adds JWT authentication that does not exist yet â€” it is an addition, not a fix.
Selecting repair would set the wrong A/B/X structure for all downstream nodes.]
```

---

## Layer 2 â€” Nine-Node Flow Pairs

Downstream node pairs must be written from real upstream outputs â€” never from
invented shapes.

### Node 01 â€” Domain Splitter

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
CHOSEN: packets for 3.4 and 5.2 only â€” both present in the transition A/B/X

REJECTED: packets for 3.4, 5.2, and 3.1 â€” 3.1 added because it usually appears
          alongside auth work
[WRONG: the domain splitter adds only territories present in the transition.
Architecture and component topology (3.1) was not stated in the prompt or in
the A/B/X statement. Adding it inflates the branch count and sends the planner
a false scope signal.]
```

### Node 02 â€” Domain Translator

```
DomainTranslatorUnit : ILogicalUnit<DomainPacket, TranslatedDomainStatement>
```

A is exactly one domain packet â€” array-node, never the full packet array.

### Node 03 â€” Scheme Planner

```
SchemePlannerUnit : ILogicalUnit<TranslatedDomainStatement[], WorkBranch[]>
```

B is the ordered work branch array with explicit dependency relationships.

Required fields per branch:

```
branch_id, domain_packet_ids, action, produces, depends_on,
parallel_group, wait_state, recovery_path, boundary_or_knowledge_edge_status
```

**DPO pair â€” highest priority for PLAN training:**

```
CHOSEN:
  branch_1: sub_phase=3.4, action="Define JWT model and bypass paths",
            depends_on=[], parallel_group=1
  branch_2: sub_phase=5.2, action="Write tenant isolation integration tests",
            depends_on=["branch_1"], parallel_group=2

REJECTED (Type B â€” reversed dependency):
  branch_1: sub_phase=5.2, action="Write tenant isolation tests with mock tokens",
            depends_on=[], parallel_group=1
  branch_2: sub_phase=3.4, action="Define JWT model after tests specify it",
            depends_on=["branch_1"], parallel_group=2
[WRONG: integration tests for tenant isolation require a working JWT mechanism to
issue and validate real tokens. Mock tokens cannot test real tenant isolation
because they bypass the actual validation logic. The security model must be
defined and deployed before integration tests can run meaningfully.]
```

### Nodes 04â€“06 â€” Branch Execution

Node 04 `DomainProcessorUnit`, Node 05 `ApplierUnit`, Node 06 `ResultTranslatorUnit`
are array-node positions. Write one branch at a time.

Node 06 may fold applied results into fewer fragments but every status class
(accepted, blocked, rejected, knowledge-edge) must remain visible in B.

### Node 07 â€” Arbiter Selector

```
ArbiterSelectorUnit : ILogicalUnit<TranslatedResultFragment[], ArbiterReviewRequest[]>
```

B names each selected arbiter and each considered-but-not-selected arbiter with
the reason. This teaches arbiter routing, not arbiter judgment.

### Node 08 â€” Arbiter Unit

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

### Node 09 â€” Formatter

```
FormatterUnit : ILogicalUnit<ReviewedResultSet, HumanResult>
```

B names accepted, blocked, rejected, knowledge-edge, and next-action sections
where those statuses exist.

---

## Layer 3 â€” Teaching Unit Pairs

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

## Stage 9 â€” Corruption Cases

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
Correct:   Routes to 3.4 (SECURITY) AND 3.3 (API_CONTRACT) â€” two decisions.
Corrupted: Routes only to 3.4, ignores API_CONTRACT.
[WRONG: API_CONTRACT predicate is present from "API endpoint". When multiple
predicates match different sub-phases, all sub-phases must appear in the routing
output. Returning only one suppresses valid routing signal.]
```

### knowledge-edge-suppressed

```
Prompt routes to 5.3 â€” no flow registered.
Correct:   OverallVerdict: "KNOWLEDGE_EDGE"
           Metrics: knowledge_edge=True, missing_sub_phase=5.3, missing_flow=True,
                    routed_phase_id=5, routing_confidence="0.84",
                    knowledge_edge_reason="no logical flow registered for routed sub-phase"
Corrupted: OverallVerdict: "PASS" â€” knowledge edge suppressed.
[WRONG: PASS fabricates a result the system cannot produce for an unregistered
sub-phase. The required output is KNOWLEDGE_EDGE with all six metrics present.]
```

---

## Canonicality Framework

Every source document must be assigned a canonicality class before any pair is
constructed from it. The source family rules govern what pairs a document can
produce. The canonicality class governs whether this particular copy is a valid
source at all. These are independent axes â€” a document can be the right family
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

external           Originates outside XIIGen â€” external research, open-source
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

- `source_canonicality: mirror` â†’ pair rejected at submission gate regardless of quality
- `source_canonicality: stale` â†’ pair rejected unless the pair is a Type 16 (superseded vocabulary) pair
- `source_canonicality: archive_container` â†’ pair rejected; unpack and reclassify first
- `source_canonicality: secure_operational` â†’ pair rejected; do not log path
- `source_canonicality: external` and `mixed_with_internal: true` â†’ hard gate failure; external and internal pairs must be in separate batch files

---

## Source Families

Every source document entering the training pipeline belongs to one of the
families below. The family determines which pair types are permitted, what the
typical overlap ratio is, and what is excluded.

The rule that applies to all families without exception: **B must be the actual
runtime output object, not a description of it.** This is EM-2 and it fires
more often from external source families than from session documents.

---

### U01 â€” Product and business flow specifications

**What it is:** Business flow analysis documents, task catalogs, engine
architecture documents per business module.

**DPO output:** These documents contain three zones that produce different pair
types. Do not treat the whole document as one type.

Zone A â€” acceptance criteria sections â†’ ArbiterUnit pairs (Node 08).
The criteria state what must be true before a result is accepted. Chosen = pass
verdict when criterion is met. Rejected = pass verdict when criterion is absent
from the implementation.

Zone B â€” task type contracts â†’ Layer 0 routing pairs.
The task type name, sub-phase, and predicate vocabulary become routing evidence
for MultiSubPhaseRouterUnit pairs and for `CompiledPredicateIds` construction.

Zone C â€” architectural decisions â†’ ARCH Type A pairs.
Ownership rules and component responsibilities in BFA documents are B-side
authority for 3.1 and 3.2 chosen responses.

**Zone mapping rule:** Before extracting any pair, label each paragraph as Zone
A, B, or C. Do not mix zones in a single pair.

**Excluded:** Do not use BFA document prose as A-side developer prompts. The
language is formal and domain-specific â€” it does not match real developer input.

**Typical overlap:** 0.10â€“0.25 for Zone A/C pairs. 0.15â€“0.30 for Zone B pairs.

---

### U02 â€” Deep research reports

**What it is:** External domain research documents, marketplace research,
technology landscape surveys. These describe the external world, not XIIGen
internal decisions.

**DPO output:** Permitted for CODE_Q pairs only â€” prompts that ask about
external standards, protocols, or technology behaviour. The research document
provides authority evidence for the B side when it cites a named external
standard (RFC number, specification name, version).

```
Permitted use:
  Prompt:  "how does GraphQL federation handle cross-service type merging?"
  Chosen:  "[from research document citing Apollo Federation spec v2]
            Each subgraph defines its own types. The gateway merges them using
            the @key directive â€” fields marked @key in one subgraph can be
            resolved from another. The merge happens at query planning time,
            not at runtime."
  Rejected: "[outdated version â€” pre-federation v2]
             Each service must republish its full schema to the gateway.
             Cross-service references are handled by stitching at the gateway."
  [WRONG: Schema stitching was the pre-federation approach. Federation v2 uses
  subgraph-owned @key directives, not gateway-side stitching. The outdated
  description teaches the wrong runtime behaviour.]
```

**Excluded:** Do not use research documents as B-side authority for any ARCH,
PLAN, or BUG pair. Research documents describe the external world â€” they cannot
confirm what XIIGen has decided. Do not use as PLAN ordering authority.

**Failure mode:** Using a research document to justify a PLAN branch order
or an ARCH decision produces pairs that may become wrong when the external
landscape changes. The training set cannot be easily corrected when the
underlying source is invalidated by external events.

**Typical overlap:** 0.20â€“0.35 for CODE_Q version-comparison pairs.

---

### U03 â€” Translation and migration plans

**What it is:** Step-by-step migration plans (NestJS â†’ Python, Figma â†’ React
Native, platform to platform), translation state JSON files recording what has
been completed and what is pending.

**DPO output:** Primary source for PLAN Type B pairs. Migration plans contain
the clearest dependency ordering in the corpus because they were written to be
executed in sequence â€” the ordering rationale is already in the document.

```
Example from a NestJS â†’ Python migration plan:

Chosen (PLAN â€” correct migration order):
  Branch 1 â€” 3.1 Architecture: map all NestJS module boundaries to Python
    service boundaries. Produce the dependency graph before any code moves.
    depends_on: []
  Branch 2 â€” 4.1 Backend: migrate core fabric interfaces to Python first â€”
    these are the only dependency that every other service imports.
    depends_on: ["branch_1"]
  Branch 3 â€” 4.1 Backend: migrate individual services against the new
    Python interfaces.
    depends_on: ["branch_2"]

Rejected (PLAN â€” migrate services before interfaces):
  Branch 1 â€” migrate individual services first.
  Branch 2 â€” migrate fabric interfaces after all services are moved.
  depends_on: [] for both.
[WRONG: individual services import the fabric interfaces. Migrating services
before the interfaces means services are rewritten against TypeScript interfaces
that will change again when the interfaces are migrated. The interfaces must
be established first â€” they are the stable contract that all services depend on.]
```

**TRANSLATION_STATE.json files** record completed steps, pending steps, and
blocked steps. Use as Layer 3 measurement evidence (parallel to Type 4 in the
registry) â€” the state file shows what a correct workflow execution record looks
like. Rejected = state file with missing required fields or incorrect status values.

**Excluded:** Do not use migration step text as A-side developer prompts without
verifying the prompt is in developer language ("fix the rxjs pipe", not "execute
migration step 3.2.1").

**Typical overlap:** 0.30â€“0.45 for Type B ordering pairs. Annotation is critical â€”
the rationale from the plan document becomes the annotation text directly.

**Pair kind:** `migration_plan`

---

### U04 â€” Plugin and extension marketplace documents

**A-side prompts only.** These documents describe external platforms (WordPress,
Adobe Express, plugin ecosystems) that XIIGen does not control. The platform
specifications may change and cannot serve as B-side authority for XIIGen decisions.

Real developer prompts from plugin integration sessions ("how do I connect a
WordPress webhook to a XIIGen flow?", "the Adobe Express plugin errors on export")
are valid Type 1 A-sides â€” they carry UI, IMPLEMENT, or BUG predicates.

**Excluded as B-side authority:** Do not use plugin documentation to justify ARCH,
PLAN, or any XIIGen-internal decision. Platform documentation is evidence for
the external context, not for internal contracts.

---

### U05 â€” Skill library documents

**What it is:** SKILL.md files, skill manifests, prompt engineering templates
defining how the agent should behave on specific task types.

**DPO output:** Type C pairs only â€” wrong domain for the prompt's verb.
A skill document defines the domain boundary for a specific task type. The
correct response stays within that boundary. The rejected response crosses it
by applying a different skill's logic to the same prompt.

```
Example from an architecture skill document:

Prompt:  "define the component boundaries for the payment service"

Chosen (ARCH â€” within skill boundary):
  PaymentGateway owns external provider calls. PaymentProcessor owns transaction
  logic. PaymentStore owns persistence. Dependencies: Processor â†’ Store â†’ Gateway
  direction only. Forbidden: Gateway must not call Processor.

Rejected (wrong skill â€” applies implementation logic):
  Create a PaymentService class with ProcessPayment(), RecordTransaction(), and
  CallProvider() methods. Wire the dependencies through DI.
[WRONG: the ARCH skill defines boundaries and dependency rules, not class bodies.
Applying the implementation skill here produces code instead of a boundary
decision. The class cannot be written until the boundary is decided.]
```

**Type C only â€” not Type A or B.** The skill library defines which domain owns
a task, not the implementation within that domain. A Type A rejected (adjacent
sub-phase) can still be used, but the chosen must satisfy the skill's domain
contract exactly.

**Excluded:** Do not use SKILL.md procedural steps as A-sides. Do not use
prompt templates as developer prompts â€” they are authority evidence for B,
not natural user language.

**Typical overlap:** 0.10â€“0.25 for Type C pairs (different skill vocabulary
sets are largely non-overlapping).

**Pair kind:** `skill_boundary`

---

### U06 â€” Delivery test packages

**What it is:** End-to-end test plans, GATE-B-FINDINGS.md files, learning
pipeline test plans, delivery verification packages.

**Split into two zones:**

Zone A â€” GATE-B-FINDINGS files â†’ Node 08 ArbiterUnit pairs. These contain
real production findings: "GATE B: pattern X absent on line Y" is a real
concern or reprocess verdict produced by an actual delivery. Use directly
as ArbiterUnit B-side content â€” these are the highest-quality arbiter pairs
available because the finding is real, not constructed.

```
GATE-B-FINDINGS entry:
  finding: "E34V2 fusion absent â€” AstRelevanceRanker has BM25 and KNN
            separately but no fusion step. EXPERIMENT-IMPLEMENTATION-STATUS
            shows NOT IMPLEMENTED."
  verdict: concern

â†’ ArbiterUnit pair:
  A: ArbiterReviewRequest for retrieval path review
  Chosen: verdict=concern, finding=[content from GATE-B-FINDINGS]
  Rejected: verdict=pass, finding="BM25 and KNN are present in AstRelevanceRanker"
  [WRONG: present is not the same as fused. The fusion is not implemented â€”
  GATE-B-FINDINGS confirms this with a direct code check. Pass verdict requires
  the control to be satisfied, not just the components to exist.]
```

Zone B â€” E2E test plans â†’ 5.2 and 5.3 sub-phase pairs. Test plans describe
what integration tests must verify. Use as B-side authority for TEST-predicate
routing decisions and for Node 03 branch content covering 5.2/5.3.

**Excluded:** Do not use delivery package pass/fail summaries as PLAN A-sides
without verifying the summary contains developer-language prompts.

**Typical overlap:** 0.10â€“0.20 for Zone A arbiter pairs.

---

### U07 â€” Flow portability protocols

**What it is:** FLOW-PORTABILITY-TEST-PROTOCOL documents, FORK-EXECUTION-DIRECTIVE
specifications, portability invariant checklists.

**DPO output:** The only source of invariant-based pairs in the corpus. Portability
protocols define what must be true for any flow regardless of tenant or environment.
An invariant violation is always a reprocess verdict â€” not a concern.

**Rejected construction rule for portability pairs:** Take a correct flow
description, introduce exactly one tenant-specific hardcode, mark it as rejected.
The annotation must name the specific invariant that fails.

```
Invariant: "no flow may embed a literal tenant ID in any route, selector,
            or configuration value."

Chosen (portability invariant satisfied):
  TenantAwareRouter resolves the tenant context from the request header at
  runtime. Route pattern: /api/{tenantId}/resource â€” tenantId from the resolved
  context, not from configuration.

Rejected (invariant violated):
  Route: /api/acme-corp/resource â€” tenant ID hardcoded in the route definition.
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

**Typical overlap:** 0.10â€“0.20. Invariant vocabulary (tenant-specific, hardcoded,
portability test, routing_confidence) does not overlap with correct flow vocabulary
(resolves at runtime, from context, tenant-agnostic).

**Pair kind:** `portability_invariant`

---

### U08 â€” Design conversion documents

**A-side prompts only.** Figma-to-code conversion maps, Adobe Express design
specs, UI asset conversion instructions. These are source material for UI and
frontend sub-phase (1.1, 1.2, 4.2) developer prompts.

Real developer prompts from conversion sessions ("convert this Figma artboard to
a React component", "the Adobe export has the wrong font weight") are valid Type 1
A-sides carrying UI predicate.

**Excluded as B-side authority:** Design specs describe visual intent, not XIIGen
runtime contracts. Do not use as B-side for any non-UI sub-phase pair.

---

### U09 â€” Machine state JSON files

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

### U10 â€” Source archives and snapshots

**Excluded.** ZIP archives and snapshots are delivery artifacts, not source
documents. A ZIP file as a unit has no DPO pair role.

**Required preprocessing:** Unpack the archive. Classify each contained file
by its document family (U01â€“U09, U11â€“U13). Apply the corresponding family rules
to each file individually.

Do not extract pairs from a file discovered inside a ZIP without first classifying
it. A session state document found inside a ZIP is a Type 13 document in the
registry â€” treat it as such, not as "a file from the ZIP".

---

### U11 â€” Agent tooling instructions

**What it is:** AGENTS.md, CLAUDE.md, copilot-instructions.md, tool-calling
instruction sets. These are machine-facing executable contracts â€” they specify
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
surfaced. The two steps are not optional â€” they are the gate before output.]
```

**What distinguishes this from Type 14 (behavioral rules):**
- Type 14: rules state what a human author must not do when writing pairs ("do not use the negation template")
- Type 11 (agent tooling): contracts state what the agent must do at runtime ("call tool X before producing output")

Type 14 pairs test whether a plan or document follows the rules. Type U11 pairs
test whether the agent's runtime tool-calling behaviour follows its instructions.

**Excluded:** Do not use AGENTS.md rule text as developer A-side prompts. Do not
use tool-calling instructions as B-side authority for sub-phase content decisions.

**Typical overlap:** 0.10â€“0.20. Correct tool sequence vocabulary (search, read,
then produce) does not overlap with skipped-step vocabulary.

**Pair kind:** `tooling_contract`

---

### U12 â€” Capability catalogs and matrices

**Split handling â€” three zones:**

Zone A â€” capability matrices showing flow-to-subphase coverage â†’ extend
corruption case `knowledge-edge-suppressed`. A matrix entry that shows "flow X
does NOT cover sub-phase W" is direct evidence that routing to W should produce
KNOWLEDGE_EDGE, not PASS.

Zone B â€” UNIFIED_SOURCE_INDEX and RAG indexes â†’ vocabulary calibration only.
These are reference indexes, not authority sources. Use to verify that predicate
terms are consistent across documents. Do not build pairs directly from index text.

Zone C â€” SKILLS_FACTORY_RAG â†’ excluded as direct pair source. RAG indexes
contain fragments from many documents mixed together. Unattributable fragments
cannot be used as B-side authority.

**Excluded as standalone B-side source:** Do not attribute a B-side claim to
a capability matrix unless the matrix cites a specific flow ID and sub-phase ID
with status evidence.

---

### U13 â€” Raw merged text dumps

**Excluded until preprocessed.** Files like `extracted_merged.txt` and
`planning_session.txt` are unsegmented concatenations of multiple conversations
or documents. A-side language from these files will contain fragments from
different contexts that were never connected in any real developer session.

**Required preprocessing protocol:**
1. Split on segment delimiters: `---`, timestamps, `Human:`, `Assistant:`,
   conversation boundary markers.
2. Classify each segment individually against U01â€“U12 family rules.
3. Extract pairs only from segments that pass the individual family gate.

Do not extract any pair from a raw merged dump without segment classification.
Segments that cannot be attributed to a single conversation or document are
excluded regardless of content quality.

---

### U14 â€” Gap remediation and fix documents

**What it is:** Documents that record a confirmed gap, the wrong fix that was
attempted, and the correct fix with evidence. Examples: `GAP-IMPLEMENTATION-PLAN`,
`FIX-PLAN`, `REMEDIATION-PLAN`, `PER-FLOW-FIX-MAP`, `Missing gaps` documents,
and the gap findings section of `GATE-B-FINDINGS.md`.

**Why this is distinct from Types 7 and 8:** Type 7 records implementation status
(a status read). Type 8 records arbiter verdicts on artifacts. U14 records the full
repair arc: gap identification â†’ wrong-fix attempt â†’ correct fix with evidence. No
other type has all three parts in one document.

**Three-part extraction rule â€” mandatory before building any pair:**

```
Part 1 â€” gap description:   context for the A-side. NOT the A-side itself.
Part 2 â€” wrong-fix attempt: what was tried and failed â†’ becomes rejected
Part 3 â€” correct fix + evidence â†’ becomes chosen
```

If the document has only Part 1 + Part 3 (no wrong-fix attempt recorded): construct
rejected using the ARCH or BUG confusion tables. Do not invent a wrong fix.
Correct fix must have evidence (test result, run output, or verification statement).
A fix without evidence is a claim, not ground truth.

**DPO pair â€” BUG repair:**

```
A (Type 1 â€” developer language, NOT gap description text):
  "fix the domain classifier KNN path â€” it never activates"

Chosen (Part 3 â€” correct fix with evidence):
  "BuildSeededClassifier() at static construction. Seed with one
   representative predicate profile per domain. _trained=true before
   the first prompt reaches the guard check.
   Evidence: REMEDIATION-PLAN Â§3 confirms KNN path activates on first call."

Rejected (Part 2 â€” wrong-fix attempt):
  "Call Train() at the top of ClassifyBaseDomain()."
[WRONG: Train() inside ClassifyBaseDomain() executes after the _trained guard
check â€” the guard fires first and falls to ClassifyByKeywords before Train()
runs. The fix must run before the guard, not after it. Only static construction
executes before the guard.]
```

**False-pass entries** â€” the highest-value form. Gate claimed PASS but
implementation was still wrong. The false pass is the rejected; the
correctly-failing verdict is chosen for an ArbiterUnit pair.

```
A (ArbiterReviewRequest):
  artifact_type: "flow_service_file"
  artifact_content: "tenantId = config.GetValue('default_tenant') ?? 'tenant-abc'"
  review_criteria: "portability check P-3: no hardcoded tenant identifiers"
  source_rule: "FLOW-PORTABILITY-TEST-PROTOCOL check P-3"

Chosen (correct detection â€” reprocess):
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
same portability test as a literal â€” the flow cannot serve any tenant but acme-corp.]
```

**Annotation rule:** Name the mechanism of failure â€” which call site, layer, or
detection gap makes the wrong fix fail. Not just "this is wrong".

**Typical overlap:** 0.15â€“0.30 for BUG/ARCH repair pairs. 0.10â€“0.20 for
false-pass arbiter pairs.

**Pair kind:** `gap_remediation`

**Excluded use:**
```
Do not use gap description text (Part 1) as A-side.
A-side is always a Type 1 developer-language prompt.
Do not invent a wrong fix if the document does not record one â€” use confusion tables.
Do not use a correct fix as chosen without evidence attached.
```

---

### U15 â€” Visual UI quality evidence

**Zone mapping â€” three distinct zones. Identify before building any pair.**

Documents: VIZ-QA files, UI-STATE-MAP, ROLE-SCREEN-MATRIX, GALLERY-DESIGN-SYSTEM,
UML_COVERAGE_ANALYSIS.

```
Zone A â€” Visual findings and defect reports
  Markers: "defect", "visual regression", "wrong color", "layout break",
           "accessibility violation", "fails visual check"
  Apply: Type 8 ArbiterUnit rules.
  Chosen: concern or reprocess verdict with specific visual finding.
  Rejected: pass verdict without checking the visual relationship.

Zone B â€” Role/screen maps and design system references
  Markers: "role", "screen", "layout", "design token", "component spec"
  Apply: Type 2 (ARCH/UI) B-side rules for sub-phases 1.1 and 4.2.
  Use as chosen content for UI ARCH boundary pairs.

Zone C â€” UML and coverage analysis
  Markers: "UML", "coverage", "class diagram", "sequence diagram",
           "which screens are covered"
  Apply: Type 7 extension rules â€” coverage as implementation status.
  Use for knowledge-edge detection pairs (what is covered vs not).
```

**Excluded:** Do not use screenshot captions or layout annotations as developer
A-sides â€” design-tool language is not developer-intent language.

---

### U16 â€” Flow generator guidance library

**Zone mapping â€” three distinct zones. Identify before building any pair.**

Documents: GUIDE-B01 through GUIDE-B50, LIST-A-COVERAGE-REPORT, LIBRARY-MASTER-INDEX.

```
Zone A â€” GUIDE-B procedure steps
  Markers: "Step N", "required before", "gate", "prerequisite"
  Apply: Type 10 PLAN ordering rules.
  Chosen: correct step order with explicit dependency chain.
  Rejected: steps in wrong order or gate skipped.

Zone B â€” Coverage reports
  Markers: "covered", "missing", "gap", "LIST-A"
  Apply: Type 8 arbiter rules (coverage = verdict on completeness)
         or Type 13 document register rules (which guide is current).

Zone C â€” Source maps and library index
  Markers: "maps to", "source", "LIBRARY-MASTER-INDEX"
  Provenance only. Not a direct pair source.
  Use for vocabulary calibration and canonical source tracing only.
```

**Excluded:** Do not use guide step text as A-side developer prompts.

---

### U17 â€” Worktree mirrors and duplicate copies

**Excluded â€” `mirror` canonicality. Use canonical copy only.**

Locations: `.claude/worktrees/`, `business-flows/` when duplicating `business flows/`,
`auth_extracted/`, `old copies/` subdirectories.

Mirrors must not be used as training sources. If both canonical and mirror exist,
use canonical only. If canonical is missing and only mirror exists, promote mirror
to `extracted_copy` canonicality, record the source gap, then proceed.

Never include both the canonical and a mirror of the same document in the same batch
â€” this creates contradictory training signal with near-identical token distributions.

---

### U18 â€” Certificates and environment material

**Excluded â€” `secure_operational` canonicality. No exception.**

Files: `certificates/*.crt`, `certificates/*.csr`, `environment.zip`, `env*.zip`,
`.env` files.

Do not inspect, read, or extract any content from these files. Do not log these
paths in any pair manifest. Treat as `secure_operational` and stop.

---

### U19 â€” Code repository snapshots with embedded documents

**Excluded as a unit â€” `archive_container` canonicality. Unpack and reclassify.**

Examples: `xiigen_v18_complete.zip`, `llm_mvp_core-main.zip`, `orchestrator-mvp-main.zip`.

The snapshot itself has no DPO value. After unpacking, deduplicate against canonical
copies in the document state register, then classify each extracted file:

```
Source code files (.cs, .ts, .py) â†’ vocabulary calibration only; not direct pair source
CLAUDE.md / AGENTS.md             â†’ U11 (agent tooling) â€” family rules apply
session state docs                â†’ Type 13 (document state register)
DECISIONS-LOCKED.md               â†’ Type 9 (decision documents)
test files with annotations       â†’ check for correction events (Type 3 pattern)
WHAT-TO-DO docs                   â†’ Type 14 (behavioral rules)
Markdown docs                     â†’ classify by content against all family rules
```

Never use the snapshot filename as a document title in an A-side prompt.
Set `source_canonicality: extracted_copy` and record the archive path.

---

### U20 â€” Benchmark, stress, and dry-run logs

**Zone mapping â€” three distinct zones. Identify before building any pair.**

Documents: V62_BFA_STRESS_TEST, REFERENCE-RAG-BENCHMARK-PLAN, DRY-RUN files,
EXECUTION-LOG, EXPECTED-TRACE.

```
Zone A â€” Actual run metrics (measured output)
  Markers: "elapsed_ms", "cpu_percent", "accuracy", "loss", recorded numbers
  Apply: Type 4 measurement evidence rules.
  Use as B-side content for ModelTrainingUnit pairs.

Zone B â€” Expected traces and reference plans
  Markers: "expected", "should produce", "reference output", "plan"
  Apply: Type 10/14 authority rules.
  Ground truth for what a correct run looks like â€” use as chosen.
  A run that diverges from expected becomes the rejected.

Zone C â€” Dry-run and stress reports (planned, not yet measured)
  Markers: "dry run", "stress test plan", "hypothetical", "expected to"
  Apply: B-side authority with explicit pair-file flag:
         "evidence_type": "expected_not_measured"
  Do not use as Type 4 measurement evidence.
  Use only as test plan authority under Type 10 rules.
```

**Excluded:** Do not treat planned metrics as actual metrics. A DRY-RUN document
that says "expected accuracy 0.82" is not a run that measured 0.82.

---

### U21 â€” Prompt handoff documents and session briefs

**Zone mapping â€” three distinct zones. Distinct from Type 15.**

Type 15 covers session-start gate prompts. U21 documents have different structure:

Documents: PROMPTS.md, basic_prompt.txt, CLAUDE-CODE-SESSION-PROMPT, HANDOFF-CONTRACT,
SESSION-BRIEF.

```
Zone A â€” Session start gates (pre-work and post-work conditions)
  Markers: "before any work", "live test count", "manifest required",
           "gate", "must verify"
  Apply: Type 15 rules exactly.

Zone B â€” Prompt templates (reusable instruction patterns)
  Markers: "template", "insert here", "{{placeholder}}", "use this format"
  Apply: Type 20 (skill library) rules for domain boundary pairs.
  Prompt templates define what a task should look like â€” rejected is
  a prompt that violates the template's required structure.
  Do not use as A-side input.

Zone C â€” Handoff contracts and session briefs
  Markers: "handoff to", "next session", "carry forward", "context for"
  Apply: Type 19 (agent tooling instructions) rules for ordering pairs.
  Handoff contracts define what the receiving agent must do and in what
  order â€” rejected skips a required handoff step.
```

**Excluded:** "Please read the following documents and confirm your understanding"
is Type 6 regardless of which document it appears in.

---

### U22 â€” External training sets and DPO pair batches

**Excluded from internal registry â€” `external` canonicality. Never silently mixed.**

Locations: `external-sources/`, `external-sources-training-sets/`,
`external-sources-training-sets 2/`, `dpo-pairs-*/`.

External DPO pair batches must not mix into internal teaching batches. The critical
failure mode: an external batch that contains XIIGen session prompts as A-sides looks
like internal material but was constructed with external B-side authority. Mixed
silently, it produces contradictory signal â€” the same A-side prompt produces different
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

### U23 â€” Generated code and debug correction histories

**What it is:** Debug traces and repair histories from design-to-code or code
generation sessions: `css-debug-history.json`, `generated_code.txt`,
`CORRECTED-DEFECT-CATALOG`, Figma-to-code conversion repair logs.

**Why distinct from Type 3 (correction events):** Type 3 corrections have overlap
< 0.25 because the wrong and correct answers come from different reasoning paths.
Type 24 (U23) corrections often have higher overlap (0.25â€“0.50) because the wrong
and correct code share most of their structure â€” only specific lines differ.
The annotation must carry more of the signal.

**DPO pair â€” BUG repair from debug history:**

```
A (Type 1 â€” the generation request, NOT the generated code):
  "generate the CSS layout for the tenant dashboard â€”
   two-column grid with sidebar collapsing below 768px"

Chosen (after debug correction â€” from css-debug-history.json):
  .tenant-dashboard {
    display: grid;
    grid-template-columns: 240px 1fr;
    gap: 16px;
  }
  @media (max-width: 768px) {
    .tenant-dashboard { grid-template-columns: 1fr; }
    .sidebar { order: 2; }
  }

Rejected (original generated code â€” before debug):
  .tenant-dashboard { display: flex; flex-direction: row; }
  .sidebar { width: 240px; flex-shrink: 0; }
  @media (max-width: 768px) {
    .tenant-dashboard { flex-direction: column; }
  }
[WRONG: flex collapse does not reorder sidebar â€” it appears above main content on
mobile. Grid with order: 2 pushes sidebar below content. The debug history records
the flex version as the defect causing visual regression on mobile devices.]
```

**Quality gate â€” apply before including any U23 pair:**

```
â–¡ Overlap ratio computed â€” if > 0.45: annotation must introduce â‰¥ 5 non-overlapping
  tokens describing why the wrong code fails functionally (not just which lines differ)
â–¡ Debug history confirms the wrong code was actually generated (not constructed) â€”
  rejected must come from a real generation event
â–¡ The correct code has a verification signal in the debug history
  (build passed, test passed, visual confirmed)
â–¡ defect class is a reasoning error, not a typo or missing import
  â€” Low quality (typo): exclude
  â€” Medium (missing edge case): include with strong annotation
  â€” High (reasoning error, wrong operator, wrong layer): include unconditionally
```

**Typical overlap:** 0.25â€“0.50. Higher than Type 3 because generated code shares
structure. Annotation weight is higher than Type 3.

**Pair kind:** `correction_event`

**Excluded:** Do not use debug trace text (stack traces, console output) as A-side.
A-side is always the original generation request. Do not include pairs where the
"wrong" version is a deliberate placeholder â€” rejected must be genuinely wrong.

---

### U24 â€” Marketplace, legal, and store assets

**Excluded â€” not core training material.**

Files like `STORE_LISTING.md`, `LICENSE`, `NOTICE.md`, `privacy.html`,
`sitemap.xml`, `marketplace.json` are distribution and compliance artifacts.

Exception: if a session document contains a developer task prompt about preparing
marketplace or legal documentation, that prompt is a valid Type 1 A-side for a
DOCUMENT-predicate pair (sub-phase 8.x). The marketplace document itself remains
excluded as direct pair source.

---

### A13 â€” Diagram and topology source files

**What it is:** Machine-readable diagram files that encode component relationships,
dependency graphs, and architecture topology as structured data.
Examples: `*.mmd`, `*.mermaid`, `*.drawio`, `*.puml`, `DEPENDENCY-GRAPH.mmd`.

**Limited authority â€” topology and graph evidence only.**

These files are executable topology evidence: a Mermaid diagram of the nine-node
flow encodes the same node and edge relationships as a prose description, but more
precisely and without ambiguity. Use as Type 5 extension for topology vocabulary
and node/edge pair content.

**Valid DPO use â€” two scopes:**

Scope 1: Topology boundary pairs (Type 5 extension). A diagram that shows
`CompilerUnit â†’ RouterUnit â†’ SchemePlannerUnit` is authoritative for which unit
receives which unit's output. Rejected = wrong dependency direction.

```
A: "which unit receives the compiler's output?"

Chosen (from DEPENDENCY-GRAPH.mmd):
  "MultiSubPhaseRouterUnit receives SubPhaseRouterSourceDocument from
   PromptRoutingInputCompilerUnit. The graph shows a directed edge:
   CompilerUnit â†’ RouterUnit. RouterUnit is the only consumer of the
   compiler's output in the nine-node flow."

Rejected:
  "The compiled prompt is passed directly to SchemePlannerUnit for branch ordering."
[WRONG: DEPENDENCY-GRAPH.mmd shows no direct edge from CompilerUnit to
SchemePlannerUnit. The edge goes CompilerUnit â†’ RouterUnit â†’ SchemePlannerUnit.
SchemePlannerUnit receives translated domain statements, not the compiled prompt.]
```

Scope 2: Architecture review pairs (Type 8 extension). A diagram that shows a
circular dependency is direct evidence for a reprocess verdict.

**Preprocessing required:** Parse as graph â€” preserve nodes and edges before
flattening to prose. Do not use diagram source text (Mermaid DSL, PlantUML syntax)
as A-side input.

**Excluded:** Do not derive sub-phase Purpose fields from diagrams â€” use Type 5
(topology seeder code) for that. Diagrams show structure; seeder code defines meaning.

**Pair kind:** `flow_node` (topology pairs), `arbiter` (review pairs)

---

### A14 â€” Runtime debug and tool logs

**Excluded as direct pair source â€” preprocessing and normalization required.**

Files: `debug.log`, `firebase-debug.log`, tool execution logs, build/test console
output.

Raw execution logs contain mixed content: timestamps, command invocations,
environment details, error messages. The error messages may be valuable evidence;
the surrounding context is noise.

**Limited use â€” failure evidence only, after normalization:**

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

### A15 â€” Test suite source files

**What it is:** Executable test files that encode expected behavior as assertions.
Examples: `*.test.ts`, `*.spec.ts`, `*Tests.cs`, `mock-figma.ts`, `test-settings.ts`.

**Why this is valuable:** A passing test is stronger evidence than a prose
description. The test file shows exactly what inputs produce what outputs, what
edge cases are expected, and what constitutes a pass. This is the most precise
available source for ArbiterUnit pairs â€” the assertion is the acceptance criterion.

**Two-part structure for extraction:**

Before extracting any pair from a test file:
```
Part 1 â€” test intent: what behavior is being verified (from describe/it block name)
Part 2 â€” assertion: what the test actually checks (from expect/assert calls)
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
  source_rule: "OrchestratorTests.cs â€” knowledge edge assertion"

Chosen (ArbiterUnit â€” reprocess: assertion fails):
  verdict: reprocess
  finding: "OverallVerdict is PASS but test suite requires KNOWLEDGE_EDGE when
            no flow is registered. Metrics.knowledge_edge and missing_flow fields
            are absent. The test assertion is the acceptance criterion â€” a result
            that does not satisfy it cannot be accepted."

Rejected (ArbiterUnit â€” pass: ignores test assertion):
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
â–¡ Test intent (Part 1) is clear from describe/it block name
â–¡ Assertion (Part 2) is in the actual expect/assert call â€” not in comments
â–¡ Test is passing in the current codebase (confirmed via document state register
  or live test run) â€” a failing test is a gap, not ground truth
â–¡ Test file is the canonical current version (three-check rule applies)
â–¡ Scaffolding (beforeEach, afterEach, mock setup) excluded from pair content
```

**Pair kind:** `arbiter` (assertion-based pairs), `flow_node` (behavior pairs)

**Excluded use:**
```
Do not use test file text as A-side developer prompts.
Do not use failing tests as chosen content â€” failing tests are Type 23 material.
Do not include mock scaffolding in pair content.
```

---

### A16 â€” Plugin and extension manifest files

**Limited authority â€” runtime capability boundary only.**

Files: `manifest.json` (browser extensions, Figma plugins), extension manifest
JSON, plugin descriptor files.

These differ from `package.json` (A02): package.json defines build dependencies.
Plugin manifests define runtime permissions and entry points â€” what the plugin
is allowed to do, which APIs it can call, which UI surfaces it can modify.

**Valid DPO use â€” one scope only:**

Capability boundary pairs: the manifest defines what the plugin is permitted to
access. A claim that the plugin does something not in its declared permissions is
a constraint violation â€” the manifest is the authority.

```
A: "can the Figma plugin read clipboard data without user permission?"

Chosen (from manifest permissions):
  "No â€” the manifest declares 'permissions': ['currentUser', 'activeUsers'].
   Clipboard access is not in the declared permissions list. Any clipboard
   read attempt will be blocked by the plugin runtime."

Rejected:
  "Yes â€” plugins can read clipboard data using the standard browser Clipboard API."
[WRONG: manifest permissions define what this plugin is allowed to do.
The 'clipboard-read' permission is not declared. Asserting clipboard access
is available contradicts the manifest contract.]
```

**Excluded:** Do not use manifest files as architecture authority for XIIGen internal
decisions. Manifests define plugin boundaries for external platforms, not XIIGen
component boundaries.

---

### A17 â€” Office, PDF, and exported document files

**Excluded as direct pair source â€” render and reclassify.**

Files: `*.docx`, `*.pdf`, exported planning documents, rendered specification packets.

Binary office and PDF files may duplicate or supersede markdown sources. They cannot
be read as text without preprocessing.

**Required preprocessing:**

```
1. Extract visible text content (strip formatting, headers, footers)
2. Deduplicate against canonical markdown sources via document state register
3. Classify extracted content by content type (Types 1â€“26)
4. Set source_canonicality appropriately:
   â€” If content matches a canonical markdown â†’ use the markdown as canonical
   â€” If content has no markdown equivalent â†’ extracted_copy canonicality
   â€” If content is older than the markdown â†’ stale canonicality (rejected vocab only)
```

Do not treat the `.docx` or `.pdf` filename as a document title in any A-side.

---

### A18 â€” Vendored third-party codebases

**Excluded from internal registry â€” external boundary applies.**

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
adoption decision records it â€” otherwise it is external.
```

---

### A19 â€” Generated UI code outputs

**What it is:** Generated HTML, React, TSX, and CSS files produced by conversion
experiments â€” not source-of-truth specifications.
Examples: `*_generated.html`, generated React/TSX files from Figma conversion,
generated CSS from design system exports.

**This is Type 24 source material.** Generated UI code outputs are the starting
point of a debug correction cycle. When a generated file has a documented defect
and a corrected version, the pair follows Type 24 (generated code debug) rules
exactly: A = the generation request, chosen = corrected output, rejected = original
generated output.

**Excluded as B-side authority:** Generated code is not architecture truth. Do not
use generated HTML or CSS as the authoritative specification for a UI component â€”
use the design spec (U08 Zone B) or the Type 26 source contract instead.

---

### A20 â€” Before/after visual comparison sets

**Zone mapping â€” visual QA evidence under U15 Zone A rules.**

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
pair content â€” the pair content is the verbal description of the visual defect
and its correction. Use image content as evidence for what the description says,
not as direct pair material.

---

### A21 â€” Positive/negative example catalogs

**High-value pre-made DPO source â€” verify labels and provenance before use.**

Files: `POSITIVE-NEGATIVE-EXAMPLES.md`, `CORRECTED-DEFECT-CATALOG`,
example catalogs, failure/success example lists.

**Why this is the highest-priority new family:** These documents have the chosen
and rejected already labeled. No pair construction is required â€” only extraction,
provenance verification, and quality rating. The expected overlap ratio is
typically < 0.30 because the example catalog was created precisely to show
contrast between good and bad outputs.

**Three-gate quality check before extraction:**

```
Gate 1 â€” Label verification:
  Confirm that "positive" / "pass" / "correct" examples are actually correct
  against current document state register and Type 9 locked decisions.
  A positive example that was correct when written but is superseded by a
  later decision is now stale â€” use as Type 16 rejected-vocabulary source only.

Gate 2 â€” Provenance verification:
  Confirm each example came from a real session or real artifact.
  Synthetic examples (written to fill a coverage gap) must be labeled
  source_canonicality: synthetic and treated with extra scrutiny.
  Synthetic examples have not been tested against the actual system.

Gate 3 â€” Overlap test:
  Compute overlap_ratio for each pair.
  If > 0.45: annotation must introduce â‰¥ 5 non-overlapping tokens.
  If > 0.60: exclude.
```

**Extraction pattern:**

```
Catalog entry:
  positive: "CompiledPredicateIds: ['TEST', 'SECURITY'] â€” correct when the
             prompt mentions 'test coverage' for the 'auth module'"
  negative: "CompiledPredicateIds: ['SECURITY', 'AUTHORIZATION'] â€” wrong;
             'test coverage' is the primary verb signal, not the object"

Extracted pair:
  A: "test coverage shows 42% on the authentication module"
  Chosen: CompiledPredicateIds: ["TEST", "SECURITY"] â€” extracted from positive example
  Rejected: CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"] â€” from negative example
  [WRONG: "test coverage" is the primary verb. "authentication" is the object
  being measured. TEST takes priority when the action is coverage measurement.
  Assigning SECURITY and AUTHORIZATION ignores the dominant predicate signal.]
```

**Overlap ratio:** typically < 0.30 â€” example catalogs are designed to show contrast.

**Pair kind:** depends on content â€” `flow_node`, `arbiter`, `adaptive_routing_compiler`,
or `correction_event` based on what the examples demonstrate.

**Excluded use:**
```
Never use catalog examples without Gate 1 (label verification).
Stale positive examples must not become chosen content.
Synthetic examples require extra scrutiny and explicit source_canonicality: synthetic.
```

---

### A22 â€” Algorithm reference and math notes

**Limited authority â€” CODE_Q only, same rules as Type 21.**

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

### A23 â€” Localized user-facing guides

**Apply A11 rules exactly â€” mirrors, not independent truth.**

Files: `README-HE.md`, Hebrew guides, translated onboarding docs, localized user
manuals.

The handling is identical to A11 (localization and language variants):
- A-side prompts in the developer's original language are valid Type 1 material
- B-side must use the canonical English source only
- Translated content is `source_canonicality: mirror` â€” never independent authority

If a localized guide contains content that differs from the canonical English
source, the canonical source wins. The discrepancy is a mirror drift issue, not
a new decision.

---

### A24 â€” Template and boilerplate libraries

**Limited authority â€” generation contract evidence after placeholder separation.**

Files: `templates/infra/*`, skill templates, component templates, boilerplate
project folders.

Templates define the expected shape of a generated output â€” what fields must be
present, what structure is required, what placeholders must be replaced. This is
the accepted contract for what the generated artifact should look like.

**Valid DPO use â€” structural completeness pairs:**

A template that defines required fields is authority for what a complete artifact
looks like. An artifact that omits a required template field is incomplete â€” this
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
Rejected: manifest with flow_id and sub_phase only â€” tenant_cert_target and
          portability_status absent
[WRONG: the template defines four required fields. A manifest without
tenant_cert_target and portability_status cannot be validated against the
portability protocol â€” the protocol checks these fields explicitly.]
```

**Required: separate placeholders from concrete content before any pair.**
A template `{{FLOW_ID}}` is not chosen content â€” the resolved value `flow-21`
is chosen content. Never include unreplaced placeholder tokens in any pair.

**Excluded:** Boilerplate folder structures and project scaffolding that contain
no field-level contracts have no DPO pair value â€” they define project layout, not
content correctness.

---



| Family | Pair type | Use as A-side | Use as B-side authority | Typical overlap | Pair kind |
|--------|-----------|--------------|------------------------|-----------------|-----------|
| U01 â€” business flow specs | A, B, C (zone-dependent) | âœ— | âœ“ (per zone) | 0.10â€“0.30 | flow_node, arbiter |
| U02 â€” research reports | C only | âœ— | âœ“ CODE_Q only | 0.20â€“0.35 | research_fact |
| U03 â€” migration plans | B primary | âœ— | âœ“ PLAN ordering | 0.30â€“0.45 | migration_plan |
| U04 â€” plugin docs | none | âœ“ prompts only | âœ— | n/a | â€” |
| U05 â€” skill library | C only | âœ— | âœ“ domain boundary | 0.10â€“0.25 | skill_boundary |
| U06 â€” delivery test packages | A (Zone A), C (Zone B) | âœ— | âœ“ (per zone) | 0.10â€“0.25 | arbiter, flow_node |
| U07 â€” portability protocols | invariant-based | âœ— | âœ“ invariant check | 0.10â€“0.20 | portability_invariant |
| U08 â€” design conversion | none | âœ“ UI prompts only | âœ— | n/a | â€” |
| U09 â€” machine state JSON | missing-field | âœ— | âœ“ state records | < 0.15 | teaching_unit |
| U10 â€” source archives | excluded | âœ— | âœ— | â€” | unpack first |
| U11 â€” agent tooling | contract-compliance | âœ— | âœ“ tool sequence | 0.10â€“0.20 | tooling_contract |
| U12 â€” capability catalogs | Zone A: corruption only | âœ— | Zone A only | 0.05â€“0.15 | corruption_case |
| U13 â€” raw merged dumps | excluded | âœ— | âœ— | â€” | segment first |
| U14 â€” gap remediation | wrong-fix / false-pass | âœ— | âœ“ correct fix | 0.10â€“0.30 | gap_remediation |
| U15 â€” visual UI quality | zone-dependent | âœ— | âœ“ (per zone) | 0.10â€“0.30 | arbiter, flow_node |
| U16 â€” flow guide library | zone-dependent | âœ— | âœ“ Zone A/B only | 0.15â€“0.45 | migration_plan, corruption_case |
| U17 â€” worktree mirrors | excluded | âœ— | âœ— | â€” | canonicalize first |
| U18 â€” certificates / env | excluded | âœ— | âœ— | â€” | sensitive, exclude |
| U19 â€” code repo snapshots | excluded as unit | âœ— | âœ— | â€” | unpack + reclassify |
| U20 â€” benchmark / dry-run | zone-dependent | âœ— | âœ“ Zone A/B only | see zones | teaching_unit, flow_node |
| U21 â€” prompt handoff | zone-dependent | âœ“ Zone C only | âœ“ Zone A/B | 0.10â€“0.30 | tooling_contract, flow_node |
| U22 â€” external DPO batches | excluded from internal | âœ— | âœ— | â€” | external registry only |
| U23 â€” generated code debug | correction_event | âœ— | âœ“ correct repair | 0.10â€“0.30 | correction_event |
| U24 â€” marketplace / legal | excluded | âœ— | âœ— | â€” | not training material |
| A13 â€” diagram / topology files | topology + review | âœ— | âœ“ graph structure | Wrong dependency direction | 0.10â€“0.20 |
| A14 â€” runtime debug logs | excluded direct; normalize first | âœ— | âœ“ failure evidence only | â€” | gap_remediation |
| A15 â€” test suite source files | excluded as A-side | âœ— | âœ“ assertion as acceptance criterion | Assertion-failing variant | 0.10â€“0.25 |
| A16 â€” plugin manifests | capability boundary only | âœ— | âœ“ permission boundary | Undeclared permission claim | 0.10â€“0.20 |
| A17 â€” office / PDF exports | excluded direct; render first | âœ— | âœ— | â€” | reclassify after extract |
| A18 â€” vendored third-party code | excluded (external boundary) | âœ— | âœ— | â€” | external registry only |
| A19 â€” generated UI code | Type 24 source material | âœ— | âœ— | â€” | correction_event |
| A20 â€” before/after visuals | U15 Zone A extension | âœ— | âœ“ visual defect evidence | Pass without checking | 0.15â€“0.30 |
| A21 â€” positive/negative catalogs | pre-made pairs | âœ— | âœ“ verified positive example | Labeled negative example | < 0.30 |
| A22 â€” algorithm / math reference | CODE_Q only (Type 21 rules) | âœ— | âœ“ CODE_Q algorithm pairs | â€” | research_fact |
| A23 â€” localized user guides | A-side prompts in original language | âœ“ | âœ— | â€” | mirror rules |
| A24 â€” template / boilerplate | structural completeness | âœ— | âœ“ required field contract | Missing field variant | 0.10â€“0.20 |

**B-series families (B01â€“B12) â€” pending registry v11.**

The extended scan identified twelve additional artifact families (serialized delivery
parts, skills factory registries, naming/mapping tables, validation reports, conflict
analysis, DoD documents, prompt pattern libraries, DNA patterns, README indexes,
and miscellaneous state files) that are not yet explicitly covered. These will be
added when registry v11 is available. Minimum expected entries:

| Family | Expected pair type | Expected pair_kind |
|---|---|---|
| B03 â€” skills factory registry | knowledge-edge detection | `knowledge_edge_detection` |
| B05 â€” naming / mapping tables | naming translation pairs | `naming_translation` |
| B06 â€” validation reports | arbiter verdict pairs | `arbiter` |
| B07 â€” conflict analysis | ARCH Type D trade-off | `flow_node` |

Until registry v11 is published: B-series documents encountered during extraction
should be classified provisionally under the closest U-series or A-series family,
with a `pending_b_series: true` field in the pair file to enable retroactive
reclassification when the rules are published.

---



1. Write `PromptRoutingInputCompilerUnit` station pairs. No upstream dependency.
2. Write `MultiSubPhaseRouterUnit` station pairs from compiler B outputs.
3. Write knowledge-edge detection station pairs. Include at least one registered-flow comparison.
4. Write flow factory selection station pairs. Include all four trust-state cases.
5. Write Layer 0-F `OrchestrationRoutingFlow` acceptance pairs from real station outputs.
6. Only after Layer 0-F exists, evaluate routing promotion; otherwise record `PAIRSET_READY_ROUTING_FLOW_PENDING`.
7. Write `LogicalTransitionOrchestrator` pairs.
8. Write Node 01 rows from selected transitions.
9. Write Nodes 02â€“09 from real upstream outputs only.
10. Write teaching unit rows after pair files exist.

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
migration_plan          â† U03 â€” migration/translation plans
portability_invariant   â† U07 â€” portability protocol checks
tooling_contract        â† U11 â€” agent tool-calling contracts
skill_boundary          â† U05 â€” skill library domain boundary
research_fact           â† U02 â€” external research CODE_Q pairs
gap_remediation         â† U14 â€” gap remediation and fix documents
topology_graph          â† A13 â€” diagram / topology source files
example_catalog         â† A21 â€” positive/negative example catalogs
naming_translation      â† naming/mapping tables such as ROSETTA_STONE and Figma-to-CSS
                          mapping matrices. chosen = current name/term, rejected = old or
                          wrong name. Lowest overlap class in the corpus: 0.05â€“0.15.
design_simulation       â† E01/F09 design simulation and graph-extension artifacts
session_round_contract  â† D01-D04/E03/F16 per-session round/load contracts
patch_addendum          â† D06/E16/F05 patch, delta, and replan artifacts
state_reconciliation    â† F08 reconciliation state documents
package_verification    â† F15/G06-G11 package and archive verification artifacts
requirements_traceability â† F12/G05 validation matrix rows
skill_recovery          â† F01 recovered missing skill files
external_structural_pattern â† Z50-Z56 external structural sources only
runtime_evidence        â† G01-G04 runtime output and handoff artifacts
archive_manifest        â† G06-G11 archive bundle manifests
named_session_artifact  â† H01-H08 named session document families
data_catalog_evolution  â† I01 versioned task-type catalogs
rule_stress_test        â† I02/I03 CF and ST rule families
merged_canonical_document â† I04 merged canonical document family
session_type_taxonomy   â† I05/I13/I14 session classification outputs
role_execution_plan     â† I08 architect behavior execution plans
visual_proof_plan       â† I10 visual proof plans
run_instruction_input   â† K01 master run instruction files
active_experiment_result â† J05 active experiment result JSON
teaching_cycle_package  â† J09 teaching cycle package structure
bash_view_path_source   â† K01-K13 bash/view-discovered file families
drawio_topology_graph   â† K07 DrawIO XML topology graphs
component_debug_history â† K08 component-scoped CSS debug histories
```

`correction_event` is a new kind for pairs extracted from Luba correction events
in XIIGen session documents. It is also used for U23 generated-code debug correction
histories and A19 generated UI code outputs â€” the extraction pattern is identical,
the domain differs.

**Cross-document navigation:** This document covers layer structure, required fields,
source family rules, and batch acceptance gates. For worked examples, confusion tables,
annotation rules, and pair type guidance (Types Aâ€“K), use the DPO pair authoring
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
| `L01-AG06` | family-specific DTO fields from the companion registry; especially machine-readable JSON/YAML/JSONL fields, project/adoption status, canonical NestJS anchor, experiment subtype fields, AF simulation/verdict fields, and AG renumbering/intent fields | broad label or path/header/count used without concrete DTO |

If any family-specific field is missing, record `family_dto_missing` and stop.

---

## Failure Modes

**EM-1 â€” Contract Not Named.** Correction: add the interface signature before A.

**EM-2 â€” B Describes Output.** Correction: rewrite B as the runtime output object.

**EM-3 â€” Transition Replaces Flow Nodes.** Correction: restore nine-node rows.

**EM-4 â€” Downstream Shape Invented.** Correction: generate from real upstream outputs.

**EM-5 â€” Corpus Teaches Topic But Not Orchestration.** Correction: map to contract
signature at the point where the fact affects orchestration.

**EM-6 â€” Adaptive Routing Layer Missing.** Correction: produce Layer 0-A/B/C/D
station pairs and Layer 0-F `OrchestrationRoutingFlow` acceptance pairs. Without
Layer 0-F, routing remains `PAIRSET_READY_ROUTING_FLOW_PENDING`.

**EM-7 â€” Knowledge Edge Has No Registered Comparison.** Correction: add registered-flow
comparison pair per batch.

**EM-8 â€” Factory Selection Missing Rejected-Unit Case.** Correction: add the
rejected-unit case to every batch with factory selection pairs.

**EM-9 â€” Router B Uses Invented Fields.** Correction: use only actual
`SubPhaseRoutingDecision` fields.

**EM-10 â€” Rejected Uses Forbidden Template.** Correction: rewrite as Type A, B,
or C with [WRONG: ...].

**EM-11 â€” Rejected Has No Annotation.** Correction: add annotation naming the
specific confusion.

**EM-12 â€” Stage 9 Corruption Cases Missing.** Correction: add all three kinds.

**EM-13 â€” TrainingText Uses Raw Prompt.** Correction: rebuild TrainingText using
compiledText as promptSide. Raw prompt in TrainingText = rejected at measurement gate.

**EM-14 â€” Annotation Is Generic.** The [WRONG: ...] block says only "this is
incorrect" or "wrong approach" without naming the confusion. Correction: name
which sub-phase the rejected incorrectly addresses and why that sub-phase does
not own this responsibility.

**EM-15 â€” Reward Margin Negative Without Diagnosis.** Batch committed with
negative reward_margin_mean and no diagnosis entry in the ledger. Correction:
run the diagnosis table, identify the cause, fix the pairs, resubmit.

**EM-16 â€” PLAN Prompt Is Abstract Meta-Commentary.** The PLAN prompt contains
no named artifacts, no dependency language, and no technical specifics. Correction:
replace with a prompt from a XIIGen session that contains at least one named
component and one explicit dependency statement.

**EM-17 â€” Correction Event Quality Not Assessed.** Pairs extracted from correction
events were not rated High/Medium/Low before inclusion. Correction: rate each
correction event and exclude Low quality events (phrasing-only corrections where
chosen and rejected share the same technical content).

**EM-18 â€” Research Document Used as ARCH or PLAN Authority.** A pair uses a
deep research report (U02) as the B-side authority for an ARCH decision or PLAN
ordering. Research documents describe the external world and may become outdated.
Correction: use research documents only for CODE_Q external-standard pairs. For
ARCH/PLAN, use first-party decision documents (DECISIONS-LOCKED.md, session state).

**EM-19 â€” Migration Plan Used Without Zone Classification.** Pairs extracted from
a translation or migration plan (U03) without first labelling each section as
ordering content, state evidence, or A-side prompts. Correction: apply the three-zone
rule before extraction. Ordering sections â†’ PLAN Type B. State JSON â†’ Layer 3
evidence. Developer prompts â†’ A-side only if in natural developer language.

**EM-20 â€” Source Archive or Merged Dump Used Without Preprocessing.** Pairs
extracted directly from a ZIP file (U10) or unsegmented text dump (U13) without
unpacking and segment classification. These sources mix contexts from multiple
conversations. Correction: apply the preprocessing protocol â€” unpack, segment,
classify each segment against its family rules, then extract.

**EM-21 â€” Portability Invariant Pairs Missing Invariant Number.** A portability
protocol pair (U07) names a violated constraint but does not cite the invariant
by number or name from the protocol document. The annotation says "portability
violated" without naming the specific check. Correction: every portability pair
annotation must name the specific invariant: "portability invariant 3 (no literal
tenant ID in route definitions) is violated."

**EM-22 â€” Agent Tooling Pair Tested as Behavioral Rule.** A pair from an agent
tooling document (U11 â€” AGENTS.md, CLAUDE.md) was written as a behavioral rule
pair (Type 14 pattern) rather than as a tool-sequence pair. The rejected describes
a human-authored document without Gate B rather than an agent skipping a required
tool call. Correction: U11 pairs test agent runtime behavior â€” chosen = correct
tool sequence, rejected = missing or wrong-order tool call. Use Type 14 pattern
only for human-authored output rules.

**EM-23 â€” Gap Remediation Pair Uses Tracking Label as A-side.** The A-side for a
U14 gap remediation pair contains a gap tracking label (`"GAP-007: E18 KNN
unreachable"`) rather than a developer-intent prompt. Correction: convert to
developer-language before use â€” `"fix the E18 KNN path â€” DomainClassifierUnit._trained
is always false"`. Gap IDs and tracking codes are B-side provenance, not A-side input.

**EM-24 â€” Mirror Document Used Without Canonicality Check.** A pair was built from
a document found in a worktree mirror path (U17 â€” `.claude/worktrees`, duplicate
folder) without verifying it is the canonical version. Correction: check the document
state register before using any document. If the register marks this path as a mirror
or extracted copy, use the canonical source instead. Pairs from non-canonical copies
may contradict canonical decisions silently.

**EM-25 â€” External Training Batch Mixed Into Internal Registry.** Pairs from an
external source batch (U22 â€” `external-sources-training-sets`, wave folders) were
registered as internal pairs without the `source_family: external` label. External
source pairs carry external authority â€” a Wave 1 security pair is authoritative for
protocol facts but not for XIIGen architecture decisions. Correction: all pairs from
U22 must remain in the external registry under EXTERNAL-SOURCE-REGISTRY rules. Never
re-register as internal without explicit authority transfer.

**EM-26 â€” Generated Code Used as A-side.** A pair from U23 (generated code debug
history) uses the generated code itself as the A-side prompt. The generated code is
the artifact, not the task that requested it. Correction: A-side must be the task
description or design spec that produced the generation request â€” in developer-language.

**EM-27 â€” Dry-Run Expected Trace Treated as Actual Measurement.** A pair from U20
(benchmark/dry-run) uses a `DRY-RUN` or `EXPECTED-TRACE` document as if it were
a real run result. Correction: mark pairs from Zone B (expected traces, plan documents)
with `coverage_status: expected_not_measured`. Do not use expected metrics as chosen
content for ModelTrainingUnit pairs â€” use only actual measured values from Zone A.

**EM-28 â€” Prompt Template Used as Developer A-side.** A U21 prompt template
(Zone B â€” `PROMPTS.md`, `CLAUDE-CODE-SESSION-PROMPT`) was used directly as a
developer-intent A-side. Prompt templates are vocabulary authority for compiler
pairs, not developer language. Correction: use Zone C (agent execution prompts
written in developer-task framing) as A-sides, subject to Type 1 eligibility.
Zone B prompt templates go to B-side vocabulary for Layer 0-A compiler pairs only.

**EM-29 â€” Canonicality Not Recorded.** A pair file is missing
`source_canonicality` or `canonical_confirmed_via` fields. Without canonicality
metadata the submission gate cannot verify the source is valid. Correction: add
both fields to every pair file. Valid values for `source_canonicality` at
submission: `canonical` or `extracted_copy`. All other values are gate failures.

**EM-30 â€” External Pairs Mixed Into Internal Batch.** A batch file contains pairs
with `source_canonicality: external` alongside internal pairs. Correction: external
and internal pairs must be in separate batch files. Set `mixed_with_internal: false`
in all external pair files. Any batch where `mixed_with_internal: true` is a hard
gate failure that requires the batch to be split before resubmission.

**EM-31 â€” Test File Failing Test Used as Chosen Content.** A pair from A15 (test
suite source files) uses a test that is currently failing in the codebase as the
acceptance criterion for chosen content. Correction: a failing test is a gap (Type 23
material), not ground truth. Only passing tests are valid A15 chosen sources.
Verify against live test run or document state register before use.

**EM-32 â€” Example Catalog Label Not Verified.** A pair from A21 (positive/negative
example catalogs) uses a "positive" example that was correct when written but is
now superseded by a later locked decision. Correction: Gate 1 (label verification)
is mandatory â€” check positive examples against current Type 9 decisions and document
state register. Stale positive examples are Type 16 rejected-vocabulary sources only.

**EM-33 â€” Diagram Source Text Used as A-Side.** A13 Mermaid or PlantUML diagram
source syntax was included in the A-side prompt. Correction: diagram source files
are topology evidence for B-sides. The A-side must be a developer-language question
("which unit receives the compiler's output?"). The diagram DSL is the B-side
authority, not the input.

**EM-34 â€” Generated Code Output Used as Architecture Authority.** An A19 generated
HTML or CSS file was used as the B-side specification for a UI component boundary
pair. Correction: generated code is not architecture truth â€” it is a Type 24 source
(before-debug artifact). Use the design spec (U08 Zone B) or Type 26 source contract
for architecture authority.

**EM-35 â€” Template Placeholder Included in Pair Content.** A pair from A24 (template
libraries) includes an unreplaced placeholder token (`{{FLOW_ID}}`, `{{TIER}}`) in
chosen or rejected content. Correction: resolve all placeholders before extraction.
Placeholder tokens produce nonsense tokens in the compiled training text.

**EM-36 â€” Vendored Codebase Used as Internal Authority Without Adoption Record.**
A pair uses content from an A18 vendored third-party codebase (e.g.,
`claude-useful-skills-master`) as B-side authority for a XIIGen ARCH or PLAN pair
without citing a Type 9 locked decision that explicitly adopted the content.
Correction: vendored codebases are external canonicality. An adoption decision is
mandatory before any vendored content becomes internal B-side authority.

**EM-37 â€” Naming/Mapping Table Pair Uses Wrong pair_kind.** A pair from a naming
table, ROSETTA_STONE, or mapping matrix (the source for `naming_translation` pairs)
was submitted with `pair_kind: migration_plan` or `correction_event`. These are
different pair classes: naming_translation pairs have the lowest overlap in the corpus
(0.05â€“0.15) because old and new names are entirely non-overlapping tokens. Incorrect
pair_kind makes batch diagnostics misleading â€” abnormally low loss from these pairs
will be attributed to the wrong class. Correction: use `pair_kind: naming_translation`
for all pairs where chosen = current name/term and rejected = deprecated name/term.

**EM-38 â€” B-Series Document Used Without Provisional Classification Flag.** A document
from a B-series family (B01â€“B12: serialized parts, skills factory registry, naming tables,
validation reports, conflict analysis, DoD documents, prompt pattern libraries, DNA
patterns) was extracted into a pair without the `pending_b_series: true` flag. Until
registry v11 is published with explicit B-series rules, pairs from these families must
carry this flag to enable retroactive reclassification. Correction: add
`pending_b_series: true` to the pair file and classify provisionally under the closest
U-series or A-series family (B05 naming tables â†’ provisional `naming_translation`;
B06 validation reports â†’ provisional `arbiter`; B07 conflict analysis â†’ provisional
`flow_node`; B03 skills factory â†’ provisional `knowledge_edge_detection`).

**EM-42 â€” Additional Family Classified By Filename Only.** A row from A01-A51,
B13-B43, C01-C12, Z41-Z56, D01-D19, E01-E16, F01-F17, G01-G13, H01-H08,
I01-I14, J01-J09, or K01-K13 lacks
the family-specific DTO fields. Correction: stop extraction, fill source_locator
and required family fields, then rebuild the row.

**EM-43 â€” External Structural Source Used As XIIGen Authority.** A Z50-Z56 row uses
third-party content as XIIGen-specific B-side authority. Correction: set
`source_canonicality=external`, `structural_pattern_only=true`, or exclude.

**EM-44 â€” Missing Skill Used Before Recovery.** An F01 missing-skill row infers
behavior from a session mention or skill name. Correction: recover the SKILL.md
content and cite activation rule plus skill boundary before authoring.

**EM-45 â€” Archive Bundle Used Without Member Classification.** A Z41-Z49 or G06-G11
archive is treated as a document. Correction: extract, verify manifest, classify
each member, and use member-level source paths.

**EM-46 â€” Session Analysis Artifact Merges Rounds.** A D/E/F session artifact row
combines several rounds or phases. Correction: split by `session_id`, `flow_id`,
`phase_id`, and `round_id`.

**EM-47 â€” Runtime Evidence Confused With DPO Metrics.** A G01-G04 runtime artifact
is measured as if it were DPO training output, or DPO metrics are used as product
runtime evidence. Correction: record runtime source id, command id, flow/phase id.

**EM-48 â€” Validation Matrix Row Missing Covered-By Evidence.** A G05 validation
matrix row claims PASS without a `Covered By` artifact. Correction: use
`requirements_traceability` with `requirement_id`, `covered_by_artifact`, and
coverage status; otherwise rejected is PASS-without-evidence.

**EM-49 â€” Named Session Document Collapsed Into Broad Type.** An H01-H08 artifact is
classified as generic Type 29/32/34 without named artifact kind, scope, or version.
Correction: record `named_artifact_kind`, target type, and artifact-specific ids.

**EM-50 â€” Session Message Output Used Without Locator.** An I01-I14 source was found
inside thinking blocks, tool calls, or assistant-generated structured output, but
the row records only a filename-like label. Correction: record `message_source_kind`,
`thinking_or_tool_locator`, and concrete artifact family before extraction.

**EM-51 â€” CF And ST Rule Families Mixed.** A BFA CF rule and a stress-test ST rule
are treated as the same family. Correction: record `rule_family=CF` for I02 and
`rule_family=ST` for I03; link ST rows to their CF rule where applicable.

**EM-52 â€” Merged Canonical Document Used Without Merge Provenance.** An I04 MERGED
document is used as canonical without `source_versions` or continuity check.
Correction: record merge sources, merge phase, and `continuity_check_passed=true`.

**EM-53 â€” Active Experiment Result Missing Measurement DTO.** A J05 experiment run
result row omits baseline/enhanced/improvement/next_action fields. Correction:
rebuild from the JSON result, not from a prose summary.

**EM-54 â€” Teaching Cycle Package Flattened Into Text.** A J09 teaching cycle package
is treated as plain documentation. Correction: preserve cycle id, layer id, ledger
row, held-out kind, and checkpoint path.

**EM-55 â€” Bash/View Path Mention Used As Authority.** A K01-K13 artifact was only
seen in a bash command or view path, but the row lacks exact path and runtime use.
Correction: record the path, loaded/opened context, and family-specific DTO fields.

**EM-56 â€” DrawIO Diagram Not Parsed As Graph.** A K07 DrawIO XML file is treated as
visual reference only. Correction: parse nodes and edges, then use
`drawio_topology_graph`.

**EM-57 â€” Component CSS Debug History Missing Component Scope.** A K08 row omits
component id or corrected/wrong revision ids. Correction: record component-scoped
debug DTO before creating Type 24 pair.

**EM-58 â€” Companion Family Collapsed After K-Series.** A source from L01-AG06 is
classified by broad label only: "skill file", "experiment plan", "ZIP", "state", or
"reference doc". Correction: apply the exact companion family DTO from
`XIIGEN-INTERNAL-SOURCE-REGISTRY-ADDITIONAL-TYPES-v1.md` before extraction.

**EM-59 â€” Machine-Readable Evidence Not Parsed.** A JSON/YAML/JSONL source from M, O,
T, or W is summarized from prose/filename instead of parsed. Correction: extract the
required DTO fields directly from the structured source.

**EM-60 â€” Project Boundary Missing.** An llm_mvp_core S/U/W source is used as XIIGen
authority without `project_id` and adoption status. Correction: keep it project-scoped
or cite adoption.

**EM-61 â€” Canonical NestJS Anchor Ignored.** A V01-V08 conflict chooses older
Python/FLOW33 material over `xiigen-COMPLETE.zip`. Correction: mark old source stale
and use the NestJS canonical source.

---

## Acceptance Checklist

- [ ] Every row names a contract signature.
- [ ] Every B side is an actual runtime output object.
- [ ] CANONICALITY GATE â€” check this first; if failed, stop and do not apply any other check.
  source_canonicality field present in every pair file: canonical or extracted_copy only.
  mirror / stale / archive_container / secure_operational â†’ STOP. Exclude entirely.
    Record rejection reason as "canonicality: {class}" â€” not "failed overlap" or other.
    This ensures accurate diagnosis: a mirror source and a high-overlap pair are different problems.
  external â†’ STOP. Must be in a separate external-only batch file; mixed_with_internal: false required.
- [ ] canonical_confirmed_via field present â€” names the document state register or
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
- [ ] Node 03 rows express `depends_on` relationships â€” not just a step list.
- [ ] Node 03 pairs use Type B rejected with annotation from the dependency template.
- [ ] Array-node rows do not group multiple items into one A.
- [ ] Node 08 pass rows are silent if runtime pass is silent.
- [ ] Teaching rows include all resource measurement fields.
- [ ] No rejected response uses the forbidden negation template or any variant.
- [ ] Every rejected response includes a [WRONG: ...] annotation.
- [ ] Annotation names the specific sub-phase confusion â€” not a generic statement.
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
- [ ] U02 (research) sources used only for CODE_Q pairs â€” not for ARCH or PLAN.
- [ ] U03 (migration) sources zone-classified before extraction â€” ordering / state / prompts.
- [ ] U07 (portability) pairs cite the specific invariant number in the annotation.
- [ ] U10 (archives) and U13 (merged dumps) unpacked/segmented before any extraction.
- [ ] U11 (agent tooling) pairs test tool-call sequence â€” not human-authored rule compliance.
- [ ] U04 and U08 sources used as A-side prompts only â€” never as B-side authority.
- [ ] For U14 (gap remediation): A-side is developer-language task prompt â€” not a gap
  tracking label. Wrong-fix entries rated High/Medium/Low before use (same as Type 3).
  False-pass entries used as ArbiterUnit pairs â€” chosen = correctly-failing output.
- [ ] For U15 (visual UI quality): zone identified before pair construction.
  Zone A (visual findings) â†’ Type 8 ArbiterUnit rules.
  Zone B (role/screen maps) â†’ Type 5 topology rules for UI sub-phases.
  Zone C (design-system analysis) â†’ Type 2 ARCH rules for 1.1 sub-phase.
- [ ] For U16 (flow guide library): zone identified before pair construction.
  Zone A (guide steps) â†’ Type 10 PLAN Type B rules.
  Zone C (index files) â†’ provenance only, not a pair source.
- [ ] For U17 (worktree mirrors): canonicality verified against document state register
  before use. Pairs from non-canonical copies excluded.
- [ ] For U18 (certificates/env): excluded entirely â€” not inspected or extracted.
- [ ] For U19 (repo snapshots): unpacked and reclassified by content type before use.
  Snapshot filename not used as document title in any A-side.
- [ ] For U20 (benchmark/dry-run): zone identified.
  Zone A (actual metrics) â†’ Type 4 rules.
  Zone B (expected traces) â†’ marked expected_not_measured, not actual measurement.
  Zone C (dry-run discrepancies) â†’ discrepancy pair with expected vs actual.
- [ ] For U21 (prompt handoff): zone identified.
  Zone A (handoff contracts) â†’ Type 19 agent tooling rules.
  Zone B (prompt templates) â†’ B-side vocabulary for Layer 0-A only, not A-sides.
  Zone C (agent execution prompts) â†’ Type 1 eligibility test applied first.
- [ ] For U22 (external training sets): not mixed into internal registry.
  source_family: external label present. External registry authority applies.
- [ ] For U23 (generated code debug): A-side is task description or design spec â€”
  not the generated code artifact itself. Correction entries rated High/Medium/Low.
- [ ] For U24 (marketplace/legal): excluded unless session contains a DOCUMENT-predicate
  task prompt about preparing marketplace material (then A-side only).
- [ ] For A13 (diagram/topology files): pair uses node/edge vocabulary from parsed graph â€”
  not raw DSL syntax. A-side is developer-language question. Three-check rule confirms current version.
- [ ] For A14 (debug logs): normalization applied â€” command, error, resolution extracted.
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
  A-side is the generation request â€” not the generated file.
- [ ] For A20 (before/after visuals): before/after direction preserved in pair metadata.
  Pair content is verbal defect description â€” binary images not in DPO text.
- [ ] For A21 (positive/negative catalogs): all three gates applied â€”
  Gate 1: label verified against current Type 9 decisions and document state register
  Gate 2: provenance checked â€” synthetic examples flagged
  Gate 3: overlap test computed; stale positive examples excluded as chosen.
- [ ] For A22 (algorithm/math reference): CODE_Q only. Type 21 rules applied exactly.
  No ARCH or PLAN authority claimed. XIIGen adoption decision cited if applicable.
- [ ] For A23 (localized guides): A-side prompts valid in original language.
  B-side uses canonical English source only. source_canonicality = mirror.
- [ ] For A24 (templates): all placeholders resolved before extraction.
  Required fields verified against template â€” missing-field logic as Type 11a/22a.
- [ ] For B-series families (B01â€“B12 â€” pending registry v11): if encountered, classify
  provisionally under closest U/A-series family and add `pending_b_series: true` to pair file.
  B05 naming tables â†’ `naming_translation`; B06 validation reports â†’ `arbiter`;
  B07 conflict analysis â†’ `flow_node`; B03 skills factory â†’ `knowledge_edge_detection`.
- [ ] For additional registry families A01-A51 / B13-B43 / C01-C12 / Z41-Z56 /
  D01-D19 / E01-E16 / F01-F17 / G01-G13 / H01-H08 / I01-I14 / J01-J09 / K01-K13 /
  L01-L08 / M01-M21 / N01-N09 / O01-O19 / P01-P12 / Q01-Q05 / R01-R13 /
  S01-S14 / T01-T10 / U01-U20 / V01-V08 / W01-W16 / X01-X16 / Y01-Y10 /
  AA01-AA17 / AB01-AB07 / AC01-AC20 / AD01-AD08 / AE01-AE10 / AF01-AF13 /
  AG01-AG06: family-specific DTO fields
  are complete before pair construction.
- [ ] For Z50-Z56: source_canonicality = external and structural_pattern_only = true.
  No XIIGen-specific B-side authority from nested external archives.
- [ ] For F01: recovered SKILL.md exists and activation rule / skill boundary are cited.
- [ ] For G06-G11: archive manifest verified and every member has its own target family.
- [ ] For G05: validation matrix row includes requirement_id, covered_by_artifact,
  and coverage_status.
- [ ] For H01-H08: named_artifact_kind, scope/version, target_type, and evidence_locator
  recorded; no broad Type 29/32/34 label without subtype.

---

## Last Updated

2026-05-08. Version 14.0.
