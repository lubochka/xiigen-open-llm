# Layer 2 — Nine-Node Flow Pairs (036)
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: nine_node_flow | SubPhase: 5.1

---

## N036 — Nine-node flow rule summary for algorithm domain services

### Algorithm-domain service results summary

| File | Service | Tests | Verdict |
|------|---------|-------|---------|
| N025 | SortingAlgorithm | 9 | PASS |
| N026 | SearchAlgorithm | 8 | PASS |
| N027 | GraphTraversal | 10 | PASS |
| N028 | TreeTraversal | 11 | PASS |
| N029 | DijkstraPath | 9 | PASS |
| N030 | BinarySearch | 10 | PASS |
| N031 | MergeSort | 9 | PASS |
| N032 | QuickSort | 9 | PASS |
| N033 | HeapSort | 8 | PASS |
| N034 | HashMapImpl | 10 | PASS |
| N035 | BloomFilter | 8 | PASS_WITH_WARNINGS |

### Algorithm domain nine-node flow invariants

Node 01 always carries the label: SelectedLogicalTransition
All algorithm domain services: CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1 | TransitionType: feature_addition
Coverage target: ≥ 85% triggers PASS; < 85% triggers PASS_WITH_WARNINGS.
BloomFilter (N035) received PASS_WITH_WARNINGS due to probabilistic path coverage < 85%.

### Key algorithm domain teaching points

1. Pure algorithm classes (no external dependencies) use Assert.Equal for output comparison.
2. Algorithm classes with interfaces (IGraph, IWeightedGraph) use mocks for graph structure.
3. Exception-producing paths (null input, negative weights) always use Assert.Throws.
4. Stability tests (MergeSort) require CollectionAssert.AreEqual with position tracking.
5. Probabilistic algorithms (BloomFilter) require deterministic stubs for repeatable tests.
