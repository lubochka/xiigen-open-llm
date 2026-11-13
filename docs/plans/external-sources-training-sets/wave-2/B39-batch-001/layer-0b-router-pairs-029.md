# Layer 0-B — Router Pairs (029)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-029-001 — PasswordResetTest: routing decision

A: "Write test cases for the password reset flow — assert the form, verify the confirmation screen." Predicates: [TEST, UI]. Route.
B: TEST → 5.3 (E2ETestingFlow53): password reset is multi-step UI flow. UI → 1.5 (UIInteractionFlow15): "form", "screen" always route to 1.5. TransitionType: documentation.

---

## C0B-029-002 — PasswordResetTest: "update" as gap — UI only

A: "Update the password reset screen to show a countdown timer." Predicates: [UI]. Route.
B: UI → 1.5 (UIInteractionFlow15): "password reset screen" → "screen" fires UI → 1.5. IMPLEMENT does not fire (update is gap). TEST does not fire. Single sub-phase. TransitionType: documentation.

---

## C0B-029-003 — PasswordResetTest: full set routing

A: "Implement the password reset modal, write E2E scenarios, assert the error banner, QA gate." Predicates: [IMPLEMENT, TEST, UI, QA]. Route all.
B: IMPLEMENT → 4.1. TEST → 5.3. UI → 1.5 ("modal", "error banner" → "modal", "banner" fire UI → 1.5). QA → 7.3 (default, no release vocabulary). Execution order: IMPLEMENT → TEST+UI (parallel) → QA. TransitionType: feature_addition.

---

## C0B-029-004 — PasswordResetTest: validate fires TEST → 5.3

A: "Validate the password reset confirmation modal closes on dismiss." Predicates: [TEST, UI]. How is TEST routed?
B: TEST → 5.3 specifically: "validate" fires TEST as a standalone assertion verb; "modal" fires UI. The modal context + standalone validate = E2E interaction test → 5.3. No unit test signals present.

---

## C0B-029-005 — PasswordResetTest: profile is gap — routing impact

A: "Profile the password reset form submission time." Predicates: [UI]. Route.
B: UI → 1.5 ("password reset form" → "form" fires UI → 1.5). TEST does not fire (profile is gap). Only one flow: UIInteractionFlow15. TransitionType: documentation. Performance context does not change the routing.
