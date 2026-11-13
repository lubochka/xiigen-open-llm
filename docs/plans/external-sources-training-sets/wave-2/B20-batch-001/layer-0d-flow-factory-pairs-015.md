# Layer 0-D — Flow Factory Pairs (015)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F015-001 — TEST+IMPLEMENT / 5.1+4.1 → two flows (DiscountEngineTests)

A:
  sub_phase: [5.1, 4.1]
  predicate: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the DiscountEngine verifying percentage, fixed-amount, and BOGO discount logic."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
  SelectionReason: "TEST+IMPLEMENT → two flows; deterministic_generic"

---

## F015-002 — TEST alone / 5.1 → single flow (validate minimum threshold)

A:
  sub_phase: 5.1
  predicate: ["TEST"]
  compiled_text: "Validate that the DiscountEngine applies 15% discount only when cart total exceeds minimum threshold."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
  SelectionReason: "TEST alone via 'validate' — single flow; documentation TransitionType"
  Note: "validate" fires TEST universally. No IMPLEMENT accompanies it here.
