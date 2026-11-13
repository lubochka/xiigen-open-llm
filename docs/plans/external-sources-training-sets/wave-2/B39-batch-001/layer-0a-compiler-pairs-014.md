# Layer 0-A — Compiler Pairs (014)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-014-001 — TEST+UI: Playwright login form automation

A: "Write Playwright test cases for the login form: valid credentials, invalid password, and locked-out account states."
B: ["TEST", "UI"] — "write test cases" → TEST; "login form" → UI (form); "Playwright" → gap.
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-014-002 — TEST+UI+IMPLEMENT: build + test login page

A: "Implement the login page and write assertions for each validation error state on the input fields."
B: ["TEST", "UI", "IMPLEMENT"] — "implement" → IMPLEMENT; "write assertions" → TEST; "login page" → UI (page); "input fields" → UI (input).
CompiledPredicateIds: ["TEST", "UI", "IMPLEMENT"]

---

## C0A-014-003 — gap: "configure Playwright retries"

A: "Configure Playwright to retry flaky tests three times before failing."
B: [] — "configure" is gap (not in IMPLEMENT lexicon); "Playwright" → gap; "retry" → gap (operational); "flaky tests" — "tests" alone without a test-writing verb is gap.
CompiledPredicateIds: []

---

## C0A-014-004 — TEST+UI: spec coverage for nav menu

A: "Add spec coverage for the top navigation menu keyboard accessibility interactions."
B: ["TEST", "UI"] — "add spec coverage" → TEST (spec + coverage in test context); "navigation menu" → UI; "keyboard accessibility" → UI (accessibility).
CompiledPredicateIds: ["TEST", "UI"]

---

## C0A-014-005 — TEST alone: validate API response in Playwright flow

A: "Write Playwright assertions that validate the API (Application Programming Interface) response status shown in the error toast after a failed login."
B: ["TEST"] — "write assertions" + "validate" → TEST; "toast" — UI fires on toast? "toast" is in UI component lexicon (snackbar/toast); yes: "error toast" → UI.
Correction: CompiledPredicateIds: ["TEST", "UI"]
CapabilityGapWarnings: ["Playwright" is gap — framework name]
