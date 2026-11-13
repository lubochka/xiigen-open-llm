# Layer 0-D — Flow Factory Selection Pairs (040)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-040-001 — "impact mapping" + "RICE" + "hypothesis" + "experiment" quad gap → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-impact-mapping-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-impact-mapping-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001"]

---

## B36-0D-040-002 — "t-shirt sizing" + "now-next-later" + "MoSCoW" triple gap → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-t-shirt-sizing-001", "gap-project-planning-now-next-later-001", "gap-project-planning-moscow-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-t-shirt-sizing-001", "gap-project-planning-now-next-later-001", "gap-project-planning-moscow-001"]

---

## B36-0D-040-003 — "north-star" + "review" + "implement" → QA + IMPLEMENT flows (QA-last)

A:
  sub_phase_ids: ["4.1", "6.1"]
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-north-star-metric-001", "gap-project-planning-okr-001"]

B:
  FlowSelections:
    - SelectedFlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic" | ExecutionOrder: "first"
    - SelectedFlowId: "universal-nine-node-development-flow-6.1" | TrustState: "deterministic_generic" | ExecutionOrder: "last (QA-last)"
  CapabilityGapWarnings: ["gap-project-planning-north-star-metric-001", "gap-project-planning-okr-001"]

---

## B36-0D-040-004 — "KPIs" + "OKRs" + "north-star" + "build" → IMPLEMENT flow

A:
  sub_phase_id: "4.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-kpi-001", "gap-project-planning-okr-001", "gap-project-planning-north-star-metric-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-kpi-001", "gap-project-planning-okr-001", "gap-project-planning-north-star-metric-001"]
