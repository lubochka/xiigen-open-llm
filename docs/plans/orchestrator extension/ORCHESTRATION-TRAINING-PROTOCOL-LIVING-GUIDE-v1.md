> **STATUS: IMPLEMENTED — Phase 37 (2026-06)**
> The training protocol described here was implemented in Phase 37 of ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.
> For current gate commands, acceptance metrics, and the trained model checkpoint, see:
> `C:\Projects\llm_mvp_core\docs\plans\ORCHESTRATION-MACHINE-TRAINING-PLAN-v20.md`
> Current checkpoint: `C:\Projects\llm_mvp_core\models\orchestration\orchestration-v1\`

---

# ORCHESTRATION TRAINING PROTOCOL - LIVING DOCUMENT GUIDE v1

Status: REQUIRED REFERENCE.
Language: English.
Applies to: ORCHESTRATION-TRAINING-PROTOCOL-v1.md and all future versions.
Date: 2026-05-15

This guide defines two things:

1. Why and how ORCHESTRATION-TRAINING-PROTOCOL-v1.md must be expanded after
   every training cycle - not just at the start of new work.

2. How the proven solution library in that protocol becomes the foundation for
   parallel training in other programming languages.

---

## Why The Protocol Must Grow After Every Cycle

The protocol currently documents procedure: how to build rows, how to run gates,
how to ablate. That is necessary but not sufficient.

What it does not yet contain is the growing library of proven correct decisions -
the patterns that actually worked for each unit, the failure modes that were
discovered in real runs, and the canonical shapes that future executors can
reuse or translate.

Without this library, every new training cycle starts from procedure alone. The
executor must rediscover by experiment what the previous executor already learned.
That is waste.

With this library, every new training cycle starts from a tested foundation.
The executor reads what worked, recognizes patterns, and either reuses them
directly or adapts them for a new language or domain.

The rule is:

```
Every promoted run updates the protocol.
Every rejected run updates the protocol.
Every new failure mode updates the protocol.
Every new language adaptation starts from the protocol.
```

---

## Section 18: Unit Solution Library

This section must be updated after every promoted run. It records the
canonical correct solution shape for each trainable unit.

The structure for every unit entry:

```
Unit name
  Responsibility: one sentence.
  Proven correct pattern: the shape of chosen side that consistently produces
    high accuracy and margin. Not a specific row - a reusable template.
  Proven wrong patterns: the shapes of rejected side that produced meaningful
    contrast in real ablations.
  Failure modes found: row shapes that failed ablation and the reason.
  Current trained checkpoint: which baseline contains this unit.
  Rows in baseline: count.
  Latest report artifact: `report_artifact_path` absolute path to the saved
    runner or phase report that supports the current metrics.
  Languages with verified rows: list.
  Languages pending: list.
```

### Current Unit Library

The canonical wiki unit list and minimum metrics come from
`PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md`. Do not invent alternate names when the
run is about project-local wiki training. The runner, trace, and reports must
use these names exactly.

#### Unit 1: ProjectWikiRequestRouter

Responsibility: decide whether a prompt belongs to project-local wiki work and
which wiki unit should handle it.

Minimum metrics:

```text
wiki_request_routing_accuracy >= 0.92
wrong_project_rejection == 1.00
stale_version_rejection >= 0.98
```

Current promoted route checkpoint: `project-wiki-request-router-weak-route-replay-trainable-route-promote-044`.
This is a trainable request-routing model snapshot, not the earlier static
lexical route gate. Historical best DPO checkpoints by held-out accuracy:
`project-wiki-request-router-weak-route-replay-019`,
`project-wiki-request-router-weak-route-replay-target-boost-021`,
`project-wiki-request-router-weak-route-replay-gentle-024`,
`project-wiki-request-router-weak-train-balanced-heldout-gentle-030`,
`project-wiki-request-router-weak-doc-validation-trace-from021-gentle-034`,
`project-wiki-request-router-docgen-contrast-gentle-035`,
`project-wiki-request-router-near-miss-contrast-gentle-038`, and
`project-wiki-request-router-weak-route-replay-trainable-route-promote-044`.
The route target is met; the generative DPO watch metric remains below `0.92`.
Rows in each attempt: 50 train / 50 held-out. Languages with
verified rows: English minimum gate passed. Languages pending: see Section 20.

Latest measured raw metrics:

```text
structural_route_accuracy = 1.0
dpo_heldout_accuracy = 0.8799999952
dpo_reward_margin = 0.0091735236
dpo_loss = 0.6906180465
cpu_time_ms = 181843.75
peak_ram_mb = 103.875
elapsed_ms = 294899.544
source_run = project-wiki-request-router-weak-route-replay-trainable-route-promote-044
```

Status note: ProjectWikiRequestRouter route target is promoted, but this is
not a full wiki completion claim. The pipeline now separates actual trainable
route accuracy from generative DPO rejected-quality accuracy. DPO remains a
watch/remediation metric. Remaining weak contrasts:
`route_validation_to_answer`, `documentation_generator_to_answer_verifier`,
and `trace_aggregator_to_request_router`.

Rejected follow-up directions:

```text
validation-focus-006/007: structural route failures; do not continue.
validation-focus-008: structural 1.0 but dpo_heldout_accuracy 0.2199999988.
weak-target-015: structural 1.0 but dpo_heldout_accuracy 0.5799999833.
general-replay-016: structural 1.0 but dpo_heldout_accuracy 0.4000000060.
precision-patch-017: structural 1.0 but dpo_heldout_accuracy 0.7200000286.
weak-route-replay-018: structural 1.0 but dpo_heldout_accuracy 0.6000000238.
weak-route-replay-022: structural 1.0 but dpo_heldout_accuracy 0.6600000262.
validation-focus-025: structural 1.0 but dpo_heldout_accuracy 0.5600000024.
weak-route-replay-clean-rejected-027: structural 1.0 but dpo_heldout_accuracy 0.7799999714.
weak-route-replay-capacity-028: structural 1.0 but dpo_heldout_accuracy 0.4000000060.
diverse-sibling-gentle-029: structural 1.0 but dpo_heldout_accuracy 0.7799999714.
weak-doc-validation-trace-gentle-031: structural 1.0 but dpo_heldout_accuracy 0.8600000143.
weak-doc-validation-trace-target-boost-032: structural 1.0 but dpo_heldout_accuracy 0.7200000286.
weak-route-replay-contrast-gentle-033: structural 1.0 but dpo_heldout_accuracy 0.8199999928.
docgen-contrast-rationale-gentle-036: structural 1.0 but dpo_heldout_accuracy 0.7599999905.
docgen-contrast-037: structural 1.0 but dpo_heldout_accuracy 0.7599999905.
precision-patch-020, target-boost-023, and wrong-first-026 passed the minimum
gate but regressed accuracy below the current best; keep them as diagnostics,
not as target checkpoints.

Rejected data/shape ablations:

```text
route-marker chosen expansion: reduced held-out accuracy.
responsibility-line chosen expansion: reduced held-out accuracy.
wrong-marker-first rejected shape: reduced held-out accuracy.
validation-focus overconcentration: reduced held-out accuracy.
target-boost repeated epochs: improved margin but did not improve count accuracy.
gentle mode: preserved count accuracy with lower resource cost but did not reach target.
clean rejected consequence text: reduced held-out accuracy.
larger capacity model: reduced held-out accuracy and increased resource cost.
broad diverse sibling data: fixed structural variety but reduced DPO accuracy.
weak-doc-validation-trace overfocus: preserved margin but reduced count accuracy.
strong rejected contrast text: made rejected sides clearer but reduced DPO accuracy.
documentation-generator-only contrast: did not flip the two docgen held-out failures.
docgen chosen-rationale expansion: reduced held-out accuracy.
near-miss contrast: preserved best count and improved margin, but did not reach target.
trainable route model snapshot: promoted the actual route target; DPO remains
below 0.92 as a watch item, not a full-wiki blocker.
```

#### Unit 2: StackAndVersionClassifier

Responsibility: detect project stack, .NET framework/runtime, C# version, UI
technology, and storage technology.

Minimum metrics:

```text
stack_version_accuracy >= 0.95
unknown_version_false_claim_rate == 0
```

Current checkpoint:

```text
status: PROMOTED for StackAndVersionClassifier only
promotion batch: stack-version-classifier-batch-001
runtime_accuracy: 1.0
target_framework_accuracy: 1.0
language_version_accuracy: 1.0
project_style_accuracy: 1.0
wrong_annotation_rate: 1.0
average_confidence: 0.94
cpu_time_ms: 546.875
peak_ram_mb: 31.1953125
elapsed_ms: 419.8844
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-20-stack-version-classifier-batch-001\stack-version-classifier-batch-001-result.json
rows in promoted sequence: 50 train rows / 50 held-out rows
full wiki done: false
```

#### Unit 3: CodeTraceBuilder

Responsibility: build method/entity/call/value-flow trace from project code.

Minimum metrics:

```text
method_trace_recall >= 0.90
call_edge_recall >= 0.90
value_flow_edge_recall >= 0.85
```

Current checkpoint:

```text
status: PROMOTED_MILESTONE_WITH_RUNNER_MANIFEST for CodeTraceBuilder
structural method_trace_recall: 1.0
structural call_edge_recall: 1.0
structural value_flow_edge_recall: 1.0
promoted DPO batch: code-trace-builder-task-prompt-compact-wrong-first-high-contrast-realistic-multihop-promotion-confirmation-batch-019
dpo_heldout_accuracy: 0.9800000191
dpo_reward_margin: 0.0158916730
dpo_loss: 0.6852436662
dpo_final_accuracy: 0.8240000105
dpo_final_margin: 0.0130904702
cpu_time_ms: 124640.625
average_cpu_percent: 12.6507018936
peak_ram_mb: 106.90625
elapsed_ms: 163074.8545
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-code-trace-builder-remediation\code-trace-builder-batch-019-promotion-confirmation-result.json
publication: published through generated capability manifest
publication manifest: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-code-trace-builder-promotion-confirmation-batch-019\code-trace-builder-unit\code-trace-builder-20260521190421\generated_capability_manifest.json
full wiki done: false
```

Failure modes found:

```text
full trace output is too long/noisy for the DPO metric.
ultra-compact chosen improves DPO from 0.22 to 0.56.
varied invented-caller labels did not improve count accuracy beyond 0.56.
caller-status chosen expansion regressed the batch to 0.30.
wrong-caller chosen expansion regressed the batch to 0.54.
caller-abstract rejected shape improved balanced DPO to 0.80 and was the first
minimum-signal shape.
high-contrast realistic multihop rows fixed missing_value_flow and
incomplete_trace, then focused invented_callee remediation plus two mixed
confirmation runs stabilized all required rejected-type gates.
Residual watch item: invented_callee had 0.9 held-out accuracy in promotion
confirmation. This passes the per-type gate but must stay in real-project QA
before any excellent project-wiki claim.
```

#### Unit 4: EntityImpactMapper

Responsibility: answer developer change-impact questions such as adding a field
to an entity/DTO and tracing where that value travels.

Minimum metrics:

```text
entity_impact_path_accuracy >= 0.85
field_touch_point_recall >= 0.85
calculation_impact_recall >= 0.80
partial_map_labeled_rate >= 0.95
```

Proven correct pattern:

```text
A-side: developer asks where a new field/property/entity value travels.
Chosen: name all touch points: input contract, validation, mapper, entity,
repository/storage, services/calculations, API/UI output, tests, and risks.
Rejected: omit a real touch point or claim certainty without evidence.
```

Current checkpoint:

```text
status: PROMOTED_MILESTONE_WITH_RUNNER_MANIFEST for EntityImpactMapper
structural evidence_recall: 1.0
structural touchpoint_recall: 1.0
best accuracy batch: entity-impact-mapper-compact-continuation-004
best accuracy dpo_heldout_accuracy: 0.8600000143
best accuracy dpo_reward_margin: 0.0054805167
best strong-margin batch: entity-impact-mapper-compact-from-target-boost-continuation-010
strong-margin dpo_heldout_accuracy: 0.8000000119
strong-margin dpo_reward_margin: 0.0303401537
strong-margin dpo_loss: 0.6753010720
promotion confirmation batch: entity-impact-mapper-compact-calculation-signal-lowdose-confirmation-018
promotion confirmation dpo_heldout_accuracy: 0.8999999762
promotion confirmation dpo_reward_margin: 0.0388998166
promotion confirmation dpo_loss: 0.6740791798
promotion confirmation cpu_time_ms: 76093.75
promotion confirmation peak_ram_mb: 101.64453125
promotion confirmation elapsed_ms: 114349.7871
promotion confirmation policy_checkpoint: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-entity-impact-mapper-lowdose-calculation-signal-confirmation-018\dpo-runs\entity-impact-mapper-unit\entity-impact-mapper-compact-calculation-signal-lowdose-confirmation-018-entity-impact-mapper-unit-20260521200850\checkpoints\policy_final.bin
promotion confirmation reference_checkpoint: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-entity-impact-mapper-lowdose-calculation-signal-confirmation-018\dpo-runs\entity-impact-mapper-unit\entity-impact-mapper-compact-calculation-signal-lowdose-confirmation-018-entity-impact-mapper-unit-20260521200850\checkpoints\reference_base.bin
generated capability manifest: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-entity-impact-mapper-lowdose-calculation-signal-confirmation-018\entity-impact-mapper-unit\entity-impact-mapper-20260521201045\generated_capability_manifest.json
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-entity-impact-mapper-remediation\ENTITY-IMPACT-MAPPER-PHASE-METRICS-v1.md
full wiki done: false
```

Failure modes found:

```text
plain full chosen/rejected passes DPO accuracy but margin is too weak.
compact chosen/rejected improves accuracy to 0.86.
target-boost improves margin above 0.02 but reduces accuracy to 0.80 and costs
more CPU.
calculation-strong focused remediation regressed to 0.6000000238.
derived-status focused remediation collapsed to 0.0 and negative margin.
calculation-path prose-heavy remediation batch 011 collapsed to 0.3000000119.
calculation-signal applied to all rejected types batch 012 fixed
missing_calculation but reduced global held-out accuracy to 0.5400000215.
coverage-signal batch 015 fixed missing_calculation and wrong_project_evidence
but broke missing_persistence and partial_touchpoint_map.
low-dose calculation-signal hybrid batch 017 and confirmation batch 018 kept
global held-out accuracy at 0.8999999762 and repaired missing_calculation to
1.0 held-out accuracy while preserving all other types at >= 0.8.
Do not repeat the failed calculation-strong or derived-status shapes without a
new evidence design.
Residual watch items after batch 018: wrong_project_evidence and
missing_persistence are stable at 0.8 held-out accuracy, and
partial_touchpoint_map keeps one negative-margin row. These pass the current
milestone but remain real-project QA watch items before an excellent wiki claim.
```

#### Unit 5: DocumentationGenerator

Responsibility: generate bottom-up documentation for code symbols and propagate
callee meaning to callers.

Minimum metrics:

```text
documentation_quality_score >= 0.95
source_evidence_score == 1.00
bottom_up_propagation_score == 1.00
```

Current checkpoint:

```text
status: PROMOTED_MILESTONE_WITH_RUNNER_MANIFEST for DocumentationGenerator stored-procedure documentation
validated gap: stored procedure evidence extraction for documentation/evidence-index
real project validation source: C:\Projects\askapediaweb
validation mode: in-memory project documentation generation; project code is validation input only
phase artifact: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-150002\phase-05-askapedia-inmemory-stored-procedure-evidence-extraction.json
accuracy: 1.0 test pass rate for the stored-procedure evidence extraction gate
cpu_ms: 97015.625
peak_working_set_mb: 1144.281
elapsed_ms: 22550.858
promoted DPO batch: documentation-generator-stored-procedure-sql-evidence-promotion-confirmation-batch-011
dpo_heldout_accuracy: 0.9399999976
dpo_reward_margin: 0.0094902813
dpo_loss: 0.6884179711
dpo_final_accuracy: 0.8120000112
dpo_final_margin: 0.0091298546
cpu_time_ms: 169390.625
average_cpu_percent: 12.5339755085
peak_ram_mb: 94.22265625
elapsed_ms: 223345.5671
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-documentation-generator-remediation\documentation-generator-batch-011-stored-procedure-promotion-confirmation-result.json
publication: published through generated capability manifest
publication manifest: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-documentation-generator-stored-procedure-promotion-confirmation-batch-011\documentation-generator-unit\documentation-generator-20260521192503\generated_capability_manifest.json
done claim: false; this promotes the stored-procedure documentation sub-capability, not the full project wiki.
```

Proven correct patterns:

- for C# methods that call `SqlQuery<T>("[schema].Procedure @Param", param)`, documentation evidence must record `stored_procedure_name`, SQL parameters with .NET source binding, tables read, tables written, result columns, transaction behavior, and side effects;
- for legacy SQL files with `CREATE PROCEDURE`, result-shape extraction must collect aliased columns across nested SELECT/UNION forms instead of trusting only the last simple SELECT;
- schema-qualified SQL identifiers must be preserved (`dbo.Orders`, not just `Orders`) so project-local wiki answers can distinguish database boundaries.

Failure modes found:

- previous evidence-index rows captured .NET caller chain and `SqlParameter` snippets only as text; this was insufficient for stored-procedure impact questions that need structured SQL parameters/result shape/side effects;
- the first result-shape extractor missed the `TypeS` alias in askapedia's nested SELECT/UNION procedure; the extractor now aggregates aliased columns across SELECT candidates;
- generic "bad documentation" negatives are insufficient; accepted rejected examples omit one trace-checkable structured SQL evidence field: parameters, result shape, side effects, .NET callers, or exact stored procedure identity;
- residual watch item: missing_sql_parameters and missing_sql_side_effects pass the per-type gate but include one negative per-row margin each in promotion confirmation. Keep both in real-project QA before any excellent project-wiki claim.

#### Unit 6: NegativeDpoGenerator

Responsibility: create plausible but provably wrong rejected examples from
accepted documentation and trace evidence.

Minimum metrics:

```text
chosen_not_equal_rejected == 1.00
wrong_annotation_rate == 1.00
provably_wrong_against_trace == 1.00
valid_negative_diversity >= 0.90
```

Proven pattern:

```text
Use near-correct rejected documentation with exactly one trace-checkable defect.
For parameter defects, do not repeat one `RequestMapper` shape. Teach the
defect across multiple parameterized roles and mark the chosen side with
`parameter_verification_status: actual_input_values_present_in_code_trace`.
```

Failure modes found:

```text
2026-05-19: isolated focused repetition of `missing_parameters` did not improve
the unit. Batch048 stayed at missing_parameters accuracy 0.70. The fix was a
mixed parameter-variety batch, not more identical focus rows.
```

Current checkpoint:

```text
status: PROMOTED for NegativeDpoGenerator only
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-19-negative-dpo-remediation-v2\PHASE-RESOURCE-REPORT.json
promotion batch: negative-dpo-generator-promotion-confirmation-051
held-out accuracy: 0.980000019
held-out reward margin: 0.003840186
rows in promoted sequence: 250 train rows / 250 held-out rows across batches 047-051
```

#### Unit 7: ProjectWikiQueryPlanningFlow

Responsibility: turn a developer question into a multi-step evidence
investigation plan before retrieval or answer composition.

Minimum metrics:

```text
query_plan_accuracy >= 0.90
required_touchpoint_recall >= 0.95
multi_hop_sequence_accuracy >= 0.90
missing_evidence_detection >= 0.95
irrelevant_query_rate <= 0.05
answer_readiness_score >= 0.90
```

Proven correct pattern:

```text
A-side: developer asks a change-impact or workflow question.
Chosen: classify intent, identify entities/symbols, produce ordered query
steps, define evidence sufficiency, and name when to abstain.
Rejected: perform one direct lookup or answer before required evidence is
planned and retrieved.
```

Promotion update 2026-05-19:

```text
status: PROMOTED for ProjectWikiQueryPlanningFlow only
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-19-project-wiki-query-planning-promotion-025\PHASE-RESOURCE-REPORT.json
promotion batch: project-wiki-query-planning-promotion-confirmation-025
held-out accuracy: 0.9800000190734863
held-out reward margin: 0.002355672884732485
held-out loss: 0.6919702887535095
CPU average percent: 12.728392094850138
CPU time ms: 20796.875
peak RAM MB: 105.9140625
elapsed ms: 60713.2766
rows in promoted sequence: 50 train rows / 50 held-out rows in the promotion batch
manifest: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-19-project-wiki-query-planning-sequence-002-dpo\project-wiki-query-planning-flow\project-wiki-query-planning-20260519175400\generated_capability_manifest.json
```

Accepted pattern:

```text
A-side: developer asks an entity-impact, field-change, workflow-trace,
architecture-boundary, regression-risk, or stored-procedure-impact question.
Chosen: produce a COMPLETE_PROJECT_LOCAL_QUERY_PLAN with project/version
checks, DTO/entity lookup, validator lookup, mapper/value-flow lookup,
caller lookup, persistence/output lookup, workflow branch lookup, test lookup,
missing-evidence check, and answer policy. For stored procedures, the chosen
plan must include stored procedure lookup, SQL parameters, tables read/written,
result shape, transaction behavior, side effects, .NET callers, value flow,
tests, missing-evidence check, and answer policy.
Rejected: near-correct plan with exactly one evidence-checkable omission and
a specific [WRONG: ...] consequence.
```

Stored procedure extension update 2026-05-20:

```text
status: PROMOTED for ProjectWikiQueryPlanningFlow stored_procedure_impact only
scope: ProjectWikiQueryPlanningFlow stored_procedure_impact
validation-only projects: DNN Platform, old nopCommerce, SmartStoreNET
training-source rule: validation-only unless source authority separately approves training use
required rejected types: procedure_name_only, missing_sql_parameters,
missing_result_shape, missing_table_side_effects, missing_dotnet_callers
promotion batch: project-wiki-query-planning-stored-procedure-promotion-confirmation-011
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-20-stored-procedure-wiki-extension\PHASE-REPORT.md
held-out accuracy: 1.0
held-out reward margin: 0.0024235269520431757
held-out loss: 0.6919364333152771
CPU average percent: 10.713235530413305
CPU time ms: 202125
peak RAM MB: 106.1796875
elapsed ms: 312987.114
warnings: none
publication manifest: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-20-project-wiki-stored-procedure-promotion-confirmation-011-dpo\project-wiki-query-planning-flow\project-wiki-query-planning-20260519232150\generated_capability_manifest.json
full wiki done: false
```

Stored procedure continuation update 2026-05-21:

```text
status: TRAINING_IN_PROGRESS for stronger stored_procedure_impact margin
best current checkpoint: project-wiki-query-planning-stored-procedure-result-parameter-replay-019
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-query-planning-stored-procedure-continuation\QUERY-PLANNING-STORED-PROCEDURE-CONTINUATION-SUMMARY.json
held-out accuracy: 0.9800000190734863
held-out reward margin: 0.015159263275563717
held-out loss: 0.6856059432029724
CPU average percent: 12.577251855273257
CPU time ms: 132265.625
peak RAM MB: 88.33984375
elapsed ms: 177306.5547
kept code change: missing_result_shape rejected side explicitly marks result_contract omitted, selected columns unknown, return codes unknown, result sets unknown, and .NET mapper output unknown.
known weak type: missing_sql_parameters remains unstable; marker-heavy rejected and chosen-marker variants worsened the result and must not be reused.
full wiki done: false
```

Query-series update 2026-05-21:

```text
status: PROMOTED_MILESTONE_WITH_RUNNER_MANIFEST for ProjectWikiQueryPlanningFlow query-series planning
scope: developer question -> ordered wiki query series -> evidence analysis before answer composition
promotion batch: project-wiki-query-planning-query-series-promotion-confirmation-028
held-out accuracy: 0.9200000166893005
held-out reward margin: 0.007554488256573677
held-out loss: 0.6893810033798218
final accuracy: 0.9200000047683716
final reward margin: 0.00773959802754689
query plan accuracy: 1.0
multi-hop sequence accuracy: 1.0
CPU average percent: 12.71370139505782
CPU time ms: 115531.25
peak RAM MB: 93.859375
elapsed ms: 160753.3263
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-query-planning-query-series-remediation\QUERY-PLANNING-QUERY-SERIES-PHASE-REPORT.json
publication manifest: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-project-wiki-query-planning-query-series-promotion-confirmation-028-dpo\project-wiki-query-planning-flow\project-wiki-query-planning-20260521205436\generated_capability_manifest.json
full wiki done: false
```

Accepted query-series pattern:

```text
A-side: developer asks what the local wiki should search before answering a
change-impact, stored-procedure-impact, workflow, or regression-risk question.
Chosen: complete project-local query plan with current project/version checks,
ordered evidence searches, evidence grouping, sufficiency checks, explicit
missing-evidence handling, and answer composition only after proof.
Rejected: near-correct plan that uses the same query steps but allows answer
composition from the first plausible evidence group.
```

Real-project validation update 2026-05-22:

```text
status: REAL_PROJECT_VALIDATION_GATE_PASSED for ProjectWikiQueryPlanningFlow validation surface
scope: five legacy .NET projects with full wiki evidence validation and stored-procedure impact questions where SQL procedure evidence exists
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-real-project-wiki-validation\REAL-PROJECT-WIKI-VALIDATION-PHASE-REPORT.md
source_summary: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-real-project-wiki-validation\REAL-PROJECT-WIKI-VALIDATION-AFTER-QUERY-INTENT-FIX-SUMMARY.json
project_count: 5
evidence_record_count: 616
total_question_count: 6393
exact_evidence_accuracy: 1.0
query_planning_invocation_rate: 1.0
entity_impact_trace_pass_rate: 1.0
stored_procedure_questions: 13
stored_procedure_project: C:\Projects\askapediaweb
CPU time ms: 23750
peak RAM MB: 16.28997802734375
elapsed ms: 22874.23179999999
full wiki done: false
```

Validated pattern:

```text
For ambiguous symbols, the developer-facing question and the planned evidence
query must include enough locator context to distinguish same-name methods:
source path plus source line is accepted. Method name alone is not sufficient
when duplicate symbols exist in the same project.

For stored procedures, intent classification must require explicit SQL or
procedure evidence terms. Generic "execute" wording is not accepted as a stored
procedure signal because ordinary code methods can also be named Execute.
```

Failure modes found:

```text
Do not use the generic token "execute" as a stored-procedure intent signal. It
misroutes ordinary code questions about methods named Execute into
stored_procedure_impact. Require explicit terms such as stored procedure,
procedure, SQL parameter, dbo, schema-qualified procedure name, tables
read/written, result set, or .NET stored-procedure caller evidence.

Rejected probe: project-wiki-query-planning-difference-contract-lowlr-021.
Observed: held-out accuracy 0.5799999833106995.
Cause: global required_difference text damaged already-stable rejected types.
Fix: remove global required_difference; keep the no-tests contrast narrow with
validation_surface_required on chosen and validation_surface_status: OMITTED on
the no_tests_or_validation_surface rejected side.

Rejected probe: project-wiki-query-planning-stored-procedure-result-parameter-replay-020.
Observed: held-out accuracy 0.6000000238418579; missing_sql_parameters accuracy 0.16666666666666666.
Cause: marker-heavy missing_sql_parameters rejected side made the negative side too token-dominant and confused the preference signal.
Fix: do not use BAD_SQL_PARAMETER_CONTRACT_GUESSED / INPUT_VALUES_UNKNOWN / OUTPUT_VALUES_UNKNOWN marker-heavy rejected shape.

Rejected probe: project-wiki-query-planning-stored-procedure-sql-parameter-replay-022.
Observed: held-out accuracy 0.8199999928474426; missing_sql_parameters accuracy 0.82.
Cause: positive-side SQL parameter markers alone did not stabilize the weak type.
Fix: source more diverse SQL-parameter rows with real argument construction variations instead of adding more generic marker tokens.

Rejected probe: project-wiki-query-planning-answer-analysis-replay-024.
Observed: held-out accuracy 0.0; answer_without_intermediate_analysis accuracy 0.0.
Cause: bad rejected shape was too coarse and short; it was not a good rejected
contrast and collapsed the preference direction.
Fix: rejected must be near-correct and keep the ordered query steps while
omitting only the evidence analysis/sufficiency step.

Rejected probe: project-wiki-query-planning-answer-analysis-replay-025.
Observed: held-out accuracy 0.3799999952; answer_without_intermediate_analysis accuracy 0.38.
Cause: focused single-type replay over-concentrated on the weak contrast.
Fix: use mixed query-series replay from the clean stored-procedure checkpoint.

Rejected probe: project-wiki-query-planning-query-series-promotion-confirmation-027.
Observed: DPO held-out accuracy 0.9399999976 but deterministic gate failed with
multi_hop_sequence_accuracy 0.8.
Cause: "avoid stale" wording triggered StaleDocumentationRejection and produced
shallow three-step plans for entity-impact rows.
Fix: phrase the precheck requirement as current-project and documentation-version
proof while preserving entity-impact intent.
```

#### Unit 8: WikiContextRoutingUnit

Responsibility: choose the correct evidence retrieval path and context scope.

Minimum metrics:

```text
context_route_accuracy >= 0.90
wrong_project_context_rate == 0
entity_impact_route_accuracy >= 0.85
```

Proven correct pattern:

```text
A-side: a query plan already produced by ProjectWikiQueryPlanningFlow plus the
developer's project-local wiki question.
Chosen: select Mode3LexicalEvidence for direct symbol/location/parameter/return
questions, Mode2SemanticHybrid for multi-hop entity/workflow/stored-procedure
questions, MissingEvidenceAbstention for missing evidence, WrongProjectRejection
for non-local project evidence, and StaleDocumentationRejection for stale docs.
Rejected: same question with one wrong context route or a route that answers
before retrieval/evidence sufficiency.
```

Proven rejected patterns:

```text
always_bm25:
  route_kind: Mode3LexicalEvidence for a multi-hop developer reasoning question.

always_semantic:
  route_kind: Mode2SemanticHybrid for a direct symbol/location question.

answer_instead_of_abstain:
  answer_before_retrieval: true when missing evidence should abstain.

ignore_wrong_project:
  use a similar symbol from another repository.

ignore_stale_version:
  use same-name old documentation without freshness proof.
```

Failure modes found:

```text
precheck label-only rows did not repair no_pre_checks. Batches 008-012 stayed
at or below 0.6 and batch 012 collapsed to 0.0.

precheck diverse/binary rows improved no_pre_checks to 0.7 in batch 018 but did
not reach the 0.75 minimum and regressed in batch 019.

near-correct query-plan-proof rejected rows in batch 020 stayed at 0.52. This
confirms that no_pre_checks is primarily a cross-unit trace requirement owned
by ProjectWikiQueryPlanningFlow and WikiTraceAggregator. WikiContextRoutingUnit
should consume a verified query plan instead of being promoted by repeating
label-only no-precheck rows.
```

Current checkpoint:

```text
status: PROMOTED_TARGET_SIGNAL_WITH_KNOWN_CROSS_UNIT_PRECHECK_DEBT
batch: wiki-context-routing-compact-boundary-strong-direct-strong-abstain-strong-balanced-combo-014
held_out_accuracy: 0.9200000167
held_out_reward_margin: 0.0257314630
final_accuracy: 0.8120000064
final_reward_margin: 0.0226762490
cpu_time_ms: 145046.875
peak_working_set_mb: 92.86328125
elapsed_ms: 187606.3201
checkpoint: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-wiki-context-routing-compact-boundary-direct-abstain-balanced-combo-014\dpo-runs\wiki-context-routing-unit\wiki-context-routing-compact-boundary-strong-direct-strong-abstain-strong-balanced-combo-014-wiki-context-routing-unit-20260521073138\checkpoints\policy_final.bin
latest_report_artifact: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-wiki-context-routing-remediation\WIKI-CONTEXT-ROUTING-PHASE-METRICS-v1.md
full wiki done: false
```

#### Unit 9: WikiAnswerComposer

Responsibility: compose project-local answers with exact citations, uncertainty,
and missing-evidence handling.

Minimum metrics:

```text
answer_completeness >= 0.88
exact_evidence_accuracy >= 0.98
hallucination_rate == 0
partial_trace_disclosure >= 0.95
```

Proven correct pattern:

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
VALUE_FLOW_PRESENT / IMPACT_PATH_PRESENT
ANSWER_PERMISSION_ALLOWED
```

Why it works: the chosen side carries every wiki answer obligation as explicit
tokens and keeps the accepted evidence id in a token-safe form. The grounding
gate accepts the token-safe evidence id as equivalent to the raw evidence id, so
the training row can stay compact without losing evidence validation.

Stored-procedure answer-composition extension, promoted 2026-05-22:

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

Why it works: stored procedure questions are developer-impact questions, not
database-name lookup questions. The answer-composition row must force the unit
to include procedure identity, SQL input parameters, result shape, table effects,
.NET callers, caller value flow, and the missing-evidence check before answering.

Proven wrong patterns:

```text
BAD_PROJECT_WIKI_ANSWER ... EVIDENCE_ID_MISSING ...
[WRONG: missing_evidence_id omits the accepted evidence id, so the answer cannot
be verified against the project-local wiki.]

BAD_PROJECT_WIKI_ANSWER ... CALLERS_MISSING CALLEES_MISSING ...
[WRONG: missing_callers_and_callees hides who invokes the symbol and what it
invokes, so developer impact analysis is incomplete.]

BAD_PROJECT_WIKI_ANSWER ... PARAMETERS_MISSING RETURNS_MISSING ...
[WRONG: missing_parameters_and_returns prevents the developer from knowing what
values enter and leave this code path.]

BAD_PROJECT_WIKI_ANSWER ... SUMMARY_ONLY LOCATION_MISSING VALUE_FLOW_MISSING ...
[WRONG: single_line_summary_only lacks location, parameters, returns, callers,
callees, and impact path, so it is not actionable project documentation.]

BAD_PROJECT_WIKI_ANSWER ... FABRICATED_BEHAVIOR ...
[WRONG: hallucinated_behavior invents behavior not present in the accepted
evidence record, corrupting the local wiki.]

BAD_PROJECT_WIKI_ANSWER ... CROSS_PROJECT_EVIDENCE ...
[WRONG: wrong_project_evidence violates the project-local boundary and can route
a developer to the wrong codebase.]

BAD_PROJECT_WIKI_ANSWER ... ONLY_NAMES_PROCEDURE OMITS_SQL_PARAMETERS OMITS_RESULT_SHAPE OMITS_TABLE_EFFECTS OMITS_DOTNET_CALLERS ...
[WRONG: procedure_name_only gives only the procedure identity and omits SQL
parameters, result shape, table side effects, and .NET callers, so the developer
cannot reason about impact.]

BAD_PROJECT_WIKI_ANSWER ... SQL_PARAMETERS_MISSING INPUT_CONTRACT_MISSING CALLER_VALUES_UNKNOWN ...
[WRONG: missing_sql_parameters omits the input contract, so the developer cannot
know what values enter the stored procedure.]

BAD_PROJECT_WIKI_ANSWER ... RESULT_SHAPE_MISSING OUTPUT_CONTRACT_UNKNOWN ...
[WRONG: missing_result_shape omits returned columns or result object shape, so
callers cannot be safely updated.]

BAD_PROJECT_WIKI_ANSWER ... TABLE_EFFECTS_MISSING PERSISTENCE_IMPACT_UNKNOWN ...
[WRONG: missing_table_side_effects omits read/write tables and persistence
effects, so the developer cannot assess data impact.]

BAD_PROJECT_WIKI_ANSWER ... DOTNET_CALLERS_MISSING VALUE_FLOW_INCOMPLETE ...
[WRONG: missing_dotnet_callers omits the C# call sites, so the wiki cannot
explain which application workflows depend on the procedure.]
```

Failure modes found:

```text
Rejected batch: project-wiki-answer-composer-signal-grounded-batch-004.
Observed: held-out accuracy 0.7400000095, held-out margin 0.0021553575.
Cause: adding raw evidence id text to the chosen side weakened DPO separation.
Fix: keep token-safe evidence id in the answer and update grounding validation to
accept token-safe ids.

Rejected batch: project-wiki-answer-composer-signal-grounded-weak-batch-006.
Observed: held-out accuracy 0.5199999809, held-out margin 0.0012082556.
Cause: weak-type focused rows over-concentrated on the already damaged raw-id
checkpoint and reduced general answer discrimination.
Fix: do not continue from the weak checkpoint; return to signal-token rows.

Rejected stored-procedure batch: project-wiki-answer-composer-signal-token-stored-procedure-answer-batch-011.
Observed: held-out accuracy 0.6600000262, held-out margin 0.0039983694.
Cause: full-text stored-procedure answer rows were too verbose and did not
separate the required answer obligations strongly enough.
Fix: use compact signal-token chosen rows and specific trace-checkable stored
procedure rejected types.

Rejected stored-procedure batch: project-wiki-answer-composer-signal-token-stored-procedure-only-batch-014.
Observed: deterministic answer-shape accuracy 0.68.
Cause: the local answer unit did not trigger stored-procedure impact language
for variants such as result shape, returned columns, input parameter contract,
read/write tables, and caller chain.
Fix: broaden the impact trigger vocabulary before training from this shape.

Rejected stored-procedure batch: project-wiki-answer-composer-signal-token-stored-procedure-only-batch-018.
Observed: held-out accuracy 0.7599999905, held-out margin 0.0104764821.
Cause: an over-hardened `missing_dotnet_callers` rejected shape collapsed that
type to 0.2 accuracy.
Fix: return to the clearer batch-017 caller omission wording and continue from
the stronger stored-procedure checkpoint.

Rejected stored-procedure residual probe:
project-wiki-answer-composer-signal-token-stored-procedure-residual-batch-025.
Observed: held-out accuracy 0.8799999952, held-out margin 0.0299607534,
final accuracy 0.9200000048, final margin 0.0326499454, publication skipped by
`answer_composer_gate_failed_publication_skipped`.
Useful evidence: `missing_dotnet_callers` improved to 0.96 held-out accuracy.
Remaining failure: `procedure_name_only` stayed at 0.8 held-out accuracy and
min margin worsened to -0.0188545212.
Fix: do not continue from batch-025. Continue from accepted batch-024 with a
low-dose `procedure_name_only` remediation mixed with stable stored-procedure
contrasts.

Rejected stored-procedure low-dose probe:
project-wiki-answer-composer-signal-token-stored-procedure-procedure-name-lowdose-batch-026.
Observed: held-out accuracy 0.8799999952, held-out margin 0.0221901704,
final accuracy 0.9600000024, final margin 0.0215510463. The runner wrote a
manifest, but this is not accepted as a continuation baseline because it
regressed against accepted batch-024 and worsened residual types.
Type metrics: `procedure_name_only` 0.7333333333 accuracy, min margin
-0.0161139518; `missing_dotnet_callers` 0.6 accuracy, min margin
-0.02278414.
Cause: the curriculum still applied stored-procedure answer obligations to
generic C# method evidence instead of structured SQL procedure evidence.
Fix: do not continue from batch-026. Continue from accepted batch-024 with a
realistic stored-procedure evidence curriculum containing procedure name, SQL
parameters, result columns, read/write tables, side effects, and C# caller
chain.
```

Current checkpoint:

```text
status: PROMOTED_TARGET_SIGNAL_FOR_UNIT_WITH_STORED_PROCEDURE_ANSWER_COMPOSITION
best general batch: project-wiki-answer-composer-signal-token-cont-batch-010
general dpo_heldout_accuracy: 1.0000000000
general dpo_reward_margin: 0.026819217950105667
stored_procedure accepted batch: project-wiki-answer-composer-signal-token-stored-procedure-only-batch-024
stored_procedure previous confirmation batch: project-wiki-answer-composer-signal-token-stored-procedure-only-batch-023
batch_023_heldout_accuracy: 0.9200000167
batch_023_heldout_reward_margin: 0.0210307781
batch_023_final_accuracy: 0.9960000002
batch_023_final_reward_margin: 0.0198698722
batch_023_cpu_time_ms: 148734.375
batch_023_average_cpu_percent: 12.6395197214
batch_023_peak_ram_mb: 101.21484375
batch_023_elapsed_ms: 192833.3391
batch_024_heldout_accuracy: 0.9200000167
batch_024_heldout_reward_margin: 0.0233659334
batch_024_heldout_loss: 0.6815686226
batch_024_final_accuracy: 1.0
batch_024_final_reward_margin: 0.0225347648
batch_024_final_loss: 0.6819608796
batch_024_cpu_time_ms: 128781.25
batch_024_average_cpu_percent: 12.6434403001
batch_024_peak_ram_mb: 99.90234375
batch_024_elapsed_ms: 166249.469
batch_024_cpu_ms_per_train_row: 2575.625
batch_024_elapsed_ms_per_train_row: 3324.98938
stored_procedure report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-STORED-PROCEDURE-PHASE-METRICS-v1.md
stored_procedure checkpoint: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-only-batch-024\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-stored-procedure-only-batch-024-project-wiki-answer-composer-unit-20260521231627\checkpoints\policy_final.bin
stored_procedure generated capability manifest: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-stored-procedure-only-batch-024\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260521231914\generated_capability_manifest.json
stored_procedure manifest_sha256: 04D4A3CBEF86247F1A21A871F73AB99E87A3DC356B904C5421104B3A0C871D4F
residual watch: procedure_name_only held-out accuracy 0.8 and min_margin -0.0085631385
residual watch: missing_dotnet_callers held-out accuracy 0.8 and min_margin -0.0244055763
rejected residual probe: project-wiki-answer-composer-signal-token-stored-procedure-residual-batch-025
batch_025_heldout_accuracy: 0.8799999952
batch_025_heldout_reward_margin: 0.0299607534
batch_025_final_accuracy: 0.9200000048
batch_025_final_reward_margin: 0.0326499454
batch_025_cpu_time_ms: 153421.875
batch_025_average_cpu_percent: 12.6422829798
batch_025_peak_ram_mb: 88.1484375
batch_025_elapsed_ms: 199985.1323
batch_025_publication: skipped
batch_025_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-STORED-PROCEDURE-RESIDUAL-BATCH-025-REJECTED.md
rejected low-dose probe: project-wiki-answer-composer-signal-token-stored-procedure-procedure-name-lowdose-batch-026
batch_026_heldout_accuracy: 0.8799999952
batch_026_heldout_reward_margin: 0.0221901704
batch_026_final_accuracy: 0.9600000024
batch_026_final_reward_margin: 0.0215510463
batch_026_cpu_time_ms: 154671.875
batch_026_average_cpu_percent: 12.6699291530
batch_026_peak_ram_mb: 92.1796875
batch_026_elapsed_ms: 200066.8494
batch_026_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-STORED-PROCEDURE-PROCEDURE-NAME-LOWDOSE-BATCH-026-REJECTED.md
next remediation: realistic stored-procedure evidence batch from accepted batch-024
realistic stored_procedure candidate batch: project-wiki-answer-composer-signal-token-realistic-stored-procedure-batch-027
batch_027_heldout_accuracy: 0.9599999785
batch_027_heldout_reward_margin: 0.0307276398
batch_027_final_accuracy: 0.8920000064
batch_027_final_reward_margin: 0.0248108821
batch_027_cpu_time_ms: 155875
batch_027_average_cpu_percent: 12.6214290112
batch_027_peak_ram_mb: 85.68359375
batch_027_elapsed_ms: 201247.0684
realistic stored_procedure confirmation batch: project-wiki-answer-composer-signal-token-realistic-stored-procedure-confirmation-batch-028
batch_028_heldout_accuracy: 0.9800000191
batch_028_heldout_reward_margin: 0.0335775688
batch_028_heldout_loss: 0.6765397787
batch_028_final_accuracy: 0.9080000055
batch_028_final_reward_margin: 0.0276577430
batch_028_final_loss: 0.6794653428
batch_028_cpu_time_ms: 153546.875
batch_028_average_cpu_percent: 12.6322662444
batch_028_peak_ram_mb: 92.96875
batch_028_elapsed_ms: 199628.6718
batch_028_cpu_ms_per_train_row: 3070.9375
batch_028_elapsed_ms_per_train_row: 3992.573436
batch_028_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-REALISTIC-STORED-PROCEDURE-BATCH-028-CONFIRMED.md
batch_028_checkpoint: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-confirmation-batch-028\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-confirmation-batch-028-project-wiki-answer-composer-unit-20260521235634\checkpoints\policy_final.bin
batch_028_generated capability manifest: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-confirmation-batch-028\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260521235954\generated_capability_manifest.json
batch_028_manifest_sha256: 53A17E074434F8246B07B936CABDA0390BF542BD3CA1BA4BDDF6A3EF0A312CB4
residual watch after batch_028: procedure_name_only held-out accuracy 0.9 and min_margin -0.0184249878
next remediation: continue from batch-028 checkpoint with realistic stored-procedure procedure_name_only low-dose replay. Do not claim full wiki done.
rejected realistic procedure-name low-dose probe: project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-lowdose-batch-029
batch_029_heldout_accuracy: 0.9399999976
batch_029_heldout_reward_margin: 0.0330770612
batch_029_final_accuracy: 0.9240000045
batch_029_final_reward_margin: 0.0290057071
batch_029_cpu_time_ms: 152468.75
batch_029_average_cpu_percent: 12.6220014544
batch_029_peak_ram_mb: 104.6171875
batch_029_elapsed_ms: 197400.0771
batch_029_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-REALISTIC-STORED-PROCEDURE-PROCEDURE-NAME-LOWDOSE-BATCH-029-REJECTED.md
batch_029_rejection: overall held-out accuracy regressed against batch-028; procedure_name_only fell to 0.8666666667 and missing_table_side_effects fell to 0.9.
next remediation: do not continue from batch-029. Continue from batch-028 checkpoint and strengthen the complete SQL impact answer contrast instead of increasing procedure_name_only ratio.
rejected complete-impact contrast probe: project-wiki-answer-composer-signal-token-realistic-stored-procedure-complete-impact-contrast-batch-030
batch_030_heldout_accuracy: 0.8999999762
batch_030_heldout_reward_margin: 0.0430410430
batch_030_final_accuracy: 0.9800000012
batch_030_final_reward_margin: 0.0439559782
batch_030_cpu_time_ms: 153015.625
batch_030_average_cpu_percent: 12.6736033520
batch_030_peak_ram_mb: 101.7734375
batch_030_elapsed_ms: 199540.691
batch_030_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-REALISTIC-STORED-PROCEDURE-COMPLETE-IMPACT-CONTRAST-BATCH-030-REJECTED.md
batch_030_rejection: procedure_name_only improved to 1.0 but overall held-out accuracy regressed; missing_dotnet_callers fell to 0.6 and missing_result_shape fell to 0.9.
next remediation: do not continue from batch-030. Continue from batch-028 checkpoint and keep complete-impact contrast specific to procedure_name_only.
rejected narrow procedure-name contrast probe: project-wiki-answer-composer-signal-token-realistic-stored-procedure-narrow-procedure-name-contrast-batch-031
batch_031_heldout_accuracy: 0.9800000191
batch_031_heldout_reward_margin: 0.0265858527
batch_031_final_accuracy: 0.9800000012
batch_031_final_reward_margin: 0.0291879413
batch_031_cpu_time_ms: 155859.375
batch_031_average_cpu_percent: 12.6324956207
batch_031_peak_ram_mb: 88.42578125
batch_031_elapsed_ms: 201090.0563
batch_031_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-REALISTIC-STORED-PROCEDURE-NARROW-PROCEDURE-NAME-CONTRAST-BATCH-031-REJECTED.md
batch_031_rejection: procedure_name_only improved to 1.0 but held-out margin fell below batch-028 and missing_dotnet_callers regressed to 0.9.
next remediation: do not continue from batch-031. Continue from batch-028 checkpoint, remove broad chosen-side COMPLETE_SQL_IMPACT_ANSWER tokens, and keep procedure_name_only rejected contrast specific.
candidate rejected-only procedure contrast batch: project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-contrast-batch-032
batch_032_heldout_accuracy: 0.9800000191
batch_032_heldout_reward_margin: 0.0383329280
batch_032_final_accuracy: 0.9280000043
batch_032_final_reward_margin: 0.0329476518
batch_032_cpu_time_ms: 153093.75
batch_032_average_cpu_percent: 12.6658704137
batch_032_peak_ram_mb: 99.29296875
batch_032_elapsed_ms: 199976.3005
batch_032_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-REALISTIC-STORED-PROCEDURE-REJECTED-ONLY-CONTRAST-BATCH-032-CANDIDATE.md
batch_032_candidate_reason: held-out accuracy tied batch-028 and held-out reward margin improved by 0.0047553591; confirmation required.
confirmed rejected-only procedure contrast batch: project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-confirmation-batch-033
batch_033_heldout_accuracy: 0.9800000191
batch_033_heldout_reward_margin: 0.0424328744
batch_033_heldout_loss: 0.6722105145
batch_033_final_accuracy: 0.9200000048
batch_033_final_reward_margin: 0.0368107630
batch_033_final_loss: 0.6749705446
batch_033_cpu_time_ms: 156203.125
batch_033_average_cpu_percent: 12.6273263091
batch_033_peak_ram_mb: 97.84375
batch_033_elapsed_ms: 203259.7024
batch_033_cpu_ms_per_train_row: 3124.0625
batch_033_elapsed_ms_per_train_row: 4065.194048
batch_033_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-REALISTIC-STORED-PROCEDURE-REJECTED-ONLY-CONFIRMATION-BATCH-033-CONFIRMED.md
batch_033_checkpoint: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-confirmation-batch-033\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-confirmation-batch-033-project-wiki-answer-composer-unit-20260522003408\checkpoints\policy_final.bin
batch_033_generated capability manifest: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-rejected-only-confirmation-batch-033\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260522003731\generated_capability_manifest.json
batch_033_manifest_sha256: 26D9A335D13727B51A47C7DF7B722F3A9B5C1D9226ABAF78D25088403E40C7BB
batch_033_confirmation_reason: held-out accuracy tied batch-028 and batch-032, held-out reward margin improved by 0.0088553056 over batch-028 and 0.0040999465 over batch-032.
residual watch after batch_033: procedure_name_only held-out accuracy 0.9 and min_margin -0.0209523141.
rejected procedure contract-absent probe: project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-contract-absent-batch-034
batch_034_heldout_accuracy: 0.9399999976
batch_034_heldout_reward_margin: 0.0434329510
batch_034_heldout_loss: 0.6717466712
batch_034_final_accuracy: 0.8800000072
batch_034_final_reward_margin: 0.0367244902
batch_034_final_loss: 0.6750381553
batch_034_cpu_time_ms: 146703.125
batch_034_average_cpu_percent: 12.2891986375
batch_034_peak_ram_mb: 97.11328125
batch_034_elapsed_ms: 16753706.5583
batch_034_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-REALISTIC-STORED-PROCEDURE-PROCEDURE-CONTRACT-ABSENT-BATCH-034-REJECTED.md
batch_034_rejection: held-out accuracy regressed to 0.9399999976 and procedure_name_only fell to 0.7, even though other omitted-field types stayed at 1.0.
batch_034_fix: revert the heavier procedure_name_only rejected-token extension and do not continue from the batch-034 checkpoint.
candidate conservative continuation batch: project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-batch-035
batch_035_heldout_accuracy: 0.9800000191
batch_035_heldout_reward_margin: 0.0444802567
batch_035_heldout_loss: 0.6712136269
batch_035_final_accuracy: 0.9200000048
batch_035_final_reward_margin: 0.0397461573
batch_035_final_loss: 0.6735373437
batch_035_cpu_time_ms: 124531.25
batch_035_average_cpu_percent: 12.6691959764
batch_035_peak_ram_mb: 102.0859375
batch_035_elapsed_ms: 154539.0666
batch_035_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-REALISTIC-STORED-PROCEDURE-CONSERVATIVE-CONTINUATION-BATCH-035-CANDIDATE.md
batch_035_candidate_reason: held-out accuracy tied batch-033 and held-out reward margin improved by 0.0020473823; confirmation required.
confirmed conservative continuation batch: project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-confirmation-batch-036
batch_036_heldout_accuracy: 0.9800000191
batch_036_heldout_reward_margin: 0.0465080887
batch_036_heldout_loss: 0.6702278256
batch_036_final_accuracy: 0.9200000048
batch_036_final_reward_margin: 0.0416738741
batch_036_final_loss: 0.6725978446
batch_036_cpu_time_ms: 132640.625
batch_036_average_cpu_percent: 12.6415114228
batch_036_peak_ram_mb: 101.7890625
batch_036_elapsed_ms: 170532.5405
batch_036_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-REALISTIC-STORED-PROCEDURE-CONSERVATIVE-CONTINUATION-BATCH-036-CONFIRMED.md
batch_036_confirmation_reason: held-out accuracy tied batch-033 and batch-035, held-out reward margin improved by 0.0040752143 over batch-033 and 0.0020278320 over batch-035.
batch_036_checkpoint: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-confirmation-batch-036\dpo-runs\project-wiki-answer-composer-unit\project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-confirmation-batch-036-project-wiki-answer-composer-unit-20260522053121\checkpoints\policy_final.bin
batch_036_generated capability manifest: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-22-project-wiki-answer-composer-signal-token-realistic-stored-procedure-conservative-continuation-confirmation-batch-036\project-wiki-answer-composer-unit\project-wiki-answer-composer-20260522053411\generated_capability_manifest.json
batch_036_manifest_sha256: 86CCAEAB8326509866849BC98903A7323C4B713531AFE44335AB525B3015DA8B
residual watch after batch_036: procedure_name_only held-out accuracy 0.9 and min_margin -0.0222661048.
rejected queue-like stored-procedure probe: project-wiki-answer-composer-signal-token-realistic-stored-procedure-queue-like-batch-037
batch_037_heldout_accuracy: 0.9800000191
batch_037_heldout_reward_margin: 0.0482534990
batch_037_heldout_loss: 0.6693804860
batch_037_final_accuracy: 0.8800000072
batch_037_final_reward_margin: 0.0356301770
batch_037_final_loss: 0.6755805588
batch_037_cpu_time_ms: 157203.125
batch_037_average_cpu_percent: 12.6048650170
batch_037_peak_ram_mb: 85.71875
batch_037_elapsed_ms: 193230.8525
batch_037_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-REALISTIC-STORED-PROCEDURE-QUEUE-LIKE-BATCH-037-REJECTED.md
batch_037_rejection: held-out margin improved by only 0.0017454103 against batch-036 and final accuracy regressed to 0.8800000072; procedure_name_only residual remained.
rejected procedure-name chosen contrast probe: project-wiki-answer-composer-signal-token-realistic-stored-procedure-procedure-name-chosen-contrast-batch-038
batch_038_heldout_accuracy: 0.9399999976
batch_038_heldout_reward_margin: 0.0452767648
batch_038_heldout_loss: 0.6708521247
batch_038_final_accuracy: 0.9400000036
batch_038_final_reward_margin: 0.0424866777
batch_038_final_loss: 0.6722084618
batch_038_cpu_time_ms: 133187.5
batch_038_average_cpu_percent: 12.6303411788
batch_038_peak_ram_mb: 101.94921875
batch_038_elapsed_ms: 173100.7084
batch_038_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-REALISTIC-STORED-PROCEDURE-PROCEDURE-NAME-CHOSEN-CONTRAST-BATCH-038-REJECTED.md
batch_038_rejection: held-out accuracy and held-out reward margin both regressed against batch-036; procedure_name_only fell to 0.7 with three failed held-out rows.
batch_038_fix: revert the procedure_name_only chosen-side contrast token change and do not continue from batch-038.
rejected low-LR continuation probe: project-wiki-answer-composer-signal-token-realistic-stored-procedure-low-lr-continuation-batch-039
batch_039_heldout_accuracy: 0.9800000191
batch_039_heldout_reward_margin: 0.0473076664
batch_039_heldout_loss: 0.6698393226
batch_039_final_accuracy: 0.9240000045
batch_039_final_reward_margin: 0.0430220260
batch_039_final_loss: 0.6719414353
batch_039_cpu_time_ms: 162187.5
batch_039_average_cpu_percent: 12.6303533850
batch_039_peak_ram_mb: 86.45703125
batch_039_elapsed_ms: 218122.2603
batch_039_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-REALISTIC-STORED-PROCEDURE-LOW-LR-CONTINUATION-BATCH-039-REJECTED.md
batch_039_rejection: held-out margin improved by only 0.0007995777 against batch-036 and procedure_name_only residual remained.
rejected simplified name-rejected probe: project-wiki-answer-composer-signal-token-realistic-stored-procedure-simplified-name-rejected-batch-040
batch_040_heldout_accuracy: 0.8000000119
batch_040_heldout_reward_margin: 0.0314470381
batch_040_heldout_loss: 0.6778411865
batch_040_final_accuracy: 0.7800000131
batch_040_final_reward_margin: 0.0271595954
batch_040_final_loss: 0.6799356949
batch_040_cpu_time_ms: 164734.375
batch_040_average_cpu_percent: 12.6506356322
batch_040_peak_ram_mb: 102.203125
batch_040_elapsed_ms: 212954.3669
batch_040_report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-22-project-wiki-answer-composer-stored-procedure\PROJECT-WIKI-ANSWER-COMPOSER-REALISTIC-STORED-PROCEDURE-SIMPLIFIED-NAME-REJECTED-BATCH-040-REJECTED.md
batch_040_rejection: simplified rejected wording collapsed procedure_name_only to 0.0 accuracy; it removed too many concrete negative cues.
batch_040_fix: revert simplified procedure_name_only rejected wording and do not continue from batch-040.
next remediation: continue from batch-036 checkpoint. Avoid changing procedure_name_only wording blindly; use a new training strategy or switch to another wiki unit if this residual requires architecture support. Do not claim full wiki done.
full wiki done: false
```

#### Unit 10: ProjectAnswerVerifier

Responsibility: verify answer correctness, citations, project boundary, and
abstention behavior.

Minimum metrics:

```text
hallucination_catch_rate == 1.00
wrong_project_catch_rate == 1.00
stale_version_catch_rate >= 0.98
unsupported_answer_catch_rate >= 0.98
```

Current checkpoint:

```text
status: PROMOTED_MILESTONE_WITH_RUNNER_MANIFEST for ProjectAnswerVerifier
promoted DPO batch: project-answer-verifier-signal-promotion-confirmation-batch-019
dpo_heldout_accuracy: 0.9800000191
dpo_reward_margin: 0.0174925700
dpo_loss: 0.6844537854
dpo_final_accuracy: 1.0
dpo_final_margin: 0.0156566568
cpu_time_ms: 132875
average_cpu_percent: 12.4866366676
peak_ram_mb: 75.26953125
elapsed_ms: 173349.5792
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-project-answer-verifier-remediation\project-answer-verifier-batch-019-promotion-confirmation-result.json
publication: published through generated capability manifest
publication manifest: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-project-answer-verifier-promotion-confirmation-batch-019\project-answer-verifier-unit\project-answer-verifier-20260521191904\generated_capability_manifest.json
full wiki done: false
```

Failure modes found:

```text
Rejected batch: project-answer-verifier-batch-001.
Observed: DPO held-out accuracy 0.3000000119 while deterministic structural gate
passed. Cause: plain "passed/warnings" rows were too weak and hid false-positive
training.

Rejected batch: project-answer-verifier-signal-focused-batch-009.
Observed: held-out accuracy 0.2399999946. Cause: focused weak-type mix collapsed
wrong-project and declared-abstention discrimination. Do not continue from this
checkpoint.

Accepted remediation path:

```text
Restore from batch 012, not from collapsed batch 016.
Batch 017: signal wrong-project boundary remediation, held-out accuracy
0.9599999785, held-out margin 0.0081864260.
Batch 018: mixed confirmation, held-out accuracy 0.9800000191, held-out margin
0.0135476617.
Batch 019: promotion confirmation, held-out accuracy 0.9800000191, held-out
margin 0.0174925700, manifest emitted.
Residual watch item: rejects_valid_answer had 0.9 held-out accuracy and one
negative per-row margin. This passes the per-type gate but must remain in
real-project QA.
```
```

#### Unit 11: WikiTraceAggregator

Responsibility: record WikiQueryTrace and WikiUnitTrace, expose weak units, and
force remediation instead of blind continuation. This unit also owns the
cross-unit sequence check: query planning must verify project/version/evidence
prechecks before context retrieval, and evidence analysis must happen before
answer composition.

Minimum metrics:

```text
unit_trace_coverage == 1.00
failed_unit_has_remediation_action == 1.00
ten_batch_gap_analysis_presence == 1.00
cross_unit_sequence_accuracy >= 0.95
```

Proven correct pattern:

```text
A-side: WikiQueryTrace[] plus WikiUnitTrace[] containing:
  planned query steps,
  executed query steps,
  skipped steps,
  routed units,
  per-unit pass/fail metrics,
  failed units,
  remediation batches,
  stop_blind_training flag.

Chosen:
  failed_units: [the measured weak unit ids]
  remediation: [the concrete next 50-row remediation action]
  stop_blind_training: true when a required stage failed or pass rate is below gate
  query_pass_rate: [measured value]

The chosen side must preserve the unit sequence. For no_prechecks debt, it
must not blindly blame WikiContextRoutingUnit. It must identify whether the
query-planning precheck proof is missing and name ProjectWikiQueryPlanningFlow
and WikiTraceAggregator as the remediation owners when the trace order is
broken.
```

Proven rejected patterns:

```text
ignores_failed_unit:
  failed_units: none; remediation: none; stop_blind_training: false.

continues_blind_training:
  names a failed unit but allows continuation after flat or failed coverage.

missing_remediation:
  names a failure but gives no next 50-row training action.

context_before_query_plan_verified:
  hides that WikiContextRoutingUnit ran before ProjectWikiQueryPlanningFlow
  verified project, documentation-version, and evidence prechecks.

wrong_owner_for_no_prechecks:
  assigns a cross-unit missing-query-plan failure to context-routing label rows
  instead of training ProjectWikiQueryPlanningFlow and WikiTraceAggregator.
```

Failure modes found:

```text
Short target-signal labels reversed failed-trace rows in early runs and should
not be used for this unit.

Broad all-unit vocabulary grouped by rejected type caused recency/type collapse.
If broad vocabulary is used again, row order must be balanced and per-type
diagnostics must pass.

Batch 021 passed numerically for context-before-query-plan rows but used the
fallback incomplete_trace_summary annotation. It is not the accepted pattern.
Batch 022 confirms the same context-before-plan capability with the precise
[WRONG: context_before_query_plan_verified ...] rejected side.
```

Current checkpoint:

```text
status: PROMOTED_TARGET_SIGNAL_WITH_CONTEXT_SEQUENCE_CONFIRMATION
batch: wiki-trace-aggregator-context-before-confirmation-022
held_out_accuracy: 1.0
held_out_reward_margin: 0.016503408551216125
final_accuracy: 1.0
final_reward_margin: 0.017778544779866935
cpu_time_ms: 103796.875
average_cpu_percent: 12.69575509149772
peak_working_set_mb: 101.1328125
elapsed_ms: 143022.8577
checkpoint: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-wiki-trace-aggregator-context-before-confirmation-022\dpo-runs\wiki-trace-aggregator-unit\wiki-trace-aggregator-context-before-confirmation-022-wiki-trace-aggregator-unit-20260521212719\checkpoints\policy_final.bin
manifest: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\runner-metrics\2026-05-21-wiki-trace-aggregator-context-before-confirmation-022\wiki-trace-aggregator-unit\wiki-trace-aggregator-20260521212942\generated_capability_manifest.json
latest_report_artifact: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-wiki-trace-aggregator-remediation\WIKI-TRACE-AGGREGATOR-PHASE-METRICS-v1.md
full wiki done: false
```

Real-project trace validation update 2026-05-22:

```text
status: REAL_PROJECT_TRACE_VALIDATION_GATE_PASSED
scope: five legacy .NET projects after orchestrated documentation rebuild and query-intent fix
report: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-real-project-wiki-validation\REAL-PROJECT-WIKI-VALIDATION-PHASE-REPORT.md
source_summary: C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\2026-05-21-real-project-wiki-validation\REAL-PROJECT-WIKI-VALIDATION-AFTER-QUERY-INTENT-FIX-SUMMARY.json
project_count: 5
evidence_record_count: 616
total_question_count: 6393
exact_evidence_accuracy: 1.0
query_planning_invocation_rate: 1.0
entity_impact_trace_pass_rate: 1.0
stored_procedure_questions: 13
CPU time ms: 23750
peak RAM MB: 16.28997802734375
elapsed ms: 22874.23179999999
full wiki done: false
```

---
## Section 19: Protocol Update Procedure

This section defines exactly when and how the protocol must be updated.

### Trigger 1: After every promoted run

When a combo run is promoted to the new baseline, update the following:

**In Section 18 (Unit Solution Library):**

```
1. Find the unit entry for the domain that was trained.
2. Add any new proven correct pattern shapes discovered in this run.
   Include: the A-side structure, the chosen side format, why it worked.
3. Add any new proven wrong patterns from rejected sides that produced
   strong contrast.
4. Update "Current trained checkpoint" and "Rows in baseline".
5. Add "Latest report artifact" with the persisted JSON `report_artifact_path`
   that supports the promoted metrics. Do not copy transient terminal numbers
   without a report artifact.
6. If this run tested a new language variant, add to "Languages with
   verified rows".
```

**In Section 15 (Current Continuation Rule):**

```
Replace the current continuation rule with the new one.
Keep the old rule as a dated archive entry below the new rule.
Format:
  ## 15.1 Archived rule from V309 (2026-05-12)
  [old rule text]
  ## 15.2 Current continuation rule from VNEW (date)
  [new rule text]
```

**In Section 13 (Common Failure Modes):**

```
If the run discovered a new failure mode, add it.
Format:
  ### Failure Mode N: [name]
  Symptom: [what was observed in metrics]
  Root cause: [why it happened]
  Fix: [what resolved it]
  Baseline where discovered: [V-number]
```

### Trigger 2: After every rejected probe

When a probe fails Gate A or Gate B, update:

**In Section 18:**

```
Under the relevant unit entry, add to "Failure modes found":
  - row shape that failed;
  - metrics observed (accuracy, margin, loss);
  - report artifact path that contains those metrics;
  - ablation result: which specific rows were bad;
  - raw ids blocked;
  - reintroduction condition ("reintroduce when X exists").
```

**In the executor checklist (Section 17):**

```
Add to known-debt filters any newly blocked raw id range or row shape.
```

### Trigger 3: After any new source family is introduced

When rows from a new source (new session export, new external dataset,
new language corpus) enter training:

**In Section 6 (How Candidate Data Is Built):**

```
Add a named entry for the new source family:
  Name: [source family name]
  Location: [path or URL]
  Row count: [how many usable rows]
  Domain distribution: [orchestration/plans/skills breakdown]
  Proven shapes: [which row shapes from this source passed standalone]
  Blocked shapes: [which shapes failed and why]
  Language: [English / Python / TypeScript / etc.]
  License: [license state]
  Added to baseline in: [V-number or PENDING]
```

### Trigger 4: After every ten-batch review

The ten-batch review (Section 9) must produce a written update:

```
## Section 9.N: Ten-batch review after VN (date)
  Coverage delta: [what improved]
  Weakest unit: [which unit had lowest metrics]
  Next 50-row batch target: [which gap the next batch addresses]
  New known debt: [any newly discovered blocked shapes]
  Language coverage: [any gaps by language]
```

---

## Section 20: Multi-Language Parallel Training Guide

The orchestration intelligence trained in this protocol is currently in English
only. The long-term goal is to run the same curriculum in Python, TypeScript,
Java, and other languages used in the validation projects and customer codebases.

### Principle

The unit solution library in Section 18 is language-independent at the
reasoning level. A `ProjectWikiQueryPlanningFlow` row that teaches "turn the
developer's entity-impact question into ordered evidence queries before
retrieval and answer composition" is as valid in Python as in C#.

Only the domain examples change per language. The routing logic, the dependency
ordering, the authority detection - these are universal.

### Parallel Training Process

For each target language:

**Step 1: Map existing English rows to language equivalents.**

Take every promoted English row in Section 18. Identify the underlying
reasoning pattern. Produce a parallel row where the domain example uses
the target language.

Example mapping, English â†’ Python:

```
English row A-side:
  "Add a new authentication provider and update the user flow tests
   in the ASP.NET Core application."

Python equivalent A-side:
  "Add a new authentication provider and update the user flow tests
   in the FastAPI application."

Chosen side remains structurally identical:
  "[domain: plans] [sub_phase: 3.1] Route to SchemePlannerUnit.
   Dependency order: authentication provider definition must precede
   test update. Authority: plan authority owns this decision."

[WRONG: ...] shape remains structurally identical:
  "Jumping to test update before the provider exists.
   [WRONG: tests reference the provider; provider must be defined first]"
```

**Step 2: Validate language-specific adaptations.**

Each new language row must pass the same standalone gate as English rows.
A Python row that produces low accuracy in standalone is not a language
translation problem - it is a row quality problem. Apply the same
ablation method.

**Step 3: Record language variants in Section 18.**

Under each unit entry, update "Languages with verified rows" and
"Languages pending".

### Language Coverage Table

Update this table after every new language batch is promoted.

| Unit | English | Python | TypeScript | Java | Rust | Other |
|---|---|---|---|---|---|---|
| ProjectWikiRequestRouter | pending | pending | pending | pending | pending | |
| StackAndVersionClassifier | pending | pending | pending | pending | pending | |
| CodeTraceBuilder | promoted 2026-05-21 runner-manifest milestone, real-project QA pending | pending | pending | pending | pending | |
| EntityImpactMapper | promoted 2026-05-21 runner-manifest milestone, real-project QA watch items | pending | pending | pending | pending | |
| DocumentationGenerator | promoted 2026-05-21 stored-procedure documentation milestone, full-project QA pending | pending | pending | pending | pending | |
| NegativeDpoGenerator | promoted 2026-05-19 | pending | pending | pending | pending | |
| ProjectWikiQueryPlanningFlow | promoted 2026-05-21 query-series + stored-procedure milestones; real-project validation passed 2026-05-22 on 5 projects / 6393 questions / exact evidence 1.0 | pending | pending | pending | pending | |
| WikiContextRoutingUnit | promoted 2026-05-21 target signal, no_pre_checks debt | pending | pending | pending | pending | |
| WikiAnswerComposer | promoted 2026-05-21 general answer signal; stored-procedure answer-composition signal promoted 2026-05-22 with batch-036, full project wiki done false, procedure_name_only residual remains | pending | pending | pending | pending | |
| ProjectAnswerVerifier | promoted 2026-05-21 runner-manifest milestone, real-project QA pending | pending | pending | pending | pending | |
| WikiTraceAggregator | promoted 2026-05-21 unit signal + context-before-query-plan sequence confirmation; real-project trace validation passed 2026-05-22 on 5 projects / entity impact trace 1.0 | pending | pending | pending | pending | |

"Pending" means no verified rows yet. Fill each cell when a batch for that
unit and language is promoted.

### Language-Specific Adaptation Notes

These notes must be expanded as each language is trained. The pattern for
every entry:

```
Language: [name]
Key differences from English/C# patterns:
  [list the structural differences in how developers phrase tasks,
   name modules, describe flows - not syntax, but reasoning patterns]
Domain examples to prioritize first:
  [which scenarios from orchestrator_prompts.txt are most natural in this language]
Blocked row shapes (language-specific):
  [any shapes that fail standalone in this language but not in others]
First verified batch:
  [V-number when first batch is promoted, or PENDING]
```

**Python:**

```
Language: Python
Key differences: module naming is different; "service" may be "module" or
  "class"; async patterns use asyncio rather than Task<T>; dependency
  injection is explicit in FastAPI/Django but not universal.
Domain examples to prioritize first:
  FastAPI routes, Pydantic models as entity equivalents, SQLAlchemy
  repository patterns. These map cleanly to the entity impact and
  change location gaps.
Blocked row shapes: [PENDING - update after first standalone run]
First verified batch: PENDING
```

**TypeScript:**

```
Language: TypeScript
Key differences: "entity" may be an interface or a Zod schema; repositories
  are often omitted in favor of ORM direct calls; "service" naming is common
  in NestJS but not in Express. The orchestrator prompts describe a React
  Native client - TypeScript rows should include frontend flow traces.
Domain examples to prioritize first:
  NestJS service/controller/repository patterns. React Native component
  data flow. Both are present in orchestrator_prompts.txt.
Blocked row shapes: [PENDING]
First verified batch: PENDING
```

---

## Section 21: How The Protocol Stays Authoritative

A living document fails if it grows without discipline. Two rules prevent that.

### Rule 1: Section 18 is the only place for proven patterns.

Do not add proven patterns to planning notes, evidence reports, or session
state documents. Those are ephemeral. Section 18 is permanent. When a pattern
is proven, it moves to Section 18. When it is superseded, the old entry is
archived under a dated heading, not deleted.

### Rule 2: Every version of the protocol has a date and a baseline reference.

When this document is updated, the header must record:

```
Version: [N]
Date: [date]
Latest promoted baseline: [V-number]
Sections updated this version: [list]
```

This means any executor can read the protocol, see that it was last updated
after V309, and know that anything promoted after V309 has not yet been
recorded. That is a clear signal to update before training.

The protocol is never "done". It is always at a point in time. The executor's
job is to push that point forward.

---

## Executor Checklist Additions (appends to Section 17)

Add these checks to the existing checklist:

Before building a new batch:

- [ ] Read the latest version of Section 18. Identify which unit is being
      trained. Confirm the proven correct pattern for that unit is understood.
- [ ] Check the Language Coverage Table. If this batch includes a new language,
      confirm the language-specific adaptation notes have been read.

After promotion:

- [ ] Update Section 18 for the promoted unit: add new proven patterns, update
      checkpoint and row count.
- [ ] Update Section 15 with the new continuation rule, archiving the old one.
- [ ] If a new failure mode was discovered, add it to Section 13.
- [ ] If a new source family was introduced, add it to Section 6.
- [ ] Update the Language Coverage Table.
- [ ] Update the protocol version header with date and baseline reference.

After rejection:

- [ ] Add the failure to Section 18 under "Failure modes found" for the
      relevant unit.
- [ ] Update the known-debt filters in Section 17.

