# PROJECT WIKI QUERY PLANNING FLOW PROTOCOL v1

Status: REQUIRED ARCHITECTURE PROTOCOL.
Language: English.

## Purpose

The local wiki must not answer a developer question by doing one direct lookup.
Before retrieval and answer generation, a trainable orchestration flow must
translate the user question into an investigation plan.

The missing unit is:

```text
ProjectWikiQueryPlanningFlow
  : ILogicalFlow<ProjectWikiUserQuestion, ProjectWikiQueryPlan>
```

Its job is to ask:

```text
What would I search for if I had to answer this question?
What evidence would prove the answer?
How should retrieved evidence be analyzed before composing the answer?
What evidence is missing, stale, wrong-project, or insufficient?
```

This is an orchestration task, not a retriever task and not an answer-writing
task.

## Position In The Wiki Pipeline

Required sequence:

```text
developer question
  -> ProjectWikiRequestRouter
  -> ProjectWikiQueryPlanningFlow
  -> WikiContextRetriever
  -> WikiAnswerComposer / ProjectLocalWikiModelUnit
  -> ProjectAnswerVerifier
  -> WikiTraceAggregator
```

`ProjectWikiRequestRouter` decides whether the question belongs to a project
wiki and which project. `ProjectWikiQueryPlanningFlow` decides which evidence
must be searched and analyzed. `WikiContextRetriever` executes the planned
retrieval steps. The answer unit must not bypass the query plan.

## Input Contract

```text
ProjectWikiUserQuestion
  question_id
  project_id
  repository_root
  documentation_version
  raw_user_text
  detected_stack
  detected_language_version
  known_project_aliases
  prior_context_ids
```

## Output Contract

```text
ProjectWikiQueryPlan
  plan_id
  project_id
  intent
  entities
  symbols
  required_query_steps[]
  required_evidence_types[]
  sufficiency_checks[]
  expected_answer_shape
  missing_evidence_policy
  wrong_project_policy
  stale_version_policy
  confidence_before_retrieval
```

Each `required_query_step` must include:

```text
step_id
step_type
query_text
target_index_or_graph
depends_on_step_ids
why_needed
evidence_expected
failure_if_missing
```

## Required Intents

The planner must support at least these developer intents:

```text
simple_symbol_lookup
entity_impact
field_addition
field_rename
workflow_trace
change_location
regression_risk
architecture_boundary
stored_procedure_impact
version_or_stack_specific_behavior
missing_evidence_abstention
wrong_project_rejection
stale_documentation_rejection
```

## Required Query Step Types

```text
symbol_lookup
location_lookup
parameter_lookup
return_lookup
caller_lookup
callee_lookup
value_flow_lookup
entity_definition_lookup
dto_lookup
mapper_lookup
validator_lookup
persistence_lookup
stored_procedure_lookup
sql_parameter_lookup
sql_table_dependency_lookup
sql_result_shape_lookup
sql_transaction_lookup
sql_side_effect_lookup
projection_or_serializer_lookup
api_contract_lookup
ui_binding_lookup
workflow_entry_lookup
workflow_branch_lookup
authorization_lookup
configuration_lookup
test_lookup
missing_evidence_check
wrong_project_check
stale_version_check
```

## Sufficiency Rules

For `entity_impact` or `field_addition`, the plan is insufficient unless it
searches for:

```text
entity/DTO definition
all field readers and writers
validators
mappers/projections/serializers
service or workflow entry points
persistence/storage path
API or UI boundary where the value enters/exits
callers and callees that transform or depend on the value
tests or missing-test evidence
```

For `workflow_trace`, the plan is insufficient unless it searches for:

```text
entry point
ordered steps
branch conditions
data passed between steps
external boundaries
outputs or side effects
failure/error paths
```

For `change_location`, the plan is insufficient unless it identifies:

```text
minimal edit location
nearby dependent locations
tests to update
places that must not be changed
evidence for why the location is correct
```

For `stored_procedure_impact`, the plan is insufficient unless it searches for:

```text
stored procedure definition and schema
SQL input/output/default parameters
tables read and tables written
result set columns or return code
transaction, locking, temp-table, and error behavior
SQL side effects such as audit rows, queues, and status changes
.NET callers that execute the procedure
argument value flow from .NET into SQL
result value flow from SQL back into .NET mappings and outputs
tests or missing-test evidence
explicit missing evidence
```

## DPO Pair Pattern

Chosen side must contain a multi-step query plan, not a final answer.

Example A-side:

```text
I want to add DiscountCode to CampaignCreateDto.
Where does this value travel, what processes and calculations does it affect,
and where can this break?
```

Chosen B-side:

```text
intent: field_addition / entity_impact
entities: CampaignCreateDto, Campaign, DiscountCode
required_query_steps:
  1. dto_lookup: find CampaignCreateDto and existing fields.
  2. mapper_lookup: find mappings from CampaignCreateDto to Campaign/entity.
  3. validator_lookup: find validation rules for CampaignCreateDto/Campaign.
  4. caller_lookup: find controllers/handlers that accept CampaignCreateDto.
  5. persistence_lookup: find repository/ORM/storage path for Campaign.
  6. projection_or_serializer_lookup: find response DTOs and API output paths.
  7. workflow_branch_lookup: find campaign creation/update workflows and
     calculations that may use campaign fields.
  8. test_lookup: find tests covering campaign creation, validation, mapping,
     persistence, and response.
  9. missing_evidence_check: if any required path is absent, answer with partial
     trace and explicit missing evidence.
expected_answer_shape: impact path, required edits, affected processes,
breakage risks, evidence_ids, missing evidence.
```

Rejected B-side:

```text
Search for CampaignCreateDto and answer from the first matching documentation
unit.
[WRONG: single lookup cannot answer entity impact because it misses mapper,
validation, persistence, callers, workflow calculations, response projection,
tests, and missing-evidence checks.]
```

Stored procedure DPO rows use the same shape, but the chosen side must include
SQL evidence families and .NET caller/value-flow evidence.

Accepted rejected types:

```text
procedure_name_only
missing_sql_parameters
missing_result_shape
missing_table_side_effects
missing_dotnet_callers
```

Example rejected side:

```text
Search for dbo.GetOrderTotals and answer from the first matching stored
procedure documentation unit.
[WRONG: procedure_name_only cannot explain stored procedure logic because it
misses SQL parameters, tables read and written, result shape, transaction
behavior, side effects, .NET callers, value flow, tests, and missing evidence.]
```

## Metrics

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

`answer_readiness_score` means the produced query plan gives the answer unit
enough evidence categories to compose a correct, cited answer without guessing.

## Trace Requirements

Every `WikiQueryTrace` must record:

```text
query_planning_flow_invoked: true
query_plan_id
planned_intent
planned_entities
planned_query_steps
executed_query_steps
skipped_query_steps
missing_evidence_from_plan
plan_sufficiency_score
answer_readiness_score
```

If `query_planning_flow_invoked` is false for a developer question, the run
fails the wiki answer gate.

## Remediation

If query planning quality is low:

- add 50-row batches for `field_addition`, `field_rename`,
  `workflow_trace`, `change_location`, and `stored_procedure_impact`;
- build negatives that perform only one symbol lookup;
- build negatives that retrieve the correct entity but omit mapper,
  persistence, callers, or tests;
- build stored-procedure negatives that name the procedure but omit SQL
  parameters, result shape, table side effects, or .NET callers;
- build negatives that answer before retrieval is sufficient;
- use project code only as validation evidence unless the source is explicitly
  authorized as training data.

## Done Rule

No local wiki answer may be considered accepted until the query plan passed
its metrics and the answer can be traced from:

```text
user question -> ProjectWikiQueryPlanningFlow -> executed evidence queries
-> answer -> verifier decision
```
