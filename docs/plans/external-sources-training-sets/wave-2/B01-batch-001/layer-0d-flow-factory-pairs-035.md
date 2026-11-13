# Layer 0-D — Flow Factory Selection Pairs (035)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-035-001 — HeroSection typography: two factory invocations

```
source_id: xiigen-b01-0d-035-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Develop a hero screen featuring the brand typography scale and full-width image layout."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-035-002 — ImageGallery logic: two factory invocations

```
source_id: xiigen-b01-0d-035-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Write the image gallery component logic for lazy loading and keyboard focus management."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-035-003 — Tab with DOCUMENT: three factory invocations

```
source_id: xiigen-b01-0d-035-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Create a design-system tab component and specify the spacing between tab items."
CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED
