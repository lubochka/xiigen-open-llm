# Layer 0-B — Router Pairs (012)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: router

---

## B15-router-012-001 — IMPLEMENT+API_CONTRACT+TEST+DOCUMENT+QA: five flows

A: "Build the carrier integration, integrate with FedEx API, write tests, document the spec, and QA."
Router: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT", "QA"]
Routing: APIContractFlow33 → ImplementationFlow41 → TestingFlow ‖ DocumentationFlow → QAGatingFlow73

---

## B15-router-012-002 — ARCHITECTURE+IMPLEMENT+API_CONTRACT+TEST four flows

A: "Design the order service architecture, build it, define the API contract, and write integration tests."
Router: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT", "TEST"]
Routing: ArchitectureFlow31 → APIContractFlow33 → ImplementationFlow41 → E2ETestingFlow53

---

## B15-router-012-003 — IMPLEMENT+SECURITY+AUTHORIZATION+QA four flows

A: "Build the order admin service, secure sensitive data, add role-based access, and QA security."
Router: ["IMPLEMENT", "SECURITY", "AUTHORIZATION", "QA"]
Routing: ImplementationFlow41 → SecurityAuditFlow + AuthorizationFlow → QAGatingFlow73

---

## B15-router-012-004 — IMPLEMENT alone (extend existing)

A: "Extend the order service to support split shipments."
Router: ["IMPLEMENT"] → ImplementationFlow41 | TransitionType: feature_addition

---

## B15-router-012-005 — Deduplication routing confirmation

A: "Build the order service, implement the order handler, create the order repository."
Router: Predicates = ["IMPLEMENT"] (three triggers → one predicate)
→ ImplementationFlow41 (single flow; not three flows)
