# Layer 0-A — Compiler Pairs (042)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-042-001 — EmptyState: "create an empty state component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-042-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create an empty state component with illustration, heading, and call-to-action button for zero-data views."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "component", "button"
  IMPLEMENT fires on: "create"
  "empty state" NOT in UI lexicon; fires via co-occurring "component" and "button".
  TransitionType: feature_addition.

---

## B01-0A-042-002 — EmptyState: "design empty state screens" fires UI only

```
source_id: xiigen-b01-0a-042-002
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Design empty state screens for the dashboard, search results, and notification center."
  CompiledPredicateIds: ["UI"]
  Confidence: 0.88

Predicate evidence:
  UI fires on: "screen"
  "design" does NOT fire IMPLEMENT.
  TransitionType: documentation (UI only).

---

## B01-0A-042-003 — EmptyState: "write user stories for empty state behavior" fires DOCUMENT+IMPLEMENT

```
source_id: xiigen-b01-0a-042-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Write user stories for the empty state behavior across all product list and search result views."
  CompiledPredicateIds: ["DOCUMENT", "IMPLEMENT"]
  Confidence: 0.89

Predicate evidence:
  DOCUMENT fires on: "user stories" (confirmed DOCUMENT+IMPLEMENT trigger)
  IMPLEMENT fires on: "write" (code artifact context)
  Note: "write user stories" fires both DOCUMENT and IMPLEMENT per special rule.
  UI does NOT fire — no UI lexicon terms present.
  TransitionType: feature_addition (IMPLEMENT present).

---

## B01-0A-042-004 — EmptyState: "enhance empty state messaging" fires nothing

```
source_id: xiigen-b01-0a-042-004
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Enhance empty state messaging to feel more friendly and encourage user action."
  CompiledPredicateIds: []
  Confidence: 0.40

Predicate evidence:
  "enhance" NOT in IMPLEMENT lexicon (gap verb — reinforced from round-2).
  "messaging" NOT in any predicate lexicon.
  "friendly" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.
