# Layer 0-D — Flow Factory Selection Pairs (034)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-034-001 — "now-next-later" + "hypothesis" + "experiment" triple gap → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-now-next-later-001", "gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-now-next-later-001", "gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001"]

---

## B36-0D-034-002 — "KPIs" + "design" → ARCHITECTURE flow

A:
  sub_phase_id: "3.4"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-kpi-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-3.4"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-kpi-001"]

---

## B36-0D-034-003 — "experiment" + "test" → TEST flow

A:
  sub_phase_id: "5.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-experiment-001", "gap-project-planning-feature-flag-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-5.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-experiment-001", "gap-project-planning-feature-flag-001"]

---

## B36-0D-034-004 — RICE + story splitting + implement → DOCUMENT + IMPLEMENT parallel flows

A:
  sub_phase_ids: ["2.4", "4.1"]
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-rice-scoring-001", "gap-project-planning-story-splitting-001"]

B:
  FlowSelections:
    - SelectedFlowId: "universal-nine-node-development-flow-2.4" | TrustState: "deterministic_generic"
    - SelectedFlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic"
  ExecutionOrder: "2.4 and 4.1 run in parallel"
  CapabilityGapWarnings: ["gap-project-planning-rice-scoring-001", "gap-project-planning-story-splitting-001"]
