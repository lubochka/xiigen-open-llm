# Layer 0-B — Router Pairs (046)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-046-001 — DragDropList: UI+IMPLEMENT (via accessibility+visual) routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-046-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Implement drag-and-drop list reordering with visual drop indicators and accessibility support."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

Note: "drag" and "drop" are gap terms; UI fires via "accessibility" and "visual".

---

## B01-0B-046-002 — DragDropList: UI+IMPLEMENT via "widget" routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-046-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Create a sortable list widget with drag handles and keyboard reorder support."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-046-003 — DragDropList: DOCUMENT only routes to 2.1

```
source_id: xiigen-b01-0b-046-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["DOCUMENT"]
   User request: "Document the drag-drop state machine specification covering all states."
B: DecisionCount: 1 | SubPhaseId: "2.1" | KnowledgeEdge: false

---

## B01-0B-046-004 — DragDropList: knowledge-edge (automate gap)

```
source_id: xiigen-b01-0b-046-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Automate drag-and-drop interaction testing across all supported browser environments."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"
