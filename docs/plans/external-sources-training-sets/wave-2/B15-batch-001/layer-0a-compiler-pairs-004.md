# Layer 0-A — Compiler Pairs (004)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: compiler

---

## B15-compiler-004-001 — "refactor" → IMPLEMENT + refactoring TransitionType

A: "Refactor the order processing pipeline to use the new event-driven pattern."
B: Predicates: ["IMPLEMENT"] | TransitionType: refactoring
Evidence: refactor → IMPLEMENT; refactor verb → refactoring TransitionType (overrides feature_addition).

---

## B15-compiler-004-002 — IMPLEMENT+API_CONTRACT+TEST three predicates

A: "Build the shipping rate calculator, integrate with the carrier API, and write tests."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; integrate → API_CONTRACT; write tests → TEST.

---

## B15-compiler-004-003 — "add" + feature → IMPLEMENT

A: "Add bulk order processing capability to the existing order service."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: add + capability = code artifact → IMPLEMENT. "bulk order processing capability" = software feature.

---

## B15-compiler-004-004 — IMPLEMENT+AUTHORIZATION → two predicates

A: "Build the order management API and add role-based access so only admins can cancel orders."
B: Predicates: ["IMPLEMENT", "AUTHORIZATION"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; add role-based access → AUTHORIZATION.

---

## B15-compiler-004-005 — Full five predicates from one order-service request

A: "Design the order domain model, build the order service, integrate with the payment API, write tests, and QA."
B: Predicates: ["ARCHITECTURE", "IMPLEMENT", "API_CONTRACT", "TEST", "QA"] | TransitionType: feature_addition
Evidence: design domain model → ARCHITECTURE; build → IMPLEMENT; integrate → API_CONTRACT; write tests → TEST; QA → QA.
