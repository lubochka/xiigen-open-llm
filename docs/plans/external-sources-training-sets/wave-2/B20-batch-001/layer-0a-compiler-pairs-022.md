# Layer 0-A — Compiler Pairs (022)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: compiler

---

## P022-001 — TEST+IMPLEMENT: "write unit tests" for PriceFormatter

A: "Write unit tests for the PriceFormatter verifying that currency symbols, decimal places, and locale-specific separators are rendered correctly."
  "write" → IMPLEMENT; "unit tests" → TEST; "verifying" → TEST (deduplicates)
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1

---

## P022-002 — TEST: "assert" locale formatting

A: "Assert that the PriceFormatter renders £1,234.56 for GBP (Great British Pound) locale and 1.234,56 € for German EUR (Euro) locale."
  "assert" → TEST; "GBP", "EUR" — domain currency codes; "locale" — domain term
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P022-003 — TEST alone: "verify" zero-price display

A: "Verify that the PriceFormatter displays 'Free' instead of '£0.00' when the price is exactly zero and the free-display flag is enabled."
  "verify" → TEST; "free-display flag" — domain feature flag; "£0.00" — domain value
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P022-004 — TEST: "validate" negative price formatting

A: "Validate that the PriceFormatter wraps negative prices in parentheses for accountancy display mode."
  "validate" → TEST (universal); "negative prices" — domain; "parentheses" — formatting term
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1

---

## P022-005 — TEST alone: "spec" rounding mode

A: "Spec the rounding mode of PriceFormatter: always round half-up, never truncate, and preserve two decimal places in all output."
  "spec" → TEST; "rounding mode" — domain formatting rule; "half-up" — domain rounding strategy
  CompiledPredicateIds: ["TEST"] | SubPhase: 5.1
