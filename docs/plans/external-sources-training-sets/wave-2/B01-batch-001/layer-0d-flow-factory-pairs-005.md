# Layer 0-D — Flow Factory Selection Pairs (005)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B01-0D-005-001 — probationary_rejected: 1.1 candidate fails Gate B

```
source_id: xiigen-b01-0d-005-001
source_title: XIIGen B01 factory-005 — probationary_rejected: Gate B fails (insufficient nodes)
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "1.1"
trust_state: probationary_rejected
pair_kind: factory_selection
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "ScreenInventoryFlow11-candidate-v1"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_node_coverage: 5 of 9 nodes covered; minimum required: 9"
  fallback_flow_id: "ScreenInventoryFlow11"

B:
  SelectedFlowId: "ScreenInventoryFlow11"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "ScreenInventoryFlow11-candidate-v1"
  Warnings:
    - "probationary_flow_rejected:ScreenInventoryFlow11-candidate-v1"
    - "gate_b_failed:insufficient_node_coverage:5_of_9_nodes_covered"
    - "fallback_used:ScreenInventoryFlow11"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

Factory logic:
  Gate B failed: only 5 of 9 nodes have coverage (nodes 01-05 covered; 06-09 missing).
  The candidate is rejected. ScreenInventoryFlow11 (generic) is used as fallback.
  FallbackUsed: true signals that a candidate was present but rejected.
  Three warnings are emitted to notify the learning pipeline to schedule missing node
  evidence pair creation. The model must not mark gate_b_passed: true here.

---

## B01-0D-005-002 — probationary_rejected: Gate B fails (low confidence)

```
source_id: xiigen-b01-0d-005-002
source_title: XIIGen B01 factory-005 — probationary_rejected: Gate B fails (low confidence)
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "1.1"
trust_state: probationary_rejected
pair_kind: factory_selection
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "ScreenInventoryFlow11-candidate-v2"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "confidence_below_threshold: min_confidence 0.71; required: 0.88"
  fallback_flow_id: "ScreenInventoryFlow11"

B:
  SelectedFlowId: "ScreenInventoryFlow11"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "ScreenInventoryFlow11-candidate-v2"
  Warnings:
    - "probationary_flow_rejected:ScreenInventoryFlow11-candidate-v2"
    - "gate_b_failed:confidence_below_threshold:0.71_of_0.88_required"
    - "fallback_used:ScreenInventoryFlow11"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

Factory logic:
  All 9 nodes are covered but one node has confidence 0.71, below the 0.88 threshold.
  Gate B fails on the confidence assertion. The candidate is rejected.
  SelectionOutcome is identical to the node-coverage failure: REJECTED_PROBATIONARY_FALLBACK_USED.
  The learning pipeline should schedule additional evidence pairs for the low-confidence node.

---

## B01-0D-005-003 — Fallback semantics: FallbackUsed true vs false

```
source_id: xiigen-b01-0d-005-003
source_title: XIIGen B01 factory-005 — FallbackUsed semantics
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

FallbackUsed semantics:

  FallbackUsed: false (two cases):
    Case 1 — deterministic_generic: No candidate exists; generic flow is primary selection
      SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
      Meaning: "We are using generic because nothing better was registered, not because
                something was tried and failed."

    Case 2 — trusted_learned: Promoted flow used directly
      SelectionOutcome: "TRUSTED_LEARNED_SELECTED"
      Meaning: "We are using the best available flow."

    Case 3 — probationary_accepted: Candidate used with monitoring
      SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"
      FallbackUsed: false — the candidate IS the selected flow
      Meaning: "We are using the candidate because it passed Gate B."

  FallbackUsed: true (one case):
    Case — probationary_rejected: Candidate failed Gate B; generic flow substituted
      SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"
      Meaning: "A candidate was registered but rejected; falling back to generic."
      RejectedCandidateFlowId: must contain the rejected candidate's ID

  Rule: FallbackUsed: true ALWAYS co-occurs with:
    - RejectedCandidateFlowId is non-null
    - Warnings array contains at least two entries
    - SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"
