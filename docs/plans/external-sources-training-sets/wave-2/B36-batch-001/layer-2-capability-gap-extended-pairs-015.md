# Layer 2 — Capability Gap Extended Pairs (015)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B36-L2-015-001 — "create the release plan" gap (no DOCUMENT)

A: "Create the release plan for Q4 and map milestones to sprints."

B:
  Predicates: []
  GapId: gap-project-planning-release-cycle-001
  DomainId: project-management
  Analysis: "create" without a code-level entity does NOT fire IMPLEMENT. "release plan",
    "milestones", "sprints" are all gap vocabulary. Zero predicates → knowledge_edge.
    DomainId: project-management. SCHEDULE_CAPABILITY_LEARNING emitted.
    Note: "release plan" is NOT in the DOCUMENT lexicon. "release" alone ≠ DOCUMENT.

---

## B36-L2-015-002 — "release plan" + "review" (QA fires)

A: "Review the release plan and sign off on the Q3 milestone targets."

B:
  Predicates: [QA]
  GapId: gap-project-planning-release-cycle-001
  DomainId: software-development
  Analysis: "review" fires QA. "release plan" and "milestones" are gap. DomainId: software-development.
    Gap warning for release-cycle-001 context not captured by QA routing.

---

## B36-L2-015-003 — "release plan" + "document" (DOCUMENT fires)

A: "Document the release plan and write the release notes for each milestone."

B:
  Predicates: [DOCUMENT]
  GapId: gap-project-planning-release-cycle-001
  DomainId: software-development
  Analysis: "document" fires DOCUMENT. "write" in documentation context fires DOCUMENT.
    "release plan" and "milestones" are gap. DomainId: software-development.
    Gap warning: release-cycle intent not fully captured by DOCUMENT routing.

---

## B36-L2-015-004 — "release planning" five gap terms

A: "Finalize release plan, assign milestones to epics, track sprint velocity, manage team capacity."

B:
  Predicates: []
  GapIds: [gap-project-planning-release-cycle-001, gap-project-planning-milestone-001, gap-project-planning-epic-001, gap-project-planning-sprint-001, gap-project-planning-velocity-001, gap-project-planning-capacity-001]
  DomainId: project-management
  Analysis: Six gap terms, zero predicates. knowledge_edge.
    SCHEDULE_CAPABILITY_LEARNING with all six IDs.

---

## B36-L2-015-005 — "release plan" boundary: "create" with code entity fires IMPLEMENT

A: "Create the release service module and implement the deployment pipeline."

B:
  Predicates: [IMPLEMENT]
  GapId: null (no planning vocabulary in this request)
  DomainId: software-development
  Analysis: "create" + "module" fires IMPLEMENT. "implement" fires IMPLEMENT.
    No planning vocabulary present → no gap warnings.
    Contrasts with "Create the release plan" (no code entity → no IMPLEMENT).
    Teaching: "create" fires IMPLEMENT only when co-occurring with a code-level entity
    (module, service, function, class, component).
