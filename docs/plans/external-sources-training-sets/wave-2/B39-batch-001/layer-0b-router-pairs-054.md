# Layer 0-B — Router Pairs (054)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-054-001 — LazyLoadingTest: routing

A: "Write E2E tests for lazy loading — assert product images load on scroll, verify skeleton loader." Predicates [TEST, UI]. Routing?
B: TEST → 5.3. UI → 1.5 (page, skeleton). AllSubPhaseIds: [5.3, 1.5].

---

## C0B-054-002 — LazyLoadingTest: "configure lazy loading" gap

A: "Configure lazy loading for images." Predicates []. Routing?
B: knowledge_edge_synthetic. No routing.

---

## C0B-054-003 — LazyLoadingTest: IMPLEMENT → 4.1

A: "Implement lazy loading for product component and write tests." AllSubPhaseIds?
B: [4.1, 5.3, 1.5]. TransitionType: feature_addition.

---

## C0B-054-004 — LazyLoadingTest: skeleton routes to 1.5

A: Does a "skeleton loader" in a lazy loading test route to 5.3 or 1.5?
B: 1.5. Skeleton is UI vocabulary. UI → 1.5 is absolute regardless of the test context (lazy loading, performance, general).

---

## C0B-054-005 — LazyLoadingTest: "configure IntersectionObserver" [UI] only

A: "Configure the IntersectionObserver for the product listing page." AllSubPhaseIds?
B: [1.5]. configure is gap. IntersectionObserver is gap. UI fires (product listing page → page) → 1.5. Single flow.
