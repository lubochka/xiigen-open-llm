# Layer 0-B — Router Pairs (007)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: router

---

## B36-0B-007-001 — [IMPLEMENT] → Phase 4.1 (implement epic features)

**Input predicate list:** `["IMPLEMENT"]`
**Request origin:** "Implement the features from Epic E-14 and close the sprint tickets when complete."

**Router decision tree:**

```
predicates contains IMPLEMENT? YES → Phase 4.1
predicates contains DOCUMENT? NO
predicates contains QA? NO
predicates contains ARCHITECTURE? NO
→ Single path: IMPLEMENT → Phase 4.1
→ TransitionType: feature_addition
→ DomainId: "software-development"
→ CapabilityGapWarnings: epic-001, sprint-001, ticket-001
```

**Router output:**
- Path: Phase 4.1 — BackendServiceFlow41
- TransitionType: feature_addition
- CapabilityGapWarnings attached to RouterResult
- Gap vocabulary does NOT affect which phase is selected

---

## B36-0B-007-002 — [IMPLEMENT] → Phase 4.1: "build" fires in sprint context

**Input predicate list:** `["IMPLEMENT"]`
**Request origin:** "Build the features listed in the epic backlog for this sprint."

**Router decision:**
- IMPLEMENT → Phase 4.1
- CapabilityGapWarnings: epic-001, backlog-001, sprint-001
- DomainId: "software-development"
- TransitionType: feature_addition

---

## B36-0B-007-003 — [IMPLEMENT] in sprint context: gap warnings are informational only

**CapabilityGapWarnings attached to RouterResult:**
```json
{
  "capabilityGapWarnings": [
    { "gapId": "gap-project-planning-epic-001", "status": "KNOWLEDGE_EDGE" },
    { "gapId": "gap-project-planning-sprint-001", "status": "KNOWLEDGE_EDGE" },
    { "gapId": "gap-project-planning-ticket-001", "status": "KNOWLEDGE_EDGE" }
  ],
  "scheduledSignals": [],
  "routerDecision": {
    "selectedPhase": "4.1",
    "selectedFlow": "BackendServiceFlow41",
    "transitionType": "feature_addition",
    "domainId": "software-development"
  }
}
```

**Note:** CapabilityGapWarnings are included in the RouterResult for logging and AutonomousLearningPipeline consumption. They are informational — they do not change routerDecision.selectedPhase or any other routing field.

---

## B36-0B-007-004 — [IMPLEMENT] routing: SCHEDULE_CAPABILITY_LEARNING not emitted

**When predicate fired:** SCHEDULE_CAPABILITY_LEARNING is NOT emitted.

SCHEDULE_CAPABILITY_LEARNING is only emitted when predicates: `[]` (zero predicates fired, full KNOWLEDGE_EDGE state). When IMPLEMENT fires — even alongside gap vocabulary — the router is NOT in KNOWLEDGE_EDGE state and does NOT emit SCHEDULE_CAPABILITY_LEARNING.

The gap IDs are reported as CapabilityGapWarnings, which the AutonomousLearningPipeline also consumes, but through a different signal path than SCHEDULE_CAPABILITY_LEARNING.

**Rule applied:** B36 Rule 9 — SCHEDULE_CAPABILITY_LEARNING emitted for every zero-predicate gap request (not for predicate-fired requests).
