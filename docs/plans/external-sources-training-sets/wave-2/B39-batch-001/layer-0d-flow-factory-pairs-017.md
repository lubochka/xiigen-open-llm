# Layer 0-D — Flow Factory Pairs (017)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F017-001 — TEST/5.3 + UI/1.5 → two flows: snapshot tests for component library

A:
  sub_phase: [5.3, 1.5]
  predicate: ["TEST", "UI"]
  compiled_text: "Write snapshot tests for the button, input, and dropdown components."

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
  SelectionReason: "snapshot tests → E2ETestingFlow53; button/input/dropdown → UIInteractionFlow15"

---

## F017-002 — TEST/5.3 + UI/1.5 + IMPLEMENT/4.1 → three flows: build harness + create tests

A:
  sub_phase: [5.3, 1.5, 4.1]
  predicate: ["TEST", "UI", "IMPLEMENT"]
  compiled_text: "Build the screenshot test harness and create snapshot assertions for every modal variant in the design system."

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
  SelectionReason: "IMPLEMENT present → feature_addition; build → 4.1; snapshot assertions → 5.3; modal → 1.5"

---

## F017-003 — TEST/5.3 + UI/1.5 + QA/7.3 → three flows: full snapshot review

A:
  sub_phase: [5.3, 1.5, 7.3]
  predicate: ["TEST", "UI", "QA"]
  compiled_text: "Create visual snapshot tests for each tooltip variant and QA sign-off that all baselines are approved."

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
  SelectionReason: "Three predicates; QA flow executes last and validates test + UI results"
