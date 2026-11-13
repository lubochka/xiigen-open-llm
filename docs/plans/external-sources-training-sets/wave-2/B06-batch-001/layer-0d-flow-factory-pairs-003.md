# Layer 0-D — Flow Factory Selection Pairs (003)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 003 covers probationary_accepted: 2.1 candidate passes Gate B.
The candidate flow is used but not yet promoted to trusted_learned.

---

## B06-0D-003-001 — probationary_accepted: 2.1 candidate passes Gate B

```
source_id: xiigen-b06-0d-003-001
source_title: XIIGen B06 factory — probationary_accepted Gate B passed
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
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.87; production_evidence_pairs: 12"
  fallback_flow_id: "universal-nine-node-development-flow-2.1"
  promotion_threshold_met: false
  promotion_threshold_reason: "production_evidence_pairs: 12 of 25 required"

B:
  SelectedFlowId: "requirements-authoring-flow-2.1-candidate-v2"
  TrustState: "probationary_accepted"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings:
    - "probationary_flow_in_use:requirements-authoring-flow-2.1-candidate-v2"
    - "promotion_threshold_not_met:12_of_25_production_evidence_pairs"
    - "monitoring_required: compare output quality against universal fallback"
  SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"

Factory selection rationale:
  Candidate flow passed Gate B (all 9 nodes covered, min confidence >= 0.87).
  However promotion threshold (25 production evidence pairs) not met at 12 pairs.
  Flow is used in probationary state — output is monitored. FallbackUsed false
  because the candidate was accepted (Gate B passed), not rejected.

---

## B06-0D-003-002 — probationary_accepted: lower threshold (9 pairs)

```
source_id: xiigen-b06-0d-003-002
source_title: XIIGen B06 factory — probationary_accepted 9 pairs
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "2.1"
trust_state: probationary_accepted
pair_kind: factory_selection
```

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: "requirements-authoring-flow-2.1-candidate-v3"
    trust_state: "probationary"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.90; production_evidence_pairs: 9"
  promotion_threshold_met: false
  promotion_threshold_reason: "production_evidence_pairs: 9 of 25 required"
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "requirements-authoring-flow-2.1-candidate-v3"
  TrustState: "probationary_accepted"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings:
    - "probationary_flow_in_use:requirements-authoring-flow-2.1-candidate-v3"
    - "promotion_threshold_not_met:9_of_25_production_evidence_pairs"
    - "monitoring_required: compare output quality against universal fallback"
  SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"

---

## B06-0D-003-003 — Gate B criteria for probationary acceptance

```
source_id: xiigen-b06-0d-003-003
source_title: XIIGen B06 factory — Gate B criteria for probationary acceptance
source_license_state: first_party
pair_kind: factory_selection
```

Teaching note — Gate B criteria for probationary acceptance:

  A candidate flow is accepted (probationary_accepted) when:
    1. All 9 nodes are covered (gate_b_passed: true)
    2. min_confidence >= 0.70 (typically >= 0.87 for strong acceptance)
    3. production_evidence_pairs >= 9 (minimum for Gate B pass)

  A candidate flow is promoted (trusted_learned) when:
    1. Gate B passed (as above)
    2. production_evidence_pairs >= 25

  A candidate flow is rejected (probationary_rejected) when:
    1. gate_b_passed: false
    2. Insufficient node coverage OR confidence below threshold

  The promotion threshold of 25 pairs is a hard gate. Even if a probationary
  flow performs well, it cannot be promoted until 25 production evidence
  pairs are accumulated.
