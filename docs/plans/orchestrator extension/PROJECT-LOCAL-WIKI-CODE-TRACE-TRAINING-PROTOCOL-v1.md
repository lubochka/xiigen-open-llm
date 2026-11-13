# PROJECT LOCAL WIKI CODE TRACE AND TRAINING PROTOCOL v1

Status: REQUIRED EXECUTION TRACE.

Governed by:

- `EXECUTOR-DISCIPLINE-RULES-v1.md` - executor behavior, stop rules, and "done" claim restrictions.
- `ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md` - raw metric reporting, CPU/RAM/time reporting, below-0.75 sourcing behavior, and 50-row loop rules.
- `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md` - prompt-derived validation tasks and required variations for developer-reasoning wiki gaps.
- `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md` - trainable query-planning flow between project routing and wiki retrieval.
- `ORCHESTRATION-TRAINING-PROTOCOL-LIVING-GUIDE-v1.md` - unit solution library and required protocol update after every promoted or rejected run.
- `EXECUTOR-STAGE-PROMPT-v2.md` - short prompt for restoring the current stage goal, sub-stage reads, and dilemmas.

This protocol defines the real goal, the trace that must be built from code,
the expected output at each stage, and the metrics that must be watched during
training.

## Main Goal

Given a real .NET project, the system must accept one natural orchestration
request and produce a project-local documentation/wiki unit that can answer
questions about that project with exact code evidence.

Correct user-level request:

```text
Document this .NET project, train its local wiki, and register it for project
questions.
```

The user must not need to know about internal files, JSONL ledgers, or relation
snapshots. Those are implementation details.

## Not The Goal

The goal is not:

- to write markdown files only;
- to write JSONL files only;
- to run BM25 over a file and call it a wiki;
- to pass one sample question;
- to report structural gate accuracy as model accuracy;
- to call the local wiki trained when no runner has produced weights.

## Code Trace Object

Every run must build a trace from code. The trace is the object followed during
training.

```text
ProjectCodeTrace
  project_id
  repository_root
  target_frameworks
  csharp_language_versions
  source_files
  symbols
  methods
  constructors
  properties
  entrypoints
  call_edges
  value_flow_edges
  unresolved_calls
  external_calls
  documentation_units
  negative_dpo_rows
  wiki_query_plans
  training_batches
  heldout_batches
  coverage_snapshots
```

The trace must be updated after every training batch. It is not optional.

## Pretrained Unit Rule

All wiki units must be trainable units and should be trained before project
validation. Project code examples are validation instruments and project-local
evidence; they are not the generic source used to teach developer reasoning.

Required trainable units:

- `ProjectWikiRequestRouter`;
- `StackAndVersionClassifier`;
- `CodeTraceBuilder`;
- `EntityImpactMapper`;
- `DocumentationGenerator`;
- `NegativeDpoGenerator`;
- `ProjectWikiQueryPlanningFlow`;
- `WikiContextRoutingUnit`;
- `WikiAnswerComposer`;
- `ProjectAnswerVerifier`;
- `WikiTraceAggregator`.

Each unit must have train rows, held-out rows, DPO rejected examples, metrics,
and remediation actions. A unit without these is not accepted as "trained".

## Stage 0: Orchestration Intake

Input:

- natural user request;
- project root;
- source authority/consent state.

Expected result:

- main orchestrator routes to documentation/wiki domain;
- selected project id is known;
- project consent is not blocked;
- run id is created;
- trace starts with status `INTAKE_ACCEPTED`.

Metrics:

- request_routed_to_documentation = 1;
- source_authority_status;
- rejected_if_blocked_no_consent.

Not accepted:

- direct manual call to a JSONL file;
- direct manual execution of a storage script as the primary product.

## Stage 1: Source Inventory And Stack/Version Detection

Input:

- project root.

Expected result:

- all relevant `.cs`, `.csproj`, `.config`, `.aspx`, `.ascx`, `.xaml`, MVC,
  WebForms, WPF, API controller, service, repository, and utility files are
  inventoried;
- target framework and C# language version are inferred per project or per
  subproject;
- generated/bin/obj/vendor files are excluded.

Metrics:

- source_files_total;
- source_files_included;
- source_files_excluded;
- target_frameworks_detected;
- csharp_versions_detected;
- stack_family_distribution.

Expected failure reasons:

- missing project files;
- unknown framework version;
- generated files not excluded;
- multiple project roots mixed without project id.

## Stage 2: Symbol And Method Trace

Input:

- source inventory.

Expected result:

- every documentable symbol receives a stable trace id;
- methods have source path, line, signature, parameters, return type, class,
  namespace, and framework version;
- leaf functions and higher-level callers are separated.

Metrics:

- methods_total;
- constructors_total;
- properties_total;
- controllers_total;
- services_total;
- repositories_total;
- methods_with_parameters;
- methods_with_return_type;
- methods_missing_location;
- methods_missing_signature.

Expected failure reasons:

- parser misses legacy method shapes;
- async/generic/partial methods missed;
- WebForms event handlers missed;
- overloaded methods collapse to one id.

## Stage 3: Callgraph And Value-Flow Trace

Input:

- symbol/method trace.

Expected result:

- caller -> callee edges are recorded;
- arguments sent to callees are recorded;
- returned values and how the caller uses them are recorded when inferable;
- unresolved calls are explicitly marked, not hidden.

Metrics:

- call_edges_total;
- value_flow_edges_total;
- unresolved_calls_total;
- external_calls_total;
- methods_with_callee_context;
- methods_with_caller_context;
- missing_transit_edges;
- missing_workflow_methods.

Expected failure reasons:

- dynamic dispatch not resolved;
- interface implementation not linked;
- reflection/string-based calls not linked;
- dependency injection call path not connected;
- WebForms lifecycle calls not connected.

## Stage 4: Bottom-Up Documentation Generation

Input:

- code trace;
- callgraph;
- value-flow trace.

Expected result:

For every documented unit:

- what the code does;
- where it is located;
- which parameters it accepts;
- what it returns;
- who calls it;
- which functions it calls;
- what values it sends to callees;
- what values it receives from callees;
- what it does with returned values;
- framework/language version context.

Metrics:

- documentation_units_total;
- methods_documented;
- leaf_units_documented;
- caller_units_documented;
- workflow_units_documented;
- documentation_quality_score;
- bottom_up_propagation_score;
- source_evidence_score;
- stack_version_score.

Expected failure reasons:

- summary says what the method name says but not what code does;
- caller docs omit callee meaning;
- parameters copied without meaning;
- return value not explained;
- framework/version missing.

## Stage 5: Documentation Acceptance Gate

Input:

- generated documentation units;
- source trace.

Expected result:

- accepted docs enter the knowledge graph;
- rejected docs are blocked and logged with repair reason.

Hard pass thresholds:

```text
documentation_quality_score >= 0.95
source_evidence_score == 1.00
stack_version_score == 1.00
bottom_up_propagation_score == 1.00
hard_failure_count == 0
```

Not accepted:

- docs without source location;
- docs without parameters/return/callers where applicable;
- caller docs without callee transit;
- invented behavior;
- wrong framework/version.

## Stage 6: Negative DPO Generation

Input:

- accepted documentation graph;
- source trace.

Expected result:

- negative documentation DTOs are generated automatically;
- negatives are plausible but provably wrong;
- no question rows are generated in Phase 1.

Required negative types:

- wrong meaning;
- missing source location;
- missing parameter meaning;
- wrong return behavior;
- missing caller context;
- missing callee transit;
- wrong argument mapping;
- wrong return-value usage;
- wrong framework/version;
- invented side effect.

Metrics:

- negative_candidates_total;
- accepted_negative_candidates;
- rejected_negative_candidates;
- negative_dto_acceptance_rate;
- negative_type_distribution;
- chosen_equals_rejected_count;
- missing_wrong_annotation_count.

Expected failure reasons:

- rejected is just a shallow negation;
- wrongness cannot be verified from source trace;
- rejected omits consequence;
- generated negative is actually valid.

## Stage 7: 50-Row Training Batch

Reporting and executor behavior for this stage are not local inventions.
Use `ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md` for the raw
numbers to display after every run. Use `EXECUTOR-DISCIPLINE-RULES-v1.md` for
the rule that a passed gate immediately advances to the next required action.

Input:

- accepted documentation DTO rows;
- accepted negative DPO rows.

Expected result:

- exactly 50 train rows;
- held-out rows are disjoint;
- batch records which trace gaps it targets.
- at least 40% of broad wiki batches are developer-reasoning rows, not simple
  method-description rows.

Default broad wiki batch distribution:

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

Metrics per batch:

- train_rows = 50;
- heldout_rows;
- pair_type_distribution;
- project_distribution;
- documentation_level_distribution;
- targeted_gap_types;
- CPU per row;
- elapsed time per row;
- peak RAM;
- DPO loss;
- reward margin;
- DPO accuracy;
- heldout accuracy.

Persistence requirement:

- write the metrics to a JSON report artifact before any user-facing report;
- training-runner metrics go to `runner-reports/`;
- phase/test validation metrics go to
  `C:\Users\lubao\Documents\xiigen\llm mvp\orchestrator extension\phase-metrics\<run-id>\PHASE-RESOURCE-REPORT.json`;
- the user-facing message must cite `report_artifact_path`;
- if the report artifact is missing, the status is
  `REPORT_ARTIFACT_WRITE_FAILED`, not `done`, `passed`, or `promoted`.

Expected failure reasons:

- batch repeats already covered easy rows;
- no negative examples for current gap;
- pair does not reference trace evidence;
- held-out overlaps train.

## Stage 8: Training Runner And Weight Publication

Input:

- train batch;
- held-out batch;
- base model manifest.

Expected result:

- runner produces model checkpoint or explicitly fails;
- model manifest is updated only after real metrics pass;
- weights are stored through model artifact storage, not committed to git.

Metrics:

- runner_status;
- checkpoint_id;
- adapter_size_mb;
- train_loss;
- heldout_accuracy;
- documentation_unit_accuracy;
- relation_edge_accuracy;
- negative_dto_rejection_accuracy;
- citation_accuracy;
- CPU/RAM/time.

If runner is missing:

```text
DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING
```

This status is not trained.

## Stage 9: Project Wiki Coverage Verification

Validation questions for this stage must be selected from
`PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md`. A single happy-path question is not
coverage. The stage must include developer-request variations such as entity
impact, workflow trace, change location, regression risk, missing evidence, and
wrong-project/stale-version rejection. If the project wiki contains
SQL/stored-procedure evidence, the stage must also include stored-procedure
logic variations: SQL parameters, tables read/written, result shape,
transaction behavior, side effects, .NET callers, value flow, tests, and
missing evidence.

Before any answer is composed, each question must pass through
`ProjectWikiQueryPlanningFlow` as defined in
`PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`. The evaluated chain is:

```text
question -> query plan -> retrieved evidence -> answer -> verifier decision
```

The plan must state what the wiki should search for, why each query is needed,
which evidence would be sufficient, and what missing evidence should force
abstention.

Input:

- trained or candidate local wiki unit;
- project code trace;
- documentation graph.

Expected result:

- generated and real questions are asked through `ProjectLocalWikiOrchestrator`;
- `ProjectWikiQueryPlanningFlow` produces a multi-step query plan for each
  developer question;
- answers cite exact evidence;
- cross-project and stale-version questions are rejected.

Question types:

- meaning;
- location;
- parameters;
- returns;
- callers;
- callees;
- workflow/transit;
- stored procedure logic when SQL/procedure evidence exists;
- why/purpose;
- multi-hop;
- wrong-project;
- stale-version.

Metrics:

- question_count;
- answered_question_count;
- query_plan_accuracy;
- required_touchpoint_recall;
- multi_hop_sequence_accuracy;
- answer_readiness_score;
- exact_evidence_accuracy;
- hallucination_rate;
- cross_project_leakage;
- stale_version_acceptance;
- missing_question_types;
- unanswered_question_types.

Expected failure reasons:

- evidence retrieved but wrong method;
- answer omits caller/callee transit;
- cross-project answer accepted;
- answer cites stale documentation.

## Stage 10: Ten-Batch Review

After every 10 batches:

- compare coverage with previous snapshot;
- report which methods remain undocumented;
- report which call edges remain unresolved;
- report which value-flow edges are missing;
- report which question types fail;
- report resource cost per row and per generated answer;
- save the ten-batch review as a persisted report artifact before showing its
  CPU/RAM/accuracy numbers to the operator;
- create the next 50-row batch from uncovered gaps.

If coverage did not improve:

```text
STOP_BLIND_TRAINING_AND_ANALYZE_GAPS
```

The next batch must target the reason for no improvement.

## Stage 11: Entity Impact Map

After documentation generation, build a touch point map for each entity or DTO:

- who accepts it;
- who validates it;
- who maps it;
- who stores it;
- who serializes it;
- who reads each field;
- which calculations/processes depend on the field;
- which tests cover it;
- which areas remain unresolved.

Metrics:

```text
entities_with_complete_touch_map
entities_with_partial_touch_map
entities_with_empty_touch_map
entity_impact_path_accuracy
calculation_impact_recall
partial_map_labeled_rate
```

Partial maps are allowed only when explicitly labeled with the missing reason.

## Stage 12: Developer Reasoning Row Generation

Generate rows from developer task patterns, not from method-description shape.

Required patterns:

- add field to entity/DTO;
- rename method/property;
- change validation rule;
- add calculation input;
- change storage mapping;
- add API response field;
- change serializer/external contract;
- add UI/display field;
- trace multi-step workflow;
- locate correct change point;
- estimate regression risk;
- reject unsupported answer due to missing evidence.

The A side is a developer request. For query-planning rows, the chosen side is
a grounded investigation plan with evidence requirements. For answer rows, the
chosen side is a grounded reasoning answer with evidence. The rejected side is
a plausible but incomplete or wrong developer response with `[WRONG: ...]`.

## Stage 13: Wiki Query And Unit Trace

The query trace must explain which trainable unit handled each part of the
prompt and must use the unit names and remediation rules from
`ORCHESTRATION-TRAINING-PROTOCOL-LIVING-GUIDE-v1.md`. If a unit is below 0.75,
its trace status is not "done"; it is "data sourcing active" with the next
50-row action recorded.

Every wiki question must produce `WikiQueryTrace`. Every batch must produce
`WikiUnitTrace`.

`WikiQueryTrace` and `WikiUnitTrace` must reference the saved metric artifact
that justified the current status. A trace with CPU/RAM/accuracy values but no
`report_artifact_path` is not an accepted trace.

`WikiQueryTrace` records:

- query id;
- project id;
- documentation version;
- question type;
- query plan id;
- planned intent;
- planned entities;
- planned query steps;
- executed query steps;
- skipped query steps;
- plan sufficiency score;
- answer readiness score;
- routed units;
- unit outputs and scores;
- evidence ids used;
- confidence;
- verifier result;
- CPU/RAM/time;
- final status.

`WikiUnitTrace` records:

- batch id;
- rows trained;
- held-out rows;
- unit metrics;
- failed units;
- remediation batches required;
- coverage delta;
- whether blind training must stop.

## Five Validation Projects

The protocol must run against:

```text
C:\Projects\wizGameserver
C:\Projects\GenieDynamicServer
C:\Projects\geniesoftGameMVPServer
C:\Projects\genietasks
C:\Projects\askapediaweb
```

The expected result is not the same for all projects. Small projects may have
few or no transit edges. That must be stated explicitly as coverage reality,
not hidden.

## Current Implementation Status

Implemented or partially implemented:

1. Main orchestrator entry point exists as `ProjectDocumentationPipelineFlowAdapter`
   registered by `LogicalOrchestratorAssistant.CreateDefault`.
2. Code trace exists in `ProjectDocumentationKnowledgeGraph.CodeTrace` for the
   current run.

These are not done until implemented and verified:

1. Code trace persistence and batch-to-batch update after every training batch.
3. Robust parser for legacy .NET beyond simple method matching.
4. Real documentation LLM generation and acceptance scoring.
5. Trainable negative DPO generator.
6. Documentation wiki training runner wired to this domain.
7. Weight publication through `IModelRepository`.
8. Full five-project coverage gate with missing coverage reasons.
9. Phase 2 trainable question database.
10. Project question routing through trained `ProjectWikiRoutingFlow`.
11. Project question planning through trained `ProjectWikiQueryPlanningFlow`.
12. Stored procedure evidence tracing for legacy SQL-backed projects.

## Correct Final Claim

The final claim may be made only in this form:

```text
The project-local wiki flow is accepted for <project_id> because the
orchestrated request completed, the code trace is complete enough for the
declared scope, documentation quality gates passed, negative DPO gates passed,
training metrics passed, project question planning passed, and project
question coverage passed with exact evidence citations.
```

Any weaker state must name the blocker directly.
