# EXECUTOR STAGE PROMPT v2

Status: REQUIRED SHORT PROMPT.
Read before every sub-stage.
Read before saying the task is done.
Read when unsure about the next action.

## Stage Goal

The goal is not to create files. The goal is:

```text
natural developer request
-> trained wiki units
-> project-local evidence retrieval
-> exact answer with evidence_ids from the correct project
-> WikiQueryTrace and WikiUnitTrace proving which unit succeeded or failed
```

Concrete target task:

```text
I want to add DiscountCode to CampaignCreateDto.
Where does this entity/value travel, what processes and calculations does it
affect, and where can this break?
```

Project code is evidence and validation. Generic reasoning must be trained
before project validation. Do not use project code as the main generic training
source.

## Goal Check Before Every Step

Before any action and before any "done" claim, ask:

```text
Does this action move the system closer to:
trained wiki units answering a real developer question with exact project
evidence and traceable unit behavior?
```

The answer is yes only if the action does at least one of these:

- improves a trainable unit metric;
- adds validated 50-row training data for a named weak unit;
- improves project evidence coverage used by the wiki;
- creates or verifies WikiQueryTrace/WikiUnitTrace;
- fixes a blocker that prevents the orchestrated request from running;
- updates the living guide with a proven pattern or failure mode.

The answer is no if the action only:

- creates unused files;
- improves JSONL structure without improving unit metrics or coverage;
- replaces a trainable decision with static code;
- reports completion without real runner/project validation;
- bypasses the orchestrator and calls internal files directly.

## Report Gate Before Any User-Facing Status

Before any user-facing report, final answer, or status update, classify the
report scope:

```text
training_runner_report
implementation_status
test_result
blocker_report
plan_review
```

Then read the governing report rule:

```text
training_runner_report -> ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md Section 22.0 and 22.1
implementation_status -> EXECUTOR-DISCIPLINE-RULES-v1.md Rule 9
test_result -> EXECUTOR-DISCIPLINE-RULES-v1.md Rule 9
blocker_report -> PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md relevant gate
plan_review -> ORCHESTRATION-TRAINING-PROTOCOL-v1.md Gate A / review protocol
all metric reports -> ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md Section 22.0A
```

Mandatory check:

```text
report_scope: <one of the scopes above>
source_of_truth: <runner output, test command, code path, or guide>
report_artifact_path: <absolute saved JSON path or none>
artifact_written_before_message: true | false
section_22_applicable: true | false
missing_runner_metrics: <list or none>
false_positive_risk: none | metrics_missing | wrong_scope | partial_gate | unverified_claim
allowed_claim: <exact status words allowed for this scope>
```

If `artifact_written_before_message` is false for a metric-bearing report, do
not send the metrics to the operator. Write the report artifact first or report
only `REPORT_ARTIFACT_WRITE_FAILED`.

If `false_positive_risk` is not `none`, do not say done, passed, promoted,
complete, or ready. State the blocker and continue the next valid action.

Never use a green test suite as a runner report. Tests prove implementation
behavior. Runner reports prove model-training metrics. They are different
report scopes.

Time is not a report scope and not a stop gate. If the process takes hours or
days, continue through state. If the session resumes after compaction, restore
the active state, run this report gate again, and continue from the next valid
action. Do not claim completion because the session is long.

## Placement Rule For New Units

If the unit is project-specific, place it in that project:

```text
<project>/.xiigen/wiki-domain/
load through IRemoteUnitLocator and domain-manifest.json
```

Project-specific means it understands that repository's entities, business
rules, workflows, documentation evidence, or local wiki weights.

If the unit improves the general algorithm, place it in `llm_mvp_core`:

```text
C:\Projects\llm_mvp_core\42.ProjectWikiDomain
or the matching shared capability project
```

General algorithm means entity impact reasoning, flow trace reasoning,
stack/version classification, answer verification, or trainable routing that
should work across projects.

Static code is allowed only for mechanical work:

```text
hashing, counting, JSONL validation, source file enumeration, deterministic
parsing, path normalization, metric aggregation
```

Anything requiring judgment should prefer a trainable unit.

## Genie DNA Check

Before accepting implementation, check:

1. Generic interfaces: uses `ILogicalFlow`, `ILogicalOrchestrator`,
   `IRemoteUnitLocator`, model repository contracts, or equivalent extension
   points where possible.
2. Fabric first: storage, queue, AI provider, search, and model loading are not
   hardcoded when an existing fabric/interface exists.
3. Dynamic document pattern: repositories and evidence records can carry
   dynamic fields; do not freeze project-specific schema too early.
4. Reusable reasoning: general units work across different repositories,
   languages, queues, databases, and UI stacks.

If a general unit fails these checks, redesign before calling it accepted.

## Canonical Units

Use these names exactly:

```text
1. ProjectWikiRequestRouter
2. StackAndVersionClassifier
3. CodeTraceBuilder
4. EntityImpactMapper
5. DocumentationGenerator
6. NegativeDpoGenerator
7. ProjectWikiQueryPlanningFlow
8. WikiContextRoutingUnit
9. WikiAnswerComposer
10. ProjectAnswerVerifier
11. WikiTraceAggregator
```

Source of truth: `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md`.

## Required Documents

Read these documents when the related stage is active:

```text
PROJECT-LOCAL-WIKI-MODEL-PLAN-v1.md
  Architecture and phase boundaries.

PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md
  Stages 0-13, expected outputs, trace metrics.

PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md
  Gates 1-7 and what must not be reported as done.

PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md
  Canonical units, unit thresholds, remediation actions.

PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md
  45 base validation variations, plus 8 stored-procedure variations when
  SQL/procedure evidence exists.

PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md
  How developer questions become multi-step wiki evidence-query plans before
  retrieval and answer composition.

EXECUTOR-DISCIPLINE-RULES-v1.md
  Continue/stop rules and forbidden early completion claims.

ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md
  Raw-number metrics, CPU/RAM/time reporting, below-0.75 behavior.

ORCHESTRATION-TRAINING-PROTOCOL-v1.md
  Gate A/B, ablation method, baseline comparison.

ORCHESTRATION-TRAINING-PROTOCOL-LIVING-GUIDE-v1.md
  Section 18 unit library and required protocol updates.

PROJECT-LOCAL-WIKI-MODEL-SESSION-01..06
  Concrete session inputs and outputs.
```

## Sub-Stage Map

Exact stage coverage:

```text
Stage 0 - Orchestration Intake
Stage 1 - Source Inventory And Stack/Version Detection
Stage 2 - Symbol And Method Trace
Stage 3 - Callgraph And Value-Flow Trace
Stage 4 - Bottom-Up Documentation Generation
Stage 5 - Documentation Acceptance Gate
Stage 6 - Negative DPO For Documentation
Stage 7 - 50-Row Training Batch
Stage 8 - Manifest And Model Registration
Stage 9 - Project Wiki Query Planning And Coverage Verification
Stage 10 - Ten-Batch Review
Stage 11 - Entity Impact Map
Stage 12 - Developer Reasoning Rows
Stage 13 - WikiQueryTrace And WikiUnitTrace
```

### Stage 0 - Orchestration Intake

Goal: a natural request enters through the orchestrator, not through a script.

Read:

- `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md` Gate 1.
- `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md` Stage 0.
- `PROJECT-LOCAL-WIKI-MODEL-SESSION-02-SCAFFOLD-AND-INGEST.md` Required Entry Point.

Dilemmas:

- Direct script call instead of orchestrator: fail Gate 1.
- No source consent: record blocker and stop.
- Manual JSONL shortcut: wrong path.

### Stage 1 - Source Inventory And Stack/Version Detection

Goal: enumerate the real project source surface and record stack, framework,
C# version, UI technology, storage, generated/excluded paths, and source
authority.

Read:

- `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md` Stage 1.
- `PROJECT-LOCAL-WIKI-MODEL-SESSION-02-SCAFFOLD-AND-INGEST.md`.
- `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md` Unit 2.

Dilemmas:

- Multiple .NET versions: record per project/subproject; do not average.
- Source consent missing: block, do not ingest.
- `bin`, `obj`, generated files: exclude unless explicitly required.

### Stage 2 - Symbol And Method Trace

Goal: discover symbols, methods, constructors, properties, parameters, returns,
entry points, and leaf functions. Leaf functions matter because callers inherit
their meaning.

Read:

- `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md` Stage 2.
- `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md` Unit 3.
- `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md` Gate 3.

Dilemmas:

- Leaf methods seem boring: still trace them.
- Partial parse: record unresolved reason; do not pretend completeness.
- Legacy WebForms method shape: preserve event-handler context.

### Stage 3 - Callgraph And Value-Flow Trace

Goal: connect callers to callees and record what arguments are passed, what
values return, and what the caller does with returned values.

Read:

- `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md` Stage 3.
- `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md` Unit 3.
- `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md` Gate 3.

Dilemmas:

- Reflection/DI call unresolved: mark unresolved; do not invent.
- Returned value has no value-flow edge: trace is incomplete.
- No callee: state explicit no-callee reason.

### Stage 4 - Bottom-Up Documentation Generation

Goal: document from leaf/internal functions upward so callee meaning is passed
to callers like compiler information.

Read:

- `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md` Stage 4.
- `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md` Unit 5.
- `PROJECT-LOCAL-WIKI-MODEL-SESSION-02-SCAFFOLD-AND-INGEST.md`.

Dilemmas:

- Caller doc omits callee meaning: not accepted.
- Parameters are listed but meaning is absent: incomplete.
- Stack/version omitted: incomplete for legacy .NET projects.

### Stage 5 - Documentation Acceptance Gate

Goal: accept only documentation that satisfies the quality gate and can be used
as evidence for training and wiki answers.

Read:

- `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md` Stage 5.
- `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md` Gate 2.
- `PROJECT-LOCAL-WIKI-MODEL-SESSION-02-SCAFFOLD-AND-INGEST.md`.

Dilemmas:

- Documentation quality 0.94 when gate is 0.95: not accepted.
- Generated docs exist but no acceptance gate: not evidence.
- Evidence id missing: do not train from it.

### Stage 6 - Negative DPO For Documentation

Goal: generate plausible but provably wrong rejected sides from accepted
documentation and trace evidence. This is Phase 1. It is not the question bank.

Read:

- `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md` Stage 6.
- `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md` Gate 4.
- `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md` Unit 6.
- `PROJECT-LOCAL-WIKI-MODEL-SESSION-03-RELATION-GRAPH-AND-NEGATIVE-DTO.md`.

Dilemmas:

- Rejected side is generic or trivially negated: reject the row.
- Rejected side might be valid: reject the row.
- Negative generator creates questions: wrong phase.

### Stage 7 - 50-Row Training Batch

Goal: train in validated 50-row batches. 50 is the minimum work unit, not a
resting point.

Read:

- `ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md` Sections 22-24.
- `EXECUTOR-DISCIPLINE-RULES-v1.md` Rules 2-6.
- `ORCHESTRATION-TRAINING-PROTOCOL-v1.md` Gate A/B.

Dilemmas:

- 47 rows ready: not a 50-row batch.
- Standalone passed: run combo without asking.
- Combo passed: promote and source next batch without asking.
- Accuracy below 0.75: source more data and continue; do not wait.

### Stage 8 - Manifest And Model Registration

Goal: publish accepted weights through model repository infrastructure and
register the trained unit without recreating the manifest from scratch.

Read:

- `PROJECT-LOCAL-WIKI-MODEL-SESSION-04-TRAIN-DOCUMENTATION-WIKI.md`.
- `PROJECT-LOCAL-WIKI-MODEL-SESSION-06-QA-EVALUATE-REGISTER.md`.
- `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md` Gate 5.

Dilemmas:

- Runner missing: status is explicit runner-missing, not trained.
- Manifest exists: update it; do not recreate from scratch.
- Local project unit: store in project repo and load through `IRemoteUnitLocator`.

### Stage 9 - Project Wiki Query Planning And Coverage Verification

Goal: turn each developer question into a multi-step evidence query plan, then
test answers against retrieved project evidence rather than structure alone.

Read:

- `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md` Stage 9.
- `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md` Gate 6.
- `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md`.
- `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`.
- `PROJECT-LOCAL-WIKI-MODEL-SESSION-06-QA-EVALUATE-REGISTER.md`.

Dilemmas:

- Answer was produced without `ProjectWikiQueryPlanningFlow`: fail.
- Query plan is one lookup for an entity-impact question: fail.
- Query plan is one lookup for a stored-procedure question: fail.
- One or two sample questions pass: not enough.
- 30/45 base validation variations pass: not enough.
- 45/53 variations pass when SQL/procedure evidence exists: not enough.
- Answer has no exact evidence ids: fail.
- Wrong project or stale version accepted: fail.

### Stage 10 - Ten-Batch Review

Goal: after every 10 batches, compare coverage and metrics. If there is no
improvement, analyze gaps instead of blindly adding rows.

Read:

- `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md` Stage 10.
- `ORCHESTRATION-TRAINING-PROTOCOL-RUNNER-REPORTING-v1.md` Section 25.
- `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md` remediation actions.

Dilemmas:

- Coverage improved: continue weakest unit.
- Coverage flat: `STOP_BLIND_TRAINING_AND_ANALYZE_GAPS`.
- CPU/RAM/time missing: instrument runner before claiming cost.

### Stage 11 - Entity Impact Map

Goal: build a touch point map for each entity: input, validation, mapping,
service/calculation, persistence, serialization/output, tests, and unknowns.

Read:

- `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md` Stage 11.
- `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md` Unit 4.
- `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md` Gap 1.

Dilemmas:

- Map only shows DTO and controller: incomplete.
- Dynamic dispatch causes partial trace: mark partial; do not hide.
- Small entity: still map it.

### Stage 12 - Developer Reasoning Rows

Goal: generate developer-task rows from trace/evidence patterns, not method
description rows. At least 40 percent of broad wiki batches must be developer
reasoning rows.

Read:

- `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md` Stage 12.
- `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md` all gaps and prompt variations.
- `PROJECT-WIKI-QUERY-PLANNING-FLOW-PROTOCOL-v1.md`.
- `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md` Unit 4, Unit 7, and Unit 9.

Dilemmas:

- A-side says "what does method X do": not developer reasoning.
- Chosen omits service/repository/serializer/test impact: incomplete.
- Not enough rows: source another 50; do not lower the standard.

### Stage 13 - WikiQueryTrace And WikiUnitTrace

Goal: every question produces `WikiQueryTrace`; every batch produces
`WikiUnitTrace`; every failed unit has a concrete remediation action.

Read:

- `PROJECT-LOCAL-WIKI-CODE-TRACE-TRAINING-PROTOCOL-v1.md` Stage 13.
- `PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md` trace sections.
- `PROJECT-LOCAL-WIKI-DONE-PROTOCOL-v1.md` Gate 5.5.
- `PROJECT-LOCAL-WIKI-MODEL-SESSION-06-QA-EVALUATE-REGISTER.md` Stop Gate.

Dilemmas:

- Trace status says "noted": invalid. Use PASS, FAIL, CONCERN, or BLOCKED.
- Unit failed without next batch/remediation: not accepted.
- Missing trace but answer looked good: not accepted.

## When More Data Is Needed

"Need more data" is an action, not a pause.

Source order:

1. XIIGen first-party sessions and prompts.
2. XIIGen planning, flow, skill, and registry documents.
3. Permissive external sources from `PLAN-WIKI-GAP-VALIDATION-EXAMPLES-v1.md`.
4. CC-BY sources only with attribution recorded.
5. Quarantine sources only with explicit approval.

Do not use Stack Overflow, FEVER, n8n, Wikipedia, MIT OCW, commercial books, or
company blogs as default commercial training rows.

Only ask the operator when:

- Gate A/B failed and ablation cannot identify rows;
- all approved sources are exhausted and 50 validated rows cannot be built;
- a new row shape appears and no guide classifies it.

## Before Claiming Done

Read this prompt again. Then verify:

```text
1. The orchestrated entry point was used.
2. All relevant stages 0-13 passed or have explicit blockers.
3. Each canonical unit met its target from PLAN-WIKI-TRAINING-GAP-CLOSURE-v1.md
   or has a named blocker and remediation action.
4. Reward margin >= 0.02 for two combo runs where required.
5. At least the minimum meaningful data volume exists; 200 rows is first signal,
   not automatic promotion.
6. All 45 base gap validation variations pass for the claimed scope.
   If SQL/procedure evidence exists, all 53 variations pass.
7. WikiQueryTrace exists for every evaluated question.
8. WikiUnitTrace exists for every batch.
9. Project-specific units live in the project repo and load through
   IRemoteUnitLocator.
10. General algorithm units live in llm_mvp_core or the correct shared
    capability project.
11. The implementation passes the Genie DNA check.
12. The final report uses raw runner numbers, not decorative metrics.
13. The report gate was run:
    report_scope is correct;
    Section 22 was used only for actual runner output;
    missing runner metrics are marked UNKNOWN;
    no false-positive completion claim is present.
14. No stop or completion claim is based on elapsed time, session length,
    context compaction, or convenience. Long work continues through state.
15. The metric report artifact was written before the user-facing report:
    report_artifact_path exists;
    artifact_written_before_message is true;
    CPU/RAM/accuracy numbers come from that saved artifact.
```

If any answer is no, do not say done. Continue or record the blocker.
