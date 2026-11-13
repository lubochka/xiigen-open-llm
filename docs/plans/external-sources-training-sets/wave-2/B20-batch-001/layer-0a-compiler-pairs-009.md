# Layer 0-A — Compiler Pairs (009)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: compiler

---

## P009-001 — TEST+ARCHITECTURE: design test structure

A: "Design the test architecture for the EventBus and write unit tests covering all event handler registrations."
  "design" → ARCHITECTURE; "test architecture" → TEST via "test" + ARCHITECTURE via "architecture";
  "write" → IMPLEMENT; "unit tests" → TEST
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "TEST"] | SubPhases: [3.1, 4.1, 5.1]

---

## P009-002 — TEST: "validate" on error codes

A: "Validate that the ApiErrorHandler returns the correct ErrorCode for each exception type in the system."
  "validate" → TEST; "ApiErrorHandler" — no predicate; "ErrorCode" — domain type; "exception type" — no predicate
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P009-003 — TEST+IMPLEMENT: test doubles pattern

A: "Implement spy and fake objects for the ExternalPaymentGateway and write integration tests for the ChargeService."
  "implement" → IMPLEMENT; "spy" — gap (test double terminology); "fake" — gap; "write" → IMPLEMENT;
  "integration tests" → TEST; "ChargeService" — no predicate
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]
  Gap warnings: "spy", "fake", "test double" — testing methodology terms; gap vocabulary.

---

## P009-004 — TEST+SECURITY+AUTHORIZATION: full auth test

A: "Test the complete authentication and authorization flow — verify login, assert token issuance, and validate role enforcement."
  "test" → TEST; "verify" → TEST; "assert" → TEST; "validate" → TEST;
  "authentication" → SECURITY; "authorization" → AUTHORIZATION; "token" → SECURITY; "role enforcement" → AUTHORIZATION
  CompiledPredicateIds: ["TEST", "SECURITY", "AUTHORIZATION"] | SubPhases: [5.1, 5.2, 5.2]

---

## P009-005 — TEST: "test" + BDD vocabulary

A: "Test the CheckoutService Given a valid cart When the user places the order Then the order ID must be returned."
  "test" → TEST; "Given/When/Then" — BDD (Behaviour-Driven Development) vocabulary; gap terms individually but "test" fires
  "CheckoutService" — no predicate; "cart" — domain
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Gap warnings: BDD framing vocabulary (Given/When/Then) is gap; TEST fires via "test".
