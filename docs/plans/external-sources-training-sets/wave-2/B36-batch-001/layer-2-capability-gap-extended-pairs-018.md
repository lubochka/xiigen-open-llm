# Layer 2 — Capability Gap Extended Pairs (018)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B36-L2-018-001 — "prioritize the sprint" gap analysis

A: "Prioritize the sprint backlog and triage the incoming tickets."

B:
  Predicates: []
  GapId: gap-project-planning-backlog-001
  DomainId: project-management
  Analysis: "prioritize", "sprint", "backlog", "triage", "tickets" are all gap vocabulary.
    Zero predicates → knowledge_edge, DomainId: project-management.
    SCHEDULE_CAPABILITY_LEARNING emitted for sprint-001, backlog-001, ticket-001.

---

## B36-L2-018-002 — "triage" + "define criteria" (DOCUMENT fires)

A: "Triage the backlog and define priority criteria for each ticket type."

B:
  Predicates: [DOCUMENT]
  GapId: gap-project-planning-ticket-001
  DomainId: software-development
  Analysis: "define" fires DOCUMENT. "criteria" fires DOCUMENT. "triage", "backlog",
    "ticket" are gap. DomainId: software-development because DOCUMENT fired.

---

## B36-L2-018-003 — "prioritize" + "review" (QA fires)

A: "Review the prioritized backlog and approve the sprint scope."

B:
  Predicates: [QA]
  GapId: gap-project-planning-backlog-001
  DomainId: software-development
  Analysis: "review" fires QA. "approve" fires QA. "prioritized backlog" and "sprint"
    are gap. QA dominates. Gap warnings for backlog-001, sprint-001.

---

## B36-L2-018-004 — "triage" six gap terms

A: "Triage epic backlog, prioritize sprint tickets, set velocity targets, plan next iteration."

B:
  Predicates: []
  GapIds: [gap-project-planning-epic-001, gap-project-planning-backlog-001, gap-project-planning-sprint-001, gap-project-planning-ticket-001, gap-project-planning-velocity-001, gap-project-planning-iteration-001]
  DomainId: project-management
  Analysis: Six gap terms, zero predicates. knowledge_edge.
    One SCHEDULE_CAPABILITY_LEARNING with all six IDs bundled.

---

## B36-L2-018-005 — "prioritize" + "build" (IMPLEMENT fires)

A: "Prioritize the backlog and build the top three features this sprint."

B:
  Predicates: [IMPLEMENT]
  GapId: gap-project-planning-backlog-001
  DomainId: software-development
  Analysis: "build" fires IMPLEMENT. "features" fires IMPLEMENT. "prioritize", "backlog",
    "sprint" are gap. IMPLEMENT dominates. DomainId: software-development.
    Gap warnings for backlog-001, sprint-001.
