# Layer 0-A — Compiler Pairs (030)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-030-001 — FilterPanel gap test: "refine the filter options" fires nothing

```
source_id: xiigen-b01-0a-030-001
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Refine the filter options and improve the sidebar search flow."
  CompiledPredicateIds: []
  Confidence: 0.52

Predicate evidence:
  "refine" NOT in any predicate lexicon.
  "improve" NOT in IMPLEMENT lexicon.
  "sidebar" NOT in UI lexicon (confirmed gap).
  "search" NOT in any predicate lexicon.
  "flow" NOT in UI lexicon (confirmed gap).
  CompiledPredicateIds: [] → knowledge-edge path.

---

## B01-0A-030-002 — ModalDialog: "build a modal with form inputs" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-030-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a modal dialog with embedded form inputs and a submit button."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "modal", "form", "input", "button"
  IMPLEMENT fires on: "build"
  TransitionType: feature_addition.

---

## B01-0A-030-003 — DatePicker: "create a date picker widget" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-030-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a date picker widget with keyboard navigation and accessible calendar view."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.91

Predicate evidence:
  UI fires on: "widget" (component family), "accessibility" (via "accessible")
  IMPLEMENT fires on: "create"
  TransitionType: feature_addition.

---

## B01-0A-030-004 — ImageGallery: "develop an image gallery with color theming" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-030-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Develop an image gallery screen with dynamic color theming and responsive layout."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "screen", "color", "theme" (via "theming"), "layout", "responsive"
  IMPLEMENT fires on: "develop"
  TransitionType: feature_addition.
