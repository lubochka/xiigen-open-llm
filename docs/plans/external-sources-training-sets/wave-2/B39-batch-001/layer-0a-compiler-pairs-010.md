# Layer 0-A — Compiler Pairs (010)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-010-001 — UI: "hover state" vocabulary

A: "The hover state on the product card does not show the add-to-cart button."
B: ["UI"] — "hover state", "product card" → UI predicate (interaction state vocabulary).
CompiledPredicateIds: ["UI"]

---

## C0A-010-002 — TEST+UI: hover interaction tests

A: "Add tests for the product card hover state showing the add-to-cart button."
B: ["TEST", "UI"] — "add tests" → TEST (5.3); "product card hover state" → UI.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-010-003 — gap: "browser compatibility"

A: "Check browser compatibility for the new checkout flow."
B: [] — "check browser compatibility" is observational gap. No test-writing verb or QA abbreviation.
CompiledPredicateIds: []

---

## C0A-010-004 — TEST fires on "validate" in test context

A: "Validate that the error messages display correctly after form submission failure."
B: ["TEST", "UI"] — "validate" (test assertion verb in form/UI context) → TEST (5.3); "error messages", "form submission" → UI.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-010-005 — QA: "QA the UI"

A: "QA the updated UI before the release."
B: ["UI", "QA"] — "QA" as imperative verb → QA; "updated UI" → UI. No test-writing verb → TEST does not fire.
CompiledPredicateIds: ["UI", "QA"]
