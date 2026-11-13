# Held-Out and Corruption Pairs (024)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06

---

### Held-Out B36B1-HO-024-001 — QA-last constraint: QA + IMPLEMENT co-occurrence

```
held_out_reason: "Tests QA-last structural rule when QA and IMPLEMENT co-occur"
expected_outcome: both predicates fire; QA executes last in flow
```

A:
  user_request: "Review the sprint deliverables and implement the remaining API endpoints."
  Task: "Compile predicates and state execution order."

B:
  CompiledPredicateIds: ["QA", "IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001"
  ExecutionOrder: "IMPLEMENT executes first; QA executes last (QA-last rule)."
  Teaching: "review" fires QA. "implement" + "endpoints" fire IMPLEMENT. QA-last: QA flow always
    executes after all other predicate flows complete.

---

### Held-Out B36B1-HO-024-002 — Pure gap "run our sprint planning" (knowledge_edge)

```
held_out_reason: "Tests knowledge_edge for pure planning vocabulary with no predicate trigger"
expected_outcome: knowledge_edge
```

A:
  user_request: "Run our sprint planning ceremony, estimate backlog items, and set velocity targets."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  CapabilityGapWarnings:
    - "gap-project-planning-sprint-001"
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-velocity-001"
  Teaching: "run", "ceremony", "estimate", "targets" are not predicate keywords. Zero predicates.
    SCHEDULE_CAPABILITY_LEARNING emitted. DomainId: "project-management".

---

### Corruption B36B1-COR-024-001 — v6-01: QA-last violated — QA placed first

```
corruption_type: "Corruption-v6-01: structural rule violated"
```

A:
  CompiledText: "Review the sprint and build the notification service."
  CompiledPredicateIds: ["QA", "IMPLEMENT"]
  (Expected: IMPLEMENT executes first, QA executes last)

CORRUPT B (do not reproduce):
  ExecutionOrder: "QA executes first; IMPLEMENT executes second."

Why this is corrupt:
  "QA-last is a hard structural rule. When QA co-occurs with any other predicate, QA flow
   executes AFTER all other flows. Placing QA first violates the QA-last constraint.
   The corrupt model reversed the order."

Correct B:
  ExecutionOrder: "IMPLEMENT executes first; QA executes last."

---

### Corruption B36B1-COR-024-002 — v6-03: DomainId project-management on knowledge_edge with gap IDs

```
corruption_type: "Corruption-v6-03: DomainId correct label but wrong reasoning"
```

A:
  CompiledText: "Plan the sprint backlog and prioritize the roadmap milestones."
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"

CORRUPT B (do not reproduce):
  DomainId: "project-management"
  Note: "DomainId: project-management because sprint and roadmap are planning"
  Reasoning: "Gap vocabulary proportion determines DomainId"

Why this is corrupt:
  "The DomainId value ('project-management') happens to be correct for this knowledge_edge case,
   but the reasoning is wrong. DomainId is determined by predicate presence, NOT by vocabulary
   proportion. CompiledPredicateIds: [] → DomainId: 'project-management' always.
   Gap vocabulary is irrelevant to the rule. Reasoning matters for generalization."

Correct B:
  DomainId: "project-management"
  Reasoning: "CompiledPredicateIds: [] → DomainId: 'project-management'. Predicate absence
    determines DomainId. Gap vocabulary proportion is irrelevant."
