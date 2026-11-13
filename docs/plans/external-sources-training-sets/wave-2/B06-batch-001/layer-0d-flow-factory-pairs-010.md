# Layer 0-D — Flow Factory Selection Pairs (010)
# Batch: B06-batch-001 | Scenario: B06 — Checkout flow business requirements and use cases
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## Method note

File 010 covers v6 factory corruption: wrong flow selected for DOCUMENT predicate.

---

## B06-0D-010-001 — v6 factory corruption: wrong flow selected for 2.1

```
source_id: xiigen-b06-0d-010-001
source_title: XIIGen B06 factory — v6 corruption wrong flow selected
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
pair_kind: factory_selection
```

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"
  routing_predicate: "DOCUMENT"

B-correct:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

B-corrupt (wrong flow):
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

Corruption: Wrong flow selected — 4.1 instead of 2.1.
Why harmful: The ImplementationFlow41 nine-node chain runs instead of
  RequirementsAuthoringFlow21. Instead of authoring use cases and acceptance
  criteria, the system generates implementation code for undefined requirements.
  The requirements artifact is never created.
Fix: Sub-phase 2.1 must map to RequirementsAuthoringFlow21
  (universal-nine-node-development-flow-2.1). The DOCUMENT predicate and
  2.1 sub-phase together always select the 2.1 flow.

---

## B06-0D-010-002 — v6 factory corruption: probationary rejected but used anyway

```
source_id: xiigen-b06-0d-010-002
source_title: XIIGen B06 factory — v6 corruption rejected flow used
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
pair_kind: factory_selection
```

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: "requirements-authoring-flow-2.1-bad-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 4 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B-correct:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "requirements-authoring-flow-2.1-bad-candidate"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

B-corrupt (rejected flow used):
  SelectedFlowId: "requirements-authoring-flow-2.1-bad-candidate"
  TrustState: "probationary"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "ACCEPTED_PROBATIONARY_GATE_B_PASSED"

Corruption: A flow with gate_b_passed: false was used as if Gate B had passed.
Why harmful: 5 missing nodes produce absent or generic outputs. The requirements
  document is incomplete. The arbiter review never runs (N07/N08 missing).
  Warnings are suppressed so the gap is never scheduled for remediation.
Fix: gate_b_passed: false always means probationary_rejected → always fallback.

---

## B06-0D-010-003 — Complete flow factory selection outcome table

```
source_id: xiigen-b06-0d-010-003
source_title: XIIGen B06 factory — complete selection outcome table
source_license_state: first_party
pair_kind: factory_selection
```

B06 Flow Factory Selection Outcome Table:

  Registry state                              | SelectionOutcome                    | FallbackUsed
  --------------------------------------------|-------------------------------------|-------------
  No registered flow                          | DETERMINISTIC_GENERIC_SELECTED      | false
  Probationary + Gate B failed                | REJECTED_PROBATIONARY_FALLBACK_USED | true
  Probationary + Gate B passed + < 25 pairs   | ACCEPTED_PROBATIONARY_GATE_B_PASSED | false
  Trusted learned (>= 25 pairs, Gate B pass)  | TRUSTED_LEARNED_SELECTED            | false

  All outcomes map to the same SelectedFlowId logic:
    DETERMINISTIC_GENERIC_SELECTED     → universal-nine-node-development-flow-{sub_phase}
    REJECTED_PROBATIONARY_FALLBACK_USED → universal-nine-node-development-flow-{sub_phase}
    ACCEPTED_PROBATIONARY_GATE_B_PASSED → registered candidate flow
    TRUSTED_LEARNED_SELECTED           → registered learned flow
