# Layer 0-D — Flow Factory Pairs (029) — NPVCalculator: "build" → IMPLEMENT flow, test harness factory
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F029-001 — TEST+IMPLEMENT: "build and create unit tests" → two flow instances

Request: "Build the NPVCalculator and create unit tests covering positive, negative, and zero NPV outcomes."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition. Service: NPVCalculatorTests.

---

## F029-002 — TEST+IMPLEMENT: "write test harness" → two flow instances

Request: "Write a test harness for the NPVCalculator that exercises all cash flow permutations."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  Note: "test harness" is in TEST lexicon; "write" fires IMPLEMENT. Two predicates → two instances.

---

## F029-003 — Zero predicates: "calculate" → factory empty, "update" → factory empty

Request A: "Calculate the NPV for a 5-year project." → FlowFactory.CreateMulti([]) → []
Request B: "Update the NPV calculator to accept variable rates." → FlowFactory.CreateMulti([]) → []
Both produce knowledge_edge. "calculate" and "update" are both outside all predicate lexicons.
Factory returns empty list in both cases. No flow instances allocated.
