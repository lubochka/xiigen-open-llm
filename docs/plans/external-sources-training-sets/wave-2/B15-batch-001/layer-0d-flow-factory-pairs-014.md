# Layer 0-D — Flow Factory Selection Pairs (014)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-014-001 — RefundService: deterministic_generic

A:
  sub_phase_id: "4.1"
  service: "RefundAndChargebackService"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B15-factory-014-002 — RefundService: trusted_learned

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: "payment-reversal-flow-4.1"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.93; production_evidence_pairs: 31"
  fallback_flow_id: "universal-nine-node-development-flow-4.1"
  promotion_threshold_met: true

B:
  SelectedFlowId: "payment-reversal-flow-4.1"
  TrustState: "trusted_learned"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

---

## B15-factory-014-003 — TransitionType: refactoring in factory selection

A:
  sub_phase_id: "4.1"
  transition_type: "refactoring"
  flow_registry_state:
    registered_flow_id: "payment-reversal-flow-4.1"
    trust_state: "trusted_learned"

B:
  SelectedFlowId: "payment-reversal-flow-4.1"
  TrustState: "trusted_learned"
  FallbackUsed: false
  TransitionTypePassthrough: "refactoring"
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"
  Note: "TransitionType: refactoring is passed through to flow execution as metadata. Factory selection outcome does not change — same flow selected regardless of TransitionType."
