# Layer 0-D — Flow Factory Selection Pairs (039)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-039-001 — "OKRs" + "KPIs" + "north-star" + "RICE" + "now-next-later" penta gap → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-okr-001", "gap-project-planning-kpi-001", "gap-project-planning-north-star-metric-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-now-next-later-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-okr-001", "gap-project-planning-kpi-001", "gap-project-planning-north-star-metric-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-now-next-later-001"]

---

## B36-0D-039-002 — "hypothesis" + "test" → TEST flow

A:
  sub_phase_id: "5.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-hypothesis-001", "gap-project-planning-north-star-metric-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-5.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-hypothesis-001", "gap-project-planning-north-star-metric-001"]

---

## B36-0D-039-003 — "OKRs" + "implement" → IMPLEMENT flow

A:
  sub_phase_id: "4.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-okr-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-okr-001"]

---

## B36-0D-039-004 — "MoSCoW" + "specify" + "criteria" → DOCUMENT flow

A:
  sub_phase_id: "2.4"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-moscow-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.4"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-moscow-001"]
