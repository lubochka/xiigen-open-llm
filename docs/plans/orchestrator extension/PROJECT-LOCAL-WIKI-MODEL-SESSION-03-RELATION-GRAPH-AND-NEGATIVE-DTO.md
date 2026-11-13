# SESSION-03 - Relation Graph And Negative Documentation DTO

Status: EXECUTION SESSION.

Do not start this session until SESSION-02 has passed its stop gate.

This is Phase 1. It builds the documentation relation system and the
documentation-negative training surface. It does not create questions.

## Goal

Turn accepted documentation into machine-readable relation rows, then generate
and verify plausible negative documentation DTOs for local documentation wiki
training.

## Required Reads

1. `PROJECT-LOCAL-WIKI-MODEL-PLAN-v1.md`
2. `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md`
3. `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md`
4. SESSION-02 `ProjectCodeTrace`
5. SESSION-02 `ProjectDocumentationKnowledgeGraph`
6. Selected project's `.xiigen/wiki-domain/domain-manifest.json`
7. Selected project's `.xiigen/wiki-domain/evidence/evidence-index.jsonl`
8. Accepted documentation under `.xiigen/wiki-domain/documentation/`
9. Selected project source files referenced by the evidence index
10. `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md`
11. `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`
12. `EXECUTOR-DISCIPLINE-RULES-v1.md`
13. `EXECUTOR-STAGE-PROMPT-v2.md`

## Documentation Relation Contract

Every accepted documentation unit must record:

- code symbol;
- meaning in the project;
- source location;
- accepted parameters and parameter meaning;
- callers;
- return type and return meaning;
- evidence ids.

Every caller/component/workflow unit must additionally record:

- called functions;
- callee meaning;
- arguments sent to each callee;
- where each sent value came from;
- return values received from each callee;
- what the caller does with each returned value.

## Entity Impact Map Contract

For every entity/DTO/class with fields or properties, build a touch point map:

- who accepts it;
- who validates it;
- who maps or converts it;
- who stores it;
- who serializes or returns it;
- who reads each field;
- which calculations/processes depend on fields;
- which tests cover it;
- which parts are unresolved.

Partial maps are allowed only when the missing path is labeled.

## Outputs

The primary output is the updated `ProjectCodeTrace` and
`ProjectDocumentationKnowledgeGraph` with relation edges and negative DPO
attached. The files below are persisted snapshots and training inputs.

```text
.xiigen/wiki-domain/relations/documentation-units.jsonl
.xiigen/wiki-domain/relations/call-edges.jsonl
.xiigen/wiki-domain/relations/value-flow-edges.jsonl
.xiigen/wiki-domain/training/batch-001/documentation-train-50.jsonl
.xiigen/wiki-domain/heldout/batch-001/documentation-heldout-50.jsonl
.xiigen/wiki-domain/training/batch-001/negative-dto-candidates.jsonl
.xiigen/wiki-domain/training/batch-001/negative-dto-validation-ledger.jsonl
.xiigen/wiki-domain/training/batch-001/negative-dto-repair-required.jsonl
.xiigen/wiki-domain/eval/relation-graph-validation.json
```

## Negative DTO Generation Rule

Use `NegativeDtoDocumentationOrchestrator` only for documentation negatives.
It may create wrong documentation DTOs such as:

- wrong symbol meaning;
- wrong caller;
- missing callee;
- wrong argument mapping;
- wrong return-value usage;
- wrong framework/version;
- invented side effect;
- stale documentation.

It must not generate questions and must not write question-bank rows.

## Developer Reasoning Row Rule

This is still Phase 1. Developer reasoning rows are not the Phase 2 question
database. They are DPO training rows that teach the wiki units how developers
ask about changes.

For every entity touch point map, generate rows such as:

- `entity_impact_question`;
- `flow_trace_question`;
- `change_location_question`;
- `impact_analysis_question`;
- `regression_risk_question`.

The A side is a developer task. For documentation and impact rows, the chosen
side must include the grounded path through the project. For query-planning
rows, the chosen side must include the multi-step evidence investigation plan
before any answer. The rejected side must be plausible, incomplete or wrong,
and must include `[WRONG: ...]`.

Every accepted negative DTO must include a specific `[WRONG: ...]` annotation
that states the defect and the consequence.

## Row Gate

- exactly 50 train rows;
- 50 documentation held-out rows;
- all train and held-out rows are disjoint;
- project code trace is updated with targeted gap types for this batch;
- developer reasoning rows are at least 40% of broad wiki batches;
- entity impact rows are present when entities exist;
- all evidence ids exist;
- relation graph rows exist for every row;
- every caller row has at least one call edge or an explicit no-callee reason;
- every call edge with a returned value has a value-flow edge;
- every accepted negative DTO is provably wrong against accepted documentation;
- no question rows are produced.

## Stop Gate

Stop after relation graph validation and negative DTO validation pass on the
trace/graph, not merely after files are written.

Do not train the adapter in this session.
