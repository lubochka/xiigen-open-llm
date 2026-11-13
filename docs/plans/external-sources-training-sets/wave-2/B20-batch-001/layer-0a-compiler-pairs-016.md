# Layer 0-A — Compiler Pairs (016)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: compiler

---

## P016-001 — TEST+IMPLEMENT: "write unit tests" for TaxCalculation

A: "Write unit tests for the TaxCalculationService to cover VAT (Value Added Tax) rates for EU (European Union) countries, US state tax, and zero-rated goods."
  "write" → IMPLEMENT; "unit tests" → TEST; "cover" — gap (coverage context, not TEST predicate)
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1

---

## P016-002 — TEST: "assert" correct tax band

A: "Assert that the TaxCalculationService selects the 20% VAT band for standard-rated UK goods."
  "assert" → TEST; "TaxCalculationService" — domain; "20% VAT" — domain value
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P016-003 — TEST: "verify" zero-rated logic

A: "Verify that children's clothing and food items are assigned a zero tax rate by the TaxCalculationService."
  "verify" → TEST; "zero tax rate" — domain; "children's clothing" — domain
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P016-004 — TEST alone: "spec" multi-jurisdiction tax

A: "Spec the multi-jurisdiction tax behaviour: when a customer ships across state lines, the TaxCalculationService must apply destination-state rates."
  "spec" → TEST; "multi-jurisdiction" — domain; "destination-state" — domain term
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P016-005 — TEST+IMPLEMENT: "create test harness"

A: "Create a test harness for the TaxCalculationService that stubs the tax-rate repository and validates output per region."
  "create" → IMPLEMENT; "test harness" → TEST via "test" and "harness"; "stubs" → TEST (stub is test vocabulary);
  "validates" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1
  Note: "harness" and "stubs" are test-infrastructure vocabulary that fire TEST. "create" fires IMPLEMENT.
