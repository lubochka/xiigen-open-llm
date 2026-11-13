# Layer 0-D — Flow Factory Pairs (054) — NumberFormatter: factory selection for number format domain
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F054-001 — TEST+IMPLEMENT: "develop NumberFormatter and add test cases" → two instances

Request: "Develop the NumberFormatter and add test cases for thousands separators and decimal precision."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F054-002 — TEST alone: "assert" on NumberFormatter zero-value edge case → one UnitTestingFlow51, documentation

Request: "Assert that the NumberFormatter displays 0 as '0.00' when precision is set to 2."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F054-003 — Zero predicate: "round the number" → empty factory

Request: "Round the number to two decimal places using the NumberFormatter precision setting."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
