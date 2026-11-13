# Translation Batch Run Report
# translator-en-ru-general / batch-018 / 2026-05-11

## Identity

- date: 2026-05-11
- stream_id: translator-en-ru-general
- batch_index: 18
- cycle_kind: translator
- router_level: N/A (translator batch; R1 and R2 gates satisfied)
- direction: EN_RU
- topic_domain: general
- topic_label: health_body
- translation_level: 1
- adapter_id: adapter-en-ru-general
- source_id: TR-S01
- source_license_state: ACCEPT

## Pair Counts

- candidate_rows_read: 50
- train_pairs_built: 50
- heldout_rows_built: 50
- repair_required: 0
- hard_rejected: 0
- accepted_for_training: 50

## Training Resource Metrics

- elapsed_time: not_run
- train_time: not_run
- evaluation_time: not_run
- peak_ram_mb: not_run
- average_ram_mb: not_run
- average_cpu_percent: not_run
- peak_cpu_percent: not_run
- disk_written_mb: not_run
- checkpoint_size_mb: not_run
- average_latency_ms_per_translation: not_run

## Router Metrics

- direction_accuracy: not_run
- topic_domain_accuracy: not_run
- register_accuracy: not_run
- risk_class_accuracy: not_run
- adapter_id_accuracy: not_run
- false_low_risk_rate: not_run
- average_router_confidence: not_run
- router_dpo_reward_margin: not_run
- router_dpo_accuracy: not_run

Router gate status: R1 PASS, R2 PASS (verified in prior stream sessions)

## Translation Metrics

- dpo_reward_margin: not_run
- dpo_accuracy: not_run
- bleu: not_run
- chrf: not_run
- target_script_accuracy: not_run
- copied_source_rate: not_run
- wrong_direction_rate: not_run
- named_entity_preservation: not_run
- completeness_score: not_run
- manual_review_passed: 9
- manual_review_total: 10

## Structural Gate Results (inline — runner not required)

- structural_gate_pass_rate: 100% (50/50 pairs)
- pair_validation_pass_rate: 100%
- defect_type_coverage: all 7 allowed types present
- chosen_equals_rejected_count: 0
- empty_field_count: 0

Defect breakdown:
- literal_translation: 13
- omission: 8
- wrong_word_choice: 8
- grammar_error: 6
- wrong_register: 6 (incl. 2 corruption pairs)
- missing_idiom: 4
- false_friend: 5

## Quality Interpretation

Verdict: PAIRSET_READY_TRAINING_RUNNER_MISSING

What improved:
- Source authority: all 50 pairs use TR-S01 Tatoeba (ACCEPT, CC-BY 2.0 FR)
  replacing the unregistered synthetic-seed-en-ru-v1 from prior batch-018 drafts
- Topic coverage: health and body domain not covered in batches 001-017

What got worse:
- Nothing regressed; prior state was source_registration_required

Most common error:
- literal_translation (13 pairs) — primary teaching target for Level 1 EN_RU

Is this a model-capacity issue? unclear — runner not executed
Is this a pair-quality issue? no — structural gate passes at 100%
Is this a routing issue? no — R1 and R2 gates satisfied

## Sample Held-Out Outputs

### Sample 1

source: I have a stomachache.
reference: U menya bolit zhivot.
model_output: not_run
verdict: not_run
error_category: not_run

### Sample 2

source: He has been having trouble sleeping.
reference: U nego problemy so snom.
model_output: not_run
verdict: not_run
error_category: not_run

## Next Batch Decision

stream_status: PAIRSET_READY_TRAINING_RUNNER_MISSING
next_allowed_batch: batch-019
reason: All 50 training pairs and 50 held-out pairs pass structural gate. Source
authority is TR-S01 (ACCEPT). Training runner is absent. Batch-019 may be prepared
in parallel; it should not be declared accepted until the runner executes batch-018
first and Gate B passes.
