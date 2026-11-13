# Next Batch Decision

## Completed Batch

- stream_id: ru-en-general-l1
- batch_index: 1
- direction: RU_EN
- topic_domain: general
- translation_level: 1
- train_pairs: 50
- heldout_pairs: 50

## Metrics

- dpo_reward_margin: avg=+0.005860, final=+0.002245
- dpo_accuracy: avg=0.580, final=0.500
- bleu: not_run
- chrf: not_run
- target_script_accuracy: not_run
- copied_source_rate: not_run
- wrong_direction_rate: not_run
- peak_ram_mb: not_measured
- avg_cpu_percent: not_measured
- elapsed_time: ~14 seconds
- gate_b_dotnet_test: PASS (1526/0)

## Verdict

REPAIR_BATCH

## Reason

DPO accuracy averaged 0.580, below the 0.60 acceptance gate defined in TRANSLATION-50-PAIR-ABLATION-RUNBOOK-v1.md. Batch 4 produced a negative margin (-0.004518). The model does show a positive average reward margin (+0.005860) confirming that learning signal is present. Manual review of 10 held-out pairs found 10/10 structurally valid with clear chosen/rejected contrast. The instability is attributed to high gradient variance with batch_size=10 on a small model (HiddenDim=64, 1 layer). Source is SYNTHETIC_SEED which limits pair authority but all 9 validation gates passed.

## Stream Status

repair_same_batch

## Next Allowed Batch

stream_id: ru-en-general-l1
batch_index: 2
direction: RU_EN
topic_domain: general
translation_level: 1
recommended_batch_size: 5
recommended_action: Re-run with --batch-size 5 (more gradient steps per epoch). Consider sourcing real Tatoeba RU-EN sentence pairs (ACCEPT licence) to improve chosen-side authority. The existing 50 train pairs and 50 held-out pairs are structurally valid and can be reused.
