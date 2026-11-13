# Layer 0-D — Flow Factory Selection Pairs
# Batch: B37-batch-001
# Scenario: B37 — Project managing — progress, coordination, and status reporting
# Contract: ILogicalFlowFactory<TInput> — flow factory selection
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Method note

For pure project managing (gap vocabulary, no predicate), the factory receives
a knowledge-edge routing result and returns the knowledge-edge synthetic flow.
For mixed-signal requests (IMPLEMENT or DOCUMENT co-occurring with gap vocab),
the factory selects a flow for the routed sub-phase.

---

## Pair B37B1-WDA-factory-001 — knowledge-edge synthetic flow: null sub-phase

```
source_id: xiigen-b37-factory-001
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: null
trust_state: knowledge_edge_synthetic
pair_kind: factory_selection
```

A:
  sub_phase_id: null
  knowledge_edge: true
  failure_classification: "no_predicate_match_capability_gap"
  capability_gap_id: "gap-project-managing-coordinate-blockers-001"
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
    - "no_sub_phase_registered_for_input: project managing vocabulary"
    - "capability_gap_id: gap-project-managing-coordinate-blockers-001"
    - "SCHEDULE_CAPABILITY_LEARNING: no registered flow will execute"
  SelectionOutcome: "KNOWLEDGE_EDGE_SYNTHETIC_FLOW_SELECTED"

Factory logic:
  KnowledgeEdge: true with sub_phase_id: null → knowledge-edge synthetic flow.
  This is not a fallback — it is the only valid selection when no sub-phase
  was identified. The capability gap vocabulary triggered SCHEDULE_CAPABILITY_LEARNING.

---

## Pair B37B1-WDA-factory-002 — deterministic_generic: 2.4 for DOCUMENT-routed managing request

```
source_id: xiigen-b37-factory-002
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "2.4"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "2.4"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
    gate_b_passed: null
    gate_b_reason: null
  fallback_flow_id: "universal-nine-node-development-flow-2.4"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.4"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## Pair B37B1-WDA-factory-003 — deterministic_generic: 4.1 for IMPLEMENT-routed managing request

```
source_id: xiigen-b37-factory-003
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "4.1"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
    gate_b_passed: null
    gate_b_reason: null
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## Pair B37B1-WDA-factory-004 — probationary_rejected: 4.1 candidate fails Gate B

```
source_id: xiigen-b37-factory-004
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "4.1"
trust_state: probationary_rejected
pair_kind: factory_selection
```

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: "auth-service-flow-4.1-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 4 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "auth-service-flow-4.1-candidate"
  Warnings:
    - "probationary_flow_rejected:auth-service-flow-4.1-candidate"
    - "gate_b_failed:insufficient_evidence_pairs:4_of_9_nodes_covered"
    - "fallback_used:universal-nine-node-development-flow-4.1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"
