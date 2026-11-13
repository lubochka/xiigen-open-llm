# Layer 0-D — Flow Factory Selection Pairs (052)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-052-001 — "RICE" + "north-star" + "now-next-later" + "MoSCoW" quad gap → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-rice-scoring-001", "gap-project-planning-north-star-metric-001", "gap-project-planning-now-next-later-001", "gap-project-planning-moscow-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-rice-scoring-001", "gap-project-planning-north-star-metric-001", "gap-project-planning-now-next-later-001", "gap-project-planning-moscow-001"]

---

## B36-0D-052-002 — "hypothesis" + "experiment" + "KPIs" triple gap → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001", "gap-project-planning-kpi-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001", "gap-project-planning-kpi-001"]

---

## B36-0D-052-003 — "north-star" + "test" → TEST flow

A:
  sub_phase_id: "5.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-north-star-metric-001", "gap-project-planning-experiment-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-5.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-north-star-metric-001", "gap-project-planning-experiment-001"]

---

## B36-0D-052-004 — "t-shirt sizing" + "RICE" + "story splitting" + "implement" → DOCUMENT + IMPLEMENT flows

A:
  sub_phase_ids: ["2.4", "4.1"]
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-t-shirt-sizing-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-story-splitting-001"]

B:
  FlowSelections:
    - SelectedFlowId: "universal-nine-node-development-flow-2.4" | TrustState: "deterministic_generic"
    - SelectedFlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic"
  ExecutionOrder: "2.4 and 4.1 run in parallel"
  CapabilityGapWarnings: ["gap-project-planning-t-shirt-sizing-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-story-splitting-001"]
