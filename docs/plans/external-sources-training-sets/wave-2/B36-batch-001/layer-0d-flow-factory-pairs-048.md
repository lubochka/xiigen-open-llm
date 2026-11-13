# Layer 0-D — Flow Factory Selection Pairs (048)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-048-001 — Max gap (8 IDs) → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-okr-001", "gap-project-planning-kpi-001", "gap-project-planning-north-star-metric-001", "gap-project-planning-impact-mapping-001", "gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-moscow-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-okr-001", "gap-project-planning-kpi-001", "gap-project-planning-north-star-metric-001", "gap-project-planning-impact-mapping-001", "gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-moscow-001"]
  Rule: "Eight gap IDs → one signal. Factory always selects knowledge-edge-missing-flow."

---

## B36-0D-048-002 — "OKRs" + "review" → QA flow

A:
  sub_phase_id: "6.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-okr-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-6.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-okr-001"]

---

## B36-0D-048-003 — "experiment" + "test" → TEST flow

A:
  sub_phase_id: "5.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-experiment-001", "gap-project-planning-feature-flag-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-5.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-experiment-001", "gap-project-planning-feature-flag-001"]

---

## B36-0D-048-004 — Factory canonical summary for files 025–060

A:
  Task: "Summarize the factory selection rule across all B36 files 025–060."

B:
  FactoryCanonical:
    "sub_phase_id: null → knowledge-edge-missing-flow (TrustState: knowledge_edge_synthetic)"
    "sub_phase_id: any → universal-nine-node-development-flow-{sub_phase_id} (TrustState: deterministic_generic)"
    "Multiple sub_phase_ids → multiple flow selections; QA (6.1) always last when present with others"
    "CapabilityGapWarnings passed through unchanged always"
    "Applies to all B36 vocabulary: ceremonies, metrics, all Round 1–3 gap terms"
