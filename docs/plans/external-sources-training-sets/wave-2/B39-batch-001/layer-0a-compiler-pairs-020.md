# Layer 0-A — Compiler Pairs (020)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-020-001 — TEST+UI+QA: accessibility audit test suite with QA gate

A: "Write accessibility audit tests for the modal, form inputs, and navigation, and QA sign-off on WCAG (Web Content Accessibility Guidelines) 2.1 AA compliance."
B: ["TEST", "UI", "QA"] — "write accessibility audit tests" → TEST; "modal", "form inputs", "navigation" → UI; "QA sign-off" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]

---

## C0A-020-002 — TEST+UI: assert ARIA (Accessible Rich Internet Applications) labels on button

A: "Assert that every button in the checkout flow has a non-empty aria-label attribute."
B: ["TEST", "UI"] — "assert" → TEST; "button" → UI (button); "aria-label" → UI (accessibility).
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-020-003 — gap: "review accessibility report"

A: "Review the accessibility report from the last axe-core scan."
B: [] — "review" → gap (not a test-writing verb; review ≠ write/assert/validate); "accessibility report" → gap; "axe-core" → gap (tool name). Zero predicates.
CompiledPredicateIds: []

---

## C0A-020-004 — TEST+UI: validate keyboard navigation on dropdown

A: "Validate that the dropdown component supports full keyboard navigation: arrow keys, Enter, and Escape key handling."
B: ["TEST", "UI"] — "validate" → TEST; "dropdown component" → UI (component); "keyboard navigation" → UI (accessibility).
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-020-005 — TEST+UI+QA: screen reader + QA accessibility acceptance

A: "Create test cases that verify screen reader announcements for each modal state and QA acceptance for accessibility compliance."
B: ["TEST", "UI", "QA"] — "create test cases" + "verify" → TEST; "modal" → UI (modal); "screen reader" → UI (accessibility); "QA acceptance" → QA.
CompiledPredicateIds: ["TEST", "UI", "QA"]
