# Layer 0-D — Flow Factory Selection Pairs (032)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-032-001 — FilterPanel screen: two factory invocations

```
source_id: xiigen-b01-0d-032-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Create a filter panel screen with collapsible category groups and price range slider."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-032-002 — Modal wireframe: UI-only factory (one invocation)

```
source_id: xiigen-b01-0d-032-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Design the modal dialog wireframe showing open, loading, and confirmation states."
CompiledPredicateIds: ["UI"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED

One predicate → one factory invocation.

---

## B01-0D-032-003 — DatePicker with theme: two factory invocations

```
source_id: xiigen-b01-0d-032-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a date picker component with full support for the design system theme tokens."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
