# Layer 0-B — Router Pairs (034)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-034-001 — WishlistFlowTest: routing decision

A: "Write E2E test scenarios for the wishlist flow — assert the toggle button saves items, verify the wishlist screen." Predicates: [TEST, UI]. Route.
B: TEST → 5.3. UI → 1.5 ("heart toggle button", "wishlist screen" → "toggle", "button", "screen" fire UI → 1.5). TransitionType: documentation.

---

## C0B-034-002 — WishlistFlowTest: ARIA vocabulary routing

A: "Create test coverage for wishlist screen accessibility — assert the toggle button has aria-label, verify focus after modal dismissal." Predicates: [TEST, UI]. Route.
B: TEST → 5.3. UI → 1.5 ("wishlist screen", "toggle button", "aria-label", "modal" → all fire UI, deduplicated → 1.5). Accessibility context reinforces 5.3 for TEST.

---

## C0B-034-003 — WishlistFlowTest: scan gap — UI only

A: "Scan the wishlist page for broken links." Predicates: [UI]. Route.
B: UI → 1.5 ("wishlist page" → "page" fires UI → 1.5). TEST does not fire (scan is gap). Single sub-phase.

---

## C0B-034-004 — WishlistFlowTest: IMPLEMENT routing

A: "Build the wishlist feature with a save-for-later button, add test cases for toggling state." Predicates: [IMPLEMENT, TEST, UI]. Route.
B: IMPLEMENT → 4.1 ("build"). TEST → 5.3 ("add test cases" → TEST via add + test noun). UI → 1.5 ("save-for-later button" → "button" fires UI → 1.5). TransitionType: feature_addition.

---

## C0B-034-005 — WishlistFlowTest: QA routing

A: "Write wishlist flow E2E specs, assert the badge count, and QA acceptance." Predicates: [TEST, UI, QA]. Route.
B: TEST → 5.3. UI → 1.5 ("badge count" → "badge" fires UI → 1.5). QA → 7.3 (default, "QA acceptance" is feature-gate vocabulary). AllSubPhaseIds: [5.3, 1.5, 7.3].
