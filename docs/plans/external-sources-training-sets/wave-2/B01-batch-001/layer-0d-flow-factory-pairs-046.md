# Layer 0-D — Flow Factory Selection Pairs (046)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-046-001 — DragDropList: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-046-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Implement drag-and-drop list reordering with visual drop indicators and accessibility support."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-046-002 — DragDropList: DOCUMENT only factory

```
source_id: xiigen-b01-0d-046-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Document the drag-drop state machine specification covering all states."
CompiledPredicateIds: ["DOCUMENT"]

Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED

Note: No UI lexicon terms present — UI factory (1.1) NOT invoked.
DOCUMENT alone routes to 2.1 only.

---

## B01-0D-046-003 — DragDropList: knowledge-edge — no factory

```
source_id: xiigen-b01-0d-046-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Automate drag-and-drop interaction testing across all supported browser environments."
CompiledPredicateIds: []

Factory: not invoked.
SelectionOutcome: "KNOWLEDGE_EDGE_NO_FACTORY"
