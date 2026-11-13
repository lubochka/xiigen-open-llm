# Layer 0-D — Flow Factory Selection Pairs (026)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-026-001 — FilterPanel: accepted probationary for sub-phase 1.1

```
source_id: xiigen-b01-0d-026-001
source_license_state: first_party
pair_kind: flow_factory
```

A: sub_phase_id: "1.1", trust_state: "probationary_accepted", gate_b_passed: true, promotion_threshold_met: false, scenario: "FilterPanel"
B: SelectedFlowId: [candidate flow] | FallbackUsed: false | SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"
   Warnings: ["probationary_flow_in_use", "promotion_threshold_not_met", "monitoring_required"]

Rule: probationary + gate_b=true → accepted + FallbackUsed: false (no fallback needed).

---

## B01-0D-026-002 — TooltipComponent: trusted_learned for sub-phase 1.1

```
source_id: xiigen-b01-0d-026-002
source_license_state: first_party
pair_kind: flow_factory
```

A: sub_phase_id: "1.1", trust_state: "trusted_learned", scenario: "TooltipComponent"
B: SelectedFlowId: "screen-inventory-flow-1.1-v3" | SelectionOutcome: "TRUSTED_LEARNED_SELECTED" | FallbackUsed: false | Warnings: []

---

## B01-0D-026-003 — Factory never crosses sub-phases: 1.1 flow is not used for 2.1

```
source_id: xiigen-b01-0d-026-003
source_license_state: first_party
pair_kind: flow_factory
```

Corrupt example (cross-sub-phase — Corruption-v6-01 at factory level):
  SelectedFlowId: "screen-inventory-flow-1.1-v3" applied to sub_phase_id: "2.1"  ← WRONG

Correct:
  sub_phase_id: "2.1" → SelectedFlowId: "universal-nine-node-development-flow-2.1" (generic fallback)
  Trusted_learned for 1.1 is never used for 2.1, 3.1, or 4.1.
