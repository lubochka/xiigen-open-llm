# Layer 0-B — Router Pairs (022)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-022-001 — IMPLEMENT (TransitionType: refactoring) → ImplementationFlow41

A: Predicates: ["IMPLEMENT"] | TransitionType: refactoring | Request: "Refactor the pricing engine to separate discount calculation from margin calculation."
Router:
  - ImplementationFlow41 | TransitionType: refactoring
  DecisionCount: 1

---

## B15-router-022-002 — IMPLEMENT+TEST (TransitionType: refactoring) → two flows

A: Predicates: ["IMPLEMENT", "TEST"] | TransitionType: refactoring | Request: "Refactor the pricing engine and write regression tests to verify pricing parity."
Router:
  - ImplementationFlow41 (refactoring mode) | TestingFlow51
  DecisionCount: 2 | Ordering: ImplementationFlow41 before TestingFlow51.

---

## B15-router-022-003 — IMPLEMENT+API_CONTRACT+TEST+DOCUMENT (TransitionType: refactoring) → four flows

A: Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"] | TransitionType: refactoring | Request: "Refactor the pricing engine, define pricing API contracts, write tests, and document pricing rules."
Router:
  - APIContractFlow33 | ImplementationFlow41 | TestingFlow51 | DocumentationFlow21
  DecisionCount: 4 | TransitionType: refactoring propagated.

---

## B15-router-022-004 — TransitionType: refactoring does not change flow routing destination

Teaching point: TransitionType: refactoring changes how ImplementationFlow41 executes (existing code refactored, not new code added), but it does NOT change the routing destination. ImplementationFlow41 is always the destination for IMPLEMENT — refactoring or feature_addition.

---

## B15-router-022-005 — v6-01 corruption: refactoring TransitionType lost in routing

v6 corrupt: "Refactor the pricing engine" → Router routes to ImplementationFlow41 | TransitionType: feature_addition (refactoring metadata lost)
Correct: TransitionType: refactoring must propagate from compiler through router to the flow selection.
Error: v6-01. TransitionType is a metadata field — it must survive the full routing pipeline, not be reset to default at the router stage.
