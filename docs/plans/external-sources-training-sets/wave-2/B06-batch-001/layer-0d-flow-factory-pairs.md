# Layer 0-D — Flow Factory Selection Pairs
# Batch: B06-batch-001
# Scenario: B06 — Checkout flow business requirements and use cases
# Contract: ILogicalFlowFactory<TInput> — flow factory selection
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Pair B06B1-WDA-factory-001 — deterministic_generic: 2.1 has no learned flow

```
source_id: xiigen-b06-factory-001
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "2.1"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
    gate_b_passed: null
    gate_b_reason: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## Pair B06B1-WDA-factory-002 — probationary_rejected: 2.1 candidate fails Gate B

```
source_id: xiigen-b06-factory-002
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "2.1"
trust_state: probationary_rejected
pair_kind: factory_selection
```

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: "requirements-authoring-flow-2.1-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 4 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "requirements-authoring-flow-2.1-candidate"
  Warnings:
    - "probationary_flow_rejected:requirements-authoring-flow-2.1-candidate"
    - "gate_b_failed:insufficient_evidence_pairs:4_of_9_nodes_covered"
    - "fallback_used:universal-nine-node-development-flow-2.1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

---

## Pair B06B1-WDA-factory-003 — probationary_accepted: 2.1 candidate passes Gate B

```
source_id: xiigen-b06-factory-003
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "2.1"
trust_state: probationary_accepted
pair_kind: factory_selection
```

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: "requirements-authoring-flow-2.1-candidate-v2"
    trust_state: "probationary"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.87; production_evidence_pairs: 9"
  fallback_flow_id: "universal-nine-node-development-flow-2.1"
  promotion_threshold_met: false
  promotion_threshold_reason: "production_evidence_pairs: 9 of 25 required"

B:
  SelectedFlowId: "requirements-authoring-flow-2.1-candidate-v2"
  TrustState: "probationary_accepted"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings:
    - "probationary_flow_in_use:requirements-authoring-flow-2.1-candidate-v2"
    - "promotion_threshold_not_met:9_of_25_production_evidence_pairs"
    - "monitoring_required: compare output quality against universal fallback"
  SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"

---

## Pair B06B1-WDA-factory-004 — trusted_learned: 3.4 security flow (reference case)

```
source_id: xiigen-b06-factory-004
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "3.4"
trust_state: trusted_learned
pair_kind: factory_selection
```

A:
  sub_phase_id: "3.4"
  flow_registry_state:
    registered_flow_id: "security-verification-flow-3.4"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.92; production_evidence_pairs: 31"
  fallback_flow_id: "universal-nine-node-development-flow-3.4"
  promotion_threshold_met: true
  promotion_threshold_reason: "production_evidence_pairs: 31 of 25 required — threshold exceeded"

B:
  SelectedFlowId: "security-verification-flow-3.4"
  TrustState: "trusted_learned"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"
