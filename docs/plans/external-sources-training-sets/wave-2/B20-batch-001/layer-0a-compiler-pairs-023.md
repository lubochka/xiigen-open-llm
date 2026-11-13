# Layer 0-A — Compiler Pairs (023)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: compiler

---

## P023-001 — TEST+IMPLEMENT: "write unit tests" for CouponApplicator

A: "Write unit tests for the CouponApplicator to verify that valid coupons reduce the cart total and expired coupons are rejected."
  "write" → IMPLEMENT; "unit tests" → TEST; "verify" → TEST (deduplicates)
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1

---

## P023-002 — TEST alone: "assert" single-use coupon

A: "Assert that the CouponApplicator marks a single-use coupon as consumed after its first successful application."
  "assert" → TEST; "single-use coupon" — domain concept; "consumed" — domain state
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P023-003 — TEST: "verify" minimum spend enforcement

A: "Verify that the CouponApplicator rejects the coupon when the cart total is below the coupon's minimum spend requirement."
  "verify" → TEST; "minimum spend" — domain rule; "rejects" — domain outcome
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P023-004 — TEST alone: "validate" coupon stacking rules

A: "Validate that the CouponApplicator allows at most one percentage-off coupon and one fixed-amount coupon to be applied simultaneously."
  "validate" → TEST (universal); "stacking rules" — domain policy; "percentage-off" — domain type
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P023-005 — TEST+IMPLEMENT: "create test cases"

A: "Create test cases for CouponApplicator that use mock coupon repositories to isolate applicator logic from database access."
  "create" → IMPLEMENT; "test cases" → TEST via "test"; "mock" → TEST (mock is test vocabulary);
  "repositories" — domain term; "isolate" — gap
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1
  Note: "mock" fires TEST independently from "test cases". Both deduuplicate to one TEST predicate.
