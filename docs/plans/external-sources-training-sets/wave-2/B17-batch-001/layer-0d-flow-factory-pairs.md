# Layer 0-D — Flow Factory Selection Pairs
# Batch: B17-batch-001
# Scenario: B17 — Development — integration
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Pair B17B1-WDA-factory-001 — deterministic_generic: 4.3

```
source_id: xiigen-b17-factory-001
sub_phase_id: "4.3"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "4.3"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-4.3"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.3"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## Pair B17B1-WDA-factory-002 — probationary_rejected: 4.3 candidate

```
source_id: xiigen-b17-factory-002
sub_phase_id: "4.3"
trust_state: probationary_rejected
pair_kind: factory_selection
```

A:
  sub_phase_id: "4.3"
  flow_registry_state:
    registered_flow_id: "payment-connector-flow-4.3-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 6 of 9 nodes covered; N08 missing"
  fallback_flow_id: "universal-nine-node-development-flow-4.3"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.3"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "payment-connector-flow-4.3-candidate"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

---

## Pair B17B1-WDA-factory-003 — trusted_learned: 4.2 (reference for IMPLEMENT-only integration)

```
source_id: xiigen-b17-factory-003
sub_phase_id: "4.2"
trust_state: trusted_learned
pair_kind: factory_selection
```

A:
  sub_phase_id: "4.2"
  flow_registry_state:
    registered_flow_id: "external-service-connector-flow-4.2"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.90; production_evidence_pairs: 26"
  promotion_threshold_met: true

B:
  SelectedFlowId: "external-service-connector-flow-4.2"
  TrustState: "trusted_learned"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"

---

## Pair B17B1-WDA-factory-004 — deterministic_generic: 3.3 (API_CONTRACT routing)

```
source_id: xiigen-b17-factory-004
sub_phase_id: "3.3"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "3.3"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-3.3"

B:
  SelectedFlowId: "universal-nine-node-development-flow-3.3"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
