# Layer 0-A — Compiler Pairs (008)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-008-001 — UI: toggle/switch component

A: "The toggle switch for notifications is not accessible via keyboard."
B: ["UI"] — "toggle switch" → UI predicate. Accessibility issue on UI component.
CompiledPredicateIds: ["UI"]

---

## C0A-008-002 — TEST+UI: "assert toggle state"

A: "Assert that the toggle switch state persists after page navigation."
B: ["TEST", "UI"] — "assert" → TEST (5.3); "toggle switch state" → UI.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-008-003 — gap: "screenshot comparison"

A: "Compare screenshots from the last two builds."
B: [] — "compare screenshots" is observational gap. No test-writing verb or QA abbreviation.
CompiledPredicateIds: []

---

## C0A-008-004 — UI fires on "dialog" vocabulary

A: "The confirmation dialog does not trap focus correctly."
B: ["UI"] — "confirmation dialog" + "focus trap" → UI predicate.
CompiledPredicateIds: ["UI"]

---

## C0A-008-005 — TEST+UI: dialog interaction tests

A: "Write tests for the confirmation dialog cancel and confirm actions."
B: ["TEST", "UI"] — "write tests" → TEST (5.3); "confirmation dialog cancel/confirm actions" → UI.
CompiledPredicateIds: ["TEST", "UI"]
