# Layer 1 — Transition Orchestrator Pairs (015)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B36-T1-015-001 — Release-planning zero predicates: unknown transition

A:
  run_id: "B36-T1-015-001"
  user_request: "Create the release plan for Q4 and map each milestone to a sprint delivery date."
  CompiledPredicateIds: []
  PredictedSubPhaseId: null

B:
  TransitionId: "transition-B36-T1-015-001-null-unknown"
  TransitionType: "unknown"
  PhaseId: null
  SubPhaseId: null
  DomainId: "project-management"
  PredicateIds: []
  ContextCarryForward: false
  EvidenceIds: ["gap-project-planning-release-cycle-001", "gap-project-planning-milestone-001", "gap-project-planning-sprint-001"]
  Metadata:
    transition_classification_basis: "no_predicate_matched: release planning vocabulary has no registered predicate"
    recommended_next_action: "SCHEDULE_CAPABILITY_LEARNING"

---

## B36-T1-015-002 — Release-planning + QA+DOCUMENT: review transition

A:
  run_id: "B36-T1-015-002"
  user_request: "Review the release plan and confirm the milestone dates with the stakeholders."
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  PredictedSubPhaseId: "6.1"

B:
  TransitionId: "transition-B36-T1-015-002-6.1-review"
  TransitionType: "review"
  PhaseId: "6"
  SubPhaseId: "6.1"
  DomainId: "software-development"
  PredicateIds: ["QA", "DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    secondary_sub_phase: "2.4"
    capability_gap_warnings: ["gap-project-planning-release-cycle-001", "gap-project-planning-milestone-001"]

---

## B36-T1-015-003 — Release-planning + IMPLEMENT: feature_addition transition

A:
  run_id: "B36-T1-015-003"
  user_request: "Release planning complete — now build the authentication module and implement the login flow."
  CompiledPredicateIds: ["IMPLEMENT"]
  PredictedSubPhaseId: "4.1"

B:
  TransitionId: "transition-B36-T1-015-003-4.1-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  PredicateIds: ["IMPLEMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-release-cycle-001"]

---

## B36-T1-015-004 — Release-planning five gap terms: unknown transition

A:
  run_id: "B36-T1-015-004"
  user_request: "Finalize the release plan timeline, assign milestones to epics, and track sprint velocity against the target."
  CompiledPredicateIds: []
  PredictedSubPhaseId: null

B:
  TransitionId: "transition-B36-T1-015-004-null-unknown"
  TransitionType: "unknown"
  PhaseId: null
  SubPhaseId: null
  DomainId: "project-management"
  PredicateIds: []
  ContextCarryForward: false
  EvidenceIds: ["gap-project-planning-release-cycle-001", "gap-project-planning-milestone-001", "gap-project-planning-epic-001", "gap-project-planning-sprint-001", "gap-project-planning-velocity-001"]
  Metadata:
    recommended_next_action: "SCHEDULE_CAPABILITY_LEARNING"
