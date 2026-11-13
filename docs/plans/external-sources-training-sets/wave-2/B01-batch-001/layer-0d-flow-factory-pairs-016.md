# Layer 0-D — Flow Factory Selection Pairs (016)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B01-0D-016-001 — trusted_learned: AddressBook flow

```
source_id: xiigen-b01-0d-016-001
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "address-book-flow-1.1-v1"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; production_evidence_pairs: 27"
  promotion_threshold_met: true

B:
  SelectedFlowId: "address-book-flow-1.1-v1"
  TrustState: "trusted_learned"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

---

## B01-0D-016-002 — deterministic_generic: DeliveryOptions — no candidate

```
source_id: xiigen-b01-0d-016-002
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
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

## B01-0D-016-003 — probationary_accepted: SearchAndFilter passes Gate B but not promoted

```
source_id: xiigen-b01-0d-016-003
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "search-filter-flow-1.1-candidate-v2"
    trust_state: "probationary"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.89; production_evidence_pairs: 18"
  promotion_threshold_met: false
  promotion_threshold_reason: "18 of 25 required"

B:
  SelectedFlowId: "search-filter-flow-1.1-candidate-v2"
  TrustState: "probationary_accepted"
  FallbackUsed: false
  Warnings:
    - "probationary_flow_in_use:search-filter-flow-1.1-candidate-v2"
    - "promotion_threshold_not_met:18_of_25_production_evidence_pairs"
    - "monitoring_required"
  SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"
