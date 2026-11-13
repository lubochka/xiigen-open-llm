# Layer 0-D — Flow Factory Selection Pairs (054)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B01-0D-054-001 — AudioPlayer: UI+IMPLEMENT factory

```
source_id: xiigen-b01-0d-054-001
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Create an audio player component with waveform visualization and volume slider."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

---

## B01-0D-054-002 — AudioPlayer: UI only factory

```
source_id: xiigen-b01-0d-054-002
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Design the audio player screen with track list, album art, and playback progress display."
CompiledPredicateIds: ["UI"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
FallbackUsed: false
TransitionType: documentation

---

## B01-0D-054-003 — AudioPlayer: accessibility triple-hit factory

```
source_id: xiigen-b01-0d-054-003
source_license_state: first_party
pair_kind: flow_factory
```

Scenario: "Build a podcast player component with accessibility keyboard controls and screen reader announcements."
CompiledPredicateIds: ["UI", "IMPLEMENT"]

Factory[1.1]: trusted_learned → "screen-inventory-flow-1.1-v3" | TRUSTED_LEARNED_SELECTED
Factory[4.1]: null → "universal-nine-node-development-flow-4.1" | DETERMINISTIC_GENERIC_SELECTED

Note: Three UI hits ("component", "accessibility", "screen" via "screen reader") still produce one Factory[1.1].
