# Layer 0-D — Flow Factory Pairs (016)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F016-001 — TEST/5.3 alone → one flow: BDD scenario for checkout

A:
  sub_phase: [5.3]
  predicate: ["TEST"]
  compiled_text: "Write BDD scenarios in Gherkin for the checkout flow: Given-When-Then for each payment method."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "E2ETestingFlow53"
      predicate: TEST
      sub_phase: 5.3
  TransitionType: documentation
  SelectionReason: "TEST alone → E2ETestingFlow53; BDD/scenario vocabulary → 5.3 (E2E context); TransitionType: documentation (TEST+no IMPLEMENT)"

---

## F016-002 — TEST/5.3 + UI/1.5 → two flows: Cucumber scenario for modal

A:
  sub_phase: [5.3, 1.5]
  predicate: ["TEST", "UI"]
  compiled_text: "Create Gherkin scenarios that assert the confirmation modal opens, traps focus, and closes on ESC."

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
  SelectionReason: "scenarios + assert → TEST → 5.3; modal + focus → UI → 1.5"

---

## F016-003 — TEST/5.3 + QA/7.3 → two flows: BDD acceptance + QA

A:
  sub_phase: [5.3, 7.3]
  predicate: ["TEST", "QA"]
  compiled_text: "Write acceptance test scenarios for the shopping cart and get QA sign-off on the test plan."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "E2ETestingFlow53"
      predicate: TEST
      sub_phase: 5.3
    - FlowId: "QAGatingFlow73"
      predicate: QA
      sub_phase: 7.3
  TransitionType: documentation
  SelectionReason: "acceptance test scenarios → TEST → 5.3; QA sign-off → QA → 7.3; no UI component vocabulary"
