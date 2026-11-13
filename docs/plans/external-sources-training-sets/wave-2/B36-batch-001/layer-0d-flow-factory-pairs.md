# Layer 0-D — Flow Factory Selection Pairs
# Batch: B36-batch-001
# Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Contract: ILogicalFlowFactory<TInput> — flow factory selection
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Method note

For pure project planning (gap vocabulary, no predicate), there is no
sub-phase to select a flow for. The factory receives a knowledge-edge
routing result and returns the knowledge-edge synthetic flow, not a
real development flow. This is a distinct factory outcome.

---

## Pair B36B1-WDA-factory-001 — knowledge-edge synthetic flow: no sub-phase registered

```
source_id: xiigen-b36-factory-001
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: null
trust_state: knowledge_edge
pair_kind: factory_selection
```

A:
  sub_phase_id: null
  knowledge_edge: true
  failure_classification: "no_predicate_match_capability_gap"
  capability_gap_id: "gap-project-planning-sprint-roadmap-001"
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
    - "no_sub_phase_registered_for_input: project planning vocabulary"
    - "capability_gap_id: gap-project-planning-sprint-roadmap-001"
    - "SCHEDULE_CAPABILITY_LEARNING: no registered flow will execute"
  SelectionOutcome: "KNOWLEDGE_EDGE_SYNTHETIC_FLOW_SELECTED"

Factory logic:
  When KnowledgeEdge: true reaches the factory with sub_phase_id: null, the factory
  selects the knowledge-edge synthetic flow. This is not a fallback — it is the
  only valid selection when no sub-phase was identified by the router. The output
  triggers capability learning, not real flow execution.

---

## Pair B36B1-WDA-factory-002 — deterministic_generic: 2.4 for DOCUMENT-routed planning request

```
source_id: xiigen-b36-factory-002
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

## Pair B36B1-WDA-factory-003 — probationary_rejected: 4.1 candidate for IMPLEMENT-routed planning

```
source_id: xiigen-b36-factory-003
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "4.1"
trust_state: probationary_rejected
pair_kind: factory_selection
```

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: "backend-service-flow-4.1-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 5 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "backend-service-flow-4.1-candidate"
  Warnings:
    - "probationary_flow_rejected:backend-service-flow-4.1-candidate"
    - "gate_b_failed:insufficient_evidence_pairs:5_of_9_nodes_covered"
    - "fallback_used:universal-nine-node-development-flow-4.1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

---

## Pair B36B1-WDA-factory-004 — trusted_learned: 3.4 security flow (reference)

```
source_id: xiigen-b36-factory-004
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

B:
  SelectedFlowId: "security-verification-flow-3.4"
  TrustState: "trusted_learned"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"
