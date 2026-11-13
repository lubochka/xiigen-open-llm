# Layer 0-A — Compiler Pairs (028)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-028-001 — HeroSection with DOCUMENT: "build and specify" fires UI+IMPLEMENT+DOCUMENT

```
source_id: xiigen-b01-0a-028-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  run_id: "B01-0a-028-001"
  user_request: "Build the hero section component and write the specification for its visual behavior."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]
  Confidence: 0.93
  Warnings: []

Predicate evidence:
  UI fires on: "component", "visual"
  IMPLEMENT fires on: "build", "write"
  DOCUMENT fires on: "specification"
  Three predicates → three independent routing decisions (1.1, 4.1, 2.1).
  TransitionType: feature_addition (IMPLEMENT present).

---

## B01-0A-028-002 — ProductCard with ARCHITECTURE: "create card + define service" fires UI+IMPLEMENT+ARCHITECTURE

```
source_id: xiigen-b01-0a-028-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  run_id: "B01-0a-028-002"
  user_request: "Create the product card component and define the product data service architecture."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["UI", "IMPLEMENT", "ARCHITECTURE"]
  Confidence: 0.91
  Warnings: []

Predicate evidence:
  UI fires on: "component"
  IMPLEMENT fires on: "create"
  ARCHITECTURE fires on: "service", "architecture"
  Three predicates → three independent routing decisions (1.1, 4.1, 3.1).
  TransitionType: feature_addition (IMPLEMENT present).

---

## B01-0A-028-003 — ReviewWidget with DOCUMENT only: "specify the review widget behavior" fires UI+DOCUMENT

```
source_id: xiigen-b01-0a-028-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  run_id: "B01-0a-028-003"
  user_request: "Specify the review widget behavior and document the star rating visual states."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Confidence: 0.88
  Warnings: []

Predicate evidence:
  UI fires on: "visual" (star rating visual states)
  DOCUMENT fires on: "specification" (via "specify"), "document", "behavior"
  IMPLEMENT does not fire: no build/create/implement/write term present.
  TransitionType: feature_addition (UI present, IMPLEMENT absent → still feature_addition per B01 rules for UI).

---

## B01-0A-028-004 — complete UI predicate reference for round-2 topics

```
source_id: xiigen-b01-0a-028-ref-r2
source_license_state: first_party
pair_kind: compiler
```

Round-2 UI firing summary (topics 025–036):
  HeroSection: fires via "component", "layout"
  ProductCard: fires via "component", "layout"
  ReviewWidget: fires via "widget" (component family)
  PaginationComponent: fires via "component", "button"
  FilterPanel: fires via "panel" (component family), "input"
  BreadcrumbNav: "navigation" does NOT fire UI; "component" + "page" do
  TooltipComponent: fires via "component", "accessibility"
  ModalDialog: fires via "modal", "button"
  DatePicker: fires via "component"
  ImageGallery: fires via "component", "layout"
  TabNavigation: fires via "component", "accessibility" (not via "navigation")
  CompleteUIPredicateReference: see layer-0a-compiler-pairs-024.md for full 22-term lexicon
