# Layer 0-A — Compiler Pairs (055)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-055-001 — ServiceWorkerTest: predicate compilation

A: "Write E2E test cases for the service worker — assert the offline page renders when cache is available, verify the update toast appears when a new version is detected."
B: TEST (write, assert, verify). UI ("offline page" → page; "update toast" → toast — two triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## C0A-055-002 — ServiceWorkerTest: "register service worker" is gap

A: "Register the service worker for the application." Predicates?
B: "register" is not a recognized test-writing verb and not in the IMPLEMENT trigger set. "register" is gap. "application" has no UI vocabulary. Zero predicates → knowledge_edge_synthetic.

---

## C0A-055-003 — ServiceWorkerTest: "implement service worker" fires IMPLEMENT

A: "Implement the service worker for offline caching and write E2E tests for the offline page." Predicates?
B: IMPLEMENT ("implement the service worker"). TEST ("write E2E tests"). UI ("offline page" → page). [IMPLEMENT, TEST, UI]. TransitionType: feature_addition.

---

## C0A-055-004 — ServiceWorkerTest: cache update banner fires UI

A: "Assert the cache update banner appears when a new service worker version activates." Predicates?
B: TEST (assert). UI (cache update banner → banner). Result: [TEST, UI].

---

## C0A-055-005 — ServiceWorkerTest: "monitor service worker" is gap

A: "Monitor the service worker for cache invalidation errors." Predicates?
B: "monitor" is gap. "cache invalidation errors" has no UI vocabulary. Zero predicates → knowledge_edge_synthetic.
