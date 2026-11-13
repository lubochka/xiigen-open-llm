# Layer 0-A — Compiler Pairs (036)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-036-001 — Complete UI predicate reference for round-2 (files 025–036)

```
source_id: xiigen-b01-0a-036-ref-r2-complete
source_license_state: first_party
pair_kind: compiler
```

Round-2 predicate firing summary:
  "hero section": no direct lexicon match; fires via co-occurring "component", "layout", "screen"
  "product card": fires via "component", "layout"
  "review widget": fires via "widget" (component family)
  "pagination": fires via "component", "button"
  "filter panel": fires via "input" (for slider/checkbox), "screen"
  "breadcrumb navigation": "navigation" is a gap term; fires only via "component", "page"
  "tooltip": fires via "component", "accessibility"
  "modal dialog": fires via "modal", "button", "form"
  "date picker": fires via "component", "color", "theme"
  "image gallery": fires via "component", "layout", "screen"
  "tab navigation": "navigation" is gap; fires via "component", "accessibility", "interaction"

---

## B01-0A-036-002 — "build a complete component library for round-2 topics" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-036-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a complete UI component library covering hero, card, modal, and gallery components."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.95

Predicate evidence:
  UI fires on: "ui", "component", "library" (via "component library" as design-system construct), "modal"
  IMPLEMENT fires on: "build"
  TransitionType: feature_addition.

---

## B01-0A-036-003 — "plan the HeroSection redesign journey" fires nothing

```
source_id: xiigen-b01-0a-036-003
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Plan the hero section redesign journey and map the user engagement strategy."
  CompiledPredicateIds: []
  Confidence: 0.49

Predicate evidence:
  "plan" NOT in any predicate lexicon.
  "redesign" NOT in IMPLEMENT lexicon (gap).
  "journey" NOT in any predicate lexicon.
  "strategy" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.

---

## B01-0A-036-004 — Four-predicate sweep on round-2 component work

```
source_id: xiigen-b01-0a-036-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build the pagination and modal components, document their accessibility requirements, and define the API service for loading paginated data."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT", "ARCHITECTURE"]
  Confidence: 0.94

Predicate evidence:
  UI fires on: "component", "modal", "accessibility"
  IMPLEMENT fires on: "build"
  DOCUMENT fires on: "document", "requirements", "accessibility" (secondary)
  ARCHITECTURE fires on: "API", "service"
  Four predicates → four independent routing decisions.
  TransitionType: feature_addition (IMPLEMENT present).
