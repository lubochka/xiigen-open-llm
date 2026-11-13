# Layer 0-B — Router Pairs (046)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-046-001 — ConcurrentUserTest: routing

A: "Write E2E tests for the collaboration page — assert the online users badge." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (page, badge). AllSubPhaseIds: [5.3, 1.5]. TransitionType: documentation.

---

## C0B-046-002 — ConcurrentUserTest: "run concurrent tests" gap

A: "Run concurrent user tests." Predicates []. Routing?
B: Zero predicates → knowledge_edge_synthetic. No routing.

---

## C0B-046-003 — ConcurrentUserTest: conflict modal → 1.5

A: Does an "edit conflict modal" route to 1.5?
B: Yes. Modal → 1.5 always.

---

## C0B-046-004 — ConcurrentUserTest: "scan concurrent sessions" gap

A: "Scan concurrent sessions." Predicates []. Routing?
B: Zero predicates → knowledge_edge_synthetic.

---

## C0B-046-005 — ConcurrentUserTest: badge routing invariant

A: Does the online/concurrent context change where badge routes?
B: No. Badge always routes to 1.5 (UIInteractionFlow15). Context (concurrent users, order tracking, notifications) is irrelevant to routing. Badge = 1.5, absolutely.
