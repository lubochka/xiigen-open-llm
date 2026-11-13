# Project Wiki Answer Composer - Procedure Rejected-Only Contrast Batch 032

report_scope: DPO training candidate report
unit: WikiAnswerComposer
unit_id: project-wiki-answer-composer-unit
batch_id: project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-contrast-batch-032
status: CANDIDATE_IMPROVEMENT_CONFIRMATION_REQUIRED_FULL_WIKI_DONE_FALSE
full_wiki_done_claim: false

## Source Of Truth

summary:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-contrast-batch-032\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-contrast-batch-032-project-wiki-answer-composer-unit-20260522002323\project-wiki-dpo-metric-summary.json

diagnostics:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-contrast-batch-032\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-contrast-batch-032-project-wiki-answer-composer-unit-20260522002323\dpo-pair-diagnostics.jsonl

generated_manifest:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-contrast-batch-032\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260522002643\generated_capability_manifest.json

manifest_sha256:
C76EA713DCF01A6865AFC9632ECC119230C8D44F01AF365173D40250E130527D

## Raw Metrics

train_rows: 50
heldout_rows: 50

heldout_accuracy: 0.9800000190734863
heldout_reward_margin: 0.03833292797207832
heldout_loss: 0.674210786819458

final_accuracy: 0.9280000042915344
final_reward_margin: 0.03294765181839466
final_loss: 0.6768604862689972

cpu_time_ms: 153093.75
average_cpu_percent: 12.665870413729605
peak_ram_mb: 99.29296875
elapsed_ms: 199976.3005

cpu_ms_per_train_row: 3061.875
elapsed_ms_per_train_row: 3999.52601

## Candidate Reason

Batch 032 keeps the batch-028 held-out accuracy and improves held-out reward margin by more than 0.002.

batch_028_heldout_accuracy: 0.9800000190734863
batch_028_heldout_reward_margin: 0.0335775688290596
batch_032_heldout_accuracy: 0.9800000190734863
batch_032_heldout_reward_margin: 0.03833292797207832
heldout_margin_delta_vs_batch_028: 0.00475535914301872

This is not a full promotion yet because the same procedure_name_only residual remains and a confirmation batch is required.

## Held-Out Metrics By Rejected Type

missing_dotnet_callers:
count: 10
accuracy: 1
avg_margin: 0.043741294741630556
min_margin: 0.017873957753181458
max_overlap: 0.1

missing_result_shape:
count: 10
accuracy: 1
avg_margin: 0.040466128289699553
min_margin: 0.030440710484981537
max_overlap: 0.16666666666666666

missing_sql_parameters:
count: 10
accuracy: 1
avg_margin: 0.04814089834690094
min_margin: 0.015838243067264557
max_overlap: 0.066666666666666666

missing_table_side_effects:
count: 10
accuracy: 1
avg_margin: 0.037001296132802966
min_margin: 0.00039911270141601563
max_overlap: 0.1

procedure_name_only:
count: 10
accuracy: 0.9
avg_margin: 0.022315016388893126
min_margin: -0.019637204706668857
max_overlap: 0.054054054054054057

## Residual

project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-contrast-batch-032-heldout-pair-036:
rejected_type: procedure_name_only
reward_margin: -0.019637204706668854

## Next Action

Run a 50-row confirmation batch from batch-032 policy/reference. If the confirmation preserves held-out accuracy >= 0.98 and held-out reward margin >= batch-028, batch-032 can supersede batch-028 as the stored-procedure answer-composer milestone. Do not claim full wiki done.
