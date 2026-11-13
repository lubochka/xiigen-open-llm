# Layer 0-D — Flow Factory Pairs (003)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B36-0D-003-001 — DOCUMENT factory: DocumentationFlow selected when DOCUMENT fires in planning context

**Input predicate list:** `["DOCUMENT"]`
**Request origin:** "Document the milestone deliverables and define the acceptance criteria for each sprint release."

**Factory selection process:**

```
predicates contains DOCUMENT? YES
→ FlowFactory.Select(DOCUMENT)
→ Returns: DocumentationFlow
→ Phase: 2.1
→ TransitionType: documentation
```

**Factory output:**
- Flow selected: DocumentationFlow
- Phase: 2.1
- CapabilityGapWarnings forwarded: milestone-001, sprint-001, release-cycle-001

**The gap vocabulary does not affect the factory selection.** DocumentationFlow is selected because DOCUMENT fired — not because of the planning context. The planning context appears only in CapabilityGapWarnings attached to the factory result.

---

## B36-0D-003-002 — DocumentationFlow in planning context: factory behavior

DocumentationFlow is designed to produce documentation artifacts. When triggered by planning-context DOCUMENT predicates, it produces the same documentation flow as when triggered by pure documentation requests. The fact that "milestone" or "sprint" appears in the request does not create a specialized planning flow.

**What DocumentationFlow does:**
- Creates Phase 2.1 execution context
- Uses DOCUMENT-phase node sequence
- Returns documentation artifact as FinalHumanResult

**What DocumentationFlow does NOT do:**
- Does not understand "milestone" or "sprint" as planning concepts
- Does not create sprint-specific documentation templates
- Does not route to any planning sub-system

---

## B36-0D-003-003 — DocumentationFlow factory result structure

```json
{
  "factoryResult": "DocumentationFlow",
  "selectedFlow": "DocumentationFlow",
  "selectedPhase": "2.1",
  "transitionType": "documentation",
  "domainId": "software-development",
  "capabilityGapWarnings": [
    "gap-project-planning-milestone-001",
    "gap-project-planning-sprint-001",
    "gap-project-planning-release-cycle-001"
  ]
}
```

---

## B36-0D-003-004 — DOCUMENT factory: "specify" and "requirements" in planning sentences

**Input:** "Specify the requirements for each milestone."
**Predicates:** ["DOCUMENT"]
**Factory:** DocumentationFlow
**Phase:** 2.1

The word "requirements" fires DOCUMENT — even in project planning sentences where the user means sprint acceptance requirements rather than software requirements specification (SRS). The factory sees ["DOCUMENT"] and selects DocumentationFlow without distinguishing between SRS requirements and sprint requirements. The planning context is noted via gap warnings but does not affect factory selection.
