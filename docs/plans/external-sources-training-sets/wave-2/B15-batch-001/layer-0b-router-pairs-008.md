# Layer 0-B — Router Pairs (008)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: router

---

## B15-router-008-001 — IMPLEMENT+AUTHORIZATION route

A: "Build the order reporting service and restrict access by department role."
Router: Predicates = ["IMPLEMENT", "AUTHORIZATION"] → ImplementationFlow41 → AuthorizationFlow

---

## B15-router-008-002 — IMPLEMENT alone (scaffold)

A: "Scaffold the order management microservice with standard middleware stack."
Router: Predicates = ["IMPLEMENT"] → ImplementationFlow41 (4.1) | TransitionType: feature_addition

---

## B15-router-008-003 — IMPLEMENT+SECURITY+TEST+DOCUMENT four flows

A: "Build the payment handler, secure card data, write security-focused tests, and document the payment API."
Router: Predicates = ["IMPLEMENT", "SECURITY", "TEST", "DOCUMENT"]
Routing: ImplementationFlow41 → SecurityAuditFlow → TestingFlow ‖ DocumentationFlow

---

## B15-router-008-004 — ARCHITECTURE+IMPLEMENT+TEST route

A: "Design the order processing pipeline architecture, implement stage one, and write integration tests."
Router: Predicates = ["ARCHITECTURE", "IMPLEMENT", "TEST"]
Routing: ArchitectureFlow31 → ImplementationFlow41 → E2ETestingFlow53

---

## B15-router-008-005 — IMPLEMENT+API_CONTRACT+SECURITY+DOCUMENT four flows

A: "Build the fraud detection integration, connect with the risk API, secure the API key handling, and document the integration spec."
Router: Predicates = ["IMPLEMENT", "API_CONTRACT", "SECURITY", "DOCUMENT"]
Routing: APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow ‖ DocumentationFlow
