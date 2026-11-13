# Held-Out and Corruption Pairs (017)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06

---

### Held-Out B36B1-HO-017-001 — "sprint" + "write" (IMPLEMENT fires via "write code")

```
held_out_reason: "Tests IMPLEMENT fires on 'write code' in sprint context"
expected_outcome: IMPLEMENT fires
```

A:
  user_request: "For this sprint, write the code for the new payment processing module."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001"
  Teaching: "write" + "code" + "module" fires IMPLEMENT. Sprint is gap warning.

---

### Held-Out B36B1-HO-017-002 — "burndown" + "define metrics" (DOCUMENT fires)

```
held_out_reason: "Tests DOCUMENT fires in burndown chart context"
expected_outcome: DOCUMENT fires
```

A:
  user_request: "Create the burndown chart and define the velocity metrics for tracking."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-burndown-001"
    - "gap-project-planning-velocity-001"
  Teaching: "define" fires DOCUMENT. "burndown" and "velocity" are gap. Not knowledge_edge.

---

### Corruption B36B1-COR-017-001 — v6-01: IMPLEMENT invented for "create the sprint plan"

```
corruption_type: "Corruption-v6-01: predicate incorrectly applied"
```

A:
  CompiledText: "Create the sprint plan and align milestones for the quarter."
  (Expected: knowledge_edge — "create" without code entity does not fire IMPLEMENT)

CORRUPT B (do not reproduce):
  CompiledPredicateIds: ["IMPLEMENT"]
  Note: "Model fired IMPLEMENT on 'create' without checking for code-level entity co-occurrence"

Why this is corrupt:
  "'create' fires IMPLEMENT only when co-occurring with a code-level entity (module, function, service, class).
   'sprint plan' and 'milestones' are not code entities. Zero predicates → knowledge_edge."

Correct B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"

---

### Corruption B36B1-COR-017-002 — v6-03: DomainId wrong for IMPLEMENT+gap scenario

```
corruption_type: "Corruption-v6-03: DomainId wrong when predicate fired"
```

A:
  CompiledText: "Plan the sprint and build the top backlog features."
  CompiledPredicateIds: ["IMPLEMENT"]
  (Expected: DomainId: "software-development")

CORRUPT B (do not reproduce):
  DomainId: "project-management"
  Note: "Model set project-management because sprint and backlog are planning vocabulary"

Why this is corrupt:
  "IMPLEMENT fired. Any predicate → DomainId: 'software-development'. The planning vocabulary
   proportion is irrelevant. DomainId is binary."

Correct B:
  DomainId: "software-development"
