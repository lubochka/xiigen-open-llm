# Layer 0-D — Flow Factory Pairs (020)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F020-001 — TEST/5.3 + UI/1.5 + QA/7.3 → three flows: accessibility audit

A:
  sub_phase: [5.3, 1.5, 7.3]
  predicate: ["TEST", "UI", "QA"]
  compiled_text: "Write accessibility audit tests for modal, form inputs, and navigation, QA sign-off on WCAG 2.1 AA."

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
  SelectionReason: "accessibility audit tests → 5.3; modal/form/nav → 1.5; QA → 7.3; QA validates accessibility compliance"

---

## F020-002 — TEST/5.3 + UI/1.5 → two flows: ARIA labels on buttons

A:
  sub_phase: [5.3, 1.5]
  predicate: ["TEST", "UI"]
  compiled_text: "Assert that every button has a non-empty aria-label attribute."

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
  SelectionReason: "'assert' → TEST → 5.3; 'button' + 'aria-label' → UI → 1.5"

---

## F020-003 — TEST/5.3 + UI/1.5 + QA/7.3 → three flows: screen reader + QA acceptance

A:
  sub_phase: [5.3, 1.5, 7.3]
  predicate: ["TEST", "UI", "QA"]
  compiled_text: "Create test cases that verify screen reader announcements for each modal state and QA acceptance."

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
