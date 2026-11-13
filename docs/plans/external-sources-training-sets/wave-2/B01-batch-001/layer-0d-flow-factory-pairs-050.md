# Layer 0-D — Flow Factory Selection Pairs (050)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-050-001 — MapComponent: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-050-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build an interactive map component with pin markers, clustering, and location search input."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-050-002 — MapComponent: ARCHITECTURE+IMPLEMENT factory

```
source_id: xiigen-b01-0d-050-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Integrate the mapping API service and implement the geolocation data pipeline."
CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]

Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

Note: Neither factory has trusted_learned state for 3.1 or 4.1 sub-phases.
Both DETERMINISTIC_GENERIC. No UI factory invoked.

---

## B01-0D-050-003 — MapComponent: UI only factory

```
source_id: xiigen-b01-0d-050-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Design the map view screens including location detail panel and search overlay."
CompiledPredicateIds: ["UI"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
FallbackUsed: false
TransitionType: documentation
