# Layer 0-A — Compiler Pairs (004)
# Batch: B20-batch-001 | Scenario: B20 — Unit testing
# Production date: 2026-05-05 | pair_kind: compiler

---

## P004-001 — TEST: "assert" on idempotency

A: "Assert that the CouponService redemption is idempotent — applying the same coupon twice must not double-apply the discount."
  "assert" → TEST; "idempotent" — gap; "redemption" — domain; "discount" — domain
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P004-002 — TEST only: "verify" on concurrency

A: "Verify the InventoryService stock reservation handles concurrent requests without overselling."
  "verify" → TEST; "concurrent requests" — gap (concurrency strategy term); "overselling" — domain
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P004-003 — TEST+IMPLEMENT: test fixtures

A: "Create test fixtures for the ShippingService and add tests for domestic vs international rate calculation."
  "create" → IMPLEMENT; "test fixtures" → TEST via "test"; "add tests" → TEST+IMPLEMENT; "ShippingService" — no predicate
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]

---

## P004-004 — TEST+DOCUMENT: test report spec

A: "Spec out the ReportService data export tests and document the expected output schema."
  "spec" → TEST; "tests" → TEST; "document" → DOCUMENT; "output schema" → ARCHITECTURE? "schema" alone does not fire ARCHITECTURE without design verb.
  CompiledPredicateIds: ["TEST", "DOCUMENT"] | SubPhases: [5.1, 6.2]

---

## P004-005 — TEST + IMPLEMENT: extend test suite

A: "Extend the existing test suite and write additional tests for the RefundService edge cases."
  "extend" → IMPLEMENT; "test suite" → TEST; "write" → IMPLEMENT; "tests" → TEST; "RefundService" — no predicate
  CompiledPredicateIds: ["IMPLEMENT", "TEST"] | SubPhases: [4.1, 5.1]
  Note: "extend" fires IMPLEMENT (extend = add to existing code).
