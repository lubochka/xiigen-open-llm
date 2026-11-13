# Layer 0-B — Router Pairs (022)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-022-001 — TEST+UI → 5.3 + 1.5: form validation e2e

A: ["TEST", "UI"] | "Write end-to-end test cases covering every validation rule on the contact form"
B: TEST → 5.3; UI → 1.5. "write end-to-end test cases" → 5.3; "contact form" → UI → 1.5.

---

## C0B-022-002 — TEST+UI → 5.3 + 1.5: assert inline error on email input

A: ["TEST", "UI"] | "Assert that submitting the contact form with invalid email input shows the inline validation error"
B: TEST → 5.3; UI → 1.5. "assert" → 5.3; "contact form" + "email input" → UI → 1.5.

---

## C0B-022-003 — DOCUMENT alone: document form validation rules

A: ["DOCUMENT"] | "Document the form validation rules for the developer handbook"
B: DOCUMENT → DocumentationFlow62. No TEST/UI/QA fires.
Note: This routing is outside the TEST+UI+QA triad — DOCUMENT predicate routes to 6.2 independently.

---

## C0B-022-004 — TEST+UI → 5.3 + 1.5: spec for password strength meter

A: ["TEST", "UI"] | "Add spec coverage for the password strength meter: weak/medium/strong states, indicator bar, aria-live region"
B: TEST → 5.3; UI → 1.5. "spec coverage" → 5.3; "password strength meter" + "indicator bar" + "aria-live region" → UI → 1.5.

---

## C0B-022-005 — TEST+UI+QA → 5.3 + 1.5 + 7.3: full form e2e + QA gate

A: ["TEST", "UI", "QA"] | "Create end-to-end test scenarios for every validation rule on the registration form and get QA acceptance"
B: TEST → 5.3; UI → 1.5; QA → 7.3.
AllSubPhaseIds: [5.3, 1.5, 7.3]
