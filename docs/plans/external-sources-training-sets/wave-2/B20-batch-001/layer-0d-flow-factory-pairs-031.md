# Layer 0-D — Flow Factory Pairs (031) — BreakEvenCalculator: "extend" and "scenario" factory
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F031-001 — TEST+IMPLEMENT: "extend and write tests" → two flow instances

Request: "Extend the BreakEvenCalculator to handle variable cost curves and write unit tests."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition. "extend" fires IMPLEMENT; "unit tests" fires TEST.

---

## F031-002 — TEST alone: "define scenario" + "verify" → one UnitTestingFlow51

Request: "Define a scenario where the BreakEvenCalculator receives a negative fixed cost and verify the result."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation. No construction verb → documentation TransitionType.

---

## F031-003 — Zero predicates: "make sure" → factory empty list

Request: "Make sure the BreakEvenCalculator handles zero contribution margin."
  FlowFactory.CreateMulti([]) → []
  CapabilityGapSignal raised: gap-make-sure-not-test-031
  Teaching: All four informal QA verbs (check, confirm, ensure, make sure) route to the same factory outcome: empty list → knowledge_edge.
