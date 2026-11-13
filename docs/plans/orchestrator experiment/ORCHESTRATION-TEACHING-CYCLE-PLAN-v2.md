# Orchestration Teaching Cycle Plan
# Version: 2.0 - 2026-05-05
# Supersedes: ORCHESTRATION-TEACHING-CYCLE-PLAN-v1.md

---

## Purpose

This document defines the first real teaching cycle for the dynamic flow.

Version 2 corrects the first learned unit.

The first learned unit is not the prompt splitter.

The first learned unit is:

```text
logical-transition-normalizer
```

The prompt splitter comes second.

---

## Why Transition Normalization Comes First

The human prompt is broad, noisy, and often emotional or conversational.

An ILogicalUnit should not work on that broad wording. It should work on a
functional principle:

```text
A -> B with transformation X
```

The transition normalizer converts the prompt into that shape. Then the splitter
divides the transition into expertise areas and task branches.

---

## Cycle 1 Unit

### Role

```text
logical-transition-normalizer
```

### Input

```text
LogicalUnitRequest<HumanPromptInput>
```

### Output

```text
LogicalUnitResult<LogicalTransitionRequest>
```

### Output fields

```text
source_state_A;
target_state_B;
transformation_intent_X;
constraints;
unknowns;
rejected_intents;
allowed_scope;
evidence_ids;
confidence;
warnings;
```

---

## Cycle 1 Scenario

O1 multi-tenant authentication addition.

### Human prompt

```text
Add multi-tenant authentication to the API, preserve internal-first execution,
avoid duplicate identity creation, enforce token and tenant isolation, test it,
review it for security, and add automation so an unsafe release cannot pass.
```

### Expected transition object

```yaml
source_state_A:
  - "API authentication lifecycle does not yet have the requested multi-tenant
     orchestration evidence."
target_state_B:
  - "API authentication lifecycle enforces tenant and token isolation through
     design, implementation, tests, QA review, and release automation."
transformation_intent_X:
  - "design identity and security boundaries"
  - "implement backend and provider enforcement"
  - "test tenant and token isolation"
  - "review security evidence"
  - "automate release blocking"
constraints:
  - "internal-first execution"
  - "duplicate identity creation rejected"
  - "missing tenant source remains a knowledge edge"
unknowns:
  - "project-specific tenant model if no source document provides it"
rejected_intents:
  - "create second identity for existing account"
allowed_scope:
  - "orchestration plan and sub-task production before code implementation"
```

---

## Cycle 1 Steps

1. Build O1 transition-normalizer teaching rows.
2. Train transition normalizer v1.
3. Run O1 original prompt and Phrasing B.
4. Run held-out prompts.
5. Add one correction row for one missed held-out transition.
6. Retrain transition normalizer v2.
7. Confirm the missed transition improves and original prompts do not regress.
8. Record timing, CPU, RAM, allocation, disk, model size, and quality metrics.

---

## Cycle 2 Unit

```text
orchestration-prompt-splitter
```

Input is no longer raw prompt alone.

Input is:

```text
LogicalTransitionRequest
human prompt reference
known topology
source authority
```

Output:

```text
expert domains;
phases;
sub-phases;
specific task branches;
dependencies;
parallel groups;
required arbiters;
knowledge edges;
```

---

## Cycle 3 Unit

```text
prompt-simplifier
```

The simplifier creates compact branch sources.

For each branch:

```text
Ai = compact source state
Bi = expected branch target
Xi = transformation required
```

This is the object a branch ILogicalUnit receives.

---

## Cycle 4 Through Cycle 8

Continue:

```text
domain-language-translator;
phase-sequencing-unit;
context-carry-forward-unit;
transition-verifier-unit;
arbiter-selection-unit;
sub-task-planner-unit;
human-result-formatter-unit.
```

Each cycle uses the same measured train-run-correct-retrain pattern.

---

## Metrics

Every cycle records:

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
transition_object_accuracy;
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

## Evidence Files For O1 Cycle 1

```text
O1-LOGICAL-TRANSITION-NORMALIZER-TRAINING-ROWS-v1.md
O1-LOGICAL-TRANSITION-REFERENCE-v1.md
O1-TRANSITION-NORMALIZER-METRICS-v1.json
O1-TRANSITION-NORMALIZER-HELDOUT-RESULTS-v1.json
O1-TRANSITION-NORMALIZER-RETRAINING-METRICS-v2.json
O1-TRANSITION-NORMALIZER-CYCLE-SUMMARY-v1.md
```

---

## Last Updated

2026-05-05. Version 2 starts real learning at transition normalization.
