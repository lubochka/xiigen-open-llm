# Layer 0-A — Compiler Pairs (034)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-034-001 — BreadcrumbNav: "create a breadcrumb page indicator" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-034-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Create a breadcrumb page indicator component with home and section links."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.89

Predicate evidence:
  UI fires on: "component", "page"
  IMPLEMENT fires on: "create"
  "breadcrumb" alone does not fire UI; "component" and "page" do.
  TransitionType: feature_addition.

---

## B01-0A-034-002 — FilterPanel + DOCUMENT: "build filter panel and document its behavior" fires UI+IMPLEMENT+DOCUMENT

```
source_id: xiigen-b01-0a-034-002
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build the filter panel component and document its interaction behavior for QA review."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT"]
  Confidence: 0.92

Predicate evidence:
  UI fires on: "component", "interaction"
  IMPLEMENT fires on: "build"
  DOCUMENT fires on: "document", "behavior"
  Three predicates → three independent routing decisions.
  TransitionType: feature_addition (IMPLEMENT present).

---

## B01-0A-034-003 — TooltipComponent: "implement tooltip with visual feedback" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-034-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Implement a tooltip component with visual feedback delay and fade animation."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.90

Predicate evidence:
  UI fires on: "component", "visual"
  IMPLEMENT fires on: "implement"
  TransitionType: feature_addition.

---

## B01-0A-034-004 — ModalDialog: "code a modal with form validation" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-034-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Code a modal dialog with form validation, error states, and an accessible close button."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "modal", "form", "button", "accessibility" (via "accessible")
  IMPLEMENT fires on: "code"
  TransitionType: feature_addition (IMPLEMENT present).
