# Layer 0-D — Flow Factory Pairs (044) — QuickSort: factory selection for sort domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F044-001 — TEST+IMPLEMENT: "implement QuickSort and spec pivot behaviour" → two instances

Request: "Implement the QuickSort class and spec the pivot selection behaviour for worst-case input."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F044-002 — TEST+IMPLEMENT: "create test suite" for QuickSort → two instances

Request: "Create a test suite covering QuickSort on arrays of size 0, 1, 2, and 1000."
  "create" → IMPLEMENT | "test suite" → TEST
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F044-003 — Zero predicate: "partition the array" → empty factory

Request: "Partition the array around the chosen pivot element using QuickSort logic."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
