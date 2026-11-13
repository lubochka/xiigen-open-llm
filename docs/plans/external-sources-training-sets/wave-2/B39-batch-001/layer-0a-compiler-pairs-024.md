# Layer 0-A — Compiler Pairs (024)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-024-001 — Complete predicate reference: TEST fires

A: What exact words fire the TEST predicate in B39 context?
B: TEST fires on: test, tests, test case, test cases, test suite, test coverage, spec, specs, spec coverage, e2e, end-to-end, scenario, scenarios, assert, assertions, verify, validate, regression, regression test, BDD (Behaviour-Driven Development), snapshot test, visual regression test, accessibility test, interaction test.
TEST does NOT fire on: run, execute, automate, automated, trigger, Cypress, Playwright, Selenium, check, review.
Sub-phase routing: "unit test"/"unit tests" → 5.1; "e2e"/"end-to-end"/"browser test"/"UI test"/"visual regression" → 5.3.

---

## C0A-024-002 — Complete predicate reference: UI fires

A: What exact vocabulary fires the UI predicate?
B: UI fires on: button, input, form, modal, page, screen, layout, component, responsive, accessibility, aria, visual, tooltip, dropdown, carousel, tab, tab panel, navigation, nav, pagination, badge, chip, accordion, popover, snackbar, toast, skeleton, header, footer, sidebar, breadcrumb, focus trap, drag-and-drop zone, placeholder, checkbox, toggle, radio, select.
UI does NOT fire on: dashboard, API, service, repository, controller, database, pipeline, module.

---

## C0A-024-003 — Complete predicate reference: QA fires

A: What exact vocabulary fires the QA predicate?
B: QA fires on: QA (abbreviation only — must appear as "QA"), quality assurance does NOT fire (spelled-out form is gap), QA sign-off, QA gate, QA review, QA acceptance, QA the [noun], get QA.
QA does NOT fire on: quality, assurance, review (standalone), validate (standalone), acceptance (standalone without QA).

---

## C0A-024-004 — Complete reference: TransitionType rules for TEST+UI+QA

A: What is the TransitionType when only TEST+UI+QA are present (no IMPLEMENT)?
B: TransitionType: documentation (Rule: TEST+UI without IMPLEMENT → documentation). When IMPLEMENT is present → feature_addition (IMPLEMENT overrides). QA alone → no TransitionType contribution. Summary: IMPLEMENT present → feature_addition; TEST+UI without IMPLEMENT → documentation; QA does not set TransitionType.

---

## C0A-024-005 — Complete reference: sub-phase routing summary for B39

A: Which sub-phases does each predicate route to?
B: TEST → 5.3 (End-to-End and UI Testing) when E2E/browser/UI context; TEST → 5.1 (Unit and Integration Testing) when unit test context.
UI → 1.5 (UI/UX Prototyping and Component Design) — always, regardless of surrounding context.
QA → 7.3 (QA Testing and Validation, feature acceptance) by default; QA → 7.1 (Release Pipeline) when release/staging/deploy vocabulary present.
Combination TEST+UI+QA → routes: 5.3 + 1.5 + 7.3 (three independent sub-phases).
