# Layer 0-D — Flow Factory Pairs (018)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F018-001 — TEST+IMPLEMENT / 5.1+4.1 → two flows (LoyaltyPointsTests)

A:
  sub_phase: [5.1, 4.1]
  predicate: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the LoyaltyPointsService verifying that points are awarded at the correct rate per pound spent."

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

## F018-002 — TEST alone / 5.1 → single flow (spec tier multiplier)

A:
  sub_phase: 5.1
  predicate: ["TEST"]
  compiled_text: "Spec the loyalty tier multiplier: gold 3x, silver 2x, standard 1x."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
  SelectionReason: "TEST alone via 'spec' → single flow; documentation TransitionType"

---

## F018-003 — TEST+IMPLEMENT / 5.1+4.1 → two flows (create test cases)

A:
  sub_phase: [5.1, 4.1]
  predicate: ["TEST", "IMPLEMENT"]
  compiled_text: "Create unit test cases for LoyaltyPointsService covering point accrual, manual adjustment, and account closure scenarios."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
