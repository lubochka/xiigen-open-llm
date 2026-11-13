# Layer 2 — Capability Gap Extended Pairs (014)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B36-L2-014-001 — "effort estimation for the backlog" gap analysis

A: "How do we run effort estimation for the backlog before sprint planning?"

B:
  Predicates: []
  GapId: gap-project-planning-estimation-001
  DomainId: project-management
  Analysis: "effort estimation", "backlog", and "sprint" are all gap vocabulary.
    Zero predicates → knowledge_edge, DomainId: project-management.
    SCHEDULE_CAPABILITY_LEARNING emitted for estimation-001, backlog-001, sprint-001.

---

## B36-L2-014-002 — "effort estimation" + "user story" false positive

A: "Estimate the effort for each user story in the sprint backlog."

B:
  Predicates: [DOCUMENT]
  GapId: gap-project-planning-story-point-001
  DomainId: software-development
  Analysis: "story" in "user story" fires DOCUMENT. "effort estimation" and "backlog"
    and "sprint" are gap vocabulary but DOCUMENT fires via "story". DomainId: software-development.
    Gap warning for effort estimation intent not captured by DOCUMENT routing.

---

## B36-L2-014-003 — "estimate effort" + "implement" mixed

A: "Estimate the effort, then implement the top backlog items this sprint."

B:
  Predicates: [IMPLEMENT]
  GapId: gap-project-planning-estimation-001
  DomainId: software-development
  Analysis: "implement" fires IMPLEMENT. "estimate effort", "backlog", "sprint" are gap.
    IMPLEMENT dominates. Effort estimation and sprint are gap warnings.
    SCHEDULE_CAPABILITY_LEARNING NOT emitted (predicate fired).

---

## B36-L2-014-004 — "effort estimate" + "document" mixed

A: "Document the effort estimates and define acceptance criteria for all backlog items."

B:
  Predicates: [DOCUMENT]
  GapId: gap-project-planning-estimation-001
  DomainId: software-development
  Analysis: "document", "define", "acceptance criteria" all fire DOCUMENT. "effort estimates"
    and "backlog" are gap. DomainId: software-development. Gap warning for estimation intent.

---

## B36-L2-014-005 — Pure effort-estimate five gap terms

A: "Run effort-estimate session for epic backlog, set sprint capacity, groom before iteration."

B:
  Predicates: []
  GapIds: [gap-project-planning-estimation-001, gap-project-planning-epic-001, gap-project-planning-backlog-001, gap-project-planning-sprint-001, gap-project-planning-capacity-001, gap-project-planning-iteration-001]
  DomainId: project-management
  Analysis: Six gap terms, zero predicates. knowledge_edge.
    One SCHEDULE_CAPABILITY_LEARNING signal with all six IDs bundled.
