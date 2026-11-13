# Layer 0-B — Router Pairs (002)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: router

---

## B15-router-002-001 — IMPLEMENT alone → ImplementationFlow41

A: "Build the order processing service from scratch."
Router: Predicates = ["IMPLEMENT"] → ImplementationFlow41 (4.1)
TransitionType: feature_addition

---

## B15-router-002-002 — IMPLEMENT+API_CONTRACT → two flows

A: "Build the order service and integrate with the carrier API."
Router: Predicates = ["IMPLEMENT", "API_CONTRACT"] → ImplementationFlow41 + APIContractFlow33
Ordering: APIContractFlow33 before ImplementationFlow41

---

## B15-router-002-003 — IMPLEMENT+TEST → two flows

A: "Implement the order cancellation handler and write unit tests."
Router: Predicates = ["IMPLEMENT", "TEST"] → ImplementationFlow41 → UnitTestingFlow51 (5.1)

---

## B15-router-002-004 — IMPLEMENT+SECURITY → two flows

A: "Build the payment handler and secure the card data."
Router: Predicates = ["IMPLEMENT", "SECURITY"] → ImplementationFlow41 → SecurityAuditFlow

---

## B15-router-002-005 — ARCHITECTURE+IMPLEMENT → two flows

A: "Design the order domain model and build the aggregate root."
Router: Predicates = ["ARCHITECTURE", "IMPLEMENT"] → ArchitectureFlow31 → ImplementationFlow41
