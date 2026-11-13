# Project Wiki Answer Composer - Realistic Stored Procedure Confirmation Batch 028

report_scope: DPO training confirmation report
unit: WikiAnswerComposer
unit_id: project-wiki-answer-composer-unit
batch_id: project-wiki-answer-composer-signal-token-realistic-stored-procedure-confirmation-batch-028
status: PROMOTED_MILESTONE_FULL_WIKI_DONE_FALSE
full_wiki_done_claim: false

## Source Of Truth

summary:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-confirmation-batch-028\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-confirmation-batch-028-project-wiki-answer-composer-unit-20260521235634\project-wiki-dpo-metric-summary.json

diagnostics:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-confirmation-batch-028\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-confirmation-batch-028-project-wiki-answer-composer-unit-20260521235634\dpo-pair-diagnostics.jsonl

generated_manifest:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-confirmation-batch-028\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260521235954\generated_capability_manifest.json

manifest_sha256:
53A17E074434F8246B07B936CABDA0390BF542BD3CA1BA4BDDF6A3EF0A312CB4

## Raw Metrics

train_rows: 50
heldout_rows: 50

heldout_accuracy: 0.9800000190734863
heldout_reward_margin: 0.0335775688290596
heldout_loss: 0.6765397787094116

final_accuracy: 0.9080000054836274
final_reward_margin: 0.02765774302184582
final_loss: 0.6794653427600861

cpu_time_ms: 153546.875
average_cpu_percent: 12.63226624438696
peak_ram_mb: 92.96875
elapsed_ms: 199628.6718

cpu_ms_per_train_row: 3070.9375
elapsed_ms_per_train_row: 3992.573436

## Confirmation Comparison

previous accepted stored-procedure batch:
project-wiki-answer-composer-signal-token-stored-procedure-only-batch-024

batch_024_heldout_accuracy: 0.9200000167
batch_024_heldout_reward_margin: 0.0233659334
batch_024_final_accuracy: 1.0
batch_024_final_reward_margin: 0.0225347648

candidate batch:
project-wiki-answer-composer-signal-token-realistic-stored-procedure-batch-027

batch_027_heldout_accuracy: 0.9599999785
batch_027_heldout_reward_margin: 0.0307276398
batch_027_final_accuracy: 0.8920000064
batch_027_final_reward_margin: 0.0248108821

confirmation batch:
project-wiki-answer-composer-signal-token-realistic-stored-procedure-confirmation-batch-028

batch_028_heldout_accuracy: 0.9800000190734863
batch_028_heldout_reward_margin: 0.0335775688290596
batch_028_final_accuracy: 0.9080000054836274
batch_028_final_reward_margin: 0.02765774302184582

promotion_reason:
Batch 028 confirms the realistic stored-procedure evidence curriculum from batch 027. Held-out accuracy and reward margin both exceed the previous accepted stored-procedure milestone and remain above the minimum answer-composer unit target.

## Held-Out Metrics By Rejected Type

missing_dotnet_callers:
count: 10
accuracy: 1
avg_margin: 0.038822355866432193
min_margin: 0.014023594558238983
max_overlap: 0.1

missing_result_shape:
count: 10
accuracy: 1
avg_margin: 0.036017313227057458
min_margin: 0.02621374279260635
max_overlap: 0.16666666666666666

missing_sql_parameters:
count: 10
accuracy: 1
avg_margin: 0.042735699564218521
min_margin: 0.014381498098373411
max_overlap: 0.066666666666666666

missing_table_side_effects:
count: 10
accuracy: 1
avg_margin: 0.034774456918239591
min_margin: 0.000057883560657501221
max_overlap: 0.1

procedure_name_only:
count: 10
accuracy: 0.9
avg_margin: 0.015538034588098526
min_margin: -0.01842498779296875
max_overlap: 0.058823529411764705

## Residual Gap

residual_rejected_type: procedure_name_only
residual_failed_pair_id:
project-wiki-answer-composer-signal-token-realistic-stored-procedure-confirmation-batch-028-heldout-pair-036

residual_question:
Before editing SQL logic for NotificationRepository.GetPendingNotifications, what exact procedure contract, inputs, outputs, side effects, and .NET callers must be cited?

residual_failure:
The model still occasionally under-separates a name-only stored-procedure answer from a complete SQL impact answer when the prompt asks for all contract fields in one sentence.

next_training_action:
Continue from batch-028 checkpoint with a realistic stored-procedure low-dose procedure_name_only replay. Keep SQL parameters, result shape, table side effects, and .NET callers in the chosen answer. Do not continue from rejected batches 025 or 026.

## Claim Boundary

This report promotes only the WikiAnswerComposer stored-procedure answer-composition sub-capability. It does not claim that the full project-local wiki is complete, that all project questions are covered, or that Phase 2 question-bank training is done.
