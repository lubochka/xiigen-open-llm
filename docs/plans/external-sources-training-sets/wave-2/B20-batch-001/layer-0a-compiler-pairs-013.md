# Layer 0-A — Compiler Pairs (013)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: compiler

---

## P013-001 — TEST: "write unit tests" for CartCalculation

A: "Write unit tests for the CartCalculationService verifying that item totals, quantity adjustments, and empty-cart edge cases are handled correctly."
  "write" → IMPLEMENT; "unit tests" → TEST via "tests" and "unit tests" compound; "verifying" → TEST;
  "edge cases" — gap (methodology term); "CartCalculationService" — domain name, no predicate
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1
  Boundary: "verifying" fires TEST independently from "unit tests" — both contribute but deduplicate to one TEST predicate.

---

## P013-002 — TEST alone: "assert" on cart subtotal

A: "Assert that the CartCalculationService returns the correct subtotal when three items with different unit prices are added."
  "assert" → TEST; "CartCalculationService" — no predicate; "correct subtotal" — informal correctness, gap
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Boundary: "correct" alone is gap; "assert" is the sole predicate-firing verb.

---

## P013-003 — TEST+IMPLEMENT: build test cases for cart rounding

A: "Build and write unit test cases for CartCalculationService to cover rounding behaviour when fractional prices accumulate."
  "build" → IMPLEMENT; "write" → IMPLEMENT; "unit test cases" → TEST via "test"; "cases" — gap alone
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1
  Note: "test cases" fires TEST; both "build" and "write" fire IMPLEMENT independently; deduplication keeps one IMPLEMENT.

---

## P013-004 — TEST alone: "validate" cart discount application

A: "Validate that the CartCalculationService applies a percentage discount only to eligible items and leaves non-eligible items unchanged."
  "validate" → TEST (universal rule: validate fires TEST in all contexts); "discount" — domain term; no SECURITY
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Boundary: "validate" fires TEST universally — business-rule validation and test assertion are the same predicate.

---

## P013-005 — TEST only: "spec" for cart empty state

A: "Spec out the expected behaviour of CartCalculationService when the cart is empty: total must be zero, tax must be zero, no discount applied."
  "spec" → TEST fires on "spec" in test context; "expected behaviour" — gap; "total must be zero" — gap
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Boundary: "spec" fires TEST. "expected behaviour" alone is informal — gap.
