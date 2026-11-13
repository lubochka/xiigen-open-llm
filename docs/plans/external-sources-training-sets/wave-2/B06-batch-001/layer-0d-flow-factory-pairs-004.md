# Layer 0-D — Flow Factory Selection Pairs (004)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 004 covers trusted_learned: sub-phase 2.1 promoted to trusted_learned state.

---

## B06-0D-004-001 — trusted_learned: 2.1 flow promoted

```
source_id: xiigen-b06-0d-004-001
source_title: XIIGen B06 factory — trusted_learned 2.1 promoted
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "2.1"
trust_state: trusted_learned
pair_kind: factory_selection
```

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: "requirements-authoring-flow-2.1-v1"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.91; production_evidence_pairs: 31"
  fallback_flow_id: "universal-nine-node-development-flow-2.1"
  promotion_threshold_met: true
  promotion_threshold_reason: "production_evidence_pairs: 31 of 25 required — threshold exceeded"

B:
  SelectedFlowId: "requirements-authoring-flow-2.1-v1"
  TrustState: "trusted_learned"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

Factory selection rationale:
  Learned flow for 2.1 has been promoted to trusted_learned (31 >= 25 pairs).
  Gate B passed: all 9 nodes covered, min_confidence 0.91. No warnings.
  The learned flow replaces the deterministic generic flow entirely.

---

## B06-0D-004-002 — trusted_learned: 2.1 with high evidence (50 pairs)

```
source_id: xiigen-b06-0d-004-002
source_title: XIIGen B06 factory — trusted_learned 2.1 high evidence
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "2.1"
trust_state: trusted_learned
pair_kind: factory_selection
```

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: "requirements-authoring-flow-2.1-v2"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.93; production_evidence_pairs: 50"
  promotion_threshold_met: true
  promotion_threshold_reason: "production_evidence_pairs: 50 of 25 required"
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "requirements-authoring-flow-2.1-v2"
  TrustState: "trusted_learned"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

---

## B06-0D-004-003 — trusted_learned: trust state progression

```
source_id: xiigen-b06-0d-004-003
source_title: XIIGen B06 factory — trust state progression
source_license_state: first_party
pair_kind: factory_selection
```

Teaching note — trust state progression:

  A flow for a sub-phase progresses through states:

  1. deterministic_generic (no registered flow)
     → SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
     → FallbackUsed: false (this is the primary path, not a fallback)

  2a. probationary_rejected (candidate fails Gate B)
     → SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"
     → FallbackUsed: true (deterministic generic used as fallback)

  2b. probationary_accepted (candidate passes Gate B, < 25 pairs)
     → SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"
     → FallbackUsed: false (candidate is used, not the fallback)
     → Warnings present (monitoring required)

  3. trusted_learned (candidate promoted, >= 25 pairs)
     → SelectionOutcome: "TRUSTED_LEARNED_SELECTED"
     → FallbackUsed: false
     → Warnings: [] (no warnings for trusted flow)

  Note: probationary_rejected always results in FallbackUsed: true.
  probationary_accepted results in FallbackUsed: false (candidate used).
  trusted_learned results in FallbackUsed: false (promoted flow used).
