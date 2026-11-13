# Layer 0-B — Router Pairs (014)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-014-001 — IMPLEMENT → ImplementationFlow41

A: Predicates: ["IMPLEMENT"] | Request: "Implement the refund and chargeback service."
Router:
  RoutingDecisions:
    - PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"] | Confidence: 0.92 | DecisionCount: 1

---

## B15-router-014-002 — IMPLEMENT+API_CONTRACT → two flows

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] | Request: "Implement the refund service and wire up the payment gateway reversal API."
Router:
  - APIContractFlow33 (API_CONTRACT) — sub-phase 3.3
  - ImplementationFlow41 (IMPLEMENT) — sub-phase 4.1
  DecisionCount: 2 | Ordering: 3.3 before 4.1 (payment API contract defines integration surface for refund implementation).

---

## B15-router-014-003 — IMPLEMENT+TEST → two flows

A: Predicates: ["IMPLEMENT", "TEST"] | Request: "Implement refund logic and write unit tests for partial refunds, full refunds, and chargeback disputes."
Router:
  - ImplementationFlow41 (IMPLEMENT) — sub-phase 4.1
  - TestingFlow51 (TEST) — sub-phase 5.1
  DecisionCount: 2 | Ordering: ImplementationFlow41 before TestingFlow51 (implement before testing).

---

## B15-router-014-004 — IMPLEMENT+ARCHITECTURE → two flows

A: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | Request: "Build the chargeback handling service and define the domain model for chargeback states."
Router:
  - ArchitectureFlow31 (ARCHITECTURE) — sub-phase 3.1
  - ImplementationFlow41 (IMPLEMENT) — sub-phase 4.1
  DecisionCount: 2 | Ordering: 3.1 → 4.1.

---

## B15-router-014-005 — Refund four-predicate routing

A: Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"] | Request: "Implement refund service, define payment reversal API contracts, write tests, and document the refund API."
Router:
  - APIContractFlow33 | ImplementationFlow41 | TestingFlow51 | DocumentationFlow21
  DecisionCount: 4 | Ordering: 3.3 → 4.1 → 5.1 ‖ 2.1
