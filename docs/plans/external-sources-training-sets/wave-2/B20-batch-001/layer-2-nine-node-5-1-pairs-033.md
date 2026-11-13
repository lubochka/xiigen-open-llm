# Layer 2 — Nine-Node Flow Pairs (033)
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: nine_node_flow | SubPhase: 5.1

---

## N033 — UnitTestingFlow51: HeapSort

Request: "Write unit tests for the HeapSort covering the heapify step and duplicate values."

### Node 01 — SelectedLogicalTransition
  CompiledPredicateIds: ["TEST", "IMPLEMENT"] | SubPhase: 5.1 | TransitionType: feature_addition

### Node 02 — Context Assembly
  Service: HeapSort | Domain: algorithm/sorting
  Dependencies identified: none (pure algorithm)
  Testable units: Sort(int[]), Heapify(int[], int n, int i)

### Node 03 — Plan Generation
  3.1 Sort unsorted array → expect ascending order
  3.2 Sort array with all duplicate values → expect all duplicates preserved
  3.3 Heapify: parent node > both children after heapify → expect max-heap invariant
  3.4 Sort single-element array → expect unchanged
  3.5 Sort empty array → expect no exception

### Node 04 — Dependency Resolution
  No external dependencies. Test framework: xUnit. Assertions: Assert.Equal, Assert.True

### Node 05 — Implementation (Test Code)
  Tests produced: 8 | Framework: xUnit
  Coverage: sort path (4 cases), heapify step (2 cases), edge cases (2 cases)

### Node 06 — Verification
  All 8 tests pass | No skipped tests

### Node 07 — Quality Gate
  Status: PASS | Coverage: 90% statement coverage

### Node 08 — Output Assembly
  OutputType: TestSuite | Files: HeapSortTests.cs

### Node 09 — Result
  Result: PASS | Tests: 8 | Status: green
