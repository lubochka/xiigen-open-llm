# Layer 3 — Teaching Unit Pairs (020)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## B36-L3-020-001 — Sprint + IMPLEMENT: predicate dominance teaching

A:
  Question: "Sprint planning sentence with IMPLEMENT: 'Plan the sprint and build the authentication features.'
             What drives the routing? What is the gap?"

B:
  Answer: "IMPLEMENT drives the routing via 'build' and 'features'. 'Plan the sprint' is
    planning framing — gap vocabulary (sprint-001). The routing is feature_addition to 4.1.
    DomainId: software-development. Sprint planning intent is noted as a gap warning.
    The model is predicate-driven, not intent-driven."

---

## B36-L3-020-002 — Roadmap + IMPLEMENT: roadmap is always gap

A:
  Question: "'Implement the Q1 roadmap items.' Is 'roadmap' a predicate? Does it affect routing?"

B:
  Answer: "'roadmap' is gap vocabulary — it has no registered predicate. 'Implement' fires
    IMPLEMENT. The routing is to 4.1. 'Roadmap' produces a gap warning (roadmap-001) but
    does not affect the routing at all. The model must not invent a separate routing step
    for 'roadmap' context."

---

## B36-L3-020-003 — Epic + IMPLEMENT: epic framing does not add a sub-phase

A:
  Question: "'Implement the user authentication epic.' 'Epic' is present. Does it add a routing?"

B:
  Answer: "'epic' is gap vocabulary. It has no registered predicate. 'Implement' fires
    IMPLEMENT. Routing: 4.1. 'Epic' produces a gap warning (epic-001) but adds no sub-phase.
    There is no 'epic routing' in the current system. The model must not invent one."

---

## B36-L3-020-004 — Sprint + IMPLEMENT + DOCUMENT: parallel flows, sprint is gap

A:
  Question: "'This sprint: build the feature and document the requirements.' What executes?"

B:
  Answer: "Two predicates fire: IMPLEMENT (build, feature) and DOCUMENT (document, requirements).
    Two flows execute in parallel: universal-nine-node-development-flow-4.1 and
    universal-nine-node-development-flow-2.4. Sprint is a gap warning (sprint-001).
    DomainId: software-development. SCHEDULE_CAPABILITY_LEARNING: NOT emitted."
