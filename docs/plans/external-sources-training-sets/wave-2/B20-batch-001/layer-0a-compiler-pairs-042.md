# Layer 0-A — Compiler Pairs (042) — BinarySearch: TEST+IMPLEMENT combo and deduplication
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P042-001 — TEST+IMPLEMENT: "create BinarySearch and add test coverage"

A: "Create the BinarySearch class and add test coverage for sorted and unsorted input arrays."
  "create" → IMPLEMENT | "add" → IMPLEMENT (deduplicates) | "test coverage" → TEST (via "test")
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P042-002 — TEST: "spec" fires TEST for BinarySearch preconditions

A: "Spec out the BinarySearch behaviour when the target appears at the first and last positions."
  "spec" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P042-003 — TEST deduplication: three TEST verbs collapse to one predicate

A: "Assert, verify, and validate that the BinarySearch returns mid-index for a single-element array."
  "assert" → TEST | "verify" → TEST (deduplicates) | "validate" → TEST (deduplicates)
  Three TEST-firing verbs → one TEST predicate.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P042-004 — GAP: "look up element" fires zero predicates

A: "Look up the target element in the sorted array using binary search."
  "look up" → domain operation phrase; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P042-005 — TEST+IMPLEMENT: "extend and write regression tests" for BinarySearch

A: "Extend the BinarySearch class to support floating-point arrays and write regression tests."
  "extend" → IMPLEMENT | "write" → IMPLEMENT (deduplicates) | "regression tests" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1
