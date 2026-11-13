# Layer 0-C — Knowledge Edge Pairs (037) — SortingAlgorithm: gap analysis for algorithm-domain verbs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E037-001 — Algorithm operation verbs that always produce zero predicates

Execution and domain verbs confirmed as zero-predicate for SortingAlgorithm context:
  sort, execute, run, invoke, trigger, start, launch — all zero predicates
  Domain computations: compare, swap, partition, pivot, merge, heapify — all zero predicates
  "sortable" (adjective) — zero predicates (same adjective rule as "testable")

---

## E037-002 — "run tests" ≠ TEST: reinforced for algorithm domain

"run unit tests for the SortingAlgorithm" → CompiledPredicateIds: []
Reason: "run" is an execution verb. "unit tests" as object of "run" does NOT independently fire TEST.
The action verb governs. "run" produces zero predicates regardless of what follows.
This is the same rule established in the billing domain — universal across all B20 domains.

---

## E037-003 — "sort" does NOT fire IMPLEMENT

"sort the array using the SortingAlgorithm" → CompiledPredicateIds: []
"sort" is a domain operation verb. It is NOT in the IMPLEMENT predicate lexicon.
Only: write, create, build, develop, implement, add, extend, generate fire IMPLEMENT.
"sort" does not fire IMPLEMENT even when the request asks to sort for implementation purposes.

---

## E037-004 — "sortable" (adjective) gap: reinforced boundary

"Ensure the collection is sortable before passing it to the SortingAlgorithm." → CompiledPredicateIds: []
"ensure" → informal QA. "sortable" → adjective; does not fire TEST.
Both produce zero predicates. This extends the "testable" adjective rule to all -able adjectives in algorithm domain.
