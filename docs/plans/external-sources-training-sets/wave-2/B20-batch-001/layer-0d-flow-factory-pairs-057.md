# Layer 0-D — Flow Factory Pairs (057) — StatisticsCalc: factory selection for statistics domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F057-001 — TEST+IMPLEMENT: "create StatisticsCalc and write tests" → two instances

Request: "Create the StatisticsCalc class and write tests for mean, median, and standard deviation."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F057-002 — TEST alone: "validate" on StatisticsCalc empty dataset → one UnitTestingFlow51, documentation

Request: "Validate that the StatisticsCalc throws InvalidOperationException when passed an empty dataset."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F057-003 — Zero predicate: "compute the mean" → empty factory

Request: "Compute the mean of the dataset using StatisticsCalc."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
