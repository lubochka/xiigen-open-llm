# Layer 1 — Transition Orchestrator Pairs (017)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B36-T1-017-001 — Grooming zero predicates: unknown transition

A:
  run_id: "B36-T1-017-001"
  user_request: "Groom the product backlog before the next sprint planning session."
  CompiledPredicateIds: []
  PredictedSubPhaseId: null

B:
  TransitionId: "transition-B36-T1-017-001-null-unknown"
  TransitionType: "unknown"
  PhaseId: null
  SubPhaseId: null
  DomainId: "project-management"
  PredicateIds: []
  ContextCarryForward: false
  EvidenceIds: ["gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]
  Metadata:
    transition_classification_basis: "no_predicate_matched: grooming/backlog vocabulary has no registered predicate"
    recommended_next_action: "SCHEDULE_CAPABILITY_LEARNING"

---

## B36-T1-017-002 — Refinement zero predicates: unknown transition

A:
  run_id: "B36-T1-017-002"
  user_request: "Run the backlog refinement meeting and reprioritize the tickets for the upcoming iteration."
  CompiledPredicateIds: []
  PredictedSubPhaseId: null

B:
  TransitionId: "transition-B36-T1-017-002-null-unknown"
  TransitionType: "unknown"
  PhaseId: null
  SubPhaseId: null
  DomainId: "project-management"
  PredicateIds: []
  ContextCarryForward: false
  EvidenceIds: ["gap-project-planning-backlog-001", "gap-project-planning-ticket-001", "gap-project-planning-iteration-001"]
  Metadata:
    recommended_next_action: "SCHEDULE_CAPABILITY_LEARNING"

---

## B36-T1-017-003 — Grooming + DOCUMENT (story+criteria): documentation transition

A:
  run_id: "B36-T1-017-003"
  user_request: "Groom the backlog and refine the user story acceptance criteria for each ticket."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"

B:
  TransitionId: "transition-B36-T1-017-003-2.1-documentation"
  TransitionType: "documentation"
  PhaseId: "2"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-backlog-001", "gap-project-planning-ticket-001"]

---

## B36-T1-017-004 — Grooming + IMPLEMENT: feature_addition transition

A:
  run_id: "B36-T1-017-004"
  user_request: "After grooming, implement the highest-priority backlog features in the current sprint."
  CompiledPredicateIds: ["IMPLEMENT"]
  PredictedSubPhaseId: "4.1"

B:
  TransitionId: "transition-B36-T1-017-004-4.1-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  PredicateIds: ["IMPLEMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]
