# Layer 0-D — Flow Factory Selection Pairs (033)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-033-001 — Tooltip spacing/color: two factory invocations

```
source_id: xiigen-b01-0d-033-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a tooltip component using spacing tokens and primary color from the design system."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-033-002 — Gallery document: UI+DOCUMENT factory (two invocations)

```
source_id: xiigen-b01-0d-033-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Document the image gallery visual layout requirements and responsive breakpoints."
CompiledPredicateIds: ["UI", "DOCUMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-033-003 — Pagination accessible: two factory invocations

```
source_id: xiigen-b01-0d-033-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Implement the pagination component with full keyboard accessibility support."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
