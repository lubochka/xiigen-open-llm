# Layer 0-A — Compiler Pairs (029)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-029-001 — PasswordResetTest: predicate compilation

A: "Write test cases for the password reset flow — assert the reset link form accepts a valid email, verify the confirmation screen appears."
B: Predicates: TEST ("write test cases", "assert", "verify" → deduplicated); UI ("password reset form", "email", "confirmation screen" → "form" and "screen" fire UI). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-029-002 — PasswordResetTest: "update" is gap for IMPLEMENT

A: "Update the password reset screen to show a countdown timer."
B: Predicates: UI ("password reset screen" → UI). IMPLEMENT does NOT fire — "update" is gap for IMPLEMENT. No test verb. Result: [UI]. Routing: [1.5]. TransitionType: documentation.

---

## C0A-029-003 — PasswordResetTest: assert fires TEST alone

A: "Assert the password reset confirmation modal closes after the user clicks the dismiss button."
B: Predicates: TEST ("assert" → standalone test-writing verb fires TEST); UI ("password reset confirmation modal", "dismiss button" → deduplicated). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-029-004 — PasswordResetTest: full predicate set

A: "Implement the password reset modal, write E2E scenarios for valid and expired reset links, assert the error message banner renders on expiry, and QA gate before merge."
B: Predicates: IMPLEMENT ("implement"); TEST ("write E2E scenarios", "assert" → deduplicated); UI ("password reset modal", "error message banner" → deduplicated); QA ("QA gate" → QA). Result: [IMPLEMENT, TEST, UI, QA]. TransitionType: feature_addition. Routing: [4.1, 5.3, 1.5, 7.3].

---

## C0A-029-005 — PasswordResetTest: "profile" is gap

A: "Profile the password reset form submission time to ensure it meets the SLA."
B: Predicates: UI ("password reset form" → "form" fires UI). TEST does NOT fire — "profile" is a gap verb. Result: [UI]. Routing: [1.5]. TransitionType: documentation.
