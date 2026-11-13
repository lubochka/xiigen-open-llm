# Layer 0-A — Compiler Pairs (034)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-034-001 — WishlistFlowTest: predicate compilation

A: "Write E2E test scenarios for the wishlist flow — assert the heart toggle button saves items, verify the wishlist screen displays saved products."
B: Predicates: TEST ("write E2E test scenarios", "assert", "verify" → deduplicated); UI ("heart toggle button", "wishlist screen" → "toggle", "button", "screen" fire UI, deduplicated). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-034-002 — WishlistFlowTest: accessibility assertion

A: "Create test coverage for wishlist screen accessibility — assert the toggle button has aria-label 'Add to wishlist', verify focus returns to the button after modal dismissal."
B: Predicates: TEST ("create test coverage", "assert", "verify" → deduplicated); UI ("wishlist screen", "toggle button", "aria-label", "modal" → "screen", "toggle", "button", "aria", "modal" fire UI, deduplicated). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-034-003 — WishlistFlowTest: "scan" is gap

A: "Scan the wishlist page for broken links."
B: Predicates: UI ("wishlist page" → "page" fires UI). TEST does NOT fire — "scan" is gap. Result: [UI]. Routing: [1.5]. TransitionType: documentation.

---

## C0A-034-004 — WishlistFlowTest: IMPLEMENT co-fires

A: "Build the wishlist feature with a save-for-later button, add test cases for toggling wishlist state and verifying persistence."
B: Predicates: IMPLEMENT ("build"); TEST ("add test cases", "verify" → deduplicated); UI ("save-for-later button" → "button" fires UI). Result: [IMPLEMENT, TEST, UI]. TransitionType: feature_addition. Routing: [4.1, 5.3, 1.5].

---

## C0A-034-005 — WishlistFlowTest: QA co-fires

A: "Write wishlist flow E2E test specs, assert the badge count updates, and QA acceptance."
B: Predicates: TEST ("write", "assert" → deduplicated); UI ("badge count" → "badge" fires UI); QA ("QA acceptance" → QA). Result: [TEST, UI, QA]. Routing: [5.3, 1.5, 7.3]. TransitionType: documentation.
