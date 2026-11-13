# Layer 1 — Transition Orchestrator Pairs
# Batch: B15-batch-001
# Scenario: B15 — Development from scratch — backend order processing service
# Contract: TransitionOrchestratorUnit : ILogicalUnit<LogicalOrchestratorRequest, SelectedLogicalTransition>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: transition_orchestrator

---

## Pair B15B1-WDA-T1-001 — feature_addition: from-scratch backend service

```
source_id: xiigen-b15-t1-001
transition_type: feature_addition
phase: 4
sub_phase: 4.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B15-t1-001"
  user_request: "Build an order processing service from scratch including order creation, status tracking, and fulfillment logic."
  CompiledPredicateIds: ["IMPLEMENT"]
  PredictedSubPhaseId: "4.1"
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  TransitionId: "transition-B15-t1-001-4.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  UserRequest: "Build an order processing service from scratch including order creation, status tracking, and fulfillment logic."
  PredicateIds: ["IMPLEMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B15-compiler-001-prompt"]
  Metadata:
    transition_classification_basis: "IMPLEMENT predicate (build, service, create, logic) classifies as feature_addition; new service being created from scratch"
    primary_intent: "create"
    domain_signal: "backend_implementation"

---

## Pair B15B1-WDA-T1-002 — feature_addition: class-level implementation

```
source_id: xiigen-b15-t1-002
transition_type: feature_addition
phase: 4
sub_phase: 4.1
pair_kind: transition_orchestrator
```

A:
  run_id: "B15-t1-002"
  user_request: "Create the OrderRepository class and implement the CreateOrder, UpdateOrderStatus, and CancelOrder methods."
  CompiledPredicateIds: ["IMPLEMENT"]
  PredictedSubPhaseId: "4.1"

B:
  TransitionId: "transition-B15-t1-002-4.1-feature_addition"
  TransitionType: "feature_addition"
  ScenarioId: "adaptive-logical-orchestration"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  UserRequest: "Create the OrderRepository class and implement the CreateOrder, UpdateOrderStatus, and CancelOrder methods."
  PredicateIds: ["IMPLEMENT"]
  ContextCarryForward: true
  EvidenceIds: ["B15-compiler-003-prompt"]
  Metadata:
    transition_classification_basis: "IMPLEMENT predicate (create, class, implement, method) — high-density match classifies as feature_addition"
    primary_intent: "create"
    domain_signal: "backend_implementation"
