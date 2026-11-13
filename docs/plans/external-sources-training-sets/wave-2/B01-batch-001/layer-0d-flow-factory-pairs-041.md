# Layer 0-D — Flow Factory Selection Pairs (041)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-041-001 — LoadingSkeleton: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-041-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a loading skeleton component to show placeholder content."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-041-002 — LoadingSkeleton: UI+IMPLEMENT+DOCUMENT three-factory

```
source_id: xiigen-b01-0d-041-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build the loading skeleton component and document its usage guidelines and screen variants."
CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-041-003 — LoadingSkeleton: "skeleton screen" vs "loading skeleton" factory distinction

```
source_id: xiigen-b01-0d-041-003
source_license_state: first_party
pair_kind: flow_factory
```

Two surface forms of the same concept, different factory activation paths:

"Create skeleton screen variants for product list" → UI fires via "screen"
  Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED

"Build a loading skeleton component" → UI fires via "component"
  Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED

Both produce the same factory selection. The surface form difference affects only the UI term that triggers routing, not the flow selected.

"Optimize skeleton screen rendering" → CompiledPredicateIds: [] → factory NOT invoked.
