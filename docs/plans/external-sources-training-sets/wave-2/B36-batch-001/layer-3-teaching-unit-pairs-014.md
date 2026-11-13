# Layer 3 — Teaching Unit Pairs (014)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B36-L3-014-001 — Effort-estimate: why it produces knowledge-edge

A:
  Question: "Why does 'provide an effort estimate for each backlog item' produce knowledge-edge?"

B:
  Answer: "'effort estimate', 'backlog', 'sprint' are all gap vocabulary with no registered
    predicate. 'estimate' is not DOCUMENT. 'provide' is not any predicate. Zero predicates
    → knowledge-edge, DomainId: project-management."

---

## B36-L3-014-002 — Effort-estimate gap ID: estimation-001

A:
  Question: "What gap ID is assigned to effort estimation requests?"

B:
  Answer: "gap-project-planning-estimation-001. This ID covers effort estimation,
    planning-poker estimation, and story-point estimation intent (when DOCUMENT has
    not fired via 'story'). The ID signals to AutonomousLearningPipeline that a
    project estimation capability is needed."

---

## B36-L3-014-003 — Effort-estimate false positive: "estimate effort for user stories"

A:
  Question: "A user says: 'Estimate the effort for each user story.' Does DOCUMENT fire?
             What gap warning is emitted?"

B:
  Answer: "Yes. DOCUMENT fires on 'story' in 'user story'. CompiledPredicateIds: ['DOCUMENT'].
    Gap warning: gap-project-planning-story-point-001 noting that 'DOCUMENT fires on story
    substring; effort estimation intent not captured'. The model MUST fire DOCUMENT and
    emit the gap warning. Do not suppress DOCUMENT to preserve knowledge-edge."

---

## B36-L3-014-004 — Effort-estimate + implement: predicate dominance

A:
  Question: "Why does 'estimate effort, then implement the payment feature' produce
             TransitionType: feature_addition and not knowledge-edge?"

B:
  Answer: "IMPLEMENT fires on 'implement' and 'feature'. Predicates dominate gap vocabulary.
    'estimate effort' is gap (estimation-001 warning emitted) but IMPLEMENT fires
    unconditionally. The routing is feature_addition to 4.1. DomainId: software-development.
    SCHEDULE_CAPABILITY_LEARNING is NOT emitted when any predicate fires."
