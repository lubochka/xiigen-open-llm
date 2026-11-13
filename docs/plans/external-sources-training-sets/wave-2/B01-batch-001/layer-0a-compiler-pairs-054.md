# Layer 0-A — Compiler Pairs (054)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-054-001 — AudioPlayer: "create an audio player component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-054-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create an audio player component with waveform visualization, playback controls, and volume slider."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "component"
  IMPLEMENT fires on: "create"
  "audio" NOT in UI lexicon; "waveform" NOT in any lexicon — fires via "component".
  TransitionType: feature_addition.

---

## B01-0A-054-002 — AudioPlayer: "design audio player screen" fires UI only

```
source_id: xiigen-b01-0a-054-002
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Design the audio player screen with track list, album art, and playback progress display."
  CompiledPredicateIds: ["UI"]
  Confidence: 0.87

Predicate evidence:
  UI fires on: "screen"
  "design" NOT IMPLEMENT.
  "audio" NOT in UI lexicon.
  TransitionType: documentation.

---

## B01-0A-054-003 — AudioPlayer: "build podcast player with accessibility features" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-054-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a podcast player component with accessibility keyboard controls and screen reader announcements."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.94

Predicate evidence:
  UI fires on: "component", "accessibility", "screen" (via "screen reader")
  IMPLEMENT fires on: "build"
  Three UI lexicon hits; high confidence.
  TransitionType: feature_addition.

---

## B01-0A-054-004 — AudioPlayer: "analyze audio compression formats" fires nothing

```
source_id: xiigen-b01-0a-054-004
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Analyze the tradeoffs between MP3, AAC, and OGG audio compression formats for web streaming."
  CompiledPredicateIds: []
  Confidence: 0.37

Predicate evidence:
  "analyze" NOT in any predicate lexicon.
  "tradeoffs" NOT in any predicate lexicon.
  "MP3", "AAC", "OGG" NOT in any predicate lexicon.
  "compression formats" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.
