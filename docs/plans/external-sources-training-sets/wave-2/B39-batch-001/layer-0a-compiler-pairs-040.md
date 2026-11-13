# Layer 0-A — Compiler Pairs (040)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-040-001 — BulkOperationTest: predicate compilation

A: "Write E2E test cases for the bulk operations page — assert the select-all checkbox activates, verify the bulk action dropdown appears."
B: TEST (write, assert, verify → one TEST). UI ("bulk operations page" → page; "select-all checkbox" → checkbox; "bulk action dropdown" → dropdown — three triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## C0A-040-002 — BulkOperationTest: progress toast fires UI

A: "Add test coverage to verify the progress toast appears during bulk delete." Predicates?
B: TEST (add test coverage, verify). UI (progress toast → toast). Result: [TEST, UI].

---

## C0A-040-003 — BulkOperationTest: confirmation modal fires UI

A: "Assert the bulk delete confirmation modal requires explicit user confirmation." Predicates?
B: TEST (assert). UI (bulk delete confirmation modal → modal). Result: [TEST, UI].

---

## C0A-040-004 — BulkOperationTest: "execute bulk operation" is gap

A: "Execute the bulk operation on all selected items." Predicates?
B: "execute" is gap. "selected items" has no UI vocabulary. Zero predicates → knowledge_edge_synthetic.

---

## C0A-040-005 — BulkOperationTest: QA gate before merge

A: "Write bulk operation E2E tests, assert the checkbox and dropdown, and QA gate before merge." Predicates?
B: TEST (write, assert). UI (checkbox, dropdown → two triggers → one UI). QA (QA gate → 7.3). [TEST, UI, QA]. AllSubPhaseIds: [5.3, 1.5, 7.3]. TransitionType: documentation.
