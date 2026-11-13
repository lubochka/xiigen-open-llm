# Layer 0-D — Flow Factory Pairs (038) — SearchAlgorithm: factory selection for search domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F038-001 — TEST+IMPLEMENT: "develop and spec SearchAlgorithm" → two instances

Request: "Develop the SearchAlgorithm class and spec the linear search behaviour for null inputs."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F038-002 — TEST alone: "verify" on SearchAlgorithm → one UnitTestingFlow51, documentation

Request: "Verify that the SearchAlgorithm returns the correct index when the target element is present."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F038-003 — Zero predicate: "invoke search" → empty factory

Request: "Invoke the SearchAlgorithm and confirm the result is the expected index."
  "invoke" → execution verb; "confirm" → informal QA. Both produce zero predicates.
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
