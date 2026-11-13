# Layer 0-B — Router Pairs (002)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: router

---

## B36-0B-002-001 — [] → knowledge_edge path (backlog grooming)

**Input predicate list:** `[]` (zero predicates — from backlog grooming request)
**Request origin:** "Groom the product backlog and prioritize the tickets for the next iteration."

**Router decision tree:**

```
predicates == [] ?
  YES →
    gap_vocabulary_present? YES
      → KNOWLEDGE_EDGE state
      → FinalHumanResult: null
      → TransitionType: unknown
      → PhaseId: null
      → DomainId: "project-management"
      → emit SCHEDULE_CAPABILITY_LEARNING
      → emit CapabilityGapWarnings
      → STOP (no nine-node flow executes)
```

**Router output:**
- Path selected: KNOWLEDGE_EDGE
- Flow selected: NONE
- CapabilityGapWarnings forwarded: backlog-001, iteration-001, ticket-001
- SCHEDULE_CAPABILITY_LEARNING: emitted once

**What does NOT happen:**
- No sub-phase is selected (no 4.1, 2.1, 3.1, or QA sub-phase)
- No nine-node flow executes
- No FinalHumanResult is generated
- No ContextCarryForward

---

## B36-0B-002-002 — [] → knowledge_edge path: roadmap/sprint

**Input predicate list:** `[]`
**Request origin:** "Update the roadmap for the next three sprints."

**Router decision:**
- predicates: [] → KNOWLEDGE_EDGE
- "update" checked: no code-level co-occurrence → not IMPLEMENT
- CapabilityGapWarnings: roadmap-001, sprint-001
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

---

## B36-0B-002-003 — [] → knowledge_edge path: standup/capacity

**Input predicate list:** `[]`
**Request origin:** "Run the daily standup and capture team capacity for the sprint planning session."

**Router decision:**
- predicates: [] → KNOWLEDGE_EDGE
- CapabilityGapWarnings: standup-001, capacity-001, sprint-001
- DomainId: "project-management"
- SCHEDULE_CAPABILITY_LEARNING emitted

---

## B36-0B-002-004 — [] → knowledge_edge: SCHEDULE_CAPABILITY_LEARNING is not a retry signal

**Input predicate list:** `[]`
**Request origin:** "Plan the velocity targets for the next iteration."

**Router output:**
- KNOWLEDGE_EDGE state: true
- SCHEDULE_CAPABILITY_LEARNING emitted

**What SCHEDULE_CAPABILITY_LEARNING does NOT do:**
- It does NOT cause the request to be retried
- It does NOT cause the router to reprocess the request with different predicates
- It does NOT block the current response
- It signals the AutonomousLearningPipeline (subproject 16) to log the gap
- The current request gets a null FinalHumanResult — the gap is noted, not filled

**Rule applied:** B36 Rule 10 — AutonomousLearningPipeline (subproject 16) consumes SCHEDULE_CAPABILITY_LEARNING signals; the current request is not rerouted.
