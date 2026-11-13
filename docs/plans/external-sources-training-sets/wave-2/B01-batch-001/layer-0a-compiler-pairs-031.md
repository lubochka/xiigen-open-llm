# Layer 0-A — Compiler Pairs (031)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-031-001 — TabNavigation: "implement tab component with typography tokens" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-031-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Implement a tab component using the design system's typography tokens and spacing scale."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "component", "typography", "spacing", "design-system"
  IMPLEMENT fires on: "implement"
  TransitionType: feature_addition.

---

## B01-0A-031-002 — "write the UI spec for the hero section" fires UI+IMPLEMENT+DOCUMENT

```
source_id: xiigen-b01-0a-031-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Write the UI spec for the hero section covering visual states and interaction patterns."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]
  Confidence: 0.91

Predicate evidence:
  UI fires on: "ui", "visual"
  IMPLEMENT fires on: "write" (always fires IMPLEMENT regardless of what is being written)
  DOCUMENT fires on: "spec", "interaction" (via "behavior" family)
  TransitionType: feature_addition (IMPLEMENT present).

---

## B01-0A-031-003 — ProductCard gap: "enhance the product card experience" fires nothing

```
source_id: xiigen-b01-0a-031-003
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Enhance the product card experience and improve the overall checkout journey."
  CompiledPredicateIds: []
  Confidence: 0.50

Predicate evidence:
  "enhance" NOT in any lexicon.
  "experience" NOT in UI lexicon (only "ux" fires UI, not "experience").
  "improve" NOT in IMPLEMENT lexicon.
  "journey" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.

---

## B01-0A-031-004 — ReviewWidget + ARCHITECTURE: "build widget + define backend schema" fires UI+IMPLEMENT+ARCHITECTURE

```
source_id: xiigen-b01-0a-031-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build the review widget and define the database schema for storing review data."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "ARCHITECTURE"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "widget" (component family)
  IMPLEMENT fires on: "build"
  ARCHITECTURE fires on: "database", "schema"
  Three independent routing decisions (1.1, 4.1, 3.1).
  TransitionType: feature_addition.
