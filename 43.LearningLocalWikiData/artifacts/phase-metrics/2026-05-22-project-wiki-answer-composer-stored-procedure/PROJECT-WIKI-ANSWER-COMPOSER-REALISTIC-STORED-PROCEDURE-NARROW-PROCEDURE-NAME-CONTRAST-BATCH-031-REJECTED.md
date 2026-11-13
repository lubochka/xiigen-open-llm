# Project Wiki Answer Composer - Narrow Procedure-Name Contrast Batch 031

report_scope: DPO training rejection report
unit: WikiAnswerComposer
unit_id: project-wiki-answer-composer-unit
batch_id: project-wiki-answer-composer-signal-token-realistic-stored-procedure-narrow-procedure-name-contrast-batch-031
status: REJECTED_FOR_CONTINUATION_FULL_WIKI_DONE_FALSE
full_wiki_done_claim: false

## Source Of Truth

summary:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-narrow-procedure-name-contrast-batch-031\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-narrow-procedure-name-contrast-batch-031-project-wiki-answer-composer-unit-20260522001716\project-wiki-dpo-metric-summary.json

diagnostics:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-narrow-procedure-name-contrast-batch-031\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-narrow-procedure-name-contrast-batch-031-project-wiki-answer-composer-unit-20260522001716\dpo-pair-diagnostics.jsonl

generated_manifest:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-narrow-procedure-name-contrast-batch-031\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260522002037\generated_capability_manifest.json

manifest_sha256:
EF1464430361C2581832022D3F74C7713CB090A1B35B4C039A83E398EFDC0612

## Raw Metrics

train_rows: 50
heldout_rows: 50

heldout_accuracy: 0.9800000190734863
heldout_reward_margin: 0.026585852727293968
heldout_loss: 0.6799772381782532

final_accuracy: 0.9800000011920929
final_reward_margin: 0.02918794134631753
final_loss: 0.6787114667892457

cpu_time_ms: 155859.375
average_cpu_percent: 12.632495620698933
peak_ram_mb: 88.42578125
elapsed_ms: 201090.0563

cpu_ms_per_train_row: 3117.1875
elapsed_ms_per_train_row: 4021.801126

## Rejection Reason

Batch 031 is rejected for continuation because it does not improve over batch 028.

batch_028_heldout_accuracy: 0.9800000190734863
batch_028_heldout_reward_margin: 0.0335775688290596
batch_031_heldout_accuracy: 0.9800000190734863
batch_031_heldout_reward_margin: 0.026585852727293968

The batch fixed procedure_name_only, but introduced a missing_dotnet_callers failure and reduced reward margin. Keep batch 028 as the continuation baseline.

## Held-Out Metrics By Rejected Type

missing_dotnet_callers:
count: 10
accuracy: 0.9
avg_margin: 0.022647763788700103
min_margin: -0.0033433958888053894
max_overlap: 0.090909090909090925

missing_result_shape:
count: 10
accuracy: 1
avg_margin: 0.024674757942557336
min_margin: 0.0035226792097091675
max_overlap: 0.15151515151515152

missing_sql_parameters:
count: 10
accuracy: 1
avg_margin: 0.033485114574432373
min_margin: 0.015411563217639925
max_overlap: 0.060606060606060608

missing_table_side_effects:
count: 10
accuracy: 1
avg_margin: 0.034822665899991986
min_margin: 0.0052471160888671875
max_overlap: 0.090909090909090925

procedure_name_only:
count: 10
accuracy: 1
avg_margin: 0.017298948019742966
min_margin: 0.0032497420907020569
max_overlap: 0.054054054054054057

## Failed Held-Out Row

project-wiki-answer-composer-signal-token-realistic-stored-procedure-narrow-procedure-name-contrast-batch-031-heldout-pair-020:
rejected_type: missing_dotnet_callers
reward_margin: -0.0033433958888053894

## Next Action

Do not continue from batch 031. Continue from the batch-028 checkpoint.

Remove broad chosen-side COMPLETE_SQL_IMPACT_ANSWER tokens and keep the procedure_name_only rejected contrast specific. The next batch should test whether a rejected-side-only procedure-name contrast can improve the residual without shifting failures to missing_dotnet_callers.
