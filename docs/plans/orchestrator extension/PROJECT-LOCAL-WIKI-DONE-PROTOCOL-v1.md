# PROJECT LOCAL WIKI DONE PROTOCOL v1

Status: REQUIRED VERIFICATION PROTOCOL.

This protocol fixes the boundary of the project-local documentation wiki work.
The product is not a folder of JSONL files. The product is an orchestration
request that can document a project, train or reject the project-local wiki
unit, and then answer project questions through the normal orchestrator.

Executor stage behavior is governed by `EXECUTOR-STAGE-PROMPT-v2.md` and
`EXECUTOR-DISCIPLINE-RULES-v1.md`. A final report is allowed only after the
stage prompt's completion conditions and the gates below pass, or after a hard
blocker is recorded.
Any final report must first pass the report eligibility gate in
`ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md` Section 22.0. A green
test run, generated manifest, or partial implementation is not enough to claim
that a training run is promoted, done, or production-ready.
Elapsed time, session length, or context compaction is never evidence of done.
Long-running work must continue from state until the gates below pass or a real
blocker is recorded.
Wiki question planning is governed by
`PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`. A project wiki answer is not
accepted if the answer bypasses the trainable query-planning flow.

## External Contract

The only external entry point is an orchestration request:

```text
Document this .NET project and prepare its project-local wiki.
```

The request must go through the main orchestrator and then through the
documentation/wiki flows. A run is not accepted if it requires the user to know
or manually call internal files such as `documentation-units.jsonl`,
`evidence-index.jsonl`, `call-edges.jsonl`, or `value-flow-edges.jsonl`.

## Internal Artifacts

Internal artifacts are allowed only as snapshots, ledgers, or cache material.
They are not the source of truth for "done".

Allowed internal snapshots:

- `evidence-index.jsonl`;
- `documentation-units.jsonl`;
- `call-edges.jsonl`;
- `value-flow-edges.jsonl`;
- `negative-dto-candidates.jsonl`;
- `documentation-dpo-pairs.jsonl`;
- `wiki-query-plans.jsonl`;
- validation ledgers and evaluation reports.

These files may be persisted for repeatability, debugging, audit, or later
training. They must not be treated as the public API of the feature.

The in-memory source of truth during execution is the documentation knowledge
graph:

```text
ProjectDocumentationKnowledgeGraph
  documentation units
  call edges
  value-flow edges
  evidence records
  negative DPO bundle
  optional question seeds for Phase 2
```

## What Counts As Done

A project-local wiki run is done only when all gates below pass.

### Gate 0: Report Eligibility Gate

Before any completion claim, pass the report gate:

- identify the report scope: training runner report, implementation status,
  test result, blocker report, or plan review;
- identify the exact source of truth for the claim;
- identify the persisted `report_artifact_path`;
- verify `artifact_written_before_message == true`;
- use Section 22 runner report format only when a real training runner emitted
  the metrics;
- mark missing runner metrics as `UNKNOWN`;
- do not use generated manifests or test logs as substitutes for missing
  runner metrics;
- reject the report if CPU, RAM, accuracy, loss, reward margin, or held-out
  quality numbers are not backed by a saved JSON report artifact;
- reject the report if it contains `done`, `complete`, `ready`, `promoted`, or
  similar words outside the scope where those words are explicitly allowed.

### Gate 1: Orchestrated Request Gate

Pass requires:

- the user-level request enters through the main orchestration layer;
- the executable entry point is `ProjectDocumentationPipelineFlowAdapter`
  through `LogicalOrchestratorAssistant` or an equivalent main orchestrator
  binding;
- the request is routed to the documentation/wiki domain;
- the run produces a `ProjectDocumentationPipelineResult`;
- the result contains a populated `ProjectDocumentationKnowledgeGraph`;
- the user does not need to name internal JSONL files.

Fail examples:

- directly running a script that writes `.xiigen/wiki-domain/`;
- checking only that JSONL files exist;
- manually invoking a retriever against `evidence-index.jsonl` and calling that
  the product.

### Gate 2: Documentation Quality Gate

Every documented code unit must include:

- code symbol;
- meaning in the project;
- source location;
- accepted parameters;
- return value;
- callers;
- callees;
- for caller/workflow units, what arguments are sent to callees and what is
  done with returned values;
- stack family and language/framework version where inferable.

Pass thresholds:

```text
documentation_quality_score >= 0.95
stack_version_score == 1.00
source_evidence_score == 1.00
bottom_up_propagation_score == 1.00
hard_failure_count == 0
```

### Gate 3: Relation Graph Gate

The knowledge graph must be internally consistent:

- every evidence id points to a documentation unit;
- every call edge points to existing caller and callee units;
- every value-flow edge points to an existing call edge;
- every caller with internal callees records the callee meaning;
- every callee used by a caller records how the caller uses its return value.

Persisted JSONL files may be checked here, but only as snapshots of the graph.
The graph is the tested object.

### Gate 4: Negative DPO Gate

Negative DPO is Phase 1. It is not tied to the question database.

Pass requires:

- every negative candidate is generated from accepted documentation and source
  evidence;
- `chosen != rejected`;
- every rejected side contains a specific `[WRONG: ...]` reason;
- the wrongness is checkable against the accepted documentation graph;
- no question rows are generated by `NegativeDtoDocumentationOrchestrator`.

### Gate 5: Training Gate

Minimum "done" conditions are governed by `EXECUTOR-DISCIPLINE-RULES-v1.md`
Rule 3. Do not report a unit as ready until that rule passes: target accuracy,
stable reward margin, minimum row count, real-project verification, and
`WikiUnitTrace` status all have to pass together.

If a runner exists, it must report:

- documentation DTO accuracy;
- meaning accuracy;
- parameter accuracy;
- caller accuracy;
- callee accuracy;
- return accuracy;
- relation edge accuracy;
- negative DTO rejection accuracy;
- citation accuracy;
- hallucination rate;
- CPU per row;
- elapsed time per row;
- peak RAM;
- adapter artifact path and checksum.

If a runner is missing, the run status is:

```text
DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING
```

It must not be reported as "trained".

### Gate 5.5: Unit Training Gate

The wiki domain is accepted only when every wiki unit is either trained and
passes held-out metrics, or explicitly blocked with a named missing resource.

Required units:

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

Pass requires:

```text
unit_trace_coverage == 1.00
failed_unit_has_remediation_action == 1.00
developer_reasoning_rows >= 40% of broad wiki batches
entity_impact_question coverage >= 0.85 for mapped entities
flow_trace_question coverage >= 0.85 for mapped workflows
```

No unit may silently fall back to a deterministic placeholder and still be
called trained.

### Gate 6: Project Wiki Answer Gate

Question coverage is governed by
`PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md`. Gate 6 is not satisfied by a small
sample of easy questions. It must test the prompt-derived validation variations
for entity impact, workflow trace, change location, regression risk,
architecture boundary, missing-evidence abstention, and query-plan
orchestration. If the project wiki contains accepted SQL/stored-procedure
evidence, Gate 6 also includes the stored-procedure logic variations from that
document.

Before answer verification, every question must pass the query-planning gate
from `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`:

```text
query_plan_accuracy >= 0.90
required_touchpoint_recall >= 0.95
multi_hop_sequence_accuracy >= 0.90
missing_evidence_detection >= 0.95
irrelevant_query_rate <= 0.05
```

The accepted trace must show:

```text
question -> ProjectWikiQueryPlanningFlow -> planned evidence queries
-> executed retrieval -> answer -> verifier decision
```

For every target project, the local wiki must be tested with multiple question
types over the generated documentation graph:

- meaning;
- location;
- parameters;
- returns;
- callers;
- callees;
- workflow/transit;
- stored procedure logic when SQL/procedure evidence exists;
- why/purpose;
- multi-hop caller-callee questions;
- wrong-project or stale-version rejection.

Pass requires:

```text
all_question_types_covered == true
all_45_base_gap_validation_variations_from_PLAN_WIKI_GAP_VALIDATION_EXAMPLES == true
if_sql_procedure_evidence_exists_all_53_variations_pass == true
exact_evidence_accuracy == 1.00
cross_project_leakage == 0
stale_version_acceptance == 0
hallucination_rate == 0
```

### Gate 7: Five-Project Coverage Gate

The following projects must be checked as projects, not as isolated sample
files:

```text
C:\Projects\wizGameserver
C:\Projects\GenieDynamicServer
C:\Projects\geniesoftGameMVPServer
C:\Projects\genietasks
C:\Projects\askapediaweb
```

For each project, report:

- methods discovered;
- callgraph edges discovered;
- evidence records;
- question count;
- answered question count;
- exact evidence accuracy;
- missing relation types;
- missing method families;
- reason for every uncovered area.

If coverage does not improve after 10 batches, the next action is not "continue
training blindly". The next action is gap analysis and source/pair generation
targeting the missing relation types.

## What Must Not Be Reported As Done

Do not report success for:

- JSONL files being present;
- markdown documentation being present;
- a BM25 lookup returning some row;
- structural tests passing without training;
- one or two sample questions passing;
- synthetic accuracy reported as real model accuracy;
- a deterministic placeholder answer being called a trained local model.

## Minimum Final Report

A final report is allowed only when all gates pass or when a hard blocker is
found. It must include:

- final status;
- which gates passed;
- which gates failed;
- five-project coverage table;
- training metrics if training ran;
- explicit statement if training did not run;
- per-unit train/held-out status;
- entity-impact and flow-trace coverage;
- next missing resource if blocked.
