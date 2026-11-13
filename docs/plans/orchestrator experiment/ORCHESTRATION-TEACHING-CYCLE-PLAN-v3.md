# Orchestration Teaching Cycle Plan
# Version: 3.0 - 2026-05-05
# Supersedes: ORCHESTRATION-TEACHING-CYCLE-PLAN-v2.md

---

## Purpose

This plan defines the first real teaching cycle after correcting the transition
layer.

The first teaching cycle trains and measures the transition orchestrator pattern,
not a single deterministic normalizer.

---

## First Component Under Teaching

```text
LogicalTransitionOrchestrator : ILogicalOrchestrator
```

The orchestrator contains learned units and selectable flows.

It returns a selected A/B/X transition or a clarification result.

---

## Internal Teaching Targets

Cycle 1 teaches these internal units in order:

| Step | Unit or flow | Learns |
|---:|---|---|
| 1 | TransitionIntentClassifier | Which transition types the prompt may imply. |
| 2 | CandidateTransitionGenerator | How to produce A/B/X candidates for each plausible type. |
| 3 | TransitionCandidateCompressor | How to shorten candidates without losing constraints. |
| 4 | TransitionArbiterSelector | Which arbiters judge candidate validity. |
| 5 | TransitionCandidateSelector | Which candidate becomes selected, rejected, or clarification. |

The public surface remains `ILogicalOrchestrator`.

---

## O1 Training Rows

O1 produces rows for:

```text
feature_addition selected;
repair rejected;
learning_cycle rejected;
topology_mutation rejected;
external_source_integration partially related but not selected;
clarification case when prompt only says "fix auth" without enough evidence.
```

The selected row teaches:

```text
feature addition across security design, implementation, testing, QA, and automation.
```

The rejected rows teach:

```text
why the same prompt is not repair;
why it is not model teaching;
why it is not methodology mutation;
why external-source material supports the security branch but is not the whole transition.
```

---

## Cycle 1 Run Shape

```text
train transition intent classifier;
train candidate generator;
train candidate selector;
run deterministic fixture;
run learned transition orchestrator v1;
run held-out prompts;
add one correction row;
retrain v2;
compare v1/v2 and deterministic fixture;
record resource and quality metrics;
zip evidence.
```

---

## Output Evidence

Cycle 1 evidence contains:

```text
candidate transitions generated;
candidate scores;
selected candidate;
rejected candidates with reasons;
arbiter verdicts;
clarification questions when needed;
resource metrics;
quality metrics;
nearest training rows or model evidence.
```

---

## Metrics

```text
candidate_count;
selected_transition_type_accuracy;
candidate_margin;
candidate_rejection_accuracy;
clarification_accuracy;
transition_object_accuracy;
false_candidate_selected;
source_gap_preserved;
rejected_intent_preserved;
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
```

---

## After Cycle 1

The second component is:

```text
ExpertiseTaskSplitter
```

It receives the selected transition from `LogicalTransitionOrchestrator`.

It does not receive raw prompt text as its main source.

---

## Last Updated

2026-05-05. Version 3 starts real learning with the non-deterministic
transition orchestrator pattern.
