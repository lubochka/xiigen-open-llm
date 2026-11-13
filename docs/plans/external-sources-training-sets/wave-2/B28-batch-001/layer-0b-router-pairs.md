# Layer 0-B — Router Pairs
# Batch: B28-batch-001
# Scenario: B28 — QA automation DevOps, sub-phase 7.1
# Contract: PromptRoutingInputRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRouterResult>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Pair B28B1-WDA-router-001 — QA + IMPLEMENT + TEST → 7.3 + 4.1 + 5.3

```
source_id: xiigen-b28-router-001
transition_type: feature_addition
phase: 7
sub_phase: 7.1
pair_kind: router
```

A:
  compiled_predicate_ids: ["QA", "IMPLEMENT", "TEST"]
  compiled_text: "Implement the automated QA pipeline for the release process and build the smoke test harness."

B:
  RoutingDecisions:
    - predicate: QA
      sub_phase: 7.3
      sub_phase_label: "QA Testing and Validation"
      confidence: 0.92
      routing_note: "'QA' → 7.3"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      sub_phase_label: "Service and Logic Implementation"
      confidence: 0.92
      routing_note: "'implement' + 'build' → IMPLEMENT → 4.1"
    - predicate: TEST
      sub_phase: 5.3
      sub_phase_label: "End-to-End and UI Testing"
      confidence: 0.92
      routing_note: "'smoke test' → TEST → 5.3 (smoke tests are E2E-style, not unit tests)"

  TransitionType: feature_addition
  PrimarySubPhaseId: 7.3
  AllSubPhaseIds: [7.3, 4.1, 5.3]

  Sub-phase note: "Smoke test" routes TEST to 5.3 (E2E testing) rather than 5.1 (unit testing)
  because smoke tests are broad system-level checks, not unit-level function tests.

---

## Pair B28B1-WDA-router-002 — QA + TEST → 7.3 + 5.3

```
source_id: xiigen-b28-router-002
transition_type: feature_addition
phase: 7
sub_phase: 7.3
pair_kind: router
```

A:
  compiled_predicate_ids: ["QA", "TEST"]
  compiled_text: "QA the deployment artifacts and verify the staging environment health checks."

B:
  RoutingDecisions:
    - predicate: QA
      sub_phase: 7.3
      sub_phase_label: "QA Testing and Validation"
      confidence: 0.92
      routing_note: "'QA' → 7.3"
    - predicate: TEST
      sub_phase: 5.3
      sub_phase_label: "End-to-End and UI Testing"
      confidence: 0.92
      routing_note: "'verify' → TEST → 5.3 (environment-level verify → E2E context)"

  TransitionType: feature_addition
  PrimarySubPhaseId: 7.3
  AllSubPhaseIds: [7.3, 5.3]

---

## Pair B28B1-WDA-router-003 — QA + ARCHITECTURE + TEST → 7.3 + 3.1 + 5.1

```
source_id: xiigen-b28-router-003
transition_type: feature_addition
phase: 7
sub_phase: 7.1
pair_kind: router
```

A:
  compiled_predicate_ids: ["QA", "ARCHITECTURE", "TEST"]
  compiled_text: "Design the QA automation architecture and structure the test infrastructure layer."

B:
  RoutingDecisions:
    - predicate: QA
      sub_phase: 7.3
      sub_phase_label: "QA Testing and Validation"
      confidence: 0.92
      routing_note: "'QA' → 7.3"
    - predicate: ARCHITECTURE
      sub_phase: 3.1
      sub_phase_label: "System Architecture and Technical Design"
      confidence: 0.92
      routing_note: "'design' + 'architecture' + 'structure' + 'layer' → ARCHITECTURE → 3.1"
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'test infrastructure' — 'test' → TEST → 5.1 (design-time test planning → unit context)"

  TransitionType: feature_addition
  PrimarySubPhaseId: 7.3
  AllSubPhaseIds: [7.3, 3.1, 5.1]
