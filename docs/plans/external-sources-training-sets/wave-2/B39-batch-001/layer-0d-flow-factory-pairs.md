# Layer 0-D — Flow Factory Pairs
# Batch: B39-batch-001
# Scenario: B39 — QA UI automation, sub-phases 5.3/7.3/1.5
# Contract: FlowFactoryUnit : ILogicalUnit<SubPhaseRouterResult, FlowInstanceDescriptor>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Pair B39B1-WDA-factory-001 — TEST/5.3 + UI/1.5 + IMPLEMENT/4.1 → three flows

```
source_id: xiigen-b39-factory-001
transition_type: feature_addition
phase: 5
sub_phase: 5.3
pair_kind: factory_selection
```

A:
  sub_phase: [5.3, 1.5, 4.1]
  predicate: ["TEST", "UI", "IMPLEMENT"]
  compiled_text: "Write end-to-end tests for the checkout form covering button clicks, input validation, and modal confirmation."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "E2ETestingFlow53"
      predicate: TEST
      sub_phase: 5.3
      routing_note: "End-to-End and UI Testing flow"
    - FlowId: "UIComponentFlow15"
      predicate: UI
      sub_phase: 1.5
      routing_note: "UI/UX Component Design flow — fires on form/button/input/modal"
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
      routing_note: "Service implementation flow — fires on 'write'"
  SelectionReason: "Three predicates → three independent flow instances; all deterministic_generic"

---

## Pair B39B1-WDA-factory-002 — QA/7.3 + UI/1.5 + TEST/5.3 → three flows

```
source_id: xiigen-b39-factory-002
transition_type: feature_addition
phase: 7
sub_phase: 7.3
pair_kind: factory_selection
```

A:
  sub_phase: [7.3, 1.5, 5.3]
  predicate: ["QA", "UI", "TEST"]
  compiled_text: "QA the login screen and automate the user interface component interactions for regression testing."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "QATestingFlow73"
      predicate: QA
      sub_phase: 7.3
    - FlowId: "UIComponentFlow15"
      predicate: UI
      sub_phase: 1.5
    - FlowId: "E2ETestingFlow53"
      predicate: TEST
      sub_phase: 5.3
  SelectionReason: "Three predicates → three independent flow instances; all deterministic_generic"

---

## Pair B39B1-WDA-factory-003 — TEST/5.3 + UI/1.5 (no IMPLEMENT for Cypress set-up)

```
source_id: xiigen-b39-factory-003
transition_type: feature_addition
phase: 5
sub_phase: 5.3
pair_kind: factory_selection
```

A:
  sub_phase: [5.3, 1.5]
  predicate: ["TEST", "UI"]
  compiled_text: "Set up Cypress to run automated browser tests on the product catalog page."
  capability_gap_warnings: ["gap-testing-automation-001: Cypress, automated, set up are gap"]

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "E2ETestingFlow53"
      predicate: TEST
      sub_phase: 5.3
    - FlowId: "UIComponentFlow15"
      predicate: UI
      sub_phase: 1.5
  SelectionReason: "Two predicates → two independent flow instances; gap warnings pass through"
  CapabilityGapWarningPassthrough: true

---

## Pair B39B1-WDA-factory-004 — zero predicates ("automate user journey") → knowledge_edge_synthetic

```
source_id: xiigen-b39-factory-004
transition_type: unknown
phase: null
sub_phase: null
pair_kind: factory_selection
```

A:
  sub_phase: null
  predicate: null
  compiled_text: "Automate the user journey through the checkout process."
  capability_gap_signal:
    gap_id: "gap-testing-automation-001"
    DomainId: "software-development"

B:
  TrustState: knowledge_edge_synthetic
  FlowId: null
  SelectionReason: "Zero predicates → knowledge_edge_synthetic; 'automate' and 'user journey' are gap"
  NodeCount: 0
  CapabilityGapSignalForwarded: true
