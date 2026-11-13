# Layer 0-D — Flow Factory Selection Pairs (015)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-015-001 — Release-planning zero predicates: knowledge-edge synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  failure_classification: "no_predicate_match_capability_gap"
  capability_gap_id: "gap-project-planning-release-cycle-001"

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  Warnings:
    - "no_sub_phase_registered_for_input: release planning vocabulary"
    - "capability_gap_id: gap-project-planning-release-cycle-001"
    - "SCHEDULE_CAPABILITY_LEARNING: no registered flow will execute"
  SelectionOutcome: "KNOWLEDGE_EDGE_SYNTHETIC_FLOW_SELECTED"

---

## B36-0D-015-002 — Release-planning with QA+DOCUMENT: two flows parallel

A:
  sub_phase_ids: ["6.1", "2.4"]
  flow_registry_state: both null registered

B:
  Selections:
    - sub_phase_id: "6.1" → SelectedFlowId: "universal-nine-node-development-flow-6.1"
    - sub_phase_id: "2.4" → SelectedFlowId: "universal-nine-node-development-flow-2.4"
  TrustState: "deterministic_generic" for both
  ExecutionModel: "parallel"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED (x2)"

---

## B36-0D-015-003 — Release-planning with IMPLEMENT: deterministic generic 4.1

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

## B36-0D-015-004 — Release-planning five gap terms: knowledge-edge synthetic

A:
  sub_phase_id: null
  knowledge_edge: true
  failure_classification: "no_predicate_match_capability_gap"
  capability_gap_ids: ["gap-project-planning-release-cycle-001", "gap-project-planning-milestone-001", "gap-project-planning-epic-001", "gap-project-planning-sprint-001", "gap-project-planning-velocity-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  Warnings:
    - "no_sub_phase_registered_for_input: release planning + four other gap terms"
    - "SCHEDULE_CAPABILITY_LEARNING: five gap IDs bundled in one signal"
  SelectionOutcome: "KNOWLEDGE_EDGE_SYNTHETIC_FLOW_SELECTED"

---

## B36-0D-015-005 — Release-planning with DOCUMENT (release notes): 2.1

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
