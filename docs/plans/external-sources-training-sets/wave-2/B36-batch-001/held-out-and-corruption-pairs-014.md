# Held-Out and Corruption Pairs (014)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06

---

### Held-Out B36B1-HO-014-001 — Release-planning with "approve" (QA fires)

```
held_out_reason: "Tests whether 'approve' fires QA in release planning context"
expected_outcome: QA fires
```

A:
  user_request: "Approve the release plan and confirm the Q4 milestone targets with the team lead."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-release-cycle-001"
    - "gap-project-planning-milestone-001"
  Teaching: "approve" fires QA unconditionally — even in release planning context.

---

### Held-Out B36B1-HO-014-002 — Product-roadmap with "structure" (ARCHITECTURE fires)

```
held_out_reason: "Tests whether 'structure' fires ARCHITECTURE in roadmap planning"
expected_outcome: ARCHITECTURE fires
```

A:
  user_request: "Structure the product roadmap to align with our architecture decisions for Q3."
  Task: "Compile predicates."

B:
  CompiledPredicateIds: ["ARCHITECTURE"]
  SourceStatus: "ready"
  CapabilityGapWarnings:
    - "gap-project-planning-roadmap-001"
  Teaching: "structure" fires ARCHITECTURE always. "architecture" also fires in "architecture decisions".

---

### Corruption B36B1-COR-014-001 — v6-02: "review" suppressed for release planning

```
corruption_type: "Corruption-v6-02: predicate suppressed because sentence feels like planning"
```

A:
  CompiledText: "Review the release plan and confirm the milestone dates."
  (Expected: QA fires on "review")

CORRUPT B (do not reproduce):
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
  Note: "Corrupt model suppressed QA because 'the sentence is about release planning, not software review'"

Why this is corrupt:
  "'review' fires QA unconditionally. The planning context does not suppress QA. This is v6-02:
   suppressing a predicate based on semantic intent. The correct output is CompiledPredicateIds: ['QA']."

Correct B:
  CompiledPredicateIds: ["QA"]
  SourceStatus: "ready"

---

### Corruption B36B1-COR-014-002 — v6-01: RELEASE_PLANNING predicate invented

```
corruption_type: "Corruption-v6-01: non-existent predicate invented"
```

A:
  CompiledText: "Create the release plan for Q4."
  (Expected: knowledge_edge — no predicate fires)

CORRUPT B (do not reproduce):
  CompiledPredicateIds: ["RELEASE_PLANNING"]
  SourceStatus: "ready"

Why this is corrupt:
  "RELEASE_PLANNING is not a registered predicate. 'create' without a code entity does not fire IMPLEMENT.
   'release plan' is gap vocabulary. Zero predicates → knowledge_edge. The corrupt model invented a
   predicate to avoid knowledge-edge."

Correct B:
  CompiledPredicateIds: []
  SourceStatus: "knowledge_edge"
