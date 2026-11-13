# Held-Out and Corruption Pairs (014)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H014-001 — Held-out: TEST+UI — Playwright login form test cases

A: "Write Playwright test cases for the login form: valid credentials, invalid password, locked-out account."
  Expected: ["TEST", "UI"]
  Held-out: "write test cases" → TEST; "login form" → UI (form). "Playwright" → gap.

---

## H014-002 — Held-out: UI only — "configure retry logic for login page"

A: "Configure retry logic for the login page tests."
  Expected: ["UI"]
  Held-out: "configure" → gap (not IMPLEMENT); "login page" → UI (page). No test-writing verb → no TEST. UI fires on "login page".

---

## H014-003 — Held-out: TEST+UI — toast fires UI

A: "Write Playwright assertions that validate the API response status shown in the error toast after a failed login."
  Expected: ["TEST", "UI"]
  Held-out: "write assertions" + "validate" → TEST; "error toast" → UI. "Playwright" → gap. "API response status" → gap.

---

## C014-001 — Corruption v6-02: "configure" → IMPLEMENT (wrong)

A: "Configure Playwright to retry flaky tests three times."
  Correct: []
  Corrupt: ["IMPLEMENT"] ← v6-02 ("configure" incorrectly treated as IMPLEMENT)
  Teaching: "configure" is consistently gap for all predicates. IMPLEMENT does not fire on "configure".

---

## C014-002 — Corruption v6-01: separate PLAYWRIGHT predicate invented

A: "Write Playwright test cases for the login form."
  Correct: ["TEST", "UI"]
  Corrupt: ["TEST", "UI", "PLAYWRIGHT"] ← v6-01 (PLAYWRIGHT invented as a predicate)
  Teaching: Framework names (Playwright, Cypress, Selenium) are gap vocabulary and never become predicates. There is no PLAYWRIGHT predicate.
