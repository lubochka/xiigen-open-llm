# Layer 0-D — Flow Factory Selection Pairs (015)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B01-0D-015-001 — trusted_learned: CartView factory selection

```
source_id: xiigen-b01-0d-015-001
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  context_hint: "CartView and MiniCart — responsive layout, buttons"
  flow_registry_state:
    registered_flow_id: "cart-inventory-flow-1.1-v1"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; production_evidence_pairs: 26"
  promotion_threshold_met: true

B:
  SelectedFlowId: "cart-inventory-flow-1.1-v1"
  TrustState: "trusted_learned"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

---

## B01-0D-015-002 — deterministic_generic: WishlistPage — no learned flow yet

```
source_id: xiigen-b01-0d-015-002
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  context_hint: "WishlistPage and SavedItems"
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

## B01-0D-015-003 — probationary_rejected: OrderHistory candidate insufficient node coverage

```
source_id: xiigen-b01-0d-015-003
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  context_hint: "OrderHistoryPage and TrackingView"
  flow_registry_state:
    registered_flow_id: "order-history-flow-1.1-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 5 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "order-history-flow-1.1-candidate"
  Warnings:
    - "probationary_flow_rejected:order-history-flow-1.1-candidate"
    - "gate_b_failed:5_of_9_nodes_covered"
    - "fallback_used:universal-nine-node-development-flow-1.1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"
