# Project Wiki Answer Composer - Realistic Stored Procedure Procedure-Name Low-Dose Batch 029

report_scope: DPO training rejection report
unit: WikiAnswerComposer
unit_id: project-wiki-answer-composer-unit
batch_id: project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-lowdose-batch-029
status: REJECTED_FOR_CONTINUATION_FULL_WIKI_DONE_FALSE
full_wiki_done_claim: false

## Source Of Truth

summary:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-lowdose-batch-029\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-lowdose-batch-029-project-wiki-answer-composer-unit-20260522000413\project-wiki-dpo-metric-summary.json

diagnostics:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-lowdose-batch-029\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-lowdose-batch-029-project-wiki-answer-composer-unit-20260522000413\dpo-pair-diagnostics.jsonl

generated_manifest:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-lowdose-batch-029\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260522000730\generated_capability_manifest.json

manifest_sha256:
1AE842C96E1D9AEF3AEFAD4E964827E41BAD6DD935B05515812FD454867B6BEB

## Raw Metrics

train_rows: 50
heldout_rows: 50

heldout_accuracy: 0.9399999976158142
heldout_reward_margin: 0.03307706117630005
heldout_loss: 0.6767958998680115

final_accuracy: 0.924000004529953
final_reward_margin: 0.029005707073956728
final_loss: 0.6788087785243988

cpu_time_ms: 152468.75
average_cpu_percent: 12.622001454416623
peak_ram_mb: 104.6171875
elapsed_ms: 197400.0771

cpu_ms_per_train_row: 3049.375
elapsed_ms_per_train_row: 3948.001542

## Rejection Reason

Batch 029 is rejected for continuation because it regressed against the batch-028 confirmed checkpoint.

batch_028_heldout_accuracy: 0.9800000190734863
batch_028_heldout_reward_margin: 0.0335775688290596
batch_029_heldout_accuracy: 0.9399999976158142
batch_029_heldout_reward_margin: 0.03307706117630005

The low-dose procedure-name replay did not close the intended residual. It also introduced a regression in missing_table_side_effects.

## Held-Out Metrics By Rejected Type

missing_dotnet_callers:
count: 5
accuracy: 1
avg_margin: 0.045215378701686862
min_margin: 0.020655445754528049
max_overlap: 0.1

missing_result_shape:
count: 10
accuracy: 1
avg_margin: 0.038849773630499837
min_margin: 0.029244422912597656
max_overlap: 0.16666666666666666

missing_sql_parameters:
count: 10
accuracy: 1
avg_margin: 0.044691380113363266
min_margin: 0.012654110789299011
max_overlap: 0.066666666666666666

missing_table_side_effects:
count: 10
accuracy: 0.9
avg_margin: 0.035457078367471695
min_margin: -0.0021288841962814331
max_overlap: 0.1

procedure_name_only:
count: 15
accuracy: 0.8666666666666667
avg_margin: 0.01585293412208557
min_margin: -0.018107704818248749
max_overlap: 0.058823529411764705

## Failed Held-Out Rows

project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-lowdose-batch-029-heldout-pair-012:
rejected_type: procedure_name_only
reward_margin: -0.002757839858531952

project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-lowdose-batch-029-heldout-pair-018:
rejected_type: missing_table_side_effects
reward_margin: -0.002128884196281433

project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-lowdose-batch-029-heldout-pair-033:
rejected_type: procedure_name_only
reward_margin: -0.018107704818248749

## Next Action

Do not continue from batch 029. Continue from the batch-028 checkpoint.

The next remediation should strengthen the chosen/rejected signal shape for complete SQL impact answers instead of increasing the procedure_name_only ratio. The issue is not absence of stored-procedure evidence; it is the contrast between complete SQL impact answer and procedure-name-only answer.
