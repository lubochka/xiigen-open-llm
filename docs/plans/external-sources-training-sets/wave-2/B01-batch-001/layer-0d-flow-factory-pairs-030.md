# Layer 0-D — Flow Factory Selection Pairs (030)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-030-001 — ModalDialog with form: two factory invocations

```
source_id: xiigen-b01-0d-030-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a modal dialog with embedded form inputs and a submit button."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-030-002 — DatePicker accessible: two factory invocations

```
source_id: xiigen-b01-0d-030-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Create a date picker widget with keyboard navigation and accessible calendar view."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-030-003 — UI+DOCUMENT factory: 1.1 + 2.1

```
source_id: xiigen-b01-0d-030-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Document the image gallery visual layout requirements and responsive breakpoints."
CompiledPredicateIds: ["UI", "DOCUMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED
