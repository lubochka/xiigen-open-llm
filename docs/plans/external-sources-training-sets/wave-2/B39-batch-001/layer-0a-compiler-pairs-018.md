# Layer 0-A — Compiler Pairs (018)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-018-001 — TEST+UI+QA: cross-browser test suite with QA gate

A: "Write cross-browser tests for the product listing page layout on Chrome, Firefox, and Safari, and QA the results."
B: ["TEST", "UI", "QA"] — "write cross-browser tests" → TEST; "product listing page", "layout" → UI; "QA the results" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]

---

## C0A-018-002 — TEST+UI: cross-browser modal test

A: "Add test coverage for the confirmation modal on Internet Explorer 11 (IE11) and Edge to catch flexbox rendering bugs."
B: ["TEST", "UI"] — "add test coverage" → TEST; "confirmation modal" → UI (modal). "Internet Explorer 11" → gap (browser name is not a predicate).
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-018-003 — gap: "check browser compatibility table"

A: "Check the browser compatibility table for CSS (Cascading Style Sheets) grid support."
B: [] — "check" → gap (operational, not test-writing); "CSS grid" → gap (no UI component vocabulary fires). Zero predicates.
CompiledPredicateIds: []

---

## C0A-018-004 — TEST+UI: validate responsive layout on browsers

A: "Validate the responsive layout of the navigation bar across Chrome and Firefox at 768px and 1440px breakpoints."
B: ["TEST", "UI"] — "validate" → TEST; "responsive layout" → UI (responsive, layout); "navigation bar" → UI (nav).
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-018-005 — TEST+UI+QA: multi-browser regression + QA acceptance

A: "Write cross-browser regression test cases for each form input component and get QA acceptance sign-off for the compatibility matrix."
B: ["TEST", "UI", "QA"] — "write regression test cases" → TEST; "form input component" → UI; "QA acceptance sign-off" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]
