# Layer 0-D — Flow Factory Selection Pairs (049)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-049-001 — ChartsAndGraphs: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-049-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a chart component library with line, bar, pie, and area chart types."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-049-002 — ChartsAndGraphs: ARCHITECTURE only factory

```
source_id: xiigen-b01-0d-049-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Define the chart data API contract specifying schema for series, labels, and configuration."
CompiledPredicateIds: ["ARCHITECTURE"]

Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED
FallbackUsed: false
TransitionType: documentation (IMPLEMENT absent)

---

## B01-0D-049-003 — ChartsAndGraphs: four-hit UI still one factory[1.1]

```
source_id: xiigen-b01-0d-049-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Create a responsive data visualization widget with tooltip interactions and theme support."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

Confirmed rule: "responsive" + "widget" + "interaction" + "theme" → four UI hits → ONE factory[1.1].
Multiple UI term matches increase confidence but do NOT multiply factory calls.
Factory selection count equals predicate count, not UI lexicon hit count.
