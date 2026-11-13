# Layer 0-B — Router Pairs (004)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: router

---

## B15-router-004-001 — IMPLEMENT+API_CONTRACT+TEST → three flows

A: "Build the shipping calculator, integrate with the carrier API, and write tests."
Router: Predicates = ["IMPLEMENT", "API_CONTRACT", "TEST"] → APIContractFlow33 → ImplementationFlow41 → E2ETestingFlow53 (integration → 5.3)

---

## B15-router-004-002 — IMPLEMENT+SECURITY+QA → three flows

A: "Build the checkout service, secure payment data, and QA the checkout flow."
Router: Predicates = ["IMPLEMENT", "SECURITY", "QA"] → ImplementationFlow41 → SecurityAuditFlow → QAGatingFlow73

---

## B15-router-004-003 — IMPLEMENT refactoring TransitionType

A: "Refactor the order status machine."
Router: Predicates = ["IMPLEMENT"] → ImplementationFlow41
TransitionType: refactoring (refactor verb present)

---

## B15-router-004-004 — Five-predicate routing

A: "Build the order service, integrate with payment API, write tests, document the API, and QA."
Router: Predicates = ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT", "QA"]
Routing: APIContractFlow33 → ImplementationFlow41 → TestingFlow ‖ DocumentationFlow → QAGatingFlow73

---

## B15-router-004-005 — ARCHITECTURE+IMPLEMENT+TEST+QA routing

A: "Design the order model, build the service, write E2E tests, and QA."
Router: Predicates = ["ARCHITECTURE", "IMPLEMENT", "TEST", "QA"]
Routing: ArchitectureFlow31 → ImplementationFlow41 → E2ETestingFlow53 → QAGatingFlow73
