# Layer 0-B — Router Pairs (040)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-040-001 — BulkOperationTest: routing [TEST, UI]

A: "Write E2E test cases for the bulk operations page — assert the select-all checkbox and dropdown." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (page, checkbox, dropdown — three triggers → one UI predicate → one routing decision to 1.5). AllSubPhaseIds: [5.3, 1.5]. TransitionType: documentation.

---

## C0B-040-002 — BulkOperationTest: progress toast → 1.5

A: Does a "progress toast" route to 1.5?
B: Yes. Toast is UI vocabulary → 1.5. All toast variants route to UIInteractionFlow15 (1.5).

---

## C0B-040-003 — BulkOperationTest: confirmation modal → 1.5

A: Does a "bulk delete confirmation modal" route to 1.5?
B: Yes. Modal is UI vocabulary → 1.5. UIInteractionFlow15 handles all modal interactions.

---

## C0B-040-004 — BulkOperationTest: "execute bulk operation" gap — no routing

A: "Execute the bulk operation." Predicates []. Routing?
B: Zero predicates → knowledge_edge_synthetic. No routing.

---

## C0B-040-005 — BulkOperationTest: [TEST, UI, QA → 7.3]

A: "Write bulk operation tests, assert checkbox and dropdown, and QA gate before merge." AllSubPhaseIds?
B: AllSubPhaseIds: [5.3, 1.5, 7.3]. QA → 7.3 (before merge = acceptance). TransitionType: documentation.
