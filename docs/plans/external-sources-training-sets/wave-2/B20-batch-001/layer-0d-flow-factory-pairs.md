# Layer 0-D — Flow Factory Pairs
# Batch: B20-batch-001
# Scenario: B20 — Unit testing, sub-phase 5.1
# Contract: FlowFactoryUnit : ILogicalUnit<SubPhaseRouterResult, FlowInstanceDescriptor>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Pair B20B1-WDA-factory-001 — TEST / 5.1 → deterministic_generic

```
source_id: xiigen-b20-factory-001
transition_type: feature_addition
phase: 5
sub_phase: 5.1
pair_kind: factory_selection
```

A:
  sub_phase: 5.1
  predicate: TEST
  compiled_text: "Write unit tests for the PaymentCalculationService covering all edge cases."

B:
  TrustState: deterministic_generic
  FlowId: "UnitTestingFlow51"
  FlowLabel: "Unit and Integration Testing Flow — sub-phase 5.1"
  SelectionReason: "TEST predicate at 5.1 with feature_addition transition — established
    flow pattern; deterministic_generic"
  NodeCount: 9

---

## Pair B20B1-WDA-factory-002 — TEST + API_CONTRACT + IMPLEMENT → three flows

```
source_id: xiigen-b20-factory-002
transition_type: feature_addition
phase: 5
sub_phase: 5.1
pair_kind: factory_selection
```

A:
  sub_phase: [5.1, 3.3, 4.1]
  predicate: ["TEST", "API_CONTRACT", "IMPLEMENT"]
  compiled_text: "Add integration tests for the OrderService API contract and mock the external payment interface."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
    - FlowId: "ApiContractDesignFlow33"
      predicate: API_CONTRACT
      sub_phase: 3.3
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
  SelectionReason: "Three predicates → three independent flow instances; all deterministic_generic"

---

## Pair B20B1-WDA-factory-003 — TEST + SECURITY + IMPLEMENT → three flows

```
source_id: xiigen-b20-factory-003
transition_type: feature_addition
phase: 5
sub_phase: 5.1
pair_kind: factory_selection
```

A:
  sub_phase: [5.1, 5.2, 4.1]
  predicate: ["TEST", "SECURITY", "IMPLEMENT"]
  compiled_text: "Write the test suite for the authentication service and verify the JWT token validation logic."

B:
  TrustState: deterministic_generic
  FlowInstances:
    - FlowId: "UnitTestingFlow51"
      predicate: TEST
      sub_phase: 5.1
    - FlowId: "SecurityTestingFlow52"
      predicate: SECURITY
      sub_phase: 5.2
    - FlowId: "BackendServiceFlow41"
      predicate: IMPLEMENT
      sub_phase: 4.1
  SelectionReason: "Three predicates → three independent flow instances; all deterministic_generic"

---

## Pair B20B1-WDA-factory-004 — zero predicates ("check") → knowledge_edge_synthetic

```
source_id: xiigen-b20-factory-004
transition_type: unknown
phase: null
sub_phase: null
pair_kind: factory_selection
```

A:
  sub_phase: null
  predicate: null
  compiled_text: "Check that the login works correctly before the release."
  capability_gap_signal:
    gap_id: "gap-testing-informal-check-001"
    DomainId: "software-development"

B:
  TrustState: knowledge_edge_synthetic
  FlowId: null
  FlowLabel: "No flow instantiated — capability gap"
  SelectionReason: "Zero predicates → knowledge_edge_synthetic; 'check' is not in TEST lexicon"
  NodeCount: 0
  CapabilityGapSignalForwarded: true
