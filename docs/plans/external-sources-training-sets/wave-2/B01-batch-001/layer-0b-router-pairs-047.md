# Layer 0-B — Router Pairs (047)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-047-001 — ResizablePanel: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-047-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a resizable panel component with drag handle and responsive collapse behavior."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-047-002 — ResizablePanel: UI only routes to 1.1

```
source_id: xiigen-b01-0b-047-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI"]
   User request: "Design a split-pane layout with resizable left sidebar and main content area."
B: DecisionCount: 1 | SubPhaseId: "1.1" | KnowledgeEdge: false

Note: fires via "layout" only — "sidebar" is gap, "split-pane" is gap.

---

## B01-0B-047-003 — ResizablePanel: IMPLEMENT+ARCHITECTURE routes to 4.1 + 3.1

```
source_id: xiigen-b01-0b-047-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
   User request: "Implement panel size state persistence using local storage schema and sync service integration."
B: DecisionCount: 2 | SubPhaseIds: ["4.1", "3.1"] | KnowledgeEdge: false (both)

---

## B01-0B-047-004 — ResizablePanel: knowledge-edge (sluggish gap)

```
source_id: xiigen-b01-0b-047-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "The resizable panel feels sluggish during resize — investigate and resolve the performance issue."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"
