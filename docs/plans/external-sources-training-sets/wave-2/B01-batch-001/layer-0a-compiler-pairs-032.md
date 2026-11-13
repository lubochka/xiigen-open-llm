# Layer 0-A — Compiler Pairs (032)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-032-001 — "create a filter panel screen" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-032-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a filter panel screen with collapsible category groups and price range slider."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.90

Predicate evidence:
  UI fires on: "screen", "input" (slider is an input element)
  IMPLEMENT fires on: "create"
  TransitionType: feature_addition.

---

## B01-0A-032-002 — "design the modal dialog wireframe" fires UI only

```
source_id: xiigen-b01-0a-032-002
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Design the modal dialog wireframe showing open, loading, and confirmation states."
  CompiledPredicateIds: ["UI"]
  Confidence: 0.88

Predicate evidence:
  UI fires on: "modal", "wireframe"
  "design" does not fire IMPLEMENT.
  TransitionType: documentation (UI only).

---

## B01-0A-032-003 — "build a date picker with theme token support" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-032-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a date picker component with full support for the design system theme tokens."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "component", "design-system", "theme"
  IMPLEMENT fires on: "build"
  TransitionType: feature_addition.

---

## B01-0A-032-004 — "prototype the tab navigation interaction" fires UI only

```
source_id: xiigen-b01-0a-032-004
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Prototype the tab navigation interaction with focus ring animations."
  CompiledPredicateIds: ["UI"]
  Confidence: 0.86

Predicate evidence:
  UI fires on: "prototype", "interaction"
  "navigation" does NOT fire UI (confirmed gap term).
  "prototype" IS in B01 UI lexicon (contrast: gap term in B32).
  "animate" is not in any predicate lexicon.
  TransitionType: documentation (UI only, IMPLEMENT absent).
