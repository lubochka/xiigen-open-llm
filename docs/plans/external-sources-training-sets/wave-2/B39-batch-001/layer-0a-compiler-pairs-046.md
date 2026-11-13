# Layer 0-A — Compiler Pairs (046)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-046-001 — ConcurrentUserTest: predicate compilation

A: "Write E2E test scenarios that verify the concurrent user indicator updates on the collaboration page when multiple users are active."
B: TEST (write, verify). UI ("concurrent user indicator" — "indicator" is not in the UI lexicon; "collaboration page" → page → one UI trigger). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-046-002 — ConcurrentUserTest: badge fires UI for user count

A: "Assert the online users badge shows the correct count during concurrent sessions." Predicates?
B: TEST (assert). UI ("online users badge" → badge). Result: [TEST, UI].

---

## C0A-046-003 — ConcurrentUserTest: "run concurrent tests" is gap

A: "Run concurrent user tests for the checkout flow." Predicates?
B: "run" is gap. "checkout flow" has no UI vocabulary. Zero predicates → knowledge_edge_synthetic.

---

## C0A-046-004 — ConcurrentUserTest: conflict modal fires UI

A: "Create test coverage to verify the edit conflict modal appears when two users edit the same record." Predicates?
B: TEST (create test coverage, verify). UI (edit conflict modal → modal). Result: [TEST, UI].

---

## C0A-046-005 — ConcurrentUserTest: "scan concurrent sessions" is gap

A: "Scan concurrent sessions for race conditions." Predicates?
B: "scan" is gap. "concurrent sessions", "race conditions" have no UI vocabulary. Zero predicates → knowledge_edge_synthetic.
