# Layer 0-D — Flow Factory Pairs (013)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F013-001 — TEST / 5.1 → UnitTestingFlow51 (CartCalculationTests)

A:
  sub_phase: 5.1
  predicate: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the CartCalculationService verifying item totals, quantity adjustments, and empty-cart edge cases."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
      SelectionReason: "TEST predicate at 5.1 — established UnitTestingFlow51 pattern"
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
      SelectionReason: "IMPLEMENT predicate at 4.1 — service logic implementation flow"
  PrimaryFlowId: "UnitTestingFlow51"

---

## F013-002 — TEST alone / 5.1 → UnitTestingFlow51 (assert subtotal)

A:
  sub_phase: 5.1
  predicate: ["TEST"]
  compiled_text: "Assert that the CartCalculationService returns the correct subtotal."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
      SelectionReason: "TEST alone at 5.1 — single flow instance; documentation TransitionType"
  Note: TEST alone produces one flow instance. No IMPLEMENT companion flow is created.
