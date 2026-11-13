# Held-Out and Corruption Pairs (021)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06

---

### Held-Out B36B1-HO-021-001 — "roadmap" + "review the API" (QA fires)

```
held_out_reason: "Tests QA fires in roadmap context via 'review'"
expected_outcome: QA fires
```

A:
  user_request: "Update the product roadmap for Q4 and review the API contract changes with the team."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-roadmap-001"
  Teaching: "review" fires QA unconditionally. "roadmap" is gap. QA fires; DomainId: "software-development".
    "update" without a code entity does NOT fire IMPLEMENT.

---

### Held-Out B36B1-HO-021-002 — "milestone" + "document requirements" (DOCUMENT fires)

```
held_out_reason: "Tests DOCUMENT fires in milestone context"
expected_outcome: DOCUMENT fires
```

A:
  user_request: "Set the Q3 milestones and document the system requirements for each release phase."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["DOCUMENT"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-milestone-001"
  Teaching: "document" fires DOCUMENT. "requirements" fires DOCUMENT. "milestones" is gap.
    "set" without code entity does not fire.

---

### Corruption B36B1-COR-021-001 — v6-02: QA suppressed in roadmap "update" context

```
corruption_type: "Corruption-v6-02: predicate suppressed"
```

A:
  CompiledText: "Review the roadmap items and approve the milestone schedule."
  (Expected: QA fires on "review"; QA fires on "approve")

CORRUPT B (do not reproduce):
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  Note: "Model suppressed QA because 'reviewing a roadmap is a planning activity'"

Why this is corrupt:
  "'review' fires QA unconditionally. 'approve' fires QA unconditionally. The planning context
   does not suppress QA. This is v6-02. Correct output: CompiledPredicateIds: ['QA']."

Correct B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"

---

### Corruption B36B1-COR-021-002 — v6-01: ROADMAP_PLANNING invented for "roadmap" vocabulary

```
corruption_type: "Corruption-v6-01: non-existent predicate invented"
```

A:
  CompiledText: "Build out the product roadmap and align on the Q4 milestones."
  (Expected: knowledge_edge — "build" without code entity, "roadmap" and "milestones" are gap)

CORRUPT B (do not reproduce):
  CompiledPredicateIds: ["ROADMAP_PLANNING"]
  SourceStatus: "ready"

Why this is corrupt:
  "ROADMAP_PLANNING is not a registered predicate. 'build out' in roadmap context does not
   co-occur with a code entity; therefore IMPLEMENT does not fire. Zero predicates → knowledge_edge.
   The corrupt model invented a predicate."

Correct B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  KnownFailureMode: "no_predicate_match_capability_gap"
