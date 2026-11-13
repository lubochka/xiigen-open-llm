# Layer 0-A — Compiler Pairs (007)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-007-001 — TEST+UI: "create test cases for radio buttons"

A: "Create test cases for the radio button selection in the payment form."
B: ["TEST", "UI"] — "create test cases" → TEST (5.3); "radio button selection" → UI; "payment form" → UI.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-007-002 — UI: checkbox vocabulary

A: "The checkbox does not maintain checked state after page refresh."
B: ["UI"] — "checkbox" + "checked state" → UI predicate.
CompiledPredicateIds: ["UI"]

---

## C0A-007-003 — gap: "visual regression testing" without verb

A: "Visual regression testing shows a 2px shift in the header."
B: ["TEST"] — "visual regression testing" contains "testing" but with observation context. However, "testing" as noun topic fires TEST here → TEST (5.3, visual/E2E context). Actually — "testing shows" is observational, not a test-writing verb. Gap.
B (corrected): [] — "visual regression testing shows" is an observation statement. No test-writing verb. Gap.
CompiledPredicateIds: []

---

## C0A-007-004 — IMPLEMENT+UI: build UI feature

A: "Implement the infinite scroll on the product listing page."
B: ["IMPLEMENT", "UI"] — "implement" → IMPLEMENT; "infinite scroll", "product listing page" → UI.
CompiledPredicateIds: ["IMPLEMENT", "UI"]

---

## C0A-007-005 — TEST+UI+QA: full triple for registration flow

A: "Add tests for the registration form validation, fix the input focus styling, and QA sign-off."
B: ["TEST", "UI", "QA"] — "add tests" → TEST; "input focus styling" → UI; "QA sign-off" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]
