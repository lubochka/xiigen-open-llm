# Layer 3 — Teaching Unit Pairs (023)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B36-L3-023-001 — "retrospective" ≠ QA: the most important B36 boundary

A:
  Question: "What is the most important boundary to learn in B36 regarding ceremony vocabulary?"

B:
  Answer: "The retrospective/review boundary:
    - 'retrospective' is gap vocabulary. It NEVER fires QA. Zero predicates when alone.
    - 'review' fires QA. Always. Even in retrospective context.
    - When both appear: only 'review' fires QA. 'retrospective' is a gap warning.
    This is the canonical B36 boundary test. A model that confuses these fails v6-02 (predicate suppression) if it suppresses QA, or fails a new corruption type if it invents QA for 'retrospective'."

---

## B36-L3-023-002 — "sprint review" is a Scrum ceremony but "review" still fires QA

A:
  Question: "In Scrum, 'sprint review' is a ceremony. Does this prevent QA from firing?"

B:
  Answer: "No. The compiler is literal. 'review' in 'sprint review' fires QA regardless
    of the Scrum ceremony context. The human intent (demonstrating sprint work to stakeholders)
    is irrelevant to the predicate. QA fires. Routes to Phase 6. 'Sprint' is gap warning.
    This is the same principle: predicates dominate intent. No exception for Scrum ceremonies."

---

## B36-L3-023-003 — "retrospective" + "review" co-occurrence: only review drives routing

A:
  Question: "'In the retrospective, review the code quality.' Both terms present. What happens?"

B:
  Answer: "'review' fires QA and drives routing to Phase 6. 'retrospective' does NOT fire
    QA — it produces a gap warning (retrospective-001). The presence of 'retrospective'
    does not suppress 'review' or alter the QA routing. Two tokens, two behaviors:
    'review' → predicate; 'retrospective' → gap."

---

## B36-L3-023-004 — Pure retrospective = unknown transition (DomainId: project-management)

A:
  Question: "'Facilitate the team retrospective.' What TransitionType and DomainId?"

B:
  Answer: "TransitionType: unknown. DomainId: project-management. ContextCarryForward: false.
    'retrospective' and 'sprint' (if present) are gap. No predicate fires. FinalHumanResult: null.
    SCHEDULE_CAPABILITY_LEARNING emitted with retrospective-001 (and sprint-001 if present).
    Contrasts with: 'Review the sprint outcomes' → TransitionType: review, DomainId: software-development."
