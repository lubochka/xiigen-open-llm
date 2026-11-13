# Layer 0-A — Compiler Pairs (045)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-045-001 — SessionTimeoutTest: predicate compilation

A: "Write E2E test cases for the session timeout screen — assert the timeout modal appears after inactivity, verify the re-login button is active."
B: TEST (write, assert, verify). UI ("session timeout screen" → screen; "timeout modal" → modal; "re-login button" → button — three triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## C0A-045-002 — SessionTimeoutTest: "configure timeout" is gap

A: "Configure the session timeout duration to 30 minutes." Predicates?
B: "configure" is gap. "session timeout duration" has no UI vocabulary. Zero predicates → knowledge_edge_synthetic.

---

## C0A-045-003 — SessionTimeoutTest: toast fires UI

A: "Add test coverage to verify the session expiry toast appears before timeout." Predicates?
B: TEST (add test coverage, verify). UI (session expiry toast → toast). Result: [TEST, UI].

---

## C0A-045-004 — SessionTimeoutTest: "monitor session" is gap

A: "Monitor active sessions for timeout violations." Predicates?
B: "monitor" is gap. "active sessions" has no UI vocabulary. Zero predicates → knowledge_edge_synthetic.

---

## C0A-045-005 — SessionTimeoutTest: [TEST, UI, QA] before merge

A: "Write session timeout E2E tests, assert the timeout modal, and QA acceptance before merge." Predicates?
B: TEST (write, assert). UI (timeout modal → modal). QA (QA acceptance → 7.3). [TEST, UI, QA]. AllSubPhaseIds: [5.3, 1.5, 7.3]. TransitionType: documentation.
