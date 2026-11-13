# Layer 0-A — Compiler Pairs (035)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-035-001 — HeroSection: "develop hero screen with typography scale" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-035-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Develop a hero screen featuring the brand typography scale and full-width image layout."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "screen", "typography", "layout"
  IMPLEMENT fires on: "develop"
  TransitionType: feature_addition.

---

## B01-0A-035-002 — ImageGallery: "write the image gallery component logic" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-035-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Write the image gallery component logic for lazy loading and keyboard focus management."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "component"
  IMPLEMENT fires on: "write", "logic"
  TransitionType: feature_addition (both "write" and "logic" fire IMPLEMENT independently).

---

## B01-0A-035-003 — DatePicker: "build date picker page with color theme" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-035-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a date picker page with support for custom color themes and RTL layout."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.91

Predicate evidence:
  UI fires on: "page", "color", "theme" (via "themes"), "layout"
  IMPLEMENT fires on: "build"
  TransitionType: feature_addition.

---

## B01-0A-035-004 — TabNavigation: "design-system tab component with spacing specification"

```
source_id: xiigen-b01-0a-035-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a design-system tab component and specify the spacing between tab items."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]
  Confidence: 0.90

Predicate evidence:
  UI fires on: "design-system", "component", "spacing"
  IMPLEMENT fires on: "create"
  DOCUMENT fires on: "specify" (spec root)
  Three predicates → three independent routing decisions.
  TransitionType: feature_addition (IMPLEMENT present).
