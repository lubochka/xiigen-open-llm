# Layer 0-A — Compiler Pairs (005)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: compiler

---

## P005-001 — TEST only: "test" as noun object

A: "Add a test for the SearchService ranking algorithm to confirm top results match highest relevance scores."
  "add" → IMPLEMENT; "a test" → TEST via "test"; "confirm" — not in predicate (confirm ≠ verify/assert)
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Boundary: "confirm" is gap — TEST fires only on: test, verify, assert, validate, spec.

---

## P005-002 — TEST+API_CONTRACT: service contract test

A: "Test that the OrderService API contract is fulfilled — assert response shape, status codes, and error payloads."
  "test" → TEST; "API contract" → API_CONTRACT; "assert" → TEST; "interface" not present but "contract" fires API_CONTRACT
  CompiledPredicateIds: ["TEST", "API_CONTRACT"] | SubPhases: [5.1, 3.3]

---

## P005-003 — TEST+SECURITY: JWT test

A: "Write tests to verify the JwtTokenService generates tokens with correct claims and rejects expired tokens."
  "write" → IMPLEMENT; "tests" → TEST; "verify" → TEST; "JWT" → SECURITY; "tokens" (JWT tokens) → SECURITY; "claims" — SECURITY fires on "claims" in JWT context
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "SECURITY"] | SubPhases: [4.1, 5.1, 5.2]

---

## P005-004 — TEST only: "assert" on state machine

A: "Assert that the OrderStateMachine transitions correctly from Pending → Processing → Shipped → Delivered."
  "assert" → TEST; "state machine" — ARCHITECTURE? No: no design/structure action verb; state machine is a domain term here.
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P005-005 — TEST+IMPLEMENT+DOCUMENT: full test coverage task

A: "Implement test coverage for the PricingEngine, write tests for all formula variants, and document the test matrix."
  "implement" → IMPLEMENT; "test coverage" → TEST via "test"; "write tests" → TEST+IMPLEMENT; "document" → DOCUMENT; "test matrix" → TEST
  CompiledPredicateIds: ["IMPLEMENT", "TEST", "DOCUMENT"] | SubPhases: [4.1, 5.1, 6.2]
