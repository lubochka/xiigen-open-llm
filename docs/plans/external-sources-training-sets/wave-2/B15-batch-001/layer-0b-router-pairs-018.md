# Layer 0-B — Router Pairs (018)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-018-001 — IMPLEMENT → ImplementationFlow41

A: Predicates: ["IMPLEMENT"] | Request: "Build the search service that indexes products and returns ranked results."
Router:
  - PredictedSubPhaseId: "4.1" | Confidence: 0.92 | DecisionCount: 1

---

## B15-router-018-002 — IMPLEMENT+API_CONTRACT → two flows (Elasticsearch integration)

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] | Request: "Implement the product search service and integrate with the Elasticsearch index API."
Router:
  - APIContractFlow33 (API_CONTRACT) — sub-phase 3.3
  - ImplementationFlow41 (IMPLEMENT) — sub-phase 4.1
  DecisionCount: 2 | Ordering: 3.3 → 4.1.

---

## B15-router-018-003 — IMPLEMENT (TransitionType: refactoring) → ImplementationFlow41

A: Predicates: ["IMPLEMENT"] | TransitionType: refactoring | Request: "Refactor the search service to replace in-memory index with Elasticsearch-backed distributed index."
Router:
  - ImplementationFlow41 | TransitionType: refactoring
  DecisionCount: 1 | Note: TransitionType: refactoring does not change the routing destination — ImplementationFlow41 still executes; but the flow operates in refactoring mode.

---

## B15-router-018-004 — IMPLEMENT+API_CONTRACT (TransitionType: refactoring) → two flows

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] | TransitionType: refactoring | Request: "Refactor the search service and update the Elasticsearch API integration contracts."
Router:
  - APIContractFlow33 | ImplementationFlow41
  DecisionCount: 2 | TransitionType: refactoring propagates to ImplementationFlow41.

---

## B15-router-018-005 — v6-03 corruption: extend+integrate collapsed

v6 corrupt: "Extend the search service to support faceted filtering and integrate with Elasticsearch" → DecisionCount: 1 (only ImplementationFlow41)
Correct: Predicates: ["IMPLEMENT", "API_CONTRACT"] | DecisionCount: 2
Error: v6-03. "integrate" fires API_CONTRACT independently. The router must return two decisions.
