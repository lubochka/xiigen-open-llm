# Project Wiki Answer Composer - Simplified Name Rejected Batch 040

report_scope: DPO training rejected probe report
unit: WikiAnswerComposer
unit_id: project-wiki-answer-composer-unit
batch_id: project-wiki-answer-composer-signal-token-realistic-stored-procedure-simplified-name-rejected-batch-040
status: REJECTED_FOR_CONTINUATION_FULL_WIKI_DONE_FALSE
full_wiki_done_claim: false

## Source Of Truth

summary:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-simplified-name-rejected-batch-040\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-simplified-name-rejected-batch-040-project-wiki-answer-composer-unit-20260522055726\project-wiki-dpo-metric-summary.json

diagnostics:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-simplified-name-rejected-batch-040\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-simplified-name-rejected-batch-040-project-wiki-answer-composer-unit-20260522055726\dpo-pair-diagnostics.jsonl

generated_manifest:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-simplified-name-rejected-batch-040\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260522060059\generated_capability_manifest.json

manifest_sha256:
839EA3BFF09219DDCAF08D6E44E6EFE5C991CC42016DC3C83D7B569AD3189725

## Raw Metrics

train_rows: 50
heldout_rows: 50
epoch_count: 5
learning_rate: 0.0002500000118743628

heldout_accuracy: 0.800000011920929
heldout_reward_margin: 0.03144703805446625
heldout_loss: 0.6778411865234375

final_accuracy: 0.7800000131130218
final_reward_margin: 0.027159595414996147
final_loss: 0.6799356949329376

cpu_time_ms: 164734.375
average_cpu_percent: 12.65063563222277
peak_ram_mb: 102.203125
elapsed_ms: 212954.3669

cpu_ms_per_train_row: 3294.6875
elapsed_ms_per_train_row: 4259.087338

## Rejection Decision

batch_036_heldout_accuracy: 0.9800000190734863
batch_036_heldout_reward_margin: 0.04650808870792389
batch_040_heldout_accuracy: 0.800000011920929
batch_040_heldout_reward_margin: 0.03144703805446625

heldout_accuracy_delta_vs_batch_036: -0.180000007152557
heldout_margin_delta_vs_batch_036: -0.01506105065345764

Batch 040 is rejected because simplified procedure_name_only rejected wording collapsed the target type.

## Held-Out Metrics By Rejected Type

missing_dotnet_callers:
count: 10
accuracy: 1
avg_margin: 0.056927826255559921
min_margin: 0.028591349720954895
max_overlap: 0.1

missing_result_shape:
count: 10
accuracy: 1
avg_margin: 0.052698326855897905
min_margin: 0.044045351445674896
max_overlap: 0.16666666666666666

missing_sql_parameters:
count: 10
accuracy: 1
avg_margin: 0.060425139218568805
min_margin: 0.017716497182846069
max_overlap: 0.066666666666666666

missing_table_side_effects:
count: 10
accuracy: 1
avg_margin: 0.043089972436428071
min_margin: 0.0028862953186035156
max_overlap: 0.1

procedure_name_only:
count: 10
accuracy: 0
avg_margin: -0.055906059220433235
min_margin: -0.10049915313720705
max_overlap: 0.066666666666666666

## Diagnosis

The simplified rejected side removed too many concrete negative cues. A good rejected must still name the missing obligation clearly enough for DPO; otherwise it becomes an easy but uninformative short negative and collapses the target type.

## Next Action

Do not continue from batch-040. Revert simplified procedure_name_only rejected wording. Continue from batch-036 checkpoint.
