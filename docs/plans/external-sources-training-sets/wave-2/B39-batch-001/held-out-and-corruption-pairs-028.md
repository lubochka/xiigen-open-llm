# Held-Out and Corruption Pairs (028)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H028-001 — Held-out: LoginFlowTest — locked-account toast

A: "Write E2E test cases for the login page — assert the error toast fires on invalid credentials and verify the locked-account message appears."
  Expected: ["TEST", "UI"]
  Held-out: TEST ("write E2E test cases", "assert", "verify" → three triggers → one TEST). UI ("login page" → page, "error toast" → toast, "locked-account message" — "message" is not UI lexicon; only "page" and "toast" fire UI → two triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## H028-002 — Held-out: LoginFlowTest — Playwright is gap

A: "Use Playwright to automate and test the login form."
  Expected: ["UI"]
  Held-out: "Playwright" is gap. "automate" is gap. "test" as imperative verb without test noun = borderline/gap. "login form" → form → UI fires. Conservative: [UI] only. Routing: [1.5]. TransitionType: documentation.

---

## H028-003 — Held-out: LoginFlowTest — QA before merge → 7.3

A: "Write login E2E test coverage, assert the login button state, and QA gate before merge."
  Expected: ["TEST", "UI", "QA"]
  Held-out: TEST (write, assert). UI (login button → button). QA (QA gate). QA → 7.3 ("before merge" = acceptance). AllSubPhaseIds: [5.3, 1.5, 7.3]. TransitionType: documentation.

---

## C028-001 — Corruption v6-01: PLAYWRIGHT predicate

Pattern: v6-01. Corrupt: predicates = ["TEST", "PLAYWRIGHT"].
Correct: predicates = ["TEST"] (or [UI] depending on request). "PLAYWRIGHT" is a framework name — not a valid predicate. Only the seven valid predicates exist.

---

## C028-002 — Corruption v6-02: "automate" → TEST

Pattern: v6-02. Corrupt: "Automate the login E2E tests" → predicates = ["TEST"].
Correct: "automate" is gap. TEST does not fire from "automate". Only UI fires if UI vocabulary is present.
