# Layer 0-D — Flow Factory Selection Pairs (031)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-031-001 — "write UI spec for hero": three factory invocations

```
source_id: xiigen-b01-0d-031-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Write the UI spec for the hero section covering visual states and interaction patterns."
CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-031-002 — Factory warnings summary

```
source_id: xiigen-b01-0d-031-002
source_license_state: first_party
pair_kind: flow_factory
```

Warnings by SelectionOutcome:

| Outcome | Warnings |
|---|---|
| TRUSTED_LEARNED_SELECTED | [] (always empty) |
| ACCEPTED_PROBATIONARY_GATE_B_PASSED | ["probationary_flow_in_use", "promotion_threshold_not_met", "monitoring_required"] |
| REJECTED_PROBATIONARY_FALLBACK_USED | ["probationary_flow_rejected", "gate_b_failed", "fallback_used"] |
| DETERMINISTIC_GENERIC_SELECTED | [] (always empty) |

Rule: trusted_learned and deterministic_generic always produce empty Warnings arrays.
Probationary outcomes always produce non-empty Warnings arrays.

---

## B01-0D-031-003 — PaginationComponent: trusted_learned for sub-phase 1.1

```
source_id: xiigen-b01-0d-031-003
source_license_state: first_party
pair_kind: flow_factory
```

A: sub_phase_id: "1.1", trust_state: "trusted_learned", scenario: "Implement pagination component"
B: SelectedFlowId: "screen-inventory-flow-1.1-v3" | SelectionOutcome: "TRUSTED_LEARNED_SELECTED" | FallbackUsed: false | Warnings: []
