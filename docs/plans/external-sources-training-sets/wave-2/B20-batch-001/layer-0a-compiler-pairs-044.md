# Layer 0-A — Compiler Pairs (044) — QuickSort: TEST fires on "spec", "assert", "test suite"
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P044-001 — TEST+IMPLEMENT: "implement QuickSort and spec pivot behaviour"

A: "Implement the QuickSort class and spec the pivot selection behaviour for worst-case input."
  "implement" → IMPLEMENT | "spec" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P044-002 — TEST: "assert" fires TEST on QuickSort partition correctness

A: "Assert that the QuickSort partition step places all elements smaller than the pivot to the left."
  "assert" → TEST.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P044-003 — GAP: "partition the array" fires zero predicates

A: "Partition the array around the chosen pivot element using QuickSort logic."
  "partition" → domain operation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P044-004 — TEST: "test suite" fires TEST for QuickSort all input sizes

A: "Create a test suite covering QuickSort on arrays of size 0, 1, 2, and 1000."
  "test suite" → TEST (compound). "create" → IMPLEMENT.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P044-005 — GAP: "invoke the quick sort routine" fires zero predicates

A: "Invoke the QuickSort routine on the unsorted dataset."
  "invoke" → execution verb; zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
