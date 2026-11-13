# Layer 0-A — Compiler Pairs (054) — NumberFormatter: TEST fires on test vocabulary, not format-domain terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P054-001 — TEST+IMPLEMENT: "develop NumberFormatter and add test cases"

A: "Develop the NumberFormatter and add test cases for thousands separators and decimal precision."
  "develop" → IMPLEMENT | "add" → IMPLEMENT (deduplicates) | "test cases" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P054-002 — TEST: "verify" fires TEST on NumberFormatter large-number display

A: "Verify that the NumberFormatter inserts a comma every three digits for numbers above 999."
  "verify" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P054-003 — GAP: "round the number to two decimals" fires zero predicates

A: "Round the number to two decimal places using the NumberFormatter precision setting."
  "round" → domain operation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P054-004 — TEST: "assert" fires TEST for NumberFormatter zero-value edge case

A: "Assert that the NumberFormatter displays 0 as '0.00' when precision is set to 2."
  "assert" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P054-005 — GAP: "process the numeric output" fires zero predicates

A: "Process the numeric output through the NumberFormatter before sending to the UI layer."
  "process" → domain computation verb; zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
