# Layer 0-C — Knowledge Edge Pairs (026)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## E026-001 — PaymentFlowTest: gap vocabulary

A: Which words in a payment test request are always gap?
B: Gap vocabulary in payment context: "process payment" ("process" is gap), "handle the payment" ("handle" is gap), "set up Playwright for payment tests" ("set up" is gap, "Playwright" is gap), "run payment tests" ("run" is gap), "configure the payment gateway" ("configure" is gap for IMPLEMENT), "check the payment status" ("check" is gap). "payment" alone — descriptor, not a predicate trigger.

---

## E026-002 — PaymentFlowTest: "assert" + form

A: "Assert the payment confirmation modal closes on submission." What fires?
B: TEST ("assert" → standalone assertion verb → TEST fires); UI ("payment confirmation modal" → "modal" fires UI). Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## E026-003 — PaymentFlowTest: "set up" is gap for IMPLEMENT

A: "Set up the payment form component." Does IMPLEMENT fire?
B: No. "Set up" is gap for IMPLEMENT. IMPLEMENT fires on: build, create, write, implement, add [feature noun], develop, generate, scaffold, extend, code. "Set up" is gap. UI fires on "payment form component" → "form", "component" fire UI. Result: [UI]. No IMPLEMENT.

---

## E026-004 — PaymentFlowTest: credit card form — multiple UI triggers

A: "Assert the credit card form renders the card number input, expiry select, and CVV toggle."
B: TEST ("assert" → TEST); UI ("credit card form" + "card number input" + "expiry select" + "CVV toggle" → four UI triggers: "form", "input", "select", "toggle" → deduplicated to one UI predicate). Result: [TEST, UI]. Routing: [5.3, 1.5].

---

## E026-005 — PaymentFlowTest: "review" alone vs "QA review"

A: "QA review the payment flow before merge" vs "Review the payment flow before merge." Difference?
B: "QA review" → QA fires (literal "QA" abbreviation present). "Review the payment flow" → QA does NOT fire (review alone is gap; "QA" abbreviation absent). First case: [QA] fires (+ any other predicates from context). Second case: QA does not fire. Critical rule: it is the presence of the two-letter "QA" abbreviation that gates QA firing.
