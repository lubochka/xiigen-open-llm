# Layer 0-D — Flow Factory Pairs (045) — HeapSort: factory selection for heap domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F045-001 — TEST+IMPLEMENT: "generate HeapSort and write tests" → two instances

Request: "Generate the HeapSort class and write tests for the heapify step with varied input sizes."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F045-002 — TEST alone: "validate" on HeapSort → one UnitTestingFlow51, documentation

Request: "Validate that HeapSort correctly handles arrays containing duplicate values."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F045-003 — Zero predicate: "heapify the array" → empty factory

Request: "Heapify the array before extracting the maximum element."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
