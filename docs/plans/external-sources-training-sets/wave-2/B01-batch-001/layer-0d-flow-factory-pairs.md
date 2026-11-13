# Layer 0-D — Flow Factory Selection Pairs
# Batch: B01-batch-001
# Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Contract: ILogicalFlowFactory<TInput> — flow factory selection
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Method note

Layer 0-D pairs teach the flow factory selection logic. The factory selects
a flow for a given sub-phase based on the trust state of any registered
candidate flow. Four trust states exist:

  trusted_learned     — promoted, Gate B passed, used in production
  deterministic_generic — no learned flow; universal nine-node flow is used
  probationary_rejected — candidate exists but failed Gate B; fallback used
  probationary_accepted — candidate passed Gate B but not yet promoted; used with monitoring

The factory must NEVER promote a probationary_rejected flow.
The factory must NEVER use a trusted_learned flow from the wrong sub-phase.

---

## Pair B01B1-WDA-factory-001 — deterministic_generic: 1.1 has no learned flow

```
source_id: xiigen-b01-factory-001
source_title: XIIGen B01 factory — deterministic_generic for sub-phase 1.1
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "1.1"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
    gate_b_passed: null
    gate_b_reason: null
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

Factory logic:
  No learned flow is registered for sub-phase 1.1. The universal nine-node
  development flow is the deterministic fallback. FallbackUsed: false because
  this is not a fallback from a rejected candidate — it is the primary selection
  in the absence of any learned flow.

---

## Pair B01B1-WDA-factory-002 — probationary_rejected: 1.1 candidate fails Gate B

```
source_id: xiigen-b01-factory-002
source_title: XIIGen B01 factory — probationary_rejected: 1.1 candidate fails Gate B
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "1.1"
trust_state: probationary_rejected
pair_kind: factory_selection
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "screen-inventory-flow-1.1-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 3 of 9 nodes covered; minimum required: 9"
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "screen-inventory-flow-1.1-candidate"
  Warnings:
    - "probationary_flow_rejected:screen-inventory-flow-1.1-candidate"
    - "gate_b_failed:insufficient_evidence_pairs:3_of_9_nodes_covered"
    - "fallback_used:universal-nine-node-development-flow-1.1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

Factory logic:
  A candidate flow exists for 1.1 but has not passed Gate B (only 3 of 9
  nodes have sufficient evidence pairs). The factory rejects it and falls back
  to the safe deterministic generic flow. Rejection warnings are emitted to
  notify the learning pipeline. The model must NOT mark gate_b_passed: true
  for a candidate that has not passed — that is Corruption-v6-02.

---

## Pair B01B1-WDA-factory-003 — probationary_accepted: 1.1 candidate passes Gate B

```
source_id: xiigen-b01-factory-003
source_title: XIIGen B01 factory — probationary_accepted: 1.1 candidate passes Gate B
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "1.1"
trust_state: probationary_accepted
pair_kind: factory_selection
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "screen-inventory-flow-1.1-candidate-v2"
    trust_state: "probationary"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.88; production_evidence_pairs: 12"
  fallback_flow_id: "universal-nine-node-development-flow-1.1"
  promotion_threshold_met: false
  promotion_threshold_reason: "production_evidence_pairs: 12 of 25 required"

B:
  SelectedFlowId: "screen-inventory-flow-1.1-candidate-v2"
  TrustState: "probationary_accepted"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings:
    - "probationary_flow_in_use:screen-inventory-flow-1.1-candidate-v2"
    - "promotion_threshold_not_met:12_of_25_production_evidence_pairs"
    - "monitoring_required: compare output quality against universal fallback"
  SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"

Factory logic:
  The candidate passed Gate B (all 9 nodes have coverage, min confidence 0.88).
  It is used — with monitoring warnings — because it has passed the structural
  gate. However, it has not yet reached the production evidence threshold for
  full promotion to trusted_learned. The model must distinguish this from
  probationary_rejected (Gate B failed) and trusted_learned (fully promoted).

---

## Pair B01B1-WDA-factory-004 — trusted_learned: 3.4 security flow (fully promoted)

```
source_id: xiigen-b01-factory-004
source_title: XIIGen B01 factory — trusted_learned for sub-phase 3.4 security flow
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

Factory logic:
  The security verification flow for 3.4 is fully promoted to trusted_learned.
  Gate B passed, promotion threshold exceeded, no warnings required. The factory
  uses it directly without considering the generic fallback. This is the target
  state for all sub-phase flows — it is the only state where Warnings is empty
  and SelectionOutcome is TRUSTED_LEARNED_SELECTED.
