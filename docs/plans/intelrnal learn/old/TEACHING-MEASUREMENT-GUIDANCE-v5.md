# Teaching Measurement Guidance
# Version: 5.0 — 2026-05-08
# Supersedes: TEACHING-MEASUREMENT-GUIDANCE-v4.md
# Authority: ORCHESTRATOR-TEACHING-PLAN-v4.md
# Grounded in: AdaptiveLogicalOrchestrator.cs, DefaultLogicalCapabilityTrainer.cs,
#               DpoTrainer.cs, XIIGEN-INTERNAL-SOURCE-REGISTRY-v1.md

# v5 additions over v4:
# M1: DPO-step metrics added to Layer 1 evidence requirements.
#     dpo_loss, dpo_reward_margin, dpo_accuracy now required per row.
#     A row where dpo_loss stays at 0.693 (ln(2) = random-guess floor)
#     is not accepted as learning evidence regardless of verdict.
# M2: Token overlap ratio added as a Layer 1 field.
#     overlap_ratio must be recorded for every pair before training.
#     A row with overlap_ratio > 0.60 should have been caught by the pair
#     quality gate; if it reached training, it is recorded and flagged.
# M3: Loss trajectory rule added. Five consecutive rows with dpo_loss
#     at or above 0.685 triggers systematic pair quality audit.
# M4: Pair source origin added to row evidence schema.
#     source_registry_id and source_session_rows now required fields.
#     This enables per-source diagnosis when margin is negative.
# M5: Annotation quality added to row evidence schema and Layer 3 verdict.
#     A row where annotation_quality = "weak" and dpo_reward_margin < 0
#     identifies the annotation as the cause — not the pair content.
# M6: Row acceptance criteria extended with DPO margin and loss floor.
#     verdict = PASS alone is no longer sufficient for ACCEPT_ROW.
# All v4 confirmed numbers (rows 1–5, resource thresholds) preserved.

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

### Layer 1 — Resources and DPO step

```
Inference elapsed ms
Teaching elapsed ms
CPU percent           — (cpuDelta / elapsed / ProcessorCount) × 100
Working-set delta MB  — before/after delta, not a sampled peak
Managed allocation MB
Disk delta MB
Checkpoint bytes

NEW in v5:
  overlap_ratio       — shared_tokens / total_tokens; > 0.60 = gate failure
  annotation_quality  — "strong", "weak", "absent"
  dpo_loss            — from dpo_step_*.json; should move below 0.685
  dpo_reward_margin   — chosen_reward - rejected_reward; negative = wrong direction
  dpo_accuracy        — fraction of pairs where chosen_reward > rejected_reward

Branch count, registered flow count, registered unit count
```

A row is not accepted as learning evidence if `dpo_loss >= 0.693`. Loss at
ln(2) = 0.693 is random-guess level — the DPO step produced no gradient signal
regardless of what the verification verdict shows.

### Layer 2 — Routing

```
Compiler predicates
Routed sub-phases, confidence values, branch count
Whether each branch is justified by the real human prompt
Knowledge edge count before and after
```

### Layer 3 — Verdict and Pair Quality

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
  create fresh orchestrator per row → proves only local self-repair
```

---

## DPO Step Metrics — What They Mean

These are new in v5. They come from `dpo_step_*.json` files written by
`DpoTrainer.Step()` after each training call.

### dpo_loss

```
dpo_loss = −(log P(chosen | prompt) − log P(rejected | prompt))
           scaled by DPO beta parameter
```

**Expected trajectory:** Loss should decrease as the model learns to prefer
chosen over rejected. A model that starts at loss = 0.693 (ln(2) = random-guess)
and moves toward 0.600 is learning. A model that stays at 0.693 across multiple
rows is receiving no gradient signal — the pairs are too similar for the optimizer.

**Calibration run 2026-05-06:** Loss stayed at 0.693 across all 40 batches.
Root cause: every rejected was `"Do not generate a capability for sub-phase X"`,
producing token overlap ~0.70. No gradient signal reached the adapter weights.

**Loss thresholds:**

| Loss value | Meaning | Action |
|---|---|---|
| 0.693 (= ln(2)) | Random-guess level, no learning | Audit pair quality — overlap too high |
| 0.685–0.692 | Beginning of movement, weak signal | Watch; check annotation quality |
| 0.650–0.685 | Learning occurring | Continue |
| < 0.600 | Strong learning signal | Healthy; monitor for overfitting |

### dpo_reward_margin

```
dpo_reward_margin = chosen_reward - rejected_reward
```

**Positive margin:** Model assigns higher probability to chosen than to rejected.
The optimizer reinforced the correct preference. Useful learning signal.

**Negative margin:** Model assigns higher probability to rejected than to chosen.
The DPO step ran but reinforced the wrong direction. Record the pair overlap
ratio and annotation quality — these identify the cause.

**Near-zero margin (|margin| < 0.0002):** The pair is providing some signal but
it is very weak. Usually caused by overlap_ratio in the 0.45–0.60 range with
a borderline annotation. Track whether it improves over multiple rows for the
same component.

### dpo_accuracy

```
dpo_accuracy = fraction of pairs where chosen_reward > rejected_reward
```

In one-row-at-a-time mode this is 0.0 or 1.0 per row. Use it as a row-level
binary: did this pair produce positive margin (1.0) or not (0.0).

Across a round, track the fraction of rows with dpo_accuracy = 1.0. The target
is ≥ 0.60 per round (6 of 10 rows with positive margin).

**Calibration run 2026-05-06 per-domain:**

| Domain | Mean accuracy | Positive margin rows | Notes |
|---|---|---|---|
| PLAN | 0.507 | 2 of 6 batches | Best batches had named artifacts |
| BUG | 0.417 | 7 of 27 batches | Wide variance |
| CODE_Q | 0.328 | 0 of 4 batches | Zero positive margin; negation template |
| ARCH | — | 0 of 0 batches | No ARCH training data existed |

---

## Token Overlap Ratio — New Layer 1 Field

The overlap_ratio must be computed and recorded before training runs:

```
shared = tokens in both chosen and rejected that are NOT also in the prompt
total  = tokens in chosen + tokens in rejected − tokens in prompt
overlap_ratio = shared / total
```

**Record this in the row evidence regardless of whether the pair was pre-screened
by the pair quality gate.** A pair that passed the gate may still have borderline
overlap, and the ratio explains why the row produced weak margin.

**Overlap ratio interpretation:**

| overlap_ratio | Expected dpo_reward_margin | Action |
|---|---|---|
| < 0.30 | Reliably positive | Proceed |
| 0.30–0.50 | Positive with strong annotation | Check annotation_quality |
| 0.50–0.60 | Borderline | Strong annotation required; flag row for monitoring |
| > 0.60 | Reliably negative | Should have been caught at pair quality gate |

**Source-specific typical overlap ratios** (from XIIGEN-INTERNAL-SOURCE-REGISTRY):

| Source | Pair type | Typical overlap |
|---|---|---|
| Source 2 correction events | Any domain | 0.10–0.25 |
| Source 1 rows 521–727 (PLAN positive zone) | Type B | 0.15–0.35 |
| Source 1 rows 1–520 (ARCH zone) | Type A | 0.10–0.20 |
| Source 1 rows 1235–1753 (ineligible zone) | Any | 0.50–0.70 (ineligible) |

If a row from Source 1 rows 1235–1753 reaches training, it will almost certainly
produce negative margin. Record the source_session_rows field so post-session
analysis can trace the pattern.

---

## Loss Trajectory Rule

Track `dpo_loss` across consecutive rows. Apply the loss trajectory rule:

**Five consecutive rows with dpo_loss ≥ 0.685 → systematic pair quality audit.**

The audit procedure:

```
1. List all five rows with their overlap_ratio and annotation_quality values.

2. Identify the pattern:

   All rows have overlap_ratio > 0.50:
     → Pair vocabulary too similar. Replace rejected with more distant sub-phase.
       Check source zone — rows 1235–1753 from Source 1 produce high overlap.

   All rows have overlap_ratio < 0.50 but annotation_quality = "weak":
     → Annotation not providing enough distributional signal.
       Strengthen Type B annotations — explain why wrong order fails functionally.

   overlap_ratio < 0.40 and annotation = "strong" but loss still high:
     → Model size may be insufficient for vocabulary. Consider LoRA Rank reduction.

   Mixed pattern across five rows:
     → Identify which rows drove the high loss and fix those pairs specifically.

3. Do not continue the round until the identified pattern is resolved.
```

The loss floor of 0.685 is calibrated from confirmed row results. Rows 1–5 from
the `legacy-docs-skills-from-old-code` bundle showed dpo_loss moving into the
0.63–0.65 range on accepted rows. Loss above 0.685 means the model is near or
at random-guess level for the batch being trained.

---

## Layer 1 — Confirmed Resource Numbers (from v4, preserved)

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
| dpo_loss | ≥ 0.693 on any row | Random-guess level | Audit pair quality immediately |
| dpo_loss | ≥ 0.685 for 5 consecutive rows | Systematic signal failure | Run loss trajectory audit |
| dpo_reward_margin | < 0 | Wrong direction | Check overlap_ratio and annotation |
| overlap_ratio | > 0.60 | Pair failed quality gate | Record and flag; do not accept row |

**Important:** Do not reduce LoRA Rank unless the current trainer uses Rank 4.
The current default capability trainer uses Rank 2. If training still exceeds
the 10-second budget at Rank 2, reduce branch count or row splitting — not rank.

---

## Layer 2 — Routing Numbers (from v4, preserved)

### Routing failure modes

| Signal | Meaning | Resolution |
|---|---|---|
| Too few branches | Compiler missed predicates | Add vocabulary or source-specific synonyms |
| Too many branches | Predicate family too broad | Tighten anchor terms or relevance floor |
| Wrong phase family | Predicate mismatch | Fix compiler lexicon before continuing |
| Correct predicates, wrong sub-phases | Sub-phase anchor vocabulary weak | Add anchor terms or teaching rows |
| PASS with wrong branch count | False success | Treat as partial; fix routing before accepting |

### Expected routing from first rows (from v4, confirmed correct)

```
Row 1: Predicates ARCHITECTURE, DOCUMENT, IMPLEMENT, LEARNING
       Branches: 2.1, 3.1, 4.1, 4.4, 7.5

Row 2: Predicates IMPLEMENT, TEST, UI
       Branches: 4.1, 4.2, 4.4, 5.1, 5.4

Row 4 (after fix): Predicates IMPLEMENT, TEST, UI
                   Branches: 5.1, 4.2, 4.4
```

---

## Layer 3 — Verdict and Pair Quality

### Verdict order

```
REJECT > REPROCESS > KNOWLEDGE_EDGE > PASS_WITH_GAPS > CONCERN > PASS
```

When multiple branches run, highest-severity verdict wins.

### Extended row acceptance criteria (v5)

A row is ACCEPT_ROW only when **all** of:

```
□ overall_verdict = PASS
□ knowledge_edge_count = 0
□ row elapsed <= 10,000 ms
□ registry_after unit_count > registry_before unit_count
□ dpo_reward_margin >= 0      ← NEW in v5
□ dpo_loss < 0.685            ← NEW in v5
□ overlap_ratio <= 0.60       ← NEW in v5 (was a gate, now also recorded)
```

If verdict = PASS but `dpo_reward_margin < 0`: decision = REVIEW_ROW.
The verification pass may be from the deterministic fallback, not the trained unit.
Check `selected_unit_ids` in the verification result — if the deterministic unit
was selected, the trained unit did not improve enough to exceed the trust threshold.

If verdict = PASS but `dpo_loss >= 0.685`: decision = REVIEW_ROW.
The optimizer step did not produce meaningful signal. The model may not have
updated from this pair. Treat as a smoke test, not learning evidence.

### Pair quality impact on Layer 3

When a row produces decision = REVIEW_ROW or the verdict degrades after a new row,
check Layer 3 pair quality fields before investigating anything else:

```
If overlap_ratio > 0.50:
  → The pair's token distributions were too similar for the DPO step to
    produce directional gradient. This is the most common cause of negative
    dpo_reward_margin.

If annotation_quality = "weak" and overlap_ratio is 0.40–0.55:
  → The annotation was not adding enough distributional signal for a Type B pair.
    Strengthen the annotation before re-teaching this row.

If overlap_ratio < 0.40 and annotation = "strong" but dpo_reward_margin < 0:
  → Chosen content may be inaccurate for the prompt. The model prefers rejected
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
| PASS but dpo_loss ≥ 0.693 | No gradient signal produced | Audit pair quality |

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
  "rejection_diagnosis": null
}
```

`dpo_step`, `pair_quality`, `source_registry_id` are new required fields in v5.

If `cycle_mode` is not `cumulative`, the row cannot be accepted as learning evidence.

---

## Per-Source Diagnosis for Negative dpo_reward_margin

When a row produces negative margin, check `source_registry_id` first:

| Source | Expected pair type | If margin negative |
|---|---|---|
| source-2-export correction events | Any; overlap 0.10–0.25 | Low-quality correction included (phrasing-only). Re-rate; exclude Low. |
| source-1-arch-zone (rows 1–520) | ARCH Type A; overlap 0.10–0.20 | Rows not reclassified as ARCH domain. Check domain_label in pair file. |
| source-1-plan-positive (rows 521–727) | PLAN Type B; overlap 0.15–0.35 | Annotation weak — states wrong order but does not explain why it fails. Strengthen. |
| source-1-ineligible (rows 1235–1753) | Should not appear | Meta-commentary prompt used as A side. Discard pair; use only corrections from this range. |

---

## Registry Version Chains and Active Snapshots (from v4, preserved)

### Correct model — checkpoint continuation

```
4.4-runtime-unit:
  v3 — starts from v2 weights, then learns row 4
  v2 — starts from v1 weights, then learns row 2
  v1 — starts from base weights, learns row 1
```

After row 5 this chain was confirmed. Row 5 manifests contain `ParentUnitId`,
`ParentPolicyCheckpointPath`, and `TrainingStepCount: 2`.

### Active/archive rule (unchanged)

```
Active:  newest Probationary or Trusted version per logical unit family
Archive: prior checkpoint versions (never delete — needed for rollback and audit)
Prune:   at end of each bundle, not mid-bundle
Rollback: mark row N version as Rejected; ResolveLatest falls back to row N-1
```

---

## Summary Decision Table (extended for v5)

| Resources | DPO step | Routing | Verdict | Decision |
|---|---|---|---|---|
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

## Current State (as of v5 baseline)

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

The five accepted rows were measured before DPO-step metrics were tracked.
When processing rows 6 onward, `dpo_loss`, `dpo_reward_margin`, and `dpo_accuracy`
must be recorded and included in the acceptance decision. A row-6 onward that
produces `dpo_loss >= 0.693` is REVIEW_ROW even if verdict = PASS.

Pair source priority for rows 6 onward:
```
1. Correction events from Source 2 export (overlap 0.10–0.25, no construction needed)
2. ARCH zone rows from Source 1 (rows 1–520, overlap 0.10–0.20)
3. PLAN positive zone rows from Source 1 (rows 521–727, overlap 0.15–0.35)
```

Do not continue processing rows from Source 1 rows 1235–1753 as PLAN A sides.
These are confirmed ineligible (batch 30, acc=0.306). Correction events from
this range remain acceptable.

---

## Last Updated

2026-05-08.
