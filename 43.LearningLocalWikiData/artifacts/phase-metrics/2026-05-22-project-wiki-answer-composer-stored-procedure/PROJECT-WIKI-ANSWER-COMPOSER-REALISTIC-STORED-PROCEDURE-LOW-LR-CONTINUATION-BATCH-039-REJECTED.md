# Project Wiki Answer Composer - Low-LR Continuation Batch 039

report_scope: DPO training rejected probe report
unit: WikiAnswerComposer
unit_id: project-wiki-answer-composer-unit
batch_id: project-wiki-answer-composer-signal-token-realistic-stored-procedure-low-lr-continuation-batch-039
status: REJECTED_FOR_CONTINUATION_FULL_WIKI_DONE_FALSE
full_wiki_done_claim: false

## Source Of Truth

summary:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-low-lr-continuation-batch-039\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-low-lr-continuation-batch-039-project-wiki-answer-composer-unit-20260522055042\project-wiki-dpo-metric-summary.json

diagnostics:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-low-lr-continuation-batch-039\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-low-lr-continuation-batch-039-project-wiki-answer-composer-unit-20260522055042\dpo-pair-diagnostics.jsonl

generated_manifest:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-low-lr-continuation-batch-039\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260522055420\generated_capability_manifest.json

manifest_sha256:
5F20E1A463A93BC96BD02004AD991FD95C6DBDCF490F58B61B7D3E63E3144830

## Raw Metrics

train_rows: 50
heldout_rows: 50
epoch_count: 5
learning_rate: 0.00009999999747378752

heldout_accuracy: 0.9800000190734863
heldout_reward_margin: 0.04730766639113426
heldout_loss: 0.6698393225669861

final_accuracy: 0.924000004529953
final_reward_margin: 0.043022026047110556
final_loss: 0.6719414353370666

cpu_time_ms: 162187.5
average_cpu_percent: 12.63035338497386
peak_ram_mb: 86.45703125
elapsed_ms: 218122.2603

cpu_ms_per_train_row: 3243.75
elapsed_ms_per_train_row: 4362.445206

## Rejection Decision

batch_036_heldout_accuracy: 0.9800000190734863
batch_036_heldout_reward_margin: 0.04650808870792389
batch_039_heldout_accuracy: 0.9800000190734863
batch_039_heldout_reward_margin: 0.04730766639113426

heldout_margin_delta_vs_batch_036: 0.00079957768321037

Batch 039 is rejected for continuation because the held-out margin gain is below the 0.002 real-improvement threshold and the residual procedure_name_only failure remains.

## Held-Out Metrics By Rejected Type

missing_dotnet_callers:
count: 10
accuracy: 1
avg_margin: 0.055091456323862073
min_margin: 0.027415931224823
max_overlap: 0.1

missing_result_shape:
count: 10
accuracy: 1
avg_margin: 0.05065947920084
min_margin: 0.041451364755630493
max_overlap: 0.16666666666666666

missing_sql_parameters:
count: 10
accuracy: 1
avg_margin: 0.058937836438417435
min_margin: 0.017276667058467865
max_overlap: 0.066666666666666666

missing_table_side_effects:
count: 10
accuracy: 1
avg_margin: 0.042536392062902453
min_margin: 0.0025859847664833069
max_overlap: 0.1

procedure_name_only:
count: 10
accuracy: 0.9
avg_margin: 0.029313154518604279
min_margin: -0.022538945078849792
max_overlap: 0.054054054054054057

## Residual

project-wiki-answer-composer-signal-token-realistic-stored-procedure-low-lr-continuation-batch-039-heldout-pair-036:
rejected_type: procedure_name_only
reward_margin: -0.022538945078849792

## Diagnosis

Lowering the learning rate and repeating the accepted curriculum did not close the residual. The next attempt should address negative-DPO wording: the procedure_name_only rejected side repeats many of the desired answer terms in the annotation, which can make the rejected response look relevant when the prompt asks for exact procedure contract, inputs, outputs, side effects, and callers.

## Next Action

Do not continue from batch-039. Continue from batch-036 checkpoint. Try a simplified procedure_name_only rejected annotation that still states the weakness and consequence but avoids repeating every desired answer field.
