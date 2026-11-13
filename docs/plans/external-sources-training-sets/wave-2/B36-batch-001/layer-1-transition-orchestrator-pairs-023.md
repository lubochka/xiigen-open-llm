# Layer 1 — Transition Orchestrator Pairs (023)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B36-T1-023-001 — "retrospective" alone: unknown transition

A:
  run_id: "B36-T1-023-001"
  user_request: "Run the sprint retrospective and capture what went well."
  CompiledPredicateIds: []
  PredictedSubPhaseId: null

B:
  TransitionId: "transition-B36-T1-023-001-null-unknown"
  TransitionType: "unknown"
  PhaseId: null
  SubPhaseId: null
  DomainId: "project-management"
  PredicateIds: []
  ContextCarryForward: false
  EvidenceIds: ["gap-project-planning-retrospective-001", "gap-project-planning-sprint-001"]
  Metadata:
    transition_classification_basis: "no_predicate_matched: 'retrospective' is gap vocabulary, NOT QA"
    recommended_next_action: "SCHEDULE_CAPABILITY_LEARNING"

BOUNDARY TEACHING: TransitionType: "unknown" because "retrospective" is gap, not QA.
  "review" would produce TransitionType: "review" (QA). The model must know this distinction.

---

## B36-T1-023-002 — "review" in sprint context: review transition

A:
  run_id: "B36-T1-023-002"
  user_request: "Review the sprint outcomes and assess team performance."
  CompiledPredicateIds: ["QA"]
  PredictedSubPhaseId: "6.1"

B:
  TransitionId: "transition-B36-T1-023-002-6.1-review"
  TransitionType: "review"
  PhaseId: "6"
  SubPhaseId: "6.1"
  DomainId: "software-development"
  PredicateIds: ["QA"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-sprint-001"]

BOUNDARY TEACHING: "review" → TransitionType: "review", DomainId: "software-development".
  "assess" alone would not fire QA and would produce unknown + project-management.

---

## B36-T1-023-003 — "retrospective" + "review": review transition (retrospective is gap warning)

A:
  run_id: "B36-T1-023-003"
  user_request: "In the retrospective, review the code quality from the last sprint."
  CompiledPredicateIds: ["QA"]
  PredictedSubPhaseId: "6.1"

B:
  TransitionId: "transition-B36-T1-023-003-6.1-review"
  TransitionType: "review"
  PhaseId: "6"
  SubPhaseId: "6.1"
  DomainId: "software-development"
  PredicateIds: ["QA"]
  ContextCarryForward: true
  Metadata:
    transition_classification_basis: "QA predicate (review) classifies as review; 'retrospective' is gap vocabulary and does not alter the classification"
    capability_gap_warnings: ["gap-project-planning-retrospective-001: retrospective is gap, not QA", "gap-project-planning-sprint-001"]

---

## B36-T1-023-004 — Retrospective-only: unknown transition (confirms Rule 1)

A:
  run_id: "B36-T1-023-004"
  user_request: "Facilitate the team retrospective and identify process improvements for the next sprint."
  CompiledPredicateIds: []
  PredictedSubPhaseId: null

B:
  TransitionId: "transition-B36-T1-023-004-null-unknown"
  TransitionType: "unknown"
  PhaseId: null
  SubPhaseId: null
  DomainId: "project-management"
  PredicateIds: []
  ContextCarryForward: false
  EvidenceIds: ["gap-project-planning-retrospective-001", "gap-project-planning-sprint-001"]
  Metadata:
    transition_classification_basis: "no_predicate_matched: retrospective and sprint are gap vocabulary"
    recommended_next_action: "SCHEDULE_CAPABILITY_LEARNING"
