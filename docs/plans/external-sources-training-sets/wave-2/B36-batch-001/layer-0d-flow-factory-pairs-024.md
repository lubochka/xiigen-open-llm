# Layer 0-D — Flow Factory Selection Pairs (024)
# Batch: B36-batch-001 | Scenario: B36 — Project planning — sprints, roadmaps, and milestones
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-024-001 — Complete factory selection table: sub_phase_id to flow map

A:
  Task: "State the complete sub_phase_id → SelectedFlowId mapping for all B36 sub-phases used."

B:
  FlowSelectionTable:
    null (knowledge_edge: true): "knowledge-edge-missing-flow" | TrustState: knowledge_edge_synthetic
    "2.1": "universal-nine-node-development-flow-2.1" | TrustState: deterministic_generic
    "2.4": "universal-nine-node-development-flow-2.4" | TrustState: deterministic_generic
    "3.4": "universal-nine-node-development-flow-3.4" | TrustState: deterministic_generic
    "4.1": "universal-nine-node-development-flow-4.1" | TrustState: deterministic_generic
    "5.1": "universal-nine-node-development-flow-5.1" | TrustState: deterministic_generic
    "6.1": "universal-nine-node-development-flow-6.1" | TrustState: deterministic_generic
  Rule: When registered_flow_id is null, the fallback universal-nine-node flow is selected.
  Rule: TrustState is always "deterministic_generic" for null-registered sub-phases.

---

## B36-0D-024-002 — QA-last execution ordering rule

A:
  Task: "When does the factory apply QA-last execution ordering?"

B:
  QALastRule:
    condition: "sub_phase_ids includes '6.1' (QA) AND at least one other sub_phase_id"
    result: "6.1 is assigned executionOrder: last; all other sub-phases run first or parallel"
    examples:
      - ["4.1", "6.1"] → "4.1 runs first, 6.1 runs after"
      - ["2.4", "6.1"] → "2.4 runs first, 6.1 runs after"
      - ["4.1", "2.1", "6.1"] → "4.1 and 2.1 run in parallel, 6.1 runs after both"
    exception: "6.1 alone → no ordering constraint needed; single flow runs normally"

---

## B36-0D-024-003 — Gap warning passthrough: factory does not modify gap warnings

A:
  sub_phase_id: "4.1"
  capability_gap_warnings: ["gap-project-planning-sprint-001", "gap-project-planning-epic-001"]
  flow_registry_state:
    registered_flow_id: null

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-sprint-001", "gap-project-planning-epic-001"]
  Note: The factory passes capability_gap_warnings through to the flow unchanged.
    The factory does NOT modify, drop, or add gap warnings.
    Gap warnings are metadata for the AutonomousLearningPipeline (subproject 16).

---

## B36-0D-024-004 — Six-gap knowledge-edge: factory always selects synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-sprint-001", "gap-project-planning-backlog-001", "gap-project-planning-velocity-001", "gap-project-planning-milestone-001", "gap-project-planning-epic-001", "gap-project-planning-ticket-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-sprint-001", "gap-project-planning-backlog-001", "gap-project-planning-velocity-001", "gap-project-planning-milestone-001", "gap-project-planning-epic-001", "gap-project-planning-ticket-001"]
  Rule: Any number of gap IDs → still one SCHEDULE_CAPABILITY_LEARNING signal.
    The factory always selects "knowledge-edge-missing-flow" when sub_phase_id is null.
  SelectionOutcome: "KNOWLEDGE_EDGE_SYNTHETIC_FLOW_SELECTED"
