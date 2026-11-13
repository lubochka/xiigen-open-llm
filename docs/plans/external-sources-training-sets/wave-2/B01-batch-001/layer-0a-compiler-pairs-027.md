# Layer 0-A — Compiler Pairs (027)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-027-001 — ModalDialog: "implement a modal dialog" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-027-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  run_id: "B01-0a-027-001"
  user_request: "Implement a modal dialog with a header, body, and close button."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.94
  Warnings: []

Predicate evidence:
  UI fires on: "modal", "button"
  IMPLEMENT fires on: "implement"
  TransitionType: feature_addition (IMPLEMENT present).

---

## B01-0A-027-002 — DatePicker: "design a date picker" fires UI only

```
source_id: xiigen-b01-0a-027-002
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  run_id: "B01-0a-027-002"
  user_request: "Design a date picker component with month/year navigation and day selection."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["UI"]
  Confidence: 0.86
  Warnings: []

Predicate evidence:
  UI fires on: "component"
  "design" does not fire IMPLEMENT.
  TransitionType: documentation (UI only, IMPLEMENT absent).

---

## B01-0A-027-003 — ImageGallery: "create an image gallery component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-027-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  run_id: "B01-0a-027-003"
  user_request: "Create an image gallery component with thumbnail grid and lightbox view."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.90
  Warnings: []

Predicate evidence:
  UI fires on: "component", "layout" (grid is a layout)
  IMPLEMENT fires on: "create"
  TransitionType: feature_addition (IMPLEMENT present).

---

## B01-0A-027-004 — TabNavigation: "tab" fires UI when used with component terms

```
source_id: xiigen-b01-0a-027-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  run_id: "B01-0a-027-004"
  user_request: "Build a tab navigation component with animated indicator and keyboard accessibility."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.89
  Warnings: []

Predicate evidence:
  UI fires on: "component", "accessibility"
  IMPLEMENT fires on: "build"
  Note: "navigation" alone does not fire UI; "component" and "accessibility" do.
  TransitionType: feature_addition (IMPLEMENT present).
