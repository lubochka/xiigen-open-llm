# Layer 0-D — Flow Factory Selection Pairs (022)
# Batch: B15-batch-001 | Scenario: B15 — IMPLEMENT 4.1 backend services
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B15-factory-022-001 — PricingEngineService: deterministic_generic (refactoring)

A:
  sub_phase_id: "4.1"
  service: "PricingEngineService"
  transition_type: "refactoring"
  flow_registry_state: registered_flow_id: null

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  TransitionTypePassthrough: "refactoring"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B15-factory-022-002 — PricingEngineService: trusted_learned (refactoring)

A:
  sub_phase_id: "4.1"
  transition_type: "refactoring"
  flow_registry_state:
    registered_flow_id: "pricing-engine-flow-4.1"
    trust_state: "trusted_learned"
    gate_b_passed: true
    production_evidence_pairs: 28

B:
  SelectedFlowId: "pricing-engine-flow-4.1"
  TrustState: "trusted_learned"
  TransitionTypePassthrough: "refactoring"
  FallbackUsed: false
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"
  Note: "TransitionType: refactoring is passed through even for trusted_learned flows."

---

## B15-factory-022-003 — IMPLEMENT+TEST (refactoring) + DOCUMENT: four factories

A:
  predicates: ["IMPLEMENT", "API_CONTRACT", "TEST", "DOCUMENT"]
  transition_type: "refactoring"

B:
  SelectedFlows:
    - SubPhaseId: "3.3" | FlowId: "universal-nine-node-contract-flow-3.3"
    - SubPhaseId: "4.1" | FlowId: "universal-nine-node-development-flow-4.1" | TransitionTypePassthrough: "refactoring"
    - SubPhaseId: "5.1" | FlowId: "universal-nine-node-testing-flow-5.1"
    - SubPhaseId: "2.1" | FlowId: "universal-nine-node-documentation-flow-2.1"
  Note: "TransitionType: refactoring applies only to ImplementationFlow41 (sub-phase 4.1). APIContractFlow33, TestingFlow51, and DocumentationFlow21 are not affected by TransitionType."
