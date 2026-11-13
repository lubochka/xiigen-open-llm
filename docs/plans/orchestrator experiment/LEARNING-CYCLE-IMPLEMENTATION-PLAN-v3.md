# Learning Cycle Implementation Plan
# Transition-orchestrator version
# Version: 3.0 - 2026-05-05
# Supersedes: LEARNING-CYCLE-IMPLEMENTATION-PLAN-v2.md

---

## Main Correction

Version 2 made transition normalization first but still described it too much
like a unit.

Version 3 makes it an orchestrator.

The first learned runtime component is:

```text
LogicalTransitionOrchestrator : ILogicalOrchestrator
```

It uses internal learned units and flows to select the right A/B/X transition.

---

## First Implementation Slice

The first implementation slice is intentionally small:

```text
define transition result records;
define a transition orchestrator adapter around one deterministic fixture;
add tracing and resource measurement shape;
add O1 reference candidate evidence;
replace fixture with learned candidate selector later.
```

This keeps the public surface as `ILogicalOrchestrator` from the beginning.

---

## Internal Units

| Unit | Input | Output |
|---|---|---|
| `TransitionIntentClassifier` | human prompt + context | candidate transition type scores |
| `CandidateTransitionGenerator` | prompt + candidate type | candidate A/B/X transition |
| `TransitionCandidateCompressor` | candidate A/B/X | compact candidate preserving constraints |
| `TransitionArbiterSelector` | candidate set | arbiters required for candidate review |
| `TransitionCandidateSelector` | candidates + arbiter verdicts | selected candidate or clarification |

---

## First Model

The first learned model is used inside `TransitionIntentClassifier` or
`TransitionCandidateSelector`, not as the whole orchestrator.

The orchestrator remains:

```text
flow selector + evidence holder + result normalizer
```

The learned units inside it are replaceable.

---

## First Measured Cycle

Cycle O1-T0:

```text
train candidate-type scoring;
generate candidate A/B/X objects;
select feature_addition as O1;
reject repair, learning_cycle, topology_mutation, and external_source_integration as primary transitions;
preserve external_source_integration as supporting source context for security branches;
record metrics;
add one correction row for a held-out ambiguity;
retrain;
confirm selected candidate improves or remains stable.
```

---

## Evidence Files

```text
O1-TRANSITION-ORCHESTRATOR-TRAINING-ROWS-v1.md
O1-TRANSITION-CANDIDATE-REFERENCE-v1.md
O1-TRANSITION-ORCHESTRATOR-METRICS-TEMPLATE-v1.json
O1-TRANSITION-ORCHESTRATOR-CYCLE-SUMMARY-TEMPLATE-v1.md
```

---

## Last Updated

2026-05-05.
