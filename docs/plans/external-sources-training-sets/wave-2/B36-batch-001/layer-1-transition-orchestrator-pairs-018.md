# Layer 1 — Transition Orchestrator Pairs (018)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B36-T1-018-001 — Prioritize/triage zero predicates: unknown transition

A:
  run_id: "B36-T1-018-001"
  user_request: "Prioritize the backlog items and triage the incoming tickets for the upcoming sprint."
  CompiledPredicateIds: []
  PredictedSubPhaseId: null

B:
  TransitionId: "transition-B36-T1-018-001-null-unknown"
  TransitionType: "unknown"
  PhaseId: null
  SubPhaseId: null
  DomainId: "project-management"
  PredicateIds: []
  ContextCarryForward: false
  EvidenceIds: ["gap-project-planning-backlog-001", "gap-project-planning-ticket-001", "gap-project-planning-sprint-001"]
  Metadata:
    transition_classification_basis: "no_predicate_matched: prioritize/triage vocabulary has no registered predicate"
    recommended_next_action: "SCHEDULE_CAPABILITY_LEARNING"

---

## B36-T1-018-002 — Triage + DOCUMENT: documentation transition

A:
  run_id: "B36-T1-018-002"
  user_request: "Triage the incoming backlog and define the priority criteria for each ticket type."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.4"

B:
  TransitionId: "transition-B36-T1-018-002-2.4-documentation"
  TransitionType: "documentation"
  PhaseId: "2"
  SubPhaseId: "2.4"
  DomainId: "software-development"
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-backlog-001", "gap-project-planning-ticket-001"]

---

## B36-T1-018-003 — Prioritize + QA: review transition

A:
  run_id: "B36-T1-018-003"
  user_request: "Review the prioritized backlog and approve the sprint scope for the team."
  CompiledPredicateIds: ["QA"]
  PredictedSubPhaseId: "6.1"

B:
  TransitionId: "transition-B36-T1-018-003-6.1-review"
  TransitionType: "review"
  PhaseId: "6"
  SubPhaseId: "6.1"
  DomainId: "software-development"
  PredicateIds: ["QA"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]

---

## B36-T1-018-004 — Triage five gap terms: unknown transition

A:
  run_id: "B36-T1-018-004"
  user_request: "Triage the epic backlog, prioritize the sprint tickets, and set velocity targets for the next iteration."
  CompiledPredicateIds: []
  PredictedSubPhaseId: null

B:
  TransitionId: "transition-B36-T1-018-004-null-unknown"
  TransitionType: "unknown"
  PhaseId: null
  SubPhaseId: null
  DomainId: "project-management"
  PredicateIds: []
  ContextCarryForward: false
  EvidenceIds: ["gap-project-planning-epic-001", "gap-project-planning-backlog-001", "gap-project-planning-sprint-001", "gap-project-planning-ticket-001", "gap-project-planning-velocity-001", "gap-project-planning-iteration-001"]
  Metadata:
    recommended_next_action: "SCHEDULE_CAPABILITY_LEARNING"
