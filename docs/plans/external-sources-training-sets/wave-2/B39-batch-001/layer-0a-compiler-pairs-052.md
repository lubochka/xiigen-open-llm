# Layer 0-A — Compiler Pairs (052)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-052-001 — TimeToInteractiveTest: predicate compilation

A: "Write E2E test cases to assert TTI (Time to Interactive) is under 3.5 seconds on the product listing page."
B: TEST (write, assert). UI ("product listing page" → page). Result: [TEST, UI]. TransitionType: documentation.

---

## C0A-052-002 — TimeToInteractiveTest: loading skeleton fires UI

A: "Create test coverage to verify the loading skeleton disappears before TTI threshold." Predicates?
B: TEST (create test coverage, verify). UI ("loading skeleton" → skeleton). Result: [TEST, UI].

---

## C0A-052-003 — TimeToInteractiveTest: "measure TTI" is gap

A: "Measure TTI for the dashboard page." Predicates?
B: "measure" is gap. "dashboard page" — "dashboard" is not UI vocabulary, "page" is not present as a standalone word here. Conservative: []. But "dashboard page" has "page" implied — however as written "dashboard page" may parse as "dashboard" + "page". If "page" is present: [UI]. "dashboard page" → UI fires (page). Result: [UI].

---

## C0A-052-004 — TimeToInteractiveTest: "validate TTI" fires TEST

A: "Validate TTI is under budget for the registration form." Predicates?
B: TEST (validate). UI (registration form → form). Result: [TEST, UI].

---

## C0A-052-005 — TimeToInteractiveTest: button state during loading fires UI

A: "Assert the submit button is disabled while the page is loading (TTI not yet reached)." Predicates?
B: TEST (assert). UI ("submit button" → button; "page" → page — two UI triggers → one UI). Result: [TEST, UI].
