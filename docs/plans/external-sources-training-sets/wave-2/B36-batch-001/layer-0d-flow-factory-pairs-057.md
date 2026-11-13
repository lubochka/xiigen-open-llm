# Layer 0-D — Flow Factory Selection Pairs (057)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-057-001 — "all metrics" + "all ceremonies" (10 IDs) → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-okr-001", "gap-project-planning-kpi-001", "gap-project-planning-north-star-metric-001", "gap-project-planning-impact-mapping-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-moscow-001", "gap-project-planning-now-next-later-001", "gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001", "gap-project-planning-t-shirt-sizing-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-okr-001", "gap-project-planning-kpi-001", "gap-project-planning-north-star-metric-001", "gap-project-planning-impact-mapping-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-moscow-001", "gap-project-planning-now-next-later-001", "gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001", "gap-project-planning-t-shirt-sizing-001"]
  Rule: "10 gap IDs → one signal with all 10 bundled. Still one synthetic flow selected."

---

## B36-0D-057-002 — "hypothesis" + "test" → TEST flow

A:
  sub_phase_id: "5.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-hypothesis-001", "gap-project-planning-north-star-metric-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-5.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-hypothesis-001", "gap-project-planning-north-star-metric-001"]

---

## B36-0D-057-003 — "OKRs" + "review" + "implement" → IMPLEMENT + QA flows (QA-last)

A:
  sub_phase_ids: ["4.1", "6.1"]
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-okr-001"]

B:
  FlowSelections:
    - SelectedFlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic" | ExecutionOrder: "first"
    - SelectedFlowId: "universal-nine-node-development-flow-6.1" | TrustState: "deterministic_generic" | ExecutionOrder: "last (QA-last)"
  CapabilityGapWarnings: ["gap-project-planning-okr-001"]

---

## B36-0D-057-004 — "KPIs" + "design" → ARCHITECTURE flow

A:
  sub_phase_id: "3.4"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-kpi-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-3.4"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-kpi-001"]
