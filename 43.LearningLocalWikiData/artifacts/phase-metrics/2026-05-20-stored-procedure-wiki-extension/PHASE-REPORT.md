# Stored Procedure Wiki Extension Phase Report

report_scope: project-wiki-query-planning-flow stored-procedure extension
status: UNIT_EXTENSION_PROMOTED_BY_RUNNER
full_wiki_done: false
source_of_truth: runner-emitted JSON + targeted dotnet tests + rg validation inventory

## Final Promotion Run

batch_id: project-wiki-query-planning-stored-procedure-promotion-confirmation-011
training_rows: 50
heldout_rows: 50
final_loss: 0.6925483167171478
final_accuracy: 0.9560000014305114
final_reward_margin: 0.001198303601704538
heldout_loss: 0.6919364333152771
heldout_accuracy: 1
heldout_reward_margin: 0.0024235269520431757
average_cpu_percent: 10.713235530413305
cpu_time_ms: 202125
peak_ram_mb: 106.1796875
elapsed_ms: 312987.114
warnings_count: 0
publication_registered: True
manifest_path: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-20-project-wiki-stored-procedure-promotion-confirmation-011-dpo\project-wiki-query-planning-flow\project-wiki-query-planning-20260519232150\generated_capability_manifest.json

## Final Held-Out By Rejected Type

rejected_type: missing_dotnet_callers; count: 10; accuracy: 1; reward_margin: 0.000927696243161336; loss: 0.692683434486389; avg_chosen_tokens: 83.4; avg_rejected_tokens: 47
rejected_type: missing_result_shape; count: 10; accuracy: 1; reward_margin: 0.00244201662717387; loss: 0.691927033662796; avg_chosen_tokens: 88.2; avg_rejected_tokens: 41
rejected_type: missing_sql_parameters; count: 10; accuracy: 1; reward_margin: 0.00211136816069484; loss: 0.692092132568359; avg_chosen_tokens: 85.4; avg_rejected_tokens: 51
rejected_type: missing_table_side_effects; count: 10; accuracy: 1; reward_margin: 0.00325294502545148; loss: 0.691522073745727; avg_chosen_tokens: 81; avg_rejected_tokens: 50
rejected_type: procedure_name_only; count: 10; accuracy: 1; reward_margin: 0.00338360795285553; loss: 0.691456800699234; avg_chosen_tokens: 52; avg_rejected_tokens: 47

## Run Sequence

run: 2026-05-20-project-wiki-stored-procedure-parameter-table-replay-004-dpo; batch_id: project-wiki-query-planning-stored-procedure-parameter-table-replay-004; heldout_accuracy: 0.47999998927116394; heldout_reward_margin: 0.0004466514219529927; heldout_loss: 0.6929241418838501; cpu_time_ms: 213125; peak_ram_mb: 103.6484375; elapsed_ms: 368863.6326; warnings: query_planning_gate_failed_publication_skipped,query_planning_publication_requires_promotion_confirmation_batch,query_planning_heldout_accuracy_below_gate_missing_sql_parameters,query_planning_heldout_margin_negative_missing_sql_parameters; publication_registered: False
run: 2026-05-20-project-wiki-stored-procedure-parameter-table-varied-005-dpo; batch_id: project-wiki-query-planning-stored-procedure-parameter-table-replay-005; heldout_accuracy: 0.47999998927116394; heldout_reward_margin: -0.0002048873866442591; heldout_loss: 0.6932499408721924; cpu_time_ms: 241515.625; peak_ram_mb: 104.56640625; elapsed_ms: 377715.7525; warnings: query_planning_gate_failed_publication_skipped,query_planning_publication_requires_promotion_confirmation_batch,query_planning_heldout_accuracy_below_gate_missing_sql_parameters,query_planning_heldout_margin_negative_missing_sql_parameters; publication_registered: False
run: 2026-05-20-project-wiki-stored-procedure-procedure-name-replay-009-dpo; batch_id: project-wiki-query-planning-stored-procedure-procedure-name-replay-009; heldout_accuracy: 0; heldout_reward_margin: -0.0020680753514170647; heldout_loss: 0.6941819190979004; cpu_time_ms: 197203.125; peak_ram_mb: 90.4921875; elapsed_ms: 320316.1365; warnings: query_planning_gate_failed_publication_skipped,query_planning_publication_requires_promotion_confirmation_batch,query_planning_heldout_accuracy_below_gate_procedure_name_only,query_planning_heldout_margin_negative_procedure_name_only; publication_registered: False
run: 2026-05-20-project-wiki-stored-procedure-procedure-name-replay-010-dpo; batch_id: project-wiki-query-planning-stored-procedure-procedure-name-replay-010; heldout_accuracy: 1; heldout_reward_margin: 0.001960571389645338; heldout_loss: 0.692167341709137; cpu_time_ms: 197671.875; peak_ram_mb: 103.46484375; elapsed_ms: 308224.227; warnings: query_planning_gate_failed_publication_skipped,query_planning_publication_requires_promotion_confirmation_batch; publication_registered: False
run: 2026-05-20-project-wiki-stored-procedure-procedure-result-replay-008-dpo; batch_id: project-wiki-query-planning-stored-procedure-procedure-result-replay-008; heldout_accuracy: 0.800000011920929; heldout_reward_margin: 0.002713958965614438; heldout_loss: 0.691791832447052; cpu_time_ms: 197968.75; peak_ram_mb: 94.84765625; elapsed_ms: 312691.2484; warnings: query_planning_gate_failed_publication_skipped,query_planning_publication_requires_promotion_confirmation_batch,query_planning_heldout_accuracy_below_gate_procedure_name_only; publication_registered: False
run: 2026-05-20-project-wiki-stored-procedure-promotion-confirmation-007-dpo; batch_id: project-wiki-query-planning-stored-procedure-promotion-confirmation-007; heldout_accuracy: 0.800000011920929; heldout_reward_margin: 0.0013356746640056372; heldout_loss: 0.6924797892570496; cpu_time_ms: 206250; peak_ram_mb: 89.7890625; elapsed_ms: 321884.3118; warnings: query_planning_gate_failed_publication_skipped,query_planning_heldout_accuracy_below_gate_procedure_name_only,query_planning_heldout_accuracy_below_gate_missing_result_shape; publication_registered: False
run: 2026-05-20-project-wiki-stored-procedure-promotion-confirmation-011-dpo; batch_id: project-wiki-query-planning-stored-procedure-promotion-confirmation-011; heldout_accuracy: 1; heldout_reward_margin: 0.0024235269520431757; heldout_loss: 0.6919364333152771; cpu_time_ms: 202125; peak_ram_mb: 106.1796875; elapsed_ms: 312987.114; warnings: none; publication_registered: True
run: 2026-05-20-project-wiki-stored-procedure-sequence-001-dpo; batch_id: project-wiki-query-planning-stored-procedure-batch-001; heldout_accuracy: 0.23999999463558197; heldout_reward_margin: -0.00034682650584727526; heldout_loss: 0.6933214664459229; cpu_time_ms: 245578.125; peak_ram_mb: 85.00390625; elapsed_ms: 414734.5117; warnings: query_planning_gate_failed_publication_skipped,query_planning_publication_requires_promotion_confirmation_batch,query_planning_heldout_accuracy_below_gate_missing_sql_parameters,query_planning_heldout_margin_negative_missing_sql_parameters,query_planning_heldout_accuracy_below_gate_missing_result_shape,query_planning_heldout_margin_negative_missing_result_shape,query_planning_heldout_accuracy_below_gate_missing_table_side_effects,query_planning_heldout_margin_negative_missing_table_side_effects,query_planning_heldout_accuracy_below_gate_missing_dotnet_callers,query_planning_heldout_margin_negative_missing_dotnet_callers; publication_registered: False
run: 2026-05-20-project-wiki-stored-procedure-shape-fix-003-dpo; batch_id: project-wiki-query-planning-stored-procedure-shape-fix-003; heldout_accuracy: 0.5199999809265137; heldout_reward_margin: 0.0004016094608232379; heldout_loss: 0.6929471492767334; cpu_time_ms: 241843.75; peak_ram_mb: 89.4296875; elapsed_ms: 420384.1118; warnings: query_planning_gate_failed_publication_skipped,query_planning_publication_requires_promotion_confirmation_batch,query_planning_heldout_accuracy_below_gate_missing_sql_parameters,query_planning_heldout_margin_negative_missing_sql_parameters,query_planning_heldout_accuracy_below_gate_missing_table_side_effects,query_planning_heldout_margin_negative_missing_table_side_effects,query_planning_heldout_accuracy_below_gate_missing_dotnet_callers; publication_registered: False
run: 2026-05-20-project-wiki-stored-procedure-sql-parameter-replay-006-dpo; batch_id: project-wiki-query-planning-stored-procedure-sql-parameter-replay-006; heldout_accuracy: 1; heldout_reward_margin: 0.002575403545051813; heldout_loss: 0.6918604969978333; cpu_time_ms: 219734.375; peak_ram_mb: 88.015625; elapsed_ms: 344055.0438; warnings: query_planning_gate_failed_publication_skipped,query_planning_publication_requires_promotion_confirmation_batch; publication_registered: False

## Validation Inventory

project: wizGameserver; candidate_files: 46; stored_procedure_signal_lines: 0; sample: 
project: GenieDynamicServer; candidate_files: 29; stored_procedure_signal_lines: 0; sample: 
project: geniesoftGameMVPServer; candidate_files: 6; stored_procedure_signal_lines: 0; sample: 
project: genietasks; candidate_files: 37; stored_procedure_signal_lines: 0; sample: 
project: askapediaweb; candidate_files: 100; stored_procedure_signal_lines: 6; sample: C:\Projects\askapediaweb\askapedia\DbUpdates\3 - storedProceduresUpdate.sql: CREATE PROCEDURE [askapedia].[GetGenStatistics] / [GetGenStatisticsAge] / [GetGenStatisticsGender]

## Tests

dotnet targeted generator tests: passed 3/3 for stored-procedure replay batches.
dotnet targeted procedure-name shape tests: passed 2/2.
dotnet validation/planning tests: passed 4/4.

## Boundary

DNN/nopCommerce/SmartStoreNET are validation candidates only and were not used as training data in this run.
This report does not claim the whole project-local wiki is done. It claims only the stored-procedure extension of ProjectWikiQueryPlanningFlow was runner-promoted.
