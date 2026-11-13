# Layer 0-D — Flow Factory Selection Pairs (057)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-057-001 — RatingStars: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-057-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a star rating component with half-star support and accessible keyboard input."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-057-002 — RatingStars: IMPLEMENT+ARCHITECTURE factory

```
source_id: xiigen-b01-0d-057-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Implement the rating submission API endpoint and database schema."
CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]

Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED

Note: No UI factory — backend rating API has no UI lexicon terms.

---

## B01-0D-057-003 — RatingStars: UI only factory

```
source_id: xiigen-b01-0d-057-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Design a rating display widget that shows aggregate score and distribution bars."
CompiledPredicateIds: ["UI"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
FallbackUsed: false
TransitionType: documentation
