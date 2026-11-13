# Layer 0-A — Compiler Pairs (053)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-053-001 — VideoPlayer: "build a video player component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-053-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a video player component with play/pause controls, progress scrubber, and fullscreen mode."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.91

Predicate evidence:
  UI fires on: "component"
  IMPLEMENT fires on: "build"
  "video" NOT in UI lexicon; "player" NOT in UI lexicon — fires via "component".
  TransitionType: feature_addition.

---

## B01-0A-053-002 — VideoPlayer: "design the player UI screen layout" fires UI only

```
source_id: xiigen-b01-0a-053-002
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Design the player UI screen layout with overlay controls, thumbnail strip, and quality selector."
  CompiledPredicateIds: ["UI"]
  Confidence: 0.90

Predicate evidence:
  UI fires on: "ui", "screen", "layout"
  "design" NOT IMPLEMENT.
  Three UI lexicon hits; higher confidence for UI.
  TransitionType: documentation (UI only).

---

## B01-0A-053-003 — VideoPlayer: "implement adaptive bitrate streaming integration" fires IMPLEMENT+ARCHITECTURE

```
source_id: xiigen-b01-0a-053-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Implement adaptive bitrate streaming integration with CDN service and manifest API."
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  Confidence: 0.92

Predicate evidence:
  IMPLEMENT fires on: "implement"
  ARCHITECTURE fires on: "service", "API", "integration" (infrastructure)
  "integrate" is gap verb but "integration" as noun + "service"/"API" → ARCHITECTURE fires.
  TransitionType: feature_addition.

---

## B01-0A-053-004 — VideoPlayer: "make video autoplay on mobile" fires nothing

```
source_id: xiigen-b01-0a-053-004
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Make video autoplay on mobile without user gesture and handle muted fallback gracefully."
  CompiledPredicateIds: []
  Confidence: 0.40

Predicate evidence:
  "autoplay" NOT in any predicate lexicon.
  "mobile" NOT in any predicate lexicon (gap term).
  "gesture" NOT in any predicate lexicon.
  "make" NOT in IMPLEMENT lexicon (gap verb — NOT equivalent to "create").
  CompiledPredicateIds: [] → knowledge-edge path.
