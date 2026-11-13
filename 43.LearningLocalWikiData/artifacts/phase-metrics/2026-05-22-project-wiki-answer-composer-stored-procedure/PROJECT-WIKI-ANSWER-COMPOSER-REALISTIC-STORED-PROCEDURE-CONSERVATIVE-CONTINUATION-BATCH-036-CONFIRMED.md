# Project Wiki Answer Composer - Conservative Continuation Confirmation Batch 036

report_scope: DPO training confirmation report
unit: WikiAnswerComposer
unit_id: project-wiki-answer-composer-unit
batch_id: project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-confirmation-batch-036
status: PROMOTED_MILESTONE_FULL_WIKI_DONE_FALSE
full_wiki_done_claim: false

## Source Of Truth

summary:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-confirmation-batch-036\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-confirmation-batch-036-project-wiki-answer-composer-unit-20260522053121\project-wiki-dpo-metric-summary.json

diagnostics:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-confirmation-batch-036\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-confirmation-batch-036-project-wiki-answer-composer-unit-20260522053121\dpo-pair-diagnostics.jsonl

generated_manifest:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-confirmation-batch-036\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260522053411\generated_capability_manifest.json

manifest_sha256:
86CCAEAB8326509866849BC98903A7323C4B713531AFE44335AB525B3015DA8B

policy_checkpoint:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-confirmation-batch-036\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-confirmation-batch-036-project-wiki-answer-composer-unit-20260522053121\checkpoints\policy_final.bin

reference_checkpoint:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-confirmation-batch-036\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-confirmation-batch-036-project-wiki-answer-composer-unit-20260522053121\checkpoints\reference_base.bin

## Raw Metrics

train_rows: 50
heldout_rows: 50
epoch_count: 5
learning_rate: 0.0002500000118743628

heldout_accuracy: 0.9800000190734863
heldout_reward_margin: 0.04650808870792389
heldout_loss: 0.6702278256416321

final_accuracy: 0.9200000047683716
final_reward_margin: 0.04167387414723635
final_loss: 0.6725978446006775

cpu_time_ms: 132640.625
average_cpu_percent: 12.64151142279896
peak_ram_mb: 101.7890625
elapsed_ms: 170532.5405

cpu_ms_per_train_row: 2652.8125
elapsed_ms_per_train_row: 3410.65081

## Confirmation Decision

batch_033_heldout_accuracy: 0.9800000190734863
batch_033_heldout_reward_margin: 0.04243287444114685
batch_035_heldout_accuracy: 0.9800000190734863
batch_035_heldout_reward_margin: 0.044480256736278534
batch_036_heldout_accuracy: 0.9800000190734863
batch_036_heldout_reward_margin: 0.04650808870792389

heldout_margin_delta_vs_batch_033: 0.00407521426677704
heldout_margin_delta_vs_batch_035: 0.00202783197164536

Batch 036 confirms the batch-035 conservative continuation improvement and supersedes batch 033 as the current stored-procedure answer-composer milestone.

This is not a full wiki completion claim. The procedure_name_only rejected type still has one held-out negative-margin row.

## Held-Out Metrics By Rejected Type

missing_dotnet_callers:
count: 10
accuracy: 1
avg_margin: 0.05406632423400879
min_margin: 0.026533707976341248
max_overlap: 0.1

missing_result_shape:
count: 10
accuracy: 1
avg_margin: 0.049739017337560656
min_margin: 0.04039011150598526
max_overlap: 0.16666666666666666

missing_sql_parameters:
count: 10
accuracy: 1
avg_margin: 0.058021908253431322
min_margin: 0.017242714762687683
max_overlap: 0.066666666666666666

missing_table_side_effects:
count: 10
accuracy: 1
avg_margin: 0.041998539119958878
min_margin: 0.0023124665021896358
max_overlap: 0.1

procedure_name_only:
count: 10
accuracy: 0.9
avg_margin: 0.02871464714407921
min_margin: -0.022266104817390438
max_overlap: 0.054054054054054057

## Residual

project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-confirmation-batch-036-heldout-pair-036:
rejected_type: procedure_name_only
reward_margin: -0.022266104817390442

## Next Action

Do not claim full wiki done. Continue training or data redesign specifically for the residual procedure_name_only held-out row shape where the prompt asks for exact procedure contract, inputs, outputs, side effects, and .NET callers.
