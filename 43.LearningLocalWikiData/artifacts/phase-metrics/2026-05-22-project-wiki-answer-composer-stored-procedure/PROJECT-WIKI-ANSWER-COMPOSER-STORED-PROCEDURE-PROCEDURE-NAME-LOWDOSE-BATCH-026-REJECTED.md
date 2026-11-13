# Project Wiki Answer Composer Stored-Procedure Procedure-Name Low-Dose Batch 026 - Rejected

Date: 2026-05-22
report_scope: runner_emitted_metrics
unit: WikiAnswerComposer
batch_id: project-wiki-answer-composer-signal-token-stored-procedure-procedure-name-lowdose-batch-026
status: REJECTED_FOR_CONTINUATION
full_wiki_done_claim: false
training_promotion_claim: false

## Source Artifacts

run_output: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-procedure-name-lowdose-batch-026\batch-026-run-output.json
dpo_summary: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-procedure-name-lowdose-batch-026\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-stored-procedure-procedure-name-lowdose-batch-026-project-wiki-answer-composer-unit-20260521233747\project-wiki-dpo-metric-summary.json
pair_diagnostics: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-procedure-name-lowdose-batch-026\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-stored-procedure-procedure-name-lowdose-batch-026-project-wiki-answer-composer-unit-20260521233747\dpo-pair-diagnostics.jsonl
runner_manifest_written: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-procedure-name-lowdose-batch-026\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260521234108\generated_capability_manifest.json

## Raw Metrics

train_rows: 50
heldout_rows: 50
final_accuracy: 0.9600000023841858
final_reward_margin: 0.021551046315580608
final_loss: 0.682459807395935
heldout_accuracy: 0.8799999952316284
heldout_reward_margin: 0.022190170362591743
heldout_loss: 0.6821571588516235
cpu_time_ms: 154671.875
average_cpu_percent: 12.669929152955602
peak_ram_mb: 92.1796875
elapsed_ms: 200066.8494
cpu_ms_per_train_row: 3093.4375
elapsed_ms_per_train_row: 4001.336988

## Held-Out Rejected-Type Metrics

procedure_name_only_count: 15
procedure_name_only_accuracy: 0.7333333333333333
procedure_name_only_average_margin: 0.012321248650550842
procedure_name_only_min_margin: -0.016113951802253723
procedure_name_only_max_overlap: 0.058823529411764705

missing_sql_parameters_count: 10
missing_sql_parameters_accuracy: 1.0
missing_sql_parameters_average_margin: 0.032860184088349342
missing_sql_parameters_min_margin: 0.020029544830322266
missing_sql_parameters_max_overlap: 0.08

missing_result_shape_count: 10
missing_result_shape_accuracy: 1.0
missing_result_shape_average_margin: 0.0170852767303586
missing_result_shape_min_margin: 0.0056349784135818481
missing_result_shape_max_overlap: 0.2

missing_table_side_effects_count: 10
missing_table_side_effects_accuracy: 1.0
missing_table_side_effects_average_margin: 0.036767263337969779
missing_table_side_effects_min_margin: 0.010052293539047239
missing_table_side_effects_max_overlap: 0.12

missing_dotnet_callers_count: 5
missing_dotnet_callers_accuracy: 0.6
missing_dotnet_callers_average_margin: 0.011512526869773864
missing_dotnet_callers_min_margin: -0.022784139961004257
missing_dotnet_callers_max_overlap: 0.12

## Acceptance Decision

Do not accept batch 026 as the next baseline despite the runner writing a
manifest. It does not improve the accepted batch-024 held-out accuracy
0.9200000166893005, and it worsens the two residual watch types:
`procedure_name_only` and `missing_dotnet_callers`.

## Root Cause

The batch still used generic C# method evidence for stored-procedure questions.
That teaches the correct answer obligations as text, but it does not teach the
unit to bind those obligations to structured SQL evidence: procedure name, SQL
parameters, result columns, read/write tables, side effects, and C# caller chain.

## Next Action

Do not continue from batch 026.
Continue from accepted batch 024.
Use a realistic stored-procedure evidence curriculum for the next 50-row batch.
The next batch must include structured SQL evidence fields before any further
procedure-name remediation is interpreted as meaningful.
