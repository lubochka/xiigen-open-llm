# Layer 0-D — Flow Factory Pairs (030)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F030-001 — ProfileUpdateTest: flow selection

A: "Create E2E test cases for the profile update page — assert the save button activates after edits, verify the success toast." Predicates [TEST, UI]. Flows?
B: E2ETestingFlow53 (5.3): TEST ("create E2E test cases", "assert", "verify"). UIInteractionFlow15 (1.5): UI ("profile update page" → page; "save button" → button; "success toast" → toast). Flows: [E2ETestingFlow53, UIInteractionFlow15]. TransitionType: documentation.

---

## F030-002 — ProfileUpdateTest: "update" is gap for IMPLEMENT

A: "Update the profile page layout." Does IMPLEMENT fire?
B: No. "update" is a gap verb for IMPLEMENT. IMPLEMENT fires on: build, create, add [feature noun], implement. "Update" used as a change/modification verb is gap. "profile page" → page fires UI. Result: [UI]. Single flow: UIInteractionFlow15 (1.5). TransitionType: documentation.

---

## F030-003 — ProfileUpdateTest: avatar upload modal fires UI

A: "Write test scenarios for the profile avatar upload modal." Full factory output?
B: TEST ("write test scenarios"). UI ("avatar upload modal" → modal). E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5). TransitionType: documentation. TrustState: deterministic_generic.

---

## F030-004 — ProfileUpdateTest: IMPLEMENT fires on "add avatar upload"

A: "Add an avatar upload component to the profile page, write E2E tests for it." Flows?
B: IMPLEMENT fires on "add an avatar upload component" (add + component noun → IMPLEMENT). TEST fires on "write E2E tests". UI fires on "avatar upload component" + "profile page" → component, page. Three flows: BackendServiceFlow41 (4.1) → E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5). TransitionType: feature_addition.

---

## F030-005 — ProfileUpdateTest: Node 1 domains

A: Node 1 output for ProfileUpdateTest.
B: SelectedLogicalTransition = E2ETestingFlow53. Domains: profile form (name, email, bio, avatar), save button state, validation errors (invalid email, required fields), success toast on save, avatar upload interaction, unsaved-changes confirmation modal.
