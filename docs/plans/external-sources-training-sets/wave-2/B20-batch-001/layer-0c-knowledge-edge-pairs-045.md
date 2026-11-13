# Layer 0-C — Knowledge Edge Pairs (045) — HeapSort: gap analysis for heap-domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E045-001 — "heapify" does NOT fire IMPLEMENT

"Heapify the array before extracting the maximum element." → CompiledPredicateIds: []
"heapify" is a domain operation verb. It is NOT in the IMPLEMENT predicate lexicon.
Domain-specific algorithm operations universally produce zero predicates.

---

## E045-002 — "trigger heap rebuild" execution gap

"Trigger a heap rebuild after inserting a new element into the priority queue." → CompiledPredicateIds: []
"trigger" → execution verb; zero predicates.
The domain context (heap rebuild, priority queue) does not rescue the execution verb from zero predicates.

---

## E045-003 — "max-heap property" domain term does NOT fire TEST

"Ensure HeapSort maintains the max-heap property." → CompiledPredicateIds: []
"ensure" → informal QA; zero predicates. "max-heap property" → domain term; does not fire TEST.
Confirmed: domain algorithmic properties (max-heap, min-heap, balance factor) do not fire predicates.

---

## E045-004 — "duplicate values" domain term does NOT fire any predicate

"Handle duplicate values correctly in HeapSort." → CompiledPredicateIds: []
"handle" → domain operation verb; zero predicates.
"duplicate values" → domain term describing input characteristics; zero predicates.
Gap type: operation instruction without test or construction vocabulary.
