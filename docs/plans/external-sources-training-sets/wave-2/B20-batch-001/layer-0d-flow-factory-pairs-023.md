# Layer 0-D — Flow Factory Pairs (023)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F023-001 — TEST+IMPLEMENT / 5.1+4.1 → two flows (CouponApplicatorTests)

A:
  sub_phase: [5.1, 4.1]
  predicate: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the CouponApplicator to verify valid coupons reduce cart total and expired coupons are rejected."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1

---

## F023-002 — TEST+IMPLEMENT / 5.1+4.1 → two flows (create test cases with mock)

A:
  sub_phase: [5.1, 4.1]
  predicate: ["TEST", "IMPLEMENT"]
  compiled_text: "Create test cases for CouponApplicator that use mock coupon repositories to isolate applicator logic."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1

---

## F023-003 — ZERO predicates → NoFlowCreated (run coupon test suite)

A:
  sub_phase: null
  predicate: []
  compiled_text: "Run the coupon test suite after deploying the new stacking rules."

B:
  TrustState: null
  FlowInstances: []
  SelectionReason: "Zero predicates from 'run' → no flow; knowledge_edge"
