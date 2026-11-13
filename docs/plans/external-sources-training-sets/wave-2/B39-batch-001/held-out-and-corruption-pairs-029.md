# Held-Out and Corruption Pairs (029)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H029-001 — Held-out: PasswordResetTest — expired token modal

A: "Assert the expired reset link modal appears, validate the reset form rejects expired tokens."
  Expected: ["TEST", "UI"]
  Held-out: TEST ("assert", "validate" → two triggers → one TEST). UI ("expired reset link modal" → modal, "reset form" → form → two triggers → one UI). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## H029-002 — Held-out: PasswordResetTest — "profile" gap

A: "Profile the password reset form submission performance."
  Expected: ["UI"]
  Held-out: "profile" is gap — TEST does not fire. "reset form" → form → UI fires. Result: [UI]. Single flow: UIInteractionFlow15 (1.5).

---

## H029-003 — Held-out: PasswordResetTest — QA before staging

A: "Write password reset E2E tests, verify the success screen, QA before staging deployment."
  Expected: ["TEST", "UI", "QA"]
  Held-out: TEST (write, verify). UI (success screen → screen). QA (QA). QA → 7.1 ("before staging deployment"). AllSubPhaseIds: [5.3, 1.5, 7.1]. TransitionType: documentation.

---

## C029-001 — Corruption v6-01: PASSWORD_RESET predicate

Pattern: v6-01. Corrupt: predicates = ["TEST", "UI", "PASSWORD_RESET"].
Correct: predicates = ["TEST", "UI"]. Domain name does not become a predicate.

---

## C029-002 — Corruption v6-02: "profile" → TEST

Pattern: v6-02. Corrupt: "Profile the reset form" → predicates = ["TEST"].
Correct: "profile" is gap. TEST does not fire. Only UI fires (form). predicates = ["UI"].
