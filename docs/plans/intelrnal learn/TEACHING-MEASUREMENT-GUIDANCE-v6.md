# Teaching Measurement Guidance
# Version: 6.0 â€” 2026-05-08
# Supersedes: TEACHING-MEASUREMENT-GUIDANCE-v5.md
# Authority: ORCHESTRATOR-TEACHING-PLAN-v5.md
# Grounded in: AdaptiveLogicalOrchestrator.cs, DefaultLogicalCapabilityTrainer.cs,
#               DpoTrainer.cs, XIIGEN-INTERNAL-SOURCE-REGISTRY-v11.md,
#               EXAMINATION-METHOD-ORCHESTRATION-v14.md

# v6 additions over v5:
# N1: Registry reference updated to v11. Per-source diagnosis table extended
#     with all B-series families (B01â€“B12) and A-series families (A01â€“A24).
#     Each new family has an expected pair type, typical overlap, and
#     negative-margin diagnosis entry.
# N2: Examination method reference updated to v13. pair_kind allowed list
#     and source family decision table in evidence schema updated accordingly.
#     topology_graph, example_catalog, naming_translation pair kinds added.
# N3: Canonicality fields added to row evidence schema.
#     source_canonicality and canonical_confirmed_via required from v6.
#     A row without canonicality fields is not accepted as learning evidence.
#     mirror/stale/archive_container/secure_operational rows are gate failures.
# N4: Batch composition alignment with DPO-PAIR-AUTHORING-GUIDANCE-v6.
#     ACCEPT_ROW criteria table updated: Type A 40% quota now subdivides
#     into v6 ARCH subtypes (boundary, monitoring, review, decision, contract).
# N5: Pre-made pair source rows added to per-source diagnosis table.
#     Types 31/32/33 (naming tables, validation reports, conflict analysis)
#     each have expected overlap, positive-margin conditions, and
#     negative-margin diagnosis.
# N6: B-series family diagnosis entries added (B02 parts mirror check,
#     B07 stale comparison check, B10 DNA pattern violation derivation,
#     B11 B-side authority prohibition).
# N7: Additional registry companion families A01-A51, B13-B43, C01-C12,
#     Z41-Z56, D01-D19, E01-E16, F01-F17, G01-G13, H01-H08, I01-I14,
#     J01-J09, K01-K13, L01-L08, M01-M21, N01-N09, O01-O19, P01-P12,
#     Q01-Q05, R01-R13, S01-S14, T01-T10, U01-U20, V01-V08, W01-W16,
#     X01-X16, Y01-Y10, AA01-AA17, AB01-AB07, AC01-AC20, AD01-AD08,
#     AE01-AE10, AF01-AF13, and AG01-AG06 added
#     to row evidence expectations. source_family_dto is required for any
#     companion-family row; family_dto_missing is a gate failure, not a
#     negative-margin learning result.
# All v5 additions (M1â€“M6) and v4 confirmed numbers preserved.

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

## What This Document Is

This document explains how to judge whether a teaching row actually improved
the orchestrator model, what the measurements mean, and what to do when the
numbers reveal a problem.

**Core principle from v2 (unchanged):**

```
Teaching must be cumulative. The active teaching session persists one
topology registry, flow registry, unit registry, capability trainer state,
and orchestrator state across rows. Each invocation teaches exactly one
next row, then re-tests 2-3 prior rows.
```

**New principle in v5:**

```
Verdict = PASS is necessary but not sufficient for ACCEPT_ROW.
A row must also show dpo_reward_margin >= 0 and dpo_loss moving below 0.685
to be accepted as learning evidence.
```

A row that produces verdict PASS but `dpo_reward_margin < 0` means the DPO
optimizer ran in the wrong direction during training. The verification step
may show PASS because the deterministic fallback is still selected, not because
the trained unit learned correctly. This false pass is not learning evidence.

---

## Required Evidence Per Row

Every row must produce evidence for three layers. Layer 1 now includes DPO-step
metrics that were absent in v4.

### Layer 1 â€” Resources and DPO step

```
Inference elapsed ms
Teaching elapsed ms
CPU percent           â€” (cpuDelta / elapsed / ProcessorCount) x 100
Working-set delta MB  â€” before/after delta, not a sampled peak
Managed allocation MB
Disk delta MB
Checkpoint bytes

Added in v5:
  overlap_ratio       â€” shared_tokens / total_tokens; > 0.60 = gate failure
  annotation_quality  â€” "strong", "weak", "absent"
  dpo_loss            â€” from dpo_step_*.json; should move below 0.685
  dpo_reward_margin   â€” chosen_reward - rejected_reward; negative = wrong direction
  dpo_accuracy        â€” fraction of pairs where chosen_reward > rejected_reward

NEW in v6:
  source_canonicality     â€” "canonical" or "extracted_copy" only at submission
  canonical_confirmed_via â€” document state register or "What changed" header
  pair_kind               â€” from allowed list in EXAMINATION-METHOD-ORCHESTRATION-v14

Branch count, registered flow count, registered unit count
```

A row without `source_canonicality` and `canonical_confirmed_via` is not accepted
as learning evidence. A row where `source_canonicality` is `mirror`, `stale`,
`archive_container`, or `secure_operational` is a gate failure â€” reject before
any other check.

A row is not accepted as learning evidence if `dpo_loss >= 0.693`. Loss at
ln(2) = 0.693 is random-guess level â€” the DPO step produced no gradient signal
regardless of what the verification verdict shows.

### Layer 2 â€” Routing

```
Compiler predicates
Routed sub-phases, confidence values, branch count
Whether each branch is justified by the real human prompt
Knowledge edge count before and after
```

### Layer 3 â€” Verdict and Pair Quality

```
Pre-teaching verdict
DPO step result: dpo_reward_margin, dpo_loss, dpo_accuracy
Post-teaching verification verdict
Selected unit id and version
Whether selected unit came from current row or earlier row
Pair quality: overlap_ratio, annotation_quality
Cumulative registry state before and after
```

**A row succeeds only when all three layers are acceptable.**

---

## Mandatory Cumulative Teaching Rule (unchanged from v4)

The teaching runner must not create a fresh orchestrator per row.

```
Correct cycle:

Start bundle
  create topology registry once
  create flow registry once
  create unit registry once
  create capability trainer once
  create adaptive orchestrator once
  save active teaching snapshot

For row N
  load active teaching snapshot from row N-1
  teach only row N
  verify row N
  re-test the newest 2-3 previously accepted rows
  accept row N only if new row passes AND regression window passes
    AND dpo_reward_margin >= 0 AND dpo_loss < 0.685
  save active teaching snapshot
  write row evidence

Wrong cycle:
  create fresh orchestrator per row â†’ proves only local self-repair
```

---

## DPO Step Metrics â€” What They Mean

These are new in v5. They come from `dpo_step_*.json` files written by
`DpoTrainer.Step()` after each training call.

### dpo_loss

```
dpo_loss = âˆ’(log P(chosen | prompt) âˆ’ log P(rejected | prompt))
           scaled by DPO beta parameter
```

**Expected trajectory:** Loss should decrease as the model learns to prefer
chosen over rejected. A model that starts at loss = 0.693 (ln(2) = random-guess)
and moves toward 0.600 is learning. A model that stays at 0.693 across multiple
rows is receiving no gradient signal â€” the pairs are too similar for the optimizer.

**Calibration run 2026-05-06:** Loss stayed at 0.693 across all 40 batches.
Root cause: every rejected was `"Do not generate a capability for sub-phase X"`,
producing token overlap ~0.70. No gradient signal reached the adapter weights.

**Loss thresholds:**

| Loss value | Meaning | Action |
|---|---|---|
| 0.693 (= ln(2)) | Random-guess level, no learning | Audit pair quality â€” overlap too high |
| 0.685â€“0.692 | Beginning of movement, weak signal | Watch; check annotation quality |
| 0.650â€“0.685 | Learning occurring | Continue |
| < 0.600 | Strong learning signal | Healthy; monitor for overfitting |

### dpo_reward_margin

```
dpo_reward_margin = chosen_reward - rejected_reward
```

**Positive margin:** Model assigns higher probability to chosen than to rejected.
The optimizer reinforced the correct preference. Useful learning signal.

**Negative margin:** Model assigns higher probability to rejected than to chosen.
The DPO step ran but reinforced the wrong direction. Record the pair overlap
ratio and annotation quality â€” these identify the cause.

**Near-zero margin (|margin| < 0.0002):** The pair is providing some signal but
it is very weak. Usually caused by overlap_ratio in the 0.45â€“0.60 range with
a borderline annotation. Track whether it improves over multiple rows for the
same component.

### dpo_accuracy

```
dpo_accuracy = fraction of pairs where chosen_reward > rejected_reward
```

In one-row-at-a-time mode this is 0.0 or 1.0 per row. Use it as a row-level
binary: did this pair produce positive margin (1.0) or not (0.0).

Across a round, track the fraction of rows with dpo_accuracy = 1.0. The target
is â‰¥ 0.60 per round (6 of 10 rows with positive margin).

**Calibration run 2026-05-06 per-domain:**

| Domain | Mean accuracy | Positive margin rows | Notes |
|---|---|---|---|
| PLAN | 0.507 | 2 of 6 batches | Best batches had named artifacts |
| BUG | 0.417 | 7 of 27 batches | Wide variance |
| CODE_Q | 0.328 | 0 of 4 batches | Zero positive margin; negation template |
| ARCH | â€” | 0 of 0 batches | No ARCH training data existed |

---

## Token Overlap Ratio â€” New Layer 1 Field

The overlap_ratio must be computed and recorded before training runs:

```
shared = tokens in both chosen and rejected that are NOT also in the prompt
total  = tokens in chosen + tokens in rejected âˆ’ tokens in prompt
overlap_ratio = shared / total
```

**Record this in the row evidence regardless of whether the pair was pre-screened
by the pair quality gate.** A pair that passed the gate may still have borderline
overlap, and the ratio explains why the row produced weak margin.

**Overlap ratio interpretation:**

| overlap_ratio | Expected dpo_reward_margin | Action |
|---|---|---|
| < 0.30 | Reliably positive | Proceed |
| 0.30â€“0.50 | Positive with strong annotation | Check annotation_quality |
| 0.50â€“0.60 | Borderline | Strong annotation required; flag row for monitoring |
| > 0.60 | Reliably negative | Should have been caught at pair quality gate |

**Source-specific typical overlap ratios** (from XIIGEN-INTERNAL-SOURCE-REGISTRY-v11):

| Source | Pair type | Typical overlap |
|---|---|---|
| Source 2 correction events | Any domain | 0.10â€“0.25 |
| Source 1 rows 521â€“727 (PLAN positive zone) | Type B | 0.15â€“0.35 |
| Source 1 rows 1â€“520 (ARCH zone) | Type A | 0.10â€“0.20 |
| Source 1 rows 1235â€“1753 (ineligible zone) | Any | 0.50â€“0.70 (ineligible) |
| Type 31 naming/mapping tables | CODE_Q | 0.05â€“0.15 |
| Type 32 validation reports | ArbiterUnit | 0.15â€“0.25 |
| Type 33 conflict analysis | ARCH Type D | 0.10â€“0.25 |
| Type 25 config schema contracts | ArbiterUnit | 0.10â€“0.30 |
| Type 26 source code contracts | ARCH/CODE_Q | 0.10â€“0.25 |
| Type 27 / A13 diagram topology | ARCH | 0.10â€“0.20 |
| Type 28 / A15 test suite assertions | ArbiterUnit/CODE_Q | 0.10â€“0.25 |
| Type 29 phase implementation plans | PLAN Type B | 0.30â€“0.45 |

If a row from Source 1 rows 1235â€“1753 reaches training, it will almost certainly
produce negative margin. Record the source_session_rows field so post-session
analysis can trace the pattern.

---

## Loss Trajectory Rule

Track `dpo_loss` across consecutive rows. Apply the loss trajectory rule:

**Five consecutive rows with dpo_loss â‰¥ 0.685 â†’ systematic pair quality audit.**

The audit procedure:

```
1. List all five rows with their overlap_ratio and annotation_quality values.

2. Identify the pattern:

   All rows have overlap_ratio > 0.50:
     â†’ Pair vocabulary too similar. Replace rejected with more distant sub-phase.
       Check source zone â€” rows 1235â€“1753 from Source 1 produce high overlap.

   All rows have overlap_ratio < 0.50 but annotation_quality = "weak":
     â†’ Annotation not providing enough distributional signal.
       Strengthen Type B annotations â€” explain why wrong order fails functionally.

   overlap_ratio < 0.40 and annotation = "strong" but loss still high:
     â†’ Model size may be insufficient for vocabulary. Consider LoRA Rank reduction.

   Mixed pattern across five rows:
     â†’ Identify which rows drove the high loss and fix those pairs specifically.

3. Do not continue the round until the identified pattern is resolved.
```

The loss floor of 0.685 is calibrated from confirmed row results. Rows 1â€“5 from
the `legacy-docs-skills-from-old-code` bundle showed dpo_loss moving into the
0.63â€“0.65 range on accepted rows. Loss above 0.685 means the model is near or
at random-guess level for the batch being trained.

---

## Layer 1 â€” Confirmed Resource Numbers (from v4, preserved)

### Row results from legacy-docs-skills-from-old-code bundle

```
Row 4 (cumulative, accepted):
  Teaching elapsed:   2,887.9 ms
  CPU:                12.89%
  Working-set delta:  16.797 MB
  Managed allocation: 299.498 MB
  Checkpoint bytes:   1,234,698
  LoRA Rank:          2
  Verdict:            PASS
  Regression:         rows 1, 2, 3 all PASS

Row 5 (cumulative, checkpoint continuation confirmed):
  Teaching elapsed:   7,748.0 ms
  CPU:                13.06%
  Working-set delta:  68.086 MB
  Managed allocation: 802.116 MB
  Verdict:            PASS
  Checkpoint continuation: confirmed (ParentUnitId present, TrainingStepCount = 2)
```

### Resource thresholds

| Number | Threshold | Meaning | Action |
|---|---|---|---|
| Inference elapsed | > 200 ms | Routing too slow | Inspect HNSW/search/tokenization cost |
| Teaching elapsed | > 10,000 ms | Row exceeds budget | Reduce branch count or training cost |
| Teaching elapsed trend | increasing > 500 ms/row | Cumulative cost growing | Check duplicate flow/model registration |
| Working-set delta | > 200 MB | Retained memory risk | Stop; inspect model/tensor retention |
| Working-set after GC | monotonically increasing | Cumulative leak | Force post-row GC measurement |
| dpo_loss | â‰¥ 0.693 on any row | Random-guess level | Audit pair quality immediately |
| dpo_loss | â‰¥ 0.685 for 5 consecutive rows | Systematic signal failure | Run loss trajectory audit |
| dpo_reward_margin | < 0 | Wrong direction | Check overlap_ratio and annotation |
| overlap_ratio | > 0.60 | Pair failed quality gate | Record and flag; do not accept row |

**Important:** Do not reduce LoRA Rank unless the current trainer uses Rank 4.
The current default capability trainer uses Rank 2. If training still exceeds
the 10-second budget at Rank 2, reduce branch count or row splitting â€” not rank.

---

## Layer 2 â€” Routing Numbers (from v4, preserved)

### Routing failure modes

| Signal | Meaning | Resolution |
|---|---|---|
| Too few branches | Compiler missed predicates | Add vocabulary or source-specific synonyms |
| Too many branches | Predicate family too broad | Tighten anchor terms or relevance floor |
| Wrong phase family | Predicate mismatch | Fix compiler lexicon before continuing |
| Correct predicates, wrong sub-phases | Sub-phase anchor vocabulary weak | Add anchor terms or teaching rows |
| PASS with wrong branch count | False success | Treat as partial; fix routing before accepting |

### Expected routing from first rows (from v4, confirmed correct)

2026-05-11 correction: routing metrics must be measured at flow level.
Compiler/router unit checks are diagnostic only. A routing batch is accepted only
when `OrchestrationRoutingFlow : ILogicalFlow<LogicalOrchestratorRequest>` or the
equivalent `AdaptiveLogicalOrchestrator` path executes held-out prompts and
returns the correct compiled document, route decisions, knowledge-edge state,
and flow/factory resolution. If this runtime path is absent, record
`PAIRSET_READY_ROUTING_FLOW_PENDING`, not PASS.

```
Row 1: Predicates ARCHITECTURE, DOCUMENT, IMPLEMENT, LEARNING
       Branches: 2.1, 3.1, 4.1, 4.4, 7.5

Row 2: Predicates IMPLEMENT, TEST, UI
       Branches: 4.1, 4.2, 4.4, 5.1, 5.4

Row 4 (after fix): Predicates IMPLEMENT, TEST, UI
                   Branches: 5.1, 4.2, 4.4
```

---

## Layer 3 â€” Verdict and Pair Quality

### Verdict order

```
REJECT > REPROCESS > KNOWLEDGE_EDGE > PASS_WITH_GAPS > CONCERN > PASS
```

When multiple branches run, highest-severity verdict wins.

### Extended row acceptance criteria (v5)

A row is ACCEPT_ROW only when **all** of:

```
â–¡ source_canonicality is "canonical" or "extracted_copy"  â† NEW in v6
â–¡ canonical_confirmed_via field present                    â† NEW in v6
â–¡ overall_verdict = PASS
â–¡ knowledge_edge_count = 0
â–¡ row elapsed <= 10,000 ms
â–¡ registry_after unit_count > registry_before unit_count
â–¡ dpo_reward_margin >= 0      â† added in v5
â–¡ dpo_loss < 0.685            â† added in v5
â–¡ overlap_ratio <= 0.60       â† added in v5
```

If verdict = PASS but `dpo_reward_margin < 0`: decision = REVIEW_ROW.
The verification pass may be from the deterministic fallback, not the trained unit.
Check `selected_unit_ids` in the verification result â€” if the deterministic unit
was selected, the trained unit did not improve enough to exceed the trust threshold.

If verdict = PASS but `dpo_loss >= 0.685`: decision = REVIEW_ROW.
The optimizer step did not produce meaningful signal. The model may not have
updated from this pair. Treat as a smoke test, not learning evidence.

### Pair quality impact on Layer 3

When a row produces decision = REVIEW_ROW or the verdict degrades after a new row,
check Layer 3 pair quality fields before investigating anything else:

```
If overlap_ratio > 0.50:
  â†’ The pair's token distributions were too similar for the DPO step to
    produce directional gradient. This is the most common cause of negative
    dpo_reward_margin.

If annotation_quality = "weak" and overlap_ratio is 0.40â€“0.55:
  â†’ The annotation was not adding enough distributional signal for a Type B pair.
    Strengthen the annotation before re-teaching this row.

If overlap_ratio < 0.40 and annotation = "strong" but dpo_reward_margin < 0:
  â†’ Chosen content may be inaccurate for the prompt. The model prefers rejected
    because the chosen B side does not match what the prompt actually asked.
    Review the chosen content.
```

### Verdict failure modes (from v4, extended)

| Signal | Meaning | Action |
|---|---|---|
| KNOWLEDGE_EDGE after teaching | Trainer did not register usable capability | Inspect training result |
| PASS but selected old unit | Row was not actually learned | Register newer unit version |
| PASS but branch count wrong | Routing false success | Fix routing before accepting |
| PASS_WITH_GAPS persists | Arbiter/coverage gap | Add arbiter pairs |
| Verdict degrades after new row | New training introduced bad behavior | Check overlap_ratio and rollback if needed |
| PASS but dpo_reward_margin < 0 | Deterministic fallback selected, not trained unit | Check selected_unit_ids |
| PASS but dpo_loss â‰¥ 0.693 | No gradient signal produced | Audit pair quality |

---

## Row Evidence Schema (updated for v5)

```json
{
  "row_id": "CEXP-B0000-LDS-0004",
  "cycle_mode": "cumulative",
  "source_registry_id": "source-2-export-bundle-legacy-docs",
  "source_session_rows": null,
  "source_bundle": "legacy-docs-skills-from-old-code",
  "prompt": "...",
  "compiler_predicates": ["IMPLEMENT", "TEST", "UI"],
  "router_sub_phases": ["5.1", "4.2", "4.4"],
  "branch_count": 3,
  "pair_quality": {
    "overlap_ratio": 0.18,
    "annotation_quality": "strong",
    "pair_source_type": "correction_event"
  },
  "source_family_dto": {
    "source_family": "G05",
    "source_document_family": "VALIDATION-MATRIX",
    "target_registry_type": "Type 29 Zone D",
    "source_locator": {
      "section_id": "Validation Matrix",
      "json_pointer": null,
      "archive_member_path": null
    },
    "family_required_fields_complete": true,
    "required_fields": {
      "requirement_id": "REQ-001",
      "covered_by_artifact": "FLOW-01-DESIGN-SIMULATION-R1.md",
      "coverage_status": "covered"
    }
  },
  "resources": {
    "inference_elapsed_ms": 0,
    "teaching_elapsed_ms": 0,
    "cpu_percent": 0,
    "working_set_delta_mb": 0,
    "managed_allocation_delta_mb": 0,
    "disk_delta_mb": 0,
    "checkpoint_bytes": 0
  },
  "dpo_step": {
    "loss": 0.641,
    "reward_margin": 0.0021,
    "accuracy": 1.0
  },
  "registry_before": { "flow_count": 0, "unit_count": 0 },
  "registry_after":  { "flow_count": 0, "unit_count": 0 },
  "registered_this_row": {
    "flow_ids": [],
    "unit_ids": [],
    "sub_phase_ids": []
  },
  "verification": {
    "overall_verdict": "PASS",
    "knowledge_edge_count": 0,
    "selected_unit_ids": []
  },
  "decision": "ACCEPT_ROW",
  "rejection_diagnosis": null,
  "canonicality": {
    "source_canonicality": "canonical",
    "canonical_confirmed_via": "DOCUMENT-STATE-REGISTER-2026-05-06-v8.md",
    "pair_kind": "correction_event"
  }
}
```

`dpo_step`, `pair_quality`, `source_registry_id` added in v5.
`canonicality` block with `source_canonicality`, `canonical_confirmed_via`, and `pair_kind` added in v6.
`source_family_dto` is required for companion-family rows from A01-A51, B13-B43,
C01-C12, Z41-Z56, D01-D19, E01-E16, F01-F17, G01-G13, H01-H08, I01-I14,
J01-J09, K01-K13, L01-L08, M01-M21, N01-N09, O01-O19, P01-P12, Q01-Q05,
R01-R13, S01-S14, T01-T10, U01-U20, V01-V08, W01-W16, X01-X16,
Y01-Y10, AA01-AA17, AB01-AB07, AC01-AC20, AD01-AD08, AE01-AE10,
AF01-AF13, or AG01-AG06.
All fields required. A row without the `canonicality` block is not accepted as evidence.
For companion-family rows, `family_required_fields_complete=false` is a gate failure
with rejection diagnosis `family_dto_missing`, not a normal negative-margin training row.

If `cycle_mode` is not `cumulative`, the row cannot be accepted as learning evidence.

---

## Per-Source Diagnosis for Negative dpo_reward_margin

When a row produces negative margin, check `source_registry_id` first:

| Source | Expected pair type | If margin negative |
|---|---|---|
| source-2-export correction events | Any; overlap 0.10â€“0.25 | Low-quality correction included (phrasing-only). Re-rate; exclude Low. |
| source-1-arch-zone (rows 1â€“520) | ARCH Type A; overlap 0.10â€“0.20 | Rows not reclassified as ARCH domain. Check domain_label in pair file. |
| source-1-plan-positive (rows 521â€“727) | PLAN Type B; overlap 0.15â€“0.35 | Annotation weak â€” states wrong order but does not explain why it fails. Strengthen. |
| source-1-ineligible (rows 1235â€“1753) | Should not appear | Meta-commentary prompt used as A side. Discard pair; use only corrections from this range. |
| Type 21 / A21 (positive/negative catalogs) | Any; overlap < 0.30 | Gate 1 skipped â€” positive example is superseded. Re-verify against current Type 9 decisions. |
| Type 23 / U14 (gap remediation) | BUG/ARCH; overlap 0.15â€“0.30 | Wrong-fix attempt absent and rejection constructed incorrectly. Rebuild from ARCH/BUG confusion tables. |
| Type 25 (config schema contracts) | ArbiterUnit; overlap 0.10â€“0.30 | Schema file not current version. Apply three-check rule; use only current schema. |
| Type 26 (source code contracts) | ARCH/CODE_Q; overlap 0.10â€“0.25 | Interface definition from stale version. Three-check rule required before use. |
| Type 27 / A13 (diagram topology) | ARCH; overlap 0.10â€“0.20 | Diagram flattened to prose before extraction â€” edge direction lost. Reparse as graph. |
| Type 28 / A15 (test suite files) | ArbiterUnit/CODE_Q; overlap 0.10â€“0.25 | Failing test used as chosen. Only passing tests are valid â€” verify via live test run. |
| Type 29 (phase implementation plans) | PLAN Type B; overlap 0.30â€“0.45 | Phase plan ordering pair missing annotation. Annotation must name why step N cannot precede step N-1. |
| Type 31 (naming/mapping tables) | CODE_Q; overlap 0.05â€“0.15 | Old name incorrectly used as chosen. Check three-check rule â€” old name is rejected, not chosen. |
| Type 32 (validation reports) | ArbiterUnit; overlap 0.15â€“0.25 | Fabricated concern used as chosen instead of the real report verdict. Use report verdict directly. |
| Type 33 (conflict analysis) | ARCH Type D; overlap 0.10â€“0.25 | Unchosen alternative from a superseded analysis used. Verify the comparison document is not stale. |
| B02 serialized parts (PHASExA_*.md) | Type 29 rules | Merged version exists â€” parts are mirrors. Discard parts pair; rebuild from merged canonical source. |
| B07 conflict analysis | ARCH Type D | Unchosen alternative superseded by later decision. Apply three-check rule before use as rejected. |
| B10 DNA patterns | Type 18 extension | Violation not derived from named pattern rule. Name the specific DNA pattern check in the annotation. |
| B11 README/summary | A-side only | B-side authority claimed from README text. README cannot be B-side. Rebuild with Type 2/5/26 source. |
| A18 vendored codebases | Should not appear in internal pipeline | External canonicality. Move to external registry with adoption decision cited. |
| M01-M21 training/examination/live-run artifacts | Measurement / evaluation / evidence rows | Structured JSON/YAML/JSONL was flattened or train/eval/measurement role mixed. Parse DTO fields and reclassify. |
| O13/O14 screenshot + UX review evidence | Visual/UX findings | Screenshot filename used without linked UX review row, or finding used without screenshot evidence. Link both records. |
| T07 portability evidence JSON | Portability pass/fail evidence | Pass verdict ignores phaseCloseScan or nonzero exit code. Rebuild from structured evidence JSON. |
| V01-V08 xiigen-COMPLETE canonical source | NestJS-era chosen source | Older Python/FLOW33 source used as chosen after canonical NestJS archive exists. Mark old source stale. |
| W01-W16 llm_mvp_core experiment/data refinements | Experiment/spec/result/gate rows | Broad "experiment plan" label used. Select subtype and check mandatory fields before measurement. |
| X01-X16 Genie skill library families | Stack/template/recipe/tool rows | Genie source used as XIIGen authority or stack/template family collapsed. Record project_id and member family. |
| Y01-Y10 module-architecture/phases_done families | Composition and translation state rows | Matrix/module guide/translated part/state/final declaration swapped. Record archive and exact family role. |
| AA01-AA17 xiigen_v18_complete families | V18 law/module/config/flow/skill rows | V18 composition-layer source collapsed into Type 20. Parse JSON/config/flow definitions before measurement. |
| AB01-AB07 canonical V18 migration/architect families | Migration/architect/schema/config/manifest rows | Stale v1-v3 plan or schema/config/manifest role swap. Record canonical anchor. |
| AC01-AC20 compliance/flow-extension/plugin-factory families | Requirement/renumbering/validation/example rows | Broad "flow doc" or "skill index" label used. Record AC family DTO fields. |
| AD01-AD08 web-session/flow-state/consolidation families | Handoff/state/validation/consolidation rows | Analysis doc used as executable instructions or validation claim lacks matrix evidence. |
| AE01-AE10 canonical extension/V37 planning families | Append/research/V37 RAG rows | Snapshot ZIP used as authority, append docs swapped with merge plans, or RAG index used as implementation source. |
| AF01-AF13 flow planning/examination/artifact verdict families | Simulation/TEACH/verdict/goal rows | Raw transcript or HTML used whole, TEACH/design/planning simulation swapped, or covered family duplicated. |
| AG01-AG06 flow input/renumbering/merge families | Renumbering/intent/research/PromptOps rows | Draft IDs used despite renumber map, merge plan used as completion, or raw intent separated from design response. |
| U22 external DPO batches | Should not appear in internal pipeline | mixed_with_internal: true is a hard gate failure. Separate into external-only batch file. |

---

## Registry Version Chains and Active Snapshots (from v4, preserved)

### Correct model â€” checkpoint continuation

```
4.4-runtime-unit:
  v3 â€” starts from v2 weights, then learns row 4
  v2 â€” starts from v1 weights, then learns row 2
  v1 â€” starts from base weights, learns row 1
```

After row 5 this chain was confirmed. Row 5 manifests contain `ParentUnitId`,
`ParentPolicyCheckpointPath`, and `TrainingStepCount: 2`.

### Active/archive rule (unchanged)

```
Active:  newest Probationary or Trusted version per logical unit family
Archive: prior checkpoint versions (never delete â€” needed for rollback and audit)
Prune:   at end of each bundle, not mid-bundle
Rollback: mark row N version as Rejected; ResolveLatest falls back to row N-1
```

---

## Summary Decision Table (extended for v5)

| Resources | DPO step | Routing | Verdict | Decision |
|---|---|---|---|---|
| Canonicality = mirror/stale/archive_container | Any | Any | Any | Gate failure â€” reject before any other check |
| External source mixed into internal batch | Any | Any | Any | Hard gate failure â€” separate batch file required |
| Within limits | margin >= 0, loss < 0.685 | Correct | PASS, new units registered | ACCEPT_ROW |
| Within limits | margin >= 0, loss < 0.685 | Too narrow | PASS | Under-routing; fix lexicon, rerun |
| Within limits | margin >= 0, loss < 0.685 | Too wide | PASS | Over-routing; fix anchors, rerun |
| Within limits | margin < 0 | Correct | PASS | REVIEW_ROW; check overlap and annotation |
| Within limits | loss >= 0.693 | Any | Any | REVIEW_ROW; audit pair quality |
| Within limits | margin < 0 for 5 rows | Any | Any | Run loss trajectory audit before continuing |
| > 10s elapsed | Any | Any | PASS | REVIEW_ROW; reduce branch count or cost |
| Working set > 200 MB | Any | Any | Any | Stop; inspect memory retention |
| cumulative registry not reused | Any | Any | Any | Reject run; rerun with one long-lived orchestrator |
| overlap_ratio > 0.60 reached training | Any | Any | Any | Record and flag; pair should have been caught at gate |

---

## Current State (as of v6 baseline)

From bundle `legacy-docs-skills-from-old-code`:

```
Rows accepted cumulatively: 5
Sub-phases covered: 2.1, 3.1, 4.1, 4.2, 4.4, 5.1, 5.4, 7.5 (8 of 31)
Generated flows in active snapshot: 50 total (18 generated capability flows)
Learned units in active snapshot: 18
Knowledge edges after verification: 0 across all five rows
Regression window (rows 2-4 tested after row 5): all PASS
Checkpoint continuation: confirmed on row 5
LoRA Rank in use: 2
```

**What v5 adds to this baseline:**

The five accepted rows (from v5 baseline) were measured before canonicality fields
were tracked. When processing rows 6 onward, all three sets of new fields must be
recorded: DPO-step metrics (`dpo_loss`, `dpo_reward_margin`, `dpo_accuracy` from v5)
and canonicality fields (`source_canonicality`, `canonical_confirmed_via`, `pair_kind`
from v6). A row-6 onward that produces `dpo_loss >= 0.693` is REVIEW_ROW even if
verdict = PASS. A row without canonicality fields is also not accepted as evidence.

Pair source priority for rows 6 onward:
```
1. Correction events from Source 2 export (overlap 0.10â€“0.25, no construction needed)
2. ARCH zone rows from Source 1 (rows 1â€“520, overlap 0.10â€“0.20)
3. PLAN positive zone rows from Source 1 (rows 521â€“727, overlap 0.15â€“0.35)
```

Do not continue processing rows from Source 1 rows 1235â€“1753 as PLAN A sides.
These are confirmed ineligible (batch 30, acc=0.306). Correction events from
this range remain acceptable.

---

## Last Updated

2026-05-08. Version 6.0.
