# Layer 0-D — Flow Factory Selection Pairs (021)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## Method note (021)

Factory ordering rules. This file teaches:
  1. Factory evaluation order: trusted_learned > probationary_accepted > deterministic_generic
  2. Rejected candidates always yield deterministic_generic with FallbackUsed: true
  3. FallbackUsed: false for deterministic_generic when no candidate was ever proposed

---

## B01-0D-021-001 — ordering: trusted_learned beats probationary_accepted when both exist

```
source_id: xiigen-b01-0d-021-001
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "screen-inventory-flow-1.1-v3"
    trust_state: "trusted_learned"
    gate_b_passed: true
    production_evidence_pairs: 28
  secondary_candidate:
    registered_flow_id: "screen-inventory-flow-1.1-candidate-v4"
    trust_state: "probationary"
    gate_b_passed: true
    production_evidence_pairs: 16
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B:
  SelectedFlowId: "screen-inventory-flow-1.1-v3"
  TrustState: "trusted_learned"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

Factory ordering rule:
  trusted_learned always wins. The probationary candidate is not evaluated when a trusted_learned
  flow is registered for the same sub-phase.

---

## B01-0D-021-002 — ordering: deterministic_generic used only when no candidate exists at all

```
source_id: xiigen-b01-0d-021-002
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

FallbackUsed rule:
  FallbackUsed: false — because no candidate was proposed and rejected.
  FallbackUsed is only true when a candidate was proposed, evaluated, and rejected.

---

## B01-0D-021-003 — ordering: FallbackUsed: true only after rejection

```
source_id: xiigen-b01-0d-021-003
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "screen-inventory-flow-1.1-candidate-low"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "2 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "screen-inventory-flow-1.1-candidate-low"
  Warnings:
    - "probationary_flow_rejected:screen-inventory-flow-1.1-candidate-low"
    - "gate_b_failed:2_of_9_nodes_covered"
    - "fallback_used:universal-nine-node-development-flow-1.1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

FallbackUsed rule confirmed:
  Candidate was proposed → Gate B failed → rejected → FallbackUsed: true.
