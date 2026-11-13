# Layer 0-D — Flow Factory Selection Pairs (018)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-018-001 — SearchService: deterministic_generic

A:
  sub_phase_id: "4.1"
  service: "SearchService"
  flow_registry_state: registered_flow_id: null

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B15-factory-018-002 — SearchService: TransitionType: refactoring passthrough

A:
  sub_phase_id: "4.1"
  transition_type: "refactoring"
  flow_registry_state: registered_flow_id: null

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  TransitionTypePassthrough: "refactoring"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
  Note: "refactoring TransitionType passes through to flow execution unchanged."

---

## B15-factory-018-003 — SearchService: IMPLEMENT+API_CONTRACT two factories

A:
  predicates: ["IMPLEMENT", "API_CONTRACT"]
  sub_phase_ids: ["4.1", "3.3"]

B:
  SelectedFlows:
    - SubPhaseId: "3.3" | FlowId: "universal-nine-node-contract-flow-3.3" | TrustState: "deterministic_generic"
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic"

---

## B15-factory-018-004 — probationary_rejected: SearchService flow

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: "search-service-flow-4.1-v1"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "N04 DomainProcessorUnit not covered; evidence_pairs: 4 of 25"
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "search-service-flow-4.1-v1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"
