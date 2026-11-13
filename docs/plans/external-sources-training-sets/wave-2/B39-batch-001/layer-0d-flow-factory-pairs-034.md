# Layer 0-D — Flow Factory Pairs (034)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F034-001 — WishlistFlowTest: flow selection

A: "Write E2E tests for the wishlist page — assert the add-to-wishlist toggle activates, verify the wishlist badge increments." Predicates [TEST, UI]. Flows?
B: E2ETestingFlow53 (5.3): TEST ("write E2E tests", "assert", "verify"). UIInteractionFlow15 (1.5): UI ("wishlist page" → page; "add-to-wishlist toggle" → toggle; "wishlist badge" → badge). Flows: [E2ETestingFlow53, UIInteractionFlow15]. TransitionType: documentation.

---

## F034-002 — WishlistFlowTest: "move to cart" button fires UI

A: "Assert the move-to-cart button on the wishlist page redirects to cart." Flows?
B: TEST (assert). UI ("move-to-cart button" → button; "wishlist page" → page — two triggers, one UI). E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5). TransitionType: documentation.

---

## F034-003 — WishlistFlowTest: gap — "monitor wishlist activity"

A: "Monitor wishlist activity for performance issues." Flows?
B: "monitor" is gap. "wishlist activity" contains no UI vocabulary. Result: [] → knowledge_edge_synthetic. No FlowInstances.

---

## F034-004 — WishlistFlowTest: [TEST, UI, QA] full factory

A: "Write wishlist E2E test coverage, assert the toggle fires, and QA acceptance before release cut." AllSubPhaseIds and flows?
B: AllSubPhaseIds: [5.3, 1.5, 7.1]. QA → ReleaseGateFlow71 (7.1) because "before release cut" = release vocabulary. Order: E2ETestingFlow53 + UIInteractionFlow15 parallel → ReleaseGateFlow71. TransitionType: documentation.

---

## F034-005 — WishlistFlowTest: Node 1 domains

A: Node 1 domain output for WishlistFlowTest.
B: SelectedLogicalTransition = E2ETestingFlow53. Domains: wishlist toggle (add/remove), wishlist page (item list), wishlist badge count, move-to-cart button, share wishlist link, empty wishlist screen.
