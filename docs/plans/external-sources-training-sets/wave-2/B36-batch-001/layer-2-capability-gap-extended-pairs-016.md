# Layer 2 — Capability Gap Extended Pairs (016)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B36-L2-016-001 — "product roadmap presentation" gap analysis

A: "Present the product roadmap to the executive team and walk through Q3 milestones."

B:
  Predicates: []
  GapId: gap-project-planning-roadmap-001
  DomainId: project-management
  Analysis: "product roadmap" and "milestones" are gap vocabulary. "present" is not a
    predicate. Zero predicates → knowledge_edge, DomainId: project-management.
    SCHEDULE_CAPABILITY_LEARNING emitted for roadmap-001, milestone-001.

---

## B36-L2-016-002 — "product roadmap" + "document" mixed

A: "Document the product roadmap and specify requirements for each milestone."

B:
  Predicates: [DOCUMENT]
  GapId: gap-project-planning-roadmap-001
  DomainId: software-development
  Analysis: "document" and "specify" fire DOCUMENT. "requirements" fires DOCUMENT.
    "roadmap" and "milestones" are gap. DomainId: software-development.

---

## B36-L2-016-003 — "product roadmap" + "design" (ARCHITECTURE fires)

A: "Design the product roadmap structure and architect the delivery phases."

B:
  Predicates: [ARCHITECTURE]
  GapId: gap-project-planning-roadmap-001
  DomainId: software-development
  Analysis: "design" and "architect" fire ARCHITECTURE. "product roadmap" is gap.
    ARCHITECTURE routes to Phase 3 (Technical Design), not project planning.
    Gap warning for roadmap context not captured.

---

## B36-L2-016-004 — "product roadmap" five gap terms

A: "Align product roadmap with sprint velocity, epic priorities, burndown charts, and backlog milestones."

B:
  Predicates: []
  GapIds: [gap-project-planning-roadmap-001, gap-project-planning-sprint-001, gap-project-planning-velocity-001, gap-project-planning-epic-001, gap-project-planning-burndown-001, gap-project-planning-milestone-001, gap-project-planning-backlog-001]
  DomainId: project-management
  Analysis: Seven gap terms, zero predicates. knowledge_edge.
    One SCHEDULE_CAPABILITY_LEARNING with all seven IDs.

---

## B36-L2-016-005 — "roadmap" with "review" (QA fires, roadmap is gap)

A: "Review the product roadmap quarterly and approve the milestone roadmap items with stakeholders."

B:
  Predicates: [QA, DOCUMENT]
  GapId: gap-project-planning-roadmap-001
  DomainId: software-development
  Analysis: "review" fires QA. "approve" fires QA. "stakeholders" fires DOCUMENT.
    "roadmap" and "milestones" are gap. DomainId: software-development (QA fired).
    Gap warning for roadmap context. DomainId is software-development not project-management
    because QA and DOCUMENT fired.
