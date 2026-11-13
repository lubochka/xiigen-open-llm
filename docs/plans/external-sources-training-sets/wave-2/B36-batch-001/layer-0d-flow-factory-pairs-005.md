# Layer 0-D — Flow Factory Pairs (005)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B36-0D-005-001 — ARCHITECTURE factory: ArchitectureFlow31 when "design" fires in planning sentence

**Input predicate list:** `["ARCHITECTURE"]`
**Request origin:** "Design the system architecture for the sprint deliverables and plan the milestone dependencies."

**Factory selection process:**

```
predicates contains ARCHITECTURE? YES
→ FlowFactory.Select(ARCHITECTURE)
→ Returns: ArchitectureFlow31
→ Phase: 3.1
→ TransitionType: documentation
```

**Factory output:**
- Flow selected: ArchitectureFlow31
- Phase: 3.1
- TransitionType: documentation
- CapabilityGapWarnings: sprint-001, milestone-001

---

## B36-0D-005-002 — ArchitectureFlow31 characteristics

ArchitectureFlow31 produces architectural documentation output. It is triggered by ARCHITECTURE predicate regardless of whether the sentence is about system architecture or project planning. The factory does not distinguish between "design the system capacity" and "design the sprint structure" — both fire ARCHITECTURE and both select ArchitectureFlow31.

**Flow output:** Architectural documentation artifact (not sprint plan, not roadmap)

---

## B36-0D-005-003 — ArchitectureFlow31 factory result structure

```json
{
  "factoryResult": "ArchitectureFlow31",
  "selectedFlow": "ArchitectureFlow31",
  "selectedPhase": "3.1",
  "transitionType": "documentation",
  "domainId": "software-development",
  "capabilityGapWarnings": [
    "gap-project-planning-sprint-001",
    "gap-project-planning-milestone-001"
  ],
  "nineNodeFlowEnabled": false
}
```

**Note:** ARCHITECTURE does not trigger the nine-node flow (that is IMPLEMENT's domain). ArchitectureFlow31 runs its own phase sequence under Phase 3.x.

---

## B36-0D-005-004 — "design the sprint structure" example

**Input:** "Design the sprint structure and define the workflow stages."

**Tokens:**
- design → ARCHITECTURE
- structure → ARCHITECTURE
- sprint → GAP
- define → DOCUMENT

**Predicates:** ["ARCHITECTURE", "DOCUMENT"]
**Factories selected:** ArchitectureFlow31 (Phase 3.1) + DocumentationFlow (Phase 2.1)
**CapabilityGapWarnings:** sprint-001

Both factories selected and both phases run. The sprint gap warning is attached. Neither factory knows about sprint planning; both execute their standard phase sequences.
