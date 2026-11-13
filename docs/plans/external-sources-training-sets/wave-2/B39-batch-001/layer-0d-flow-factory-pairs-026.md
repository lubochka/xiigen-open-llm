# Layer 0-D — Flow Factory Pairs (026)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F026-001 — PaymentFlowTest: flow selection

A: "Write E2E tests for the payment flow — assert the credit card form, validate the CVV input." Predicates [TEST, UI]. Flows?
B: E2ETestingFlow53 (5.3) for TEST. UIInteractionFlow15 (1.5) for UI ("credit card form" → form; "CVV input" → input). Two flows parallel. TransitionType: documentation. TrustState: deterministic_generic.

---

## F026-002 — PaymentFlowTest: QA with release vocabulary → 7.1

A: "Write payment flow E2E tests, assert the payment form, QA sign-off before release." Predicates [TEST, UI, QA]. Route QA.
B: QA routes to ReleaseGateFlow71 (7.1) because "before release" is explicit release-pipeline vocabulary. AllSubPhaseIds: [5.3, 1.5, 7.1]. QA (7.1) executes last. TransitionType: documentation.

---

## F026-003 — PaymentFlowTest: IMPLEMENT changes TransitionType

A: "Build the payment processor integration and write E2E test coverage for the payment form." Flows?
B: BackendServiceFlow41 (4.1) — "build ... integration". E2ETestingFlow53 (5.3) — "write E2E test coverage". UIInteractionFlow15 (1.5) — "payment form" → form. Three flows. TransitionType: feature_addition (IMPLEMENT present). Order: 4.1 → 5.3 + 1.5 parallel.

---

## F026-004 — PaymentFlowTest: "Stripe" is gap

A: "Configure Stripe for the payment flow and verify the payment button." Does "Stripe" fire IMPLEMENT?
B: No. "Stripe" is a third-party service name — gap. "configure" is gap. "verify the payment button" → TEST (verify fires TEST) + UI (button fires UI). Result: [TEST, UI]. Flows: E2ETestingFlow53 + UIInteractionFlow15. TransitionType: documentation.

---

## F026-005 — PaymentFlowTest: Node 1 domain for payment scenarios

A: When E2ETestingFlow53 runs for PaymentFlowTest, what domains does Node 1 identify?
B: SelectedLogicalTransition = E2ETestingFlow53. Domains: payment form (credit card, CVV, expiry), payment method selection (card/bank/wallet), payment confirmation screen, error states (card declined, timeout). Node 1 produces domain list for Node 2 (DomainTranslatorUnit) to map to concrete test scenarios.
