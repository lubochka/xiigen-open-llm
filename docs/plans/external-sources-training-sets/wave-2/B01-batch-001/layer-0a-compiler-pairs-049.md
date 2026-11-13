# Layer 0-A — Compiler Pairs (049)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-049-001 — ChartsAndGraphs: "build a chart component library" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-049-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a chart component library with line, bar, pie, and area chart types for the analytics dashboard."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.91

Predicate evidence:
  UI fires on: "component"
  IMPLEMENT fires on: "build"
  "chart" NOT in UI lexicon; fires via co-occurring "component".
  "dashboard" NOT in UI lexicon; does not contribute.
  TransitionType: feature_addition.

---

## B01-0A-049-002 — ChartsAndGraphs: "create a responsive data visualization widget" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-049-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a responsive data visualization widget with tooltip interactions and theme support."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.94

Predicate evidence:
  UI fires on: "responsive", "widget", "interaction", "theme"
  IMPLEMENT fires on: "create"
  Four UI lexicon hits; high confidence.
  TransitionType: feature_addition.

---

## B01-0A-049-003 — ChartsAndGraphs: "define chart API contract" fires ARCHITECTURE only

```
source_id: xiigen-b01-0a-049-003
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Define the chart data API contract specifying the schema for series, labels, and configuration."
  CompiledPredicateIds: ["ARCHITECTURE"]
  Confidence: 0.89

Predicate evidence:
  ARCHITECTURE fires on: "API", "schema"
  "define" is NOT in IMPLEMENT lexicon (gap verb).
  No UI lexicon terms in this request.
  Single predicate → single routing decision.
  TransitionType: documentation (IMPLEMENT absent).

---

## B01-0A-049-004 — ChartsAndGraphs: "explain chart rendering mechanics" fires nothing

```
source_id: xiigen-b01-0a-049-004
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Explain how SVG chart rendering works and why canvas-based charts perform better for large datasets."
  CompiledPredicateIds: []
  Confidence: 0.38

Predicate evidence:
  "explain" NOT in any predicate lexicon.
  "SVG" NOT in any predicate lexicon.
  "rendering" NOT in any predicate lexicon.
  "canvas" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.
