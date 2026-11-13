# Layer 0-A — Compiler Pairs (033)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-033-001 — "build a tooltip with spacing and color tokens" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-033-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a tooltip component using spacing tokens and primary color from the design system."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "component", "spacing", "color", "design-system"
  IMPLEMENT fires on: "build"
  TransitionType: feature_addition.

---

## B01-0A-033-002 — "document the image gallery visual layout" fires UI+DOCUMENT

```
source_id: xiigen-b01-0a-033-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Document the image gallery visual layout requirements and responsive breakpoints."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Confidence: 0.89

Predicate evidence:
  UI fires on: "visual", "layout", "responsive"
  DOCUMENT fires on: "document", "requirements"
  TransitionType: feature_addition (UI present per B01 rule).

---

## B01-0A-033-003 — "implement pagination with keyboard accessibility" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-033-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Implement the pagination component with full keyboard accessibility support."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "component", "accessibility"
  IMPLEMENT fires on: "implement"
  TransitionType: feature_addition.

---

## B01-0A-033-004 — "redesign the review widget look and feel" fires nothing

```
source_id: xiigen-b01-0a-033-004
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Redesign the review widget look and feel to match the new brand identity."
  CompiledPredicateIds: []
  Confidence: 0.51

Predicate evidence:
  "redesign" NOT in IMPLEMENT lexicon (confirmed gap: see layer-0a-024 reference).
  "look and feel" NOT in any predicate lexicon.
  "brand" NOT in any predicate lexicon.
  "identity" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.
