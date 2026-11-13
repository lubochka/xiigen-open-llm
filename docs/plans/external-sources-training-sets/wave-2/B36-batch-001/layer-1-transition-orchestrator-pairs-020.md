# Layer 1 — Transition Orchestrator Pairs (020)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B36-T1-020-001 — Sprint + IMPLEMENT: feature_addition transition

A:
  run_id: "B36-T1-020-001"
  user_request: "Plan the sprint and build the authentication features scheduled for this iteration."
  CompiledPredicateIds: ["IMPLEMENT"]
  PredictedSubPhaseId: "4.1"

B:
  TransitionId: "transition-B36-T1-020-001-4.1-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  PredicateIds: ["IMPLEMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-sprint-001", "gap-project-planning-iteration-001"]

---

## B36-T1-020-002 — Roadmap + IMPLEMENT: feature_addition

A:
  run_id: "B36-T1-020-002"
  user_request: "Implement the first quarter roadmap items: build the API service and create the database module."
  CompiledPredicateIds: ["IMPLEMENT"]
  PredictedSubPhaseId: "4.1"

B:
  TransitionId: "transition-B36-T1-020-002-4.1-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  PredicateIds: ["IMPLEMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-roadmap-001"]

---

## B36-T1-020-003 — Epic + IMPLEMENT: feature_addition

A:
  run_id: "B36-T1-020-003"
  user_request: "In this sprint, implement the user authentication epic: build the login module and create the token service."
  CompiledPredicateIds: ["IMPLEMENT"]
  PredictedSubPhaseId: "4.1"

B:
  TransitionId: "transition-B36-T1-020-003-4.1-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  PredicateIds: ["IMPLEMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-epic-001", "gap-project-planning-sprint-001"]

---

## B36-T1-020-004 — Sprint + IMPLEMENT + DOCUMENT: feature_addition (primary) + documentation

A:
  run_id: "B36-T1-020-004"
  user_request: "For this sprint: build the notification service, implement the email feature, and document the API requirements."
  CompiledPredicateIds: ["IMPLEMENT", "DOCUMENT"]
  PredictedSubPhaseId: "4.1"

B:
  TransitionId: "transition-B36-T1-020-004-4.1-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  PredicateIds: ["IMPLEMENT", "DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    secondary_sub_phase: "2.4"
    execution_model: "parallel"
    capability_gap_warnings: ["gap-project-planning-sprint-001"]
  Note: IMPLEMENT is the primary predicate (feature_addition TransitionType). DOCUMENT
    is secondary. Sprint is gap warning. DomainId: software-development.
