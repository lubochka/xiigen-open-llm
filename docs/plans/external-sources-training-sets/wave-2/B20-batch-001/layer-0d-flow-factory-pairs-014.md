# Layer 0-D — Flow Factory Pairs (014)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F014-001 — TEST+IMPLEMENT / 5.1+4.1 → two flows (InventoryAllocationTests)

A:
  sub_phase: [5.1, 4.1]
  predicate: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the InventoryAllocationService to verify that stock is reserved correctly."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
  SelectionReason: "Two predicates → two independent flow instances"

---

## F014-002 — TEST alone / 5.1 → single flow (assert over-allocation)

A:
  sub_phase: 5.1
  predicate: ["TEST"]
  compiled_text: "Assert that the InventoryAllocationService raises InsufficientStockException when quantity exceeds stock."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
  SelectionReason: "TEST alone — single flow; documentation TransitionType"
