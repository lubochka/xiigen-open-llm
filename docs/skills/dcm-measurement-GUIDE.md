# dcm-measurement-GUIDE.md

## Luba Why / Loophole Repair-First Rule

When Luba asks "why" (or its operator-language equivalent, e.g. "почему"), challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.
**Version:** 1.0 (2026-06-22)
**Applies to:** DesignConversionDomain (llm_mvp_core subproject, .NET 9 / C#, ML from scratch)
**Audience:** architect, sub-agent implementer, reviewer
**Language:** the operator's language (plain language; Russian for the original author), human-readable
**Status:** active guide; tier targets marked "Luba decides"

---

## 0. How this skill is used

Use this guide as a measurement checklist before any claim that DCM CriticUnit or ImproverUnit have become trained.
The reviewer/architect/implementer needs it to check the runner report: are there held-out metrics, ablation, leakage checks,
fresh-load, continue-training, speed numbers, and proof that the source/license gate is closed.

If the report does not close this guide, you may not write `PASS_AS_TRAINED_FEATURE`, promote weights, promise product quality,
or treat a deterministic scaffold as trained behavior. The permitted outcome in that case is `OPEN`,
`PROMOTION_BLOCKED`, or `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`.

---

## 1. Purpose

This guide describes how to measure the quality, speed, and honesty of the two trainable units of DesignConversionDomain — CriticUnit and ImproverUnit. The guide answers three questions: what counts as accuracy, when a unit's work is considered trained (and when it is an honest unfinished skeleton), and which numeric threshold metrics are needed to promote weights from Bootstrap to a trained state per decision A-LM-03. Without these measurements, the phase is considered open regardless of the presence of code or artifacts.

Allowed trainable levels a measurement claim may be made about: `ILearningMachinePair`, `ILearningMachine`,
`ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit`. Any deterministic lookup, regex/BM25 shortcut,
static validator, or helper-service can only be bootstrap/scaffold evidence and does not count as a trained unit.

---

## 2. Accuracy Per Unit

### 2.1 ImproverUnit — visual render accuracy

ImproverUnit takes the design tree, the source html+css, and a list of defects, and returns improved html+css. Accuracy is measured as the closeness of the produced html render to the reference render at four resolutions: 360 px, 768 px, 1280 px, and 1920 px.

**Formula:**

```
visual_accuracy(w) = 1 - norm_pixel_diff(render_produced(w), render_reference(w))

where norm_pixel_diff is the normalized per-coordinate pixel diff
                      (sum |delta_px| / (width × height × 3))

accuracy_improver = mean( visual_accuracy(360),
                          visual_accuracy(768),
                          visual_accuracy(1280),
                          visual_accuracy(1920) )
```

The metric lives on the held-out set (50 pairs of batch A-LM-04). For each pair, accuracy_chosen and accuracy_rejected are taken from a field of the DPO record.

**Example numbers (one held-out pair):**

| Resolution | render_produced vs reference (norm diff) | visual_accuracy |
|------------|--------------------------------------|-----------------|
| 360 px     | 0.042                                | 0.958           |
| 768 px     | 0.031                                | 0.969           |
| 1280 px    | 0.027                                | 0.973           |
| 1920 px    | 0.038                                | 0.962           |
| **Total**  |                                      | **0.966**       |

For this pair, accuracy_chosen = 0.966. The rejected variant with gross layout errors gives 0.71.

---

### 2.2 CriticUnit — critique accuracy (precision / recall / F1)

CriticUnit takes the design tree, html+css, and renders at 4 resolutions plus visual-diff, and returns a QualityVerdict: accuracy 0..1 and a list of defects with type and location.

**Important:** accuracy inside QualityVerdict is a semantic field of the verdict that the unit predicts. To measure the quality of the unit itself, something else is used — precision/recall/F1 over detected defects against ground-truth.

**Formulas:**

```
TP = |detected_defects ∩ ground_truth_defects|   (match by type and location)
FP = |detected_defects| - TP                     (extra defects)
FN = |ground_truth_defects| - TP                 (missed defects)

precision  = TP / (TP + FP)
recall     = TP / (TP + FN)
F1         = 2 × precision × recall / (precision + recall)

accuracy_critic = F1   (primary CriticUnit metric for the DPO record)
```

Match by type and location: the defect type must match exactly; the location counts as matched if the bounding box intersects the ground-truth box with IoU >= 0.5.

**Example numbers (one held-out reference):**

- ground_truth_defects: 8 defects (button color, padding, h2 font, etc.)
- detected_defects: 9 defects
- TP = 7, FP = 2, FN = 1
- precision = 7/9 = 0.778, recall = 7/8 = 0.875, F1 = 0.824
- accuracy_critic (chosen) = 0.824

Rejected variant: TP = 4, FP = 5, FN = 4 → precision = 0.444, recall = 0.500, F1 = 0.471.

---

## 3. Training Quality

### 3.1 Reward margin

For each DPO pair: reward_margin = accuracy_chosen - accuracy_rejected

Aggregate over held-out: mean_reward_margin — the mean over 50 held-out pairs.
- CriticUnit target: mean_reward_margin >= 0.20
- ImproverUnit target: mean_reward_margin >= 0.15

Below the threshold, the unit does not distinguish good solutions from bad ones.

### 3.2 Preference win-rate

win_rate = count(accuracy_chosen > accuracy_rejected) / 50

Target for both units: win_rate >= 0.80 on held-out. Below 0.75, the unit is weakly trained.

**Example:** 43 pairs out of 50 gave accuracy_chosen > accuracy_rejected → win_rate = 0.86. Passes.

### 3.3 Coverage of new defect classes

After each batch, compute the fraction of held-out pairs in which the unit correctly detected a defect type not seen in train. Target: > 0.30 (at least 30% of new classes detected). If 0, the unit memorized rather than generalized.

### 3.4 Stability under candidate permutation

For 10 random held-out examples, swap the candidate order (chosen/rejected permuted) and rerun inference. accuracy_critic must not change by more than ±0.05. A violation is a sign of positional leakage.

---

## 4. Speed

Measured on a single inference run with batch=1 on held-out. All fields are mandatory; UNKNOWN is not accepted for the PASS_AS_TRAINED_FEATURE stage.

| Field              | Description                                          | Target level    |
|--------------------|------------------------------------------------------|-----------------|
| throughput         | pairs/sec at batch=8                                 | >= 2 pairs/sec  |
| cpu_ms             | milliseconds per pair (1 inference call)             | <= 800 ms       |
| ram_mb             | peak RAM during inference                            | <= 512 MB       |
| total_train_time   | full training time of a 50-pair batch, seconds       | <= 300 s        |
| inference_p95_ms   | 95th percentile of inference latency, ms             | <= 1200 ms      |

**Example metrics row (ImproverUnit, batch A-LM-04, run 2026-06-22):**

```json
{
  "unit": "ImproverUnit",
  "batch": "A-LM-04",
  "throughput_pairs_per_sec": 3.1,
  "cpu_ms_per_pair": 320,
  "ram_mb_peak": 410,
  "total_train_time_sec": 187,
  "inference_p95_ms": 640
}
```

---

## 5. Ablation / anti-determinism

This is the most critical section: the unit must not disguise a deterministic lookup (regex, BM25, dictionary) as a trained answer.

### 5.1 Run without trained state (untrained ablation)

Load the unit without a checkpoint (cold state). Feed the same 50 held-out examples. Expected result: the unit honestly returns not_ready / DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING, NOT a pseudo-smart answer. If cold-state accuracy > 0.40, that is a suspicion of a deterministic shortcut; investigation and code fixes are required.

**Example of honest cold-state output (CriticUnit):**

```json
{
  "status": "not_ready",
  "reason": "checkpoint_missing",
  "verdict": null
}
```

### 5.2 Leakage-rate (exact memory / source)

For 5 held-out examples, check: is the exact prompt from held-out present in the train set? If yes, that is data leakage. leakage_rate = count(held-out_in_train) / 5. Norm: 0.0. If leakage_rate > 0, rebuild held-out; the metric is invalid.

### 5.3 Permutation test (anti-positional)

Described in paragraph 3.4. If delta accuracy under permutation > 0.05, the metric is open and the unit is not accepted as trained.

### 5.4 Firewall: what is NOT accepted as proof of "trained"

- validation_only: true
- metric_type: static / contract / mixed
- cpu_ms: UNKNOWN, ram_mb: UNKNOWN
- a report without numeric runtime metrics
- a checkpoint without held-out F1/accuracy
- a pass report without an ablation row
- a source/license gate without an explicit `passed`
- a product or semantic claim without a QA/eval evidence reference

---

## 6. Two PASS Stages

### PASS_AS_HONEST_UNTRAINED_SCAFFOLD

A unit skeleton is considered honest if ALL conditions hold:
- cold-state inference returns an explicit not_ready with no semantic content;
- no deterministic answer (regex / dictionary / BM25 / flow-index) passed off as "smart";
- no fake DPO pairs, fake checkpoint, or fabricated metrics;
- through a real host entrypoint, an honest degraded answer arrives (for example, an empty defect list with a not_ready flag);
- the cold-state test passes automatically (assert: status == "not_ready").

This is a milestone, not a finished feature. Report as PASS_AS_HONEST_UNTRAINED_SCAFFOLD.

### PASS_AS_TRAINED_FEATURE

A unit is considered trained if ALL conditions hold:
- a checkpoint exists with a known path in the manifest;
- held-out numeric metrics: win_rate >= 0.80, mean_reward_margin >= 0.15 (Improver) / 0.20 (Critic), F1/accuracy_improver recorded;
- ablation passed: cold-state → not_ready, leakage_rate = 0.0, delta_accuracy_permutation <= 0.05;
- fresh-load: checkpoint unloaded, reloaded from scratch, metrics reproduced (tolerance ±0.02);
- backup: before overwriting the checkpoint, a backup reference is saved, or it is explicitly recorded why a backup is not required;
- continue-training: +10 train pairs run on top of the checkpoint, a new checkpoint saved, the metric did not drop;
- export/import lifecycle verified: `ExportAsync` is treated as metadata-only, `ImportAsync` does not replace `LoadCheckpointAsync`;
- source/license gate = passed for all accepted DPO pairs, including distinguishing Personal and Global legs;
- QA/eval evidence attached before any semantic/product claim;
- e2e through a real host entrypoint: at least 3 scenarios with real inputs and measured responses;
- all speed fields (paragraph 4) are not UNKNOWN.

Report as PASS_AS_TRAINED_FEATURE only when all conditions pass.

---

## 7. Tier Targets (provisional — Luba decides)

| Tier  | accuracy_improver target | accuracy_critic target (F1) | cpu_ms ceiling | Note                    |
|-------|--------------------------|-----------------------------|----------------|-------------------------|
| free  | >= 0.80                | >= 0.70                   | <= 1200 ms     | placeholder, Luba to confirm |
| paid  | >= 0.90                | >= 0.82                   | <= 800 ms      | placeholder, Luba to confirm |
| pro   | >= 0.95                | >= 0.90                   | <= 500 ms      | placeholder, Luba to confirm |

These values are chosen as guidelines based on the range of DPO pairs in batch A-LM-04; the final thresholds are confirmed by Luba before weight promotion.

---

## 8. Weight Promotion Gate (A-LM-03)

Bootstrap → trained promotion is allowed only after the training runner publishes validated weights. Promotion requires:

| Condition                       | CriticUnit                          | ImproverUnit                         |
|---------------------------------|-------------------------------------|--------------------------------------|
| win_rate (held-out, 50 pairs)   | >= 0.80                             | >= 0.80                              |
| mean_reward_margin              | >= 0.20                             | >= 0.15                              |
| ablation cold-state             | not_ready when checkpoint absent    | not_ready when checkpoint absent     |
| leakage_rate                    | 0.0                                 | 0.0                                  |
| delta_accuracy_permutation      | <= 0.05                             | <= 0.05                              |
| fresh-load reproduction         | tolerance ±0.02                     | tolerance ±0.02                      |
| backup checkpoint               | ref present or waiver justified     | ref present or waiver justified      |
| source/license gate             | passed                              | passed                               |
| QA/eval evidence                | attached before product claim       | attached before product claim        |
| cpu_ms, ram_mb                  | numbers, not UNKNOWN                | numbers, not UNKNOWN                 |
| continue-training checkpoint    | new checkpoint after +10 pairs      | new checkpoint after +10 pairs       |
| export/import scope             | Export metadata-only, Import no load | Export metadata-only, Import no load |
| e2e through host                | >= 3 scenarios with real inputs     | >= 3 scenarios with real inputs      |

If even one condition is not met, the runner returns PROMOTION_BLOCKED naming the unclosed condition. Promotion without closing all conditions is a phase execution error.

---

## 9. Metrics Record Form and Example Filled Rows

### Metrics record fields (runner report)

```
run_id               : unique run UUID
unit                 : CriticUnit | ImproverUnit
batch                : batch identifier (A-LM-04)
run_date             : ISO-8601 date
train_rows           : number of train pairs used
held_out_rows        : number of held-out pairs evaluated
win_rate             : fraction of pairs with accuracy_chosen > accuracy_rejected
mean_reward_margin   : mean (accuracy_chosen - accuracy_rejected)
ablation_cold_state  : passed | failed | not_run
leakage_rate         : 0.0 .. 1.0
delta_accuracy_perm  : maximum accuracy change under permutation
fresh_load_delta     : |accuracy_after_reload - accuracy_original|
backup_checkpoint_ref: backup checkpoint path or WAIVED_WITH_REASON
continue_train_ckpt  : path of the new checkpoint after continue-training
export_ref           : metadata ref after ExportAsync
import_scope_verified: true if ImportAsync is verified as metadata-only and does not replace LoadCheckpointAsync
source_license_gate  : passed | failed | not_run
qa_eval_evidence_ref : link/path to QA/eval evidence before semantic/product claim
throughput_pps       : pairs/sec (batch=8)
cpu_ms               : ms per pair
ram_mb               : MB peak
total_train_time_sec : seconds of full batch training
inference_p95_ms     : 95th percentile latency, ms
promotion_decision   : PROMOTION_ALLOWED | PROMOTION_BLOCKED
promotion_blocked_reason : (if BLOCKED) — which condition is not closed
pass_stage           : PASS_AS_HONEST_UNTRAINED_SCAFFOLD | PASS_AS_TRAINED_FEATURE | OPEN
```

---

### Example filled row — CriticUnit

```json
{
  "run_id": "c71a3f9e-0044-4b2f-aab3-d2e8c0f17b3c",
  "unit": "CriticUnit",
  "batch": "A-LM-04",
  "run_date": "2026-06-22",
  "train_rows": 50,
  "held_out_rows": 50,
  "win_rate": 0.86,
  "mean_reward_margin": 0.244,
  "ablation_cold_state": "passed",
  "leakage_rate": 0.0,
  "delta_accuracy_perm": 0.031,
  "fresh_load_delta": 0.011,
  "backup_checkpoint_ref": "checkpoints/critic/backup/A-LM-04-before-continue-v1.ckpt",
  "continue_train_ckpt": "checkpoints/critic/A-LM-04-continue-v1.ckpt",
  "export_ref": "manifests/critic/A-LM-04-v1.json",
  "import_scope_verified": true,
  "source_license_gate": "passed",
  "qa_eval_evidence_ref": "reports/critic/A-LM-04-qa-eval.json",
  "throughput_pps": 2.8,
  "cpu_ms": 410,
  "ram_mb": 388,
  "total_train_time_sec": 214,
  "inference_p95_ms": 720,
  "promotion_decision": "PROMOTION_ALLOWED",
  "promotion_blocked_reason": null,
  "pass_stage": "PASS_AS_TRAINED_FEATURE"
}
```

Interpretation: win_rate 0.86 >= 0.80; mean_reward_margin 0.244 >= 0.20; ablation passed; leakage_rate 0.0; delta_accuracy_perm 0.031 <= 0.05; source/license and QA/eval evidence closed; all speed fields are numbers. Promotion allowed.

---

### Example filled row — ImproverUnit

```json
{
  "run_id": "a88d5e12-ff33-41a7-9c6b-11f0a3b7c204",
  "unit": "ImproverUnit",
  "batch": "A-LM-04",
  "run_date": "2026-06-22",
  "train_rows": 50,
  "held_out_rows": 50,
  "win_rate": 0.82,
  "mean_reward_margin": 0.178,
  "ablation_cold_state": "passed",
  "leakage_rate": 0.0,
  "delta_accuracy_perm": 0.019,
  "fresh_load_delta": 0.008,
  "backup_checkpoint_ref": "checkpoints/improver/backup/A-LM-04-before-continue-v1.ckpt",
  "continue_train_ckpt": "checkpoints/improver/A-LM-04-continue-v1.ckpt",
  "export_ref": "manifests/improver/A-LM-04-v1.json",
  "import_scope_verified": true,
  "source_license_gate": "passed",
  "qa_eval_evidence_ref": "reports/improver/A-LM-04-qa-eval.json",
  "throughput_pps": 3.1,
  "cpu_ms": 320,
  "ram_mb": 410,
  "total_train_time_sec": 187,
  "inference_p95_ms": 640,
  "promotion_decision": "PROMOTION_ALLOWED",
  "promotion_blocked_reason": null,
  "pass_stage": "PASS_AS_TRAINED_FEATURE"
}
```

Interpretation: win_rate 0.82 >= 0.80; mean_reward_margin 0.178 >= 0.15; ablation passed; leakage 0.0; delta_perm 0.019 <= 0.05. Promotion allowed.

---

*End of guide. Version 1.0 — 2026-06-22. Tier targets (paragraph 7) are provisional, confirmed by Luba.*
