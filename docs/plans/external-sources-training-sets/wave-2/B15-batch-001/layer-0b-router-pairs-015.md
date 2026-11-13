# Layer 0-B — Router Pairs (015)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: router

---

## B15-router-015-001 — IMPLEMENT → ImplementationFlow41

A: Predicates: ["IMPLEMENT"] | Request: "Create the inventory allocation service."
Router:
  - PredictedSubPhaseId: "4.1" | EvidencePredicates: ["IMPLEMENT"] | Confidence: 0.92 | DecisionCount: 1

---

## B15-router-015-002 — IMPLEMENT+API_CONTRACT → two flows

A: Predicates: ["IMPLEMENT", "API_CONTRACT"] | Request: "Implement the inventory allocation service and define the data contracts for stock reservation requests."
Router:
  - APIContractFlow33 (API_CONTRACT) — sub-phase 3.3
  - ImplementationFlow41 (IMPLEMENT) — sub-phase 4.1
  DecisionCount: 2 | Ordering: 3.3 → 4.1.

---

## B15-router-015-003 — IMPLEMENT+ARCHITECTURE → two flows

A: Predicates: ["IMPLEMENT", "ARCHITECTURE"] | Request: "Build the inventory allocation service within the inventory bounded context."
Router:
  - ArchitectureFlow31 (ARCHITECTURE) — sub-phase 3.1
  - ImplementationFlow41 (IMPLEMENT) — sub-phase 4.1
  DecisionCount: 2 | Routing note: "bounded context" fires ARCHITECTURE and routes to ArchitectureFlow31.

---

## B15-router-015-004 — IMPLEMENT+SECURITY → two flows

A: Predicates: ["IMPLEMENT", "SECURITY"] | Request: "Create the inventory allocation service and harden it against race conditions."
Router:
  - ImplementationFlow41 (IMPLEMENT) — sub-phase 4.1
  - SecurityAuditFlow (SECURITY) — security sub-phase
  DecisionCount: 2

---

## B15-router-015-005 — IMPLEMENT+API_CONTRACT+SECURITY+TEST → four flows

A: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST"] | Request: "Build the inventory allocation service, define the reservation API contracts, secure the allocation endpoint, and write concurrency tests."
Router:
  - APIContractFlow33 | SecurityAuditFlow | ImplementationFlow41 | TestingFlow51
  DecisionCount: 4 | Ordering: 3.3 → SecurityAuditFlow → 4.1 → 5.1
