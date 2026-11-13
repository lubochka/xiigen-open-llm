# Held-Out and Corruption Pairs (015)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06

---

### Held-Out B36B1-HO-015-001 — Grooming with "specify" (DOCUMENT fires)

```
held_out_reason: "Tests whether 'specify' fires DOCUMENT in grooming context"
expected_outcome: DOCUMENT fires
```

A:
  user_request: "Groom the backlog and specify the acceptance criteria for each ticket."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-ticket-001"
  Teaching: "specify" fires DOCUMENT. "criteria" fires DOCUMENT. Grooming/backlog/ticket are gap.

---

### Held-Out B36B1-HO-015-002 — Triage with "write code" (IMPLEMENT fires)

```
held_out_reason: "Tests that IMPLEMENT fires on 'write code' even after triage framing"
expected_outcome: IMPLEMENT fires
```

A:
  user_request: "After triaging the tickets, write the code to fix the top three bugs."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-ticket-001"
  Teaching: "write" + "code" fires IMPLEMENT. Triage framing is gap. IMPLEMENT dominates.

---

### Corruption B36B1-COR-015-001 — v6-02: DOCUMENT suppressed for grooming context

```
corruption_type: "Corruption-v6-02: predicate suppressed"
```

A:
  CompiledText: "Groom the backlog and refine user story acceptance criteria."
  (Expected: DOCUMENT fires on "story" and "criteria")

CORRUPT B (do not reproduce):
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  Note: "Model suppressed DOCUMENT because 'grooming context means project management'"

Why this is corrupt:
  "DOCUMENT fires on 'story' and 'criteria' unconditionally. The grooming context does not
   suppress predicates. This is v6-02. Correct output: CompiledPredicateIds: ['DOCUMENT']."

Correct B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

---

### Corruption B36B1-COR-015-002 — v6-03: DomainId project-management when DOCUMENT fired

```
corruption_type: "Corruption-v6-03: DomainId wrong when predicate fired"
```

A:
  CompiledText: "Define the backlog requirements and document functional specs for each epic."
  CompiledPredicateIds: ["DOCUMENT"]

CORRUPT B (do not reproduce):
  DomainId: "project-management"
  Note: "Model set project-management because backlog and epic are planning vocabulary"

Why this is corrupt:
  "DOCUMENT fired. One predicate → DomainId: 'software-development'. The planning vocabulary
   (backlog, epic) does not override the predicate-driven DomainId."

Correct B:
  DomainId: "software-development"
