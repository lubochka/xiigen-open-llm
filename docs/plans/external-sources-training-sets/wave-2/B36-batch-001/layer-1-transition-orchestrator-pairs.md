# Layer 1 — Transition Orchestrator Pairs
# Batch: B36-batch-001
# Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Contract: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: transition_orchestrator

---

## Method note

For pure project planning gap vocabulary (no predicate), the transition
orchestrator produces a special "unknown" transition with null phase/sub-phase.
For mixed-signal requests (gap vocab + development predicates), the orchestrator
classifies based on the fired predicates, not the gap vocabulary.

---

## Pair B36B1-WDA-T1-001 — unknown transition: pure project planning gap

```
source_id: xiigen-b36-t1-001
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: unknown
phase: null
sub_phase: null
pair_kind: transition_orchestrator
```

A:
  run_id: "B36-t1-001"
  user_request: "We need to plan the next sprint and update the roadmap with the new milestones for Q3."
  CompiledPredicateIds: []
  PredictedSubPhaseId: null
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B36-t1-001-null-unknown"
  TransitionType: "unknown"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: null
  SubPhaseId: null
  DomainId: "project-management"
  UserRequest: "We need to plan the next sprint and update the roadmap with the new milestones for Q3."
  PredicateIds: []
  ContextCarryForward: false
  EvidenceIds: ["gap-project-planning-sprint-roadmap-001"]
  Metadata:
    transition_classification_basis: "no_predicate_matched: capability_gap vocabulary (sprint, roadmap, milestone) has no registered development predicate"
    primary_intent: "unknown"
    domain_signal: "project_planning_gap"
    capability_gap_ids: ["gap-project-planning-sprint-001", "gap-project-planning-roadmap-001", "gap-project-planning-milestone-001"]
    recommended_next_action: "SCHEDULE_CAPABILITY_LEARNING"

Transition rationale:
  Zero predicates → TransitionType: unknown. PhaseId and SubPhaseId: null.
  The domain is identified as "project-management" (not "software-development") based
  on the presence of gap vocabulary. ContextCarryForward: false because no nine-node
  flow will execute. Capability gap IDs are emitted for all three unmatched terms.

---

## Pair B36B1-WDA-T1-002 — feature_addition: sprint planning with IMPLEMENT co-signal

```
source_id: xiigen-b36-t1-002
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: feature_addition
phase: 4
sub_phase: 4.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B36-t1-002"
  user_request: "Plan the sprint to implement the checkout feature and build the payment integration module."
  CompiledPredicateIds: ["IMPLEMENT"]
  PredictedSubPhaseId: "4.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B36-t1-002-4.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  UserRequest: "Plan the sprint to implement the checkout feature and build the payment integration module."
  PredicateIds: ["IMPLEMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B36-compiler-003-prompt"]
  Metadata:
    transition_classification_basis: "IMPLEMENT predicate (implement, build, feature) classifies as feature_addition; 'sprint' and 'plan' are capability gap vocabulary and do not alter the classification"
    primary_intent: "create"
    domain_signal: "backend_implementation"
    capability_gap_warnings: ["gap-project-planning-sprint-not-captured"]

Transition rationale:
  The IMPLEMENT predicate dominates. "Plan the sprint" is gap vocabulary — it does
  not alter the transition type. The orchestrator routes based on fired predicates,
  not on gap vocabulary intent. This teaches that "sprint planning to implement X"
  is treated as a feature_addition transition for X, with sprint planning noted as
  an uncaptured capability gap.
