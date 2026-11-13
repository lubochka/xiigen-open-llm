# Layer 1 — Transition Orchestrator Pairs (014)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B36-T1-014-001 — Effort-estimate zero predicates: unknown transition

A:
  run_id: "B36-T1-014-001"
  user_request: "Provide an effort estimate for each item in the backlog before we commit to the sprint."
  CompiledPredicateIds: []
  PredictedSubPhaseId: null

B:
  TransitionId: "transition-B36-T1-014-001-null-unknown"
  TransitionType: "unknown"
  PhaseId: null
  SubPhaseId: null
  DomainId: "project-management"
  PredicateIds: []
  ContextCarryForward: false
  EvidenceIds: ["gap-project-planning-estimation-001", "gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]
  Metadata:
    transition_classification_basis: "no_predicate_matched: effort estimation vocabulary has no registered predicate"
    recommended_next_action: "SCHEDULE_CAPABILITY_LEARNING"

---

## B36-T1-014-002 — Effort-estimate + DOCUMENT (story fires): documentation transition

A:
  run_id: "B36-T1-014-002"
  user_request: "Estimate the effort for each user story in the backlog and record the story point values."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"

B:
  TransitionId: "transition-B36-T1-014-002-2.1-documentation"
  TransitionType: "documentation"
  PhaseId: "2"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-story-point-001: DOCUMENT fires on 'story' substring", "gap-project-planning-backlog-001"]

---

## B36-T1-014-003 — Effort-estimate + IMPLEMENT: feature_addition transition

A:
  run_id: "B36-T1-014-003"
  user_request: "Estimate the effort required to implement the payment gateway feature before the sprint starts."
  CompiledPredicateIds: ["IMPLEMENT"]
  PredictedSubPhaseId: "4.1"

B:
  TransitionId: "transition-B36-T1-014-003-4.1-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  PredicateIds: ["IMPLEMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-estimation-001", "gap-project-planning-sprint-001"]

---

## B36-T1-014-004 — Effort-estimate five gap terms: unknown transition

A:
  run_id: "B36-T1-014-004"
  user_request: "Run a team effort-estimate session for the epic backlog and align on sprint capacity before grooming."
  CompiledPredicateIds: []
  PredictedSubPhaseId: null

B:
  TransitionId: "transition-B36-T1-014-004-null-unknown"
  TransitionType: "unknown"
  PhaseId: null
  SubPhaseId: null
  DomainId: "project-management"
  PredicateIds: []
  ContextCarryForward: false
  EvidenceIds: ["gap-project-planning-estimation-001", "gap-project-planning-epic-001", "gap-project-planning-backlog-001", "gap-project-planning-sprint-001", "gap-project-planning-capacity-001"]
  Metadata:
    recommended_next_action: "SCHEDULE_CAPABILITY_LEARNING"
