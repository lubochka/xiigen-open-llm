# Layer 0-A — Compiler Pairs (043) — MergeSort: TEST fires on test-infrastructure vocabulary
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P043-001 — TEST+IMPLEMENT: "build MergeSort and write unit tests"

A: "Build the MergeSort class and write unit tests for the merge step with overlapping ranges."
  "build" → IMPLEMENT | "write" → IMPLEMENT (deduplicates) | "unit tests" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P043-002 — TEST: "assert" fires TEST on MergeSort stability

A: "Assert that the MergeSort preserves the relative order of equal elements (stable sort)."
  "assert" → TEST. "stable sort" — domain term; does not fire any predicate.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P043-003 — GAP: "merge the sub-arrays" fires zero predicates

A: "Merge the sub-arrays produced in the divide phase of the MergeSort."
  "merge" → domain operation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P043-004 — TEST: "test fixture" fires TEST for MergeSort

A: "Set up a test fixture with large randomised arrays to benchmark the MergeSort performance."
  "test fixture" → TEST (compound). TransitionType: documentation (no construction verb present).
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P043-005 — GAP: "start the sort" fires zero predicates

A: "Start the MergeSort on the input collection before the batch job completes."
  "start" → execution verb; zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
