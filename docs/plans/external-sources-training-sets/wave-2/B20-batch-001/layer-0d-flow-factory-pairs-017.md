# Layer 0-D — Flow Factory Pairs (017)
# Batch: B20-batch-001 | Scenario: B20 — Unit test design for billing calculation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F017-001 — TEST+IMPLEMENT / 5.1+4.1 → two flows (ShippingCostTests)

A:
  sub_phase: [5.1, 4.1]
  predicate: ["TEST", "IMPLEMENT"]
  compiled_text: "Write unit tests for the ShippingCostCalculator covering standard, express, and overnight rates by weight tier."

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

## F017-002 — ZERO predicates / null → NoFlowCreated (run unit tests)

A:
  sub_phase: null
  predicate: []
  compiled_text: "Run unit tests for the ShippingCostCalculator after the weight-tier refactor."

B:
  TrustState: null
  FlowInstances: []
  SelectionReason: "Zero predicates from 'run' execution verb → no flow instance created; knowledge_edge"
  CapabilityGapSignal:
    gap_id: "gap-run-not-test-factory-017"
    gap_vocabulary: ["run unit tests"]
  Note: The flow factory produces no flow when predicate list is empty. "Run" does not produce any FlowInstanceDescriptor.
