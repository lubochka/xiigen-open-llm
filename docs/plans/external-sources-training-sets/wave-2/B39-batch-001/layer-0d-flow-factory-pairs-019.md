# Layer 0-D — Flow Factory Pairs (019)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F019-001 — TEST/5.3 + UI/1.5 → two flows: mobile responsive e2e

A:
  sub_phase: [5.3, 1.5]
  predicate: ["TEST", "UI"]
  compiled_text: "Write end-to-end tests for the mobile checkout flow at 320px covering the sticky footer button."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "E2ETestingFlow53"
      predicate: TEST
      sub_phase: 5.3
    - FlowId: "UIInteractionFlow15"
      predicate: UI
      sub_phase: 1.5
  TransitionType: documentation
  SelectionReason: "E2E tests → 5.3; button → UI → 1.5; 'mobile' is gap alone"

---

## F019-002 — TEST/5.3 + UI/1.5 + IMPLEMENT/4.1 → three flows: build + test responsive

A:
  sub_phase: [5.3, 1.5, 4.1]
  predicate: ["TEST", "UI", "IMPLEMENT"]
  compiled_text: "Implement the responsive grid layout and add test coverage for each breakpoint on mobile."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
    - FlowId: "E2ETestingFlow53"
      predicate: TEST
      sub_phase: 5.3
    - FlowId: "UIInteractionFlow15"
      predicate: UI
      sub_phase: 1.5
  TransitionType: feature_addition
  SelectionReason: "IMPLEMENT present → feature_addition; implement → 4.1; test coverage → 5.3; responsive layout → 1.5"

---

## F019-003 — TEST/5.3 + UI/1.5 + QA/7.3 → three flows: full mobile test + QA

A:
  sub_phase: [5.3, 1.5, 7.3]
  predicate: ["TEST", "UI", "QA"]
  compiled_text: "Create responsive test cases for the mobile product card, verify image scaling and tap targets, and QA sign-off."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "E2ETestingFlow53"
      predicate: TEST
      sub_phase: 5.3
    - FlowId: "UIInteractionFlow15"
      predicate: UI
      sub_phase: 1.5
    - FlowId: "QAGatingFlow73"
      predicate: QA
      sub_phase: 7.3
  TransitionType: documentation
  SelectionReason: "TEST+UI+QA without IMPLEMENT → documentation; QAGatingFlow73 last"
