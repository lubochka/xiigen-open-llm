# Layer 0-A — Compiler Pairs (041)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-041-001 — LoadingSkeleton: "build a loading skeleton component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-041-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build a loading skeleton component to show placeholder content while data is being fetched."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.91

Predicate evidence:
  UI fires on: "component"
  IMPLEMENT fires on: "build"
  "skeleton" NOT in UI lexicon; "loading" NOT in UI lexicon — fires only via "component".
  TransitionType: feature_addition.

---

## B01-0A-041-002 — LoadingSkeleton: "create skeleton screens for product list" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-041-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create skeleton screen variants for the product list, card grid, and table views."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "screen" (UI lexicon term — "skeleton screen" triggers via "screen")
  IMPLEMENT fires on: "create"
  "skeleton" alone NOT in UI lexicon; "screen" IS in UI lexicon.
  TransitionType: feature_addition.

---

## B01-0A-041-003 — LoadingSkeleton: "optimize skeleton rendering performance" fires nothing

```
source_id: xiigen-b01-0a-041-003
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Optimize skeleton screen rendering performance and reduce layout shift during loading."
  CompiledPredicateIds: []
  Confidence: 0.41

Predicate evidence:
  "optimize" NOT in IMPLEMENT lexicon (gap verb).
  "rendering performance" NOT in any predicate lexicon.
  "layout shift" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.

---

## B01-0A-041-004 — LoadingSkeleton: UI+IMPLEMENT+DOCUMENT three-predicate

```
source_id: xiigen-b01-0a-041-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build the loading skeleton component and document its usage guidelines and screen variants."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "component", "screen"
  IMPLEMENT fires on: "build"
  DOCUMENT fires on: "document", "guidelines"
  Three predicates → three independent routing decisions.
  TransitionType: feature_addition (IMPLEMENT present).
