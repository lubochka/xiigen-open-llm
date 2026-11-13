# Layer 0-D — Flow Factory Selection Pairs (017)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B01-0D-017-001 — deterministic_generic: sub-phase 3.1 architecture flow

```
source_id: xiigen-b01-0d-017-001
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "3.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
    gate_b_passed: null
  fallback_flow_id: "universal-nine-node-development-flow-3.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-3.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B01-0D-017-002 — trusted_learned: cross-context — 1.1 for multi-screen inventory

```
source_id: xiigen-b01-0d-017-002
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  context_hint: "Multi-screen inventory covering UserProfile, Notification, Settings"
  flow_registry_state:
    registered_flow_id: "multi-screen-inventory-flow-1.1-v2"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; production_evidence_pairs: 31"
  promotion_threshold_met: true

B:
  SelectedFlowId: "multi-screen-inventory-flow-1.1-v2"
  TrustState: "trusted_learned"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

---

## B01-0D-017-003 — probationary_rejected: low-coverage candidate for sub-phase 2.1

```
source_id: xiigen-b01-0d-017-003
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: "requirements-flow-2.1-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 3 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "requirements-flow-2.1-candidate"
  Warnings:
    - "probationary_flow_rejected:requirements-flow-2.1-candidate"
    - "gate_b_failed:3_of_9_nodes_covered"
    - "fallback_used:universal-nine-node-development-flow-2.1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"
