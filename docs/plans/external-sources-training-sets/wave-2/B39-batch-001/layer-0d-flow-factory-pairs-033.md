# Layer 0-D — Flow Factory Pairs (033)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F033-001 — CartFlowTest: flow selection

A: "Create E2E test cases for the cart page — assert item quantity controls update totals, verify the remove button deletes items." Predicates [TEST, UI]. Flows?
B: E2ETestingFlow53 (5.3): TEST. UIInteractionFlow15 (1.5): UI ("cart page" → page; "quantity controls" → not explicit UI lexicon, but "remove button" → button). Flows: [E2ETestingFlow53, UIInteractionFlow15]. TransitionType: documentation.

---

## F033-002 — CartFlowTest: "cart badge" fires UI

A: "Write test coverage to assert the cart badge updates when items are added." Flows?
B: TEST ("write test coverage", "assert"). UI ("cart badge" → badge). E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5). TransitionType: documentation.

---

## F033-003 — CartFlowTest: coupon code input fires UI

A: "Add E2E tests for the coupon code input on the cart page, verify the discount badge appears." Flows?
B: TEST ("add E2E tests", "verify"). UI ("coupon code input" → input; "cart page" → page; "discount badge" → badge — three triggers, one UI). E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5). TransitionType: documentation ("add E2E tests" = test-writing → TEST not IMPLEMENT).

---

## F033-004 — CartFlowTest: IMPLEMENT for cart feature build

A: "Implement the save-for-later feature and write E2E tests for the cart page." Full factory output?
B: IMPLEMENT ("implement ... feature"). TEST ("write E2E tests"). UI ("cart page" → page). Three flows: BackendServiceFlow41 (4.1) → E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5). TransitionType: feature_addition. AllSubPhaseIds: [4.1, 5.3, 1.5].

---

## F033-005 — CartFlowTest: Node 1 domains

A: Node 1 domain output for CartFlowTest.
B: SelectedLogicalTransition = E2ETestingFlow53. Domains: cart item list (quantity selector, remove button), cart badge count, price subtotal, coupon code input, discount badge, proceed-to-checkout button state, empty cart screen.
