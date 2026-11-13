# Layer 0-D — Flow Factory Selection Pairs (047)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-047-001 — "impact mapping" + "hypothesis" + "experiment" + "MoSCoW" + "now-next-later" penta gap → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-impact-mapping-001", "gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001", "gap-project-planning-moscow-001", "gap-project-planning-now-next-later-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-impact-mapping-001", "gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001", "gap-project-planning-moscow-001", "gap-project-planning-now-next-later-001"]

---

## B36-0D-047-002 — "OKRs" + "review" + "implement" + "document" → DOCUMENT + IMPLEMENT + QA flows

A:
  sub_phase_ids: ["2.4", "4.1", "6.1"]
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-okr-001"]

B:
  FlowSelections:
    - SelectedFlowId: "universal-nine-node-development-flow-2.4" | TrustState: "deterministic_generic" | ExecutionOrder: "parallel"
    - SelectedFlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic" | ExecutionOrder: "parallel"
    - SelectedFlowId: "universal-nine-node-development-flow-6.1" | TrustState: "deterministic_generic" | ExecutionOrder: "last (QA-last)"
  CapabilityGapWarnings: ["gap-project-planning-okr-001"]

---

## B36-0D-047-003 — "t-shirt sizing" + "test" → TEST flow

A:
  sub_phase_id: "5.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-t-shirt-sizing-001", "gap-project-planning-hypothesis-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-5.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-t-shirt-sizing-001", "gap-project-planning-hypothesis-001"]

---

## B36-0D-047-004 — "north-star" + "define" → DOCUMENT flow

A:
  sub_phase_id: "2.4"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-north-star-metric-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.4"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-north-star-metric-001"]
