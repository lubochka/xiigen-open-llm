# Layer 0-D — Flow Factory Selection Pairs (043)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-043-001 — ErrorBoundary: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-043-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Implement a React error boundary component that catches errors and displays fallback UI."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-043-002 — ErrorBoundary: IMPLEMENT only factory

```
source_id: xiigen-b01-0d-043-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Add error boundary wrappers to all production routes and service modules."
CompiledPredicateIds: ["IMPLEMENT"]

Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

Note: UI factory (1.1) NOT invoked — no UI lexicon terms in request.
Single predicate → single factory invocation.

---

## B01-0D-043-003 — ErrorBoundary: four-predicate factory

```
source_id: xiigen-b01-0d-043-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build error boundary components, write tests, document recovery requirements, define logging API schema."
CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT", "ARCHITECTURE"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[2.1]: null → "universal-nine-node-development-flow-2.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED

Rule: four predicates → four factory invocations.
