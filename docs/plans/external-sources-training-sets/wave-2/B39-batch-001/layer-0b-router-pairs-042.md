# Layer 0-B — Router Pairs (042)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-042-001 — EmailVerificationTest: routing

A: "Write E2E tests for the email verification page — assert button and success screen." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (page, button, screen). AllSubPhaseIds: [5.3, 1.5]. TransitionType: documentation.

---

## C0B-042-002 — EmailVerificationTest: "configure email provider" gap

A: "Configure the email provider." Predicates []. Routing?
B: Zero predicates → knowledge_edge_synthetic. No routing.

---

## C0B-042-003 — EmailVerificationTest: [TEST, UI, QA → 7.1]

A: "Write email verification tests, assert the page, and QA sign-off before release." AllSubPhaseIds?
B: AllSubPhaseIds: [5.3, 1.5, 7.1]. QA → 7.1 ("before release"). Order: 5.3 + 1.5 → 7.1.

---

## C0B-042-004 — EmailVerificationTest: expired token modal → 1.5

A: Does an "expired verification token modal" route to 1.5?
B: Yes. Modal → 1.5. Always UIInteractionFlow15.

---

## C0B-042-005 — EmailVerificationTest: resend button → 1.5

A: Does a "resend email button" route to 1.5?
B: Yes. Button → 1.5. All button vocabulary routes to UIInteractionFlow15.
