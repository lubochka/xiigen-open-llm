# Layer 0-D — Flow Factory Pairs (043) — MergeSort: factory with "test fixture" → documentation
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F043-001 — TEST alone: "test fixture" for MergeSort → one UnitTestingFlow51, documentation

Request: "Set up a test fixture with large randomised arrays to benchmark the MergeSort performance."
  "test fixture" → TEST (compound). No construction verb.
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F043-002 — TEST+IMPLEMENT: "build MergeSort and write unit tests" → two instances

Request: "Build the MergeSort class and write unit tests for the merge step with overlapping ranges."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F043-003 — Zero predicate: "merge the sub-arrays" → empty factory

Request: "Merge the sub-arrays produced in the divide phase of the MergeSort."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
