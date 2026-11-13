# Layer 0-D — Flow Factory Pairs (015)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F015-001 — TEST/5.3 + UI/1.5 + QA/7.3 → three flows: visual regression + QA

A:
  sub_phase: [5.3, 1.5, 7.3]
  predicate: ["TEST", "UI", "QA"]
  compiled_text: "Write visual regression tests for buttons, cards, modals and QA sign-off on all baselines."

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
  SelectionReason: "Three predicates; TEST+UI+QA without IMPLEMENT → documentation"

---

## F015-002 — TEST/5.3 + UI/1.5 → two flows: snapshot baseline

A:
  sub_phase: [5.3, 1.5]
  predicate: ["TEST", "UI"]
  compiled_text: "Create snapshot tests for the product card component across light and dark themes."

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
  SelectionReason: "Snapshot test context → E2ETestingFlow53; component vocabulary → UIInteractionFlow15"

---

## F015-003 — UI/1.5 alone → one flow: update layout without test verb

A:
  sub_phase: [1.5]
  predicate: ["UI"]
  compiled_text: "Update the visual snapshots after the theme change."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UIInteractionFlow15"
      predicate: UI
      sub_phase: 1.5
  TransitionType: documentation
  SelectionReason: "'update' is gap; only UI fires on 'visual' vocabulary; one flow"
