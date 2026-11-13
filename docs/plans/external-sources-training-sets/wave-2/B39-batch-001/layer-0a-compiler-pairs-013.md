# Layer 0-A — Compiler Pairs (013)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-013-001 — TEST fires on "write Cypress e2e tests"

A: "Write Cypress e2e tests for the shopping cart add-to-cart button flow."
B: ["TEST", "UI"] — "write" (test-writing verb) + "e2e tests" → TEST; "cart button" → UI. "Cypress" is gap (framework name, not a predicate trigger).
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-013-002 — TEST fires on "create test cases" for modal

A: "Create end-to-end test cases covering the checkout confirmation modal dismiss and confirm actions."
B: ["TEST", "UI"] — "create test cases" → TEST; "modal", "confirm actions" → UI.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-013-003 — gap: "run Cypress against staging"

A: "Run the Cypress suite against the staging checkout page."
B: [] — "run" is operational gap; "Cypress" is framework name gap. No test-writing verb present.
CompiledPredicateIds: []

---

## C0A-013-004 — TEST+QA: write e2e + QA sign-off

A: "Write e2e tests for the product detail page and QA sign-off before release."
B: ["TEST", "UI", "QA"] — "write e2e tests" → TEST; "product detail page" → UI (page); "QA sign-off" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]

---

## C0A-013-005 — boundary: "assert" fires TEST in Cypress context

A: "Assert that the Cypress test for the header navigation passes on all breakpoints."
B: ["TEST", "UI"] — "assert" → TEST (assert is a test-writing/test-assertion verb); "header navigation" → UI; "Cypress" → gap.
CompiledPredicateIds: ["TEST", "UI"]
