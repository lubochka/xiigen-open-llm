# Layer 0-D — Flow Factory Pairs (028) — ROICalculator: factory path for "add and test"
# Batch: B20-batch-001 | Scenario: B20 — Business logic algorithms and unit testing
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F028-001 — TEST alone: "spec" → factory produces UnitTestingFlow51

Request: "Spec the test suite for the ROICalculator covering positive, zero, and negative ROI scenarios."
  FlowFactory.CreateMulti(["TEST"]) → [UnitTestingFlow51]
  TransitionType: documentation. Service: ROICalculatorTests.

---

## F028-002 — TEST+IMPLEMENT: "add and test" → factory produces two instances

Request: "Add the ROICalculator and test its calculation against known benchmarks."
  FlowFactory.CreateMulti(["TEST", "IMPLEMENT"]) → [BackendServiceFlow41, UnitTestingFlow51]
  TransitionType: feature_addition. Two instances, execution order 4.1 → 5.1.

---

## F028-003 — Zero predicates: "invoke" → factory returns empty list immediately

Request: "Invoke the ROICalculator with a £10,000 investment and £2,500 gain."
  CompiledPredicateIds: []
  FlowFactory.CreateMulti([]) → []
  Factory terminates immediately with empty list. No flow instance allocated.
  CapabilityGapSignal raised: gap-invoke-not-implement-028
  Note: "invoke" is execution, not construction. The factory has no handling path for execution-only requests.
