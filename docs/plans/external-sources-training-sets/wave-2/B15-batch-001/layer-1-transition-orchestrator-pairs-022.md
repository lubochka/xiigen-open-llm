# Layer 1 — Transition Orchestrator Pairs (022)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B15-t1-022-001 — PricingEngineService: refactoring TransitionType

A:
  user_request: "Refactor the pricing engine to separate discount and margin calculation into strategy classes."
  CompiledPredicateIds: ["IMPLEMENT"]
  TransitionType: "refactoring"

B:
  TransitionId: "transition-B15-t1-022-001-4.1-refactoring"
  TransitionType: "refactoring"
  SubPhaseId: "4.1"
  PredicateIds: ["IMPLEMENT"]
  Metadata:
    transition_classification_basis: "refactor verb present → TransitionType: refactoring; existing pricing engine being restructured, not new feature added"

---

## B15-t1-022-002 — PricingEngineService: refactoring + TEST

A:
  user_request: "Refactor the pricing engine and write regression tests to verify pricing parity."
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  TransitionType: "refactoring"

B:
  TransitionId: "transition-B15-t1-022-002-multi-refactoring"
  TransitionType: "refactoring"
  SubPhaseIds: ["4.1", "5.1"]
  PredicateIds: ["IMPLEMENT", "TEST"]
  Metadata:
    ordering: "ImplementationFlow41 (refactoring mode) before TestingFlow51"

---

## B15-t1-022-003 — PricingEngineService: refactoring + four predicates

A:
  user_request: "Refactor the pricing engine, define pricing API contracts, write tests, and document pricing rules."
  CompiledPredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
  TransitionType: "refactoring"

B:
  TransitionId: "transition-B15-t1-022-003-multi-refactoring"
  TransitionType: "refactoring"
  SubPhaseIds: ["3.3", "4.1", "5.1", "2.1"]
  PredicateIds: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
  Metadata:
    ordering: "APIContractFlow33 → ImplementationFlow41 (refactoring) → TestingFlow51 ‖ DocumentationFlow21"
    note: "TransitionType: refactoring propagates to ImplementationFlow41 only."
