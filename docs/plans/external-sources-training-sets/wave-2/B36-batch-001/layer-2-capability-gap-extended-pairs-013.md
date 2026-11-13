# Layer 2 — Capability Gap Extended Pairs (013)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B36-L2-013-001 — "planning poker session" gap analysis

A: "How do I run a planning poker session to estimate the sprint backlog?"

B:
  Predicates: []
  GapId: gap-project-planning-sprint-001
  DomainId: project-management
  Analysis: "planning poker" and "sprint" are both gap vocabulary with no registered
    predicate. Zero predicates → knowledge_edge, DomainId: project-management.
    SCHEDULE_CAPABILITY_LEARNING emitted.

---

## B36-L2-013-002 — "planning poker" + "story points" mixed analysis

A: "Assign story points during planning poker for each backlog item."

B:
  Predicates: [DOCUMENT]
  GapId: gap-project-planning-story-point-001
  DomainId: software-development
  Analysis: "story" in "story points" fires DOCUMENT. "planning poker" and "backlog"
    are gap vocabulary but DOCUMENT fires first. DomainId: software-development because
    DOCUMENT fired. Gap warning emitted for story-point estimation intent.

---

## B36-L2-013-003 — "planning poker session" + "review results" mixed

A: "After planning poker, review the final estimates with the team lead."

B:
  Predicates: [QA]
  GapId: gap-project-planning-sprint-001
  DomainId: software-development
  Analysis: "review" fires QA. "planning poker" is gap. DomainId: software-development
    because QA fired. Gap warning for planning-poker context not captured.

---

## B36-L2-013-004 — "planning poker" + "implement" mixed

A: "Planning poker is done — now implement the authentication service."

B:
  Predicates: [IMPLEMENT]
  GapId: gap-project-planning-sprint-001
  DomainId: software-development
  Analysis: "implement" fires IMPLEMENT. "planning poker" is gap. IMPLEMENT dominates.
    DomainId: software-development. Planning poker context is gap warning only.

---

## B36-L2-013-005 — Pure planning-poker vocabulary: full gap path

A: "Facilitate a planning-poker session, estimate the epic backlog, and track iteration velocity."

B:
  Predicates: []
  GapIds: [gap-project-planning-epic-001, gap-project-planning-backlog-001, gap-project-planning-iteration-001, gap-project-planning-velocity-001]
  DomainId: project-management
  Analysis: Four gap terms. Zero predicates. knowledge_edge path.
    One SCHEDULE_CAPABILITY_LEARNING signal with all four gap IDs bundled.
