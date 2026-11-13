# Layer 0-D — Flow Factory Selection Pairs (044)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-044-001 — InfiniteScroll: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-044-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Create a reusable infinite scroll component with loading indicator and end-of-list detection."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-044-002 — InfiniteScroll: IMPLEMENT+TEST factory (no UI)

```
source_id: xiigen-b01-0d-044-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Write integration tests for the infinite scroll behavior including edge cases."
CompiledPredicateIds: ["IMPLEMENT", "TEST"]

Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[TEST.1]: null → "universal-nine-node-development-flow-test.1" | DETERMINISTIC_GENERIC_SELECTED

Note: UI factory (1.1) NOT invoked — no UI lexicon terms present.

---

## B01-0D-044-003 — InfiniteScroll: knowledge-edge — no factory

```
source_id: xiigen-b01-0d-044-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Optimize infinite scroll performance using virtual rendering and throttling."
CompiledPredicateIds: []

Factory: not invoked.
SelectionOutcome: "KNOWLEDGE_EDGE_NO_FACTORY"

Rule confirmed: FallbackUsed is null (not false) when knowledge-edge fires.
No candidate flow was proposed; there is nothing to fall back FROM.
