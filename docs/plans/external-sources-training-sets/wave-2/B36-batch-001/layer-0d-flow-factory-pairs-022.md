# Layer 0-D — Flow Factory Selection Pairs (022)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-022-001 — Roadmap + DOCUMENT: 2.4

A:
  sub_phase_id: "2.4"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-2.4"
  capability_gap_warnings: ["gap-project-planning-roadmap-001", "gap-project-planning-milestone-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.4"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B36-0D-022-002 — Sprint + DOCUMENT (acceptance criteria): 2.4

A:
  sub_phase_id: "2.4"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-2.4"
  capability_gap_warnings: ["gap-project-planning-sprint-001", "gap-project-planning-ticket-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.4"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B36-0D-022-003 — Backlog + DOCUMENT (functional+spec): 2.1

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"
  capability_gap_warnings: ["gap-project-planning-backlog-001", "gap-project-planning-epic-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B36-0D-022-004 — Velocity + DOCUMENT: 2.1

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"
  capability_gap_warnings: ["gap-project-planning-velocity-001", "gap-project-planning-sprint-001", "gap-project-planning-capacity-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B36-0D-022-005 — Milestone + DOCUMENT (use-case): 2.1

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"
  capability_gap_warnings: ["gap-project-planning-milestone-001", "gap-project-planning-roadmap-001", "gap-project-planning-epic-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
