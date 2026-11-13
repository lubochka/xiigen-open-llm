# Layer 0-D — Flow Factory Pairs (004)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B36-0D-004-001 — IMPLEMENT factory: BackendServiceFlow41 selected when IMPLEMENT fires in sprint context

**Input predicate list:** `["IMPLEMENT"]`
**Request origin:** "Implement the features from Epic E-14 and close the sprint tickets when complete."

**Factory selection process:**

```
predicates contains IMPLEMENT? YES
→ FlowFactory.Select(IMPLEMENT)
→ Returns: BackendServiceFlow41
→ Phase: 4.1
→ TransitionType: feature_addition
```

**Factory output:**
- Flow selected: BackendServiceFlow41
- Phase: 4.1
- CapabilityGapWarnings: epic-001, sprint-001, ticket-001

---

## B36-0D-004-002 — BackendServiceFlow41 in sprint context: nine-node flow executes

When IMPLEMENT fires, BackendServiceFlow41 executes the nine-node flow starting at Phase 4.1. The presence of sprint/epic/ticket gap vocabulary does not alter the nine-node flow structure. The nine nodes execute in their standard sequence; gap warnings are logged.

**Nine-node flow is triggered by IMPLEMENT, not suppressed by gap vocabulary.**

---

## B36-0D-004-003 — IMPLEMENT factory result structure

```json
{
  "factoryResult": "BackendServiceFlow41",
  "selectedFlow": "BackendServiceFlow41",
  "selectedPhase": "4.1",
  "transitionType": "feature_addition",
  "domainId": "software-development",
  "capabilityGapWarnings": [
    "gap-project-planning-epic-001",
    "gap-project-planning-sprint-001",
    "gap-project-planning-ticket-001"
  ],
  "nineNodeFlowEnabled": true
}
```

---

## B36-0D-004-004 — "build the sprint tracker service" → BackendServiceFlow41

**Input:** "Build the sprint tracking service."
**Tokens firing IMPLEMENT:** "build", "service"
**Gap tokens:** "sprint"
**Factory:** BackendServiceFlow41
**Phase:** 4.1

This is a common pattern in B36: the user wants to build a technical service that supports sprint tracking. IMPLEMENT fires on "build" and "service" (code-level entity). The factory selects BackendServiceFlow41. The sprint context is a gap warning. The service implementation proceeds through the nine-node flow.
