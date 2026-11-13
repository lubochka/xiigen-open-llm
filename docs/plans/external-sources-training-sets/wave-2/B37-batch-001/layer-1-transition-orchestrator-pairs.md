# Layer 1 — Transition Orchestrator Pairs
# Batch: B37-batch-001
# Scenario: B37 — Project managing — progress, coordination, and status reporting
# Contract: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: transition_orchestrator

---

## Method note

B37 follows the same orchestrator rules as B36. Pure project managing vocabulary
(no predicate) → TransitionType: unknown, PhaseId: null, SubPhaseId: null,
DomainId: "project-management". Mixed-signal (predicate co-occurs with gap vocab)
→ predicate determines the transition type; gap vocabulary produces warnings only.

---

## Pair B37B1-WDA-T1-001 — unknown transition: pure project managing gap

```
source_id: xiigen-b37-t1-001
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: unknown
phase: null
sub_phase: null
pair_kind: transition_orchestrator
```

A:
  run_id: "B37-t1-001"
  user_request: "Coordinate the cross-team dependencies and resolve the blockers preventing the release."
  CompiledPredicateIds: []
  PredictedSubPhaseId: null
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B37-t1-001-null-unknown"
  TransitionType: "unknown"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: null
  SubPhaseId: null
  DomainId: "project-management"
  UserRequest: "Coordinate the cross-team dependencies and resolve the blockers preventing the release."
  PredicateIds: []
  ContextCarryForward: false
  EvidenceIds: ["gap-project-managing-coordinate-blockers-001"]
  Metadata:
    transition_classification_basis: "no_predicate_matched: capability_gap vocabulary (coordinate, dependencies, blockers) has no registered development predicate"
    primary_intent: "unknown"
    domain_signal: "project_managing_gap"
    capability_gap_ids: ["gap-project-managing-coordinate-001", "gap-project-managing-blockers-001", "gap-project-managing-dependencies-team-001"]
    recommended_next_action: "SCHEDULE_CAPABILITY_LEARNING"

Transition rationale:
  Zero predicates → TransitionType: unknown. PhaseId and SubPhaseId: null.
  DomainId: "project-management" inferred from gap vocabulary. "dependencies" does
  NOT fire API_CONTRACT here because there is no API co-occurrence. ContextCarryForward:
  false because no nine-node flow will execute.

---

## Pair B37B1-WDA-T1-002 — feature_addition: IMPLEMENT with managing co-signal

```
source_id: xiigen-b37-t1-002
contract_signature: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
transition_type: feature_addition
phase: 4
sub_phase: 4.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B37-t1-002"
  user_request: "Manage the delivery of the authentication module — build the service, track progress, and escalate blockers."
  CompiledPredicateIds: ["IMPLEMENT"]
  PredictedSubPhaseId: "4.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B37-t1-002-4.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  UserRequest: "Manage the delivery of the authentication module — build the service, track progress, and escalate blockers."
  PredicateIds: ["IMPLEMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B37-compiler-003-prompt"]
  Metadata:
    transition_classification_basis: "IMPLEMENT predicate (build, service, module) classifies as feature_addition; 'manage', 'track progress', 'escalate blockers' are capability gap vocabulary and do not alter the classification"
    primary_intent: "create"
    domain_signal: "backend_implementation"
    capability_gap_warnings:
      - "gap-project-managing-track-progress-001"
      - "gap-project-managing-escalation-001"
      - "gap-project-managing-manage-delivery-001"

Transition rationale:
  IMPLEMENT predicate dominates. "Manage the delivery" is gap vocabulary — it does
  not alter the transition type. The orchestrator routes based on fired predicates,
  not on gap vocabulary intent. DomainId: "software-development" because IMPLEMENT
  fired (NOT "project-management" — that only applies when zero predicates fire).
  The managing vocabulary is noted as uncaptured capability gaps.
