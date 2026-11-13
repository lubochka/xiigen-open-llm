# Layer 3 — Teaching Unit Pairs (029)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU029-001 — PasswordResetTest: "profile" is gap — core lesson

A: What is the single most important lesson from PasswordResetTest?
B: "profile the password reset form submission" → [UI] only, not [TEST, UI]. "profile" is an explicitly listed gap verb for TEST. This is a high-importance disambiguation: "profile" looks like a test verb (it's used in performance profiling of tests) but it fires nothing. Only "form" (UI vocabulary) fires, giving [UI].

---

## TU029-002 — PasswordResetTest: "validate" fires TEST regardless of scenario

A: "Validate the reset form rejects expired tokens." Does the expired-token context prevent TEST from firing?
B: No. "validate" fires TEST in any scenario context — valid, invalid, expired, error. The token-expiry domain is irrelevant to predicate firing. "validate" is a test assertion trigger in all contexts.

---

## TU029-003 — PasswordResetTest: "countdown timer" is not UI vocabulary

A: "Update the password reset screen to show a countdown timer." What fires UI?
B: "countdown timer" is not in the UI lexicon. "screen" is in the UI lexicon. UI fires on "password reset screen" via the "screen" token. "update" is gap for IMPLEMENT. Result: [UI] only. Lesson: identify the explicit UI lexicon token ("screen"), not the semantic description.

---

## TU029-004 — PasswordResetTest: QA before staging deployment → 7.1

A: "QA before staging deployment" — what routing?
B: QA → 7.1 (ReleaseGateFlow71). "staging deployment" is explicit staging vocabulary → 7.1 override. Contrast: "QA before merge" or "QA acceptance" → 7.3.

---

## TU029-005 — PasswordResetTest: expired-link modal pattern

A: "Assert the expired reset link modal appears." Teach the full predicate derivation.
B: "assert" → TEST (standalone test assertion verb). "expired reset link modal" → "modal" → UI. Two independent vocabulary categories each fire one predicate. Result: [TEST, UI]. This is the standard assert + UI component = [TEST, UI] template that recurs in all Round 2 topics.
