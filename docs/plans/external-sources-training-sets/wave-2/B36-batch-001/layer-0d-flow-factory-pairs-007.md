# Layer 0-D — Flow Factory Pairs (007)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B36-0D-007-001 — IMPLEMENT+DOCUMENT factory: both selected; gap vocabulary warnings attached

**Input predicate list:** `["IMPLEMENT", "DOCUMENT"]`
**Request origin:** "Write the user stories for the checkout feature and estimate the story points."

**Factory selection process:**

```
predicates contains IMPLEMENT? YES → FlowFactory.Select(IMPLEMENT) → BackendServiceFlow41
predicates contains DOCUMENT? YES → FlowFactory.Select(DOCUMENT) → DocumentationFlow
→ Both factories selected
→ Parallel execution eligible
→ Phase: 4.1 (primary), 2.1 (parallel)
→ TransitionType: feature_addition (IMPLEMENT is primary)
```

**Factory output:**
- Primary flow: BackendServiceFlow41 (Phase 4.1)
- Secondary flow: DocumentationFlow (Phase 2.1)
- Parallel eligible: true
- CapabilityGapWarnings: story-point-001

---

## B36-0D-007-002 — two-factory parallel execution model

When IMPLEMENT and DOCUMENT both fire:

```
BackendServiceFlow41 (Phase 4.1) ─┐
                                   ├─ parallel execution
DocumentationFlow (Phase 2.1)     ─┘
         ↓
Both complete → merge results
```

The gap warnings (story-point-001 in this case) are attached to the merged factory result and forwarded to the orchestrator.

---

## B36-0D-007-003 — IMPLEMENT+DOCUMENT factory result structure

```json
{
  "factoryResults": [
    {
      "flow": "BackendServiceFlow41",
      "phase": "4.1",
      "transitionType": "feature_addition",
      "primary": true
    },
    {
      "flow": "DocumentationFlow",
      "phase": "2.1",
      "transitionType": "documentation",
      "primary": false
    }
  ],
  "domainId": "software-development",
  "capabilityGapWarnings": [
    "gap-project-planning-story-point-001",
    "gap-project-planning-estimation-001"
  ],
  "parallelEligible": true
}
```

---

## B36-0D-007-004 — "build the sprint tracker service, document the backlog requirements"

**Input:** "Build the sprint tracking service and document the backlog requirements."
**Predicates:** ["IMPLEMENT", "DOCUMENT"]
**Factories:** BackendServiceFlow41 + DocumentationFlow
**CapabilityGapWarnings:** sprint-001, backlog-001

Both factories selected. Sprint and backlog are gap warnings — they do not create a "sprint tracking" factory or a "backlog documentation" factory. The standard IMPLEMENT and DOCUMENT factories handle the request.
