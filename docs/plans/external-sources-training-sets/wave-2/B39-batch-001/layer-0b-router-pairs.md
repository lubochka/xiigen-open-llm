# Layer 0-B — Router Pairs
# Batch: B39-batch-001
# Scenario: B39 — QA UI automation, sub-phases 5.3/7.3/1.5
# Contract: PromptRoutingInputRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRouterResult>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Pair B39B1-WDA-router-001 — TEST + UI + IMPLEMENT → 5.3 + 1.5 + 4.1

```
source_id: xiigen-b39-router-001
transition_type: feature_addition
phase: 5
sub_phase: 5.3
pair_kind: router
```

A:
  compiled_predicate_ids: ["TEST", "UI", "IMPLEMENT"]
  compiled_text: "Write end-to-end tests for the checkout form covering button clicks, input validation, and modal confirmation."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.3
      sub_phase_label: "End-to-End and UI Testing"
      confidence: 0.92
      routing_note: "'end-to-end tests' → TEST → 5.3 (E2E testing, not 5.1 unit testing)"
    - predicate: UI
      sub_phase: 1.5
      sub_phase_label: "UI/UX Prototyping and Component Design"
      confidence: 0.92
      routing_note: "'form' + 'button' + 'input' + 'modal' → UI → 1.5 (UI predicate always routes to Phase 1)"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      sub_phase_label: "Service and Logic Implementation"
      confidence: 0.92
      routing_note: "'write' → IMPLEMENT → 4.1"

  TransitionType: feature_addition
  PrimarySubPhaseId: 5.3
  AllSubPhaseIds: [5.3, 1.5, 4.1]

  Sub-phase note: UI predicate routes to 1.5 regardless of whether the request is about
  testing or designing UI. The predicate routing is deterministic: UI → 1.5. The testing
  aspect is captured by TEST → 5.3 independently.

---

## Pair B39B1-WDA-router-002 — QA + UI + TEST → 7.3 + 1.5 + 5.3

```
source_id: xiigen-b39-router-002
transition_type: feature_addition
phase: 7
sub_phase: 7.3
pair_kind: router
```

A:
  compiled_predicate_ids: ["QA", "UI", "TEST"]
  compiled_text: "QA the login screen and automate the user interface component interactions for regression testing."

B:
  RoutingDecisions:
    - predicate: QA
      sub_phase: 7.3
      sub_phase_label: "QA Testing and Validation"
      confidence: 0.92
      routing_note: "'QA' → 7.3"
    - predicate: UI
      sub_phase: 1.5
      sub_phase_label: "UI/UX Prototyping and Component Design"
      confidence: 0.92
      routing_note: "'screen' + 'interface' + 'component' → UI → 1.5"
    - predicate: TEST
      sub_phase: 5.3
      sub_phase_label: "End-to-End and UI Testing"
      confidence: 0.92
      routing_note: "'testing' (regression testing) → TEST → 5.3"

  TransitionType: feature_addition
  PrimarySubPhaseId: 7.3
  AllSubPhaseIds: [7.3, 1.5, 5.3]

---

## Pair B39B1-WDA-router-003 — TEST + UI + IMPLEMENT → 5.3 + 1.5 + 4.1

```
source_id: xiigen-b39-router-003
transition_type: feature_addition
phase: 5
sub_phase: 5.3
pair_kind: router
```

A:
  compiled_predicate_ids: ["TEST", "UI", "IMPLEMENT"]
  compiled_text: "Implement the Playwright test suite to validate the payment page layout and UI components."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.3
      sub_phase_label: "End-to-End and UI Testing"
      confidence: 0.92
      routing_note: "'test suite' + 'validate' → TEST → 5.3"
    - predicate: UI
      sub_phase: 1.5
      sub_phase_label: "UI/UX Prototyping and Component Design"
      confidence: 0.92
      routing_note: "'page' + 'layout' + 'components' → UI → 1.5"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      sub_phase_label: "Service and Logic Implementation"
      confidence: 0.92
      routing_note: "'implement' → IMPLEMENT → 4.1"

  TransitionType: feature_addition
  PrimarySubPhaseId: 5.3
  AllSubPhaseIds: [5.3, 1.5, 4.1]
