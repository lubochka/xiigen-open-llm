# Layer 1 — Transition Orchestrator Pairs (013)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B36-T1-013-001 — Planning-poker zero predicates: unknown transition

A:
  run_id: "B36-T1-013-001"
  user_request: "Let's run a planning-poker session to estimate the backlog items for the next sprint."
  CompiledPredicateIds: []
  PredictedSubPhaseId: null

B:
  TransitionId: "transition-B36-T1-013-001-null-unknown"
  TransitionType: "unknown"
  PhaseId: null
  SubPhaseId: null
  DomainId: "project-management"
  PredicateIds: []
  ContextCarryForward: false
  EvidenceIds: ["gap-project-planning-sprint-001", "gap-project-planning-backlog-001"]
  Metadata:
    transition_classification_basis: "no_predicate_matched: planning-poker and estimation vocabulary has no registered predicate"
    capability_gap_ids: ["gap-project-planning-sprint-001", "gap-project-planning-backlog-001"]
    recommended_next_action: "SCHEDULE_CAPABILITY_LEARNING"

---

## B36-T1-013-002 — Planning-poker + DOCUMENT: documentation transition

A:
  run_id: "B36-T1-013-002"
  user_request: "Run planning poker and assign story point estimates for each backlog item."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"

B:
  TransitionId: "transition-B36-T1-013-002-2.1-documentation"
  TransitionType: "documentation"
  PhaseId: "2"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-story-point-001", "gap-project-planning-backlog-001"]

---

## B36-T1-013-003 — Planning-poker + QA: review transition

A:
  run_id: "B36-T1-013-003"
  user_request: "Review the planning poker results and finalize the sprint estimate."
  CompiledPredicateIds: ["QA"]
  PredictedSubPhaseId: "6.1"

B:
  TransitionId: "transition-B36-T1-013-003-6.1-review"
  TransitionType: "review"
  PhaseId: "6"
  SubPhaseId: "6.1"
  DomainId: "software-development"
  PredicateIds: ["QA"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-sprint-001"]

---

## B36-T1-013-004 — Planning-poker + IMPLEMENT: feature_addition transition

A:
  run_id: "B36-T1-013-004"
  user_request: "After planning poker, implement the top-priority backlog features for the sprint."
  CompiledPredicateIds: ["IMPLEMENT"]
  PredictedSubPhaseId: "4.1"

B:
  TransitionId: "transition-B36-T1-013-004-4.1-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  PredicateIds: ["IMPLEMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]
