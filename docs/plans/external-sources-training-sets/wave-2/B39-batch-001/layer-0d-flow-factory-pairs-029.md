# Layer 0-D — Flow Factory Pairs (029)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F029-001 — PasswordResetTest: flow selection

A: "Write E2E test coverage for the password reset flow — assert the reset form accepts valid email, verify the success screen." Predicates [TEST, UI]. Flows?
B: E2ETestingFlow53 (5.3) for TEST. UIInteractionFlow15 (1.5) for UI ("reset form" → form; "success screen" → screen). Two flows parallel. TransitionType: documentation. TrustState: deterministic_generic.

---

## F029-002 — PasswordResetTest: "profile" is gap — UI still fires

A: "Profile the password reset form submission." Flows?
B: "profile" is gap — does not fire TEST. UI fires on "password reset form" → "form". Single flow: UIInteractionFlow15 (1.5). TransitionType: documentation. TrustState: deterministic_generic. No E2ETestingFlow53 because TEST did not fire.

---

## F029-003 — PasswordResetTest: expired-token modal fires UI

A: "Assert the expired reset link modal appears." Full factory output?
B: TEST fires (assert). UI fires (modal). E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5). TransitionType: documentation. TrustState: deterministic_generic. Two flows parallel.

---

## F029-004 — PasswordResetTest: QA with 7.1 routing

A: "Write password reset E2E tests, verify the reset screen, and QA before staging deployment." Full factory output?
B: E2ETestingFlow53 (5.3): TEST. UIInteractionFlow15 (1.5): UI (screen). ReleaseGateFlow71 (7.1): QA → "before staging deployment" = staging vocabulary → 7.1. Order: 5.3 + 1.5 parallel → 7.1. TransitionType: documentation.

---

## F029-005 — PasswordResetTest: Node 1 domain identification

A: Node 1 for PasswordResetTest. SelectedLogicalTransition and domains?
B: SelectedLogicalTransition = E2ETestingFlow53. Domains: reset request form (email input), reset link email delivery, reset link expiry state, new password form (password + confirm-password inputs), expired token modal, success screen after reset.
