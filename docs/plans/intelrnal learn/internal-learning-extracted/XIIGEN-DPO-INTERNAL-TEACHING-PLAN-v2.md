# XIIGen Document-Driven DPO Teaching Plan
# Version: 2.0 — 2026-05-08
# Supersedes: XIIGEN-DPO-INTERNAL-TEACHING-PLAN-v1.0
# Authority: ORCHESTRATOR-TEACHING-PLAN-v3.md
#            EXAMINATION-METHOD-ORCHESTRATION-v9.md
#            XIIGEN-INTERNAL-SOURCE-REGISTRY-v1.md
# Calibration basis: run 2026-05-06 — 40 batches, 29 negative margin, loss=0.693

---

## What changed from v1

**Token overlap test added to every phase.** v1 stated pair quality rules but
did not give a concrete pre-submission test. v2 adds the overlap ratio formula
at the point where it must be applied — Phase 3 pair construction — not just
as a policy note.

**Per-source extraction rules added from XIIGEN-INTERNAL-SOURCE-REGISTRY-v1.md.**
Three session row zones (521–727 positive, 1–520 ARCH gap, 1235–1753 ineligible)
and the export bundle ordering are now part of the processing sequence.

**Correction event priority promoted.** v1 listed corrections as Priority 3.
v2 promotes them to first priority — correction events have confirmed overlap
ratio < 0.25 and require no constructed rejected. They are the fastest path
to positive reward margin.

**Diagnosis table expanded.** v1 had 4 rows. v2 has 8 rows covering per-source
symptoms from the internal registry.

**Phase 5 measurement gate made executable.** Added the exact comparison
against the ln(2) = 0.693 threshold and the overlap ratio scan that must run
before committing any batch.

---

## What this plan is

This plan produces DPO pair files from XIIGen session history for the batch
calibration runner (`DefaultLogicalCapabilityTrainer`). It is the internal-first
counterpart to the external source pipeline: external sources provide authority
evidence for B sides; XIIGen documents provide the only authentic A-side language.

This plan does **not** teach one row at a time through live LoRA steps — that is
ORCHESTRATOR-TEACHING-PLAN-v3.md. This plan produces pair files in bulk, measures
batch reward margin, and commits only batches that pass the gate.

**Why calibration run 2026-05-06 failed:**

```
40 batches total.
29 of 40 had negative reward margin (chosen_reward < rejected_reward).
Loss across all batches: 0.693 = ln(2) = random-guess level.
Root cause: rejected was "Do not generate a capability for sub-phase X"
  — overlap ratio ~0.70 with chosen.
  — The model could not distinguish chosen from rejected.
  — Every optimizer step ran but produced no useful gradient.
```

This plan is the direct response to that failure. Every rule traces to a specific
batch in that run.

---

## Hard gates — apply before any pair is written

| Gate | Rule | Calibration run evidence |
|---|---|---|
| Token overlap test | overlap_ratio > 0.60 → do not submit | Negation template overlap ~0.70 → 29 negative batches |
| Forbidden rejected template | "Do not generate a capability for sub-phase X" and all negation variants | 100% of batches used this; 73% had negative margin |
| Annotation required | Every rejected ends with [WRONG: ...] naming the specific confusion | Type B pairs without annotation → overlap ~0.55, borderline |
| One prompt per pair per batch | Same prompt not chosen in one pair and rejected in another | Batch 9: prompt reused, accuracy 0.275 |
| ARCH first | 3.1, 3.2, 3.5, 6.1 had zero ARCH batches — zero training signal | Run analysis: ARCH domain absent |
| Compiled text in TrainingText | compiledText not raw prompt | Near-zero CompiledPredicateIds → near-zero training signal |
| Eligible prompt only | Prompt must contain named artifact OR dependency language OR technical specificity | Batch 30 acc=0.306: meta-commentary, no predicates |

---

## The token overlap test — apply before every pair submission

```
shared = tokens in both chosen and rejected that are NOT also in the prompt
total  = tokens in chosen + tokens in rejected − tokens in prompt
overlap_ratio = shared / total
```

**overlap_ratio > 0.60 → do not submit the pair.**

Expected ratios by pair type:

| Type | Expected overlap | Expected margin |
|---|---|---|
| Type A ARCH — boundary vs implementation | 0.10 – 0.20 | Reliably positive |
| Type A testing — unit vs integration | 0.20 – 0.35 | Reliably positive |
| Type B PLAN — dependency order reversed | 0.35 – 0.50 | Positive with strong annotation |
| Type C — wrong domain | 0.15 – 0.35 | Reliably positive |
| Correction events from XIIGen history | 0.10 – 0.25 | Reliably positive |
| Negation template | 0.65 – 0.80 | Reliably negative |

---

## Processing cycle — 6 steps per session document

```
Step 1. Session scan       → prompts, corrections, dependency statements
Step 2. Eligibility filter → remove ineligible prompts (see rules below)
Step 3. Compiler simulation → derive CompiledPredicateIds per prompt
Step 4. Pair construction  → chosen + rejected + annotation
Step 5. Batch assembly     → group with composition minimums
Step 6. Measurement gate   → verify reward_margin_mean >= 0 before commit
```

Each step has a gate. A step that fails its gate stops processing that session
document until the failure is resolved.

---

## Step 1 — Session document scan

### What to record

```
session_id              — filename or source identifier
source_registry_id      — which entry in XIIGEN-INTERNAL-SOURCE-REGISTRY-v1.md
prompt_count            — all human prompts found
eligible_prompt_count   — prompts passing the eligibility filter
correction_count        — H0/A0/H1/A1 chains found
dependency_statements   — explicit ordering constraints in the session
multi_phase_prompts     — prompts spanning two or more sub-phases
session_row_range       — XIIGEN-SESSION-XXXX-NNNN range if from Source 1
```

### Session row zone assignment (Source 1 only)

Before scanning, assign the session to a zone from the internal registry:

```
Rows 521–727   → POSITIVE ZONE: proven positive margin in calibration run
                 Priority: extract PLAN Type B pairs from here first
                 Expected overlap: 0.15–0.35

Rows 1–520     → ARCH ZONE: ARCHITECTURE vocabulary present but never extracted
                 Priority: extract ARCH Type A pairs (reclassify from BUG label)
                 Expected overlap: 0.10–0.20

Rows 1235–1753 → INELIGIBLE ZONE for PLAN prompts: meta-commentary confirmed
                 Only correction events may be extracted from here
                 Do not use as PLAN or ARCH A sides
```

### Gate 1

Minimum to continue: `eligible_prompt_count + correction_count >= 3`.
A session that fails Gate 1 is logged and skipped. Do not attempt pair construction.

---

## Step 2 — Eligibility filter

### Prompt eligibility for PLAN and ARCH

A prompt must satisfy **at least one**:

```
Named artifact: "Flow 10", "Phase 3", "AuthGuard", "TenantResolver",
                "Factory Interface", specific component or file name

Dependency language: "before", "after", "then write", "once X is complete",
                     "depends on", "when X is in place", "following X"

Technical specificity: predicate vocabulary (jwt, api, architecture, test),
                       sub-phase reference by number or name, technology name
```

### Ineligible prompts — do not use as A sides for any pair type

```
"Option A is clearly the right path"
"this is grounded in the existing documents"
"let's work on the next steps"
"we're making good progress"
"Continue."   "Read this."   "Please load the session."
```

These produce empty or near-empty `CompiledPredicateIds`. TrainingText carries no
routing signal regardless of chosen/rejected quality. Reward margin will be ~0.

### Correction event eligibility — separate from prompt eligibility

Correction events do NOT require the prompt eligibility criteria. The original
prompt (H0) may be abstract or short — the correction signal comes from the
divergence between A0 (wrong) and A1 (correct), not from the prompt vocabulary.

Rate each correction event:
- **High** (include unconditionally): chosen and rejected have clearly different vocabulary
- **Medium** (include with strong annotation): different responsibilities, some shared vocabulary
- **Low** (exclude): phrasing-only correction — same technical content, different words

### Gate 2

For PLAN and ARCH pairs: `eligible_prompt_count >= 2` after filtering.
For correction-only sessions: `correction_count >= 2` with at least 1 High or Medium.

---

## Step 3 — Compiler simulation

Derive `CompiledPredicateIds` for every eligible prompt before writing any pair.

**Nine-predicate lexicon:**

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

**Confidence:** 0.92 when at least one predicate matches. 0.70 when none match.

**Empty predicate result:** A prompt with zero matched predicates produces a
knowledge-edge pair only (not a routing pair). Do not discard it — it is
Stage 9 material.

### TrainingText construction rule

```
TrainingText = "semantic_prompt: {compiledText} branch_result: {branchResult}
                sub_phase: {subPhaseId} {subPhaseName} {subPhasePurpose} {predicateIds}
                semantic_compressed: {compiledText} teaching_vector_60: {vector}"
```

`compiledText` = output of `SemanticTeachingPromptCompiler.Compile()`, not the raw
prompt. This is a hard gate: raw prompt in TrainingText = batch rejected at Step 6
regardless of accuracy.

Verify implementation before processing:
```
grep -n "BuildTrainingTextCore\|compiledText\|promptSide" \
  21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs
Expected: compiledText used as promptSide when not null
```

### Gate 3

For every prompt with non-empty predicates: record the predicate set.
For every prompt with empty predicates: mark as knowledge-edge candidate.
Do not proceed to Step 4 without predicate records for all eligible prompts.

---

## Step 4 — Pair construction

### Source-specific pair type assignment

**From correction events (all sources) — construct first:**

```
H0 (original prompt)  → A side
A0 (Claude's answer)  → rejected
  [WRONG: {H1 correction text reformatted as annotation}]
A1 (corrected answer) → chosen
```

Why correction events first: overlap ratio < 0.25 confirmed. No construction
needed for rejected content — it already exists. These are the fastest path to
positive reward margin.

Token overlap test for correction events: the test still applies. Rate each
event before including. Low-quality corrections (phrasing only, overlap > 0.50)
are excluded.

**From Source 1 rows 1–520 (ARCH zone) — Type A pairs:**

These rows were labelled BUG/PLAN in calibration run but contain ARCHITECTURE
predicate vocabulary. Reclassify rows where the session text discusses component
boundaries, dependency direction, entity shape, or observability strategy.

Chosen: the boundary decision or design rule from the session.
Rejected: use the ARCH confusion table — implementation response for the same prompt.

ARCH confusion table with expected overlap ratios:

| Chosen | Rejected | Expected overlap |
|---|---|---|
| 3.1 Architecture and Component Topology | 4.1 Backend Services (class body) | 0.10–0.20 |
| 3.1 Architecture and Component Topology | 4.4 Refactoring (code extraction) | 0.15–0.25 |
| 3.2 Data Model and Persistence Design | 4.1 Backend Services (repository) | 0.15–0.25 |
| 3.2 Data Model and Persistence Design | 4.5 Integration (adapter) | 0.20–0.30 |
| 3.5 Observability and Operational Design | 4.1 Backend Services (logger wiring) | 0.10–0.20 |
| 3.5 Observability and Operational Design | 7.3 Monitoring (dashboard setup) | 0.10–0.15 |
| 6.1 Manual QA and Acceptance Review | 5.1 Unit Test Design | 0.15–0.25 |
| 6.1 Manual QA and Acceptance Review | 5.3 End-to-End Testing | 0.20–0.30 |

All rows in this table are below 0.30 expected overlap. ARCH Type A pairs are
the most reliable positive-margin pairs from the internal source.

**ARCH annotation template:**

```
[WRONG: "{prompt verb}" asks {what 3.x sub-phase owns} —
not {what rejected sub-phase does}. {What rejected sub-phase does} follows from
{what 3.x decides}. The decision must exist before the implementation can proceed.]
```

**From Source 1 rows 521–727 (PLAN positive zone) — Type B pairs:**

These rows produced batches 16 (acc=0.618) and 17 (acc=0.771). Extend this range.

Prompts in this zone contain named artifacts and dependency language. These are
eligible for PLAN training. Every multi-task prompt (two or more sub-phase signals)
gets a Type B rejected.

Type B construction:
- Chosen: branches in correct dependency order with explicit `depends_on`
- Rejected: same branches in reversed order OR same branches with `depends_on=[]`
- Annotation: explains why the wrong order fails functionally (not just "it's wrong")

**Why annotation quality is critical for Type B:** Chosen and rejected share the
same sub-phase names. The distributional difference comes from `depends_on` field
content and the annotation tokens. Without a functional explanation, Type B pairs
reach overlap ~0.55 — borderline. Strong annotation drops it to ~0.40.

PLAN dependency ordering with "why wrong order fails":

| Correct sequence | Rejected order | Why wrong order fails |
|---|---|---|
| 3.4 Security → 5.2 Integration Tests | 5.2 first with mocks | Mock tokens bypass validation logic — cannot test real isolation |
| 3.1 Architecture → 4.1 Backend Services | 4.1 first, 3.1 later | Implementing without decided boundaries creates coupling to undo |
| 3.2 Data Model → 3.3 API Contract | 3.3 first, model from contract | Contract shape depends on model shape — reversed leaks implementation into public API |
| 3.5 Observability Design → 7.3 Monitoring | 7.3 first, design retroactively | Cannot configure collection of signals that have not been defined |

**From Source 1 rows 1235–1753 (ineligible zone) — corrections only:**

Do not use any prompt from this range as an A side for PLAN or ARCH pairs.
The prompts are meta-commentary — confirmed negative margin in batch 30.

Only correction events (H0/A0/H1/A1 chains) from this range may be used.

**From Source 2 export — correction events from bundles:**

Processing order from bundle map:
```
1. legacy-docs-skills-from-old-code  (35 corrections, ARCH/CODE_Q)
2. figma-to-code-skills-to-xiigen    (69 corrections, PLAN/ARCH)
3. self-learning-cycle-validation    (35 corrections, PLAN/QA)
4. flow-01-station-testing           (79 corrections, BUG/IMPLEMENT)
5. session-prompt-and-learning-protocol (71 corrections, PLAN/CODE_Q)
6. value-proposition-code-gaps       (104 corrections, ARCH/BUG)
```

Each conversation must be extracted in streaming mode before pair construction
(conversations.json is ~1.1 GB — full-file load is forbidden).

**CODE_Q pairs — Type C rejected required:**

CODE_Q had 0 of 4 batches with positive margin. Every batch had overlap ~0.70
(negation template). For CODE_Q pairs, rejected must be an action response
(BUG fix or PLAN step), not another explanation.

```
CHOSEN — CODE_Q: explains how X works
REJECTED — PLAN or BUG: implements or fixes X
[WRONG: prompt asks "how does X work" — CODE_Q scope.
Providing a fix when an explanation was asked treats the prompt as a
BUG report it is not.]
```

### Gate 4 — token overlap check

Before adding any pair to the batch:

```
□ Calculate overlap_ratio
□ overlap_ratio > 0.60 → revise rejected or discard pair
□ [WRONG: ...] annotation is specific — names the sub-phase confusion
□ For Type B: annotation explains why wrong order fails (not just states it)
□ For correction events: quality rated High or Medium (not Low)
```

---

## Step 5 — Batch assembly

### Composition minimums

| Pair type | Minimum | When required |
|---|---|---|
| Type A (adjacent sub-phase) | 40% | Always |
| Type B (wrong dependency order) | 20% | Any multi-task prompt in the batch |
| Type C (wrong domain) | 30% | Every domain present in the batch |
| Stage 9 corruption rows | 1 of each kind | Every batch |

### One-prompt-per-batch rule

Scan all pairs in the assembled batch. No prompt string may appear as A side
in more than one pair. If duplicates exist:
- Keep the pair where chosen and rejected have lowest overlap_ratio
- Discard the others

### Stage 9 corruption rows — three required every batch

**wrong-predicate-assignment** — authority: predicate lexicon

Find a session prompt where a noun modifier mentions a predicate term while the
primary verb triggers a different predicate. Correct assigns both; corrupted
assigns only the noun modifier's predicate.

```
Prompt: "test coverage shows 42% on the authentication module"
Correct:   CompiledPredicateIds: ["TEST", "SECURITY"]
Corrupted: CompiledPredicateIds: ["SECURITY", "AUTHORIZATION"]
[WRONG: "test coverage" triggers TEST as primary verb signal.
"authentication module" is the object being measured.
TEST takes priority when the primary verb is coverage measurement.]
```

**ambiguous-predicate-case** — authority: router output structure

Find a session prompt where two different predicates each independently justify
a different sub-phase. Correct routes both; corrupted routes only the
higher-confidence one.

```
Prompt: "add authorization to the API endpoint"
Correct:   Routes to 3.4 (SECURITY) AND 3.3 (API_CONTRACT) — two decisions.
Corrupted: Routes only to 3.4.
[WRONG: API_CONTRACT predicate from "API endpoint" is present.
When multiple predicates match different sub-phases, all must be routed.
Dropping one suppresses valid routing signal for 3.3.]
```

**knowledge-edge-suppressed** — authority: KNOWLEDGE_EDGE result structure

Any session prompt routing to an unregistered sub-phase (all sub-phases except 3.4).
Correct shows KNOWLEDGE_EDGE with all six metrics; corrupted shows PASS.

```
Prompt: routes to 5.2 Integration Tests — no flow registered.
Correct:   OverallVerdict: "KNOWLEDGE_EDGE"
           Metrics: knowledge_edge=True, missing_sub_phase=5.2, missing_flow=True,
                    routed_phase_id=5, routing_confidence="0.84",
                    knowledge_edge_reason="no logical flow registered for routed sub-phase"
Corrupted: OverallVerdict: "PASS" — knowledge edge suppressed.
[WRONG: PASS fabricates a result the system cannot produce for this sub-phase.
The required output is KNOWLEDGE_EDGE with all six metrics.]
```

---

## Step 6 — Pre-commit measurement gate

Before committing any batch to the ledger, verify three metrics.

### Metric 1 — Reward margin

```
reward_margin_mean = mean(ChosenReward − RejectedReward) across all DPO steps
```

**Rule: reward_margin_mean < 0 → reject batch. Do not commit.**

Run the diagnosis table before resubmitting.

### Metric 2 — Loss vs random-guess floor

```
loss_mean across batch
```

**Rule: loss_mean ≈ 0.693 (= ln(2)) → no learning is occurring.**

Loss at ln(2) means the model is at random-guess level for the batch.
Even if reward_margin_mean is slightly positive, a batch with loss near 0.693
indicates the pairs are too weakly separated for the model to learn from.
Run overlap scan (Metric 3) to identify which pairs are causing this.

### Metric 3 — Overlap scan

After a batch fails Metrics 1 or 2, scan every pair:

```
For each pair in batch:
  compute overlap_ratio
  if overlap_ratio > 0.50: flag as suspect
  if overlap_ratio > 0.60: mark as discard
```

Report the flagged pairs with their overlap ratios. This identifies which
specific pairs caused the negative margin — not just that the batch failed.

### Diagnosis table

| Symptom | Source zone | Likely cause | Fix |
|---|---|---|---|
| All pairs negative margin | Any | Negation template in rejected | Rewrite all rejected as Type A, B, or C |
| All pairs near-zero margin, loss ~0.693 | Source 1 rows 1235–1753 | Meta-commentary prompts used | Replace with eligible prompts or use corrections only from this zone |
| Type B pairs negative | Source 1, 5 | Annotation only states wrong order | Add why wrong order fails functionally |
| ARCH pairs missing from batch | Source 1 rows 1–520 | Rows labelled BUG/PLAN, not reclassified | Reclassify ARCHITECTURE-predicate rows as ARCH domain |
| Correction events have low margin | Source 2 | Low-quality corrections included | Re-rate; exclude phrasing-only (Low) corrections |
| CODE_Q pairs all negative | Any | Rejected is another explanation | Rejected must be action response: BUG fix or PLAN step |
| Corruption rows positive but real pairs negative | Any | Real pairs weaker than corruption pairs | Apply overlap scan to all real pairs; rebuild weakest |
| All pairs near-zero, TrainingText issue | Any | Raw prompt in TrainingText | Verify compiledText used as promptSide — rerun compiler simulation |

---

## Session ledger — one entry per accepted batch

**Location:**
```
C:\Users\lubao\Documents\xiigen\llm mvp\teaching-ledger\
  dpo-internal-{date}.jsonl
```

**Required fields:**

```json
{
  "batch_id": "xiigen-dpo-001-arch-3.1",
  "source_registry_id": "source-1-arch-zone",
  "source_session_rows": "1-520",
  "source_bundle": null,
  "domain": "ARCH",
  "sub_phases": ["3.1"],
  "row_count": 8,
  "pair_types": {"A": 4, "B": 2, "C": 2},
  "correction_events_included": 2,
  "stage9_corruption": {
    "wrong_predicate": 1,
    "ambiguous_predicate": 1,
    "knowledge_edge_suppressed": 1
  },
  "overlap_scan": {
    "max_overlap_ratio": 0.28,
    "pairs_above_0.50": 0,
    "pairs_above_0.60": 0
  },
  "reward_margin_mean": 0.0014,
  "loss_mean": 0.641,
  "accuracy": 0.581,
  "verdict": "ACCEPT_BATCH",
  "compiled_prompt_in_training": true,
  "annotation_quality": "specific"
}
```

`compiled_prompt_in_training: false` → batch rejected regardless of accuracy.
`annotation_quality: "generic"` → batch rejected: generic [WRONG: ...] blocks
do not provide sufficient distributional separation for Type B pairs.

---

## Processing priority order

This order is derived from calibration run data and internal registry zone analysis.

### Priority 1 — Correction events from Source 2 export (all bundles)

Expected overlap ratio: 0.10–0.25. No construction needed for rejected content.
Ground truth from real human corrections. Fastest path to positive reward margin.

```
Bundle order:
1. legacy-docs-skills-from-old-code  (35 corrections — ARCH/CODE_Q)
2. figma-to-code-skills-to-xiigen    (69 corrections — PLAN/ARCH)
3. flow-01-station-testing           (79 corrections — BUG/IMPLEMENT)
4. value-proposition-code-gaps       (104 corrections — ARCH/BUG)
5. session-prompt-and-learning-protocol (71 corrections — PLAN/CODE_Q)
6. self-learning-cycle-validation    (35 corrections — PLAN/QA)
```

Minimum before moving to Priority 2: 20 correction pairs with reward_margin_mean >= 0.

### Priority 2 — ARCH Type A from Source 1 rows 1–520 (reclassify)

Expected overlap ratio: 0.10–0.20. Zero ARCH training signal in calibration run.
ARCHITECTURE predicate vocabulary confirmed present in rows 1–520 but never
used for ARCH pairs.

Extract: session text discussing component boundaries, dependency direction,
entity shape, observability strategy. Reclassify as ARCH domain. Construct
Type A rejected using ARCH confusion table.

Minimum before moving to Priority 3: 20 ARCH pairs across at least 3 of 4
ARCH sub-phases, reward_margin_mean >= 0 confirmed.

### Priority 3 — PLAN Type B from Source 1 rows 521–727 (extend positive zone)

Expected overlap ratio: 0.15–0.35. Batches 16 and 17 came from this zone.
Extend the positive-margin range with more Type B pairs from these rows.

Extract: prompts with named artifacts and dependency language. Every multi-task
prompt gets a Type B rejected with functional annotation.

Minimum before moving to Priority 4: 15 Type B PLAN pairs with
reward_margin_mean >= 0 confirmed.

### Priority 4 — CODE_Q Type C from Source 2 export

Expected overlap ratio: 0.15–0.30. Zero positive margin in calibration run.
Rejected must be action response (BUG fix or PLAN step), not another explanation.

Target bundles: `legacy-docs-skills-from-old-code`,
`session-prompt-and-learning-protocol`.

### Priority 5 — Layer 0 adaptive routing pairs

Sources 1 and 3. Compiler (Layer 0-A) and router (Layer 0-B) pairs.
Required before the adaptive orchestrator can train.

---

## What this plan does not do

**Does not teach live LoRA.** ORCHESTRATOR-TEACHING-PLAN-v3.md handles that.
This plan produces batch DPO pair files.

**Does not invent prompts.** Every A side comes from a real session document or
correction event. Synthetic prompts are not permitted.

**Does not process ineligible sessions.** A session containing only
meta-commentary, status updates, or abstract planning with no named artifacts
is logged as ineligible and skipped.

**Does not accept batches with loss near 0.693.** Loss at ln(2) means
random-guess level. Even a technically positive reward_margin_mean on a
loss=0.693 batch indicates the signal is too weak. Run the overlap scan.

---

## Pre-condition check

Before processing any session document:

```
PC-1: compiledText used as promptSide in DefaultLogicalCapabilityTrainer
      grep -n "BuildTrainingTextCore\|compiledText\|promptSide" \
        21.AgentInfrastructure/Logical/DefaultLogicalCapabilityTrainer.cs
      Expected: compiledText != null ? compiledText : CompileForTrainingText(...)

PC-2: Forbidden template absent from all existing pair files
      grep -r "Do not generate a capability" \
        external-sources-training-sets/
      Expected: 0 matches

PC-3: conversations.json stream access verified (Source 2 only)
      Test: open zip, read first conversation in streaming mode without
      loading full file. If streaming not available: extract to temp,
      process one conversation at a time, delete temp.

PC-4: Internal source registry loaded
      XIIGEN-INTERNAL-SOURCE-REGISTRY-v1.md — confirm zone assignments
      for Source 1 session rows before scanning begins

PC-5: Teaching ledger directory exists
      C:\Users\lubao\Documents\xiigen\llm mvp\teaching-ledger\
      Expected: directory present (create if absent)
```

---

## Success metrics

This plan produces pair files, not unit tests. Success is measured by:

| Metric | Minimum acceptable | Target |
|---|---|---|
| Batches with reward_margin_mean >= 0 | 60% of all batches | 80% |
| Loss moving below 0.693 across sessions | Mean loss < 0.660 after 10 batches | Mean loss < 0.620 |
| ARCH sub-phases with at least one batch | 3 of 4 (3.1, 3.2, 3.5, 6.1) | 4 of 4 |
| PLAN batches with positive margin | 4 of 6 | 5 of 6 |
| Correction-derived pairs | 20 | 50 |
| Batches with overlap_max < 0.50 | 80% | 95% |
| Batches with compiled_prompt_in_training=true | 100% | 100% |
| Stage 9 corruption rows per batch | 1 of each kind | 1 of each kind |

---

## Last Updated

2026-05-08.
