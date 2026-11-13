# Layer 0-B — Router Pairs (049)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: router

---

## B01-0B-049-001 — ChartsAndGraphs: UI+IMPLEMENT routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-049-001
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Build a chart component library with line, bar, pie, and area chart types."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

---

## B01-0B-049-002 — ChartsAndGraphs: UI+IMPLEMENT (four-hit) routes to 1.1 + 4.1

```
source_id: xiigen-b01-0b-049-002
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["UI", "IMPLEMENT"]
   User request: "Create a responsive data visualization widget with tooltip interactions and theme support."
B: DecisionCount: 2 | SubPhaseIds: ["1.1", "4.1"] | KnowledgeEdge: false (both)

Note: Multiple UI hits ("responsive", "widget", "interaction", "theme") → still one routing decision for UI.

---

## B01-0B-049-003 — ChartsAndGraphs: ARCHITECTURE only routes to 3.1

```
source_id: xiigen-b01-0b-049-003
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: ["ARCHITECTURE"]
   User request: "Define the chart data API contract specifying schema for series, labels, and configuration."
B: DecisionCount: 1 | SubPhaseId: "3.1" | KnowledgeEdge: false

---

## B01-0B-049-004 — ChartsAndGraphs: knowledge-edge (explain gap)

```
source_id: xiigen-b01-0b-049-004
source_license_state: first_party
pair_kind: router
```

A: CompiledPredicateIds: []
   User request: "Explain how SVG chart rendering works and why canvas-based charts perform better."
B: DecisionCount: 1 | SubPhaseId: null | KnowledgeEdge: true | FailureClassification: "no_predicate_match"
