# Layer 0-D — Flow Factory Pairs (013)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F013-001 — TEST/5.3 + UI/1.5 → two flows: Cypress e2e

A:
  sub_phase: [5.3, 1.5]
  predicate: ["TEST", "UI"]
  compiled_text: "Write Cypress e2e tests for the shopping cart button flow."

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
  SelectionReason: "Two predicates → two flows; TEST+UI without IMPLEMENT → documentation"

---

## F013-002 — TEST/5.3 + UI/1.5 + QA/7.3 → three flows

A:
  sub_phase: [5.3, 1.5, 7.3]
  predicate: ["TEST", "UI", "QA"]
  compiled_text: "Write e2e tests for the product detail page and QA sign-off before release."

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
  SelectionReason: "Three predicates; TEST+UI+QA without IMPLEMENT → documentation; QA flow last"

---

## F013-003 — zero predicates → knowledge_edge_synthetic

A:
  sub_phase: null
  predicate: null
  compiled_text: "Run the Cypress suite against the staging checkout page."

B:
  TrustState: knowledge_edge_synthetic
  FlowId: null
  SelectionReason: "Zero predicates → no flow; 'run' and 'Cypress' are gap vocabulary"
  CapabilityGapSignalForwarded: true
