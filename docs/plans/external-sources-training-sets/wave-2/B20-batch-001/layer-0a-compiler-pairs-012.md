# Layer 0-A — Compiler Pairs (012)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: compiler

---

## P012-001 — TEST: "test" in imperative form

A: "Test the PriceOverrideService ensures manager-approved overrides are applied and rejects unapproved overrides."
  "test" → TEST; "ensures" — not a predicate (ensures ≠ verify/assert/validate); "PriceOverrideService" — no predicate
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Boundary: "ensures" is informal correctness language — gap. TEST fires via "test".

---

## P012-002 — TEST+IMPLEMENT: test-first TDD

A: "Write failing tests for the SubscriptionService plan upgrade flow before implementing the service logic."
  "write" → IMPLEMENT; "failing tests" → TEST via "tests"; "before implementing" → IMPLEMENT; "service logic" — domain
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]

---

## P012-003 — TEST+DOCUMENT+ARCHITECTURE: complete testing design

A: "Design the test suite architecture for the PaymentModule, write tests for all payment strategies, and document the test coverage map."
  "design" → ARCHITECTURE; "test suite architecture" → TEST (test) + ARCHITECTURE (architecture);
  "write tests" → TEST+IMPLEMENT; "document" → DOCUMENT; "coverage map" — gap
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "TEST", "DOCUMENT"] | SubPhases: [3.1, 4.1, 5.1, 6.2]

---

## P012-004 — TEST only: "assert" on event emission

A: "Assert the DomainEventDispatcher raises the OrderCreated event exactly once when a new order is placed."
  "assert" → TEST; "DomainEventDispatcher" — no predicate; "OrderCreated" — domain event name; no predicate
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P012-005 — TEST+SECURITY+AUTHORIZATION: complete auth test

A: "Test and verify the complete JWT lifecycle — issuance, refresh, revocation — and assert that revoked tokens are rejected by the authorization layer."
  "test" → TEST; "verify" → TEST; "assert" → TEST; "JWT" → SECURITY; "issuance" — SECURITY (token issuance);
  "revocation" → SECURITY; "authorization layer" → AUTHORIZATION
  CompiledPredicateIds: ["TEST", "SECURITY", "AUTHORIZATION"] | SubPhases: [5.1, 5.2, 5.2]
