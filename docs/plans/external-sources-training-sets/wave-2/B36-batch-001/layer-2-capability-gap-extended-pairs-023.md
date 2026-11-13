# Layer 2 — Capability Gap Extended Pairs (023)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B36-L2-023-001 — Multiple planning terms + IMPLEMENT: mixed case

A: "Plan the sprint, groom the backlog, then implement the highest-priority features."

B:
  Predicates: [IMPLEMENT]
  GapIds: [gap-project-planning-sprint-001, gap-project-planning-backlog-001]
  DomainId: software-development
  Analysis: "implement" fires IMPLEMENT. "features" fires IMPLEMENT. "sprint", "groom",
    "backlog" are all gap. IMPLEMENT dominates. DomainId: software-development.
    Gap warnings for sprint-001, backlog-001.

---

## B36-L2-023-002 — Multiple planning terms + QA + IMPLEMENT

A: "Review the sprint, implement the fixes, and update the backlog velocity."

B:
  Predicates: [QA, IMPLEMENT]
  GapIds: [gap-project-planning-sprint-001, gap-project-planning-backlog-001, gap-project-planning-velocity-001]
  DomainId: software-development
  Analysis: "review" fires QA. "implement" fires IMPLEMENT. "sprint", "backlog", "velocity"
    are gap. Two predicates fire. DomainId: software-development.
    QA-last execution ordering applies: IMPLEMENT flow runs before QA flow.

---

## B36-L2-023-003 — Multiple planning terms + DOCUMENT

A: "Document the sprint milestones and define backlog requirements for each epic."

B:
  Predicates: [DOCUMENT]
  GapIds: [gap-project-planning-sprint-001, gap-project-planning-milestone-001, gap-project-planning-backlog-001, gap-project-planning-epic-001]
  DomainId: software-development
  Analysis: "document" fires DOCUMENT. "define" fires DOCUMENT. "requirements" fires DOCUMENT.
    "sprint", "milestones", "backlog", "epic" are all gap. DomainId: software-development.

---

## B36-L2-023-004 — Multiple planning terms + ARCHITECTURE

A: "Design the sprint architecture, structure the backlog epics, and plan the milestone roadmap."

B:
  Predicates: [ARCHITECTURE]
  GapIds: [gap-project-planning-sprint-001, gap-project-planning-backlog-001, gap-project-planning-epic-001, gap-project-planning-milestone-001, gap-project-planning-roadmap-001]
  DomainId: software-development
  Analysis: "design" fires ARCHITECTURE. "structure" fires ARCHITECTURE. Five gap terms.
    ARCHITECTURE dominates. DomainId: software-development.

---

## B36-L2-023-005 — Six planning terms + all three predicates: IMPLEMENT + DOCUMENT + QA

A: "Sprint planning: review the backlog, document the requirements, build the features, and test the velocity metrics."

B:
  Predicates: [QA, DOCUMENT, IMPLEMENT, TEST]
  GapIds: [gap-project-planning-sprint-001, gap-project-planning-backlog-001, gap-project-planning-velocity-001]
  DomainId: software-development
  Analysis: "review" fires QA. "document" fires DOCUMENT. "requirements" fires DOCUMENT.
    "build" fires IMPLEMENT. "features" fires IMPLEMENT. "test" fires TEST.
    "sprint", "backlog", "velocity" are gap. Four predicates fire. DomainId: software-development.
    Execution: IMPLEMENT + DOCUMENT parallel, then TEST, then QA (QA-last).
