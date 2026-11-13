# Layer 0-D — Flow Factory Selection Pairs (019)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## B01-0D-019-001 — trusted_learned: OnboardingScreenFlow fully promoted

```
source_id: xiigen-b01-0d-019-001
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  context_hint: "SaaS onboarding — signup, email activation, first-run"
  flow_registry_state:
    registered_flow_id: "onboarding-screen-flow-1.1-v5"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.92; production_evidence_pairs: 42"
  promotion_threshold_met: true

B:
  SelectedFlowId: "onboarding-screen-flow-1.1-v5"
  TrustState: "trusted_learned"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

---

## B01-0D-019-002 — deterministic_generic: sub-phase 1.1 — responsive layout context

```
source_id: xiigen-b01-0d-019-002
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  context_hint: "Responsive layout breakpoints for all screens"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B01-0D-019-003 — probationary_rejected: profile flow candidate low confidence

```
source_id: xiigen-b01-0d-019-003
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "profile-flow-1.1-candidate-early"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "min_confidence_below_threshold: 0.62 < 0.70 required"
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "profile-flow-1.1-candidate-early"
  Warnings:
    - "probationary_flow_rejected:profile-flow-1.1-candidate-early"
    - "gate_b_failed:min_confidence_0.62_below_threshold_0.70"
    - "fallback_used:universal-nine-node-development-flow-1.1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"
