# Layer 0-B — Router Pairs (003)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: router

---

## B15-router-003-001 — IMPLEMENT+DOCUMENT → two flows parallel

A: "Implement the order history service and document the query API."
Router: Predicates = ["IMPLEMENT", "DOCUMENT"] → ImplementationFlow41 ‖ DocumentationFlow (parallel-eligible)

---

## B15-router-003-002 — IMPLEMENT+QA → two flows sequential

A: "Build the order archival service and QA before release."
Router: Predicates = ["IMPLEMENT", "QA"] → ImplementationFlow41 → QAGatingFlow73 (7.1 — release)

---

## B15-router-003-003 — IMPLEMENT+AUTHORIZATION → two flows

A: "Build the order admin API and add role-based access for managers."
Router: Predicates = ["IMPLEMENT", "AUTHORIZATION"] → ImplementationFlow41 → AuthorizationFlow

---

## B15-router-003-004 — IMPLEMENT+TEST+DOCUMENT → three flows

A: "Implement the pricing engine, write unit tests, and document the rules."
Router: Predicates = ["IMPLEMENT", "TEST", "DOCUMENT"] → ImplementationFlow41 → UnitTestingFlow51 ‖ DocumentationFlow
TransitionType: feature_addition

---

## B15-router-003-005 — v6 corruption: TEST routed before IMPLEMENT

v6 corrupt: Predicates: ["IMPLEMENT", "TEST"] → UnitTestingFlow51 → ImplementationFlow41 (wrong order)
Correct: ImplementationFlow41 → UnitTestingFlow51 (IMPLEMENT must complete before TEST runs)
Error type: ordering violation.
