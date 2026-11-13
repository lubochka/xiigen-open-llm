# Layer 0-D — Flow Factory Pairs (036)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F036-001 — Complete flow reference: Round 2 all-predicate combinations

A: List every valid flow combination produced for Round 2 (CheckoutFlow through OrderTracking) topics.
B:
  [TEST, UI] → [E2ETestingFlow53, UIInteractionFlow15]
  [TEST, UI, QA] → [E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73] (default)
  [TEST, UI, QA] → [E2ETestingFlow53, UIInteractionFlow15, ReleaseGateFlow71] (release vocab)
  [IMPLEMENT, TEST, UI] → [BackendServiceFlow41, E2ETestingFlow53, UIInteractionFlow15]
  [IMPLEMENT, TEST, UI, QA] → [BackendServiceFlow41, E2ETestingFlow53, UIInteractionFlow15, QAGatingFlow73]
  [UI] → [UIInteractionFlow15]
  [TEST] → [E2ETestingFlow53]
  [] → knowledge_edge_synthetic

---

## F036-002 — Complete flow reference: TransitionType rules for Round 2

A: Enumerate TransitionType outcomes for all Round 2 predicate combinations.
B: IMPLEMENT present → feature_addition (overrides all). TEST+UI → documentation. TEST alone → documentation. UI alone → documentation. QA alone → no TransitionType. TEST+UI+QA → documentation. IMPLEMENT+TEST+UI+QA → feature_addition. Rule is absolute: IMPLEMENT is the only predicate that produces feature_addition.

---

## F036-003 — Complete flow reference: QA 7.1 vs 7.3 Round 2 summary

A: Summarize the QA routing decision (7.1 vs 7.3) for all Round 2 topics.
B: Default 7.3 (QAGatingFlow73): "QA gate before merge", "QA acceptance", "QA gate before sprint review", "QA sign-off" without release vocabulary. Override 7.1 (ReleaseGateFlow71): any of — "before release", "before staging", "before production", "pre-production deploy", "staging deployment", "release cut". Seen in Round 2: PaymentFlowTest (026), WishlistFlowTest (034), OrderTrackingTest (035).

---

## F036-004 — Complete flow reference: execution ordering rule

A: What is the absolute execution ordering rule when multiple flows are instantiated?
B: 1. IMPLEMENT (BackendServiceFlow41) always executes first if present. 2. TEST (E2ETestingFlow53) + UI (UIInteractionFlow15) always execute in parallel. 3. DOCUMENT (DocumentationFlow62) executes after TEST+UI if present. 4. QA (QAGatingFlow73 or ReleaseGateFlow71) always executes last. Rule: QA never precedes TEST, IMPLEMENT, or DOCUMENT.

---

## F036-005 — Complete flow reference: CapabilityGapWarningPassthrough

A: When gap vocabulary co-occurs with valid predicates, what does the factory output?
B: Factory instantiates flows for valid predicates normally. CapabilityGapWarningPassthrough: true is annotated on the FlowBundle. The gap vocabulary set is logged for AutonomousLearningPipeline (subproject 16) but does not block or alter flow instantiation. Example: "Automate and write E2E tests for the payment form" — "Automate" is gap, "write E2E tests" fires TEST, "payment form" fires UI → [E2ETestingFlow53, UIInteractionFlow15] with CapabilityGapWarningPassthrough: true.
