# Layer 0-B — Router Pairs (054)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-054-001 — AudioPlayer: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-054-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Create an audio player component with waveform visualization and volume slider."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-054-002 — AudioPlayer: UI only routes to 1.1

```
source_id: xiigen-b01-0b-054-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI"]
   User request: "Design the audio player screen with track list, album art, and playback progress display."
B: DecisionCount: 1 | SubPhaseId: "1.1" | KnowledgeEdge: false

---

## B01-0B-054-003 — AudioPlayer: UI+IMPLEMENT (accessibility triple-hit) routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-054-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a podcast player component with accessibility keyboard controls and screen reader announcements."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-054-004 — AudioPlayer: knowledge-edge (analyze formats gap)

```
source_id: xiigen-b01-0b-054-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Analyze the tradeoffs between MP3, AAC, and OGG audio compression formats."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"
