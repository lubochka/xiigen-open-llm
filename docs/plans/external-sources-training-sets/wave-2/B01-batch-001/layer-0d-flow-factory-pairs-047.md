# Layer 0-D — Flow Factory Selection Pairs (047)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-047-001 — ResizablePanel: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-047-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a resizable panel component with drag handle and responsive collapse behavior."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-047-002 — ResizablePanel: IMPLEMENT+ARCHITECTURE factory

```
source_id: xiigen-b01-0d-047-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Implement panel size state persistence using local storage schema and sync service integration."
CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]

Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED

Note: No UI factory (1.1) invocation — no UI lexicon terms.

---

## B01-0D-047-003 — ResizablePanel: UI only factory

```
source_id: xiigen-b01-0d-047-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Design a split-pane layout with resizable left sidebar and main content area."
CompiledPredicateIds: ["UI"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
FallbackUsed: false
TransitionType: documentation
