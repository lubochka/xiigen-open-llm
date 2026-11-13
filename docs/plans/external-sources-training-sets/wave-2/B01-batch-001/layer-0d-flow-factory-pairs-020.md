# Layer 0-D — Flow Factory Selection Pairs (020)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## Method note (020)

Multi-factory selection: when multiple routing decisions fire (multiple predicates),
each sub-phase's factory selection runs independently. This file shows parallel factory
selections for the same request with UI+DOCUMENT+IMPLEMENT predicates.

---

## B01-0D-020-001 — multi-factory: UI → 1.1 (trusted_learned), DOCUMENT → 2.1 (deterministic_generic)

```
source_id: xiigen-b01-0d-020-001
source_license_state: first_party
pair_kind: flow_factory
```

A:
  routing_decisions:
    - sub_phase_id: "1.1"
      evidence_predicates: ["UI"]
    - sub_phase_id: "2.1"
      evidence_predicates: ["DOCUMENT"]

  factory_inputs:
    sub_phase_1.1:
      registered_flow_id: "screen-inventory-flow-1.1-v3"
      trust_state: "trusted_learned"
      gate_b_passed: true
    sub_phase_2.1:
      registered_flow_id: null
      trust_state: null

B:
  factory_selections:
    sub_phase_1.1:
      SelectedFlowId: "screen-inventory-flow-1.1-v3"
      TrustState: "trusted_learned"
      SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

    sub_phase_2.1:
      SelectedFlowId: "universal-nine-node-development-flow-2.1"
      TrustState: "deterministic_generic"
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

Multi-factory rule:
  Each sub-phase's factory selection is independent.
  A trusted_learned flow for 1.1 does NOT affect 2.1's selection.
  Each factory examines only its own sub-phase registry.

---

## B01-0D-020-002 — multi-factory: UI+IMPLEMENT — two factories run in parallel

```
source_id: xiigen-b01-0d-020-002
source_license_state: first_party
pair_kind: flow_factory
```

A:
  routing_decisions:
    - sub_phase_id: "1.1"
    - sub_phase_id: "4.1"

  factory_inputs:
    sub_phase_1.1:
      registered_flow_id: null
    sub_phase_4.1:
      registered_flow_id: "implementation-flow-4.1-v2"
      trust_state: "trusted_learned"
      gate_b_passed: true

B:
  factory_selections:
    sub_phase_1.1:
      SelectedFlowId: "universal-nine-node-development-flow-1.1"
      TrustState: "deterministic_generic"
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

    sub_phase_4.1:
      SelectedFlowId: "implementation-flow-4.1-v2"
      TrustState: "trusted_learned"
      SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

---

## B01-0D-020-003 — multi-factory: all four sub-phases, mixed trust states

```
source_id: xiigen-b01-0d-020-003
source_license_state: first_party
pair_kind: flow_factory
```

A: [Four-predicate request — UI+DOCUMENT+IMPLEMENT+ARCHITECTURE]
  routing_decisions: [{1.1}, {2.1}, {3.1}, {4.1}]

B:
  factory_selections:
    sub_phase_1.1:
      SelectedFlowId: "screen-inventory-flow-1.1-v3"
      TrustState: "trusted_learned"
      SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

    sub_phase_2.1:
      SelectedFlowId: "universal-nine-node-development-flow-2.1"
      TrustState: "deterministic_generic"
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

    sub_phase_3.1:
      SelectedFlowId: "universal-nine-node-development-flow-3.1"
      TrustState: "deterministic_generic"
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

    sub_phase_4.1:
      SelectedFlowId: "implementation-flow-4.1-v2"
      TrustState: "trusted_learned"
      SelectionOutcome: "TRUSTED_LEARNED_SELECTED"
