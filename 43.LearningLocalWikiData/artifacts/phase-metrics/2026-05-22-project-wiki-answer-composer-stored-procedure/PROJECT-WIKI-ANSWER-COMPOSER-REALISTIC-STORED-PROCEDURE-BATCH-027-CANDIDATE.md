# Project Wiki Answer Composer Realistic Stored-Procedure Batch 027 - Candidate Improvement

Date: 2026-05-22
report_scope: runner_emitted_metrics
unit: WikiAnswerComposer
batch_id: project-wiki-answer-composer-signal-token-realistic-stored-procedure-batch-027
status: CANDIDATE_IMPROVEMENT_CONFIRMATION_REQUIRED
full_wiki_done_claim: false
training_promotion_claim: false

## Source Artifacts

run_output: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-batch-027\batch-027-run-output.json
dpo_summary: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-batch-027\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-batch-027-project-wiki-answer-composer-unit-20260521234611\project-wiki-dpo-metric-summary.json
pair_diagnostics: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-batch-027\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-batch-027-project-wiki-answer-composer-unit-20260521234611\dpo-pair-diagnostics.jsonl
runner_manifest_written: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-batch-027\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260521234932\generated_capability_manifest.json

## Raw Metrics

train_rows: 50
heldout_rows: 50
final_accuracy: 0.8920000064373016
final_reward_margin: 0.024810882061719896
final_loss: 0.6808625042438508
heldout_accuracy: 0.9599999785423279
heldout_reward_margin: 0.03072763979434967
heldout_loss: 0.6779359579086304
cpu_time_ms: 155875
average_cpu_percent: 12.621429011167923
peak_ram_mb: 85.68359375
elapsed_ms: 201247.0684
cpu_ms_per_train_row: 3117.5
elapsed_ms_per_train_row: 4024.941368

## Comparison Against Accepted Batch 024

batch_024_heldout_accuracy: 0.9200000166893005
batch_024_heldout_reward_margin: 0.023365933448076248
batch_027_heldout_accuracy_delta_vs_024: 0.039999961853027344
batch_027_heldout_margin_delta_vs_024: 0.007361706346273422

## Held-Out Rejected-Type Metrics

procedure_name_only_count: 10
procedure_name_only_accuracy: 0.8
procedure_name_only_average_margin: 0.013347472250461578
procedure_name_only_min_margin: -0.017055414617061615
procedure_name_only_max_overlap: 0.058823529411764705

missing_sql_parameters_count: 10
missing_sql_parameters_accuracy: 1.0
missing_sql_parameters_average_margin: 0.039515276253223416
missing_sql_parameters_min_margin: 0.014124296605587006
missing_sql_parameters_max_overlap: 0.06666666666666667

missing_result_shape_count: 10
missing_result_shape_accuracy: 1.0
missing_result_shape_average_margin: 0.0332873921841383
missing_result_shape_min_margin: 0.023830130696296692
missing_result_shape_max_overlap: 0.16666666666666666

missing_table_side_effects_count: 10
missing_table_side_effects_accuracy: 1.0
missing_table_side_effects_average_margin: 0.0337103184312582
missing_table_side_effects_min_margin: 0.002528950572013855
missing_table_side_effects_max_overlap: 0.1

missing_dotnet_callers_count: 10
missing_dotnet_callers_accuracy: 1.0
missing_dotnet_callers_average_margin: 0.033777753636240956
missing_dotnet_callers_min_margin: 0.010628990828990936
missing_dotnet_callers_max_overlap: 0.1

## Interpretation

The realistic stored-procedure evidence curriculum fixed the broad regression
from batches 025 and 026. It restored `missing_dotnet_callers` to 1.0 and
improved global held-out accuracy to 0.9599999785.

This is not a full done claim and not yet a stable promotion claim. The
remaining watch item is `procedure_name_only`, still at 0.8 held-out accuracy
with negative min margin.

## Next Action

Run one confirmation batch from the batch-027 checkpoint using the same
realistic stored-procedure evidence curriculum. If the confirmation preserves
held-out accuracy >= 0.92 and margin >= 0.02, update the living guide as a
stored-procedure evidence milestone. Continue treating `procedure_name_only` as
a residual watch item until it improves above 0.8.
