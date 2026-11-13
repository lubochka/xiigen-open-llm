# Layer 0-D — Flow Factory Pairs (051) — DateTimeCalc: factory selection for datetime domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F051-001 — TEST+IMPLEMENT: "implement DateTimeCalc and write tests" → two instances

Request: "Implement the DateTimeCalc class and write tests for leap year boundary dates."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F051-002 — TEST alone: "validate" on DateTimeCalc → one UnitTestingFlow51, documentation

Request: "Validate that the DateTimeCalc rejects February 30 as an invalid date."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F051-003 — Zero predicate: "calculate date difference" → empty factory

Request: "Calculate the date difference between the start date and end date using DateTimeCalc."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
