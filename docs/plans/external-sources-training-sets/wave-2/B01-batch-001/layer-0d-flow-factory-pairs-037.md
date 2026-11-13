# Layer 0-D — Flow Factory Selection Pairs (037)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-037-001 — AccordionPanel: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-037-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build an accordion panel component with expand and collapse animation."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-037-002 — AccordionPanel: four-predicate factory

```
source_id: xiigen-b01-0d-037-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build accordion component, document accessibility requirements, define API schema, write tests."
CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT", "TEST"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[TEST.1]: null → "universal-nine-node-development-flow-test.1" | DETERMINISTIC_GENERIC_SELECTED

Rule: four predicates → four independent factory invocations; four SelectedFlowIds.

---

## B01-0D-037-003 — AccordionPanel: knowledge-edge — no factory invocation

```
source_id: xiigen-b01-0d-037-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Improve the accordion animation speed and transition timing."
CompiledPredicateIds: []

Factory: not invoked (knowledge-edge path).
SelectedFlowId: null
FallbackUsed: null
SelectionOutcome: "KNOWLEDGE_EDGE_NO_FACTORY"
