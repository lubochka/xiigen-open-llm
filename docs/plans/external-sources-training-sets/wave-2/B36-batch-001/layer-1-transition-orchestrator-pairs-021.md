# Layer 1 — Transition Orchestrator Pairs (021)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B36-T1-021-001 — Sprint + QA+IMPLEMENT: review transition (QA primary)

A:
  run_id: "B36-T1-021-001"
  user_request: "Review the sprint deliverables and approve the features before releasing to production."
  CompiledPredicateIds: ["QA", "IMPLEMENT"]
  PredictedSubPhaseId: "6.1"

B:
  TransitionId: "transition-B36-T1-021-001-6.1-review"
  TransitionType: "review"
  PhaseId: "6"
  SubPhaseId: "6.1"
  DomainId: "software-development"
  PredicateIds: ["QA", "IMPLEMENT"]
  ContextCarryForward: true
  Metadata:
    secondary_sub_phase: "4.1"
    execution_model: "sequential: 4.1 first, 6.1 after (QA-last)"
    capability_gap_warnings: ["gap-project-planning-sprint-001"]

---

## B36-T1-021-002 — Velocity + QA: review transition

A:
  run_id: "B36-T1-021-002"
  user_request: "Review the team velocity report from the last sprint and identify any testing gaps."
  CompiledPredicateIds: ["QA"]
  PredictedSubPhaseId: "6.1"

B:
  TransitionId: "transition-B36-T1-021-002-6.1-review"
  TransitionType: "review"
  PhaseId: "6"
  SubPhaseId: "6.1"
  DomainId: "software-development"
  PredicateIds: ["QA"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-velocity-001", "gap-project-planning-sprint-001"]

---

## B36-T1-021-003 — Milestone + QA + DOCUMENT: review transition (QA primary)

A:
  run_id: "B36-T1-021-003"
  user_request: "Review the milestone completion criteria and approve the Q3 deliverables for sign-off."
  CompiledPredicateIds: ["QA", "DOCUMENT"]
  PredictedSubPhaseId: "6.1"

B:
  TransitionId: "transition-B36-T1-021-003-6.1-review"
  TransitionType: "review"
  PhaseId: "6"
  SubPhaseId: "6.1"
  DomainId: "software-development"
  PredicateIds: ["QA", "DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    secondary_sub_phase: "2.4"
    execution_model: "sequential: 2.4 first, 6.1 after (QA-last)"
    capability_gap_warnings: ["gap-project-planning-milestone-001"]

---

## B36-T1-021-004 — Roadmap + QA + TEST: review transition

A:
  run_id: "B36-T1-021-004"
  user_request: "Review the product roadmap items and test whether the current sprint covers the Q3 goals."
  CompiledPredicateIds: ["QA", "TEST"]
  PredictedSubPhaseId: "6.1"

B:
  TransitionId: "transition-B36-T1-021-004-6.1-review"
  TransitionType: "review"
  PhaseId: "6"
  SubPhaseId: "6.1"
  DomainId: "software-development"
  PredicateIds: ["QA", "TEST"]
  ContextCarryForward: true
  Metadata:
    secondary_sub_phase: "5.1"
    execution_model: "sequential: 5.1 first, 6.1 after (QA-last)"
    capability_gap_warnings: ["gap-project-planning-roadmap-001", "gap-project-planning-sprint-001"]
