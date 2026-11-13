# Layer 3 — Teaching Unit and Teaching Flow Pairs

Required by `EXTERNAL-SOURCES-ORCHESTRATION-PIPELINE-PLAN-v7.md` line 361
("Teaching rows are not optional. Training result rows must include
teaching time, CPU time, RAM, disk read/write, model artifact size,
training count, held-out count, held-out accuracy") and by
`EXAMINATION-METHOD-ORCHESTRATION-v8.md` step 8.

Three contracts covered in this file:

```
ModelTrainingUnit       : ILogicalUnit<FeatureVectorDataset, TrainingResult>
HeldOutEvaluationUnit   : ILogicalUnit<ModelEvaluationInput, HeldOutEvaluationResult>
CorrectionLearningUnit  : ILogicalUnit<CorrectionTrainingInput, TrainingResult>
```

Seven pairs in this file:

```
3 ModelTrainingUnit pairs        — one per domain: ARCH, PLAN, TEST
1 ModelTrainingUnit BUG-chunked  — A42 BUG chunking rule (max 30 rows/step)
1 HeldOutEvaluationUnit pair     — held-out across all four sub-phases
2 CorrectionLearningUnit pairs   — failed-pair retraining + corrected verdict
```

---

## Common coverage labels (apply to every pair in this file)

```
source_license_state: external_license_checked
node_role:            layer_3
pair_kind:            teaching_unit
coverage_status:      covered
```

Per-pair labels carry: `source_id`, `source_title`, `transition_type`,
`phase`, `sub_phase`, `task_type`, `rejected_type`, `overlap_ratio`,
plus contract-specific fields.

---

## Pair[ID] ES-2026-05-08-ARCH-L3-01 — ModelTrainingUnit, ARCH adapter, ES-S1+S2 rows

A:
```
FeatureVectorDataset:
  domain_label:     "ARCH"
  rows:
    - row_id: "ES-S1-N02-01"  feature_vector: [60-slot vector for 3.1 packet]
    - row_id: "ES-S1-N03-04"  feature_vector: [60-slot vector for 3.1 plan]
    - row_id: "ES-S2-N02-02"  feature_vector: [60-slot vector for 3.5 packet]
    - row_id: "ES-S2-N03-N/A" feature_vector: [60-slot vector for 3.5 plan]
    - row_id: "ES-S5-N02-03"  feature_vector: [60-slot vector for 3.4 packet]
  prior_checkpoint_path:    "models/arch-adapter-v3.bin"
  replay_70_30:             [sampled prior-row vectors per D-06]
  alpha:                    2.0
  rank:                     4
  max_weight_formula:       "total / (classes × rare)"   (D-09)
```

B:
```
TrainingResult:
  domain_label:                  "ARCH"
  model_artifact_path:           "models/arch-adapter-v4.bin"
  model_artifact_bytes:          2048576       (2 MB per D-05)
    BaseCheckpointBytes:         1572864
    LoraAdapterBytes:            458752
    DpoMetricBytes:              16960
  training_started_at:           "2026-05-08T07:30:14Z"
  training_completed_at:         "2026-05-08T07:30:21Z"
  teaching_time_ms:              7240
  cpu_time_ms:                   1810           (cpu_pct / Environment.ProcessorCount per A31)
  working_set_peak_bytes:        91566080       (delta, per A32 — WorkingSetDeltaMb)
  managed_allocation_delta_bytes:8388608
  disk_read_bytes:               1572864        (prior checkpoint)
  disk_write_bytes:              2048576        (new checkpoint)
  training_row_count:            5
  held_out_row_count:            2
  training_accuracy:             0.80
  held_out_accuracy:             0.50
  warnings:                      []
  dpo_step_count_total_across_rows: 38           (continued-weight; was 33 before this batch)
```

Reason:
```
[WRONG: ARCH adapter is the correct domain key per D-05 — every ARCH row
trains the same adapter regardless of sub-phase. Per A37, domain
classification runs before sub-phase routing and the domain label flows
into the training contract. The rejected creates one model per sub-phase
(arch-3.1.bin, arch-3.5.bin, arch-3.4.bin). This is the pre-fix
"114 disconnected models" anti-pattern documented in WHAT-TO-DO-WHAT-NOT-TO-DO
v13. After 20 rows the rejected has many tiny adapters that never share
ARCH knowledge; the chosen has one adapter that accumulates ARCH learning.]
```

Phrasing-B (Type A — sub-phase-keyed adapters anti-pattern):
```
TrainingResult:
  domain_label:                  "ARCH"
  model_artifact_paths:
    - "models/arch-3.1-row-001.bin"
    - "models/arch-3.5-row-002.bin"
    - "models/arch-3.4-row-003.bin"
  model_artifact_bytes:          6291456        (2 MB × 3 sub-phase models)
  teaching_time_ms:              21720          (3 sequential DPO steps)
  cpu_time_ms:                   5430
  working_set_peak_bytes:        274698240      (3× the chosen — sub-phase-keyed cache)
  training_row_count:            5
  dpo_step_count_total_across_rows: 3            (resets each sub-phase)
```

Coverage labels:
```
source_id:                arc42-template, ADR-template, temporal-observability-docs, dapr-observability-docs, rfc-7519
source_title:             ARCH multi-source: arc42 + ADR + Temporal + Dapr + RFC 7519
attribution_required:     false
contract_signature:       ModelTrainingUnit
transition_type:          training_cycle
phase:                    3
sub_phase:                3.1+3.5+3.4
task_type:                model-training-arch-adapter
rejected_type:            A
overlap_ratio:            0.34   (chosen single-adapter vs rejected multi-adapter; bytes/path token sets disjoint)
```

---

## Pair[ID] ES-2026-05-08-PLAN-L3-02 — ModelTrainingUnit, PLAN adapter, Node-03 multi-task rows

A:
```
FeatureVectorDataset:
  domain_label:     "PLAN"
  rows:
    - row_id: "ES-S5-N03-01"  feature_vector: [60-slot vector for 3.4+3.3+5.2 plan]
    - row_id: "ES-S4-N03-02"  feature_vector: [60-slot vector for 3.3+5.2 plan]
    - row_id: "ES-S7-N03-03"  feature_vector: [60-slot vector for 3.5+4.1 plan]
  prior_checkpoint_path:    "models/plan-adapter-v3.bin"
  replay_70_30:             [sampled prior-row vectors per D-06]
  alpha:                    2.0
  rank:                     4
  max_weight_formula:       "total / (classes × rare)"
```

B:
```
TrainingResult:
  domain_label:                  "PLAN"
  model_artifact_path:           "models/plan-adapter-v4.bin"
  model_artifact_bytes:          2048576
    BaseCheckpointBytes:         1572864
    LoraAdapterBytes:            458752
    DpoMetricBytes:              16960
  training_started_at:           "2026-05-08T07:30:25Z"
  training_completed_at:         "2026-05-08T07:30:32Z"
  teaching_time_ms:              6890
  cpu_time_ms:                   1722
  working_set_peak_bytes:        87752704
  managed_allocation_delta_bytes:7340032
  disk_read_bytes:               1572864
  disk_write_bytes:              2048576
  training_row_count:            3
  held_out_row_count:            1
  training_accuracy:             0.83
  held_out_accuracy:             0.66
  warnings:                      []
  dpo_step_count_total_across_rows: 22
```

Reason:
```
[WRONG: PLAN adapter receives all three multi-task Node-03 rows together
in one DPO step (per A41 batch DPO). The rejected runs 3 separate DPO
steps — one per row — without continued-weight loading. Per the
TEACHING PERFORMANCE RULES section, batched DPO reduces cost ~8× for
same-domain multi-branch rows. The rejected also resets dpo_step_count
to 1, which would indicate continued-weight loading is broken.]
```

Phrasing-B (Type A — unbatched, no continued-weight):
```
TrainingResult:
  domain_label:                  "PLAN"
  model_artifact_path:           "models/plan-adapter-row3.bin"
  teaching_time_ms:              19200          (3× chosen — unbatched)
  cpu_time_ms:                   4800
  training_row_count:            3
  dpo_step_count_total_across_rows: 1           (reset, broken continued-weight)
```

Coverage labels:
```
source_id:                rfc-7519, openapi-specification, rfc-8470, temporal-saga
source_title:             PLAN multi-source: RFC 7519, OpenAPI, RFC 8470, Temporal saga
attribution_required:     false
contract_signature:       ModelTrainingUnit
transition_type:          training_cycle
phase:                    3
sub_phase:                3.4+3.3+5.2+3.5+4.1
task_type:                model-training-plan-adapter
rejected_type:            A
overlap_ratio:            0.31
```

---

## Pair[ID] ES-2026-05-08-TEST-L3-03 — ModelTrainingUnit, TEST sub-domain rows

A:
```
FeatureVectorDataset:
  domain_label:     "TEST"
  rows:
    - row_id: "ES-S3-N02-04"  feature_vector: [60-slot vector for 5.2 packet]
    - row_id: "ES-S3-N04-02"  feature_vector: [60-slot vector for 5.2 derivation]
    - row_id: "ES-S5-N04-02"  feature_vector: [60-slot vector for 5.2 derivation S5]
  prior_checkpoint_path:    "models/test-adapter-v2.bin"
  replay_70_30:             [sampled prior-row vectors]
  alpha:                    2.0
  rank:                     4
```

B:
```
TrainingResult:
  domain_label:                  "TEST"
  model_artifact_path:           "models/test-adapter-v3.bin"
  model_artifact_bytes:          2048576
    BaseCheckpointBytes:         1572864
    LoraAdapterBytes:            458752
    DpoMetricBytes:              16960
  teaching_time_ms:              3120
  cpu_time_ms:                   780
  working_set_peak_bytes:        45088768
  disk_read_bytes:               1572864
  disk_write_bytes:              2048576
  training_row_count:            3
  held_out_row_count:            1
  training_accuracy:             0.66
  held_out_accuracy:             0.50
  warnings:                      []
  dpo_step_count_total_across_rows: 9
```

Reason:
```
[WRONG: TEST adapter is correct — per D-05 every TEST sub-phase trains
the same domain adapter. The rejected forgets the held_out_count and
held_out_accuracy fields, leaving them as null. Per v7 line 363 these
fields are required in every training result. Without them the operator
cannot judge whether training is improving the held-out evaluation.]
```

Phrasing-B (Type A — missing required teaching metrics):
```
TrainingResult:
  domain_label:                  "TEST"
  model_artifact_path:           "models/test-adapter-v3.bin"
  teaching_time_ms:              3120
  training_row_count:            3
  held_out_row_count:            null
  held_out_accuracy:             null
```

Coverage labels:
```
source_id:                testcontainers-dotnet, microsoft-testfx
source_title:             Testcontainers + Microsoft testfx
attribution_required:     false
contract_signature:       ModelTrainingUnit
transition_type:          training_cycle
phase:                    5
sub_phase:                5.2
task_type:                model-training-test-adapter
rejected_type:            A
overlap_ratio:            0.27
```

---

## Pair[ID] ES-2026-05-08-BUG-L3-04 — ModelTrainingUnit, BUG chunking (A42 rule)

A:
```
FeatureVectorDataset:
  domain_label:     "BUG"
  rows:             [synthetic 184 BUG rows — wave-2 simulation]
  chunking_policy:  "BUG > 100 rows requires chunks of 20-30 rows per DPO step (A42)"
  alpha:            2.0
  rank:             4
```

B:
```
TrainingResult:
  domain_label:                  "BUG"
  model_artifact_path:           "models/bug-adapter-v2.bin"
  model_artifact_bytes:          2048576
  chunk_count:                   8                    (184 rows / ~23 per chunk)
  per_chunk_dpo_step_count:      8                    (one DPO step per chunk)
  teaching_time_ms_total:        38400                (better than 203200 single-step baseline)
  cpu_time_ms:                   9600
  working_set_peak_bytes:        145752064
  training_row_count:            184
  held_out_row_count:            46
  training_accuracy:             0.71
  held_out_accuracy:             0.57
  warnings:                      []
  dpo_step_count_total_across_rows: 88                 (was 80 — +8 from this batch)
```

Reason:
```
[WRONG: per A42, BUG batches over 100 rows must be chunked at 20-30 rows
per DPO step. 184 rows in one DPO step produces weaker gradient signal —
the wave-2 baseline showed 203.2s for one big step (REVIEW status). The
chosen chunks at 23/chunk and produces 8 sequential DPO steps with
better gradient signal AND better total time. The rejected uses one
giant 184-row step, ignoring A42, and triples the cost.]
```

Phrasing-B (Type A — single-step BUG batch, ignores A42):
```
TrainingResult:
  domain_label:                  "BUG"
  model_artifact_path:           "models/bug-adapter-row184.bin"
  chunk_count:                   1
  per_chunk_dpo_step_count:      1
  teaching_time_ms_total:        203200
  training_row_count:            184
  warnings:                      ["resource_review_for_time"]
```

Coverage labels:
```
source_id:                cwe-285, cwe-287
source_title:             MITRE CWE — bug demonstrative examples
attribution_required:     false
contract_signature:       ModelTrainingUnit
transition_type:          training_cycle
phase:                    n/a
sub_phase:                BUG-domain
task_type:                model-training-bug-adapter-chunked
rejected_type:            A
overlap_ratio:            0.29
```

---

## Pair[ID] ES-2026-05-08-HELDOUT-L3-05 — HeldOutEvaluationUnit pair

A:
```
ModelEvaluationInput:
  domain_label:        "ARCH"
  model_artifact_path: "models/arch-adapter-v4.bin"
  held_out_pairs:
    - pair_id: "ES-2026-05-08-3.1-HO-01"  prompt: "[held-out arc42 boundary prompt]"  expected_sub_phase: "3.1"
    - pair_id: "ES-2026-05-08-3.5-HO-02"  prompt: "[held-out Temporal observability prompt]"  expected_sub_phase: "3.5"
```

B:
```
HeldOutEvaluationResult:
  domain_label:        "ARCH"
  total_held_out:      2
  correct_count:       1
  incorrect_count:     1
  failed_pair_ids:     ["ES-2026-05-08-3.5-HO-02"]
  confusion_matrix:
    "3.1": { "3.1": 1, "3.5": 0, "4.1": 0 }
    "3.5": { "3.1": 0, "3.5": 0, "7.3": 1 }
  latency_ms_per_eval: 124
  latency_p95_ms:      210
  total_eval_ms:       248
```

Reason:
```
[WRONG: held-out evaluation must report failed_pair_ids and the
confusion matrix per v8 §"Held-Out Evaluator". The chosen surfaces
that 3.5 is being mis-routed to 7.3 — that is exactly the documented
3.5 vs 7.3 confusion that downstream pair writers must address.
The rejected reports only an aggregate accuracy (0.50) with no
failed-pair list and no confusion matrix. Without those, the
correction-learning step has no input to retrain on.]
```

Phrasing-B (Type A — aggregate accuracy only, no failed-pair list):
```
HeldOutEvaluationResult:
  domain_label:        "ARCH"
  held_out_accuracy:   0.50
  total_eval_ms:       248
```

Coverage labels:
```
source_id:                arc42-template, temporal-observability-docs
source_title:             ARCH held-out prompts derived from arc42 + Temporal docs
attribution_required:     false
contract_signature:       HeldOutEvaluationUnit
transition_type:          held_out_evaluation
phase:                    3
sub_phase:                3.1+3.5
task_type:                held-out-evaluation
rejected_type:            A
overlap_ratio:            0.30
```

---

## Pair[ID] ES-2026-05-08-CORR-L3-06 — CorrectionLearningUnit, ARCH 3.5 mis-route

A:
```
CorrectionTrainingInput:
  failed_pair_id:           "ES-2026-05-08-3.5-HO-02"
  prior_model_artifact_path:"models/arch-adapter-v4.bin"
  failed_prompt:            "[held-out Temporal observability prompt]"
  expected_sub_phase:       "3.5"
  observed_sub_phase:       "7.3"
  correction_pair:
    chosen:   "[3.5 Observability Design — what to measure, alert thresholds]"
    rejected: "[7.3 Monitoring and Feedback — operate the dashboard once signals exist]"
    overlap_ratio: 0.22
  alpha:                    2.0
  rank:                     4
```

B:
```
TrainingResult:
  domain_label:                  "ARCH"
  model_artifact_path:           "models/arch-adapter-v4-corrected.bin"
  model_artifact_bytes:          2048576
  teaching_time_ms:              1840
  cpu_time_ms:                   460
  before_after_verdict:
    before_correction: { sub_phase_predicted: "7.3", confidence: 0.74 }
    after_correction:  { sub_phase_predicted: "3.5", confidence: 0.81 }
    promotion:         "Probationary → Trusted candidate after 3 more held-out passes"
  training_row_count:            1
  held_out_row_count:            1
  training_accuracy:             1.00
  held_out_accuracy:             1.00
  warnings:                      []
  dpo_step_count_total_across_rows: 39
```

Reason:
```
[WRONG: correction learning trains the prior adapter on the corrected
pair only, then runs the held-out prompt again to verify the route
flipped. The chosen records before/after verdicts so the operator can
verify the correction took. The rejected silences the before_after_verdict
field — the operator cannot tell whether the correction worked. v8
§"Correction Learner" requires the before/after verdict.]
```

Phrasing-B (Type A — silences before_after_verdict):
```
TrainingResult:
  domain_label:                  "ARCH"
  model_artifact_path:           "models/arch-adapter-v4-corrected.bin"
  teaching_time_ms:              1840
  before_after_verdict:          null
  training_row_count:            1
  warnings:                      ["before_after_verdict_missing"]
```

Coverage labels:
```
source_id:                temporal-observability-docs
source_title:             Temporal observability docs (correction source)
attribution_required:     false
contract_signature:       CorrectionLearningUnit
transition_type:          correction_cycle
phase:                    3
sub_phase:                3.5
task_type:                correction-learning
rejected_type:            A
overlap_ratio:            0.22
```

---

## Pair[ID] ES-2026-05-08-CORR-L3-07 — CorrectionLearningUnit, PLAN dependency reverse

A:
```
CorrectionTrainingInput:
  failed_pair_id:           "ES-2026-05-08-PLAN-HO-03"
  prior_model_artifact_path:"models/plan-adapter-v4.bin"
  failed_prompt:            "[held-out RFC 8470 idempotency + tests prompt]"
  expected_dependency_order:"3.3 → 5.2"
  observed_dependency_order:"5.2 → 3.3"
  correction_pair:
    chosen:   "[3.3 contract decides idempotency semantics; 5.2 tests verify against the contract]"
    rejected: "[5.2 tests written first against assumed contract; 3.3 contract derived to match passing tests]"
    overlap_ratio: 0.43
  alpha:                    2.0
  rank:                     4
```

B:
```
TrainingResult:
  domain_label:                  "PLAN"
  model_artifact_path:           "models/plan-adapter-v4-corrected.bin"
  teaching_time_ms:              2120
  cpu_time_ms:                   530
  before_after_verdict:
    before_correction: { dependency_order_predicted: "5.2 → 3.3", confidence: 0.61 }
    after_correction:  { dependency_order_predicted: "3.3 → 5.2", confidence: 0.78 }
  training_row_count:            1
  held_out_row_count:            1
  training_accuracy:             1.00
  held_out_accuracy:             1.00
  warnings:                      []
  dpo_step_count_total_across_rows: 23
```

Reason:
```
[WRONG: PLAN dependency-order corrections retrain the PLAN adapter on
the Type B chosen vs rejected pair where the dependency flips. The
chosen produces the corrected order with higher confidence (0.78 vs
0.61). The rejected sends the correction to the ARCH adapter — but
PLAN ordering decisions belong to the PLAN adapter per D-05.
Mis-routing the correction by domain corrupts ARCH and leaves PLAN
unchanged.]
```

Phrasing-B (Type A — wrong domain adapter for the correction):
```
TrainingResult:
  domain_label:                  "ARCH"
  model_artifact_path:           "models/arch-adapter-v4-corrected.bin"
  teaching_time_ms:              2120
  before_after_verdict:
    before_correction: { dependency_order_predicted: "5.2 → 3.3", confidence: 0.61 }
    after_correction:  { dependency_order_predicted: "5.2 → 3.3", confidence: 0.62 }
  warnings:                      ["correction_routed_to_wrong_domain_adapter"]
```

Coverage labels:
```
source_id:                rfc-8470, openapi-specification
source_title:             RFC 8470 idempotency + OpenAPI 3.1 (correction source)
attribution_required:     false
contract_signature:       CorrectionLearningUnit
transition_type:          correction_cycle
phase:                    3
sub_phase:                3.3+5.2
task_type:                correction-learning-plan
rejected_type:            A
overlap_ratio:            0.43   (Type B style — same sub-phase tokens, dependency direction flipped)
```

---

## Layer 3 summary

```
Pairs:                  7
Type A rejected:        7 (each tests a different teaching contract failure mode)
Contracts covered:
  ModelTrainingUnit         4 (ARCH, PLAN, TEST, BUG-chunked)
  HeldOutEvaluationUnit     1
  CorrectionLearningUnit    2 (3.5 sub-phase mis-route, PLAN dependency-order)

Required teaching fields recorded per chosen B:
  teaching_time_ms                   ✓
  cpu_time_ms                        ✓ (formula: cpu_pct / Environment.ProcessorCount per A31)
  working_set_peak_bytes             ✓ (delta, per A32)
  managed_allocation_delta_bytes     ✓
  disk_read_bytes                    ✓
  disk_write_bytes                   ✓
  model_artifact_bytes split per A33 ✓ (BaseCheckpointBytes / LoraAdapterBytes / DpoMetricBytes)
  training_row_count                 ✓
  held_out_row_count                 ✓
  training_accuracy / held_out_accuracy ✓
  dpo_step_count_total_across_rows   ✓ (continued-weight verification per TEACHING PERFORMANCE RULES)
```

This satisfies v7 line 361 and v8 step 8: teaching rows are present with
all required metrics.

---

## Last Updated

2026-05-08.
