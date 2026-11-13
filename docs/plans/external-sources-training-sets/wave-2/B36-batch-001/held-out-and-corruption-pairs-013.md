# Held-Out and Corruption Pairs (013)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06

---

### Held-Out B36B1-HO-013-001 — Planning-poker with "test" (TEST fires)

```
held_out_reason: "Tests whether 'test' fires TEST in a planning-poker context"
expected_outcome: TEST fires (literal match)
```

A:
  user_request: "During planning poker, test the team's understanding of each backlog item before estimating."
  Task: "Compile predicates and determine routing."

B:
  CompiledPredicateIds: ["TEST"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-backlog-001"
  Teaching: "test" fires TEST literally even in planning-poker context. False positive in intent,
    correct in lexicon terms.

---

### Held-Out B36B1-HO-013-002 — Effort-estimate: "plan" does not fire

```
held_out_reason: "Tests whether 'plan' fires any predicate"
expected_outcome: knowledge_edge (plan is not a predicate)
```

A:
  user_request: "Plan and estimate the effort for the entire backlog before the quarterly sprint cycle."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-estimation-001"
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-sprint-001"
  Teaching: "plan" is not a predicate. Zero predicates → knowledge_edge.

---

### Corruption B36B1-COR-013-001 — v6-01: PROJECT_PLANNING invented for planning-poker

```
corruption_type: "Corruption-v6-01: non-existent predicate invented"
```

A:
  CompiledText: "Run a planning-poker session to estimate the sprint backlog."
  CompiledPredicateIds: []

CORRUPT B (do not reproduce):
  CompiledPredicateIds: ["PROJECT_PLANNING"]
  SourceStatus: "ready"

Why this is corrupt:
  "PROJECT_PLANNING is not a registered predicate. The correct output is CompiledPredicateIds: [].
   The model invented a predicate to avoid knowledge-edge. This is v6-01."

Correct B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  KnownFailureMode: "no_predicate_match_capability_gap"

---

### Corruption B36B1-COR-013-002 — v6-03: DomainId wrong when DOCUMENT fired (story false positive)

```
corruption_type: "Corruption-v6-03: DomainId wrong when predicate fired"
```

A:
  CompiledText: "Run planning poker and assign story point estimates."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"

CORRUPT B (do not reproduce):
  DomainId: "project-management"

Why this is corrupt:
  "DOCUMENT fired. Any predicate → DomainId: 'software-development'. DomainId: 'project-management'
   is only correct when CompiledPredicateIds: []. The corrupt model set project-management because
   the intent is project planning. Intent does not determine DomainId — predicates do."

Correct B:
  DomainId: "software-development"
