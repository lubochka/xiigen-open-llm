# Layer 0-A — Compiler Pairs (045) — HeapSort: TEST fires on test vocabulary, not heap-domain terms
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: compiler

---

## P045-001 — TEST+IMPLEMENT: "generate HeapSort and write tests"

A: "Generate the HeapSort class and write tests for the heapify step with varied input sizes."
  "generate" → IMPLEMENT | "write" → IMPLEMENT (deduplicates) | "tests" → TEST
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | TransitionType: feature_addition | SubPhaseId: 5.1 + 4.1

---

## P045-002 — TEST: "verify" fires TEST on HeapSort max-heap property

A: "Verify that HeapSort maintains the max-heap property after each heapify call."
  "verify" → TEST. "max-heap" is a domain term; it does not fire any predicate.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P045-003 — GAP: "heapify the array" fires zero predicates

A: "Heapify the array before extracting the maximum element."
  "heapify" → domain operation verb; not in any predicate lexicon.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge

---

## P045-004 — TEST: "validate" fires TEST for HeapSort on duplicate values

A: "Validate that HeapSort correctly handles arrays containing duplicate values."
  "validate" → TEST universally.
  CompiledPredicateIds: ["TEST"] | TransitionType: documentation | SubPhaseId: 5.1

---

## P045-005 — GAP: "trigger heap rebuild" fires zero predicates

A: "Trigger a heap rebuild after inserting a new element into the priority queue."
  "trigger" → execution verb; zero predicates.
  CompiledPredicateIds: [] | TransitionType: unknown | SourceStatus: knowledge_edge
