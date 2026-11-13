# Layer 0-A — Compiler Pairs (054)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-054-001 — LazyLoadingTest: predicate compilation

A: "Write E2E test cases for lazy loading — assert product images load as the user scrolls the product listing page, verify the skeleton loader appears before images load."
B: TEST (write, assert, verify). UI ("product listing page" → page; "skeleton loader" → skeleton — two triggers → one UI). Result: [TEST, UI]. TransitionType: documentation.

---

## C0A-054-002 — LazyLoadingTest: "configure lazy loading" is gap

A: "Configure lazy loading for all product images." Predicates?
B: "configure" is gap. No UI vocabulary. Zero predicates → knowledge_edge_synthetic.

---

## C0A-054-003 — LazyLoadingTest: skeleton fires UI

A: "Assert the skeleton placeholder renders before lazy-loaded content." Predicates?
B: TEST (assert). UI (skeleton placeholder → skeleton). Result: [TEST, UI].

---

## C0A-054-004 — LazyLoadingTest: "implement lazy loading" fires IMPLEMENT

A: "Implement lazy loading for the product component and write E2E tests for it." Predicates?
B: IMPLEMENT ("implement lazy loading"). TEST ("write E2E tests"). UI ("product component" → component). [IMPLEMENT, TEST, UI]. TransitionType: feature_addition.

---

## C0A-054-005 — LazyLoadingTest: intersection observer is gap

A: "Configure the IntersectionObserver for the product listing page." Predicates?
B: "configure" is gap. "IntersectionObserver" is a browser API — gap. "product listing page" → page → UI fires. Result: [UI]. TEST does not fire.
