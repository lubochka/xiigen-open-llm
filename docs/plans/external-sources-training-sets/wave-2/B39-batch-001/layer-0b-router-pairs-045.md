# Layer 0-B — Router Pairs (045)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-045-001 — SessionTimeoutTest: routing

A: "Write E2E tests for the session timeout screen — assert timeout modal and verify re-login button." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (screen, modal, button). AllSubPhaseIds: [5.3, 1.5]. TransitionType: documentation.

---

## C0B-045-002 — SessionTimeoutTest: "configure timeout" gap

A: "Configure the session timeout duration." Predicates []. Routing?
B: Zero predicates → knowledge_edge_synthetic. No routing.

---

## C0B-045-003 — SessionTimeoutTest: expiry toast → 1.5

A: Does a "session expiry toast" route to 1.5?
B: Yes. Toast → 1.5 always.

---

## C0B-045-004 — SessionTimeoutTest: "monitor sessions" gap

A: "Monitor active sessions." Routing?
B: Zero predicates → knowledge_edge_synthetic. No routing.

---

## C0B-045-005 — SessionTimeoutTest: [TEST, UI, QA → 7.3] before merge

A: "Write session timeout tests, assert the modal, and QA acceptance before merge." AllSubPhaseIds?
B: [5.3, 1.5, 7.3]. QA → 7.3 (before merge = acceptance).
