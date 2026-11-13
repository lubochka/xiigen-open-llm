# Layer 0-D — Flow Factory Selection Pairs (042)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-042-001 — All ceremony gap + all metrics gap → synthetic flow (max IDs)

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-scrum-ceremony-001", "gap-project-planning-mvp-001", "gap-project-planning-launch-readiness-001", "gap-project-planning-okr-001", "gap-project-planning-kpi-001", "gap-project-planning-north-star-metric-001", "gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-scrum-ceremony-001", "gap-project-planning-mvp-001", "gap-project-planning-launch-readiness-001", "gap-project-planning-okr-001", "gap-project-planning-kpi-001", "gap-project-planning-north-star-metric-001", "gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001"]
  Rule: "Eight gap IDs → one signal, eight IDs bundled. Still one flow selected."

---

## B36-0D-042-002 — "impact mapping" + "document" → DOCUMENT flow

A:
  sub_phase_id: "2.4"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-impact-mapping-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.4"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-impact-mapping-001"]

---

## B36-0D-042-003 — "t-shirt sizing" + "review" → QA flow

A:
  sub_phase_id: "6.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-t-shirt-sizing-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-6.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-t-shirt-sizing-001"]

---

## B36-0D-042-004 — "now-next-later" + "design" → ARCHITECTURE flow

A:
  sub_phase_id: "3.4"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-now-next-later-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-3.4"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-now-next-later-001"]
