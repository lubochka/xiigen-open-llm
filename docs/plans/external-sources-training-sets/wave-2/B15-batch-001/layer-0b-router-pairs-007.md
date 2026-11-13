# Layer 0-B — Router Pairs (007)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: router

---

## B15-router-007-001 — IMPLEMENT+API_CONTRACT+DOCUMENT three flows

A: "Build the order tracking service, integrate with carrier APIs, and document the tracking webhook schema."
Router: Predicates = ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"]
Routing: APIContractFlow33 → ImplementationFlow41 ‖ DocumentationFlow

---

## B15-router-007-002 — ARCHITECTURE+IMPLEMENT+SECURITY three flows

A: "Design the payment domain model, build the payment processor, and harden the transaction handling."
Router: Predicates = ["ARCHITECTURE", "IMPLEMENT", "SECURITY"]
Routing: ArchitectureFlow31 → ImplementationFlow41 → SecurityAuditFlow

---

## B15-router-007-003 — IMPLEMENT+TEST: unit testing route

A: "Implement the order discount calculator and write unit tests for all discount scenarios."
Router: Predicates = ["IMPLEMENT", "TEST"] → ImplementationFlow41 → UnitTestingFlow51 (unit + calculator → 5.1)

---

## B15-router-007-004 — IMPLEMENT+TEST: E2E testing route

A: "Build the order checkout flow and write E2E tests for the complete purchase journey."
Router: Predicates = ["IMPLEMENT", "TEST"] → ImplementationFlow41 → E2ETestingFlow53 (E2E + journey → 5.3)

---

## B15-router-007-005 — Four-predicate IMPLEMENT+API_CONTRACT+TEST+QA

A: "Build the carrier integration, integrate with DHL, write integration tests, and QA."
Router: Predicates = ["IMPLEMENT", "API_CONTRACT", "TEST", "QA"]
Routing: APIContractFlow33 → ImplementationFlow41 → E2ETestingFlow53 → QAGatingFlow73
