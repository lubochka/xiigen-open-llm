# Held-Out and Corruption Pairs (023)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06

---

### Held-Out B36B1-HO-023-001 — Multi-gap with "specify" (DOCUMENT fires)

```
held_out_reason: "Tests DOCUMENT fires with high gap-vocabulary density"
expected_outcome: DOCUMENT fires (gap density does not suppress predicates)
```

A:
  user_request: "For the sprint backlog, specify the acceptance criteria for each epic ticket before the iteration begins."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-epic-001"
    - "gap-project-planning-ticket-001"
    - "gap-project-planning-iteration-001"
  Teaching: "specify" fires DOCUMENT. "criteria" fires DOCUMENT. High gap-vocabulary density
    (5 gap terms) does not suppress predicates. DomainId: "software-development".

---

### Held-Out B36B1-HO-023-002 — "velocity" + "standup" pure gap (knowledge_edge)

```
held_out_reason: "Tests knowledge_edge when all vocabulary is gap with no predicate"
expected_outcome: knowledge_edge (zero predicates)
```

A:
  user_request: "Discuss team velocity trends and standup blockers before the next sprint planning session."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-velocity-001"
    - "gap-project-planning-standup-001"
    - "gap-project-planning-sprint-001"
  Teaching: No predicate fires. "discuss", "trends", "blockers" are not predicate keywords.
    All vocabulary is gap. SCHEDULE_CAPABILITY_LEARNING emitted. DomainId: "project-management".

---

### Corruption B36B1-COR-023-001 — v6-03: DomainId project-management with high gap density and DOCUMENT fired

```
corruption_type: "Corruption-v6-03: DomainId wrong when predicate fired"
```

A:
  CompiledText: "Specify the backlog ticket criteria for the sprint epic breakdown."
  CompiledPredicateIds: ["DOCUMENT"]

CORRUPT B (do not reproduce):
  DomainId: "project-management"
  Note: "Model weighted 4 gap terms vs 1 predicate and chose project-management"

Why this is corrupt:
  "DomainId is binary. One predicate fires → 'software-development'. The ratio of gap terms to
   predicate triggers is irrelevant. Gap vocabulary proportion does not factor into DomainId
   determination. This is v6-03."

Correct B:
  DomainId: "software-development"

---

### Corruption B36B1-COR-023-002 — v6-02: DOCUMENT suppressed when "sprint" density is high

```
corruption_type: "Corruption-v6-02: predicate suppressed"
```

A:
  CompiledText: "Document the sprint velocity, define acceptance criteria, and record burndown notes."
  (Expected: DOCUMENT fires on "document", "define", "criteria", "record")

CORRUPT B (do not reproduce):
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  Note: "Model suppressed DOCUMENT because 'the sentence is dominated by sprint planning vocabulary'"

Why this is corrupt:
  "'document', 'define', 'criteria', 'record' all fire DOCUMENT unconditionally. Sprint planning
   vocabulary does not suppress predicates. This is v6-02. Correct output:
   CompiledPredicateIds: ['DOCUMENT']."

Correct B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
