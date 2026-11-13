# PLAN WIKI TRAINING GAP CLOSURE v1

Status: REQUIRED REFERENCE.
Language: English.
Scope: project-local wiki orchestration, documentation reasoning, developer-task answering, and unit-level training trace.

Required companion references:

- `PROJECT-LOCAL-WIKI-MODEL-PLAN-v1.md` - architecture and session map.
- `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md` - trace stages and expected outputs.
- `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md` - done gates.
- `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md` - prompt-derived validation tasks and required variations.
- `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md` - required trainable flow that turns developer questions into wiki evidence-query plans.
- `EXECUTOR-DISCIPLINE-RULES-v1.md` - executor stop/continue rules and "done" claim restrictions.
- `ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md` - raw metrics, resource reporting, and below-0.75 behavior.
- `ORCHESTRATION-TRAINING-PROTOCOL-LIVING-GUIDE-v1.md` - unit solution library and protocol update cadence.
- `EXECUTOR-STAGE-PROMPT-v2.md` - short stage prompt that tells the executor where to read and which dilemmas must be resolved by protocol.

This document closes the gap between "the system documents code" and "the system
helps a developer understand how to change a real legacy project safely."

## Main Goal

All wiki-related units must be trainable units. They should be trained before
being tested on customer/project code.

Project code examples are validation instruments. They are not the main source
used to teach generic developer reasoning.

Correct goal:

```text
Given a developer request such as:
"I want to add DiscountCode to CampaignCreateDto. What path does this entity
travel through, what calculations and processes will it affect, and where can
this break?"

The orchestrator routes the request through trained wiki units, retrieves the
right project-local evidence, follows entity/data/control-flow impact, answers
with exact citations, and records which trained unit succeeded or failed.
```

## What Was Wrong Before

The earlier plan trained mostly on code-description examples:

```text
A: What does MethodX do?
Chosen: MethodX returns a string and is called by ControllerY.
Rejected: MethodX does something but omits return type.
```

That teaches documentation shape. It does not teach developer work.

The wiki must learn developer reasoning patterns before it sees a concrete
project:

- entity impact;
- data-flow trace;
- change location;
- regression risk;
- dependency impact;
- architectural boundary;
- missing evidence and abstention;
- stale version rejection;
- cross-project rejection.

## Core Training Principle

There are two layers:

1. Generic trainable wiki units.
   These are trained ahead of time from licensed/open/internal approved learning
   sets and synthetic developer-task rows. They learn how to reason.

2. Project-local evidence and validation.
   A real project provides code trace, documentation graph, and question checks.
   It validates whether the already-trained units can answer grounded project
   questions. It may produce a project-local adapter only after consent and
   after the generic units pass.

Do not collapse these two layers.

## Trainable Units

Every unit below must have:

- a train set;
- a held-out set;
- DPO rejected examples;
- per-batch metrics;
- per-unit trace;
- remediation actions when quality is low.

Per-batch metrics are valid only when persisted. Every batch must write a JSON
report artifact before any user-facing CPU, RAM, accuracy, loss, reward margin,
or held-out metric is shown. Use `runner-reports/` for training-runner reports
and `orchestrator extension/phase-metrics/<run-id>/PHASE-RESOURCE-REPORT.json`
for phase/test validation reports. If the artifact is missing, the status is
`REPORT_ARTIFACT_WRITE_FAILED`, not a completion or promotion claim.

### Unit 1: ProjectWikiRequestRouter

Responsibility:

- decide whether a user prompt belongs to project-local wiki;
- choose documentation, entity-impact, flow-trace, architecture, or abstention
  path;
- reject wrong project and stale version prompts.

Minimum metrics:

```text
wiki_request_routing_accuracy >= 0.92
wrong_project_rejection == 1.00
stale_version_rejection >= 0.98
```

Remediation when low:

- add 50-row batches with confusing prompts: generic coding request vs local
  project question vs stale-version question;
- add negative DPO where rejected routes directly to code generation instead of
  wiki reasoning.

### Unit 2: StackAndVersionClassifier

Responsibility:

- identify .NET Framework, .NET Core, ASP.NET MVC, WebForms, WPF, Web API,
  library/service/test project, C# language era;
- decide when version is unknown and must be marked as unknown.

Minimum metrics:

```text
stack_version_accuracy >= 0.95
unknown_version_false_claim_rate == 0
```

Remediation when low:

- add 50 rows for mixed legacy project layouts;
- add negatives that claim ASP.NET Core when evidence shows WebForms/MVC;
- add negatives that invent C# language version.

### Unit 3: CodeTraceBuilder

Responsibility:

- build the code trace: symbols, methods, constructors, properties, entrypoints,
  call edges, value-flow edges, unresolved calls, external calls;
- mark unresolved areas honestly.

Minimum metrics:

```text
method_trace_recall >= 0.90
call_edge_precision >= 0.90
unresolved_call_labeled_rate >= 0.95
```

Remediation when low:

- add rows for overloads, event handlers, async methods, generic methods,
  interface dispatch, WebForms lifecycle, reflection, DI-created services;
- add negatives that hide unresolved calls instead of marking them.

### Unit 4: EntityImpactMapper

Responsibility:

- build `touch_point_map` for entities and fields;
- find where an entity is accepted, validated, mapped, persisted, serialized,
  returned, displayed, calculated, or tested;
- answer "what breaks if I add/change/remove this field?"

Minimum metrics:

```text
entity_impact_path_accuracy >= 0.85
field_touch_point_recall >= 0.85
calculation_impact_recall >= 0.80
partial_map_labeled_rate >= 0.95
```

Remediation when low:

- add 50-row batches where the chosen answer must include full path:
  DTO -> validator -> mapper -> service -> repository -> storage -> response/UI
  -> tests;
- add negatives that mention only DTO/controller and omit storage/calculation;
- add negatives that invent database impact without evidence.

### Unit 5: DocumentationGenerator

Responsibility:

- create bottom-up documentation from code trace;
- explain meaning, location, parameters, return value, callers, callees, value
  sent to callees, value received from callees, and return usage.

Minimum metrics:

```text
documentation_quality_score >= 0.95
source_evidence_score == 1.00
bottom_up_propagation_score == 1.00
```

Remediation when low:

- add rows where rejected describes the method name but not behavior;
- add rows where rejected omits caller/callee transit;
- add rows where rejected omits return usage.

### Unit 6: NegativeDpoGenerator

Responsibility:

- generate plausible but provably wrong rejected sides;
- name the defect and functional consequence in `[WRONG: ...]`.

Minimum metrics:

```text
chosen_not_equal_rejected == 1.00
wrong_annotation_specificity >= 0.95
provably_wrong_against_trace >= 0.95
shallow_negation_rate == 0
```

Remediation when low:

- add 50-row batches for specific negative types:
  missing_callee, wrong_argument_mapping, missing_return_usage,
  wrong_version, invented_side_effect, partial_entity_path;
- reject negatives that are only "not X".

### Unit 7: ProjectWikiQueryPlanningFlow

Responsibility:

- convert a developer question into a multi-step wiki evidence query plan;
- decide what should be searched, in what order, and why;
- define sufficiency checks before answer composition is allowed;
- detect missing evidence, stale documentation, and wrong-project risk before
  retrieval is treated as answer-ready.
- for legacy SQL questions, plan stored procedure definition, SQL parameters,
  table dependencies, result shape, transaction behavior, side effects, .NET
  callers, value flow, tests, and missing evidence before answer composition.

Minimum metrics:

```text
query_plan_accuracy >= 0.90
required_touchpoint_recall >= 0.95
multi_hop_sequence_accuracy >= 0.90
missing_evidence_detection >= 0.95
wrong_project_rejection >= 0.98
stale_version_rejection >= 0.98
irrelevant_query_rate <= 0.05
answer_readiness_score >= 0.90
```

Remediation when low:

- add 50-row batches where the A side is a real developer question and the
  chosen side is an investigation plan, not an answer;
- add negatives that do only one symbol lookup for entity-impact questions;
- add negatives that retrieve the correct entity but omit mapper, validator,
  persistence, callers, workflow branches, response projection, or tests;
- add stored-procedure negatives that retrieve the correct procedure but omit
  SQL parameters, result shape, table side effects, transaction behavior, or
  .NET callers;
- add rows where the correct plan is "do not answer yet; missing evidence is
  required."

### Unit 8: WikiContextRoutingUnit

Responsibility:

- choose evidence route: symbol lookup, callgraph, entity impact map, flow trace,
  BM25, GraphRAG, or abstention;
- avoid searching the wrong project.

Minimum metrics:

```text
context_route_accuracy >= 0.90
wrong_project_context_rate == 0
entity_impact_route_accuracy >= 0.85
```

Remediation when low:

- add rows where entity questions must route to entity impact map, not symbol
  summary;
- add rows where flow questions must route to callgraph/value-flow trace;
- add rows where missing evidence must abstain.

### Unit 9: WikiAnswerComposer

Responsibility:

- answer developer questions using retrieved project-local evidence;
- include impact path, consequences, risks, and citations;
- avoid pretending confidence when trace is partial.

Minimum metrics:

```text
answer_completeness >= 0.88
exact_evidence_accuracy >= 0.98
hallucination_rate == 0
partial_trace_disclosure >= 0.95
```

Remediation when low:

- add rows that require "known path", "uncertain path", and "next inspection";
- add negatives that omit risk/consequence;
- add negatives that give implementation advice without evidence.

### Unit 10: ProjectAnswerVerifier

Responsibility:

- catch wrong project, stale version, missing evidence, invented behavior,
  incomplete entity path, and unsupported confidence.

Minimum metrics:

```text
hallucination_catch_rate == 1.00
wrong_project_catch_rate == 1.00
stale_version_catch_rate >= 0.98
incomplete_entity_path_catch_rate >= 0.90
false_positive_rate <= 0.02
```

Remediation when low:

- add negative answer rows where the answer cites real evidence but skips a
  required impact segment;
- add negatives with valid-looking evidence from the wrong project.

### Unit 11: WikiTraceAggregator

Responsibility:

- record every unit invoked for every query;
- aggregate per-batch unit quality;
- decide the next training action when quality is low.

Minimum metrics:

```text
unit_trace_coverage == 1.00
failed_unit_has_remediation_action == 1.00
ten_batch_gap_analysis_present == 1.00
```

Remediation when low:

- block training continuation until each failed unit has a next 50-row batch
  plan tied to a measured failure type.

## Developer Question Taxonomy

Batch size is always 50 training rows. Do not change this to 54 or any other
number.

### Level 0: Project And Version Routing

- `wrong_project_question`
- `stale_version_question`

### Level 1: Symbol Understanding

- `symbol_question`
- `location_question`
- `parameter_question`
- `return_question`

### Level 2: Local Dependency And Transit

- `caller_question`
- `callee_question`
- `dependency_question`
- `value_flow_question`

### Level 3: Developer Change Reasoning

- `entity_impact_question`
- `flow_trace_question`
- `change_location_question`
- `impact_analysis_question`
- `regression_risk_question`
- `stored_procedure_impact_question`
- `stored_procedure_trace_question`

### Level 4: Architectural Reasoning

- `architectural_decision_question`
- `system_boundary_question`
- `feature_question`
- `integration_boundary_question`

### Level 5: Evidence Discipline

- `missing_evidence_question`
- `evidence_question`
- `wrong_level_question`
- `partial_trace_question`

## Default 50-Row Batch Distribution

Use this when a batch is broad. Focused remediation batches may target one
failed unit, but must still contain exactly 50 rows.

```text
entity_impact_question:          10
flow_trace_question:              8
change_location_question:         6
impact_analysis_question:         6
caller/callee/dependency/value:   6
architectural/boundary/feature:   5
missing_evidence/abstention:      4
wrong_project/stale/wrong_level:  5
TOTAL:                           50
```

Developer reasoning rows must be at least 40% of every broad wiki batch.
When a focused stored-procedure remediation batch is needed, keep exactly 50
rows and use `stored_procedure_impact_question` plus
`stored_procedure_trace_question` only. Project repositories with stored
procedures are validation surfaces unless their source authority explicitly
approves training use.

## Required Developer Task Patterns

These patterns must exist before testing on project code:

1. Add a field to an entity or DTO.
2. Rename a method or property.
3. Change validation rule.
4. Add a new calculation input.
5. Change persistence/storage mapping.
6. Add an API response field.
7. Change serializer or external contract.
8. Add a UI/display field.
9. Add an integration provider field.
10. Change or document a SQL Server stored procedure used by .NET code.
11. Add a field returned by a stored procedure result set.
12. Change stored procedure parameters, table writes, or transaction behavior.
10. Trace a multi-step workflow.
11. Locate correct change point.
12. Estimate regression risk.
13. Explain architectural boundary.
14. Reject unsupported answer due to missing evidence.

## Entity Impact Answer Contract

For "I want to add field X to entity Y", chosen answer must include:

```text
1. entity identity and evidence;
2. entrypoints that accept Y or Y's DTO;
3. validators that must accept/check X;
4. mappers/converters that copy X;
5. services/calculations affected by X;
6. repository/storage mapping affected by X;
7. serializers/API/UI outputs affected by X;
8. tests likely affected;
9. unresolved or partial trace areas;
10. concrete next inspection/action list.
```

Rejected answer examples:

- only mentions DTO and controller;
- skips repository/storage;
- skips calculations;
- invents a database migration when there is no evidence;
- says "update all references" without naming path;
- gives code changes without evidence citations.

## Wiki Query Trace

Every wiki question must produce:

```text
WikiQueryTrace
  query_id
  project_id
  documentation_version
  question_type
  expected_level
  routed_units[]
  unit_results[]
  retrieved_evidence_ids[]
  selected_answer_evidence_ids[]
  missing_evidence[]
  confidence
  verifier_result
  cpu_ms
  elapsed_ms
  peak_ram_mb
  final_status
```

Every unit result must include:

```text
unit_id
unit_version
trained_checkpoint_id
input_summary
output_summary
quality_score
pass_or_fail
failure_type
remediation_action
```

## Wiki Unit Trace

After every batch:

```text
WikiUnitTrace
  batch_id
  rows_trained
  heldout_rows
  unit_metrics[]
  failed_units[]
  remediation_batches_required[]
  coverage_delta
  stop_blind_training
```

If any unit has `FAIL` and no remediation action, the batch cannot be promoted.

## Ten-Batch Review

After every 10 batches:

- run all five validation projects;
- compare coverage to previous snapshot;
- report entity-impact coverage;
- report flow-trace coverage;
- report missing callgraph/value-flow reasons;
- report per-unit quality trend;
- report CPU/RAM/time per training row;
- report CPU/RAM/time per wiki question;
- generate the next 50-row remediation batch from the weakest measured unit.

If no coverage improvement:

```text
STOP_BLIND_TRAINING_AND_ANALYZE_GAPS
```

## Five Validation Projects

Project code is used for validation and project-local evidence, not for generic
pre-training of reasoning units.

Validation projects:

```text
C:\Projects\wizGameserver
C:\Projects\GenieDynamicServer
C:\Projects\geniesoftGameMVPServer
C:\Projects\genietasks
C:\Projects\askapediaweb
```

Each validation run must report:

- methods discovered;
- entities discovered;
- entity touch maps;
- callgraph edges;
- value-flow edges;
- unresolved calls;
- question count by type;
- exact evidence accuracy;
- entity-impact answer accuracy;
- flow-trace answer accuracy;
- missing coverage reasons.

## Required Updates To Existing Documents

Update `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md`:

- add trainable-unit list;
- add entity impact map stage;
- add developer reasoning row stage;
- add `WikiQueryTrace`;
- add `WikiUnitTrace`;
- add "project code validates, generic units pre-train" rule.

Update `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md`:

- done requires all wiki units to be trained or explicitly blocked;
- done requires entity-impact coverage;
- done requires unit trace coverage;
- done requires remediation actions for failed units.

Update SESSION-03:

- exactly 50 rows per batch;
- developer reasoning rows >= 40%;
- include entity impact and flow trace rows;
- keep questions as Phase 2, but developer reasoning DPO is Phase 1.

Update SESSION-05 and SESSION-06:

- use expanded question taxonomy;
- require `WikiQueryTrace`;
- require `WikiUnitTrace`;
- gate every project question through verifier and unit trace.

## Correct Final Acceptance Claim

Use this form only:

```text
The project-local wiki flow is accepted for <project_id> because the generic
wiki units were trained before project validation, the orchestrated request
completed, the code trace is sufficient for declared scope, entity-impact and
flow-trace questions pass thresholds, negative DPO gates pass, unit traces show
PASS or remediation for every unit, and project validation questions pass with
exact evidence citations.
```

If any part is missing, do not claim completion. Name the missing unit, metric,
or training resource.
