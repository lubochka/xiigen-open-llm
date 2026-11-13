# Layer 0-B — Router Pairs (030)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-030-001 — ProfileUpdateTest: routing decision

A: "Write E2E test specs for the profile update page — assert the avatar upload input, verify the save button triggers the success toast." Predicates: [TEST, UI]. Route.
B: TEST → 5.3 (E2ETestingFlow53): profile update is multi-step UI interaction. UI → 1.5 (UIInteractionFlow15): "profile update page", "avatar upload input", "save button", "success toast" → "page", "input", "button", "toast" all fire UI → 1.5. TransitionType: documentation.

---

## C0B-030-002 — ProfileUpdateTest: IMPLEMENT routing

A: "Build the profile settings form with an avatar upload component, write test coverage." Predicates: [IMPLEMENT, TEST, UI]. Route.
B: IMPLEMENT → 4.1. TEST → 5.3. UI → 1.5 ("profile settings form", "avatar upload component" → "form", "component" fire UI → 1.5). TransitionType: feature_addition. Execution: IMPLEMENT → TEST+UI (parallel).

---

## C0B-030-003 — ProfileUpdateTest: review is gap

A: "Review the profile update form." Predicates: [UI]. Route.
B: UI → 1.5 ("form" fires UI → 1.5). QA does not fire (review is gap — "review" without "QA" abbreviation is gap). TEST does not fire. Single sub-phase: 1.5.

---

## C0B-030-004 — ProfileUpdateTest: assert fires TEST — deduplication routing

A: "Assert the profile page form renders the input, checkbox, toggle, and avatar component." Predicates: [TEST, UI]. Route — note the multi-UI-trigger.
B: TEST → 5.3 (one TEST predicate from "assert"). UI → 1.5 (six UI vocabulary triggers: "page", "form", "input", "checkbox", "toggle", "component" → all deduplicated to one UI predicate → one routing decision: 1.5). Two sub-phases total: [5.3, 1.5].

---

## C0B-030-005 — ProfileUpdateTest: TEST only (no UI vocabulary)

A: "Write test cases for the profile update flow, verify the display name field saves correctly." Predicates: [TEST, QA]. (No UI vocabulary found.) Route.
B: TEST → 5.3 (profile update is UI-adjacent but no explicit UI component word was present in this request; TEST still routes to 5.3 by E2E context). QA → 7.3 (default, no release vocabulary). No UI sub-phase. AllSubPhaseIds: [5.3, 7.3]. TransitionType: documentation.
