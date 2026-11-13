# Layer 0-D — Flow Factory Pairs (032) — MarginCalculator: factory with "stub" and TransitionType documentation
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F032-001 — TEST+IMPLEMENT: "create and add test cases" → two instances

Request: "Create the MarginCalculator and add test cases for gross and net margin."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F032-002 — TEST+IMPLEMENT: "stub and write tests" → two instances

Request: "Stub the product cost provider and write tests for the MarginCalculator."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  Note: "stub" fires TEST (test-adjacent); "write" fires IMPLEMENT. Two predicates → two instances.

---

## F032-003 — TEST alone: three TEST verbs → one UnitTestingFlow51, TransitionType documentation

Request: "Validate, assert, and verify that the MarginCalculator handles 0% margin."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.
  Invariant: three TEST-firing verbs deduplicate to one predicate → one flow instance → one TransitionType: documentation.
  No construction verb means no feature_addition. Documentation applies.
