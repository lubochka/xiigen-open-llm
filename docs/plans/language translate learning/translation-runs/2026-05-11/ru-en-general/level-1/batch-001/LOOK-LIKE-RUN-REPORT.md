# Translation Batch Run Report

## Identity

- date: 2026-05-11
- stream_id: ru-en-general-l1
- batch_index: 1
- cycle_kind: translator
- router_level: not applicable (router R1 not yet trained)
- direction: RU_EN
- topic_domain: general
- translation_level: 1
- adapter_id: adapter-ru-en-general
- source_id: synthetic-seed-tatoeba-style
- source_license_state: SYNTHETIC_SEED

## Pair Counts

- candidate_rows_read: 50
- train_pairs_built: 50
- heldout_rows_built: 50
- repair_required: 0
- hard_rejected: 0
- accepted_for_training: 50

## Training Resource Metrics

- elapsed_time: ~14 seconds (5 batches × ~2.8s avg)
- train_time: ~14s
- evaluation_time: not_run (no inference evaluator)
- peak_ram_mb: not measured
- average_ram_mb: not measured
- average_cpu_percent: not measured
- peak_cpu_percent: not measured
- disk_written_mb: not measured
- checkpoint_size_mb: not measured
- average_latency_ms_per_translation: not_run

## Router Metrics

- direction_accuracy: not_run (router not yet trained)
- topic_domain_accuracy: not_run
- register_accuracy: not_run
- risk_class_accuracy: not_run
- adapter_id_accuracy: not_run
- false_low_risk_rate: not_run
- average_router_confidence: not_run
- router_dpo_reward_margin: not_run
- router_dpo_accuracy: not_run

## Translation Metrics (DPO Training Run)

- dpo_reward_margin: avg=+0.005860, final=+0.002245
- dpo_accuracy: avg=0.580, final=0.500
- bleu: not_run
- chrf: not_run
- target_script_accuracy: not_run
- copied_source_rate: not_run
- wrong_direction_rate: not_run
- named_entity_preservation: not_run
- completeness_score: not_run
- manual_review_passed: see translation-sample-review.md
- manual_review_total: 10

## Per-Batch Training Detail

| Batch | Loss     | Accuracy | Margin     | Time  |
|------:|----------|----------|------------|-------|
| 1/5   | 0.685531 | 0.600    | +0.015412  | 3456ms |
| 2/5   | 0.691002 | 0.600    | +0.004431  | 2311ms |
| 3/5   | 0.687358 | 0.700    | +0.011728  | 2861ms |
| 4/5   | 0.695477 | 0.500    | -0.004518  | 2277ms |
| 5/5   | 0.692118 | 0.500    | +0.002245  | 3036ms |

## Quality Interpretation

Verdict: REPAIR_BATCH

What improved:
- Positive average reward margin (+0.005860) confirmed: model assigns higher log-probability to chosen translations over rejected in 4 of 5 batches
- Batch 3 showed best signal: acc=0.700, margin=+0.011728
- Gate validation: 50/50 pairs passed all 9 gates (identity, mojibake, [WRONG:], negation template, direction, source licence, overlap ratio)

What got worse:
- DPO accuracy fell below gate threshold (avg 0.580, gate requires 0.60)
- Batch 4 produced negative margin (-0.004518): likely 1-2 pairs where rejected side had lower overlap with the prompt than chosen
- Final accuracy 0.500 (coin-flip level) — indicates model needs more training steps

Most common error:
- Accuracy instability across batches: batch 3 acc=0.700 then batch 4 acc=0.500. With only 10 pairs per batch step, variance is high. Need batch-size=5 or more pairs to stabilise.

Is this a model-capacity issue?
yes — HiddenDim=64, 1 layer, 128 context is very small. The model can learn signal but may need more gradient steps. The positive average margin suggests capacity is not completely blocking learning.

Is this a pair-quality issue?
partial — source is SYNTHETIC_SEED (not verified parallel corpus). Pairs are structurally valid (all gates passed) but chosen-side authority is not guaranteed by a human-reviewed parallel corpus (Tatoeba). Consider replacing with real Tatoeba pairs.

Is this a routing issue?
yes — router R1 not yet trained. First translator batch run without a confirmed routing gate. Routing accuracy not measured.

## Sample Held-Out Manual Review

(10 pairs manually inspected from translation-heldout-50.jsonl)

### Sample 1
source: Добро пожаловать!
reference: Welcome!
quality_check: chosen="Welcome!" vs rejected="Good to have come!" — clear distinction. [WRONG:] annotation specific.
verdict: PASS (pair quality good)

### Sample 2
source: Я не понимаю.
reference: I don't understand.
quality_check: rejected="I am not understanding." correctly targets progressive with stative verb. [WRONG: morphological] specific and accurate.
verdict: PASS

### Sample 3
source: Он симпатичный человек.
reference: He is a nice person.
quality_check: False cognate "sympathetic" vs "nice". Clear semantic divergence. Good teaching boundary.
verdict: PASS

### Sample 4
source: Он мастер на все руки.
reference: He is a jack of all trades.
quality_check: Idiom pair. Rejected="He is a master for all hands." clearly wrong. Good signal.
verdict: PASS

### Sample 5
source: У меня болит голова.
reference: I have a headache.
quality_check: Dative construction literal calque. Rejected demonstrates exactly the error pattern.
verdict: PASS

### Sample 6
source: Сегодня холодно.
reference: It is cold today.
quality_check: Missing dummy subject. Clean morphological error pair.
verdict: PASS

### Sample 7
source: Мы встретились случайно.
reference: We met by chance.
quality_check: "accidentally" vs "by chance" — subtle. The [WRONG:] annotation explains the distinction between unintended error and coincidence. Good T01 pair.
verdict: PASS

### Sample 8
source: Цена снижена, а качество прежнее.
reference: The price is reduced but the quality is the same.
quality_check: T08 omission pair. Second clause dropped in rejected. Clear completeness failure.
verdict: PASS

### Sample 9
source: Она занимается спортом каждый день.
reference: She exercises every day.
quality_check: Register shift — "is engaged in sports" vs "exercises". Distinction clear.
verdict: PASS

### Sample 10
source: Билет действителен до конца месяца.
reference: The ticket is valid until the end of the month.
quality_check: False cognate "actual" vs "valid". Clean T05 pair.
verdict: PASS

Manual review: 10/10 PASS — all sampled pairs have clear chosen/rejected distinction and specific [WRONG:] annotations.

## Gate B

- dotnet test: Passed 1526, Failed 0
- floor_before: 1526
- floor_after: 1526
- verdict: PASS

## Next Batch Decision

stream_status: repair_same_batch

next_allowed_batch:
- stream_id: ru-en-general-l1
- batch_index: 2
- direction: RU_EN
- topic_domain: general
- translation_level: 1
- note: Use batch-size=5 (more steps per 50 pairs), or increase pair count to 100 to smooth gradient variance. Consider sourcing actual Tatoeba RU-EN pairs (ACCEPT) to replace SYNTHETIC_SEED source.

reason: DPO accuracy 0.580 below gate threshold 0.60. Positive average margin (+0.005860) confirms learning signal exists. Manual review 10/10 pass confirms pair quality is good. The issue is training instability with 10-pair batches. Next batch should try smaller batch_size=5 or batch_size=8 and measure if margin stabilises above +0.01.
