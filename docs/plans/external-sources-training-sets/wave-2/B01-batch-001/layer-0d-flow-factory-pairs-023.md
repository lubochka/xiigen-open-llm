# Layer 0-D — Flow Factory Selection Pairs (023)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## Method note (023)

v6-01 invented flows — additional factory corruption patterns.
v6-01 in the factory context: the factory returns a flow for a sub-phase that was never routed to.

---

## B01-0D-023-001 — v6-01 factory corruption: returning a 1.1 flow when input was 2.1

```
source_id: xiigen-b01-0d-023-001
source_license_state: first_party
pair_kind: flow_factory
corruption_type: Corruption-v6-01-wrong-subphase-flow
```

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B-correct:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

B-corrupt (v6-01 wrong sub-phase):
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

Corruption analysis:
  B-corrupt returns the 1.1 flow for a 2.1 sub-phase request.
  The 1.1 flow (screen inventory) executes business requirements work.
  The output is an invalid screen inventory artifact instead of a requirements artifact.

---

## B01-0D-023-002 — v6-01 factory: trusted_learned flow from wrong sub-phase applied

```
source_id: xiigen-b01-0d-023-002
source_license_state: first_party
pair_kind: flow_factory
corruption_type: Corruption-v6-01-trusted-learned-wrong-subphase
```

A:
  sub_phase_id: "3.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-3.1"

B-correct:
  SelectedFlowId: "universal-nine-node-development-flow-3.1"
  TrustState: "deterministic_generic"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

B-corrupt (v6-01):
  SelectedFlowId: "screen-inventory-flow-1.1-v3"
  TrustState: "trusted_learned"
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

Corruption analysis:
  B-corrupt applies the trusted_learned 1.1 flow to a 3.1 (architecture) request.
  The factory must NEVER use a trusted_learned flow from the wrong sub-phase.
  Even a high-quality trusted_learned flow produces incorrect outputs when applied
  to a sub-phase it was not trained for.

---

## B01-0D-023-003 — correct factory boundary: each sub-phase looks up only its own registry

```
source_id: xiigen-b01-0d-023-003
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: "implementation-flow-4.1-v2"
    trust_state: "trusted_learned"
    gate_b_passed: true
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "implementation-flow-4.1-v2"
  TrustState: "trusted_learned"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

Boundary rule confirmed:
  Factory for 4.1 uses only the 4.1 registry. It cannot see 1.1 or 2.1 flows.
  Cross-sub-phase flow application is Corruption-v6-01.
