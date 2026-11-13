# Layer 0-B — Router Pairs (055)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-055-001 — CodeEditor: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-055-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Implement an embedded code editor component with syntax highlighting and auto-complete."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-055-002 — CodeEditor: UI+IMPLEMENT (theme triple-hit) routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-055-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Create a theme-switchable code editor with design-system color tokens."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

Note: "theme", "color", "design-system" all fire UI → three UI hits, one routing decision.

---

## B01-0B-055-003 — CodeEditor: UI+DOCUMENT routes to 1.1 + 2.1

```
source_id: xiigen-b01-0b-055-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "DOCUMENT"]
   User request: "Document the code editor accessibility requirements for keyboard navigation and screen reader compatibility."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "2.1"] | KnowledgeEdge: false (both)

---

## B01-0B-055-004 — CodeEditor: knowledge-edge (performance gap)

```
source_id: xiigen-b01-0b-055-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "The editor is too slow to type in — profile the keystroke handler and reduce latency."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"
