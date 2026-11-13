# Layer 0-D — Flow Factory Selection Pairs (013)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-013-001 — Planning-poker zero predicates: knowledge-edge synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  failure_classification: "no_predicate_match_capability_gap"
  capability_gap_id: "gap-project-planning-sprint-001"

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  Warnings:
    - "no_sub_phase_registered_for_input: planning-poker estimation vocabulary"
    - "capability_gap_id: gap-project-planning-sprint-001"
    - "SCHEDULE_CAPABILITY_LEARNING: no registered flow will execute"
  SelectionOutcome: "KNOWLEDGE_EDGE_SYNTHETIC_FLOW_SELECTED"

---

## B36-0D-013-002 — Planning-poker with DOCUMENT: deterministic generic 2.1

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B36-0D-013-003 — Planning-poker with IMPLEMENT: deterministic generic 4.1

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B36-0D-013-004 — Planning-poker with QA: deterministic generic 6.1

A:
  sub_phase_id: "6.1"
  flow_registry_state:
    registered_flow_id: null
    trust_state: null
  fallback_flow_id: "universal-nine-node-development-flow-6.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-6.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B36-0D-013-005 — Planning-poker IMPLEMENT+DOCUMENT multi-flow: two factories called

A:
  sub_phase_ids: ["4.1", "2.4"]
  flow_registry_state_4.1:
    registered_flow_id: null
  flow_registry_state_2.4:
    registered_flow_id: null

B:
  Selections:
    - sub_phase_id: "4.1" → SelectedFlowId: "universal-nine-node-development-flow-4.1"
    - sub_phase_id: "2.4" → SelectedFlowId: "universal-nine-node-development-flow-2.4"
  TrustState: "deterministic_generic" for both
  ExecutionModel: "parallel"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED (x2)"
