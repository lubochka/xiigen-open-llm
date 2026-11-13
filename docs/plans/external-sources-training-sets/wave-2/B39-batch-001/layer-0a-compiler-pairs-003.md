# Layer 0-A — Compiler Pairs (003)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-003-001 — UI fires on form input vocabulary

A: "The form input fields need accessible label attributes."
B: ["UI"] — "form input fields" → UI predicate. No test-writing verb.
CompiledPredicateIds: ["UI"]

---

## C0A-003-002 — TEST+UI: assert UI behavior

A: "Assert that the dropdown renders all 50 product categories correctly."
B: ["TEST", "UI"] — "assert" (test assertion verb) → TEST; "dropdown renders" → UI.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-003-003 — gap: framework configuration

A: "Configure Cypress to run against the staging environment."
B: [] — "configure" is operational gap; "Cypress" is framework name → gap.
CompiledPredicateIds: []

---

## C0A-003-004 — TEST+UI+QA: full triple

A: "Write E2E tests for the checkout form, ensure the UI meets design specs, and QA sign-off."
B: ["TEST", "UI", "QA"] — "write E2E tests" → TEST; "UI meets design specs" → UI; "QA sign-off" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]

---

## C0A-003-005 — gap: "set up Selenium"

A: "Set up Selenium for the browser test environment."
B: [] — "set up" is gap; "Selenium" is framework name → gap. No predicate fires.
CompiledPredicateIds: []
