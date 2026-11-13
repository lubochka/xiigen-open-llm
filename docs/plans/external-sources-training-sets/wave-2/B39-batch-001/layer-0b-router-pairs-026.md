# Layer 0-B — Router Pairs (026)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-026-001 — PaymentFlowTest: routing decision

A: "Write test cases for the payment flow — assert the credit card form, verify the confirmation screen." Predicates: [TEST, UI]. Route each.
B: TEST → 5.3 (E2ETestingFlow53): payment flow is multi-step UI interaction, E2E context. UI → 1.5 (UIInteractionFlow15): "credit card form", "confirmation screen" → "form", "screen" always route to 1.5. PrimarySubPhaseId: 5.3. TransitionType: documentation.

---

## C0B-026-002 — PaymentFlowTest: QA → 7.1 on staging vocabulary

A: "Write payment flow tests, assert the form, and QA before staging deployment." Predicates: [TEST, UI, QA]. Route QA specifically.
B: QA → 7.1 (ReleaseGateFlow71): "staging deployment" is explicit release/staging vocabulary → overrides default 7.3. TEST → 5.3. UI → 1.5. AllSubPhaseIds: [5.3, 1.5, 7.1]. This is one of the rare cases where QA departs from its default 7.3 routing.

---

## C0B-026-003 — PaymentFlowTest: UI always to 1.5

A: Does "payment form" ever route UI anywhere other than 1.5?
B: No. UI predicate always routes to 1.5 (UIInteractionFlow15). This is a hard rule with zero exceptions. "payment form" fires UI; UI → 1.5. Context (testing, building, reviewing, documenting) does not change the UI routing. UIInteractionFlow15 is always the destination.

---

## C0B-026-004 — PaymentFlowTest: IMPLEMENT routing

A: "Implement the payment modal component." Predicates: [IMPLEMENT, UI]. Route each.
B: IMPLEMENT → 4.1 (BackendServiceFlow41). UI → 1.5 (UIInteractionFlow15). No TEST in this request. TransitionType: feature_addition (IMPLEMENT present). Note: "implement" fires IMPLEMENT but not TEST. AllSubPhaseIds: [4.1, 1.5].

---

## C0B-026-005 — PaymentFlowTest: PrimarySubPhaseId with QA present

A: Predicates [TEST, UI, QA] fire. What is PrimarySubPhaseId?
B: PrimarySubPhaseId: 5.3. Ordering: QA (7.3) executes last as validator; TEST (5.3) is the primary action predicate. Rule: IMPLEMENT (4.1) > QA (7.3) > TEST (5.3) > UI (1.5) — but PrimarySubPhaseId follows the primary action predicate when IMPLEMENT is absent. With TEST present: PrimarySubPhaseId = 5.3.
