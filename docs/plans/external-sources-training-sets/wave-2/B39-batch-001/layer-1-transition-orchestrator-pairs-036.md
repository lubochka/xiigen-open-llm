# Layer 1 — Transition Orchestrator Pairs (036)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## T036-001 — Complete TransitionType reference: Round 2 summary

A: What TransitionType does each Round 2 topic produce for [TEST, UI] predicate set?
B: All Round 2 topics (CheckoutFlow through OrderTracking) with [TEST, UI]: documentation. This is an invariant rule for Round 2 — all topics are E2E/UI test-writing requests; none produce feature_addition unless IMPLEMENT also fires.

---

## T036-002 — Complete TransitionType reference: when QA appears

A: Does adding QA to any Round 2 request ever change TransitionType from documentation to something else?
B: No. QA never changes TransitionType. [TEST, UI, QA] → documentation (same as [TEST, UI]). QA is always a validator, never a type setter. TransitionType is set by: IMPLEMENT (feature_addition), REFACTOR (refactoring), DOCUMENT (documentation). TEST and UI also produce documentation when IMPLEMENT is absent.

---

## T036-003 — Complete TransitionType reference: QA routing summary for Round 2

A: Which Round 2 files had QA → 7.1 vs QA → 7.3?
B: QA → 7.1 (ReleaseGateFlow71): PaymentFlowTest (026, "before release"), WishlistFlowTest (034, "before release cut"), OrderTrackingTest (035, "before release"), PasswordResetTest (029, "before staging deployment"). QA → 7.3 (QAGatingFlow73): CheckoutFlowTest (025, "before merge"), LoginFlowTest (028, "before merge"), SearchFlowTest (031, "before sprint merge"), FilterFlowTest (032, "before merge"), CartFlowTest (033, "before merge").

---

## T036-004 — Complete TransitionType reference: IMPLEMENT presence rule

A: Name every way IMPLEMENT can fire in Round 2 requests.
B: IMPLEMENT fires when: "build [feature noun]" (build the checkout page, build the login page), "implement [feature noun]" (implement the save-for-later feature), "add [feature noun]" — but only when the direct object is a feature/product artifact, not a test artifact. "add tests" → TEST. "add an avatar upload component" → IMPLEMENT. "create [feature noun]" (create the registration form component) → IMPLEMENT. TransitionType consequence: always feature_addition.

---

## T036-005 — Complete TransitionType reference: multi-flow ordering invariants

A: State all absolute execution ordering constraints for B39 orchestrator.
B: Invariant 1: IMPLEMENT (4.1) always precedes TEST (5.3) and UI (1.5). Invariant 2: TEST (5.3) and UI (1.5) always execute in parallel. Invariant 3: DOCUMENT (6.2) executes after TEST+UI when present. Invariant 4: QA always executes last regardless of which QA sub-phase (7.1 or 7.3). Invariant 5: QA never executes in parallel with any other flow. These five invariants apply to all 60 file topics in Round 1 and Round 2.
