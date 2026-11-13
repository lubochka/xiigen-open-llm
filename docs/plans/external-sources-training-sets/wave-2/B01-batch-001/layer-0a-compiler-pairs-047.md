# Layer 0-A — Compiler Pairs (047)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-047-001 — ResizablePanel: "build a resizable panel component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-047-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a resizable panel component with drag handle, min/max constraints, and responsive collapse behavior."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "component", "responsive"
  IMPLEMENT fires on: "build"
  "panel" fires via "component" context (layout family).
  TransitionType: feature_addition.

---

## B01-0A-047-002 — ResizablePanel: "design split-pane layout" fires UI only

```
source_id: xiigen-b01-0a-047-002
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Design a split-pane layout with resizable left sidebar and main content area."
  CompiledPredicateIds: ["UI"]
  Confidence: 0.87

Predicate evidence:
  UI fires on: "layout"
  "design" NOT IMPLEMENT.
  "split-pane" NOT in UI lexicon.
  "sidebar" NOT in UI lexicon (confirmed gap).
  TransitionType: documentation.

---

## B01-0A-047-003 — ResizablePanel: "implement panel state persistence" fires IMPLEMENT+ARCHITECTURE

```
source_id: xiigen-b01-0a-047-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Implement panel size state persistence using local storage schema and sync service integration."
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  Confidence: 0.91

Predicate evidence:
  IMPLEMENT fires on: "implement"
  ARCHITECTURE fires on: "service", "schema", "integration"
  No UI lexicon terms in this request.
  Two predicates → two independent routing decisions.
  TransitionType: feature_addition.

---

## B01-0A-047-004 — ResizablePanel: "resizable layout feels sluggish" fires nothing

```
source_id: xiigen-b01-0a-047-004
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "The resizable panel feels sluggish during resize — investigate and resolve the performance issue."
  CompiledPredicateIds: []
  Confidence: 0.38

Predicate evidence:
  "feels sluggish" NOT in any predicate lexicon.
  "investigate" NOT in any predicate lexicon.
  "resolve" NOT in IMPLEMENT lexicon (gap verb).
  "performance issue" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.
