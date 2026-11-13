# Layer 1 — Transition Orchestrator Pairs (019)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B36-T1-019-001 — "story" false positive DOCUMENT: documentation transition

A:
  run_id: "B36-T1-019-001"
  user_request: "Estimate the story points for each backlog item before the sprint starts."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"

B:
  TransitionId: "transition-B36-T1-019-001-2.1-documentation"
  TransitionType: "documentation"
  PhaseId: "2"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-story-point-001: DOCUMENT fires on 'story' substring; estimation intent not captured", "gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]

---

## B36-T1-019-002 — "user story" correct DOCUMENT: documentation transition

A:
  run_id: "B36-T1-019-002"
  user_request: "Write up the user story for the checkout flow and assign story points."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"

B:
  TransitionId: "transition-B36-T1-019-002-2.1-documentation"
  TransitionType: "documentation"
  PhaseId: "2"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-story-point-001: story-point assignment partially captured"]

---

## B36-T1-019-003 — "story" + retrospective + sprint: review+documentation transition

A:
  run_id: "B36-T1-019-003"
  user_request: "In the retrospective, review the story points assigned last sprint and update velocity."
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  PredictedSubPhaseId: "6.1"

B:
  TransitionId: "transition-B36-T1-019-003-6.1-review"
  TransitionType: "review"
  PhaseId: "6"
  SubPhaseId: "6.1"
  DomainId: "software-development"
  PredicateIds: ["QA", "DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    secondary_sub_phase: "2.1"
    capability_gap_warnings: ["gap-project-planning-retrospective-001: 'retrospective' is gap, NOT QA", "gap-project-planning-sprint-001", "gap-project-planning-velocity-001", "gap-project-planning-story-point-001"]
  Teaching: "retrospective" in Metadata.capability_gap_warnings confirms it is gap,
    not the driver of the QA transition. "review" drove the QA transition.

---

## B36-T1-019-004 — "stories" + backlog: documentation transition

A:
  run_id: "B36-T1-019-004"
  user_request: "Assign story points to all backlog stories before the sprint planning meeting."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"

B:
  TransitionId: "transition-B36-T1-019-004-2.1-documentation"
  TransitionType: "documentation"
  PhaseId: "2"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-story-point-001", "gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]
