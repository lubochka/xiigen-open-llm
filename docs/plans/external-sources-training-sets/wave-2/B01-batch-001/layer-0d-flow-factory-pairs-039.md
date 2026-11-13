# Layer 0-D — Flow Factory Selection Pairs (039)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-039-001 — SidebarNav: UI+IMPLEMENT factory (via "component" only)

```
source_id: xiigen-b01-0d-039-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Create a collapsible sidebar navigation component with nested menu items."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

Note: "sidebar" and "navigation" are gap terms; "component" fires UI enabling 1.1.

---

## B01-0D-039-002 — SidebarNav: knowledge-edge — no factory

```
source_id: xiigen-b01-0d-039-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Improve the sidebar navigation design for better usability."
CompiledPredicateIds: []

Factory: not invoked.
SelectionOutcome: "KNOWLEDGE_EDGE_NO_FACTORY"

---

## B01-0D-039-003 — SidebarNav: UI+IMPLEMENT (four-hit) factory

```
source_id: xiigen-b01-0d-039-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Implement the sidebar component with responsive layout breakpoints for mobile screens."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

Note: Four UI lexicon hits ("component", "responsive", "layout", "screen") still produce ONE factory[1.1] invocation.
Multiple UI term hits → same flow, same sub-phase, higher confidence — not additional factory calls.
