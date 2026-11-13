# Layer 0-B — Router Pairs (025)
# Batch: B39-batch-001 | Scenario: B39 — E2E and UI testing, sub-phase 5.3
# Production date: 2026-05-06 | pair_kind: router

---

## C0B-025-001 — CheckoutFlowTest: routing decision

A: "Write E2E test cases for the checkout flow — assert the payment button state." Predicates: [TEST, UI]. How are they routed?
B: TEST → 5.3 (E2ETestingFlow53): checkout flow is E2E/browser context, not unit test context. UI → 1.5 (UIInteractionFlow15): "payment button" is UI component vocabulary — always routes to 1.5. PrimarySubPhaseId: 5.3. AllSubPhaseIds: [5.3, 1.5]. TransitionType: documentation.

---

## C0B-025-002 — CheckoutFlowTest: disambiguating 5.1 vs 5.3

A: For a CheckoutFlowTest request, how is it confirmed that TEST routes to 5.3 and not 5.1?
B: 5.3 disambiguation signals present: "checkout flow" (multi-step browser flow), "payment button" (UI component interaction), "E2E" (explicit). 5.1 signals absent: no "unit test", no pure function, no service/repository/logic vocabulary without UI. Conclusion: 5.3. Rule: when UI vocabulary is present alongside TEST, 5.3 is confirmed (UI + TEST always → 5.3 for TEST).

---

## C0B-025-003 — CheckoutFlowTest: QA routing added

A: "Write checkout E2E tests, assert the modal renders, and QA gate before merge." Predicates: [TEST, UI, QA]. Route each.
B: TEST → 5.3 (E2ETestingFlow53). UI → 1.5 (UIInteractionFlow15). QA → 7.3 (QAGatingFlow73): "before merge" is feature-acceptance vocabulary, not release/staging. Default 7.3 applies. AllSubPhaseIds: [5.3, 1.5, 7.3]. QA executes last.

---

## C0B-025-004 — CheckoutFlowTest: IMPLEMENT added

A: "Build the checkout page, write E2E test coverage for it." Predicates: [IMPLEMENT, TEST, UI]. Route each.
B: IMPLEMENT → 4.1 (BackendServiceFlow41). TEST → 5.3 (E2ETestingFlow53). UI → 1.5 (UIInteractionFlow15). TransitionType: feature_addition (IMPLEMENT present). Execution order: IMPLEMENT (4.1) → TEST+UI parallel (5.3 + 1.5). AllSubPhaseIds: [4.1, 5.3, 1.5].

---

## C0B-025-005 — CheckoutFlowTest: gap request — no routing

A: "Run the checkout tests." Predicates: []. What is the routing?
B: Zero predicates → knowledge_edge_synthetic routing. No FlowInstances created. CapabilityGapSignal forwarded to AutonomousLearningPipeline (subproject 16). SCHEDULE_CAPABILITY_LEARNING emitted. TrustState: knowledge_edge_synthetic.
