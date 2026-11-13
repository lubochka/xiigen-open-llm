# Layer 0-D — Flow Factory Selection Pairs
# Batch: B15-batch-001
# Scenario: B15 — Development from scratch — backend order processing service
# Contract: ILogicalFlowFactory<TInput>
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Pair B15B1-WDA-factory-001 — deterministic_generic: 4.1 (no learned flow)

```
source_id: xiigen-b15-factory-001
sub_phase_id: "4.1"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## Pair B15B1-WDA-factory-002 — probationary_accepted: 4.1 candidate passes Gate B

```
source_id: xiigen-b15-factory-002
sub_phase_id: "4.1"
trust_state: probationary_accepted
pair_kind: factory_selection
```

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: "backend-service-flow-4.1-v2"
    trust_state: "probationary"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.89; evidence_pairs: 11 of 25 required for promotion"
  fallback_flow_id: "universal-nine-node-development-flow-4.1"
  promotion_threshold_met: false

B:
  SelectedFlowId: "backend-service-flow-4.1-v2"
  TrustState: "probationary_accepted"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings:
    - "probationary_flow_accepted: gate_b_passed but promotion_threshold not yet met (11/25 evidence pairs)"
  SelectionOutcome: "PROBATIONARY_ACCEPTED"

---

## Pair B15B1-WDA-factory-003 — probationary_rejected: insufficient node coverage

```
source_id: xiigen-b15-factory-003
sub_phase_id: "4.1"
trust_state: probationary_rejected
pair_kind: factory_selection
```

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: "backend-service-flow-4.1-v1"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 7 of 9 nodes covered; N07 and N08 missing"
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "backend-service-flow-4.1-v1"
  Warnings:
    - "probationary_flow_rejected:backend-service-flow-4.1-v1"
    - "gate_b_failed:N07_and_N08_not_covered"
    - "fallback_used:universal-nine-node-development-flow-4.1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

---

## Pair B15B1-WDA-factory-004 — trusted_learned: 4.1 promoted flow (reference)

```
source_id: xiigen-b15-factory-004
sub_phase_id: "4.1"
trust_state: trusted_learned
pair_kind: factory_selection
```

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: "ecommerce-backend-flow-4.1"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.93; production_evidence_pairs: 34"
  fallback_flow_id: "universal-nine-node-development-flow-4.1"
  promotion_threshold_met: true

B:
  SelectedFlowId: "ecommerce-backend-flow-4.1"
  TrustState: "trusted_learned"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"
