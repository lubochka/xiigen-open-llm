# Layer 0-D — Flow Factory Selection Pairs (014)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-014-001 — Effort-estimate zero predicates: knowledge-edge synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  failure_classification: "no_predicate_match_capability_gap"
  capability_gap_id: "gap-project-planning-estimation-001"

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  Warnings:
    - "no_sub_phase_registered_for_input: effort estimation vocabulary"
    - "capability_gap_id: gap-project-planning-estimation-001"
    - "SCHEDULE_CAPABILITY_LEARNING: no registered flow will execute"
  SelectionOutcome: "KNOWLEDGE_EDGE_SYNTHETIC_FLOW_SELECTED"

---

## B36-0D-014-002 — Effort-estimate with DOCUMENT: deterministic generic 2.1

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

## B36-0D-014-003 — Effort-estimate with IMPLEMENT: deterministic generic 4.1

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

## B36-0D-014-004 — Effort-estimate with DOCUMENT (acceptance criteria): 2.4

A:
  sub_phase_id: "2.4"
  flow_registry_state:
    registered_flow_id: null
  fallback_flow_id: "universal-nine-node-development-flow-2.4"

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.4"
  TrustState: "deterministic_generic"
  FallbackUsed: false
  Warnings: []
  SelectionOutcome: "DETERMINISTIC_GENERIC_SELECTED"

---

## B36-0D-014-005 — Effort-estimate five gap terms: knowledge-edge synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  failure_classification: "no_predicate_match_capability_gap"
  capability_gap_ids: ["gap-project-planning-estimation-001", "gap-project-planning-epic-001", "gap-project-planning-backlog-001", "gap-project-planning-sprint-001", "gap-project-planning-capacity-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  Warnings:
    - "no_sub_phase_registered_for_input: five gap terms (effort-estimate, epic, backlog, sprint, capacity)"
    - "SCHEDULE_CAPABILITY_LEARNING: five gap IDs bundled in one signal"
  SelectionOutcome: "KNOWLEDGE_EDGE_SYNTHETIC_FLOW_SELECTED"
