# Layer 0-D — Flow Factory Selection Pairs (045)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-045-001 — VirtualList: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-045-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a virtual list component that renders only visible rows."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-045-002 — VirtualList: IMPLEMENT only factory

```
source_id: xiigen-b01-0d-045-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Add virtual scrolling to the data table to handle 10,000-row datasets."
CompiledPredicateIds: ["IMPLEMENT"]

Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-045-003 — VirtualList: four-predicate factory

```
source_id: xiigen-b01-0d-045-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build virtual list component, define data pipeline API, document performance requirements, write benchmarks."
CompiledPredicateIds: ["UI", "IMPLEMENT", "ARCHITECTURE", "DOCUMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED

Rule: four predicates → four independent factory invocations.
