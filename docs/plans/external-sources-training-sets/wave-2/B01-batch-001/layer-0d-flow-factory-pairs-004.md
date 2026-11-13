# Layer 0-D — Flow Factory Selection Pairs (004)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping and screen inventory
# Production date: 2026-05-05 | pair_kind: factory_selection

---

## B01-0D-004-001 — trusted_learned: 1.1 ScreenInventoryFlow fully promoted

```
source_id: xiigen-b01-0d-004-001
source_title: XIIGen B01 factory-004 — trusted_learned for sub-phase 1.1 (promoted)
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "1.1"
trust_state: trusted_learned
pair_kind: factory_selection
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "ScreenInventoryFlow11-v4"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.93; production_evidence_pairs: 31"
  fallback_flow_id: "ScreenInventoryFlow11"
  promotion_threshold_met: true
  promotion_threshold_reason: "production_evidence_pairs: 31 of 25 required — threshold exceeded"

B:
  SelectedFlowId: "ScreenInventoryFlow11-v4"
  TrustState: "trusted_learned"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

Factory logic:
  ScreenInventoryFlow11-v4 is fully promoted to trusted_learned. All Gate B assertions
  pass, production evidence threshold exceeded (31 > 25), explicit promotion received.
  No warnings issued. FallbackUsed: false. The generic ScreenInventoryFlow11 is not
  considered — the trusted_learned variant supersedes it entirely.

---

## B01-0D-004-002 — trusted_learned: cross-sub-phase example (3.4)

```
source_id: xiigen-b01-0d-004-002
source_title: XIIGen B01 factory-004 — trusted_learned: sub-phase 3.4 security flow
source_license_state: first_party
contract_signature: ILogicalFlowFactory<TInput> — flow factory selection
sub_phase_id: "3.4"
trust_state: trusted_learned
pair_kind: factory_selection
```

A:
  sub_phase_id: "3.4"
  flow_registry_state:
    registered_flow_id: "SecurityVerificationFlow34-v2"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.92; production_evidence_pairs: 33"
  fallback_flow_id: "universal-nine-node-development-flow-3.4"
  promotion_threshold_met: true
  promotion_threshold_reason: "production_evidence_pairs: 33 of 25 required"

B:
  SelectedFlowId: "SecurityVerificationFlow34-v2"
  TrustState: "trusted_learned"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

Factory logic:
  This demonstrates that trusted_learned flows exist across multiple sub-phases.
  The pattern is identical: no warnings, TRUSTED_LEARNED_SELECTED, FallbackUsed: false.
  Only the FlowId and sub_phase_id differ. The factory uses the same selection logic
  regardless of which sub-phase is being served.

---

## B01-0D-004-003 — trusted_learned vs probationary_accepted: state transition

```
source_id: xiigen-b01-0d-004-003
source_title: XIIGen B01 factory-004 — state transition: probationary_accepted to trusted_learned
source_license_state: first_party
pair_kind: factory_selection
coverage_status: covered
```

State transition diagram for flow trust states:

  [null / no flow registered]
         ↓ (first candidate submitted)
  [probationary — not yet Gate-B-evaluated]
         ↓ Gate B passes
  [probationary_accepted — Gate B passed, below promotion threshold]
         ↓ threshold met + explicit PROMOTE action
  [trusted_learned — fully promoted, warnings suppressed]

  Alternate path:
  [probationary — not yet Gate-B-evaluated]
         ↓ Gate B fails
  [probationary_rejected — Gate B failed, fallback used]
         ↓ (on re-submission with more pairs)
  [probationary — re-evaluated]

  At any state, a deterministic_generic flow is always available as fallback.
  trusted_learned is the only state where:
    - Warnings: []
    - FallbackUsed: false
    - SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

  The factory must never shortcut this progression:
    - Never mark probationary_rejected as gate_b_passed: true
    - Never use a probationary_rejected flow for production routing
    - Never auto-promote a probationary_accepted flow without explicit action
