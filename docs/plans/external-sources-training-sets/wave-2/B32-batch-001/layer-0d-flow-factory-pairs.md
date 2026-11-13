# Layer 0-D — Flow Factory Selection Pairs
# Batch: B32-batch-001
# Scenario: B32 — Vibe coding gap
# Source: XIIGen first-party prompts
# License state: first_party
# Production date: 2026-05-05
# pair_kind: factory_selection

---

## Pair B32B1-WDA-factory-001 — knowledge-edge synthetic: null sub-phase

```
source_id: xiigen-b32-factory-001
sub_phase_id: null
trust_state: knowledge_edge_synthetic
pair_kind: factory_selection
```

A:
  sub_phase_id: null
  knowledge_edge: true
  failure_classification: "no_predicate_match_capability_gap"
  capability_gap_id: "gap-vibe-coding-exploratory-001"

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  Warnings:
    - "no_sub_phase_registered_for_input: vibe coding gap vocabulary"
    - "SCHEDULE_CAPABILITY_LEARNING: no registered flow will execute"
  SelectionOutcome: "KNOWLEDGE_EDGE_SYNTHETIC_FLOW_SELECTED"

---

## Pair B32B1-WDA-factory-002 — deterministic_generic: 1.1 (UI-vibe route)

```
source_id: xiigen-b32-factory-002
sub_phase_id: "1.1"
trust_state: deterministic_generic
pair_kind: factory_selection
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## Pair B32B1-WDA-factory-003 — deterministic_generic: 4.1 (IMPLEMENT-vibe route)

```
source_id: xiigen-b32-factory-003
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

## Pair B32B1-WDA-factory-004 — probationary_rejected: 1.1 candidate (reference)

```
source_id: xiigen-b32-factory-004
sub_phase_id: "1.1"
trust_state: probationary_rejected
pair_kind: factory_selection
```

A:
  sub_phase_id: "1.1"
  flow_registry_state:
    registered_flow_id: "ui-vibe-prototype-flow-1.1"
    trust_state: "probationary"
    gate_b_passed: false
    gate_b_reason: "insufficient_evidence_pairs: 5 of 9 nodes covered"
  fallback_flow_id: "universal-nine-node-development-flow-1.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-1.1"
  TrustState: "deterministic_generic"
  FallbackUsed: true
  RejectedCandidateFlowId: "ui-vibe-prototype-flow-1.1"
  SelectionOutcome: "REJECTED_PROBATIONARY_FALLBACK_USED"
