# Layer 0-A — Compiler Pairs (037)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-037-001 — AdminPanelTest: predicate compilation

A: "Write E2E test cases for the admin panel page — assert the user management table renders, verify the action button is enabled for admins."
B: TEST ("write E2E test cases", "assert", "verify" → three triggers → one TEST). UI ("admin panel page" → page; "user management table" — "table" is not in the UI lexicon; "action button" → button → one UI trigger). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-037-002 — AdminPanelTest: "access control" is not UI vocabulary

A: "Validate the admin panel enforces access control." Does UI fire?
B: TEST (validate). "access control" is not UI vocabulary. "admin panel" — "panel" fires UI. Result: [TEST, UI]. UI fires on "panel", not "access control".

---

## C0A-037-003 — AdminPanelTest: sidebar fires UI

A: "Create test coverage for the admin panel sidebar navigation." What fires?
B: TEST ("create test coverage"). UI ("admin panel sidebar" → sidebar; "navigation" → navigation — two UI triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## C0A-037-004 — AdminPanelTest: IMPLEMENT fires on "build admin panel"

A: "Build the admin panel component and write E2E tests for it." Predicates?
B: IMPLEMENT ("build the admin panel component" — "component" is product noun). TEST ("write E2E tests"). UI ("admin panel component" → component). Three predicates: [IMPLEMENT, TEST, UI]. TransitionType: feature_addition. Routing: [4.1, 5.3, 1.5].

---

## C0A-037-005 — AdminPanelTest: "review access logs" is gap

A: "Review the admin panel access logs." Does any predicate fire?
B: UI fires on "admin panel" → "panel". TEST does not fire — "review" is gap. Result: [UI] only. Routing: [1.5]. TransitionType: documentation.
