# Layer 0-A — Compiler Pairs (047)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-047-001 — OfflineModeTest: predicate compilation

A: "Write E2E test cases for offline mode — assert the offline banner appears when network is disconnected, verify the sync screen shows pending changes."
B: TEST (write, assert, verify). UI ("offline banner" → banner; "sync screen" → screen — two triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## C0A-047-002 — OfflineModeTest: "offline" alone is not UI vocabulary

A: "Test offline behavior." Predicates?
B: "test" as imperative verb without test noun = borderline/gap. "offline behavior" has no UI vocabulary. Conservative result: [] → knowledge_edge_synthetic.

---

## C0A-047-003 — OfflineModeTest: toast fires UI for sync status

A: "Assert the sync complete toast fires after reconnection." Predicates?
B: TEST (assert). UI (sync complete toast → toast). Result: [TEST, UI].

---

## C0A-047-004 — OfflineModeTest: banner fires UI

A: "Create test coverage to verify the offline banner is visible during network disconnection." Predicates?
B: TEST (create test coverage, verify). UI (offline banner → banner). Result: [TEST, UI]. "banner" is in the UI lexicon.

---

## C0A-047-005 — OfflineModeTest: [TEST, UI, QA] before staging

A: "Write offline mode E2E tests, assert the offline banner, and QA before staging deployment." Predicates?
B: TEST (write, assert). UI (banner). QA (QA → 7.1 "before staging deployment"). [TEST, UI, QA]. AllSubPhaseIds: [5.3, 1.5, 7.1]. TransitionType: documentation.
