# External Sources Orchestration Pipeline Plan
# Transition orchestrator edition
# Version: 4.0 - 2026-05-05
# Supersedes: EXTERNAL-SOURCES-ORCHESTRATION-PIPELINE-PLAN-v3.md

---

## Purpose

This pipeline describes how external and first-party sources become learning
material for:

```text
LogicalTransitionOrchestrator
```

before expert splitting begins.

---

## Corrected Pipeline

```text
source registry
-> prompt extraction
-> candidate transition extraction
-> selected/rejected candidate oracle
-> transition-orchestrator teaching rows
-> expert split rows from selected transition
-> compact branch rows
-> measured offline training
-> held-out run
-> one-row correction
-> retraining
-> evidence zip
```

---

## Stage 1 - Candidate Transition Extraction

For every prompt, identify:

```text
selected transition type;
plausible rejected transition types;
clarification cases;
source evidence for each candidate;
candidate confidence target.
```

---

## Stage 2 - Transition-Orchestrator Row Production

Rows must include:

```text
candidates;
selected transition;
rejected candidates;
candidate reasons;
transition arbiters;
resource measurement requirement.
```

---

## Stage 3 - Expert Split From Selected Transition

The expert splitter receives:

```text
selected A/B/X transition
```

The raw prompt remains evidence, but not the primary working object.

---

## Stage 4 - Measured Teaching Cycle

Train and run:

```text
LogicalTransitionOrchestrator with learned internal units;
deterministic reference;
held-out prompt variants;
corruption variants;
one-row correction and retraining.
```

---

## Wave 1 Use

Wave 1 supports candidate evidence. It can make a candidate more plausible, but
it does not force selection.

Example:

```text
O1 has Wave 1 security evidence, but the selected transition is feature_addition.
O4 has Wave 1 external-source evidence and may select external_source_integration.
```

---

## Last Updated

2026-05-05.
