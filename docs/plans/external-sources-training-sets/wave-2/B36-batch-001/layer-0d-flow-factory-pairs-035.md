# Layer 0-D — Flow Factory Selection Pairs (035)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-035-001 — Six-gap knowledge_edge (all Round 3 metrics) → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-okr-001", "gap-project-planning-kpi-001", "gap-project-planning-north-star-metric-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-moscow-001", "gap-project-planning-now-next-later-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-okr-001", "gap-project-planning-kpi-001", "gap-project-planning-north-star-metric-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-moscow-001", "gap-project-planning-now-next-later-001"]
  Rule: "Six gap IDs → one signal with all six bundled. Factory always selects synthetic flow."

---

## B36-0D-035-002 — "hypothesis" + "test" + "RICE" + "review" → TEST + QA flows (QA-last)

A:
  sub_phase_ids: ["5.1", "6.1"]
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-hypothesis-001", "gap-project-planning-rice-scoring-001"]

B:
  FlowSelections:
    - SelectedFlowId: "universal-nine-node-development-flow-5.1" | TrustState: "deterministic_generic" | ExecutionOrder: "first"
    - SelectedFlowId: "universal-nine-node-development-flow-6.1" | TrustState: "deterministic_generic" | ExecutionOrder: "last (QA-last)"
  CapabilityGapWarnings: ["gap-project-planning-hypothesis-001", "gap-project-planning-rice-scoring-001"]

---

## B36-0D-035-003 — "OKRs" + "north-star" + "review" + "document" → DOCUMENT + QA flows (QA-last)

A:
  sub_phase_ids: ["2.4", "6.1"]
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-okr-001", "gap-project-planning-north-star-metric-001"]

B:
  FlowSelections:
    - SelectedFlowId: "universal-nine-node-development-flow-2.4" | TrustState: "deterministic_generic" | ExecutionOrder: "first"
    - SelectedFlowId: "universal-nine-node-development-flow-6.1" | TrustState: "deterministic_generic" | ExecutionOrder: "last (QA-last)"
  CapabilityGapWarnings: ["gap-project-planning-okr-001", "gap-project-planning-north-star-metric-001"]

---

## B36-0D-035-004 — Factory passthrough rule reminder

A:
  Task: "State the factory passthrough rule for capability_gap_warnings."

B:
  PassthroughRule: "The FlowFactory passes capability_gap_warnings through unchanged.
    It does NOT add, remove, or modify gap warnings.
    Gap warnings are consumed downstream by AutonomousLearningPipeline (subproject 16).
    This rule applies to all B36 files regardless of the number of gap IDs present."
