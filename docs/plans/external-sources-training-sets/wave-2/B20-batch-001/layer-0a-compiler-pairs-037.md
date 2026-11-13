# Layer 0-A — Compiler Pairs (037) — SortingAlgorithm: TEST fires on "assert", not on sort-domain terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P037-001 — TEST: "assert" fires TEST on sorting output correctness

A: "Assert that the SortingAlgorithm returns elements in ascending order for an unsorted integer array."
  "assert" → TEST. Fires universally regardless of domain.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P037-002 — TEST+IMPLEMENT: "write tests and implement SortingAlgorithm"

A: "Write unit tests for the SortingAlgorithm and implement the insertion sort variant."
  "write" → IMPLEMENT | "unit tests" → TEST | "implement" → IMPLEMENT (deduplicates)
  "insertion sort" — domain algorithm name; does not fire any predicate.
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P037-003 — GAP: "run the sort and check output" fires zero predicates

A: "Run the SortingAlgorithm on a sample array and check the output is sorted."
  "run" → execution verb; zero predicates. "check" → informal QA; zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P037-004 — TEST: "validate" fires TEST for edge-case boundary

A: "Validate that the SortingAlgorithm handles an empty array without throwing."
  "validate" → TEST universally.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P037-005 — GAP: "execute the sort algorithm" fires zero predicates

A: "Execute the SortingAlgorithm on the dataset before the report is generated."
  "execute" → execution verb; not in TEST or IMPLEMENT predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
