# Translation Look-Like Run Report Template
# Version: 1.0 - 2026-05-11
# Domain: Russian/Hebrew <-> English Translation
# Purpose: Human-readable status report for every 50-pair batch

---

## Purpose

Every translation learning batch must produce a readable report in addition to
JSON metrics. The report should let a human see, at a glance:

- what was trained
- how many pairs were used
- whether routing passed
- whether translation quality improved
- CPU/RAM/time cost
- whether the next batch should continue the same stream or move up in
  complexity

File name:

```text
LOOK-LIKE-RUN-REPORT.md
```

---

## Template

```markdown
# Translation Batch Run Report

## Identity

- date:
- stream_id:
- batch_index:
- cycle_kind: router | translator
- router_level:
- direction:
- topic_domain:
- translation_level:
- adapter_id:
- source_id:
- source_license_state:

## Pair Counts

- candidate_rows_read:
- train_pairs_built:
- heldout_rows_built:
- repair_required:
- hard_rejected:
- accepted_for_training:

## Training Resource Metrics

- elapsed_time:
- train_time:
- evaluation_time:
- peak_ram_mb:
- average_ram_mb:
- average_cpu_percent:
- peak_cpu_percent:
- disk_written_mb:
- checkpoint_size_mb:
- average_latency_ms_per_translation:

## Router Metrics

- direction_accuracy:
- topic_domain_accuracy:
- register_accuracy:
- risk_class_accuracy:
- adapter_id_accuracy:
- false_low_risk_rate:
- average_router_confidence:
- router_dpo_reward_margin:
- router_dpo_accuracy:

## Translation Metrics

- dpo_reward_margin:
- dpo_accuracy:
- bleu:
- chrf:
- target_script_accuracy:
- copied_source_rate:
- wrong_direction_rate:
- named_entity_preservation:
- completeness_score:
- manual_review_passed:
- manual_review_total:

## Quality Interpretation

Verdict:
ACCEPT_BATCH | REPAIR_BATCH | REJECT_BATCH | SOURCE_BLOCKED | TOKENIZER_BLOCKED

What improved:
- 

What got worse:
- 

Most common error:
- 

Is this a model-capacity issue?
yes | no | unclear

Is this a pair-quality issue?
yes | no | unclear

Is this a routing issue?
yes | no | unclear

## Sample Held-Out Outputs

### Sample 1

source:

reference:

model_output:

verdict: pass | fail

error_category:

### Sample 2

source:

reference:

model_output:

verdict: pass | fail

error_category:

## Next Batch Decision

stream_status:
continue_same_level | promote_to_next_level | repair_same_batch | stop_stream

next_allowed_batch:

reason:
```

---

## Interpretation Rules

### Accept Batch

Accept when:

- DPO reward margin is positive
- DPO accuracy is non-regressive
- BLEU and chrF are stable or improving
- target script accuracy is high
- copied-source and wrong-direction rates are low
- manual review does not find a systematic error
- CPU/RAM/time are acceptable for the small-model target

### Repair Batch

Repair when:

- rejected sides are weak
- [WRONG:] annotations are too generic
- chosen/rejected contrast teaches artifacts instead of translation
- one family dominates the failures

### Reject Batch

Reject when:

- source authority is invalid
- chosen equals rejected
- mojibake appears
- model learns wrong direction
- Hebrew script is corrupted
- DPO improves but translation metrics collapse
- the batch is too noisy to repair safely

---

## Required Human Summary

The final paragraph must answer:

```text
Did this 50-pair batch improve translation ability enough to continue this
stream, or should the next batch repair pair quality/routing/tokenization first?
```
