# Project Wiki Answer Composer - Conservative Continuation Batch 035

report_scope: DPO training candidate report
unit: WikiAnswerComposer
unit_id: project-wiki-answer-composer-unit
batch_id: project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-batch-035
status: CANDIDATE_IMPROVEMENT_CONFIRMATION_REQUIRED_FULL_WIKI_DONE_FALSE
full_wiki_done_claim: false

## Source Of Truth

summary:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-batch-035\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-batch-035-project-wiki-answer-composer-unit-20260522052624\project-wiki-dpo-metric-summary.json

diagnostics:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-batch-035\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-batch-035-project-wiki-answer-composer-unit-20260522052624\dpo-pair-diagnostics.jsonl

generated_manifest:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-batch-035\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260522052859\generated_capability_manifest.json

manifest_sha256:
0C4A2F2A5BE3489ACF0218FF9C05B3F9B30324DE9681CDCB3F4A79EDC625E451

policy_checkpoint:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-batch-035\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-batch-035-project-wiki-answer-composer-unit-20260522052624\checkpoints\policy_final.bin

reference_checkpoint:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-batch-035\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-batch-035-project-wiki-answer-composer-unit-20260522052624\checkpoints\reference_base.bin

## Raw Metrics

train_rows: 50
heldout_rows: 50
epoch_count: 5
learning_rate: 0.0002500000118743628

heldout_accuracy: 0.9800000190734863
heldout_reward_margin: 0.044480256736278534
heldout_loss: 0.6712136268615723

final_accuracy: 0.9200000047683716
final_reward_margin: 0.03974615730345249
final_loss: 0.6735373437404633

cpu_time_ms: 124531.25
average_cpu_percent: 12.669195976434732
peak_ram_mb: 102.0859375
elapsed_ms: 154539.0666

cpu_ms_per_train_row: 2490.625
elapsed_ms_per_train_row: 3090.781332

## Candidate Decision

batch_033_heldout_accuracy: 0.9800000190734863
batch_033_heldout_reward_margin: 0.04243287444114685
batch_035_heldout_accuracy: 0.9800000190734863
batch_035_heldout_reward_margin: 0.044480256736278534

heldout_margin_delta_vs_batch_033: 0.00204738229513168

Batch 035 is a candidate improvement because held-out accuracy stayed at 0.9800000190734863 and held-out reward margin improved by more than 0.002 over batch 033.

This is not a promotion yet. A confirmation batch is required, and full wiki done remains false because procedure_name_only still has one held-out negative-margin row.

## Held-Out Metrics By Rejected Type

missing_dotnet_callers:
count: 10
accuracy: 1
avg_margin: 0.051472131162881851
min_margin: 0.024304389953613281
max_overlap: 0.1

missing_result_shape:
count: 10
accuracy: 1
avg_margin: 0.047412550076842308
min_margin: 0.037766553461551666
max_overlap: 0.16666666666666666

missing_sql_parameters:
count: 10
accuracy: 1
avg_margin: 0.055666591972112656
min_margin: 0.017072871327400208
max_overlap: 0.066666666666666666

missing_table_side_effects:
count: 10
accuracy: 1
avg_margin: 0.040685357898473738
min_margin: 0.0016886666417121887
max_overlap: 0.1

procedure_name_only:
count: 10
accuracy: 0.9
avg_margin: 0.027164688706398009
min_margin: -0.021603494882583615
max_overlap: 0.054054054054054057

## Residual

project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-batch-035-heldout-pair-036:
rejected_type: procedure_name_only
reward_margin: -0.021603494882583618

## Next Action

Run a 50-row confirmation batch from batch-035 policy/reference with the same conservative learning rate. If confirmation preserves held-out accuracy >= 0.9800000191 and held-out reward margin >= batch-033, batch-035 can supersede batch-033 as the current stored-procedure answer-composer milestone. Do not claim full wiki done.
