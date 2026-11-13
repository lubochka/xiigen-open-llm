# Layer 0-D — Flow Factory Selection Pairs (013)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-013-001 — OrderFulfillmentService: deterministic_generic

A:
  sub_phase_id: "4.1"
  service: "OrderFulfillmentService"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B15-factory-013-002 — OrderFulfillmentService: probationary_accepted

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: "fulfillment-service-flow-4.1-v1"
    trust_state: "probationary"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.90; evidence_pairs: 8 of 25"
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "fulfillment-service-flow-4.1-v1"
  TrustState: "probationary_accepted"
  FallbackUsed: false
  Warnings:
    - "probationary_flow_accepted: gate_b_passed but promotion_threshold not yet met (8/25)"
  SelectionOutcome: "PROBATIONARY_ACCEPTED"

---

## B15-factory-013-003 — OrderFulfillmentService: probationary_rejected

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: "fulfillment-service-flow-4.1-v0"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_node_coverage: N06 and N07 not covered"
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "fulfillment-service-flow-4.1-v0"
  Warnings:
    - "probationary_flow_rejected:fulfillment-service-flow-4.1-v0"
    - "gate_b_failed:N06_and_N07_not_covered"
    - "fallback_used:universal-nine-node-development-flow-4.1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"
