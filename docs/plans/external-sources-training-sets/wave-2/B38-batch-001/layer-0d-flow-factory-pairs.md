# Layer 0-D — Flow Factory Selection Pairs
# Batch: B38-batch-001
# Scenario: B38 — Project delivery — release gates and go-live checklists
# Contract: ILogicalFlowFactory<TInput> — flow factory selection
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Pair B38B1-WDA-factory-001 — knowledge-edge synthetic: null sub-phase

```
source_id: xiigen-b38-factory-001
sub_phase_id: null
trust_state: knowledge_edge_synthetic
pair_kind: factory_selection
```

A:
  sub_phase_id: null
  knowledge_edge: true
  failure_classification: "no_predicate_match_capability_gap"
  capability_gap_id: "gap-project-delivery-rollout-golive-001"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
  fallback_flow_id: null

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings:
    - "no_sub_phase_registered_for_input: project delivery gap vocabulary"
    - "SCHEDULE_CAPABILITY_LEARNING: no registered flow will execute"
  SelectionOutcome: "KNOWLEDGE_EDGE_SYNTHETIC_FLOW_SELECTED"

---

## Pair B38B1-WDA-factory-002 — deterministic_generic: 7.1

```
source_id: xiigen-b38-factory-002
sub_phase_id: "7.1"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "7.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
  fallback_flow_id: "universal-nine-node-development-flow-7.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-7.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## Pair B38B1-WDA-factory-003 — probationary_rejected: 7.1 candidate fails Gate B

```
source_id: xiigen-b38-factory-003
sub_phase_id: "7.1"
trust_state: probationary_rejected
pair_kind: factory_selection
```

A:
  sub_phase_id: "7.1"
  flow_registry_state:
    registered_flow_id: "release-gate-flow-7.1-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 6 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-7.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-7.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "release-gate-flow-7.1-candidate"
  Warnings:
    - "probationary_flow_rejected:release-gate-flow-7.1-candidate"
    - "gate_b_failed:insufficient_evidence_pairs:6_of_9_nodes_covered"
    - "fallback_used:universal-nine-node-development-flow-7.1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

---

## Pair B38B1-WDA-factory-004 — trusted_learned: 6.3 (reference)

```
source_id: xiigen-b38-factory-004
sub_phase_id: "6.3"
trust_state: trusted_learned
pair_kind: factory_selection
```

A:
  sub_phase_id: "6.3"
  flow_registry_state:
    registered_flow_id: "security-compliance-flow-6.3"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.91; production_evidence_pairs: 28"
  fallback_flow_id: "universal-nine-node-development-flow-6.3"
  promotion_threshold_met: true

B:
  SelectedFlowId: "security-compliance-flow-6.3"
  TrustState: "trusted_learned"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"
