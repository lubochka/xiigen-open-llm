# Layer 0-D — Flow Factory Selection Pairs (027)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-027-001 — ModalDialog: deterministic_generic for sub-phase 1.1

```
source_id: xiigen-b01-0d-027-001
source_license_state: first_party
pair_kind: flow_factory
```

A: sub_phase_id: "1.1", trust_state: null (no candidate), scenario: "ModalDialog"
B: SelectedFlowId: "universal-nine-node-development-flow-1.1" | SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED" | FallbackUsed: false

---

## B01-0D-027-002 — DatePicker: trusted_learned for sub-phase 1.1

```
source_id: xiigen-b01-0d-027-002
source_license_state: first_party
pair_kind: flow_factory
```

A: sub_phase_id: "1.1", trust_state: "trusted_learned", scenario: "DatePicker"
B: SelectedFlowId: "screen-inventory-flow-1.1-v3" | SelectionOutcome: "TRUSTED_LEARNED_SELECTED" | FallbackUsed: false | Warnings: []

---

## B01-0D-027-003 — ImageGallery: two-sub-phase factory invocations (UI + IMPLEMENT)

```
source_id: xiigen-b01-0d-027-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Create an image gallery component" → CompiledPredicateIds: ["UI", "IMPLEMENT"]
Two factory invocations (one per sub-phase):

Factory[1.1]:
  Input: sub_phase_id: "1.1", trust_state: "trusted_learned"
  Output: SelectedFlowId: "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED | FallbackUsed: false

Factory[4.1]:
  Input: sub_phase_id: "4.1", trust_state: null
  Output: SelectedFlowId: "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED | FallbackUsed: false

Rule: each factory invocation is scoped to exactly one sub_phase_id.
