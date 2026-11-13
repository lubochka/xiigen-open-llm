# Layer 0-A — Compiler Pairs (043)
# Batch: B01-batch-001 | Scenario: B01 — UI components and frontend flows
# Production date: 2026-05-06 | pair_kind: compiler

---

## B01-0A-043-001 — ErrorBoundary: "implement a React error boundary component" fires UI+IMPLEMENT

```
source_id: xiigen-b01-0a-043-001
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Implement a React error boundary component that catches runtime errors and displays a fallback UI."
  CompiledPredicateIds: ["UI", "IMPLEMENT"]
  Confidence: 0.93

Predicate evidence:
  UI fires on: "component", "ui" (via "fallback UI")
  IMPLEMENT fires on: "implement"
  "error boundary" NOT in UI lexicon; fires via co-occurring "component" and "ui".
  TransitionType: feature_addition.

---

## B01-0A-043-002 — ErrorBoundary: "document error boundary behavior" fires UI+DOCUMENT

```
source_id: xiigen-b01-0a-043-002
source_license_state: first_party
pair_kind: compiler
transition_type: documentation
```

A:
  user_request: "Document the error boundary component behavior, fallback screen requirements, and recovery flow."
  CompiledPredicateIds: ["UI", "DOCUMENT"]
  Confidence: 0.91

Predicate evidence:
  UI fires on: "component", "screen"
  DOCUMENT fires on: "document", "requirements"
  TransitionType: documentation (no IMPLEMENT).

---

## B01-0A-043-003 — ErrorBoundary: "add error boundary to production app" fires IMPLEMENT only

```
source_id: xiigen-b01-0a-043-003
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Add error boundary wrappers to all production routes and service modules."
  CompiledPredicateIds: ["IMPLEMENT"]
  Confidence: 0.87

Predicate evidence:
  IMPLEMENT fires on: "add" (code artifact context)
  No UI lexicon terms present — "error boundary" is not in UI lexicon.
  "routes" NOT in any lexicon.
  Single predicate → single routing decision.
  TransitionType: feature_addition.

---

## B01-0A-043-004 — ErrorBoundary: four-predicate sweep with TEST

```
source_id: xiigen-b01-0a-043-004
source_license_state: first_party
pair_kind: compiler
transition_type: feature_addition
```

A:
  user_request: "Build error boundary components, write unit tests, document error recovery requirements, and define the logging API schema."
  CompiledPredicateIds: ["UI", "IMPLEMENT", "DOCUMENT", "ARCHITECTURE"]
  Confidence: 0.94

Predicate evidence:
  UI fires on: "component"
  IMPLEMENT fires on: "build", "write"
  DOCUMENT fires on: "document", "requirements"
  ARCHITECTURE fires on: "API", "schema"
  Four predicates → four independent routing decisions.
