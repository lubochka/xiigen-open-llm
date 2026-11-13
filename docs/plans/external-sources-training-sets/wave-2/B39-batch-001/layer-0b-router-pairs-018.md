# Layer 0-B — Router Pairs (018)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-018-001 — TEST+UI+QA → 5.3 + 1.5 + 7.3: cross-browser suite

A: ["TEST", "UI", "QA"] | "Write cross-browser tests for the product listing page layout on Chrome, Firefox, Safari, and QA the results"
B: TEST → 5.3; UI → 1.5; QA → 7.3.
Reason: cross-browser test context → 5.3; "product listing page" + "layout" → UI → 1.5; "QA the results" → 7.3.

---

## C0B-018-002 — TEST+UI → 5.3 + 1.5: modal on IE11/Edge

A: ["TEST", "UI"] | "Add test coverage for the confirmation modal on IE11 and Edge"
B: TEST → 5.3; UI → 1.5. "add test coverage" → 5.3; "confirmation modal" → UI → 1.5.
Reason: Browser compatibility test context → 5.3 not 5.1.

---

## C0B-018-003 — zero predicates → GapHandler: check compatibility table

A: [] | "Check the browser compatibility table for CSS grid support"
B: GapHandler. "check" is operational gap. "CSS grid" → gap. Zero predicates.

---

## C0B-018-004 — TEST+UI → 5.3 + 1.5: validate responsive layout on browsers

A: ["TEST", "UI"] | "Validate the responsive layout of the navigation bar across Chrome and Firefox"
B: TEST → 5.3; UI → 1.5. "validate" in browser test context → 5.3; "responsive layout" + "navigation bar" → UI → 1.5.

---

## C0B-018-005 — TEST+UI+QA → 5.3 + 1.5 + 7.3: multi-browser regression + QA acceptance

A: ["TEST", "UI", "QA"] | "Write cross-browser regression test cases for each form input component and get QA acceptance"
B: TEST → 5.3; UI → 1.5; QA → 7.3. Three routing decisions.
AllSubPhaseIds: [5.3, 1.5, 7.3]
