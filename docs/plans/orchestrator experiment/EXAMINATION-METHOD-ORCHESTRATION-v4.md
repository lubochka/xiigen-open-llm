# Document Examination To Training Sets
# Logical transition orchestration edition
# Version: 4.0 - 2026-05-05
# Supersedes: EXAMINATION-METHOD-ORCHESTRATION-v3.md

---

## Purpose

This document defines how source material becomes learning rows for the corrected
dynamic flow:

```text
human prompt -> A/B/X transition -> expert branches -> compact Ai/Bi/Xi sources
-> logical unit results -> arbiters -> human result
```

The examination method must now look for transition structure before looking for
expert territories.

---

## First Examination Question

Before asking "which sub-phase is this?", ask:

```text
What source state A is the prompt trying to transform into target state B, and
what transformation X does it imply?
```

If A, B, and X cannot be identified, the correct output is a clarification or
source-gap row.

---

## Row Family 1 - Transition Normalization

### A side

Raw human prompt plus available context.

### B side

Actual transition object:

```yaml
source_state_A: []
target_state_B: []
transformation_intent_X: []
constraints: []
unknowns: []
rejected_intents: []
allowed_scope: []
```

The B side must not say "extract the source and target state." It must contain
the extracted source and target state.

---

## Row Family 2 - Expertise And Task Split

### A side

The transition object from Row Family 1.

### B side

Actual branch graph:

```yaml
branches:
  - branch_id:
    expertise_area:
    phase:
    sub_phase:
    grounded_by:
    depends_on:
    can_run_parallel_with:
    required_arbiters:
    knowledge_edges:
```

Every branch must be grounded in A, B, X, a constraint, source context, or a
necessary dependency.

---

## Row Family 3 - Prompt Simplification

### A side

One branch from the branch graph.

### B side

Compact logical source:

```yaml
Ai:
Bi:
Xi:
preserved_constraints:
source_authority:
```

This row teaches the machine to shorten the work for a specific unit without
losing the functional principle.

---

## Row Family 4 - Logical Unit Result

### A side

Compact Ai/Bi/Xi branch source.

### B side

Logical result:

```yaml
Ri:
Xi_evidence:
confidence:
warnings:
knowledge_edges:
```

The unit solves the branch, not the whole prompt.

---

## Row Family 5 - Arbiter Review

### A side

All branch results plus original A/B/X transition.

### B side

Actual arbiter review:

```yaml
selected_arbiters:
verdicts:
reprocess_branches:
blocked_branches:
concerns:
```

If a branch must be reprocessed, the B side names the corrected Xi or the missing
source authority that prevents reprocessing.

---

## Row Family 6 - Human Result Translation

### A side

Reviewed logical results.

### B side

Human result with:

```text
can proceed;
blocked;
rejected;
unknown;
next decisions;
resource measurements when this was a teaching run.
```

---

## Failure Modes

### LTO-1 - Skipping transition normalization

The row sends human wording directly into expert splitting.

Correction: add A/B/X transition row first.

### LTO-2 - Bloated branch prompt

The branch source repeats the whole human prompt.

Correction: shorten to Ai/Bi/Xi for that branch only.

### LTO-3 - Lost constraint during simplification

The compact prompt removes a constraint required by the original transition.

Correction: add preserved_constraints and carry it through the branch.

### LTO-4 - Unit solves outside branch scope

The unit result solves other branches or invents follow-up work.

Correction: keep Ri scoped to Ai/Bi/Xi.

### LTO-5 - Arbiter reprocesses whole prompt unnecessarily

The arbiter sends the whole prompt back when only one branch failed.

Correction: reprocess the failed branch unless the split itself was wrong.

### LTO-6 - Human formatter hides logical status

The final result omits blocked, rejected, unknown, or reprocess items.

Correction: preserve all status classes.

---

## O1 Required Row Set

The first O1 artifact set must include:

```text
one transition-normalizer row;
one expertise-split row;
one prompt-simplifier row per selected branch;
one logical-unit-result row per selected branch family;
one arbiter-review row;
one human-result row;
held-out transition-normalizer prompts;
one correction row for a missed held-out prompt.
```

---

## Last Updated

2026-05-05. Version 4 locks transition normalization and compact Ai/Bi/Xi branch
sources as required before expert-unit processing.
