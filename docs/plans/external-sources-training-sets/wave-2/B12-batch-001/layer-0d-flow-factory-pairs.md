# Layer 0-D — Flow Factory Selection Pairs
# Batch: B12-batch-001
# Scenario: B12 — Development — API contract documentation
# Contract: ILogicalFlowFactory<TInput>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Pair B12B1-WDA-factory-001 — deterministic_generic: 3.3

```
source_id: xiigen-b12-factory-001
sub_phase_id: "3.3"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "3.3"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
  fallback_flow_id: "universal-nine-node-development-flow-3.3"

B:
  SelectedFlowId: "universal-nine-node-development-flow-3.3"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## Pair B12B1-WDA-factory-002 — probationary_rejected: 3.3 candidate fails Gate B

```
source_id: xiigen-b12-factory-002
sub_phase_id: "3.3"
trust_state: probationary_rejected
pair_kind: factory_selection
```

A:
  sub_phase_id: "3.3"
  flow_registry_state:
    registered_flow_id: "rest-api-contract-flow-3.3-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 5 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-3.3"

B:
  SelectedFlowId: "universal-nine-node-development-flow-3.3"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "rest-api-contract-flow-3.3-candidate"
  Warnings:
    - "probationary_flow_rejected:rest-api-contract-flow-3.3-candidate"
    - "gate_b_failed:insufficient_evidence_pairs:5_of_9_nodes_covered"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

---

## Pair B12B1-WDA-factory-003 — trusted_learned: 3.3 (promoted flow)

```
source_id: xiigen-b12-factory-003
sub_phase_id: "3.3"
trust_state: trusted_learned
pair_kind: factory_selection
```

A:
  sub_phase_id: "3.3"
  flow_registry_state:
    registered_flow_id: "api-contract-design-flow-3.3"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.91; production_evidence_pairs: 29"
  promotion_threshold_met: true

B:
  SelectedFlowId: "api-contract-design-flow-3.3"
  TrustState: "trusted_learned"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

---

## Pair B12B1-WDA-factory-004 — deterministic_generic: 3.4 (SECURITY routing from pair-001)

```
source_id: xiigen-b12-factory-004
sub_phase_id: "3.4"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "3.4"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
  fallback_flow_id: "universal-nine-node-development-flow-3.4"

B:
  SelectedFlowId: "universal-nine-node-development-flow-3.4"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
