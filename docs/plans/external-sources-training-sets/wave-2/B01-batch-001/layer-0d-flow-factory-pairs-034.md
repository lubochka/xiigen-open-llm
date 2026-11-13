# Layer 0-D — Flow Factory Selection Pairs (034)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-034-001 — BreadcrumbNav page component: two factory invocations

```
source_id: xiigen-b01-0d-034-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Create a breadcrumb page indicator component with home and section links."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-034-002 — FilterPanel+DOCUMENT: three factory invocations

```
source_id: xiigen-b01-0d-034-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build the filter panel component and document its interaction behavior for QA review."
CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-034-003 — Modal form validation: two factory invocations

```
source_id: xiigen-b01-0d-034-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Code a modal dialog with form validation, error states, and an accessible close button."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
