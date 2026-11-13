# Layer 1 — Transition Orchestrator Pairs (022)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B36-T1-022-001 — Roadmap + DOCUMENT: documentation transition

A:
  run_id: "B36-T1-022-001"
  user_request: "Document the roadmap items and specify the requirements for each quarterly milestone."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.4"

B:
  TransitionId: "transition-B36-T1-022-001-2.4-documentation"
  TransitionType: "documentation"
  PhaseId: "2"
  SubPhaseId: "2.4"
  DomainId: "software-development"
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-roadmap-001", "gap-project-planning-milestone-001"]

---

## B36-T1-022-002 — Backlog + DOCUMENT (functional+spec): documentation transition

A:
  run_id: "B36-T1-022-002"
  user_request: "Define the backlog requirements and document the functional specifications for each epic."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"

B:
  TransitionId: "transition-B36-T1-022-002-2.1-documentation"
  TransitionType: "documentation"
  PhaseId: "2"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-backlog-001", "gap-project-planning-epic-001"]

---

## B36-T1-022-003 — Velocity + DOCUMENT: documentation transition

A:
  run_id: "B36-T1-022-003"
  user_request: "Document the team velocity metrics and specify how they relate to the sprint capacity."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"

B:
  TransitionId: "transition-B36-T1-022-003-2.1-documentation"
  TransitionType: "documentation"
  PhaseId: "2"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-velocity-001", "gap-project-planning-sprint-001", "gap-project-planning-capacity-001"]

---

## B36-T1-022-004 — Milestone + DOCUMENT (use-case): documentation transition

A:
  run_id: "B36-T1-022-004"
  user_request: "Describe the deliverables for each roadmap milestone and define the use-case for each major epic."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.1"

B:
  TransitionId: "transition-B36-T1-022-004-2.1-documentation"
  TransitionType: "documentation"
  PhaseId: "2"
  SubPhaseId: "2.1"
  DomainId: "software-development"
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-milestone-001", "gap-project-planning-roadmap-001", "gap-project-planning-epic-001"]
