# Layer 0-D — Flow Factory Selection Pairs (031)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-031-001 — "hypothesis" knowledge_edge → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-hypothesis-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-hypothesis-001"]

---

## B36-0D-031-002 — "experiment" knowledge_edge → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-experiment-001", "gap-project-planning-hypothesis-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-experiment-001", "gap-project-planning-hypothesis-001"]

---

## B36-0D-031-003 — "hypothesis" + "test" → TEST flow

A:
  sub_phase_id: "5.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-hypothesis-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-5.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-hypothesis-001"]

---

## B36-0D-031-004 — "RICE scoring" + "review" → QA flow

A:
  sub_phase_id: "6.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-rice-scoring-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-6.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-rice-scoring-001"]
