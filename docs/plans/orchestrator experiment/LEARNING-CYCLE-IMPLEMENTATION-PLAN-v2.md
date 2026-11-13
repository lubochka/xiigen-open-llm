# Learning Cycle Implementation Plan
# Orchestration-first version
# Version: 2.0 - 2026-05-05
# Supersedes: LEARNING-CYCLE-IMPLEMENTATION-PLAN-v1.md

---

## Main Correction From v1

Version 1 started learning at node-08, the security arbiter.

That is too low in the architecture for the current goal.

The current goal is to prove that the dynamic flow can manage a whole software
development request. Therefore learning starts at the orchestration boundary:

```text
orchestration-prompt-splitter
phase-sequencing-unit
context-carry-forward-unit
transition-verifier-unit
arbiter-selection-unit
sub-task-planner-unit
human-result-formatter-unit
```

Node-level training comes after the orchestration units have evidence.

---

## What "Trained" Means Here

A trained orchestration unit is one whose output is produced by a learned model
over predicate and graph features.

It must not be a hardcoded scenario switch.

It must produce real confidence values based on training neighbors or another
measurable model score.

It must write resource evidence for teaching time, prompt processing time, CPU,
RAM, allocation, and model size.

---

## First Unit

```text
orchestration-prompt-splitter
```

This unit receives a user prompt and produces the lifecycle split graph.

The first scenario is O1 multi-tenant authentication addition.

---

## Implementation Shape

### Training command

Create an explicit offline training command.

```text
dotnet run --project LlmInspector.Training -- train-orchestration-splitter --scenario O1
```

The command:

```text
reads O1 teaching rows;
extracts predicate and graph features;
fits a small KNN model;
writes JSON model file;
writes metrics JSON;
writes human summary markdown.
```

### Runtime unit

The runtime unit:

```text
loads the JSON model;
extracts features from a prompt;
scores nearest neighbors;
returns selected domains, phases, sub-phases, dependencies, transitions, arbiters, and warnings.
```

If the model file is absent, the unit returns a knowledge-edge result with
`MODEL_NOT_TRAINED`. It does not throw and does not silently use the
deterministic fixture.

---

## Model File Shape

```json
{
  "model_id": "orchestration-prompt-splitter-O1-v1",
  "unit_role": "orchestration-prompt-splitter",
  "scenario_id": "O1",
  "created_at_utc": "2026-05-05T00:00:00Z",
  "k": 5,
  "distance_metric": "cosine",
  "feature_schema_version": "orchestration-graph-v1",
  "training_rows": [
    {
      "row_id": "O1-TRAIN-001",
      "source_state": "source_backed",
      "feature_vector": [],
      "output_graph": {},
      "evidence_ids": []
    }
  ]
}
```

The model remains inspectable. A reviewer can see which rows trained it.

---

## Cycle 1 - O1 Prompt Splitter

1. Build O1 teaching rows.
2. Build the deterministic O1 reference split.
3. Train splitter v1.
4. Run original and Phrasing B prompts.
5. Run held-out prompt variants.
6. Add one correction row for a missed held-out case.
7. Retrain splitter v2.
8. Confirm the missed case improves and original cases remain stable.
9. Zip the evidence.

---

## Cycle 2 - O1 Phase Sequencing

Train the phase sequencing unit using the accepted splitter outputs.

The unit learns:

```text
which branches can run in parallel;
which branches must wait;
which phase transitions carry context;
which work cannot start until an upstream decision exists.
```

---

## Cycle 3 - O1 Context Carry-Forward

Train the context carry-forward unit.

The unit learns to carry:

```text
tenant isolation constraints;
token validation constraints;
duplicate identity rejection;
source authority markers;
knowledge edges;
release blocking rules.
```

---

## Cycle 4 - O1 Transition Verification

Train the transition verifier.

The unit learns to detect:

```text
implementation ignores security model;
tests omit cross-tenant failure paths;
QA accepts missing evidence;
automation gate does not block unsafe release.
```

---

## Cycle 5 - O1 Arbiter Selection And Sub-Tasks

Train arbiter selection and sub-task planning.

The unit learns:

```text
which arbiter has authority at each branch;
which sub-tasks are actionable;
which sub-tasks are blocked;
which human decisions are needed.
```

---

## Cycles 6 Through 9 - O2 Through O5

Repeat the same cycle for:

```text
O2 wave-gated flow registration;
O3 performance regression investigation;
O4 external source integration under license constraint;
O5 skill advisor full lifecycle.
```

O5 is the final lifecycle scenario and covers all seven phases and all
thirty-one sub-phases.

---

## Metrics Required In Every Cycle

```text
teaching_elapsed_ms;
feature_extraction_elapsed_ms;
model_build_elapsed_ms;
model_load_elapsed_ms;
prompt_process_elapsed_ms;
cpu_ms_delta;
working_set_mb_peak;
private_memory_mb_peak;
managed_allocated_mb_delta;
disk_read_mb_delta;
disk_write_mb_delta;
model_file_bytes;
selected_sub_phase_recall;
selected_sub_phase_precision;
transition_recall;
dependency_violation_count;
context_loss_count;
arbiter_missing_count;
knowledge_edge_invention_count;
rejected_item_reopened_count;
```

---

## Relationship To Node-Level Training

After the orchestration units have evidence, node-level learned units become
the next layer:

```text
node-01 domain splitter;
node-02 domain translator;
node-03 scheme planner;
node-04 processor;
node-05 applier;
node-06 result translator;
node-07 arbiter selector;
node-08 arbiter;
node-09 formatter.
```

Node-level accuracy improves local behavior. Orchestration-level accuracy
decides whether the machine is managing the right problem.

---

## Last Updated

2026-05-05.
