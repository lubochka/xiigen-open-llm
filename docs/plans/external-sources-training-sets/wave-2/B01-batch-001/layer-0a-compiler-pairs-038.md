# Layer 0-A — Compiler Pairs (038)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-038-001 — DropdownMenu: "create a dropdown menu component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-038-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a dropdown menu component with keyboard navigation, search filtering, and multi-select support."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "component", "input" (via "search filtering" — input family)
  IMPLEMENT fires on: "create"
  "dropdown" NOT in UI lexicon; fires via co-occurring "component".
  TransitionType: feature_addition (IMPLEMENT present).

---

## B01-0A-038-002 — DropdownMenu: "document dropdown accessibility" fires UI+DOCUMENT

```
source_id: xiigen-b01-0a-038-002
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Document the dropdown menu accessibility requirements and ARIA specification."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Confidence: 0.91

Predicate evidence:
  UI fires on: "accessibility"
  DOCUMENT fires on: "document", "requirements", "specification"
  TransitionType: documentation (no IMPLEMENT).
  Two predicates → two independent routing decisions.

---

## B01-0A-038-003 — DropdownMenu: "refactor dropdown logic" fires nothing

```
source_id: xiigen-b01-0a-038-003
source_license_state: first_party
pair_kind: compiler
transition_type: knowledge_edge
```

A:
  user_request: "Refactor the dropdown selection logic to improve reusability and reduce bundle size."
  CompiledPredicateIds: []
  Confidence: 0.42

Predicate evidence:
  "refactor" NOT in IMPLEMENT lexicon (gap verb).
  "reusability" NOT in any predicate lexicon.
  "bundle size" NOT in any predicate lexicon.
  CompiledPredicateIds: [] → knowledge-edge path.

---

## B01-0A-038-004 — DropdownMenu: three-predicate UI+IMPLEMENT+TEST

```
source_id: xiigen-b01-0a-038-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Implement the dropdown menu component and write unit tests for keyboard interaction."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "TEST"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "component", "interaction"
  IMPLEMENT fires on: "implement"
  TEST fires on: "tests" (via "unit tests")
  Three predicates → three independent routing decisions.
  TransitionType: feature_addition (IMPLEMENT present).
