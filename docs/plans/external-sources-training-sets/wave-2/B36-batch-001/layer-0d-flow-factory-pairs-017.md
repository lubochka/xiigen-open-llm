# Layer 0-D — Flow Factory Selection Pairs (017)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-017-001 — Grooming zero predicates: knowledge-edge synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  failure_classification: "no_predicate_match_capability_gap"
  capability_gap_id: "gap-project-planning-backlog-001"

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  Warnings:
    - "no_sub_phase_registered_for_input: grooming/backlog vocabulary"
    - "SCHEDULE_CAPABILITY_LEARNING: no registered flow will execute"
  SelectionOutcome: "KNOWLEDGE_EDGE_SYNTHETIC_FLOW_SELECTED"

---

## B36-0D-017-002 — Refinement zero predicates: knowledge-edge synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  failure_classification: "no_predicate_match_capability_gap"
  capability_gap_ids: ["gap-project-planning-backlog-001", "gap-project-planning-ticket-001", "gap-project-planning-iteration-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  Warnings:
    - "no_sub_phase_registered_for_input: backlog refinement vocabulary"
    - "SCHEDULE_CAPABILITY_LEARNING: three gap IDs bundled in one signal"
  SelectionOutcome: "KNOWLEDGE_EDGE_SYNTHETIC_FLOW_SELECTED"

---

## B36-0D-017-003 — Grooming with DOCUMENT: 2.1

A:
  sub_phase_id: "2.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B36-0D-017-004 — Grooming with IMPLEMENT: 4.1

A:
  sub_phase_id: "4.1"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-4.1"

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B36-0D-017-005 — Grooming with story false positive DOCUMENT: 2.1

A:
  sub_phase_id: "2.1"
  knowledge_edge: false
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-2.1"
  capability_gap_warnings: ["gap-project-planning-backlog-001", "gap-project-planning-epic-001", "gap-project-planning-ticket-001", "gap-project-planning-story-point-001", "gap-project-planning-sprint-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.1"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"
  Note: Gap warnings are passed as metadata but do not affect flow selection.
    The factory selects based on sub_phase_id only.
