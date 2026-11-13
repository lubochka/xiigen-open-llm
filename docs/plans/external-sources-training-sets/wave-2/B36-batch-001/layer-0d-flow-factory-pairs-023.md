# Layer 0-D — Flow Factory Selection Pairs (023)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-023-001 — "retrospective" only: knowledge-edge synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  failure_classification: "no_predicate_match_capability_gap"
  capability_gap_ids: ["gap-project-planning-retrospective-001", "gap-project-planning-sprint-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  Warnings:
    - "no_sub_phase_registered_for_input: retrospective vocabulary"
    - "SCHEDULE_CAPABILITY_LEARNING: two gap IDs bundled"
  SelectionOutcome: "KNOWLEDGE_EDGE_SYNTHETIC_FLOW_SELECTED"

---

## B36-0D-023-002 — "review" in planning context: QA → 6.1

A:
  sub_phase_id: "6.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-6.1"
  capability_gap_warnings: ["gap-project-planning-sprint-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-6.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B36-0D-023-003 — "retrospective" + "review": QA fires → 6.1 (retrospective is gap)

A:
  sub_phase_id: "6.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-6.1"
  capability_gap_warnings: ["gap-project-planning-retrospective-001", "gap-project-planning-sprint-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-6.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
  Note: "retrospective" is a gap warning, not a routing signal. Only "review" drove
    the QA routing and thus the 6.1 sub-phase selection.

---

## B36-0D-023-004 — "sprint review": QA+IMPLEMENT+DOCUMENT → three flows

A:
  sub_phase_ids: ["6.1", "4.1", "2.4"]
  execution_constraint: "QA last"
  flow_registry_state: all null registered

B:
  Selections:
    - sub_phase_id: "4.1" → SelectedFlowId: "universal-nine-node-development-flow-4.1" | executionOrder: 1
    - sub_phase_id: "2.4" → SelectedFlowId: "universal-nine-node-development-flow-2.4" | executionOrder: 1 (parallel with 4.1)
    - sub_phase_id: "6.1" → SelectedFlowId: "universal-nine-node-development-flow-6.1" | executionOrder: 2 (QA last)
  ExecutionModel: "parallel 4.1+2.4, then 6.1"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED (x3)"

---

## B36-0D-023-005 — Retrospective-only: knowledge-edge synthetic (confirm Rule 22)

A:
  sub_phase_id: null
  knowledge_edge: true
  failure_classification: "no_predicate_match_capability_gap"
  capability_gap_ids: ["gap-project-planning-retrospective-001", "gap-project-planning-sprint-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-retrospective-001", "gap-project-planning-sprint-001"]
  Note: Rule 22 confirmed: one SCHEDULE_CAPABILITY_LEARNING signal with all gap IDs bundled.
  SelectionOutcome: "KNOWLEDGE_EDGE_SYNTHETIC_FLOW_SELECTED"
