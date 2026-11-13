# Layer 0-D — Flow Factory Pairs (025) — InterestCalculator: factory output for TEST and TEST+IMPLEMENT
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F025-001 — TEST alone: factory produces one UnitTestingFlow51

Request: "Validate that the InterestCalculator returns correct simple interest."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  One flow instance. TransitionType: documentation.
  Service: InterestCalculatorTests | Expected node count: 9

---

## F025-002 — TEST+IMPLEMENT: factory produces two flow instances

Request: "Write unit tests for the InterestCalculator and implement the simple interest formula."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [UnitTestingFlow51, BackendServiceFlow41]
  Two flow instances. Execution order: 4.1 → 5.1 (implement first, then test).
  TransitionType: feature_addition

---

## F025-003 — Zero predicates: factory produces empty list

Request: "Check that the InterestCalculator returns correct interest."
  FlowFactory.CreateMulti([]) → []
  Zero flow instances. No nine-node flow is started.
  CapabilityGapSignal raised. Request logged as knowledge_edge.
