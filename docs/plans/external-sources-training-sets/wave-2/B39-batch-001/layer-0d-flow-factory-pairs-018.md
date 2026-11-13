# Layer 0-D — Flow Factory Pairs (018)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F018-001 — TEST/5.3 + UI/1.5 + QA/7.3 → three flows: cross-browser suite

A:
  sub_phase: [5.3, 1.5, 7.3]
  predicate: ["TEST", "UI", "QA"]
  compiled_text: "Write cross-browser tests for the product listing page layout on Chrome, Firefox, Safari, and QA the results."

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
  SelectionReason: "cross-browser test context → 5.3; page + layout → 1.5; QA → 7.3"

---

## F018-002 — TEST/5.3 + UI/1.5 → two flows: modal on IE11/Edge

A:
  sub_phase: [5.3, 1.5]
  predicate: ["TEST", "UI"]
  compiled_text: "Add test coverage for the confirmation modal on IE11 and Edge to catch flexbox rendering bugs."

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
  SelectionReason: "'add test coverage' → TEST → 5.3; 'confirmation modal' → UI → 1.5"

---

## F018-003 — TEST/5.3 + UI/1.5 + QA/7.3 → three flows: regression + QA acceptance

A:
  sub_phase: [5.3, 1.5, 7.3]
  predicate: ["TEST", "UI", "QA"]
  compiled_text: "Write cross-browser regression test cases for each form input component and get QA acceptance."

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
  AllSubPhaseIds: [5.3, 1.5, 7.3]
