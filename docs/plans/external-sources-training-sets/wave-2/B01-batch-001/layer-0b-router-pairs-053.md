# Layer 0-B — Router Pairs (053)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-053-001 — VideoPlayer: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-053-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a video player component with play/pause controls and fullscreen mode."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-053-002 — VideoPlayer: UI only (triple-hit) routes to 1.1

```
source_id: xiigen-b01-0b-053-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI"]
   User request: "Design the player UI screen layout with overlay controls, thumbnail strip, and quality selector."
B: DecisionCount: 1 | SubPhaseId: "1.1" | KnowledgeEdge: false

Note: "ui", "screen", "layout" all fire UI — still one routing decision.

---

## B01-0B-053-003 — VideoPlayer: IMPLEMENT+ARCHITECTURE routes to 4.1 + 3.1

```
source_id: xiigen-b01-0b-053-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
   User request: "Implement adaptive bitrate streaming integration with CDN service and manifest API."
B: DecisionCount: 2 | SubPhaseIds: ["4.1", "3.1"] | KnowledgeEdge: false (both)

---

## B01-0B-053-004 — VideoPlayer: knowledge-edge (autoplay gap)

```
source_id: xiigen-b01-0b-053-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Make video autoplay on mobile without user gesture and handle muted fallback gracefully."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"
