# Layer 1 — Transition Orchestrator Pairs (013)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-013-001 — OrderFulfillmentService: feature_addition

A:
  run_id: "B15-t1-013-001"
  user_request: "Build the order fulfillment service that picks, packs, and dispatches orders to the carrier."
  CompiledPredicateIds: ["IMPLEMENT"]
  PredictedSubPhaseId: "4.1"

B:
  TransitionId: "transition-B15-t1-013-001-4.1-feature_addition"
  TransitionType: "feature_addition"
  PhaseId: "4"
  SubPhaseId: "4.1"
  DomainId: "software-development"
  PredicateIds: ["IMPLEMENT"]
  Metadata:
    transition_classification_basis: "IMPLEMENT predicate (build, service) classifies as feature_addition; new service being created"
    domain_signal: "backend_implementation"

---

## B15-t1-013-002 — OrderFulfillmentService + carrier API: IMPLEMENT+API_CONTRACT

A:
  user_request: "Implement the fulfillment service and integrate with the carrier dispatch API."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  PredictedSubPhaseIds: ["4.1", "3.3"]

B:
  TransitionId: "transition-B15-t1-013-002-multi-feature_addition"
  TransitionType: "feature_addition"
  SubPhaseIds: ["3.3", "4.1"]
  PredicateIds: ["IMPLEMENT", "API_CONTRACT"]
  Metadata:
    ordering: "APIContractFlow33 before ImplementationFlow41"
    domain_signal: "backend_implementation + api_integration"

---

## B15-t1-013-003 — Refactoring TransitionType: does not apply to OrderFulfillmentService new build

Note: OrderFulfillmentService in B15 is built new (feature_addition). If a future request says "refactor the fulfillment service", TransitionType would become refactoring. For new-build requests, TransitionType is always feature_addition.
