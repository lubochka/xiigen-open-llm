# Layer 0-A — Compiler Pairs (017)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: compiler

---

## P017-001 — TEST+IMPLEMENT: "write unit tests" for ShippingCost

A: "Write unit tests for the ShippingCostCalculator covering standard, express, and overnight rates by weight tier."
  "write" → IMPLEMENT; "unit tests" → TEST; "covering" — gap
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1

---

## P017-002 — TEST alone: "assert" free-shipping threshold

A: "Assert that the ShippingCostCalculator returns zero shipping cost when the order total exceeds the free-shipping threshold."
  "assert" → TEST; "zero shipping cost" — domain value; "threshold" — domain term
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P017-003 — TEST: "verify" weight-tier boundary

A: "Verify that the ShippingCostCalculator transitions from the 'standard' to 'heavy' rate at exactly 5 kg."
  "verify" → TEST; "5 kg" — domain measurement; "transitions" — domain verb, not IMPLEMENT
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
  Boundary: "transitions" is a domain state change, not the IMPLEMENT verb.

---

## P017-004 — TEST alone: "validate" international surcharge

A: "Validate that the ShippingCostCalculator adds the correct international surcharge for orders shipped outside the domestic zone."
  "validate" → TEST (universal); "international surcharge" — domain; "domestic zone" — domain
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P017-005 — TEST+IMPLEMENT: "build test fixture" for carrier mock

A: "Build a test fixture that stubs the carrier-rate API (Application Programming Interface) and confirms ShippingCostCalculator handles timeout responses gracefully."
  "build" → IMPLEMENT; "test fixture" → TEST via "test" and "fixture"; "stubs" → TEST; "confirms" — gap (confirms ≠ verify/assert)
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1
  Boundary: "confirms" is informal verification — gap. "fixture" and "stubs" fire TEST.
