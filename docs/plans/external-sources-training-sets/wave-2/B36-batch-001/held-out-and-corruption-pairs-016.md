# Held-Out and Corruption Pairs (016)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06

---

### Held-Out B36B1-HO-016-001 — Retrospective with "document" (DOCUMENT fires)

```
held_out_reason: "Tests DOCUMENT fires in retrospective context"
expected_outcome: DOCUMENT fires (not knowledge_edge)
```

A:
  user_request: "During the retrospective, document the team's key learnings and define action items."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-retrospective-001: 'retrospective' is gap — not QA"
  Teaching: "document" fires DOCUMENT. "define" fires DOCUMENT. "retrospective" is gap warning.
    Not knowledge_edge because DOCUMENT fired.

---

### Held-Out B36B1-HO-016-002 — Standup with "build" (IMPLEMENT fires)

```
held_out_reason: "Tests IMPLEMENT fires in standup framing"
expected_outcome: IMPLEMENT fires
```

A:
  user_request: "At today's standup, confirm the team will build the notification service and implement the queue module."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["IMPLEMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-standup-001"
  Teaching: "build", "implement", "service", "module" fire IMPLEMENT. Standup is gap warning.

---

### Corruption B36B1-COR-016-001 — v6-01: QA invented for "retrospective"

```
corruption_type: "Corruption-v6-01: non-existent predicate application"
```

A:
  CompiledText: "Run the sprint retrospective and identify improvements."
  (Expected: CompiledPredicateIds: [] — retrospective is gap)

CORRUPT B (do not reproduce):
  CompiledPredicateIds: ["QA"]
  EvidencePredicates: ["QA"]
  Note: "Model fired QA on 'retrospective' confusing it with 'review'"

Why this is corrupt:
  "'retrospective' is NOT in the QA lexicon. Only 'review' fires QA. The corrupt model
   over-generalized from B36 boundary teaching (retrospective is like review) and incorrectly
   applied QA to 'retrospective'. Zero predicates → knowledge_edge."

Correct B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  KnownFailureMode: "no_predicate_match_capability_gap"

---

### Corruption B36B1-COR-016-002 — v6-02: QA suppressed for "velocity review"

```
corruption_type: "Corruption-v6-02: predicate suppressed"
```

A:
  CompiledText: "Review the team velocity metrics for the past sprint."
  (Expected: QA fires on "review")

CORRUPT B (do not reproduce):
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  Note: "Model suppressed QA because 'velocity review is a planning ceremony, not a QA review'"

Why this is corrupt:
  "'review' fires QA unconditionally. 'Velocity review' contains 'review' → QA fires.
   The Scrum ceremony context does not suppress QA. This is v6-02."

Correct B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
