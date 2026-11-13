# Layer 0-A — Compiler Pairs (057)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-057-001 — RatingStars: "build a star rating component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-057-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a star rating component with half-star support, read-only mode, and accessible keyboard input."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.94

Predicate evidence:
  UI fires on: "component", "accessibility" (via "accessible"), "input"
  IMPLEMENT fires on: "build"
  "rating" NOT in UI lexicon; "star" NOT in UI lexicon — fires via "component", "accessibility", "input".
  TransitionType: feature_addition.

---

## B01-0A-057-002 — RatingStars: "design rating display widget" fires UI only

```
source_id: xiigen-b01-0a-057-002
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Design a rating display widget that shows aggregate score, review count, and distribution bars."
  CompiledPredicateIds: ["UI"]
  Confidence: 0.87

Predicate evidence:
  UI fires on: "widget"
  "design" NOT IMPLEMENT.
  "rating" NOT in UI lexicon; fires only via "widget".
  TransitionType: documentation.

---

## B01-0A-057-003 — RatingStars: "implement rating submission API" fires IMPLEMENT+ARCHITECTURE

```
source_id: xiigen-b01-0a-057-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Implement the rating submission API endpoint and database schema for persisting user ratings."
  CompiledPredicateIds: ["IMPLEMENT", "ARCHITECTURE"]
  Confidence: 0.92

Predicate evidence:
  IMPLEMENT fires on: "implement"
  ARCHITECTURE fires on: "API", "database", "schema"
  No UI lexicon terms present.
  Two predicates → two independent routing decisions.
  TransitionType: feature_addition.

---

## B01-0A-057-004 — RatingStars: "users are gaming ratings" fires nothing

```
source_id: xiigen-b01-0a-057-004
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Users are gaming the rating system — investigate fake review submissions and propose mitigation."
  CompiledPredicateIds: []
  Confidence: 0.38

Predicate evidence:
  "gaming" NOT in any predicate lexicon.
  "fake review" NOT in any predicate lexicon.
  "investigate" NOT in any predicate lexicon.
  "propose mitigation" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.
