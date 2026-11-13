# Layer 0-C — Knowledge Edge Pairs (029)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E029-001 — PasswordResetTest: gap vocabulary

A: Which words are gap in password reset test requests?
B: Gap: "run password reset tests" ("run" is gap), "check that reset link works" ("check" is gap), "profile the reset form performance" ("profile" is gap), "configure the reset token expiry" ("configure" is gap), "update the password reset logic" ("update" is gap for IMPLEMENT), "monitor reset requests" ("monitor" is gap). "password reset" alone does not fire any predicate.

---

## E029-002 — PasswordResetTest: countdown timer is not UI

A: "Update the password reset screen to show a countdown timer." Does "countdown timer" fire UI?
B: "countdown timer" is not in the UI lexicon. "password reset screen" → "screen" fires UI. Result: [UI] from "screen". IMPLEMENT does not fire ("update" is gap). No TEST. Lesson: identify the explicit UI vocabulary word ("screen") rather than the semantic description ("countdown timer").

---

## E029-003 — PasswordResetTest: "expired link" modal fires UI

A: "Assert the expired reset link modal appears." What fires?
B: TEST ("assert" → TEST); UI ("expired reset link modal" → "modal" fires UI). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## E029-004 — PasswordResetTest: "profile" is gap

A: Does "profile the password reset form submission" fire TEST?
B: No. "profile" is explicitly listed as a gap verb for TEST. "profile" does not fire TEST, IMPLEMENT, or QA. "password reset form" → "form" fires UI. Result: [UI]. Only the UI predicate fires. This is a B39-specific teaching point about gap verbs that look like test verbs.

---

## E029-005 — PasswordResetTest: "validate" fires TEST even for expired-token scenario

A: "Validate the reset form rejects expired tokens." What fires?
B: TEST ("validate" → standalone assertion verb → TEST fires); UI ("reset form" → "form" fires UI). Result: [TEST, UI]. Routing: [5.3, 1.5]. "validate" is always a TEST trigger regardless of the scenario (valid, invalid, expired-token context).
