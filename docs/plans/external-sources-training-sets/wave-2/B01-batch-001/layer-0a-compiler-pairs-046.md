# Layer 0-A — Compiler Pairs (046)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-046-001 — DragDropList: "implement drag-and-drop list reordering" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-046-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Implement drag-and-drop list reordering for the task board with visual drop indicators and accessibility support."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "accessibility", "visual"
  IMPLEMENT fires on: "implement"
  "drag" NOT in any predicate lexicon (gap term).
  "drop" NOT in any predicate lexicon (gap term).
  Fires via "accessibility" and "visual" for UI; "implement" for IMPLEMENT.
  TransitionType: feature_addition.

---

## B01-0A-046-002 — DragDropList: "create a sortable list widget" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-046-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a sortable list widget with drag handles and keyboard reorder support."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.91

Predicate evidence:
  UI fires on: "widget" (component family)
  IMPLEMENT fires on: "create"
  "sortable" NOT in any predicate lexicon.
  "drag handles" NOT in any predicate lexicon.
  TransitionType: feature_addition.

---

## B01-0A-046-003 — DragDropList: "drag-drop state machine specification" fires DOCUMENT only

```
source_id: xiigen-b01-0a-046-003
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Document the drag-drop state machine specification covering idle, dragging, dropping, and cancelled states."
  CompiledPredicateIds: ["DOCUMENT"]
  Confidence: 0.88

Predicate evidence:
  DOCUMENT fires on: "document", "specification"
  No UI lexicon terms: "drag-drop", "state machine" NOT in UI lexicon.
  Single predicate → single routing decision.
  TransitionType: documentation.

---

## B01-0A-046-004 — DragDropList: "automate drag-and-drop testing" fires nothing

```
source_id: xiigen-b01-0a-046-004
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Automate drag-and-drop interaction testing across all supported browser environments."
  CompiledPredicateIds: []
  Confidence: 0.41

Predicate evidence:
  "automate" NOT in IMPLEMENT lexicon (gap verb — confirmed from round-2).
  "drag-and-drop" NOT in any predicate lexicon.
  "browser environments" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.
