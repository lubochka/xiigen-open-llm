# Layer 0-A — Compiler Pairs (027)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-027-001 — UserRegistrationTest: predicate compilation

A: "Write E2E tests for the user registration form — assert required fields show inline error messages, verify the success screen appears after valid submission."
B: Predicates: TEST ("write E2E tests", "assert", "verify" → deduplicated); UI ("registration form", "inline error messages", "success screen" → deduplicated). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-027-002 — UserRegistrationTest: IMPLEMENT overrides

A: "Build the registration form component and write test cases for email format validation and password strength indicators."
B: Predicates: IMPLEMENT ("build"); TEST ("write test cases" → TEST); UI ("registration form component", "email format", "password strength indicators" → deduplicated). Result: [IMPLEMENT, TEST, UI]. TransitionType: feature_addition. Routing: [4.1, 5.3, 1.5].

---

## C0A-027-003 — UserRegistrationTest: "check" is gap

A: "Check that the registration screen accepts valid email addresses."
B: Predicates: UI ("registration screen" → UI). TEST does NOT fire — "check" is gap. No test-writing verb present. Result: [UI]. Routing: [1.5]. TransitionType: documentation.

---

## C0A-027-004 — UserRegistrationTest: "validate" fires TEST

A: "Validate the registration form submits correctly when all required fields are filled."
B: Predicates: TEST ("validate" → standalone assertion verb fires TEST); UI ("registration form" → UI). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-027-005 — UserRegistrationTest: QA co-fires

A: "Write user registration E2E test scenarios, assert the confirmation email screen appears, and QA acceptance before merge."
B: Predicates: TEST ("write", "assert" → deduplicated); UI ("confirmation email screen" → UI); QA ("QA acceptance" → QA). Result: [TEST, UI, QA]. Routing: [5.3, 1.5, 7.3]. TransitionType: documentation.
