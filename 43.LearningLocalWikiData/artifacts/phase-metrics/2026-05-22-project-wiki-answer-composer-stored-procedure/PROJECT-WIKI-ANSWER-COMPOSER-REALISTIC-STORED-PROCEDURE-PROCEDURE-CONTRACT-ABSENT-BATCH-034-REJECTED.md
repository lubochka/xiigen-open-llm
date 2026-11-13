# Project Wiki Answer Composer - Procedure Contract-Absent Batch 034

report_scope: DPO training rejected probe report
unit: WikiAnswerComposer
unit_id: project-wiki-answer-composer-unit
batch_id: project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-contract-absent-batch-034
status: REJECTED_FOR_CONTINUATION_FULL_WIKI_DONE_FALSE
full_wiki_done_claim: false

## Source Of Truth

summary:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-contract-absent-batch-034\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-contract-absent-batch-034-project-wiki-answer-composer-unit-20260522004333\project-wiki-dpo-metric-summary.json

diagnostics:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-contract-absent-batch-034\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-contract-absent-batch-034-project-wiki-answer-composer-unit-20260522004333\dpo-pair-diagnostics.jsonl

generated_manifest:
C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-contract-absent-batch-034\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260522052246\generated_capability_manifest.json

manifest_sha256:
578EBA75D1DC79520D050B741C2ABDC86CC5C1683472269227D2460DC2089EAF

## Raw Metrics

train_rows: 50
heldout_rows: 50
epoch_count: 5
learning_rate: 0.0005000000237487257

heldout_accuracy: 0.9399999976158142
heldout_reward_margin: 0.04343295097351074
heldout_loss: 0.6717466711997986

final_accuracy: 0.8800000071525573
final_reward_margin: 0.03672449022531509
final_loss: 0.6750381553173065

cpu_time_ms: 146703.125
average_cpu_percent: 12.289198637466558
peak_ram_mb: 97.11328125
elapsed_ms: 16753706.5583

cpu_ms_per_train_row: 2934.0625
elapsed_ms_per_train_row: 335074.131166

## Rejection Decision

batch_033_heldout_accuracy: 0.9800000190734863
batch_033_heldout_reward_margin: 0.04243287444114685
batch_034_heldout_accuracy: 0.9399999976158142
batch_034_heldout_reward_margin: 0.04343295097351074

heldout_accuracy_delta_vs_batch_033: -0.0400000214576721
heldout_margin_delta_vs_batch_033: 0.00100007653236389

Batch 034 is rejected because the small margin gain came with a held-out accuracy regression and the targeted procedure_name_only type worsened.

The elapsed time is persisted as emitted by the runner. The run initially exceeded the shell tool timeout, continued as an active dotnet process, and later wrote a complete summary; CPU time and RAM are the runner-emitted values.

## Held-Out Metrics By Rejected Type

missing_dotnet_callers:
count: 10
accuracy: 1
avg_margin: 0.055459187179803845
min_margin: 0.026960372924804688
max_overlap: 0.1

missing_result_shape:
count: 10
accuracy: 1
avg_margin: 0.049636545404791829
min_margin: 0.040877051651477814
max_overlap: 0.16666666666666666

missing_sql_parameters:
count: 10
accuracy: 1
avg_margin: 0.058315327018499376
min_margin: 0.018054962158203125
max_overlap: 0.066666666666666666

missing_table_side_effects:
count: 10
accuracy: 1
avg_margin: 0.041476510465145111
min_margin: 0.00080174952745437622
max_overlap: 0.1

procedure_name_only:
count: 10
accuracy: 0.7
avg_margin: 0.012277203798294067
min_margin: -0.02924794703722
max_overlap: 0.04878048780487805

## Failed Held-Out Rows

project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-contract-absent-batch-034-heldout-pair-026:
rejected_type: procedure_name_only
reward_margin: -0.00011377036571502686

project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-contract-absent-batch-034-heldout-pair-031:
rejected_type: procedure_name_only
reward_margin: -0.012179568409919739

project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-contract-absent-batch-034-heldout-pair-036:
rejected_type: procedure_name_only
reward_margin: -0.02924794703722

## Diagnosis

The rejected-side token extension made procedure_name_only more explicit but damaged held-out discrimination. It likely made the rejected side too pattern-heavy rather than improving the chosen answer's procedure-contract obligation.

Do not continue from the batch-034 checkpoint. Do not keep the procedure_name_only rejected-token extension as the default generator shape.

## Next Action

Revert the batch-034 procedure_name_only rejected-token extension. Continue from batch-033 checkpoint with a different 50-row strategy that preserves other rejected types and improves the chosen-side contract through balanced data rather than heavier rejected-side labels.
