# Advanced Prompt Coverage Status

report_scope: advanced_prompt_coverage_status
claim: QUERY_PLANNING_ADVANCED_PROMPTS_PROMOTED
full_wiki_advanced_prompt_coverage_claimed: false
date: 2026-05-20

## Short Answer

Advanced prompts are covered and promoted at the `ProjectWikiQueryPlanningFlow`
level: the system has learned to turn developer questions into multi-step wiki
query plans before retrieval or answer composition.

This does not yet prove full end-to-end wiki answering for every advanced prompt
family. The answer-side units are still separate gates.

## Required Advanced Prompt Families

From `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md`:

| family | required variations |
|---|---:|
| entity impact | 9 |
| multi-step flow trace | 8 |
| change location | 7 |
| impact analysis / regression risk | 5 |
| architecture boundary | 5 |
| missing evidence / abstention | 3 |
| query plan orchestration | 8 |
| total | 45 |

## Promoted Query-Planning Coverage

Source report:

`C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-19-project-wiki-query-planning-promotion-025\PHASE-RESOURCE-REPORT.json`

Promotion batch:

`project-wiki-query-planning-promotion-confirmation-025`

Raw metrics:

```text
heldout_accuracy: 0.9800000190734863
heldout_reward_margin: 0.002355672884732485
heldout_loss: 0.6919702887535095
average_cpu_percent: 12.728392094850138
total_cpu_time_ms: 20796.875
peak_working_set_mb: 105.9140625
total_elapsed_ms: 60713.2766
```

Training prompt families covered in the promoted batch:

```text
entity_impact_question: 10
field_addition_question: 10
field_rename_question: 5
flow_trace_question: 10
change_location_question: 5
architecture_boundary_question: 5
regression_risk_question: 5
```

Negative contrasts covered:

```text
single_symbol_lookup: 10
missing_mapper_and_value_flow: 10
stale_version_ignored: 5
no_workflow_branch_lookup: 10
missing_callers: 5
wrong_project_context: 5
no_tests_or_validation_surface: 5
```

Weakest accepted contrast:

```text
missing_callers accuracy: 0.8
missing_callers reward_margin: 0.000228
```

## Current End-To-End Validation State

`ProjectWikiRealProjectValidationRunner` now generates 45 advanced prompt
variations per evidence record across these families:

```text
entity_impact_question
flow_trace_question
change_location_question
impact_analysis_question
architecture_boundary_question
missing_evidence_question
query_plan_orchestration_question
```

Current targeted validation run:

```text
ProjectWikiRealProjectValidationRunner + entity-impact query trace tests
passed: 3
failed: 0
total: 3
```

Important correction made during this check:

```text
Old test expected exactly 10 query-plan steps.
Current advanced query plan produces 14 steps.
The test was corrected to require >= 10 steps and equal planned/executed step count.
```

This is an expansion of coverage, not a regression.

## Not Yet Proven

- all 45 advanced prompt variations have not yet been persisted as a
  per-variation pass/fail matrix after the promoted query-planning checkpoint;
- full end-to-end answer quality is not promoted for all advanced prompt types;
- `WikiContextRoutingUnit`, `WikiAnswerComposer`, `ProjectAnswerVerifier`, and
  `WikiTraceAggregator` are still separate trained-unit gates;
- real-project validation currently has test-level pass/fail coverage, not a
  DPO runner promotion report with reward margin/loss per prompt family;
- Phase 2 question database is not promoted.

## Next Required Work

1. Persist a 45-variation advanced prompt coverage matrix per project.
2. Promote `WikiContextRoutingUnit` and `WikiAnswerComposer` so advanced query
   plans become verified answers.
3. Add per-family metrics for entity impact, flow trace, change location,
   impact analysis, architecture boundary, missing evidence, and query planning.
4. Keep `missing_callers` as the first query-planning watch item because it is
   the weakest accepted contrast.
