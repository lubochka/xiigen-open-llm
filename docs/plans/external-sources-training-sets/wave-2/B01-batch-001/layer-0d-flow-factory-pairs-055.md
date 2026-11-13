# Layer 0-D — Flow Factory Selection Pairs (055)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-055-001 — CodeEditor: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-055-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Implement an embedded code editor component with syntax highlighting and auto-complete."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-055-002 — CodeEditor: UI+IMPLEMENT (theme triple-hit) factory

```
source_id: xiigen-b01-0d-055-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Create a theme-switchable code editor with design-system color tokens."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

Note: "theme", "design-system", "color" — three UI lexicon hits → still one Factory[1.1].

---

## B01-0D-055-003 — CodeEditor: UI+DOCUMENT factory

```
source_id: xiigen-b01-0d-055-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Document the code editor accessibility requirements for keyboard navigation and screen reader compatibility."
CompiledPredicateIds: ["UI", "DOCUMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED
FallbackUsed: false (both)
TransitionType: documentation (IMPLEMENT absent)
