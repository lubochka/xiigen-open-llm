# Layer 0-A — Compiler Pairs (030)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-030-001 — ProfileUpdateTest: predicate compilation

A: "Write E2E test specs for the profile update page — assert the avatar upload input accepts PNG files, verify the save button triggers the success toast."
B: Predicates: TEST ("write E2E test specs", "assert", "verify" → deduplicated); UI ("profile update page", "avatar upload input", "save button", "success toast" → deduplicated). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-030-002 — ProfileUpdateTest: IMPLEMENT present

A: "Build the profile settings form with an avatar upload component, write test coverage for valid and invalid file types."
B: Predicates: IMPLEMENT ("build"); TEST ("write test coverage" → TEST); UI ("profile settings form", "avatar upload component" → deduplicated). Result: [IMPLEMENT, TEST, UI]. TransitionType: feature_addition. Routing: [4.1, 5.3, 1.5].

---

## C0A-030-003 — ProfileUpdateTest: "review" is gap for QA

A: "Review the profile update form to ensure the bio field accepts 500 characters."
B: Predicates: UI ("profile update form" → "form" fires UI). QA does NOT fire — "review" is a gap verb (QA fires only on the literal abbreviation "QA"). TEST does NOT fire — "review" is gap. Result: [UI]. Routing: [1.5]. TransitionType: documentation.

---

## C0A-030-004 — ProfileUpdateTest: multi-UI-trigger deduplication

A: "Assert the profile page form renders the input, checkbox, toggle, and avatar component correctly."
B: Predicates: TEST ("assert" → TEST); UI ("profile page", "form", "input", "checkbox", "toggle", "component" → six UI triggers → one UI predicate, deduplicated). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-030-005 — ProfileUpdateTest: QA fires

A: "Write test cases for the profile update flow, verify the display name field saves correctly, and QA sign-off."
B: Predicates: TEST ("write", "verify" → deduplicated); UI ("display name field" → "field" does not fire UI; but "profile update" flow — check: "profile update page"/"screen" needed for UI). Note: "display name field" → "field" is not in the UI lexicon. Correct result: TEST only unless there is other UI vocabulary. Re-evaluation: no explicit UI component word. Result: [TEST, QA] (QA fires from "QA sign-off"). Routing: [5.3, 7.3]. TransitionType: documentation.
