# Layer 0-D — Flow Factory Selection Pairs (020)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-020-001 — Sprint + IMPLEMENT: 4.1 flow

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-4.1"
  capability_gap_warnings: ["gap-project-planning-sprint-001", "gap-project-planning-iteration-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B36-0D-020-002 — Roadmap + IMPLEMENT: 4.1 flow

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-4.1"
  capability_gap_warnings: ["gap-project-planning-roadmap-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B36-0D-020-003 — Epic + IMPLEMENT: 4.1 flow

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-4.1"
  capability_gap_warnings: ["gap-project-planning-epic-001", "gap-project-planning-sprint-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B36-0D-020-004 — Sprint + IMPLEMENT + DOCUMENT: two flows parallel

A:
  sub_phase_ids: ["4.1", "2.4"]
  flow_registry_state: both null registered
  capability_gap_warnings: ["gap-project-planning-sprint-001"]

B:
  Selections:
    - sub_phase_id: "4.1" → SelectedFlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic"
    - sub_phase_id: "2.4" → SelectedFlowId: "universal-nine-node-development-flow-2.4" | TrustState: "deterministic_generic"
  ExecutionModel: "parallel"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED (x2)"
  Note: Gap warnings do NOT prevent parallel flow selection.

---

## B36-0D-020-005 — Backlog + IMPLEMENT ("write the code"): 4.1

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-4.1"
  capability_gap_warnings: ["gap-project-planning-backlog-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
