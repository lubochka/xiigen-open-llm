# Layer 0-D — Flow Factory Pairs (030) — IRRCalculator: "test-driven" and mock factory outputs
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F030-001 — TEST+IMPLEMENT: "implement and spec" → two flows in algorithm context

Request: "Implement the IRRCalculator using Newton-Raphson iteration and spec the unit tests."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition. Algorithm domain terms do not change factory output.

---

## F030-002 — TEST+IMPLEMENT: "test-driven develop" → two flows

Request: "Use a test-driven approach to develop the IRRCalculator iteration logic."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  Note: "test-driven" and "develop" are parsed independently. "test-driven" → TEST; "develop" → IMPLEMENT.
  Factory receives two predicates and produces two flow instances.

---

## F030-003 — TEST alone: "mock" + "assert" → one UnitTestingFlow51

Request: "Mock the cash flow data provider and assert the IRRCalculator handles non-convergent series."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation. "mock" and "assert" both fire TEST; deduplicated to one instance.
