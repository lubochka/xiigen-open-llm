# Layer 0-D — Flow Factory Selection Pairs (006)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 006 covers DOCUMENT + IMPLEMENT factory: two flows instantiated for 2.1 and 4.1.

---

## B06-0D-006-001 — DOCUMENT+IMPLEMENT factory: RequirementsAuthoringFlow21 + ImplementationFlow41

```
source_id: xiigen-b06-0d-006-001
source_title: XIIGen B06 factory — DOCUMENT+IMPLEMENT two flow instantiation
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
pair_kind: factory_selection
```

A:
  routing_decisions:
    - sub_phase_id: "2.1"
      evidence_predicate: "DOCUMENT"
      flow_registry_state:
        registered_flow_id: null
        trust_state: null
      fallback_flow_id: "universal-nine-node-development-flow-2.1"

    - sub_phase_id: "4.1"
      evidence_predicate: "IMPLEMENT"
      flow_registry_state:
        registered_flow_id: null
        trust_state: null
      fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  FlowInstantiations:
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

  InstantiationCount: 2
  TransitionType: "feature_addition"

Factory selection rationale:
  Two routing decisions produce two flow instantiations.
  Both sub-phases use deterministic_generic flow (no learned flows registered).
  Both flows will execute their nine-node chains independently.

---

## B06-0D-006-002 — DOCUMENT+IMPLEMENT factory with learned 2.1 flow

```
source_id: xiigen-b06-0d-006-002
source_title: XIIGen B06 factory — DOCUMENT+IMPLEMENT with learned 2.1
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
        gate_b_reason: "all_9_nodes_covered; production_evidence_pairs: 31"
      fallback_flow_id: "universal-nine-node-development-flow-2.1"

    - sub_phase_id: "4.1"
      flow_registry_state:
        registered_flow_id: null
      fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  FlowInstantiations:
    - sub_phase_id: "2.1"
      SelectedFlowId: "requirements-authoring-flow-2.1-v1"
      TrustState: "trusted_learned"
      FallbackUsed: false
      SelectionOutcome: "TRUSTED_LEARNED_SELECTED"
      Warnings: []

    - sub_phase_id: "4.1"
      SelectedFlowId: "universal-nine-node-development-flow-4.1"
      TrustState: "deterministic_generic"
      FallbackUsed: false
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
      Warnings: []

  InstantiationCount: 2

---

## B06-0D-006-003 — Multi-flow instantiation independence

```
source_id: xiigen-b06-0d-006-003
source_title: XIIGen B06 factory — multi-flow independence
source_license_state: first_party
pair_kind: factory_selection
```

Teaching note — multi-flow instantiation independence:

  When multiple routing decisions produce multiple flow instantiations:
  - Each instantiation selects its own flow independently
  - A trusted_learned flow for 2.1 does NOT affect the 4.1 selection
  - A rejected candidate for 4.1 does NOT affect the 2.1 selection
  - Each sub-phase's factory selection is evaluated against that sub-phase's
    own flow registry state

  Factory does NOT:
    - Select a single flow to "cover" all sub-phases
    - Use the same trust state for all instantiations
    - Cancel an instantiation because another sub-phase's flow was rejected
