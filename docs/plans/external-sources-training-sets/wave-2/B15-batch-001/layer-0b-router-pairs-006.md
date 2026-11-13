# Layer 0-B — Router Pairs (006)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: router

---

## B15-router-006-001 — IMPLEMENT+SECURITY+AUTHORIZATION

A: "Build the order admin API, secure the data, and add manager-only access."
Router: Predicates = ["IMPLEMENT", "SECURITY", "AUTHORIZATION"] → ImplementationFlow41 + SecurityAuditFlow + AuthorizationFlow

---

## B15-router-006-002 — IMPLEMENT alone: refactoring

A: "Refactor the order processing pipeline for better separation of concerns."
Router: Predicates = ["IMPLEMENT"] → ImplementationFlow41 | TransitionType: refactoring

---

## B15-router-006-003 — Six-predicate routing

A: "Build the checkout service, integrate with payment API, secure tokens, write tests, document API, QA."
Router: Predicates = ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT", "QA"]
Routing: APIContractFlow33 → ImplementationFlow41 → SecurityAuditFlow → TestingFlow ‖ DocumentationFlow → QAGatingFlow73

---

## B15-router-006-004 — IMPLEMENT+API_CONTRACT parallel-eligible

A: "Build the fulfillment service and define the warehouse API contract."
Router: Predicates = ["IMPLEMENT", "API_CONTRACT"]
Note: APIContractFlow33 before ImplementationFlow41 (contract informs implementation).

---

## B15-router-006-005 — IMPLEMENT+DOCUMENT parallel-eligible

A: "Implement the order export service and document the export format."
Router: Predicates = ["IMPLEMENT", "DOCUMENT"] → ImplementationFlow41 ‖ DocumentationFlow (parallel; no dependency)
