# Layer 0-A — Compiler Pairs (053) — CurrencyFormatter: TEST fires on test vocabulary, not currency terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P053-001 — TEST+IMPLEMENT: "create CurrencyFormatter and write test suite"

A: "Create the CurrencyFormatter and write a test suite covering GBP (Great British Pound), EUR (Euro), and USD (US Dollar) formatting."
  "create" → IMPLEMENT | "write" → IMPLEMENT (deduplicates) | "test suite" → TEST
  GBP (Great British Pound), EUR (Euro), USD (US Dollar) — domain abbreviations; none fire predicates.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P053-002 — TEST: "assert" fires TEST on symbol placement

A: "Assert that the CurrencyFormatter places the currency symbol before the amount for GBP (Great British Pound)."
  "assert" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P053-003 — GAP: "format the currency value" fires zero predicates

A: "Format the currency value to two decimal places using the CurrencyFormatter."
  "format" → domain operation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P053-004 — TEST: "validate" fires TEST for CurrencyFormatter negative amounts

A: "Validate that the CurrencyFormatter correctly displays negative amounts with a leading minus sign."
  "validate" → TEST universally.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P053-005 — GAP: "evaluate output format" fires zero predicates

A: "Evaluate the output format produced by the CurrencyFormatter for accuracy."
  "evaluate" → domain computation verb; zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
