# Layer 0-B — Router Pairs (013)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-013-001 — IMPLEMENT → ImplementationFlow41

A: Predicates: ["IMPLEMENT"] | Request: "Build the order fulfillment service."
Router:
  RoutingDecisions:
    - PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"] | Confidence: 0.92 | DecisionCount: 1
Routing rationale: IMPLEMENT alone → ImplementationFlow41 (sub-phase 4.1). No secondary flows.

---

## B15-router-013-002 — IMPLEMENT+API_CONTRACT → two flows

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] | Request: "Implement the fulfillment service and integrate with the carrier dispatch API."
Router:
  - ImplementationFlow41 (IMPLEMENT) — sub-phase 4.1
  - APIContractFlow33 (API_CONTRACT) — sub-phase 3.3
  DecisionCount: 2
  Ordering: APIContractFlow33 before ImplementationFlow41 (contract spec informs implementation).

---

## B15-router-013-003 — IMPLEMENT+ARCHITECTURE → two flows

A: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | Request: "Build the fulfillment service and design the domain model for packing workflow."
Router:
  - ArchitectureFlow31 (ARCHITECTURE) — sub-phase 3.1
  - ImplementationFlow41 (IMPLEMENT) — sub-phase 4.1
  DecisionCount: 2
  Ordering: ArchitectureFlow31 before ImplementationFlow41.

---

## B15-router-013-004 — v6-03 corruption: two decisions collapsed to one

v6 corrupt: Predicates: ["IMPLEMENT", "API_CONTRACT"] → DecisionCount: 1 (only ImplementationFlow41 returned)
Correct: DecisionCount: 2 — both ImplementationFlow41 and APIContractFlow33 must be returned.
Error: v6-03 collapse — dominant IMPLEMENT selected; API_CONTRACT routing dropped.

---

## B15-router-013-005 — IMPLEMENT+API_CONTRACT+ARCHITECTURE → three flows

A: Predicates: ["IMPLEMENT", "API_CONTRACT", "ARCHITECTURE"] | Request: "Design the fulfillment domain model, define the carrier API contracts, and implement the fulfillment service."
Router:
  - ArchitectureFlow31 (ARCHITECTURE) — sub-phase 3.1
  - APIContractFlow33 (API_CONTRACT) — sub-phase 3.3
  - ImplementationFlow41 (IMPLEMENT) — sub-phase 4.1
  DecisionCount: 3 | Ordering: 3.1 → 3.3 → 4.1
