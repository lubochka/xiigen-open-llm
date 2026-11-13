# Layer 0-D — Flow Factory Selection Pairs (014)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B01-0D-014-001 — probationary_accepted: SearchResults candidate passes Gate B

```
source_id: xiigen-b01-0d-014-001
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "search-results-inventory-flow-1.1-candidate"
    trust_state: "probationary"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.88; production_evidence_pairs: 15"
  promotion_threshold_met: false
  promotion_threshold_reason: "production_evidence_pairs: 15 of 25 required"

B:
  SelectedFlowId: "search-results-inventory-flow-1.1-candidate"
  TrustState: "probationary_accepted"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings:
    - "probationary_flow_in_use:search-results-inventory-flow-1.1-candidate"
    - "promotion_threshold_not_met:15_of_25_production_evidence_pairs"
    - "monitoring_required: compare output quality against universal fallback"
  SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"

---

## B01-0D-014-002 — deterministic_generic: sub-phase 2.1 no learned flow

```
source_id: xiigen-b01-0d-014-002
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
    gate_b_passed: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B01-0D-014-003 — trusted_learned: sub-phase 4.1 implementation flow promoted

```
source_id: xiigen-b01-0d-014-003
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: "implementation-flow-4.1-v2"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.91; production_evidence_pairs: 33"
  promotion_threshold_met: true

B:
  SelectedFlowId: "implementation-flow-4.1-v2"
  TrustState: "trusted_learned"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"
