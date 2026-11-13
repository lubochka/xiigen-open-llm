# Layer 0-D — Flow Factory Selection Pairs (024)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## Complete B01 Factory Map

---

## B01-0D-024-001 — Complete factory state matrix

```
source_id: xiigen-b01-0d-024-matrix
source_license_state: first_party
pair_kind: flow_factory
```

### Trust state → SelectionOutcome mapping

| TrustState | GateBPassed | FallbackUsed | SelectionOutcome |
|---|---|---|---|
| trusted_learned | true | false | TRUSTED_LEARNED_SELECTED |
| probationary_accepted | true | false | ACCEPTED_PROBATIONARY_GATE_B_PASSED |
| probationary_rejected | false | true | REJECTED_PROBATIONARY_FALLBACK_USED |
| deterministic_generic (no candidate) | null | false | DETERMINISTIC_GENERIC_SELECTED |

### Warnings matrix

| Outcome | Warnings |
|---|---|
| TRUSTED_LEARNED_SELECTED | [] (empty) |
| ACCEPTED_PROBATIONARY_GATE_B_PASSED | probationary_flow_in_use + promotion_threshold_not_met + monitoring_required |
| REJECTED_PROBATIONARY_FALLBACK_USED | probationary_flow_rejected + gate_b_failed + fallback_used |
| DETERMINISTIC_GENERIC_SELECTED | [] (empty) |

### Sub-phase to flow ID mapping (B01 registry)

| SubPhaseId | Registered flow (if promoted) | Generic fallback |
|---|---|---|
| 1.1 | screen-inventory-flow-1.1-v3 (trusted_learned) | universal-nine-node-development-flow-1.1 |
| 2.1 | (none registered) | universal-nine-node-development-flow-2.1 |
| 3.1 | (none registered) | universal-nine-node-development-flow-3.1 |
| 4.1 | implementation-flow-4.1-v2 (trusted_learned) | universal-nine-node-development-flow-4.1 |

---

## B01-0D-024-002 — Factory summary pairs

A: sub_phase_id: "1.1", trust_state: "trusted_learned"
B: SelectedFlowId: "screen-inventory-flow-1.1-v3" | SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

A: sub_phase_id: "1.1", trust_state: null (no candidate)
B: SelectedFlowId: "universal-nine-node-development-flow-1.1" | SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

A: sub_phase_id: "1.1", trust_state: "probationary", gate_b_passed: false
B: SelectedFlowId: "universal-nine-node-development-flow-1.1" | FallbackUsed: true | SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

A: sub_phase_id: "1.1", trust_state: "probationary", gate_b_passed: true, promotion_threshold_met: false
B: SelectedFlowId: [candidate flow] | FallbackUsed: false | SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"

---

## B01-0D-024-003 — Factory rule: never cross sub-phases

```
source_id: xiigen-b01-0d-024-rule
source_license_state: first_party
pair_kind: flow_factory
```

Rule: Each factory invocation is scoped to exactly one sub_phase_id.
  A trusted_learned flow registered for 1.1 is NEVER used for 2.1, 3.1, or 4.1.
  Cross-sub-phase application is Corruption-v6-01 at the factory level.
  FallbackUsed is only true when a candidate was explicitly rejected after Gate B evaluation.
  FallbackUsed is never true for a clean deterministic_generic selection.
