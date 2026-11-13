# Layer 3 — Teaching Unit Pairs (030)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU030-001 — ProfileUpdateTest: "update" is gap for IMPLEMENT

A: What does ProfileUpdateTest teach about the verb "update"?
B: "update" is gap — it does not fire IMPLEMENT. IMPLEMENT fires on: build, implement, create, add [feature noun]. "Update the profile page layout" → UI fires (page) but IMPLEMENT does not. This is a common error in v6-02 corruptions: treating "update" as IMPLEMENT.

---

## TU030-002 — ProfileUpdateTest: save button fires UI

A: "Assert the save button activates after edits." What fires?
B: TEST (assert) + UI (save button → button). Result: [TEST, UI]. The save button is explicit UI lexicon (button category).

---

## TU030-003 — ProfileUpdateTest: success toast fires UI

A: "Verify the success toast fires after profile save." What fires?
B: TEST (verify) + UI (success toast → toast). Result: [TEST, UI]. TransitionType: documentation.

---

## TU030-004 — ProfileUpdateTest: "add an avatar upload component" fires IMPLEMENT

A: "Add an avatar upload component to the profile page." Does IMPLEMENT fire?
B: Yes. "add an avatar upload component" → IMPLEMENT. Direct object "component" is a feature/product noun. "component" in this context (product noun) triggers IMPLEMENT, not TEST. Contrast: "add test coverage for the avatar upload component" → TEST ("add test coverage" = test-writing).

---

## TU030-005 — ProfileUpdateTest: unsaved-changes modal → documentation

A: "Write tests that assert the unsaved-changes confirmation modal appears." TransitionType?
B: documentation. TEST (write tests, assert) + UI (confirmation modal → modal). No IMPLEMENT. TransitionType: documentation.
