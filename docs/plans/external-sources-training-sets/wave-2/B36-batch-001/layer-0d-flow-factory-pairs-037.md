# Layer 0-D — Flow Factory Selection Pairs (037)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-037-001 — "t-shirt sizing" + "RICE" + "hypothesis" triple gap → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-t-shirt-sizing-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-hypothesis-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-t-shirt-sizing-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-hypothesis-001"]

---

## B36-0D-037-002 — "impact mapping" + "now-next-later" knowledge_edge → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-impact-mapping-001", "gap-project-planning-now-next-later-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-impact-mapping-001", "gap-project-planning-now-next-later-001"]

---

## B36-0D-037-003 — "story splitting" + "review" → DOCUMENT + QA flows (QA-last)

A:
  sub_phase_ids: ["2.4", "6.1"]
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-story-splitting-001", "gap-project-planning-rice-scoring-001"]

B:
  FlowSelections:
    - SelectedFlowId: "universal-nine-node-development-flow-2.4" | TrustState: "deterministic_generic" | ExecutionOrder: "first"
    - SelectedFlowId: "universal-nine-node-development-flow-6.1" | TrustState: "deterministic_generic" | ExecutionOrder: "last (QA-last)"
  CapabilityGapWarnings: ["gap-project-planning-story-splitting-001", "gap-project-planning-rice-scoring-001"]

---

## B36-0D-037-004 — "north-star" + "KPIs" + "build" → IMPLEMENT flow

A:
  sub_phase_id: "4.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-north-star-metric-001", "gap-project-planning-kpi-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-north-star-metric-001", "gap-project-planning-kpi-001"]
