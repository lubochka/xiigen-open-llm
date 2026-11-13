# Project Wiki Answer Composer Stored Procedure Phase Metrics v1

report_scope: project_wiki_answer_composer_stored_procedure_training_phase
unit_id: project-wiki-answer-composer-unit
unit_name: WikiAnswerComposer
report_format: raw_numbers_only
section_22_applicable: true
full_wiki_done_claim: false
training_promotion_claim: unit_signal_promoted_for_stored_procedure_answer_composition
approved_stop_reason: protocol checkpoint reached after two consecutive held-out reward margins above 0.02, persisted runner artifacts, and generated capability manifest.

## Source Artifacts

batch_023_run_output: `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-only-batch-023\batch-023-run-output.json`
batch_023_metric_summary: `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-only-batch-023\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-stored-procedure-only-batch-023-project-wiki-answer-composer-unit-20260521231244\project-wiki-dpo-metric-summary.json`
batch_023_manifest: `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-only-batch-023\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260521231557\generated_capability_manifest.json`

batch_024_run_output: `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-only-batch-024\batch-024-run-output.json`
batch_024_metric_summary: `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-only-batch-024\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-stored-procedure-only-batch-024-project-wiki-answer-composer-unit-20260521231627\project-wiki-dpo-metric-summary.json`
batch_024_manifest: `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-only-batch-024\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260521231914\generated_capability_manifest.json`

real_project_validation_report: `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-real-project-wiki-validation\REAL-PROJECT-WIKI-VALIDATION-PHASE-REPORT.md`

## Current Accepted Checkpoint

accepted_batch: project-wiki-answer-composer-signal-token-stored-procedure-only-batch-024
accepted_manifest: `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-only-batch-024\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260521231914\generated_capability_manifest.json`
accepted_policy: `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-only-batch-024\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260521231914\policy.bin`
accepted_training_run_id: project-wiki-answer-composer-20260521231914
manifest_sha256: 04D4A3CBEF86247F1A21A871F73AB99E87A3DC356B904C5421104B3A0C871D4F

## Batch Metrics

| batch | train_rows | heldout_rows | developer_reasoning_rows | wrong_annotation_rate | evidence_grounding_rate | heldout_accuracy | heldout_reward_margin | heldout_loss | final_accuracy | final_reward_margin | final_loss | cpu_time_ms | average_cpu_percent | peak_ram_mb | elapsed_ms | cpu_ms_per_train_row | elapsed_ms_per_train_row |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| batch-023 | 50 | 50 | 50 | 1.0 | 1.0 | 0.9200000167 | 0.0210307781 | 0.6827172041 | 0.9960000002 | 0.0198698722 | 0.6832764030 | 148734.375 | 12.6395197214 | 101.21484375 | 192833.3391 | 2974.6875 | 3856.666782 |
| batch-024 | 50 | 50 | 50 | 1.0 | 1.0 | 0.9200000167 | 0.0233659334 | 0.6815686226 | 1.0 | 0.0225347648 | 0.6819608796 | 128781.25 | 12.6434403001 | 99.90234375 | 166249.469 | 2575.625 | 3324.98938 |

## Promotion Evidence

consecutive_margin_gate: passed
batch_023_heldout_reward_margin: 0.0210307781
batch_024_heldout_reward_margin: 0.0233659334
batch_024_final_reward_margin: 0.0225347648
batch_024_heldout_accuracy: 0.9200000167
batch_024_final_accuracy: 1.0
generated_capability_manifest_exists: true
policy_artifact_exists: true
tokenizer_artifact_exists: true

This promotes the stored-procedure answer-composition unit signal. It does not promote the full project wiki, the question database, or all wiki units.

## Held-Out Rejected-Type Metrics

### Batch 023

| rejected_type | count | accuracy | avg_margin | min_margin | max_overlap |
|---|---:|---:|---:|---:|---:|
| procedure_name_only | 10 | 0.8 | 0.0136939526 | -0.0090089813 | 0.0588235294 |
| missing_sql_parameters | 10 | 1.0 | 0.0295594616 | 0.0173627883 | 0.08 |
| missing_result_shape | 10 | 1.0 | 0.0143634701 | 0.0051874146 | 0.2 |
| missing_table_side_effects | 10 | 1.0 | 0.0329276964 | 0.0106361397 | 0.12 |
| missing_dotnet_callers | 10 | 0.8 | 0.0146093264 | -0.0227167122 | 0.12 |

### Batch 024

| rejected_type | count | accuracy | avg_margin | min_margin | max_overlap |
|---|---:|---:|---:|---:|---:|
| procedure_name_only | 10 | 0.8 | 0.0160937414 | -0.0085631385 | 0.0588235294 |
| missing_sql_parameters | 10 | 1.0 | 0.0328385346 | 0.0214409791 | 0.08 |
| missing_result_shape | 10 | 1.0 | 0.0158845332 | 0.0053938851 | 0.2 |
| missing_table_side_effects | 10 | 1.0 | 0.0358466148 | 0.0113581680 | 0.12 |
| missing_dotnet_callers | 10 | 0.8 | 0.0161662582 | -0.0244055763 | 0.12 |

## Good DPO Pattern

Chosen rows use compact answer obligation tokens:

```text
GOOD_PROJECT_WIKI_ANSWER
PROJECT_ID_<project>
EVIDENCE_ID_<token-safe evidence id>
LOCAL_EVIDENCE_PRESENT
LOCATION_PRESENT
PARAMETERS_PRESENT
RETURNS_PRESENT
CALLERS_PRESENT
CALLEES_PRESENT
PROJECT_BOUNDARY_SAME_PROJECT
VALUE_FLOW_PRESENT
IMPACT_PATH_PRESENT
ANSWER_PERMISSION_ALLOWED
PROCEDURE_IDENTITY_PRESENT
PROCEDURE_NAME_PRESENT
STORED_PROCEDURE_PRESENT
SQL_PARAMETERS_PRESENT
SQL_INPUT_CONTRACT_PRESENT
RESULT_SHAPE_PRESENT
OUTPUT_CONTRACT_PRESENT
TABLE_SIDE_EFFECTS_PRESENT
PERSISTENCE_IMPACT_PRESENT
DOTNET_CALLERS_PRESENT
CALLER_VALUE_FLOW_PRESENT
MISSING_EVIDENCE_CHECK_PRESENT
```

This pattern improved stored-procedure answer composition because the chosen side names the exact dimensions a developer needs before changing SQL-backed logic: procedure identity, SQL input contract, result shape, table effects, .NET callers, value flow, and missing-evidence check.

## Good Rejected DPO Pattern

Rejected rows are specific and trace-checkable. They are not generic bad answers:

```text
BAD_PROJECT_WIKI_ANSWER ... ONLY_NAMES_PROCEDURE OMITS_SQL_PARAMETERS OMITS_RESULT_SHAPE OMITS_TABLE_EFFECTS OMITS_DOTNET_CALLERS ...
[WRONG: procedure_name_only gives only the procedure identity and omits SQL parameters, result shape, table side effects, and .NET callers, so the developer cannot reason about impact.]

BAD_PROJECT_WIKI_ANSWER ... SQL_PARAMETERS_MISSING INPUT_CONTRACT_MISSING CALLER_VALUES_UNKNOWN ...
[WRONG: missing_sql_parameters omits the input contract, so the developer cannot know what values enter the stored procedure.]

BAD_PROJECT_WIKI_ANSWER ... RESULT_SHAPE_MISSING OUTPUT_CONTRACT_UNKNOWN ...
[WRONG: missing_result_shape omits returned columns or result object shape, so callers cannot be safely updated.]

BAD_PROJECT_WIKI_ANSWER ... TABLE_EFFECTS_MISSING PERSISTENCE_IMPACT_UNKNOWN ...
[WRONG: missing_table_side_effects omits read/write tables and persistence effects, so the developer cannot assess data impact.]

BAD_PROJECT_WIKI_ANSWER ... DOTNET_CALLERS_MISSING VALUE_FLOW_INCOMPLETE ...
[WRONG: missing_dotnet_callers omits the C# call sites, so the wiki cannot explain which application workflows depend on the procedure.]
```

## Residual Gaps

procedure_name_only accuracy is 0.8 on batch-024 held-out and still has min_margin -0.0085631385.

missing_dotnet_callers accuracy is 0.8 on batch-024 held-out and still has min_margin -0.0244055763.

These are not blockers for this stored-procedure AnswerComposer unit-signal promotion because the global held-out accuracy is 0.9200000167 and two consecutive held-out margins are above 0.02. They remain remediation targets before any full project-wiki completion claim.

## Next Training Action

next_action: source_next_50_rows_for_weakest_measured_wiki_unit
preferred_if_continuing_answer_composer: target procedure_name_only and missing_dotnet_callers with stronger near-miss contrasts that preserve the batch-024 chosen token pattern.
do_not_do: do not report full wiki done from this phase.
do_not_do: do not treat stored-procedure evidence from test projects as training authority; project code remains validation input unless source authority is separately approved.
