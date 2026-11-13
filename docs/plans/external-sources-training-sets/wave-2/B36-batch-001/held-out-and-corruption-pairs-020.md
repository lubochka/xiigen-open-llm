# Held-Out and Corruption Pairs (020)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06

---

### Held-Out B36B1-HO-020-001 — "epic" + "implement" (IMPLEMENT fires)

```
held_out_reason: "Tests IMPLEMENT fires in epic-breakdown context"
expected_outcome: IMPLEMENT fires
```

A:
  user_request: "Break the epic into tasks and implement the authentication service for this sprint."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-epic-001"
    - "gap-project-planning-sprint-001"
  Teaching: "implement" + "service" fire IMPLEMENT. "epic" and "sprint" are gap warnings. IMPLEMENT fires unconditionally.

---

### Held-Out B36B1-HO-020-002 — "velocity" + "test the module" (TEST fires)

```
held_out_reason: "Tests TEST fires in velocity-tracking context"
expected_outcome: TEST fires
```

A:
  user_request: "Track the team velocity and test the payment module before the sprint ends."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["TEST"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-velocity-001"
    - "gap-project-planning-sprint-001"
  Teaching: "test" + "module" fire TEST. "velocity" and "sprint" are gap vocabulary.

---

### Corruption B36B1-COR-020-001 — v6-01: EPIC_BREAKDOWN invented for "break the epic"

```
corruption_type: "Corruption-v6-01: non-existent predicate invented"
```

A:
  CompiledText: "Break down the epic into user stories and milestones."
  (Expected: knowledge_edge — "epic", "stories", "milestones" are gap; "story" fires DOCUMENT)

CORRUPT B (do not reproduce):
  CompiledPredicateIds: ["EPIC_BREAKDOWN"]
  SourceStatus: "ready"

Why this is corrupt:
  "EPIC_BREAKDOWN is not a registered predicate. 'break down' without a code entity does not fire.
   'stories' does NOT fire DOCUMENT because the root 'story' requires exact token match — the
   plural does fire on 'story' if the exact word appears. However 'stories' ≠ 'story' literal.
   In this sentence 'stories' is the plural form: zero predicates → knowledge_edge.
   The corrupt model invented a predicate."

Correct B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  KnownFailureMode: "no_predicate_match_capability_gap"

---

### Corruption B36B1-COR-020-002 — v6-03: DomainId project-management when TEST fired in sprint context

```
corruption_type: "Corruption-v6-03: DomainId wrong when predicate fired"
```

A:
  CompiledText: "Validate sprint goals and test the deployment pipeline."
  CompiledPredicateIds: ["TEST"]

CORRUPT B (do not reproduce):
  DomainId: "project-management"
  Note: "Model set project-management because sprint goals validation sounds like planning"

Why this is corrupt:
  "TEST fired. Any predicate → DomainId: 'software-development'. Sprint context does not
   override the predicate-driven DomainId. This is v6-03."

Correct B:
  DomainId: "software-development"
