# Layer 1 — Transition Orchestrator Pairs (036) — Algorithm domain TransitionType summary reference
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T036-001 — TransitionType decision table: all algorithm-domain predicate combinations

| CompiledPredicateIds | TransitionType | Rationale |
|---------------------|----------------|-----------|
| ["TEST"] | documentation | TEST alone = specifying expected behaviour; no construction action |
| ["IMPLEMENT"] | feature_addition | Construction verb present (e.g., "generate") |
| ["TEST", "IMPLEMENT"] | feature_addition | Construction verb present; new test artifacts |
| [] | unknown | No predicates → no TransitionType |

---

## T036-002 — TransitionType belongs to IMPLEMENT; TEST alone → documentation (algorithm domain)

Rule confirmed for algorithm domain (SortingAlgorithm, SearchAlgorithm, GraphTraversal, TreeTraversal, DijkstraPath, BinarySearch, MergeSort, QuickSort, HeapSort, HashMapImpl, BloomFilter):
  - IMPLEMENT governs TransitionType when present.
  - TEST alone → documentation.
  - Algorithm domain verbs (sort, search, traverse, partition, merge, heapify) → NOT IMPLEMENT.
  - Only: write, create, build, develop, implement, add, extend, generate → IMPLEMENT.

---

## T036-003 — Boundary: "generate" produces feature_addition alone; domain operation verbs do not

A-generate: "Generate the HeapSort class." → IMPLEMENT → feature_addition
A-sort: "Sort the array using HeapSort." → [] → unknown
A-merge: "Merge the sub-arrays." → [] → unknown
Teaching: "generate" is the only algorithm-adjacent operation verb in the IMPLEMENT lexicon.
All other algorithm operation verbs (sort, search, traverse, merge, partition) produce TransitionType: unknown.
