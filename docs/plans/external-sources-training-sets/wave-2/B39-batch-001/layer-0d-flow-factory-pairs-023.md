# Layer 0-D — Flow Factory Pairs (023)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F023-001 — TEST/5.3 + UI/1.5 + QA/7.3 → three flows: three-predicate modal + ARIA + QA

A:
  sub_phase: [5.3, 1.5, 7.3]
  predicate: ["TEST", "UI", "QA"]
  compiled_text: "Write e2e test cases for the accessibility-compliant modal, validate every ARIA role, and QA the test suite."

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
  SelectionReason: "Dense three-predicate signal; QAGatingFlow73 validates both E2E test results and UI accessibility"

---

## F023-002 — TEST/5.3 + UI/1.5 + QA/7.3 → three flows: checkout button + responsive + QA

A:
  sub_phase: [5.3, 1.5, 7.3]
  predicate: ["TEST", "UI", "QA"]
  compiled_text: "Create test coverage for checkout button states, update the responsive layout, and QA sign-off."
  capability_gap_warnings: ["'update' is gap — no IMPLEMENT fires"]

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
  CapabilityGapWarningPassthrough: true
  SelectionReason: "'update' is gap; three predicates fire; documentation TransitionType"

---

## F023-003 — TEST/5.3 + UI/1.5 + QA/7.3 → three flows: navigation + interaction tests + QA

A:
  sub_phase: [5.3, 1.5, 7.3]
  predicate: ["TEST", "UI", "QA"]
  compiled_text: "Assert that every navigation component passes interaction tests and QA sign-off before shipping."

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
