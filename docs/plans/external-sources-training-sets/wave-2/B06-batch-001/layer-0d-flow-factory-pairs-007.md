# Layer 0-D — Flow Factory Selection Pairs (007)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 007 covers DOCUMENT + UI factory: RequirementsAuthoringFlow21 + ScreenInventoryFlow11.

---

## B06-0D-007-001 — DOCUMENT+UI factory: 2.1 + 1.1

```
source_id: xiigen-b06-0d-007-001
source_title: XIIGen B06 factory — DOCUMENT+UI two flow instantiation
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
      fallback_flow_id: "universal-nine-node-development-flow-2.1"

    - sub_phase_id: "1.1"
      evidence_predicate: "UI"
      flow_registry_state:
        registered_flow_id: null
      fallback_flow_id: "universal-nine-node-development-flow-1.1"

B:
  FlowInstantiations:
    - sub_phase_id: "2.1"
      SelectedFlowId: "universal-nine-node-development-flow-2.1"
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

  InstantiationCount: 2
  TransitionType: "feature_addition"

---

## B06-0D-007-002 — ScreenInventoryFlow11 identity

```
source_id: xiigen-b06-0d-007-002
source_title: XIIGen B06 factory — ScreenInventoryFlow11 identity
source_license_state: first_party
pair_kind: factory_selection
```

Teaching note — ScreenInventoryFlow11:

  The universal-nine-node-development-flow-1.1 is the deterministic generic
  instantiation of ScreenInventoryFlow11.

  Flow identity:
    FlowId: "universal-nine-node-development-flow-1.1"
    Also referenced as: "ScreenInventoryFlow11"
    Sub-phase: 1.1 (Screen Inventory and UX Requirements)
    Purpose: Inventory all screens in the feature; document UX requirements
      per screen; define component hierarchy; specify interaction flows
    Node count: 9 (same nine-node structure as all other flows)

  B06 primary scope is 2.1. ScreenInventoryFlow11 appears in B06 only
  when UI predicate fires alongside DOCUMENT.

---

## B06-0D-007-003 — DOCUMENT+UI: TransitionType is feature_addition

```
source_id: xiigen-b06-0d-007-003
source_title: XIIGen B06 factory — DOCUMENT+UI TransitionType
source_license_state: first_party
pair_kind: factory_selection
```

Teaching note — DOCUMENT+UI TransitionType:

  When both DOCUMENT and UI predicates are compiled, TransitionType is
  feature_addition. The UI predicate signals new UX work is being created —
  this is feature addition work regardless of whether IMPLEMENT also fires.

  Rule: TransitionType = feature_addition if UI is present.
  Rule: TransitionType = feature_addition if IMPLEMENT is present.
  Rule: TransitionType = documentation if only DOCUMENT (and/or ARCHITECTURE).

  DOCUMENT + UI → feature_addition (UI presence is sufficient)
  DOCUMENT + UI + IMPLEMENT → feature_addition (both UI and IMPLEMENT present)
  DOCUMENT + ARCHITECTURE → documentation (no UI, no IMPLEMENT)
  DOCUMENT alone → documentation
