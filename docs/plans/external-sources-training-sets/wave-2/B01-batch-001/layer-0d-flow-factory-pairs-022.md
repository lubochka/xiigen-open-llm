# Layer 0-D — Flow Factory Selection Pairs (022)
# Batch: B01-batch-001 | Scenario: B01 — SaaS onboarding screen flow mapping
# Production date: 2026-05-05 | pair_kind: flow_factory

---

## Method note (022)

v6-01 invented flows — Corruption-v6-02 applied to flow factory.
This file shows cases where a corrupt output invents a non-existent trusted_learned flow
or incorrectly promotes a probationary_rejected flow.

---

## B01-0D-022-001 — v6-02 corruption: accepting a probationary_rejected flow

```
source_id: xiigen-b01-0d-022-001
source_license_state: first_party
pair_kind: flow_factory
corruption_type: Corruption-v6-02-flow-factory-fallback-suppression
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "wishlist-flow-1.1-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 4 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B-correct:
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "wishlist-flow-1.1-candidate"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

B-corrupt (v6-02):
  SelectedFlowId: "wishlist-flow-1.1-candidate"
  TrustState: "trusted_learned"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

Corruption analysis:
  B-corrupt falsely promotes a rejected probationary flow to trusted_learned.
  Gate B was not passed — only 4 of 9 nodes covered. Promoting it risks
  incorrect screen inventory output from 5 unvalidated nodes.

---

## B01-0D-022-002 — v6-02 corruption: inventing a non-existent flow

```
source_id: xiigen-b01-0d-022-002
source_license_state: first_party
pair_kind: flow_factory
corruption_type: Corruption-v6-02-fabricated-flow
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B-correct:
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

B-corrupt (v6-02 fabrication):
  SelectedFlowId: "screen-inventory-flow-1.1-invented"
  TrustState: "trusted_learned"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

Corruption analysis:
  B-corrupt invents a flow ID that does not exist in the registry.
  No entry for "screen-inventory-flow-1.1-invented" is in the flow registry.
  The correct behavior is to use the deterministic generic fallback when no registered flow exists.

---

## B01-0D-022-003 — correct: FallbackUsed: false when deterministic_generic, no candidate

```
source_id: xiigen-b01-0d-022-003
source_license_state: first_party
pair_kind: flow_factory
```

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B-correct:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  RejectedCandidateFlowId: null
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

Rule reinforcement:
  FallbackUsed: false here because there was no candidate to reject.
  This is primary selection, not fallback from rejection.
