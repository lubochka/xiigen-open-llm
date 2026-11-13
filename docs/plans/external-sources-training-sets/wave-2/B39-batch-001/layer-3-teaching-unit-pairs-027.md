# Layer 3 — Teaching Unit Pairs (027)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU027-001 — UserRegistrationTest: "add tests" vs "add component" disambiguation

A: What is the registration-specific lesson about the verb "add"?
B: "Add E2E tests for the registration form" → TEST (add + test noun). "Add an avatar upload component" → IMPLEMENT (add + component noun). The verb "add" is context-sensitive. Registering this pattern: always check the direct object noun to resolve "add" to TEST or IMPLEMENT.

---

## TU027-002 — UserRegistrationTest: duplicate email error modal

A: Teach why "assert the duplicate email error modal" fires both TEST and UI.
B: "assert" is a standalone test assertion verb → TEST. "error modal" → "modal" is in the UI lexicon → UI. Both fire independently from different vocabulary categories. This is the canonical TEST+UI co-fire pattern: assertion verb (TEST trigger) + UI component noun (UI trigger).

---

## TU027-003 — UserRegistrationTest: "check registration works" → gap

A: "Check that user registration works." Why does this produce zero predicates?
B: "check" is an explicitly listed gap verb for TEST. "user registration" is a domain descriptor — not a predicate trigger. "works" is not a vocabulary trigger. Zero predicates → knowledge_edge_synthetic.

---

## TU027-004 — UserRegistrationTest: sign-up screen fires UI

A: Does "sign-up screen" fire UI?
B: Yes. "screen" is in the UI lexicon. The compound "sign-up screen" fires UI via the "screen" token. Similarly: "registration screen", "confirmation screen", "welcome screen" all fire UI.

---

## TU027-005 — UserRegistrationTest: submit button disabled state

A: "Write test scenarios asserting the submit button is disabled until required fields are filled." Predicate lesson?
B: Four predicate triggers: "write test scenarios" (TEST), "asserting" (TEST), "submit button" (UI), "required fields" — "required fields" is not in the UI lexicon, but "submit button" is. Two TEST triggers → one TEST. One UI trigger → one UI. Result: [TEST, UI]. TransitionType: documentation.
