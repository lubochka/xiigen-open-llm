# Layer 0-A — Compiler Pairs (009)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: compiler

---

## B15-compiler-009-001 — "implement" queue consumer → IMPLEMENT

A: "Implement the order event queue consumer for processing fulfillment requests."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition

---

## B15-compiler-009-002 — "implement" + AUTHORIZATION

A: "Implement the order admin panel API and restrict order deletion to admin roles only."
B: Predicates: ["IMPLEMENT", "AUTHORIZATION"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; restrict to admin roles → AUTHORIZATION (role-based access control).

---

## B15-compiler-009-003 — IMPLEMENT+API_CONTRACT+SECURITY+TEST+DOCUMENT: five

A: "Build the order gateway, integrate with the fraud detection API, secure the transaction data, write E2E tests, and document the gateway API."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "SECURITY", "TEST", "DOCUMENT"] | TransitionType: feature_addition

---

## B15-compiler-009-004 — "implement" batch processor → IMPLEMENT

A: "Implement the nightly batch order reconciliation processor."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition

---

## B15-compiler-009-005 — v6 corruption: IMPLEMENT collapsed from multi-predicate

v6 corrupt: "Build the service, integrate the API, write tests" → ["IMPLEMENT"] (collapsed)
Correct: Predicates: ["IMPLEMENT", "API_CONTRACT", "TEST"]
Error type: v6-03 — integrate → API_CONTRACT; write tests → TEST. All three fire independently.
