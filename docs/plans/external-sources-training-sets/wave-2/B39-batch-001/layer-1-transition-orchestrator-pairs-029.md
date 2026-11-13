# Layer 1 — Transition Orchestrator Pairs (029)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T029-001 — PasswordResetTest: TransitionType

A: "Write E2E test coverage for the password reset flow — assert the form accepts valid email." TransitionType?
B: documentation. TEST+UI without IMPLEMENT → documentation. Password reset test-writing is documentation.

---

## T029-002 — PasswordResetTest: "profile the form" fires only UI

A: "Profile the password reset form submission." TransitionType?
B: documentation. Only UI fires (form). "profile" is gap for TEST. UI alone → documentation. TransitionType: documentation.

---

## T029-003 — PasswordResetTest: QA 7.1 before staging — execution phases

A: "Write password reset tests, verify the screen, and QA before staging deployment." Execution phases?
B: Phase 1 (parallel): E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5). Phase 2 (last): ReleaseGateFlow71 (7.1) — "before staging deployment" = staging vocabulary → 7.1. TransitionType: documentation.

---

## T029-004 — PasswordResetTest: "validate" fires TEST regardless of context

A: "Validate the reset form rejects expired tokens." TransitionType?
B: documentation. TEST (validate) + UI (reset form → form). TEST+UI without IMPLEMENT → documentation. "validate" fires TEST even in an expired-token error scenario — the verb is a test assertion trigger regardless of the domain context.

---

## T029-005 — PasswordResetTest: expired-link modal execution summary

A: "Assert the expired reset link modal appears." Full orchestrator output?
B: Predicates: [TEST, UI]. TransitionType: documentation. Flows: E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5) in parallel. No QA flow (QA predicate not present). TrustState: deterministic_generic.
