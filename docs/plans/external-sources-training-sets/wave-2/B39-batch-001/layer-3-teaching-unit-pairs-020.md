# Layer 3 — Teaching Unit Pairs (020)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU020-001 — "accessibility" fires UI; "aria-label" fires UI

A: "Write accessibility audit tests for the modal, form inputs, and navigation." What fires?
B: TEST fires on "write accessibility audit tests". UI fires on "modal" (modal), "form inputs" (form + input), "navigation" (nav), and "accessibility" (accessibility is in UI lexicon).
Result: ["TEST", "UI"]. Teaching: "accessibility" is confirmed in the UI lexicon. ARIA vocabulary and accessibility terms fire UI.

---

## TU020-002 — "review accessibility report" — "review" is gap

A: "Review the accessibility report from the axe-core scan." Does QA fire?
B: No. "review" → gap. "accessibility report" → gap (no predicate fires on "report"). "axe-core" → gap (tool name). Zero predicates.
Result: []. Teaching: "review" never fires QA or TEST. The literal "QA" abbreviation must appear to fire QA.

---

## TU020-003 — "keyboard navigation" fires UI

A: "Validate that the dropdown supports full keyboard navigation." What fires?
B: TEST fires on "validate". UI fires on "dropdown" (dropdown → UI) and "keyboard navigation" (accessibility vocabulary → UI).
Result: ["TEST", "UI"]. Teaching: "keyboard navigation", "keyboard accessibility", and "keyboard interaction" all fire UI as accessibility sub-vocabulary.

---

## TU020-004 — "screen reader" fires UI

A: "Create test cases that verify screen reader announcements for each modal state." What fires?
B: TEST fires on "create test cases" + "verify" (deduplicated). UI fires on "modal" and "screen reader" (accessibility → UI).
Result: ["TEST", "UI"]. Teaching: "screen reader" is in the UI accessibility vocabulary. It fires UI.

---

## TU020-005 — WCAG vocabulary fires UI

A: "Assert WCAG 2.1 AA compliance for the form inputs and modal." What fires?
B: TEST fires on "assert". UI fires on "form inputs" (form, input) and "modal". Does "WCAG 2.1 AA compliance" fire UI? Yes — WCAG/accessibility compliance vocabulary fires UI.
Result: ["TEST", "UI"]. Teaching: WCAG vocabulary fires UI as part of the accessibility sub-lexicon.
