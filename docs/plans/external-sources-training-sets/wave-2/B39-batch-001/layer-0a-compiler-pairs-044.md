# Layer 0-A — Compiler Pairs (044)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-044-001 — TwoFactorAuthTest: predicate compilation

A: "Write E2E test cases for the two-factor authentication (2FA) screen — assert the OTP (One-Time Password) input field renders, verify the verify button activates."
B: TEST (write, assert, verify). UI ("2FA screen" → screen; "OTP input field" → input; "verify button" → button — three triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## C0A-044-002 — TwoFactorAuthTest: backup code modal fires UI

A: "Create test coverage asserting the backup code modal appears when OTP fails." Predicates?
B: TEST (create test coverage, asserting). UI (backup code modal → modal). Result: [TEST, UI].

---

## C0A-044-003 — TwoFactorAuthTest: toggle fires UI

A: "Assert the 2FA enable/disable toggle on the security settings page works correctly." Predicates?
B: TEST (assert). UI ("enable/disable toggle" → toggle; "security settings page" → page — two triggers → one UI). Result: [TEST, UI].

---

## C0A-044-004 — TwoFactorAuthTest: "configure 2FA" is gap

A: "Configure the 2FA settings for all users." Predicates?
B: "configure" is gap. "2FA settings" has no UI vocabulary. Zero predicates → knowledge_edge_synthetic.

---

## C0A-044-005 — TwoFactorAuthTest: QA before production

A: "Write 2FA E2E tests, assert the OTP input, and QA sign-off before production deploy." Predicates?
B: TEST (write, assert). UI (OTP input → input). QA (QA sign-off → 7.1 "before production deploy"). [TEST, UI, QA]. AllSubPhaseIds: [5.3, 1.5, 7.1]. TransitionType: documentation.
