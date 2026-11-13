# Layer 0-D — Flow Factory Selection Pairs (053)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-053-001 — VideoPlayer: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-053-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a video player component with play/pause controls and fullscreen mode."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-053-002 — VideoPlayer: UI only (triple-hit) factory

```
source_id: xiigen-b01-0d-053-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Design the player UI screen layout with overlay controls and quality selector."
CompiledPredicateIds: ["UI"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
FallbackUsed: false
TransitionType: documentation

---

## B01-0D-053-003 — VideoPlayer: IMPLEMENT+ARCHITECTURE factory

```
source_id: xiigen-b01-0d-053-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Implement adaptive bitrate streaming integration with CDN service and manifest API."
CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]

Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED
Factory[3.1]: null → "universal-nine-node-development-flow-3.1" | DETERMINISTIC_GENERIC_SELECTED

Note: No UI factory (1.1) — backend/streaming integration context has no UI terms.
