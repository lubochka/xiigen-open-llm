# Layer 0-A — Compiler Pairs
# Batch: B39-batch-001
# Scenario: B39 — QA UI automation — end-to-end UI test automation, sub-phases 5.3/7.3/1.5
# Contract: PromptRoutingInputCompilerUnit : ILogicalUnit<LogicalOrchestratorRequest, SubPhaseRouterSourceDocument>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: compiler

---

## Pair B39B1-WDA-compiler-001 — TEST + UI: E2E tests for checkout form

```
source_id: xiigen-b39-compiler-001
transition_type: feature_addition
phase: 5
sub_phase: 5.3
coverage_status: ready
pair_kind: compiler
```

A:
  user_request: "Write end-to-end tests for the checkout form covering button clicks, input validation, and modal confirmation."

B:
  DocumentId: "B39-compiler-001-prompt"
  DocumentTitle: "E2E tests for checkout form — UI interactions"
  CompiledText: "Write end-to-end tests for the checkout form covering button clicks, input validation, and modal confirmation."
  CompiledPredicateIds: ["TEST", "UI", "IMPLEMENT"]
  SourceStatus: "ready"

  Predicate evidence:
    "tests" — TEST fires on "tests"
    "end-to-end tests" — TEST fires on "end-to-end tests" compound
    "form" — UI fires on "form"
    "button clicks" — UI fires on "button"
    "input validation" — UI fires on "input"
    "modal confirmation" — UI fires on "modal"
    "write" → IMPLEMENT (write as action verb)
    Result: ["TEST", "UI", "IMPLEMENT"]

  Teaching: UI fires four times (form, button, input, modal) — dense UI signal.
  TEST fires on "end-to-end tests". IMPLEMENT fires on "write". All three predicates
  are independent and produce independent routing decisions.

---

## Pair B39B1-WDA-compiler-002 — QA + UI: QA automation of UI screens

```
source_id: xiigen-b39-compiler-002
transition_type: feature_addition
phase: 7
sub_phase: 7.3
coverage_status: ready
pair_kind: compiler
```

A:
  user_request: "QA the login screen and automate the user interface component interactions for regression testing."

B:
  DocumentId: "B39-compiler-002-prompt"
  DocumentTitle: "QA automation for login screen and UI components"
  CompiledText: "QA the login screen and automate the user interface component interactions for regression testing."
  CompiledPredicateIds: ["QA", "UI", "TEST"]
  SourceStatus: "ready"

  Predicate evidence:
    "QA" — QA predicate fires on "QA"
    "login screen" — UI fires on "screen"
    "user interface" — UI fires on "interface"
    "component" — UI fires on "component"
    "regression testing" — TEST fires on "testing"
    "automate" — not in any predicate lexicon ("automate" ≠ IMPLEMENT ≠ TEST)
    Result: ["QA", "UI", "TEST"]

  Boundary: "automate" is gap vocabulary. It does not fire IMPLEMENT or TEST.
  "regression testing" fires TEST via "testing". All three predicates are valid.

---

## Pair B39B1-WDA-compiler-003 — TEST + UI + IMPLEMENT: Playwright test suite for payment page

```
source_id: xiigen-b39-compiler-003
transition_type: feature_addition
phase: 5
sub_phase: 5.3
coverage_status: ready
pair_kind: compiler
```

A:
  user_request: "Implement the Playwright test suite to validate the payment page layout and UI components."

B:
  DocumentId: "B39-compiler-003-prompt"
  DocumentTitle: "Playwright test suite for payment page UI validation"
  CompiledText: "Implement the Playwright test suite to validate the payment page layout and UI components."
  CompiledPredicateIds: ["TEST", "UI", "IMPLEMENT"]
  SourceStatus: "ready"

  Predicate evidence:
    "implement" → IMPLEMENT
    "test suite" — TEST fires on "test suite" and "test"
    "validate" — TEST fires on "validate"
    "payment page" — UI fires on "page"
    "layout" — UI fires on "layout"
    "components" — UI fires on "component"
    "Playwright" — gap (framework name; not in any predicate lexicon)
    Result: ["TEST", "UI", "IMPLEMENT"]

  Teaching: "Playwright" as a framework name is gap vocabulary. The predicates fire on
  the generic vocabulary (test, validate, page, layout, components, implement) not on
  the named tool. TEST + UI + IMPLEMENT via dense multi-predicate signal.
