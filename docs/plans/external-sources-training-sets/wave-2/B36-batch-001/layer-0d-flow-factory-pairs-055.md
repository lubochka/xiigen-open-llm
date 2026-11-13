# Layer 0-D — Flow Factory Selection Pairs (055)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-055-001 — "OKRs" + "KPIs" + "north-star" + "RICE" + "MoSCoW" + "hypothesis" hexa gap → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-okr-001", "gap-project-planning-kpi-001", "gap-project-planning-north-star-metric-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-moscow-001", "gap-project-planning-hypothesis-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-okr-001", "gap-project-planning-kpi-001", "gap-project-planning-north-star-metric-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-moscow-001", "gap-project-planning-hypothesis-001"]

---

## B36-0D-055-002 — "impact mapping" + "t-shirt sizing" + "experiment" triple gap → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-impact-mapping-001", "gap-project-planning-t-shirt-sizing-001", "gap-project-planning-experiment-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-impact-mapping-001", "gap-project-planning-t-shirt-sizing-001", "gap-project-planning-experiment-001"]

---

## B36-0D-055-003 — "OKRs" + "review" + "implement" + "document" → all three flows

A:
  sub_phase_ids: ["2.4", "4.1", "6.1"]
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-okr-001", "gap-project-planning-north-star-metric-001"]

B:
  FlowSelections:
    - SelectedFlowId: "universal-nine-node-development-flow-2.4" | TrustState: "deterministic_generic" | ExecutionOrder: "parallel"
    - SelectedFlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic" | ExecutionOrder: "parallel"
    - SelectedFlowId: "universal-nine-node-development-flow-6.1" | TrustState: "deterministic_generic" | ExecutionOrder: "last (QA-last)"
  CapabilityGapWarnings: ["gap-project-planning-okr-001", "gap-project-planning-north-star-metric-001"]

---

## B36-0D-055-004 — "t-shirt sizing" + "define" → DOCUMENT flow

A:
  sub_phase_id: "2.4"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-t-shirt-sizing-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.4"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-t-shirt-sizing-001"]
