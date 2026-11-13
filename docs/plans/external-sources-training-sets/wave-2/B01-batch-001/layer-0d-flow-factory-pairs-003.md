# Layer 0-D — Flow Factory Selection Pairs (003)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B01-0D-003-001 — probationary_accepted: 1.1 candidate passes Gate B

```
source_id: xiigen-b01-0d-003-001
source_title: XIIGen B01 factory-003 — probationary_accepted for sub-phase 1.1
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "1.1"
trust_state: probationary_accepted
pair_kind: factory_selection
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "ScreenInventoryFlow11-candidate-v3"
    trust_state: "probationary"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.91; production_evidence_pairs: 18"
  fallback_flow_id: "ScreenInventoryFlow11"
  promotion_threshold_met: false
  promotion_threshold_reason: "production_evidence_pairs: 18 of 25 required"

B:
  SelectedFlowId: "ScreenInventoryFlow11-candidate-v3"
  TrustState: "probationary_accepted"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings:
    - "probationary_flow_in_use:ScreenInventoryFlow11-candidate-v3"
    - "promotion_threshold_not_met:18_of_25_production_evidence_pairs"
    - "monitoring_required: compare output quality against ScreenInventoryFlow11"
  SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"

Factory logic:
  The candidate flow ScreenInventoryFlow11-candidate-v3 has passed Gate B:
  all 9 nodes have evidence coverage and min_confidence is 0.91. It is used
  in production (FallbackUsed: false) but with monitoring warnings. It has
  not reached the 25-pair promotion threshold for trusted_learned promotion.
  The factory correctly issues three warnings and uses ACCEPTED_PROBATIONARY_GATE_B_PASSED.

---

## B01-0D-003-002 — probationary_accepted: candidate with exactly 25 pairs (on threshold)

```
source_id: xiigen-b01-0d-003-002
source_title: XIIGen B01 factory-003 — probationary_accepted at promotion threshold
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "1.1"
trust_state: probationary_accepted
pair_kind: factory_selection
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "ScreenInventoryFlow11-candidate-v3"
    trust_state: "probationary"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.91; production_evidence_pairs: 25"
  fallback_flow_id: "ScreenInventoryFlow11"
  promotion_threshold_met: true
  promotion_threshold_reason: "production_evidence_pairs: 25 of 25 required — threshold met exactly"

B:
  SelectedFlowId: "ScreenInventoryFlow11-candidate-v3"
  TrustState: "probationary_accepted"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings:
    - "probationary_flow_in_use:ScreenInventoryFlow11-candidate-v3"
    - "promotion_eligible: production_evidence_pairs threshold met; awaiting explicit promotion"
  SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"

Factory logic:
  Even at exactly 25 pairs (meeting the threshold), the flow remains probationary_accepted
  until an explicit promotion action is triggered. The factory does NOT auto-promote.
  A "promotion_eligible" warning is emitted. The TrustState only becomes trusted_learned
  after explicit PROMOTE_TO_TRUSTED action is taken by the learning pipeline.

---

## B01-0D-003-003 — Gate B criteria for ScreenInventoryFlow11 promotion

```
source_id: xiigen-b01-0d-003-003
source_title: XIIGen B01 factory-003 — Gate B promotion criteria for ScreenInventoryFlow11
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

Gate B assertion set for ScreenInventoryFlow11 promotion to probationary_accepted:

  Assertion 1: all_9_nodes_covered
    Check: All nodes 01-09 have at least one production evidence pair each
    Threshold: count == 9

  Assertion 2: min_confidence >= 0.88
    Check: Lowest confidence across all node pairs >= 0.88
    Threshold: value >= 0.88

  Assertion 3: production_evidence_pairs >= 12
    Check: Total evidence pairs across all nodes >= 12
    Threshold: count >= 12

  Assertion 4: no_failed_pairs
    Check: pairs_rejected == 0 across all node runs
    Threshold: count == 0

  All four assertions must pass simultaneously for Gate B to pass.
  Failing any single assertion keeps the flow in probationary state (or rejects it).

Gate B assertion set for trusted_learned promotion:
  All four above PLUS:
  Assertion 5: production_evidence_pairs >= 25
  Assertion 6: explicit_promotion_action received from learning pipeline
