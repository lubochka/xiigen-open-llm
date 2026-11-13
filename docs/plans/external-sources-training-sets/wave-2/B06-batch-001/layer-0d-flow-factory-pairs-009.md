# Layer 0-D — Flow Factory Selection Pairs (009)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 009 covers the full four-predicate factory map: 4 routing decisions → 4 instantiations.

---

## B06-0D-009-001 — Full four-predicate factory: 4 instantiations

```
source_id: xiigen-b06-0d-009-001
source_title: XIIGen B06 factory — four predicate four instantiation
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
pair_kind: factory_selection
```

A:
  routing_decisions:
    - sub_phase_id: "3.1"
      evidence_predicate: "ARCHITECTURE"
      flow_registry_state: { registered_flow_id: null }
      fallback_flow_id: "universal-nine-node-development-flow-3.1"

    - sub_phase_id: "2.1"
      evidence_predicate: "DOCUMENT"
      flow_registry_state: { registered_flow_id: null }
      fallback_flow_id: "universal-nine-node-development-flow-2.1"

    - sub_phase_id: "4.1"
      evidence_predicate: "IMPLEMENT"
      flow_registry_state: { registered_flow_id: null }
      fallback_flow_id: "universal-nine-node-development-flow-4.1"

    - sub_phase_id: "1.1"
      evidence_predicate: "UI"
      flow_registry_state: { registered_flow_id: null }
      fallback_flow_id: "universal-nine-node-development-flow-1.1"

B:
  FlowInstantiations:
    - sub_phase_id: "3.1"
      SelectedFlowId: "universal-nine-node-development-flow-3.1"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
      Warnings: []

    - sub_phase_id: "2.1"
      SelectedFlowId: "universal-nine-node-development-flow-2.1"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
      Warnings: []

    - sub_phase_id: "4.1"
      SelectedFlowId: "universal-nine-node-development-flow-4.1"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
      Warnings: []

    - sub_phase_id: "1.1"
      SelectedFlowId: "universal-nine-node-development-flow-1.1"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
      Warnings: []

  InstantiationCount: 4
  TransitionType: "feature_addition"

---

## B06-0D-009-002 — Four-predicate factory with mixed trust states

```
source_id: xiigen-b06-0d-009-002
source_title: XIIGen B06 factory — four predicate mixed trust states
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
pair_kind: factory_selection
```

A:
  routing_decisions:
    - sub_phase_id: "2.1"
      flow_registry_state:
        registered_flow_id: "requirements-authoring-flow-2.1-v1"
        trust_state: "trusted_learned"
        gate_b_passed: true

    - sub_phase_id: "3.1"
      flow_registry_state: { registered_flow_id: null }

    - sub_phase_id: "4.1"
      flow_registry_state:
        registered_flow_id: "impl-flow-4.1-candidate"
        trust_state: "probationary"
        gate_b_passed: false
        gate_b_reason: "insufficient_node_coverage: 5 of 9"

    - sub_phase_id: "1.1"
      flow_registry_state: { registered_flow_id: null }

B:
  FlowInstantiations:
    - sub_phase_id: "2.1"
      SelectedFlowId: "requirements-authoring-flow-2.1-v1"
      TrustState: "trusted_learned"
      SelectionOutcome: "TRUSTED_LEARNED_SELECTED"
      Warnings: []

    - sub_phase_id: "3.1"
      SelectedFlowId: "universal-nine-node-development-flow-3.1"
      TrustState: "deterministic_generic"
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
      Warnings: []

    - sub_phase_id: "4.1"
      SelectedFlowId: "universal-nine-node-development-flow-4.1"
      TrustState: "deterministic_generic"
      FallbackUsed: true
      RejectedCandidateFlowId: "impl-flow-4.1-candidate"
      SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"
      Warnings:
        - "probationary_flow_rejected:impl-flow-4.1-candidate"
        - "gate_b_failed:insufficient_node_coverage:5_of_9"
        - "fallback_used:universal-nine-node-development-flow-4.1"

    - sub_phase_id: "1.1"
      SelectedFlowId: "universal-nine-node-development-flow-1.1"
      TrustState: "deterministic_generic"
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
      Warnings: []

  InstantiationCount: 4
  Teaching: Each sub-phase factory selection is independent.
    2.1 uses trusted_learned; 4.1 rejects probationary and uses fallback.
    These independent outcomes do not affect each other.
