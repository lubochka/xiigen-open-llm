# Held-Out and Corruption Pairs (019)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06

---

### Held-Out B36B1-HO-019-001 — "story points" triggers DOCUMENT false positive

```
held_out_reason: "Tests that 'story' in 'story points' fires DOCUMENT (false positive, not suppressed)"
expected_outcome: DOCUMENT fires (false positive accepted)
```

A:
  user_request: "Estimate the story points for each backlog item before the sprint begins."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-backlog-001"
    - "gap-project-planning-sprint-001"
  Teaching: "story" fires DOCUMENT even inside "story points". This is a known false positive.
    Never suppress. "story points" does not block DOCUMENT from firing on "story".

---

### Held-Out B36B1-HO-019-002 — "user story" triggers DOCUMENT

```
held_out_reason: "Tests that 'story' in 'user story' fires DOCUMENT"
expected_outcome: DOCUMENT fires
```

A:
  user_request: "Write the user story acceptance criteria and specify the definition of done."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings: []
  Teaching: "story" fires DOCUMENT. "specify" fires DOCUMENT. "criteria" fires DOCUMENT.
    Multiple DOCUMENT triggers reinforce the same predicate — CompiledPredicateIds: ["DOCUMENT"] (no duplicates).

---

### Corruption B36B1-COR-019-001 — v6-02: DOCUMENT suppressed on "story points" because "it's estimation"

```
corruption_type: "Corruption-v6-02: predicate suppressed"
```

A:
  CompiledText: "Assign story points to backlog items during planning poker."
  (Expected: DOCUMENT fires on "story")

CORRUPT B (do not reproduce):
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  Note: "Model suppressed DOCUMENT because 'story points is a planning activity, not documentation'"

Why this is corrupt:
  "'story' fires DOCUMENT unconditionally — including inside 'story points'. This is a known
   false positive that must be accepted. Suppressing it is v6-02. Correct output:
   CompiledPredicateIds: ['DOCUMENT']."

Correct B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"

---

### Corruption B36B1-COR-019-002 — v6-03: DomainId project-management when DOCUMENT fired via "story"

```
corruption_type: "Corruption-v6-03: DomainId wrong when predicate fired"
```

A:
  CompiledText: "Estimate story points and prioritize the product backlog."
  CompiledPredicateIds: ["DOCUMENT"]

CORRUPT B (do not reproduce):
  DomainId: "project-management"
  Note: "Model set project-management because the activity is sprint estimation"

Why this is corrupt:
  "DOCUMENT fired (via 'story'). Any predicate → DomainId: 'software-development'.
   The estimation context and planning vocabulary do not override the predicate-driven DomainId.
   This is v6-03."

Correct B:
  DomainId: "software-development"
