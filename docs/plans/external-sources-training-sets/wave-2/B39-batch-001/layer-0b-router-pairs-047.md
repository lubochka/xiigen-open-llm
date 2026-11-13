# Layer 0-B — Router Pairs (047)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-047-001 — OfflineModeTest: routing

A: "Write E2E tests for offline mode — assert offline banner and sync screen." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (banner, screen). AllSubPhaseIds: [5.3, 1.5]. TransitionType: documentation.

---

## C0B-047-002 — OfflineModeTest: banner routes to 1.5

A: Does an "offline banner" route to 1.5?
B: Yes. Banner → 1.5. UIInteractionFlow15 handles all banner interactions.

---

## C0B-047-003 — OfflineModeTest: sync complete toast → 1.5

A: Does a "sync complete toast" route to 1.5?
B: Yes. Toast → 1.5 always.

---

## C0B-047-004 — OfflineModeTest: [TEST, UI, QA → 7.1] before staging

A: "Write offline mode tests, assert banner, and QA before staging deployment." AllSubPhaseIds?
B: [5.3, 1.5, 7.1]. QA → 7.1 (before staging deployment). Order: 5.3 + 1.5 → 7.1.

---

## C0B-047-005 — OfflineModeTest: "offline" alone is gap — routing impact

A: "Test offline behavior." Predicates []. Routing?
B: Zero predicates (conservative: "test" as imperative verb without test noun = gap). → knowledge_edge_synthetic. No routing.
