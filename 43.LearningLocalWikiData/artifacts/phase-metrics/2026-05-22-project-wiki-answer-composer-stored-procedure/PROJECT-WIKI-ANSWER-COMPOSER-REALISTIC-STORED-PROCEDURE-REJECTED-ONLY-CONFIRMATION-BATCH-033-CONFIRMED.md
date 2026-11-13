# Project Wiki Answer Composer - Procedure Rejected-Only Confirmation Batch 033

report_scope: DPO training confirmation report
unit: WikiAnswerComposer
unit_id: project-wiki-answer-composer-unit
batch_id: project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-confirmation-batch-033
status: PROMOTED_MILESTONE_FULL_WIKI_DONE_FALSE
full_wiki_done_claim: false

## Source Of Truth

summary:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-confirmation-batch-033\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-confirmation-batch-033-project-wiki-answer-composer-unit-20260522003408\project-wiki-dpo-metric-summary.json

diagnostics:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-confirmation-batch-033\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-confirmation-batch-033-project-wiki-answer-composer-unit-20260522003408\dpo-pair-diagnostics.jsonl

generated_manifest:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-confirmation-batch-033\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260522003731\generated_capability_manifest.json

manifest_sha256:
26D9A335D13727B51A47C7DF7B722F3A9B5C1D9226ABAF78D25088403E40C7BB

policy_checkpoint:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-confirmation-batch-033\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-confirmation-batch-033-project-wiki-answer-composer-unit-20260522003408\checkpoints\policy_final.bin

reference_checkpoint:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-confirmation-batch-033\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-confirmation-batch-033-project-wiki-answer-composer-unit-20260522003408\checkpoints\reference_base.bin

## Raw Metrics

train_rows: 50
heldout_rows: 50
epoch_count: 5
learning_rate: 0.0005000000237487257

heldout_accuracy: 0.9800000190734863
heldout_reward_margin: 0.04243287444114685
heldout_loss: 0.6722105145454407

final_accuracy: 0.9200000047683716
final_reward_margin: 0.03681076303124428
final_loss: 0.6749705445766448

cpu_time_ms: 156203.125
average_cpu_percent: 12.627326309074201
peak_ram_mb: 97.84375
elapsed_ms: 203259.7024

cpu_ms_per_train_row: 3124.0625
elapsed_ms_per_train_row: 4065.194048

## Confirmation Decision

batch_028_heldout_accuracy: 0.9800000190734863
batch_028_heldout_reward_margin: 0.0335775688290596
batch_032_heldout_accuracy: 0.9800000190734863
batch_032_heldout_reward_margin: 0.03833292797207832
batch_033_heldout_accuracy: 0.9800000190734863
batch_033_heldout_reward_margin: 0.04243287444114685

heldout_margin_delta_vs_batch_028: 0.00885530561208725
heldout_margin_delta_vs_batch_032: 0.00409994646906853

Batch 033 confirms the batch-032 rejected-only contrast improvement and supersedes batch 028 as the stored-procedure answer-composer milestone.

This is not a full wiki completion claim. The procedure_name_only rejected type still has one held-out negative-margin row.

## Held-Out Metrics By Rejected Type

missing_dotnet_callers:
count: 10
accuracy: 1
avg_margin: 0.048871640861034391
min_margin: 0.022097006440162659
max_overlap: 0.1

missing_result_shape:
count: 10
accuracy: 1
avg_margin: 0.045085199549794196
min_margin: 0.035214804112911224
max_overlap: 0.16666666666666666

missing_sql_parameters:
count: 10
accuracy: 1
avg_margin: 0.053234150260686876
min_margin: 0.016787149012088776
max_overlap: 0.066666666666666666

missing_table_side_effects:
count: 10
accuracy: 1
avg_margin: 0.03940311744809151
min_margin: 0.0011543259024620056
max_overlap: 0.1

procedure_name_only:
count: 10
accuracy: 0.9
avg_margin: 0.025570259243249894
min_margin: -0.020952314138412476
max_overlap: 0.054054054054054057

## Residual

project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-confirmation-batch-033-heldout-pair-036:
rejected_type: procedure_name_only
reward_margin: -0.020952314138412476
prompt: Before editing SQL logic for NotificationRepository.GetPendingNotifications, what exact procedure contract, inputs, outputs, side effects, and .NET callers must be cited?

## Next Action

Continue with a 50-row remediation batch for procedure_name_only residuals. The next batch must not use a broad chosen-side global token that regresses other omitted-field rejected types. It must specifically strengthen cases where the rejected side lists only the stored procedure identity while the developer asks for procedure contract, inputs, outputs, side effects, and .NET callers.
