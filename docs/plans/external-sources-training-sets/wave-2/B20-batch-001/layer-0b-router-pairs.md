# Layer 0-B — Router Pairs
# Batch: B20-batch-001
# Scenario: B20 — Unit testing, sub-phase 5.1
# Contract: PromptRoutingInputRouterUnit : ILogicalUnit<SubPhaseRouterSourceDocument, SubPhaseRouterResult>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: router

---

## Pair B20B1-WDA-router-001 — TEST → 5.1

```
source_id: xiigen-b20-router-001
transition_type: feature_addition
phase: 5
sub_phase: 5.1
pair_kind: router
```

A:
  compiled_predicate_ids: ["TEST"]
  compiled_text: "Write unit tests for the PaymentCalculationService covering all edge cases."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'unit tests' → TEST → 5.1"

  TransitionType: feature_addition
  PhaseId: 5
  SubPhaseId: 5.1

---

## Pair B20B1-WDA-router-002 — TEST + API_CONTRACT + IMPLEMENT → three routing decisions

```
source_id: xiigen-b20-router-002
transition_type: feature_addition
phase: 5
sub_phase: 5.1
pair_kind: router
```

A:
  compiled_predicate_ids: ["TEST", "API_CONTRACT", "IMPLEMENT"]
  compiled_text: "Add integration tests for the OrderService API contract and mock the external payment interface."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'integration tests' → TEST → 5.1"
    - predicate: API_CONTRACT
      sub_phase: 3.3
      sub_phase_label: "API and Integration Design"
      confidence: 0.92
      routing_note: "'API contract' + 'interface' → API_CONTRACT → 3.3"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      sub_phase_label: "Service and Logic Implementation"
      confidence: 0.92
      routing_note: "'add' → IMPLEMENT → 4.1 (no connector/migration vocabulary)"

  TransitionType: feature_addition
  PrimarySubPhaseId: 5.1
  AllSubPhaseIds: [5.1, 3.3, 4.1]

  Teaching: Three predicates produce three independent routing decisions. The primary
  sub-phase is 5.1 (TEST, the dominant intent), but all three routing decisions are valid
  and none is collapsed or suppressed.

---

## Pair B20B1-WDA-router-003 — TEST + SECURITY + IMPLEMENT → three routing decisions

```
source_id: xiigen-b20-router-003
transition_type: feature_addition
phase: 5
sub_phase: 5.1
pair_kind: router
```

A:
  compiled_predicate_ids: ["TEST", "SECURITY", "IMPLEMENT"]
  compiled_text: "Write the test suite for the authentication service and verify the JWT token validation logic."

B:
  RoutingDecisions:
    - predicate: TEST
      sub_phase: 5.1
      sub_phase_label: "Unit and Integration Testing"
      confidence: 0.92
      routing_note: "'test suite' + 'verify' → TEST → 5.1"
    - predicate: SECURITY
      sub_phase: 5.2
      sub_phase_label: "Security and Compliance Testing"
      confidence: 0.92
      routing_note: "'authentication' + 'JWT' + 'token validation' → SECURITY → 5.2"
    - predicate: IMPLEMENT
      sub_phase: 4.1
      sub_phase_label: "Service and Logic Implementation"
      confidence: 0.92
      routing_note: "'write' → IMPLEMENT → 4.1"

  TransitionType: feature_addition
  PrimarySubPhaseId: 5.1
  AllSubPhaseIds: [5.1, 5.2, 4.1]

  Sub-phase note: SECURITY in a testing context routes to 5.2 (Security and Compliance
  Testing), not to a Phase 1-3 design sub-phase. The test suite framing shifts SECURITY
  routing to the testing phase.
