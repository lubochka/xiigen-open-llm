# Layer 0-D — Flow Factory Pairs (033) — BonusCalculator: factory with TEST alone → documentation
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F033-001 — TEST alone: "validate" on BonusCalculator → one UnitTestingFlow51, documentation

Request: "Validate that the BonusCalculator applies the quarterly bonus multiplier correctly."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation.

---

## F033-002 — TEST+IMPLEMENT: "create and write tests" → two instances

Request: "Create the BonusCalculator and write unit tests for the tiered bonus thresholds."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition.

---

## F033-003 — Zero predicate: "calculate bonus" → empty factory

Request: "Calculate the bonus for all employees in Q4 (Quarter 4) using the BonusCalculator."
  FlowFactory.CreateMulti([]) → []
  TransitionType: unknown | SourceStatus: knowledge_edge.
  Q4 (Quarter 4) → domain abbreviation; does not fire predicates.
