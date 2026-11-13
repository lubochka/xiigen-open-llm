# Layer 0-A — Compiler Pairs (005)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: compiler

---

## B15-compiler-005-001 — "write" + service code → IMPLEMENT

A: "Write the order status state machine for the fulfillment pipeline."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: write + state machine (code artifact) → IMPLEMENT. "state machine" is a software component.

---

## B15-compiler-005-002 — IMPLEMENT+DOCUMENT: service + API docs

A: "Implement the returns processing service and document the returns API."
B: Predicates: ["IMPLEMENT", "DOCUMENT"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; document the returns API → DOCUMENT.

---

## B15-compiler-005-003 — "integrate" + "build" → API_CONTRACT+IMPLEMENT

A: "Integrate the order service with the warehouse management system and build the sync handler."
B: Predicates: ["API_CONTRACT", "IMPLEMENT"] | TransitionType: feature_addition
Evidence: integrate → API_CONTRACT; build → IMPLEMENT.

---

## B15-compiler-005-004 — "implement" + "write tests" + "document" → three predicates

A: "Implement the price calculation engine, write unit tests for the rounding logic, and document the pricing rules."
B: Predicates: ["IMPLEMENT", "TEST", "DOCUMENT"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; write unit tests → TEST (5.1); document → DOCUMENT.

---

## B15-compiler-005-005 — "implement" + SECURITY + QA

A: "Build the order API with input validation, secure it against injection attacks, and QA the security posture."
B: Predicates: ["IMPLEMENT", "SECURITY", "QA"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; secure against injection → SECURITY; QA → QA (abbreviation).
