# Layer 0-D — Flow Factory Selection Pairs (019)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-019-001 — "story" DOCUMENT false positive: flow selection unchanged

A:
  sub_phase_id: "2.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-story-point-001", "gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
  Note: Gap warnings in capability_gap_warnings do NOT affect flow selection.
    The factory selects based on sub_phase_id: "2.1" only.

---

## B36-0D-019-002 — "story" + "feature" DOCUMENT+IMPLEMENT: two flows

A:
  sub_phase_ids: ["2.1", "4.1"]
  flow_registry_state: both null registered

B:
  Selections:
    - sub_phase_id: "2.1" → SelectedFlowId: "universal-nine-node-development-flow-2.1"
    - sub_phase_id: "4.1" → SelectedFlowId: "universal-nine-node-development-flow-4.1"
  ExecutionModel: "parallel"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED (x2)"

---

## B36-0D-019-003 — "story" + retrospective + review: QA+DOCUMENT two flows

A:
  sub_phase_ids: ["6.1", "2.1"]
  flow_registry_state: both null registered
  capability_gap_warnings: ["gap-project-planning-retrospective-001", "gap-project-planning-sprint-001", "gap-project-planning-velocity-001"]

B:
  Selections:
    - sub_phase_id: "6.1" → SelectedFlowId: "universal-nine-node-development-flow-6.1"
    - sub_phase_id: "2.1" → SelectedFlowId: "universal-nine-node-development-flow-2.1"
  ExecutionModel: "parallel"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED (x2)"

---

## B36-0D-019-004 — "stories" + backlog + sprint: DOCUMENT fires → 2.1 selection

A:
  sub_phase_id: "2.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-story-point-001", "gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
