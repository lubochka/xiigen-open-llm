# Layer 3 — Teaching Unit Pairs (017)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B36-L3-017-001 — Grooming: "groom" is gap, not a predicate

A:
  Question: "Does 'groom' fire any predicate? What about 'refine'?"

B:
  Answer: "'groom' is not in any predicate lexicon. It is gap vocabulary for backlog grooming.
    'refine' is also not in any predicate lexicon in the grooming context. Neither fires.
    'Groom the backlog' → zero predicates → knowledge-edge, DomainId: project-management."

---

## B36-L3-017-002 — Grooming: when DOCUMENT fires despite grooming intent

A:
  Question: "Grooming sentence: 'Groom the backlog and refine the user story acceptance criteria.'
             What fires and why?"

B:
  Answer: "DOCUMENT fires on 'story' (via 'user story') and on 'criteria' (via 'acceptance
    criteria'). 'groom' and 'backlog' are gap. DOCUMENT fires unconditionally via two
    separate lexicon tokens. CompiledPredicateIds: ['DOCUMENT']. The grooming intent is
    a gap warning. Routes to 2.1 (user stories sub-phase)."

---

## B36-L3-017-003 — Grooming with story false positive: SCHEDULE_CAPABILITY_LEARNING NOT emitted

A:
  Question: "Grooming + story-points sentence: is SCHEDULE_CAPABILITY_LEARNING emitted?"

B:
  Answer: "No. SCHEDULE_CAPABILITY_LEARNING is only emitted when CompiledPredicateIds: [].
    When DOCUMENT fires (via 'story' false positive), predicates are not empty. No
    SCHEDULE_CAPABILITY_LEARNING. Gap warnings are still logged as metadata and forwarded
    to AutonomousLearningPipeline separately."

---

## B36-L3-017-004 — Grooming + IMPLEMENT: predicate dominance over grooming intent

A:
  Question: "After grooming: 'implement the highest-priority backlog features.' What happens?"

B:
  Answer: "IMPLEMENT fires on 'implement' and 'features'. 'grooming' and 'backlog' are
    gap warnings. IMPLEMENT dominates. Routes to 4.1. DomainId: software-development.
    The grooming framing is captured only as a gap warning (backlog-001).
    The actual work (implementing features) is correctly routed."
