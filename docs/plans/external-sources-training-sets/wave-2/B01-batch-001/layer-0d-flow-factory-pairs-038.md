# Layer 0-D — Flow Factory Selection Pairs (038)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-038-001 — DropdownMenu: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-038-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Create a dropdown menu component with keyboard navigation and multi-select support."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-038-002 — DropdownMenu: UI+DOCUMENT factory

```
source_id: xiigen-b01-0d-038-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Document the dropdown menu accessibility requirements and ARIA specification."
CompiledPredicateIds: ["UI", "DOCUMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-038-003 — DropdownMenu: UI+IMPLEMENT+TEST three-factory

```
source_id: xiigen-b01-0d-038-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Implement the dropdown component and write unit tests for keyboard interaction."
CompiledPredicateIds: ["UI", "IMPLEMENT", "TEST"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[TEST.1]: null → "universal-nine-node-development-flow-test.1" | DETERMINISTIC_GENERIC_SELECTED
