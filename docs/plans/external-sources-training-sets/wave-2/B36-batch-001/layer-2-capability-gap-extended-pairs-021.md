# Layer 2 — Capability Gap Extended Pairs (021)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B36-L2-021-001 — "review the sprint deliverables" → QA fires

A: "Review the sprint deliverables and approve the completed features."

B:
  Predicates: [QA, IMPLEMENT]
  GapId: gap-project-planning-sprint-001
  DomainId: software-development
  Analysis: "review" fires QA. "approve" fires QA. "features" fires IMPLEMENT. "sprint"
    is gap. Two predicates fire. DomainId: software-development.

---

## B36-L2-021-002 — Sprint + QA only: "review sprint outcomes"

A: "Review the sprint outcomes and identify testing gaps in the delivery."

B:
  Predicates: [QA]
  GapId: gap-project-planning-sprint-001
  DomainId: software-development
  Analysis: "review" fires QA. "testing" fires QA in testing-gaps context. "sprint" is gap.
    DomainId: software-development.

---

## B36-L2-021-003 — Velocity + QA: "review velocity"

A: "Review the team velocity report and test whether the sprint met its targets."

B:
  Predicates: [QA, TEST]
  GapId: gap-project-planning-velocity-001
  DomainId: software-development
  Analysis: "review" fires QA. "test" fires TEST. "velocity" and "sprint" are gap.
    Two predicates fire. DomainId: software-development.

---

## B36-L2-021-004 — Backlog + QA + DOCUMENT: "review backlog with stakeholders"

A: "Review the prioritized backlog with stakeholders and approve the sprint scope."

B:
  Predicates: [QA, DOCUMENT]
  GapId: gap-project-planning-backlog-001
  DomainId: software-development
  Analysis: "review" fires QA. "approve" fires QA. "stakeholders" fires DOCUMENT.
    "backlog" and "sprint" are gap. Two predicates fire. DomainId: software-development.

---

## B36-L2-021-005 — Milestone + QA: "review milestone completion"

A: "Review milestone completion criteria and approve Q3 deliverables for sign-off."

B:
  Predicates: [QA, DOCUMENT]
  GapId: gap-project-planning-milestone-001
  DomainId: software-development
  Analysis: "review" fires QA. "approve" fires QA. "criteria" fires DOCUMENT. "milestone"
    is gap. DomainId: software-development. Gap warning for milestone-001.
