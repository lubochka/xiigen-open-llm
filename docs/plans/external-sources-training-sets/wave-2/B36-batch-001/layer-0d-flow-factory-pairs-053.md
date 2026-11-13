# Layer 0-D — Flow Factory Selection Pairs (053)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-053-001 — "OKRs" + "impact mapping" + "RICE" triple gap → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-okr-001", "gap-project-planning-impact-mapping-001", "gap-project-planning-rice-scoring-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-okr-001", "gap-project-planning-impact-mapping-001", "gap-project-planning-rice-scoring-001"]

---

## B36-0D-053-002 — "hypothesis" + "MoSCoW" + "now-next-later" triple gap → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-hypothesis-001", "gap-project-planning-moscow-001", "gap-project-planning-now-next-later-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-hypothesis-001", "gap-project-planning-moscow-001", "gap-project-planning-now-next-later-001"]

---

## B36-0D-053-003 — "KPIs" + "review" + "document" → DOCUMENT + QA flows (QA-last)

A:
  sub_phase_ids: ["2.4", "6.1"]
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-kpi-001", "gap-project-planning-retrospective-001", "gap-project-planning-sprint-001"]

B:
  FlowSelections:
    - SelectedFlowId: "universal-nine-node-development-flow-2.4" | TrustState: "deterministic_generic" | ExecutionOrder: "first"
    - SelectedFlowId: "universal-nine-node-development-flow-6.1" | TrustState: "deterministic_generic" | ExecutionOrder: "last (QA-last)"
  CapabilityGapWarnings: ["gap-project-planning-kpi-001", "gap-project-planning-retrospective-001", "gap-project-planning-sprint-001"]

---

## B36-0D-053-004 — "impact mapping" + "implement" → IMPLEMENT flow

A:
  sub_phase_id: "4.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-impact-mapping-001", "gap-project-planning-okr-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-impact-mapping-001", "gap-project-planning-okr-001"]
