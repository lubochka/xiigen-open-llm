# Layer 0-C — Knowledge Edge Pairs (008)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-05 | pair_kind: knowledge_edge

---

## KE-008-001 — TEST fires on "write" with test target nouns

"write tests for X" → TEST (test-writing verb + test target)
"write test cases for X" → TEST
"write assertions for X" → TEST
"write scenarios for X" (BDD context) → TEST
Gap: "write documentation for X" → DOCUMENT (not TEST)
Gap: "write code for X" → IMPLEMENT (not TEST, unless "tests" is in the clause)

---

## KE-008-002 — "implement tests" vs "implement a feature"

"implement E2E tests" → TEST (test-writing verb in test context)
"implement the button component" → IMPLEMENT+UI (feature implementation)
Rule: "implement" fires TEST when followed by test vocabulary; fires IMPLEMENT when followed by feature/component vocabulary.
Note: "implement tests" is the edge case where IMPLEMENT fires TEST (test-writing intent). The test-writing context takes precedence.

---

## KE-008-003 — UI predicate: accessibility vocabulary

"ARIA role" / "ARIA label" / "ARIA attribute" → UI (accessibility of UI elements)
"aria-required" / "aria-expanded" / "aria-hidden" → UI
"keyboard navigation" / "keyboard shortcut in UI" / "tab order" / "focus management" → UI
"screen reader" / "high contrast mode" / "color contrast ratio" → UI (accessibility)
Gap: "accessibility audit" alone (without test-writing verb or IMPLEMENT) → gap

---

## KE-008-004 — "error state" disambiguation

"error state" in UI context (form validation error display) → UI
"write tests for error states" → TEST+UI
"handle error state in API" → not UI (backend error handling)
Rule: "error state" fires UI when applied to UI/form display; not for backend error handling.
