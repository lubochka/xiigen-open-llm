# Layer 0-D — Flow Factory Selection Pairs (036)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-036-001 — Component library: two factory invocations

```
source_id: xiigen-b01-0d-036-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a complete UI component library covering hero, card, modal, and gallery components."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-036-002 — Four-predicate factory: four invocations

```
source_id: xiigen-b01-0d-036-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build the pagination and modal components, document their accessibility requirements, and define the API service."
CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT", "ARCHITECTURE"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED

Rule: four predicates → four independent factory invocations; four SelectedFlowIds.

---

## B01-0D-036-003 — Round-2 factory state matrix summary

```
source_id: xiigen-b01-0d-036-r2-matrix
source_license_state: first_party
pair_kind: flow_factory
```

Round-2 factory patterns observed (files 025–036):
  Sub-phase 1.1 (UI): screen-inventory-flow-1.1-v3 when trusted_learned
  Sub-phase 4.1 (IMPLEMENT): universal-nine-node-development-flow-4.1 (no registered learned flow)
  Sub-phase 2.1 (DOCUMENT): universal-nine-node-development-flow-2.1 (no registered learned flow)
  Sub-phase 3.1 (ARCHITECTURE): universal-nine-node-development-flow-3.1 (no registered learned flow)
  Knowledge-edge: no factory invocation (CompiledPredicateIds: [])
  FallbackUsed summary: true only on probationary_rejected; false on all other outcomes
