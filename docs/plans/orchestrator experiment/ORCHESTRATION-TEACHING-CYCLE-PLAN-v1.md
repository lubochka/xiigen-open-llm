# Orchestration Teaching Cycle Plan
# Version: 1.0 - 2026-05-05

---

## Purpose

This document defines the first real teaching cycle for the dynamic flow.

The target is not a single node and not a code generator. The target is the
orchestrator's ability to manage a real software-development request across
domains, phases, sub-phases, transitions, flows, arbiters, and sub-tasks.

The first learned unit is the orchestration prompt splitter.

---

## Why The Prompt Splitter Comes First

A security arbiter can be perfectly accurate and the orchestrator can still
fail if the original prompt was split into the wrong work areas.

The user prompt is the first boundary. It decides whether the machine sees:

```text
one security task
or
a lifecycle request spanning architecture, contracts, implementation, tests,
QA, and automation.
```

Therefore the first learning cycle teaches the machine to produce the lifecycle
split graph.

---

## Unit Under Training

### Role

```text
orchestration-prompt-splitter
```

### Input

```text
LogicalUnitRequest<OrchestrationPromptInput>
```

### Input shape

```text
run_id:
user_prompt:
available_context_documents:
known_topology:
source_authority_rows:
default_flags:
```

### Output

```text
LogicalUnitResult<OrchestrationPromptSplit>
```

### Output shape

```text
selected_domains:
selected_phases:
selected_sub_phases:
parallel_groups:
sequential_dependencies:
transition_points:
context_items_to_carry_forward:
conflicts:
knowledge_edges:
required_arbiters:
first_sub_tasks:
confidence_by_branch:
evidence_ids:
warnings:
```

---

## Model Choice For The First Cycle

The first model is KNN over predicate and graph features.

The reason is practical:

```text
it is small enough for this PC;
it is inspectable;
it can be retrained quickly;
it explains decisions through nearest neighbors;
it does not require GPU training;
it fits the existing predicate-vector architecture.
```

The model file is JSON. It stores training vectors, graph labels, source row
ids, and output labels. It is not hidden inside a binary checkpoint.

---

## Feature Vector

The splitter feature vector combines four groups.

### Predicate features

The existing predicate vocabulary:

```text
VALIDATE_OP
CREATE_OP
GET_OP
UPDATE_OP
DELETE_OP
AUTH_OP
SEND_OP
CALL
BRANCH
FAIL
RETURN
ITERATE
PROCESS_OP
HANDLE_ERROR
LOG_OP
CONVERT_OP
DECLARE
TYPE_CHECK
and the remaining local predicate slots
```

### Lifecycle vocabulary features

Signals for:

```text
phase names
sub-phase names
source types
artifact types
test evidence
review evidence
automation evidence
security evidence
performance evidence
accessibility evidence
```

### Graph features

Signals for:

```text
sequential dependency
parallel branch
transition requirement
missing context
conflict
knowledge edge
arbiter required
release blocking behavior
```

### Source authority features

Signals for:

```text
local XIIGen source
external license-checked source
source gap
corruption case
held-out prompt
```

---

## Training Data For Cycle 1

Cycle 1 uses O1: multi-tenant authentication addition.

Training rows:

```text
O1 original prompt
O1 Phrasing B prompt
O1 compact prompt
O1 long developer-session prompt
O1 missing-source prompt
O1 contradiction prompt
O1 safety-boundary prompt
```

The output labels are not a single class. They are the expected lifecycle graph.

Expected selected sub-phases:

```text
3.1 architecture and component topology
3.3 API contract definition
3.4 security and authorization model
4.1 backend services
4.3 integration and provider connectors
5.1 unit and contract tests
5.2 integration tests
6.3 security and compliance review
7.3 CI/CD gate configuration
7.4 coverage and reporting
```

Expected transitions:

```text
3.1 -> 3.3
3.3 -> 3.4
3.4 -> 4.1
4.1 -> 4.3
4.3 -> 5.2
5.2 -> 6.3
6.3 -> 7.3
7.3 -> 7.4
```

Expected arbiters:

```text
identity consistency arbiter
tenant isolation arbiter
token safety arbiter
source authority arbiter
security evidence arbiter
release gate arbiter
```

---

## Cycle Steps

### Step 1 - Build deterministic reference split

Produce the reference O1 split graph by hand from the accepted source documents.
This reference is the comparison object for the first training cycle.

### Step 2 - Train splitter v1

Read the O1 teaching rows, extract features, build the KNN model, write the
model JSON, and record teaching metrics.

### Step 3 - Run O1 original and Phrasing B

Run the learned splitter on the two prompt rows that are closest to training.
Record process metrics and split quality.

### Step 4 - Run held-out prompts

Run compact, long, missing-source, contradiction, and safety-boundary variants.
Record what the learned splitter missed.

### Step 5 - Add one corrective teaching row

Choose one held-out miss. Add exactly one teaching row that repairs it.

### Step 6 - Retrain splitter v2

Retrain with the new row included. Measure teaching time and model file growth.

### Step 7 - Re-run all O1 prompts

Confirm that the missed case improved and original behavior did not regress.

### Step 8 - Promote evidence into the orchestration corpus

Write the cycle evidence, resource metrics, model summary, and corpus delta.

---

## Quality Measurements

The splitter is measured by relationships, not exact wording.

| Measurement | Meaning |
|---|---|
| selected_sub_phase_recall | How many required sub-phases were selected. |
| selected_sub_phase_precision | How many selected sub-phases were truly relevant. |
| transition_recall | How many required transition edges were present. |
| dependency_violation_count | How many selected steps were ordered too early. |
| context_loss_count | How many upstream constraints disappeared. |
| arbiter_missing_count | How many required arbiters were not selected. |
| knowledge_edge_invention_count | How many unknowns were converted into invented answers. |
| rejected_item_reopened_count | How many rejected branches were allowed back into accepted work. |

---

## Resource Measurements

Every teaching run and prompt run writes a resource record.

```text
teaching_elapsed_ms
feature_extraction_elapsed_ms
model_build_elapsed_ms
model_write_elapsed_ms
model_bytes
prompt_process_elapsed_ms
cpu_ms
working_set_mb_start
working_set_mb_peak
private_memory_mb_peak
managed_allocated_mb
disk_read_mb
disk_write_mb
```

The purpose is to prove that learning remains practical on the actual machine,
not on an ideal workstation.

---

## Evidence Files

Cycle 1 produces:

```text
O1-ORCHESTRATION-PROMPT-SPLITTER-TRAINING-ROWS-v1.md
O1-ORCHESTRATION-PROMPT-SPLIT-REFERENCE-v1.md
O1-SPLITTER-TRAINING-METRICS-v1.json
O1-SPLITTER-HELDOUT-RESULTS-v1.json
O1-SPLITTER-RETRAINING-METRICS-v2.json
O1-SPLITTER-CYCLE-SUMMARY-v1.md
round-phase-115-orchestration-learning-plan-rewrite-2026-05-05.zip
```

---

## After Cycle 1

The next cycles train:

```text
phase-sequencing-unit
context-carry-forward-unit
transition-verifier-unit
arbiter-selection-unit
sub-task-planner-unit
human-result-formatter-unit
```

Only after those orchestration units have evidence should the project return to
individual node optimization.

---

## Last Updated

2026-05-05. This plan starts real learning at the orchestration boundary.
