# Layer 2 — Capability Gap Extended Pairs (017)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B36-L2-017-001 — "groom the backlog" gap analysis

A: "Groom the backlog before our next sprint planning session."

B:
  Predicates: []
  GapId: gap-project-planning-backlog-001
  DomainId: project-management
  Analysis: "groom" and "backlog" and "sprint" are all gap vocabulary. Zero predicates
    → knowledge_edge, DomainId: project-management.
    SCHEDULE_CAPABILITY_LEARNING emitted for backlog-001, sprint-001.

---

## B36-L2-017-002 — "groom" + "story acceptance criteria" (DOCUMENT fires)

A: "Groom the backlog and refine user story acceptance criteria."

B:
  Predicates: [DOCUMENT]
  GapId: gap-project-planning-backlog-001
  DomainId: software-development
  Analysis: "story" fires DOCUMENT. "criteria" fires DOCUMENT. "groom" and "backlog"
    are gap. DomainId: software-development because DOCUMENT fired.

---

## B36-L2-017-003 — "backlog refinement" synonym: same as grooming

A: "Run the backlog refinement session and triage new tickets."

B:
  Predicates: []
  GapId: gap-project-planning-backlog-001
  DomainId: project-management
  Analysis: "backlog refinement" is synonymous with grooming — both are gap vocabulary.
    "triage" and "tickets" are also gap. Zero predicates → knowledge_edge.
    Gap IDs: backlog-001, ticket-001.

---

## B36-L2-017-004 — "groom" + "implement" (IMPLEMENT dominates)

A: "Grooming complete — implement the top backlog items this sprint."

B:
  Predicates: [IMPLEMENT]
  GapId: gap-project-planning-backlog-001
  DomainId: software-development
  Analysis: "implement" fires IMPLEMENT. "grooming", "backlog", "sprint" are gap.
    IMPLEMENT dominates. Gap warnings for grooming/backlog/sprint.
    DomainId: software-development.

---

## B36-L2-017-005 — Grooming five gap terms

A: "Facilitate backlog grooming, triage epic tickets, estimate story-points, groom before sprint."

B:
  Predicates: [DOCUMENT]
  Note: "story" in "story-points" fires DOCUMENT.
  GapId: gap-project-planning-story-point-001
  DomainId: software-development
  Analysis: DOCUMENT fires on "story" substring. All other terms (facilitate, grooming,
    triage, epic, tickets, sprint) are gap. DomainId: software-development because
    DOCUMENT fired. Gap warnings for backlog, epic, ticket, story-point, sprint.
