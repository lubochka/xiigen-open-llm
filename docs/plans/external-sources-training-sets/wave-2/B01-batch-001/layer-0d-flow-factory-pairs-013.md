# Layer 0-D — Flow Factory Selection Pairs (013)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B01-0D-013-001 — trusted_learned: ScreenInventoryFlow11 fully promoted for UserProfilePage

```
source_id: xiigen-b01-0d-013-001
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  context_hint: "UserProfilePage — avatar, display name, bio form"
  flow_registry_state:
    registered_flow_id: "screen-inventory-flow-1.1-v3"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.90; production_evidence_pairs: 28"
  promotion_threshold_met: true

B:
  SelectedFlowId: "screen-inventory-flow-1.1-v3"
  TrustState: "trusted_learned"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

---

## B01-0D-013-002 — deterministic_generic: 1.1 — no learned flow for NotificationCenter context

```
source_id: xiigen-b01-0d-013-002
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  context_hint: "NotificationCenter — list, detail, empty state"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
    gate_b_passed: null
    gate_b_reason: null
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B01-0D-013-003 — probationary_rejected: candidate flow for SettingsDashboard fails Gate B

```
source_id: xiigen-b01-0d-013-003
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  context_hint: "SettingsDashboard — account, security, billing"
  flow_registry_state:
    registered_flow_id: "settings-inventory-flow-1.1-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 4 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "settings-inventory-flow-1.1-candidate"
  Warnings:
    - "probationary_flow_rejected:settings-inventory-flow-1.1-candidate"
    - "gate_b_failed:4_of_9_nodes_covered"
    - "fallback_used:universal-nine-node-development-flow-1.1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"
