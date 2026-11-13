# Layer 0-A — Compiler Pairs (025)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-025-001 — HeroSection: UI+IMPLEMENT fires on "build a hero section"

```
source_id: xiigen-b01-0a-025-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  run_id: "B01-0a-025-001"
  user_request: "Build a hero section component with a headline, subheading, and CTA button."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.91
  Warnings: []

Predicate evidence:
  UI fires on: "component", "button"
  IMPLEMENT fires on: "build"
  TransitionType: feature_addition (IMPLEMENT present).
  Routing: 1.1 (UI) + 4.1 (IMPLEMENT).

---

## B01-0A-025-002 — ProductCard: "design a product card" fires UI only

```
source_id: xiigen-b01-0a-025-002
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  run_id: "B01-0a-025-002"
  user_request: "Design a product card layout with image, title, and price fields."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["UI"]
  Confidence: 0.87
  Warnings: []

Predicate evidence:
  UI fires on: "layout", "card" (via "component" family)
  "design" alone does not fire IMPLEMENT — only build/create/code/implement fire IMPLEMENT.
  TransitionType: documentation (UI present, IMPLEMENT absent).
  Routing: 1.1 only.

---

## B01-0A-025-003 — ReviewWidget: "create a review widget" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-025-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  run_id: "B01-0a-025-003"
  user_request: "Create a review widget showing star ratings, reviewer name, and comment text."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.90
  Warnings: []

Predicate evidence:
  UI fires on: "widget" (component family)
  IMPLEMENT fires on: "create"
  TransitionType: feature_addition (IMPLEMENT present).
  Two predicates → two independent routing decisions.

---

## B01-0A-025-004 — PaginationComponent: "implement a pagination component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-025-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  run_id: "B01-0a-025-004"
  user_request: "Implement a pagination component with previous/next buttons and page number display."
  flags:
    UseLearnedUnitWhenAvailable: true
    UseKnowledgeEdgeDetection: true

B:
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.93
  Warnings: []

Predicate evidence:
  UI fires on: "component", "button"
  IMPLEMENT fires on: "implement"
  TransitionType: feature_addition (IMPLEMENT present).
  Routing: two independent decisions (1.1, 4.1).
