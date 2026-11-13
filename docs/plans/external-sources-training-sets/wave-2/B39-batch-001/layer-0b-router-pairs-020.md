# Layer 0-B — Router Pairs (020)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-020-001 — TEST+UI+QA → 5.3 + 1.5 + 7.3: accessibility audit

A: ["TEST", "UI", "QA"] | "Write accessibility audit tests for modal, form inputs, and navigation, and QA sign-off on WCAG 2.1 AA"
B: TEST → 5.3; UI → 1.5; QA → 7.3.
Reason: "accessibility audit tests" → 5.3; UI vocabulary → 1.5; "QA sign-off" → 7.3.

---

## C0B-020-002 — TEST+UI → 5.3 + 1.5: ARIA labels on buttons

A: ["TEST", "UI"] | "Assert that every button in the checkout flow has a non-empty aria-label attribute"
B: TEST → 5.3; UI → 1.5. "assert" → 5.3; "button" + "aria-label" → UI → 1.5.

---

## C0B-020-003 — zero predicates → GapHandler: review accessibility report

A: [] | "Review the accessibility report from the last axe-core scan"
B: GapHandler. "review" is gap; "axe-core" → gap (tool name). Zero predicates.

---

## C0B-020-004 — TEST+UI → 5.3 + 1.5: keyboard navigation on dropdown

A: ["TEST", "UI"] | "Validate that the dropdown component supports full keyboard navigation: arrow keys, Enter, Escape"
B: TEST → 5.3; UI → 1.5. "validate" → 5.3; "dropdown component" + "keyboard navigation" → UI → 1.5.

---

## C0B-020-005 — TEST+UI+QA → 5.3 + 1.5 + 7.3: screen reader + QA acceptance

A: ["TEST", "UI", "QA"] | "Create test cases that verify screen reader announcements for each modal state and QA acceptance"
B: TEST → 5.3; UI → 1.5; QA → 7.3.
AllSubPhaseIds: [5.3, 1.5, 7.3]
