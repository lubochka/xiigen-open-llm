# Layer 0-A — Compiler Pairs (044)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-044-001 — InfiniteScroll: "implement infinite scroll for product feed" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-044-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Implement infinite scroll for the product feed so additional items load automatically when reaching the bottom."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.90

Predicate evidence:
  UI fires on: "screen" (via "feed" as screen context — scroll fires nothing, screen context implied)
  IMPLEMENT fires on: "implement"
  "scroll" NOT in any predicate lexicon (confirmed gap from round-2).
  "infinite" NOT in any predicate lexicon.
  Fires via "implement" + implied screen context.
  TransitionType: feature_addition.

---

## B01-0A-044-002 — InfiniteScroll: "create a paginated scroll component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-044-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a reusable infinite scroll component with loading indicator and end-of-list detection."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "component"
  IMPLEMENT fires on: "create"
  "scroll" NOT in lexicon; "component" fires UI.
  TransitionType: feature_addition.

---

## B01-0A-044-003 — InfiniteScroll: "scroll behavior optimization" fires nothing

```
source_id: xiigen-b01-0a-044-003
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Optimize infinite scroll performance by using virtual rendering and throttling scroll events."
  CompiledPredicateIds: []
  Confidence: 0.40

Predicate evidence:
  "optimize" NOT in IMPLEMENT lexicon (gap verb).
  "scroll" NOT in any predicate lexicon (confirmed gap).
  "virtual rendering" NOT in any predicate lexicon.
  "throttling" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.

---

## B01-0A-044-004 — InfiniteScroll: "write tests for scroll behavior" fires IMPLEMENT+TEST

```
source_id: xiigen-b01-0a-044-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Write integration tests for the infinite scroll behavior including edge cases and network failure handling."
  CompiledPredicateIds: ["IMPLEMENT", "TEST"]
  Confidence: 0.89

Predicate evidence:
  IMPLEMENT fires on: "write" (code artifact context)
  TEST fires on: "tests" (via "integration tests")
  No UI lexicon terms present in this specific request.
  TransitionType: feature_addition.
