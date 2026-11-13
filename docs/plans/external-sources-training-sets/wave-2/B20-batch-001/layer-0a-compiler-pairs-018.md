# Layer 0-A — Compiler Pairs (018)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: compiler

---

## P018-001 — TEST+IMPLEMENT: "write unit tests" for LoyaltyPoints

A: "Write unit tests for the LoyaltyPointsService verifying that points are awarded at the correct rate per pound spent."
  "write" → IMPLEMENT; "unit tests" → TEST; "verifying" → TEST (deduplicates)
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1

---

## P018-002 — TEST alone: "assert" points expiry

A: "Assert that the LoyaltyPointsService marks points as expired when they have not been redeemed within 12 months."
  "assert" → TEST; "12 months" — domain value; "expired" — domain state
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P018-003 — TEST: "verify" redemption cap

A: "Verify that the LoyaltyPointsService does not allow a customer to redeem more points than their current balance."
  "verify" → TEST; "redemption cap" — domain concept; "current balance" — domain term
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P018-004 — TEST alone: "spec" tier multiplier logic

A: "Spec the loyalty tier multiplier: gold members earn 3x points, silver members earn 2x, standard members earn 1x."
  "spec" → TEST; "3x", "2x", "1x" — domain multiplier values; no IMPLEMENT vocabulary
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P018-005 — TEST+IMPLEMENT: "create unit test cases"

A: "Create unit test cases for LoyaltyPointsService covering point accrual, manual adjustment, and account closure scenarios."
  "create" → IMPLEMENT; "unit test cases" → TEST; "scenarios" — gap alone
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1
