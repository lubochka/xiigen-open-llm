# Held-Out and Corruption Pairs (030)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H030-001 — Held-out: ProfileUpdateTest — save button + success toast

A: "Create E2E test cases for the profile update page — assert the save button activates after edits, verify the success toast renders."
  Expected: ["TEST", "UI"]
  Held-out: TEST ("create E2E test cases", "assert", "verify"). UI ("profile update page" → page, "save button" → button, "success toast" → toast — three triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## H030-002 — Held-out: ProfileUpdateTest — "update page" gap for IMPLEMENT

A: "Update the profile page to show the new avatar upload component."
  Expected: ["UI"]
  Held-out: "update" is gap for IMPLEMENT. UI fires: "profile page" → page, "avatar upload component" → component — two UI triggers → one UI. Result: [UI]. No TEST. No IMPLEMENT. TransitionType: documentation.

---

## H030-003 — Held-out: ProfileUpdateTest — "add component" IMPLEMENT

A: "Add an avatar upload component to the profile page and write E2E tests for it."
  Expected: ["IMPLEMENT", "TEST", "UI"]
  Held-out: IMPLEMENT ("add an avatar upload component" — "component" is product noun). TEST ("write E2E tests"). UI (page, component). Three predicates. TransitionType: feature_addition.

---

## C030-001 — Corruption v6-02: "update" → IMPLEMENT

Pattern: v6-02. Corrupt: "Update the profile page" → predicates = ["IMPLEMENT"].
Correct: "update" is gap. IMPLEMENT does not fire. Only UI fires (page). predicates = ["UI"].

---

## C030-002 — Corruption v6-03: UI routing collapsed into TEST routing

Pattern: v6-03. Corrupt: "Assert the save button — UI routed to 5.3."
Correct: UI always routes to 1.5. UI routing to 5.3 is a v6-03 corruption. TEST routes to 5.3; UI routes to 1.5; they are independent sub-phases.
