# Layer 0-D — Flow Factory Selection Pairs (021)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-021-001 — Sprint + QA+IMPLEMENT: two flows, QA last

A:
  sub_phase_ids: ["6.1", "4.1"]
  execution_constraint: "QA last (QA-last structural constraint)"
  flow_registry_state: both null registered

B:
  Selections:
    - sub_phase_id: "4.1" → SelectedFlowId: "universal-nine-node-development-flow-4.1" | executionOrder: 1
    - sub_phase_id: "6.1" → SelectedFlowId: "universal-nine-node-development-flow-6.1" | executionOrder: 2 (QA last)
  ExecutionModel: "sequential: 4.1 first, 6.1 after"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED (x2)"

---

## B36-0D-021-002 — Velocity + QA: 6.1 only

A:
  sub_phase_id: "6.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-6.1"
  capability_gap_warnings: ["gap-project-planning-velocity-001", "gap-project-planning-sprint-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-6.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B36-0D-021-003 — Milestone + QA + DOCUMENT: two flows parallel

A:
  sub_phase_ids: ["6.1", "2.4"]
  flow_registry_state: both null registered
  capability_gap_warnings: ["gap-project-planning-milestone-001"]

B:
  Selections:
    - sub_phase_id: "2.4" → SelectedFlowId: "universal-nine-node-development-flow-2.4" | executionOrder: 1
    - sub_phase_id: "6.1" → SelectedFlowId: "universal-nine-node-development-flow-6.1" | executionOrder: 2 (QA last)
  ExecutionModel: "sequential: 2.4 first, 6.1 after (QA-last)"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED (x2)"

---

## B36-0D-021-004 — Roadmap + QA + TEST: two flows, QA and TEST ordering

A:
  sub_phase_ids: ["6.1", "5.1"]
  flow_registry_state: both null registered

B:
  Selections:
    - sub_phase_id: "5.1" → SelectedFlowId: "universal-nine-node-development-flow-5.1" | executionOrder: 1
    - sub_phase_id: "6.1" → SelectedFlowId: "universal-nine-node-development-flow-6.1" | executionOrder: 2 (QA last)
  ExecutionModel: "sequential: 5.1 first, 6.1 after (QA-last)"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED (x2)"

---

## B36-0D-021-005 — Backlog + QA + DOCUMENT: two flows, QA last

A:
  sub_phase_ids: ["6.1", "2.4"]
  flow_registry_state: both null registered
  capability_gap_warnings: ["gap-project-planning-backlog-001", "gap-project-planning-sprint-001"]

B:
  Selections:
    - sub_phase_id: "2.4" → SelectedFlowId: "universal-nine-node-development-flow-2.4" | executionOrder: 1
    - sub_phase_id: "6.1" → SelectedFlowId: "universal-nine-node-development-flow-6.1" | executionOrder: 2 (QA last)
  ExecutionModel: "sequential: 2.4 first, 6.1 after (QA-last)"
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED (x2)"
