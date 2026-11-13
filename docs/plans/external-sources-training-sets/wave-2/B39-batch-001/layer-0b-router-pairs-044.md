# Layer 0-B — Router Pairs (044)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-044-001 — TwoFactorAuthTest: routing

A: "Write E2E tests for the 2FA screen — assert OTP input and verify button." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (screen, input, button). AllSubPhaseIds: [5.3, 1.5]. TransitionType: documentation.

---

## C0B-044-002 — TwoFactorAuthTest: backup code modal → 1.5

A: Does a "backup code modal" route to 1.5?
B: Yes. Modal → 1.5 always.

---

## C0B-044-003 — TwoFactorAuthTest: toggle + page → 1.5

A: "Assert the 2FA toggle on the security settings page." AllSubPhaseIds?
B: [5.3, 1.5]. Toggle → 1.5. Page → 1.5. Two UI triggers → one UI predicate → one routing decision to 1.5.

---

## C0B-044-004 — TwoFactorAuthTest: QA before production → 7.1

A: "Write 2FA tests, assert OTP input, and QA before production deploy." AllSubPhaseIds?
B: [5.3, 1.5, 7.1]. QA → 7.1 ("before production deploy"). Order: 5.3 + 1.5 → 7.1.

---

## C0B-044-005 — TwoFactorAuthTest: "configure 2FA" gap

A: "Configure 2FA settings." Predicates []. Routing?
B: Zero predicates → knowledge_edge_synthetic. No routing.
