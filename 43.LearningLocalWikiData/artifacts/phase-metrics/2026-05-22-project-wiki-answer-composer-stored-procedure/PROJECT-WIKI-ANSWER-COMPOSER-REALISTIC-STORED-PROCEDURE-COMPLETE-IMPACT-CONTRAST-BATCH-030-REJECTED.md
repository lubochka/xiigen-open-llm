# Project Wiki Answer Composer - Complete Impact Contrast Batch 030

report_scope: DPO training rejection report
unit: WikiAnswerComposer
unit_id: project-wiki-answer-composer-unit
batch_id: project-wiki-answer-composer-signal-token-realistic-stored-procedure-complete-impact-contrast-batch-030
status: REJECTED_FOR_CONTINUATION_FULL_WIKI_DONE_FALSE
full_wiki_done_claim: false

## Source Of Truth

summary:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-complete-impact-contrast-batch-030\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-complete-impact-contrast-batch-030-project-wiki-answer-composer-unit-20260522001106\project-wiki-dpo-metric-summary.json

diagnostics:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-complete-impact-contrast-batch-030\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-complete-impact-contrast-batch-030-project-wiki-answer-composer-unit-20260522001106\dpo-pair-diagnostics.jsonl

generated_manifest:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-complete-impact-contrast-batch-030\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260522001426\generated_capability_manifest.json

manifest_sha256:
8B3FF2ADD27EAF3676AF2C3B9AF755981C634E13084E624954D734CC512E3202

## Raw Metrics

train_rows: 50
heldout_rows: 50

heldout_accuracy: 0.8999999761581421
heldout_reward_margin: 0.04304104298353195
heldout_loss: 0.6719982624053955

final_accuracy: 0.9800000011920929
final_reward_margin: 0.043955978192389014
final_loss: 0.6715652894973755

cpu_time_ms: 153015.625
average_cpu_percent: 12.673603351978722
peak_ram_mb: 101.7734375
elapsed_ms: 199540.691

cpu_ms_per_train_row: 3060.3125
elapsed_ms_per_train_row: 3990.81382

## Rejection Reason

Batch 030 is rejected for continuation. It fixed procedure_name_only but damaged other stored-procedure answer obligations.

batch_028_heldout_accuracy: 0.9800000190734863
batch_028_heldout_reward_margin: 0.0335775688290596
batch_030_heldout_accuracy: 0.8999999761581421
batch_030_heldout_reward_margin: 0.04304104298353195

The reward margin improved because the broad complete-impact tokens made some distinctions easier, but the held-out type distribution shows a real regression.

## Held-Out Metrics By Rejected Type

missing_dotnet_callers:
count: 10
accuracy: 0.6
avg_margin: 0.021202258020639419
min_margin: -0.0051343962550163269
max_overlap: 0.090909090909090925

missing_result_shape:
count: 10
accuracy: 0.9
avg_margin: 0.026952724531292917
min_margin: -0.01514921337366104
max_overlap: 0.15151515151515152

missing_sql_parameters:
count: 10
accuracy: 1
avg_margin: 0.065747100859880447
min_margin: 0.044050022959709167
max_overlap: 0.060606060606060608

missing_table_side_effects:
count: 10
accuracy: 1
avg_margin: 0.083898765593767161
min_margin: 0.048448942601680749
max_overlap: 0.090909090909090925

procedure_name_only:
count: 10
accuracy: 1
avg_margin: 0.017404374480247498
min_margin: 0.0029078498482704163
max_overlap: 0.054054054054054057

## Failed Held-Out Rows

project-wiki-answer-composer-signal-token-realistic-stored-procedure-complete-impact-contrast-batch-030-heldout-pair-010:
rejected_type: missing_dotnet_callers
reward_margin: -0.0035493820905685425

project-wiki-answer-composer-signal-token-realistic-stored-procedure-complete-impact-contrast-batch-030-heldout-pair-020:
rejected_type: missing_dotnet_callers
reward_margin: -0.005134396255016327

project-wiki-answer-composer-signal-token-realistic-stored-procedure-complete-impact-contrast-batch-030-heldout-pair-033:
rejected_type: missing_result_shape
reward_margin: -0.015149213373661041

project-wiki-answer-composer-signal-token-realistic-stored-procedure-complete-impact-contrast-batch-030-heldout-pair-035:
rejected_type: missing_dotnet_callers
reward_margin: -0.001988127827644348

project-wiki-answer-composer-signal-token-realistic-stored-procedure-complete-impact-contrast-batch-030-heldout-pair-040:
rejected_type: missing_dotnet_callers
reward_margin: -0.0047090500593185425

## Next Action

Do not continue from batch 030. Continue from the batch-028 checkpoint.

The broad COMPLETE_SQL_IMPACT_MISSING token should not be shared across all missing-field rejected types. Keep the procedure_name_only contrast specific, and restore the other rejected types to field-specific negatives.
