# Layer 0-D — Flow Factory Selection Pairs (005)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 005 covers probationary_rejected: 2.1 candidate fails Gate B → fallback used.

---

## B06-0D-005-001 — probationary_rejected: insufficient node coverage

```
source_id: xiigen-b06-0d-005-001
source_title: XIIGen B06 factory — probationary_rejected insufficient nodes
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "2.1"
trust_state: probationary_rejected
pair_kind: factory_selection
```

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: "requirements-authoring-flow-2.1-candidate-v1"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_node_coverage: 6 of 9 nodes covered; nodes N07 N08 N09 missing"
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "requirements-authoring-flow-2.1-candidate-v1"
  Warnings:
    - "probationary_flow_rejected:requirements-authoring-flow-2.1-candidate-v1"
    - "gate_b_failed:insufficient_node_coverage:6_of_9_nodes_covered"
    - "missing_nodes:N07,N08,N09"
    - "fallback_used:universal-nine-node-development-flow-2.1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

Factory selection rationale:
  Nodes N07 (ArbiterSelectorUnit), N08 (ArbiterUnit), N09 (FormatterUnit) are missing.
  The arbiter review and output formatting are not covered. Gate B fails.
  Fallback to universal-nine-node-development-flow-2.1.
  Warnings emitted so coverage gap is scheduled for remediation.

---

## B06-0D-005-002 — probationary_rejected: low confidence

```
source_id: xiigen-b06-0d-005-002
source_title: XIIGen B06 factory — probationary_rejected low confidence
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "2.1"
trust_state: probationary_rejected
pair_kind: factory_selection
```

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: "requirements-authoring-flow-2.1-candidate-low-conf"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "min_confidence_below_threshold: min_confidence: 0.55 < 0.70"
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "requirements-authoring-flow-2.1-candidate-low-conf"
  Warnings:
    - "probationary_flow_rejected:requirements-authoring-flow-2.1-candidate-low-conf"
    - "gate_b_failed:min_confidence_below_threshold:0.55_lt_0.70"
    - "fallback_used:universal-nine-node-development-flow-2.1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

---

## B06-0D-005-003 — probationary_rejected vs probationary_accepted comparison

```
source_id: xiigen-b06-0d-005-003
source_title: XIIGen B06 factory — probationary rejected vs accepted comparison
source_license_state: first_party
pair_kind: factory_selection
```

Teaching note — probationary_rejected vs probationary_accepted:

  Both states represent a candidate flow in probationary state.

  probationary_rejected:
    gate_b_passed: false
    SelectedFlowId: fallback (universal-nine-node-development-flow-2.1)
    FallbackUsed: true
    SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"
    Why: Candidate fails Gate B → not safe to use → fallback mandatory

  probationary_accepted:
    gate_b_passed: true (but < 25 pairs)
    SelectedFlowId: candidate flow
    FallbackUsed: false
    SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"
    Why: Candidate passes Gate B → safe to use under monitoring

  Key distinction: gate_b_passed is the deciding factor.
  If gate_b_passed is false → always rejected → always fallback.
  If gate_b_passed is true → accepted unless promotion threshold met.
