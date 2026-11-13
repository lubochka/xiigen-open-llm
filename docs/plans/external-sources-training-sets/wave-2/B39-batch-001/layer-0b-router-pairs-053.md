# Layer 0-B — Router Pairs (053)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-053-001 — BundleSizeTest: routing

A: "Write E2E tests for bundle size budget — verify main bundle under 250KB on home page." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (home page → page). AllSubPhaseIds: [5.3, 1.5].

---

## C0B-053-002 — BundleSizeTest: "scan bundle" gap

A: "Scan the bundle for unused dependencies." Predicates []. Routing?
B: knowledge_edge_synthetic. No routing.

---

## C0B-053-003 — BundleSizeTest: "validate bundle size" → [TEST] only

A: "Validate the chunk bundle size." Predicates [TEST]. AllSubPhaseIds?
B: [5.3]. TEST only (validate). No UI vocabulary in this request. Single flow: E2ETestingFlow53 (5.3).

---

## C0B-053-004 — BundleSizeTest: lazy-loaded component → 1.5

A: "Assert the lazy-loaded product component is not in initial bundle." AllSubPhaseIds?
B: [5.3, 1.5]. Component → 1.5. TEST (assert) → 5.3.

---

## C0B-053-005 — BundleSizeTest: [TEST, UI, QA → 7.1]

A: "Write bundle size tests, assert main page load, and QA before release." AllSubPhaseIds?
B: [5.3, 1.5, 7.1]. QA → 7.1 (before release).
