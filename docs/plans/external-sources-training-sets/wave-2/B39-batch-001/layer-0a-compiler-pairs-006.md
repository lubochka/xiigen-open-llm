# Layer 0-A — Compiler Pairs (006)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: compiler

---

## C0A-006-001 — UI: "tab" component vocabulary

A: "The tab component loses focus state on keyboard navigation."
B: ["UI"] — "tab component" + "focus state" → UI predicate. Accessibility/interaction vocabulary.
CompiledPredicateIds: ["UI"]

---

## C0A-006-002 — TEST+UI: write tests for tab behavior

A: "Write keyboard navigation tests for the tab panel."
B: ["TEST", "UI"] — "write keyboard navigation tests" → TEST (5.3); "tab panel" → UI.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-006-003 — gap: "accessibility audit"

A: "Run an accessibility audit on the checkout page."
B: [] — "accessibility audit" without test-writing verb or QA abbreviation → gap. "run" is operational.
CompiledPredicateIds: []

---

## C0A-006-004 — IMPLEMENT+TEST+UI+QA: four-predicate UI flow

A: "Build the file upload component, test drag-and-drop interactions, and QA before sprint review."
B: ["IMPLEMENT", "TEST", "UI", "QA"] — "build" → IMPLEMENT; "test drag-and-drop" → TEST+UI; "QA before sprint review" → QA.
CompiledPredicateIds: ["IMPLEMENT", "TEST", "UI", "QA"]

---

## C0A-006-005 — gap: "CSS variables"

A: "Update the CSS variables for the dark mode theme."
B: ["UI"] — "CSS variables", "dark mode theme" → UI predicate fires on styling vocabulary.
CompiledPredicateIds: ["UI"]
