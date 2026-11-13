# Layer 0-A — Compiler Pairs (047) — BloomFilter: TEST fires on test predicates, not bloom-domain terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P047-001 — TEST+IMPLEMENT: "develop BloomFilter and add test cases"

A: "Develop the BloomFilter class and add test cases for false-positive rate verification."
  "develop" → IMPLEMENT | "add" → IMPLEMENT (deduplicates) | "test cases" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P047-002 — TEST: "validate" fires TEST for BloomFilter membership query

A: "Validate that the BloomFilter correctly identifies an element as probably-present after insertion."
  "validate" → TEST universally.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P047-003 — GAP: "calculate false-positive rate" fires zero predicates

A: "Calculate the expected false-positive rate for a BloomFilter with 1000 elements and 10 hash functions."
  "calculate" → domain computation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P047-004 — TEST: "assert" fires TEST for definitely-absent element

A: "Assert that the BloomFilter never produces a false-negative for elements that were inserted."
  "assert" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P047-005 — GAP: "apply the bit array" fires zero predicates

A: "Apply the bit array updates from the hash functions during element insertion into the BloomFilter."
  "apply" (standalone) → domain operation verb; not in IMPLEMENT lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
