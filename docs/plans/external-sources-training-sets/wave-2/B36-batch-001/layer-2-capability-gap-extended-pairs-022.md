# Layer 2 — Capability Gap Extended Pairs (022)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: capability_gap_extended

---

## B36-L2-022-001 — "retrospective" boundary analysis: not QA

A: "Run the sprint retrospective and identify process improvements."

B:
  Predicates: []
  GapId: gap-project-planning-retrospective-001
  DomainId: project-management
  Analysis: "retrospective" is gap vocabulary, NOT in the QA lexicon. "run" and "identify"
    are not predicates. Zero predicates → knowledge_edge, DomainId: project-management.
    BOUNDARY TEACHING: "retrospective" ≠ QA.

---

## B36-L2-022-002 — "review" boundary analysis: IS QA

A: "Review the sprint outcomes and assess team velocity."

B:
  Predicates: [QA]
  GapId: gap-project-planning-sprint-001
  DomainId: software-development
  Analysis: "review" fires QA (always — even in sprint/velocity context). "sprint" and
    "velocity" are gap. QA fires. DomainId: software-development.
    BOUNDARY TEACHING: "review" = QA always.

---

## B36-L2-022-003 — "retrospective" + "review" in same sentence

A: "In the retrospective, review the completed sprint work."

B:
  Predicates: [QA]
  GapId: gap-project-planning-retrospective-001
  DomainId: software-development
  Analysis: "review" fires QA. "retrospective" does NOT fire QA — it is gap vocabulary.
    When both appear, only "review" drives the QA routing. "retrospective" is a gap
    warning. DomainId: software-development because QA fired.
    BOUNDARY TEACHING: "retrospective" never suppresses "review" and never fires QA.

---

## B36-L2-022-004 — "sprint review" vs "sprint retrospective"

A: "Compare: 'sprint review' vs 'sprint retrospective'. Which fires QA?"

B:
  Analysis:
    "sprint review":
      "review" fires QA (literal match). "sprint" is gap.
      Predicates: [QA], DomainId: software-development.

    "sprint retrospective":
      "retrospective" does NOT fire QA. "sprint" is gap.
      Predicates: [], DomainId: project-management.
      knowledge_edge — SCHEDULE_CAPABILITY_LEARNING emitted.

  BOUNDARY TEACHING: "sprint review" → QA. "sprint retrospective" → knowledge_edge.
    The Scrum terminology distinction is preserved in the lexicon: "review" is in QA;
    "retrospective" is not.

---

## B36-L2-022-005 — "retrospective" zero-predicate complete path summary

A: "Retrospective-only sentence: what is the complete orchestrator path?"

B:
  Input: "Facilitate the team retrospective and identify sprint improvements."
  Predicates: []
  DomainId: project-management
  OverallVerdict: KNOWLEDGE_EDGE
  TransitionType: unknown
  ContextCarryForward: false
  FinalHumanResult: null
  SCHEDULE_CAPABILITY_LEARNING: emitted
  GapIds: [gap-project-planning-retrospective-001, gap-project-planning-sprint-001]
  Summary: "retrospective" is gap. No sub-phase exists. Capability learning scheduled.
