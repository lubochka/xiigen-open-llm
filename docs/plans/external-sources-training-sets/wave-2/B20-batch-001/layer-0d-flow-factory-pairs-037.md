# Layer 0-D — Flow Factory Pairs (037) — SortingAlgorithm: factory selection for algorithm domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F037-001 — TEST+IMPLEMENT: "build SortingAlgorithm and write tests" → two instances

Request: "Build the SortingAlgorithm and write unit tests for the insertion sort variant."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F037-002 — TEST alone: "assert" on SortingAlgorithm → one UnitTestingFlow51, documentation

Request: "Assert that the SortingAlgorithm returns elements in ascending order for an unsorted array."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F037-003 — Zero predicate: "run the sort" → empty factory

Request: "Run the SortingAlgorithm on the sample dataset."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
