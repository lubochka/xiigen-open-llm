# Layer 0-B — Router Pairs (008)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-05 | pair_kind: router

---

## B36-0B-008-001 — [DOCUMENT] → Phase 2.1 (document milestone deliverables)

**Input predicate list:** `["DOCUMENT"]`
**Request origin:** "Document the milestone deliverables and define the acceptance criteria for each sprint release."

**Router decision tree:**

```
predicates contains DOCUMENT? YES → Phase 2.1
predicates contains IMPLEMENT? NO
predicates contains QA? NO
predicates contains ARCHITECTURE? NO
→ Single path: DOCUMENT → Phase 2.1
→ TransitionType: documentation
→ DomainId: "software-development"
→ CapabilityGapWarnings: milestone-001, sprint-001, release-cycle-001
```

**Router output:**
- Path: Phase 2.1 — DocumentationFlow
- TransitionType: documentation
- CapabilityGapWarnings: milestone-001, sprint-001, release-cycle-001

---

## B36-0B-008-002 — [DOCUMENT] in planning context: documentation TransitionType

**TransitionType for DOCUMENT is always "documentation" regardless of context.**

When the user says "document the milestone deliverables" — even though the subject is a project planning artifact — the TransitionType is "documentation" because the predicate DOCUMENT fired. The TransitionType is determined by the predicate, not by the artifact being documented.

---

## B36-0B-008-003 — [DOCUMENT] routing: "stakeholder" fires DOCUMENT

**Input predicate list:** `["DOCUMENT"]`
**Request origin:** "Gather stakeholder input on the sprint goals and document the backlog priorities."

**Router decision:**
- DOCUMENT → Phase 2.1
- TransitionType: documentation
- DomainId: "software-development"
- CapabilityGapWarnings: sprint-001, backlog-001

---

## B36-0B-008-004 — [DOCUMENT] routing: "requirements" fires DOCUMENT

**Input predicate list:** `["DOCUMENT"]`
**Request origin:** "Specify the requirements for each milestone and document the sprint deliverables."

**Router decision:**
- DOCUMENT → Phase 2.1
- TransitionType: documentation
- DomainId: "software-development"
- CapabilityGapWarnings: milestone-001, sprint-001

**Note on "requirements":** This is B36 Rule 5 — "requirements" fires DOCUMENT. Even in project planning sentences where the user means acceptance requirements for a sprint, the compiler fires DOCUMENT. The routing then proceeds to Phase 2.1. The planning context is noted via CapabilityGapWarnings but does not override the documentation routing.
