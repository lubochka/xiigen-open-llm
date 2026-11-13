# Layer 1 — Transition Orchestrator Pairs (016)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B36-T1-016-001 — Product-roadmap zero predicates: unknown transition

A:
  run_id: "B36-T1-016-001"
  user_request: "Present the product roadmap to the board and walk them through the Q3 and Q4 milestones."
  CompiledPredicateIds: []
  PredictedSubPhaseId: null

B:
  TransitionId: "transition-B36-T1-016-001-null-unknown"
  TransitionType: "unknown"
  PhaseId: null
  SubPhaseId: null
  DomainId: "project-management"
  PredicateIds: []
  ContextCarryForward: false
  EvidenceIds: ["gap-project-planning-roadmap-001", "gap-project-planning-milestone-001"]
  Metadata:
    transition_classification_basis: "no_predicate_matched: product roadmap vocabulary has no registered predicate"
    recommended_next_action: "SCHEDULE_CAPABILITY_LEARNING"

---

## B36-T1-016-002 — Product-roadmap + ARCHITECTURE: architecture transition

A:
  run_id: "B36-T1-016-002"
  user_request: "Design the product roadmap structure and architect the delivery phases for each epic."
  CompiledPredicateIds: ["ARCHITECTURE"]
  PredictedSubPhaseId: "3.4"

B:
  TransitionId: "transition-B36-T1-016-002-3.4-architecture"
  TransitionType: "architecture"
  PhaseId: "3"
  SubPhaseId: "3.4"
  DomainId: "software-development"
  PredicateIds: ["ARCHITECTURE"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-roadmap-001", "gap-project-planning-epic-001"]

---

## B36-T1-016-003 — Product-roadmap + DOCUMENT: documentation transition

A:
  run_id: "B36-T1-016-003"
  user_request: "Document the product roadmap and specify the requirements for each milestone deliverable."
  CompiledPredicateIds: ["DOCUMENT"]
  PredictedSubPhaseId: "2.4"

B:
  TransitionId: "transition-B36-T1-016-003-2.4-documentation"
  TransitionType: "documentation"
  PhaseId: "2"
  SubPhaseId: "2.4"
  DomainId: "software-development"
  PredicateIds: ["DOCUMENT"]
  ContextCarryForward: true
  Metadata:
    capability_gap_warnings: ["gap-project-planning-roadmap-001", "gap-project-planning-milestone-001"]

---

## B36-T1-016-004 — Product-roadmap five gap terms: unknown transition

A:
  run_id: "B36-T1-016-004"
  user_request: "Align the product roadmap with sprint velocity, epic priorities, and burndown projections for Q3."
  CompiledPredicateIds: []
  PredictedSubPhaseId: null

B:
  TransitionId: "transition-B36-T1-016-004-null-unknown"
  TransitionType: "unknown"
  PhaseId: null
  SubPhaseId: null
  DomainId: "project-management"
  PredicateIds: []
  ContextCarryForward: false
  EvidenceIds: ["gap-project-planning-roadmap-001", "gap-project-planning-sprint-001", "gap-project-planning-velocity-001", "gap-project-planning-epic-001", "gap-project-planning-burndown-001"]
  Metadata:
    recommended_next_action: "SCHEDULE_CAPABILITY_LEARNING"
