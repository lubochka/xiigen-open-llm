# Layer 0-A — Compiler Pairs (026)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: compiler

---

## C0A-026-001 — PaymentFlowTest: predicate compilation

A: "Write test cases for the payment flow — assert the credit card form validates required fields, verify the confirmation screen shows the order ID."
B: Predicates: TEST ("write test cases", "assert", "verify" → deduplicated); UI ("credit card form", "confirmation screen" → deduplicated). No IMPLEMENT, no QA. Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## C0A-026-002 — PaymentFlowTest: IMPLEMENT fires on building

A: "Implement the payment form component, write E2E scenarios for failed payment states, and assert the error toast appears."
B: Predicates: IMPLEMENT ("implement"); TEST ("write E2E scenarios", "assert" → deduplicated); UI ("payment form component", "error toast" → deduplicated). Result: [IMPLEMENT, TEST, UI]. TransitionType: feature_addition. Routing: [4.1, 5.3, 1.5].

---

## C0A-026-003 — PaymentFlowTest: QA with release vocabulary → 7.1

A: "Write payment flow E2E tests, verify the card input form, and QA the release before staging deployment."
B: Predicates: TEST ("write", "verify" → deduplicated); UI ("card input form" → UI); QA ("QA the release" + staging/deployment vocabulary → QA routes to 7.1, not 7.3). Result: [TEST, UI, QA]. Routing: [5.3, 1.5, 7.1]. TransitionType: documentation.

---

## C0A-026-004 — PaymentFlowTest: framework name is gap

A: "Set up Playwright scripts to test the payment gateway form submission."
B: Predicates: UI ("payment gateway form" → "form" fires UI). TEST does not fire — "set up" is gap, "Playwright" is gap. No test-writing verb present. Result: [UI]. Routing: [1.5]. TransitionType: documentation.

---

## C0A-026-005 — PaymentFlowTest: "verify" as standalone TEST trigger

A: "Verify the payment confirmation modal renders the correct billing summary."
B: Predicates: TEST ("verify" — standalone assertion verb fires TEST); UI ("payment confirmation modal", "billing summary" → "modal" fires UI). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.
