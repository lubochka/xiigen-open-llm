# Held-Out and Corruption Pairs (025)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H025-001 — Held-out: CheckoutFlowTest — multi-trigger deduplication

A: "Write E2E test coverage for the checkout flow — assert the payment button state, verify the order summary modal displays correct totals, validate the cart items list."
  Expected: ["TEST", "UI"]
  Held-out: "write E2E test coverage" + "assert" + "verify" + "validate" → four TEST triggers → one TEST predicate. "payment button" + "order summary modal" + "cart items list" — "button" + "modal" → two UI triggers → one UI predicate ("cart items list" has no UI lexicon word). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## H025-002 — Held-out: CheckoutFlowTest — "run the tests" = gap

A: "Run the checkout E2E tests to verify the payment button works."
  Expected: ["UI"]
  Held-out: "run" is gap → TEST does not fire. "verify" fires TEST — wait: "verify the payment button works" → TEST (verify is a standalone assertion verb). "payment button" → UI (button). Result: [TEST, UI]. Correction: even with "run" gap, "verify" fires TEST. Result: [TEST, UI].

---

## H025-003 — Held-out: CheckoutFlowTest — IMPLEMENT via "build"

A: "Build the checkout page and write E2E test coverage for the payment form."
  Expected: ["IMPLEMENT", "TEST", "UI"]
  Held-out: "build the checkout page" → IMPLEMENT. "write E2E test coverage" → TEST. "payment form" → form → UI. Three predicates. TransitionType: feature_addition. AllSubPhaseIds: [4.1, 5.3, 1.5].

---

## C025-001 — Corruption v6-01: invented CHECKOUT_FLOW predicate

Pattern: v6-01. Corrupt output invents a CHECKOUT_FLOW predicate from the domain name.
Corrupt: predicates = ["TEST", "UI", "CHECKOUT_FLOW"].
Correct: predicates = ["TEST", "UI"]. "CHECKOUT_FLOW" is a domain descriptor, not a predicate. Only seven valid predicates: TEST, UI, QA, IMPLEMENT, DOCUMENT, ARCHITECTURE, REFACTOR.

---

## C025-002 — Corruption v6-02: "run" → TEST

Pattern: v6-02. Corrupt output fires TEST from the gap verb "run".
Corrupt: "Run the checkout tests" → predicates = ["TEST"].
Correct: predicates = []. "run" is gap. TEST does not fire without a test-writing verb or standalone assertion verb.
