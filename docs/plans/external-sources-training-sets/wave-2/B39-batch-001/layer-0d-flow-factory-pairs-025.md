# Layer 0-D — Flow Factory Pairs (025)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F025-001 — CheckoutFlowTest: flow selection for [TEST, UI]

A: Predicates [TEST, UI] fire for a CheckoutFlowTest request. Which flows are instantiated?
B: TEST → E2ETestingFlow53 (5.3). UI → UIInteractionFlow15 (1.5). Two flows instantiated in parallel after the predicate stage. TransitionType: documentation (no IMPLEMENT). TrustState: deterministic_generic.

---

## F025-002 — CheckoutFlowTest: flow selection for [TEST, UI, QA]

A: Predicates [TEST, UI, QA] fire. Which flows and in what order?
B: E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5) run in parallel. QAGatingFlow73 (7.3) runs last. Three flows. QA validates output of both 5.3 and 1.5 before signaling completion. TransitionType: documentation.

---

## F025-003 — CheckoutFlowTest: flow selection for [IMPLEMENT, TEST, UI]

A: "Build the checkout page and write E2E test coverage." Predicates [IMPLEMENT, TEST, UI]. Flows?
B: BackendServiceFlow41 (4.1) runs first. Then E2ETestingFlow53 (5.3) + UIInteractionFlow15 (1.5) run in parallel. TransitionType: feature_addition (IMPLEMENT overrides). AllSubPhaseIds: [4.1, 5.3, 1.5].

---

## F025-004 — CheckoutFlowTest: TrustState for gap request

A: "Run the checkout tests." Zero predicates. What is TrustState and what flows instantiate?
B: TrustState: knowledge_edge_synthetic. Zero FlowInstances. CapabilityGapSignal forwarded to AutonomousLearningPipeline. SCHEDULE_CAPABILITY_LEARNING emitted. No TransitionType.

---

## F025-005 — CheckoutFlowTest: Node 1 output for E2ETestingFlow53

A: When E2ETestingFlow53 is instantiated for CheckoutFlowTest, what does Node 1 (DomainSplitterUnit) output?
B: Node 1 outputs SelectedLogicalTransition = E2ETestingFlow53. Target domain: checkout flow (multi-step browser interaction). UI domains to test: cart state, payment button, order summary modal, success screen. SelectedLogicalTransition is the non-negotiable first output of Node 1 for all 5.3 flows.
