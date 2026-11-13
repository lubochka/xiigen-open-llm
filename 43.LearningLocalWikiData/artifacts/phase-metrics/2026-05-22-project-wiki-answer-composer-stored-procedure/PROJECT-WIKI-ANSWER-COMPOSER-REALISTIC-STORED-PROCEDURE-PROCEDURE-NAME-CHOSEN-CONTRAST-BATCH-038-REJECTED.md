# Project Wiki Answer Composer - Procedure Name Chosen-Contrast Batch 038

report_scope: DPO training rejected probe report
unit: WikiAnswerComposer
unit_id: project-wiki-answer-composer-unit
batch_id: project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-chosen-contrast-batch-038
status: REJECTED_FOR_CONTINUATION_FULL_WIKI_DONE_FALSE
full_wiki_done_claim: false

## Source Of Truth

summary:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-chosen-contrast-batch-038\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-chosen-contrast-batch-038-project-wiki-answer-composer-unit-20260522054431\project-wiki-dpo-metric-summary.json

diagnostics:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-chosen-contrast-batch-038\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-chosen-contrast-batch-038-project-wiki-answer-composer-unit-20260522054431\dpo-pair-diagnostics.jsonl

generated_manifest:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-chosen-contrast-batch-038\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260522054724\generated_capability_manifest.json

manifest_sha256:
1E5C0D19521AD76C2B897BDB1A5E92017DA47FD449ACEA19DA9F387C3CBFB445

## Raw Metrics

train_rows: 50
heldout_rows: 50
epoch_count: 5
learning_rate: 0.0002500000118743628

heldout_accuracy: 0.9399999976158142
heldout_reward_margin: 0.045276764780282974
heldout_loss: 0.6708521246910095

final_accuracy: 0.9400000035762787
final_reward_margin: 0.04248667765408754
final_loss: 0.6722084617614746

cpu_time_ms: 133187.5
average_cpu_percent: 12.630341178752623
peak_ram_mb: 101.94921875
elapsed_ms: 173100.7084

cpu_ms_per_train_row: 2663.75
elapsed_ms_per_train_row: 3462.014168

## Rejection Decision

batch_036_heldout_accuracy: 0.9800000190734863
batch_036_heldout_reward_margin: 0.04650808870792389
batch_038_heldout_accuracy: 0.9399999976158142
batch_038_heldout_reward_margin: 0.045276764780282974

heldout_accuracy_delta_vs_batch_036: -0.0400000214576721
heldout_margin_delta_vs_batch_036: -0.00123132392764092

Batch 038 is rejected because both held-out accuracy and held-out reward margin regressed against the accepted batch-036 baseline.

## Held-Out Metrics By Rejected Type

missing_dotnet_callers:
count: 10
accuracy: 1
avg_margin: 0.056810092180967331
min_margin: 0.02890719473361969
max_overlap: 0.1

missing_result_shape:
count: 10
accuracy: 1
avg_margin: 0.052140134572982791
min_margin: 0.043031126260757446
max_overlap: 0.16666666666666666

missing_sql_parameters:
count: 10
accuracy: 1
avg_margin: 0.060460397601127626
min_margin: 0.017375841736793518
max_overlap: 0.066666666666666666

missing_table_side_effects:
count: 10
accuracy: 1
avg_margin: 0.043482846766710284
min_margin: 0.0031222328543663025
max_overlap: 0.1

procedure_name_only:
count: 10
accuracy: 0.7
avg_margin: 0.013490381091833115
min_margin: -0.02317732572555542
max_overlap: 0.054054054054054057

## Failed Held-Out Rows

project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-chosen-contrast-batch-038-heldout-pair-026:
rejected_type: procedure_name_only
reward_margin: -0.02317732572555542

project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-chosen-contrast-batch-038-heldout-pair-036:
rejected_type: procedure_name_only
reward_margin: -0.02151794731616974

project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-chosen-contrast-batch-038-heldout-pair-046:
rejected_type: procedure_name_only
reward_margin: -0.011143013834953308

## Diagnosis

Adding procedure_name_only-specific chosen-side contract tokens did not improve the residual. It widened the number of failed procedure_name_only held-out rows from one to three.

## Next Action

Do not continue from batch-038. Revert the chosen-side procedure_name_only contrast token change. Continue from batch-036 checkpoint.
