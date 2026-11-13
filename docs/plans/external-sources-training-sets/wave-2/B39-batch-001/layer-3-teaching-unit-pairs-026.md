# Layer 3 — Teaching Unit Pairs (026)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: teaching_unit

---

## TU026-001 — PaymentFlowTest: QA 7.1 vs 7.3 lesson

A: What is the core lesson of PaymentFlowTest regarding QA routing?
B: QA → 7.1 when release/staging vocabulary is present. "QA sign-off before release" → 7.1. "QA gate before merge" → 7.3. The distinction is purely lexical: the release pipeline vocabulary triggers 7.1; acceptance/gate vocabulary without release terms triggers 7.3.

---

## TU026-002 — PaymentFlowTest: "Stripe" is gap

A: Why doesn't "Stripe" or "PayPal" fire IMPLEMENT?
B: Third-party service names are gap — they do not fire any predicate. IMPLEMENT fires on build/implement/create/add [feature noun] verbs. Naming a payment provider is a configuration detail, not a feature-building verb. "Configure Stripe" = gap verb + gap noun → zero predicates (unless co-occurring UI vocabulary fires UI).

---

## TU026-003 — PaymentFlowTest: "verify" fires TEST in payment context

A: "Verify the payment button redirects to the confirmation screen." Does TEST fire?
B: Yes. "verify" is a standalone test assertion verb → TEST fires. "confirmation screen" → screen → UI fires. Result: [TEST, UI]. "verify" fires TEST even when the assertion is about redirect behavior, not UI state.

---

## TU026-004 — PaymentFlowTest: credit card form + CVV input → one UI

A: "Assert the credit card form accepts input, verify the CVV input validates." How many UI predicates fire?
B: One. "credit card form" → form (UI trigger 1). "CVV input" → input (UI trigger 2). Two triggers → one UI predicate. Deduplication rule is absolute. AllSubPhaseIds: [5.3, 1.5].

---

## TU026-005 — PaymentFlowTest: TransitionType invariant

A: Under what conditions does a PaymentFlowTest request produce feature_addition?
B: Only when IMPLEMENT fires. IMPLEMENT fires if "build", "implement", "create", or "add [feature noun]" is present. "Build the payment processor integration" → IMPLEMENT → feature_addition. Absent IMPLEMENT: always documentation, regardless of how many other predicates (TEST, UI, QA) are present.
