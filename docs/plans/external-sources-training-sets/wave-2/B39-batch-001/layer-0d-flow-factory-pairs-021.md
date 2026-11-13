# Layer 0-D — Flow Factory Pairs (021)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F021-001 — TEST/5.3 + UI/1.5 + QA/7.3 → three flows: performance UI test + QA gate

A:
  sub_phase: [5.3, 1.5, 7.3]
  predicate: ["TEST", "UI", "QA"]
  compiled_text: "Write performance tests that assert the product listing page loads within 2s and QA the p95 timing."

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
  SelectionReason: "performance tests in UI context → 5.3; product listing page → 1.5; QA timing review → 7.3"

---

## F021-002 — TEST/5.3 + UI/1.5 → two flows: time-to-interactive modal

A:
  sub_phase: [5.3, 1.5]
  predicate: ["TEST", "UI"]
  compiled_text: "Assert that the search results modal renders fully within 500ms of the trigger."

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
  SelectionReason: "'assert' → TEST → 5.3; 'search results modal' → UI → 1.5"

---

## F021-003 — TEST/5.3 + UI/1.5 + QA/7.3 → three flows: perf regression + QA

A:
  sub_phase: [5.3, 1.5, 7.3]
  predicate: ["TEST", "UI", "QA"]
  compiled_text: "Write performance regression test cases for every page component and QA sign-off on timing baselines."

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
