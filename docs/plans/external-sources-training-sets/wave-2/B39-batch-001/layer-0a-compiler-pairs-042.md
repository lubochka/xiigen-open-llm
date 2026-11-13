# Layer 0-A — Compiler Pairs (042)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-042-001 — EmailVerificationTest: predicate compilation

A: "Write E2E test cases for the email verification page — assert the verification button is enabled after token entry, verify the success screen renders."
B: TEST (write, assert, verify). UI ("email verification page" → page; "verification button" → button; "success screen" → screen — three triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## C0A-042-002 — EmailVerificationTest: expired token modal

A: "Assert the expired verification token modal appears." Predicates?
B: TEST (assert). UI (expired verification token modal → modal). Result: [TEST, UI].

---

## C0A-042-003 — EmailVerificationTest: "resend email button" fires UI

A: "Create test coverage to verify the resend email button triggers correctly." Predicates?
B: TEST (create test coverage, verify). UI (resend email button → button). Result: [TEST, UI].

---

## C0A-042-004 — EmailVerificationTest: "configure email provider" is gap

A: "Configure the email provider for verification emails." Predicates?
B: "configure" is gap. "email provider" has no UI vocabulary. Zero predicates → knowledge_edge_synthetic.

---

## C0A-042-005 — EmailVerificationTest: [TEST, UI, QA] before release

A: "Write email verification E2E tests, assert the verification page, and QA sign-off before release." Predicates?
B: TEST (write, assert). UI (page). QA (QA sign-off → 7.1 "before release"). [TEST, UI, QA]. AllSubPhaseIds: [5.3, 1.5, 7.1]. TransitionType: documentation.
