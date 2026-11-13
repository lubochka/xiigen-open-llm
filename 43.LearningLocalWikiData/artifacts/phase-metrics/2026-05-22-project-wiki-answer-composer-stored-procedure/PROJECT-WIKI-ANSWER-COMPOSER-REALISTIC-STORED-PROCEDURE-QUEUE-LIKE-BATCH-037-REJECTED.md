# Project Wiki Answer Composer - Queue-Like Stored Procedure Batch 037

report_scope: DPO training rejected probe report
unit: WikiAnswerComposer
unit_id: project-wiki-answer-composer-unit
batch_id: project-wiki-answer-composer-signal-token-realistic-stored-procedure-queue-like-batch-037
status: REJECTED_FOR_CONTINUATION_FULL_WIKI_DONE_FALSE
full_wiki_done_claim: false

## Source Of Truth

summary:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-queue-like-batch-037\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-queue-like-batch-037-project-wiki-answer-composer-unit-20260522053816\project-wiki-dpo-metric-summary.json

diagnostics:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-queue-like-batch-037\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-queue-like-batch-037-project-wiki-answer-composer-unit-20260522053816\dpo-pair-diagnostics.jsonl

generated_manifest:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-queue-like-batch-037\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260522054129\generated_capability_manifest.json

manifest_sha256:
C23121317FBE7D498F580918859CF7819CA37E5DEC4C3D940987CB71E4A40769

## Raw Metrics

train_rows: 50
heldout_rows: 50
epoch_count: 5
learning_rate: 0.0002500000118743628

heldout_accuracy: 0.9800000190734863
heldout_reward_margin: 0.04825349897146225
heldout_loss: 0.6693804860115051

final_accuracy: 0.8800000071525573
final_reward_margin: 0.03563017701730132
final_loss: 0.6755805587768555

cpu_time_ms: 157203.125
average_cpu_percent: 12.60486501704522
peak_ram_mb: 85.71875
elapsed_ms: 193230.8525

cpu_ms_per_train_row: 3144.0625
elapsed_ms_per_train_row: 3864.61705

## Rejection Decision

batch_036_heldout_accuracy: 0.9800000190734863
batch_036_heldout_reward_margin: 0.04650808870792389
batch_037_heldout_accuracy: 0.9800000190734863
batch_037_heldout_reward_margin: 0.04825349897146225

heldout_margin_delta_vs_batch_036: 0.00174541026353836
final_accuracy_delta_vs_batch_036: -0.0399999976158142

Batch 037 is rejected for continuation because the held-out margin gain is below the 0.002 real-improvement threshold and final accuracy regressed. The residual procedure_name_only failure remains.

## Held-Out Metrics By Rejected Type

missing_dotnet_callers:
count: 10
accuracy: 1
avg_margin: 0.056326800584793092
min_margin: 0.028707504272460938
max_overlap: 0.1

missing_result_shape:
count: 10
accuracy: 1
avg_margin: 0.051542121917009354
min_margin: 0.0429324135184288
max_overlap: 0.16666666666666666

missing_sql_parameters:
count: 10
accuracy: 1
avg_margin: 0.060105306655168535
min_margin: 0.017140686511993408
max_overlap: 0.066666666666666666

missing_table_side_effects:
count: 10
accuracy: 1
avg_margin: 0.043529243767261507
min_margin: 0.00299205631017685
max_overlap: 0.1

procedure_name_only:
count: 10
accuracy: 0.9
avg_margin: 0.029764025658369064
min_margin: -0.023223876953125
max_overlap: 0.054054054054054057

## Residual

project-wiki-answer-composer-signal-token-realistic-stored-procedure-queue-like-batch-037-heldout-pair-036:
rejected_type: procedure_name_only
reward_margin: -0.023223876953125

## Diagnosis

The queue-like generic evidence slice did not close the NotificationRepository.GetPendingNotifications residual. The model already recognizes most omitted-field rejected types; the remaining failure is more likely tied to answer-composition emphasis for full contract prompts than to lack of queue-like examples alone.

## Next Action

Do not continue from batch-037. Continue from batch-036 checkpoint. The next attempt should change the curriculum shape, not just add analogous queue/read-write procedure examples.
