# Layer 0-D — Flow Factory Pairs (022)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F022-001 — TEST/5.3 + UI/1.5 → two flows: form validation e2e

A:
  sub_phase: [5.3, 1.5]
  predicate: ["TEST", "UI"]
  compiled_text: "Write end-to-end test cases covering every validation rule on the contact form."

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
  SelectionReason: "end-to-end test cases → 5.3; contact form → 1.5; TEST+UI without IMPLEMENT → documentation"

---

## F022-002 — TEST/5.3 + UI/1.5 → two flows: inline error on email input

A:
  sub_phase: [5.3, 1.5]
  predicate: ["TEST", "UI"]
  compiled_text: "Assert that the contact form shows the inline validation error on invalid email input."

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
  SelectionReason: "'assert' → TEST → 5.3; 'contact form' + 'email input' → UI → 1.5"

---

## F022-003 — TEST/5.3 + UI/1.5 + QA/7.3 → three flows: full form e2e + QA gate

A:
  sub_phase: [5.3, 1.5, 7.3]
  predicate: ["TEST", "UI", "QA"]
  compiled_text: "Create end-to-end test scenarios for every validation rule on the registration form and get QA acceptance."

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
