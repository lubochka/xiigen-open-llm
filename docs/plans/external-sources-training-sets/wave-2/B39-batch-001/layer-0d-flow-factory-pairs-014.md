# Layer 0-D — Flow Factory Pairs (014)
# Batch: B39-batch-001 | Scenario: B39 — QA UI automation, TEST+UI+QA predicates
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## F014-001 — TEST/5.3 + UI/1.5 → two flows: Playwright login form

A:
  sub_phase: [5.3, 1.5]
  predicate: ["TEST", "UI"]
  compiled_text: "Write Playwright test cases for the login form: valid credentials, invalid password, locked-out account."

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
  SelectionReason: "TEST+UI without IMPLEMENT → documentation; login form context → E2E + UI flows"

---

## F014-002 — TEST/5.3 + UI/1.5 + IMPLEMENT/4.1 → three flows

A:
  sub_phase: [5.3, 1.5, 4.1]
  predicate: ["TEST", "UI", "IMPLEMENT"]
  compiled_text: "Implement the login page and write assertions for each validation error state on input fields."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "E2ETestingFlow53"
      predicate: TEST
      sub_phase: 5.3
    - FlowId: "UIInteractionFlow15"
      predicate: UI
      sub_phase: 1.5
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
  TransitionType: feature_addition
  SelectionReason: "IMPLEMENT present → feature_addition (overrides documentation default)"

---

## F014-003 — zero predicates → knowledge_edge_synthetic

A:
  sub_phase: null
  predicate: null
  compiled_text: "Configure Playwright to retry flaky tests three times before failing."

B:
  TrustState: knowledge_edge_synthetic
  FlowId: null
  SelectionReason: "Zero predicates; 'configure' and 'Playwright' are gap; no flow instantiated"
  CapabilityGapSignalForwarded: true
