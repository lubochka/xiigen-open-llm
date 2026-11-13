# Layer 0-A — Compiler Pairs (007)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-05 | pair_kind: compiler

---

## B15-compiler-007-001 — "implement" alone: purest form

A: "Implement the order event publisher using the message bus interface."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT. Clean single-predicate IMPLEMENT request.

---

## B15-compiler-007-002 — "add" + middleware → IMPLEMENT

A: "Add request logging middleware to the order API gateway."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: add + middleware (code artifact) → IMPLEMENT.

---

## B15-compiler-007-003 — IMPLEMENT+SECURITY: "validate and sanitize input"

A: "Implement the order submission handler and validate all input fields against injection attacks."
B: Predicates: ["IMPLEMENT", "SECURITY"] | TransitionType: feature_addition
Evidence: implement → IMPLEMENT; validate against injection attacks → SECURITY.

---

## B15-compiler-007-004 — "create" + handler → IMPLEMENT

A: "Create the webhook handler for order status change events."
B: Predicates: ["IMPLEMENT"] | TransitionType: feature_addition
Evidence: create + handler → IMPLEMENT. Webhook handler = software component.

---

## B15-compiler-007-005 — IMPLEMENT+API_CONTRACT+DOCUMENT: build+integrate+doc

A: "Build the order tracking service, integrate it with the carrier tracking API, and document the tracking schema."
B: Predicates: ["IMPLEMENT", "API_CONTRACT", "DOCUMENT"] | TransitionType: feature_addition
Evidence: build → IMPLEMENT; integrate → API_CONTRACT; document → DOCUMENT.
