# Layer 0-D — Flow Factory Selection Pairs (025)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-025-001 — HeroSection: trusted_learned flow for sub-phase 1.1

```
source_id: xiigen-b01-0d-025-001
source_license_state: first_party
pair_kind: flow_factory
```

A: sub_phase_id: "1.1", trust_state: "trusted_learned", scenario: "HeroSection component build"
B: SelectedFlowId: "screen-inventory-flow-1.1-v3" | SelectionOutcome: "TRUSTED_LEARNED_SELECTED" | FallbackUsed: false | Warnings: []

---

## B01-0D-025-002 — HeroSection: no candidate for sub-phase 4.1 → deterministic_generic

```
source_id: xiigen-b01-0d-025-002
source_license_state: first_party
pair_kind: flow_factory
```

A: sub_phase_id: "4.1", trust_state: null, scenario: "HeroSection IMPLEMENT branch"
B: SelectedFlowId: "universal-nine-node-development-flow-4.1" | SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED" | FallbackUsed: false

Rule: FallbackUsed is false for a clean deterministic_generic selection (no candidate was proposed then rejected).

---

## B01-0D-025-003 — ProductCard: probationary flow rejected → fallback for sub-phase 1.1

```
source_id: xiigen-b01-0d-025-003
source_license_state: first_party
pair_kind: flow_factory
```

A: sub_phase_id: "1.1", trust_state: "probationary_rejected", gate_b_passed: false, scenario: "ProductCard"
B: SelectedFlowId: "universal-nine-node-development-flow-1.1" | FallbackUsed: true | SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"
   Warnings: ["probationary_flow_rejected", "gate_b_failed", "fallback_used"]

Rule: probationary + gate_b=false → rejected + FallbackUsed: true always.
