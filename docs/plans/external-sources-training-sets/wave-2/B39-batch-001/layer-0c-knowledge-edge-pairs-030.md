# Layer 0-C — Knowledge Edge Pairs (030)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E030-001 — ProfileUpdateTest: gap vocabulary

A: Which words are gap in profile update test requests?
B: Gap: "update the profile page" ("update" is gap for IMPLEMENT), "review the profile settings" ("review" is gap), "check the profile form" ("check" is gap), "configure the profile upload" ("configure" is gap), "describe the profile page behavior" ("describe" is gap). "profile" alone is not a trigger.

---

## E030-002 — ProfileUpdateTest: "update" is gap for IMPLEMENT

A: "Update the profile page layout to show a new sidebar." Does IMPLEMENT fire?
B: No. "update" is gap. UI fires on "profile page" → page, "sidebar" → sidebar. Result: [UI]. No IMPLEMENT. TransitionType: documentation.

---

## E030-003 — ProfileUpdateTest: avatar upload component fires IMPLEMENT

A: "Add an avatar upload component to the profile page." Does IMPLEMENT fire?
B: Yes. "add" + "component" (feature noun) → IMPLEMENT. "profile page" → UI (page). "component" also fires UI. Result: [IMPLEMENT, UI]. No TEST (no test-writing verb). TransitionType: feature_addition.

---

## E030-004 — ProfileUpdateTest: save button assertion

A: "Assert the save button activates after form edits." Does TEST fire?
B: Yes. "assert" is a standalone assertion verb → TEST fires. "save button" → button → UI fires. Result: [TEST, UI].

---

## E030-005 — ProfileUpdateTest: unsaved-changes modal

A: "Write tests that assert the unsaved-changes modal appears." Predicate edge analysis?
B: "write tests" → TEST (test-writing verb + test noun). "assert" → TEST (second trigger, deduplicated). "unsaved-changes modal" → modal → UI. Result: [TEST, UI]. The modal is the UI trigger; the transient state description ("unsaved-changes") is not relevant to predicate firing.
