# Layer 0-D — Flow Factory Selection Pairs (015)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-015-001 — InventoryAllocationService: deterministic_generic

A:
  sub_phase_id: "4.1"
  service: "InventoryAllocationService"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B15-factory-015-002 — InventoryAllocationService: probationary_accepted with SECURITY context

A:
  sub_phase_id: "4.1"
  predicates: ["IMPLEMENT", "SECURITY"]
  flow_registry_state:
    registered_flow_id: "inventory-allocation-flow-4.1-v2"
    trust_state: "probationary"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.91; evidence_pairs: 12 of 25"
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "inventory-allocation-flow-4.1-v2"
  TrustState: "probationary_accepted"
  FallbackUsed: false
  Warnings:
    - "probationary_flow_accepted: gate_b_passed but promotion_threshold not yet met (12/25)"
  SelectionOutcome: "PROBATIONARY_ACCEPTED"
  Note: "SECURITY predicate fires SecurityAuditFlow in addition to InventoryAllocationService ImplementationFlow41."

---

## B15-factory-015-003 — Multi-predicate factory: IMPLEMENT+API_CONTRACT → two factories

A:
  predicates: ["IMPLEMENT", "API_CONTRACT"]
  sub_phase_ids: ["4.1", "3.3"]
  flow_registry_state_4.1:
    registered_flow_id: null
  flow_registry_state_3.3:
    registered_flow_id: null

B:
  SelectedFlows:
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic"
    - SubPhaseId: "3.3" | FlowId: "universal-nine-node-contract-flow-3.3" | TrustState: "deterministic_generic"
  Note: "Two predicates → two factory calls → two flow selections. Factory is called once per predicate."
