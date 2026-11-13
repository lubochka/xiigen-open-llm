# Layer 0-D — Flow Factory Selection Pairs
# Batch: B18-batch-001
# Scenario: B18 — Development — refactoring and data migration
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Pair B18B1-WDA-factory-001 — deterministic_generic: 4.4

```
source_id: xiigen-b18-factory-001
sub_phase_id: "4.4"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "4.4"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-4.4"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.4"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## Pair B18B1-WDA-factory-002 — deterministic_generic: 4.1 (refactoring route)

```
source_id: xiigen-b18-factory-002
sub_phase_id: "4.1"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## Pair B18B1-WDA-factory-003 — probationary_rejected: 4.4 candidate

```
source_id: xiigen-b18-factory-003
sub_phase_id: "4.4"
trust_state: probationary_rejected
pair_kind: factory_selection
```

A:
  sub_phase_id: "4.4"
  flow_registry_state:
    registered_flow_id: "data-migration-flow-4.4-candidate"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 4 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-4.4"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.4"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "data-migration-flow-4.4-candidate"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"

---

## Pair B18B1-WDA-factory-004 — trusted_learned: 4.1 refactoring flow (promoted)

```
source_id: xiigen-b18-factory-004
sub_phase_id: "4.1"
trust_state: trusted_learned
pair_kind: factory_selection
```

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: "code-refactoring-flow-4.1"
    trust_state: "trusted_learned"
    gate_b_passed: true
    gate_b_reason: "all_9_nodes_covered; min_confidence: 0.91; production_evidence_pairs: 27"
  promotion_threshold_met: true

B:
  SelectedFlowId: "code-refactoring-flow-4.1"
  TrustState: "trusted_learned"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "TRUSTED_LEARNED_SELECTED"
