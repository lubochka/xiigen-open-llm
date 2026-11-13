# Layer 0-A — Compiler Pairs (026)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-026-001 — FilterPanel: "build a filter panel" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-026-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  run_id: "B01-0a-026-001"
  user_request: "Build a filter panel with checkboxes, sliders, and an apply button for the product listing."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92
  Warnings: []

Predicate evidence:
  UI fires on: "panel" (component family), "button", "input" (checkbox/slider are input elements)
  IMPLEMENT fires on: "build"
  TransitionType: feature_addition (IMPLEMENT present).

---

## B01-0A-026-002 — BreadcrumbNav: "breadcrumb navigation" does NOT fire UI

```
source_id: xiigen-b01-0a-026-002
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  run_id: "B01-0a-026-002"
  user_request: "Add breadcrumb navigation to every page in the product catalog."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: []
  Confidence: 0.54
  Warnings: ["no_predicate_matched: knowledge_edge_result"]

Predicate evidence:
  "navigation" is NOT in the UI lexicon (confirmed gap term).
  "breadcrumb" is NOT in any predicate lexicon.
  "page" alone does not fire when surrounded entirely by gap vocabulary.
  CompiledPredicateIds: [] → knowledge-edge path.

---

## B01-0A-026-003 — BreadcrumbNav with "create": fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-026-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  run_id: "B01-0a-026-003"
  user_request: "Create a breadcrumb component that renders the current page path."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.89
  Warnings: []

Predicate evidence:
  UI fires on: "component", "page"
  IMPLEMENT fires on: "create"
  "breadcrumb" alone would not fire UI, but "component" and "page" do.
  TransitionType: feature_addition (IMPLEMENT present).

---

## B01-0A-026-004 — TooltipComponent: "build a tooltip" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-026-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  run_id: "B01-0a-026-004"
  user_request: "Build a tooltip component that appears on hover with accessible ARIA labels."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.91
  Warnings: []

Predicate evidence:
  UI fires on: "component", "accessibility" (via "accessible")
  IMPLEMENT fires on: "build"
  TransitionType: feature_addition (IMPLEMENT present).
