# ProjectWikiQueryPlanningFlow Promotion Report

report_scope: dpo_training_promotion_report
unit_id: project-wiki-query-planning-flow
claim: PROMOTED_UNIT_ONLY
whole_wiki_done: false
all_project_wikis_covered: false
question_database_done: false

## Source Of Truth

- batch output: `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-19-project-wiki-query-planning-sequence-002-dpo\batch-025-run-output.json`
- pair diagnostics: `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-19-project-wiki-query-planning-sequence-002-dpo\dpo-runs\project-wiki-query-planning-flow\project-wiki-query-planning-promotion-confirmation-025-project-wiki-query-planning-flow-20260519175259\dpo-pair-diagnostics.jsonl`
- manifest: `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-19-project-wiki-query-planning-sequence-002-dpo\project-wiki-query-planning-flow\project-wiki-query-planning-20260519175400\generated_capability_manifest.json`

## Promotion Metrics

batch_id: project-wiki-query-planning-promotion-confirmation-025
training_rows: 50
heldout_rows: 50
final_accuracy: 0.9600000023841858
final_reward_margin: 0.002415243291761726
final_loss: 0.6919407427310944
heldout_accuracy: 0.9800000190734863
heldout_reward_margin: 0.002355672884732485
heldout_loss: 0.6919702887535095
average_cpu_percent: 12.728392094850138
total_cpu_time_ms: 20796.875
peak_working_set_mb: 105.9140625
total_elapsed_ms: 60713.2766
warnings: []

publication_registered: true
remote_unit_endpoint: /models/project-wiki-query-planning-flow
manifest_sha256: 7F46974634146B0652CC064C52209D039584A1C145BE95120A0DD8F3A92906E2

## Per Rejected Type

| rejected_type | heldout_count | accuracy | reward_margin | loss |
|---|---:|---:|---:|---:|
| missing_callers | 5 | 0.8 | 0.000228 | 0.693033 |
| missing_mapper_and_value_flow | 10 | 1.0 | 0.001238 | 0.692529 |
| no_tests_or_validation_surface | 5 | 1.0 | 0.004254 | 0.691023 |
| no_workflow_branch_lookup | 10 | 1.0 | 0.005017 | 0.690642 |
| single_symbol_lookup | 10 | 1.0 | 0.001493 | 0.692401 |
| stale_version_ignored | 5 | 1.0 | 0.001403 | 0.692446 |
| wrong_project_context | 5 | 1.0 | 0.002177 | 0.692059 |

## Phase History

| batch | role | heldout_accuracy | reward_margin | CPU % | CPU ms | peak RAM MB | elapsed ms | status | reason |
|---|---|---:|---:|---:|---:|---:|---:|---|---|
| 019 | best_pre_promotion_checkpoint | 0.8799999952316284 | 0.0018521499587222934 | 12.7015 | 43531.25 | 93.703 | 76251.85 | HELD | no_tests_or_validation_surface below gate |
| 021 | rejected_shape_probe | 0.5799999833106995 | 0.0005495165823958814 | 12.839611445611814 | 43171.875 | 93.890625 | 75108.5139 | REJECTED | global required_difference text damaged stable contrasts |
| 022 | accepted_shape_fix | 0.9800000190734863 | 0.0023597697727382183 | 12.708808342685629 | 38859.375 | 93.51171875 | 69023.6671 | HELD | quality gate passed; promotion confirmation required |
| 023 | mixed_confirmation | 0.9800000190734863 | 0.0023503322154283524 | 12.778709986174203 | 42406.25 | 93.69140625 | 74239.9731 | HELD | explicit promotion confirmation required |
| 024 | failed_promotion_confirmation | 0.9599999785423279 | 0.00234183669090271 | 12.747719538522091 | 44000.0 | 103.71875 | 76506.3071 | REJECTED | missing_callers fell to 0.6 |
| 025 | accepted_promotion_confirmation | 0.9800000190734863 | 0.002355672884732485 | 12.728392094850138 | 20796.875 | 105.9140625 | 60713.2766 | PROMOTED | manifest generated and registered |

## Accepted Learning

Correct pattern: developer change-impact and workflow questions must be converted into ordered project-local query plans before retrieval or answer composition.

Chosen shape: `plan_status: COMPLETE_PROJECT_LOCAL_QUERY_PLAN`; intent; entities; symbols; required guard; ordered query steps including project/version checks, mapper/value flow, callers, validators/tests, workflow branch, and missing evidence; answer policy.

Rejected shape: near-correct query plan with exactly one evidence-checkable omission and a specific `[WRONG: ...]` consequence.

Important rejected-shape fix: global `required_difference` text damaged stable contrasts. Keep the no-tests fix narrow: use `validation_surface_required` only for `no_tests_or_validation_surface`.

## Next Action

Continue with the next weakest unpromoted wiki unit. Do not claim whole wiki completion.
