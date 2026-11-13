# Layer 0-A — Compiler Pairs (015)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: compiler

---

## P015-001 — TEST alone: "write unit tests" for DiscountEngine

A: "Write unit tests for the DiscountEngine verifying percentage, fixed-amount, and buy-one-get-one discount logic."
  "write" → IMPLEMENT; "unit tests" → TEST; "verifying" → TEST (deduplicates);
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1

---

## P015-002 — TEST: "assert" discount exclusion rules

A: "Assert that the DiscountEngine does not apply stacked discounts when the product is flagged as non-combinable."
  "assert" → TEST; "stacked discounts" — domain term; "non-combinable" — domain flag; no other predicate
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P015-003 — TEST alone: "verify" discount expiry

A: "Verify that the DiscountEngine rejects discount codes whose expiry date has passed."
  "verify" → TEST; "expiry date" — domain term; "rejects" — domain verb; no IMPLEMENT
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Boundary: "rejects" is a domain outcome, not a predicate-firing verb.

---

## P015-004 — TEST+IMPLEMENT: "develop test scenarios"

A: "Develop test scenarios for DiscountEngine covering zero-value discounts, maximum-cap enforcement, and first-order-only rules."
  "develop" → IMPLEMENT; "test scenarios" → TEST via "test"; "scenarios" alone — gap
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1

---

## P015-005 — TEST only: "validate" minimum order threshold

A: "Validate that the DiscountEngine applies a 15% discount only when the cart total exceeds the minimum order threshold."
  "validate" → TEST (universal); "15%" — domain value; "threshold" — domain term
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
