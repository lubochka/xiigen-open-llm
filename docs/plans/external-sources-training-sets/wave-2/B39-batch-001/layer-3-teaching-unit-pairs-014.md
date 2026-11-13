# Layer 3 — Teaching Unit Pairs (014)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU014-001 — Playwright is gap; "test cases" fires TEST

A: "Write Playwright test cases for the login form." What predicates fire?
B: TEST fires on "write test cases". UI fires on "login form" (form). "Playwright" is gap. Result: ["TEST", "UI"].
Teaching: Same rule as Cypress — framework names are consistently gap in B39.

---

## TU014-002 — "configure" does not fire IMPLEMENT

A: "Configure retry logic for the login page tests." Does IMPLEMENT fire?
B: No. "configure" is not in the IMPLEMENT lexicon. IMPLEMENT lexicon: build, create, write, implement, add, develop, generate, scaffold, extend. "configure" is gap. "login page" → UI (page). "tests" without test-writing verb in this context — "Configure retry logic" is the main clause; "tests" is a noun used as an object of "for", not a test-writing verb.
Result: ["UI"] — only "login page" fires UI. No IMPLEMENT, no TEST.

---

## TU014-003 — "spec coverage" fires TEST via compound pattern

A: "Add spec coverage for the navigation menu keyboard accessibility." Does TEST fire?
B: Yes. "add spec coverage" → TEST. "spec" is a test noun; "coverage" is a test noun; "add" is the writing verb. The compound "add spec coverage" fires TEST. "navigation menu" + "keyboard accessibility" → UI.
Result: ["TEST", "UI"].

---

## TU014-004 — "run tests" with "login page" — only UI fires

A: "Run Playwright tests for the login page before deploying." What predicates fire?
B: UI fires on "login page" (page). TEST does not fire — "run" is operational gap. "Playwright" is gap. "before deploying" is gap.
Result: ["UI"]. Teaching: "run tests" is the same as "execute tests" — operational, not test-writing.

---

## TU014-005 — "toast" is in UI component vocabulary

A: "Write assertions for the error toast after a failed login." Does UI fire?
B: Yes. "write assertions" → TEST. "error toast" → UI (toast/snackbar is in UI component vocabulary).
Result: ["TEST", "UI"]. Teaching: Toast, snackbar, and notification popup are all in the UI component lexicon.
