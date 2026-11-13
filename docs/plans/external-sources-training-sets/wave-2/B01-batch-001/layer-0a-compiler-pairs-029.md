# Layer 0-A — Compiler Pairs (029)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-029-001 — "build a hero section with responsive layout" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-029-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a hero section with a responsive layout for mobile and desktop viewports."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "layout", "responsive"
  IMPLEMENT fires on: "build"
  TransitionType: feature_addition.

---

## B01-0A-029-002 — "design the product card visual hierarchy" fires UI only

```
source_id: xiigen-b01-0a-029-002
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Design the product card visual hierarchy and spacing between image and text."
  CompiledPredicateIds: ["UI"]
  Confidence: 0.87

Predicate evidence:
  UI fires on: "visual", "spacing"
  "design" does not fire IMPLEMENT.
  TransitionType: documentation (UI only).

---

## B01-0A-029-003 — "implement a review widget with accessibility" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-029-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Implement a review widget with accessible keyboard interaction and color contrast."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "accessibility" (via "accessible"), "color"
  IMPLEMENT fires on: "implement"
  TransitionType: feature_addition.

---

## B01-0A-029-004 — "specify pagination UI behavior" fires UI+DOCUMENT

```
source_id: xiigen-b01-0a-029-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Specify the pagination UI behavior and document the button state transitions."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Confidence: 0.89

Predicate evidence:
  UI fires on: "ui", "button"
  DOCUMENT fires on: "specify" (spec root), "document", "behavior"
  TransitionType: feature_addition (UI present without IMPLEMENT → feature_addition per B01 rule).
