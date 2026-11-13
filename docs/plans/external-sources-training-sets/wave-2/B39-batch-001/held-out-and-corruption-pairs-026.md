# Held-Out and Corruption Pairs (026)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: held_out + corruption

---

## H026-001 — Held-out: PaymentFlowTest — valid + invalid states

A: "Write payment E2E tests — assert the credit card form accepts valid card numbers, validate the CVV input rejects 2-digit entries."
  Expected: ["TEST", "UI"]
  Held-out: TEST: "write payment E2E tests" + "assert" + "validate" → three triggers → one TEST. UI: "credit card form" (form) + "CVV input" (input) → two triggers → one UI. Result: [TEST, UI]. Routing: [5.3, 1.5]. TransitionType: documentation.

---

## H026-002 — Held-out: PaymentFlowTest — "Stripe" gap passthrough

A: "Configure Stripe for payments and verify the payment button is active."
  Expected: ["TEST", "UI"]
  Held-out: "configure" is gap. "Stripe" is gap. "verify" fires TEST. "payment button" → button → UI. Result: [TEST, UI] with CapabilityGapWarningPassthrough: true for "configure" and "Stripe". Routing: [5.3, 1.5].

---

## H026-003 — Held-out: PaymentFlowTest — QA before release → 7.1

A: "Write payment E2E tests, assert the confirmation screen, and QA sign-off before release."
  Expected: ["TEST", "UI", "QA"]
  Held-out: TEST (write, assert). UI (confirmation screen → screen). QA (QA sign-off). QA → 7.1 ("before release"). AllSubPhaseIds: [5.3, 1.5, 7.1]. TransitionType: documentation.

---

## C026-001 — Corruption v6-01: PAYMENT predicate invented

Pattern: v6-01. Corrupt: predicates = ["TEST", "UI", "PAYMENT"].
Correct: predicates = ["TEST", "UI"]. "PAYMENT" is a domain term. No predicate may be invented from domain names.

---

## C026-002 — Corruption v6-02: "configure" → IMPLEMENT

Pattern: v6-02. Corrupt: "Configure Stripe" → predicates = ["IMPLEMENT"].
Correct: "configure" is gap. IMPLEMENT does not fire. No predicate fires for "configure Stripe" alone.
