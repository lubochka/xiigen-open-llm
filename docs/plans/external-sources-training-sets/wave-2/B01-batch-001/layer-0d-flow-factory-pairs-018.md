# Layer 0-D — Flow Factory Selection Pairs (018)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B01-0D-018-001 — trusted_learned: sub-phase 1.1 — checkout screen inventory flow

```
source_id: xiigen-b01-0d-018-001
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  context_hint: "Checkout and payment screens"
  flow_registry_state:
    registered_flow_id: "checkout-screen-flow-1.1-v1"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; production_evidence_pairs: 29"
  promotion_threshold_met: true

B:
  SelectedFlowId: "checkout-screen-flow-1.1-v1"
  TrustState: "trusted_learned"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

---

## B01-0D-018-002 — deterministic_generic: sub-phase 1.1 — accessibility audit context

```
source_id: xiigen-b01-0d-018-002
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  context_hint: "Accessibility audit for all screens"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
    gate_b_passed: null
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B01-0D-018-003 — probationary_accepted: design-system flow passes Gate B

```
source_id: xiigen-b01-0d-018-003
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  context_hint: "Design-system component inventory"
  flow_registry_state:
    registered_flow_id: "design-system-flow-1.1-candidate-v3"
    trust_state: "probationary"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.90; production_evidence_pairs: 20"
  promotion_threshold_met: false
  promotion_threshold_reason: "20 of 25 required"

B:
  SelectedFlowId: "design-system-flow-1.1-candidate-v3"
  TrustState: "probationary_accepted"
  FallbackUsed: false
  Warnings:
    - "probationary_flow_in_use:design-system-flow-1.1-candidate-v3"
    - "promotion_threshold_not_met:20_of_25_production_evidence_pairs"
    - "monitoring_required"
  SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"
