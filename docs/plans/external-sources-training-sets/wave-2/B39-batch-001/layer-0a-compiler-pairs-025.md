# Layer 0-A — Compiler Pairs (025)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-025-001 — CheckoutFlowTest: predicate compilation

A: "Write E2E test cases for the checkout flow — assert the payment button is enabled after cart items are added, verify the order summary modal displays correct totals."
B: Predicates: TEST ("write E2E test cases", "assert", "verify" → deduplicated to one TEST); UI ("payment button", "cart", "order summary modal" → deduplicated to one UI). No IMPLEMENT (no build/implement verb). No QA. Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-025-002 — CheckoutFlowTest: gap verb near test context

A: "Run the checkout E2E tests and check that the payment button is active."
B: Predicates: UI only ("payment button" → UI). TEST does NOT fire — "run" and "check" are gap verbs. "E2E tests" with only gap verbs does not fire TEST. Result: [UI]. Routing: [1.5]. TransitionType: documentation.

---

## C0A-025-003 — CheckoutFlowTest: IMPLEMENT co-fires

A: "Build the checkout page, add E2E test coverage for the payment flow, and assert the success screen renders."
B: Predicates: IMPLEMENT ("build", "add" [feature noun: checkout page]); TEST ("add E2E test coverage", "assert" → deduplicated); UI ("checkout page", "payment flow", "success screen" → deduplicated). Result: [IMPLEMENT, TEST, UI]. TransitionType: feature_addition (IMPLEMENT present). Routing: [4.1, 5.3, 1.5].

---

## C0A-025-004 — CheckoutFlowTest: QA fires

A: "Write regression tests for the checkout flow, verify the cart total, and QA sign-off before the release."
B: Predicates: TEST ("write regression tests", "verify" → deduplicated); UI ("cart" → UI); QA ("QA sign-off" → QA). Result: [TEST, UI, QA]. Routing: [5.3, 1.5, 7.3]. TransitionType: documentation.

---

## C0A-025-005 — CheckoutFlowTest: "automate" is gap

A: "Automate the checkout E2E flow with Cypress to validate the payment form."
B: Predicates: UI only ("payment form" → UI). "Automate" is gap — does not fire TEST. "Cypress" is gap (framework name). "validate" fires TEST only when used as a standalone assertion verb or test-writing verb — here "validate the payment form" reads as execution-context; however "validate" IS a TEST trigger when it appears as a test-writing assertion. Re-evaluation: "validate the payment form" → TEST fires (validate is a test-writing verb). But "automate" and "Cypress" remain gap. Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.
