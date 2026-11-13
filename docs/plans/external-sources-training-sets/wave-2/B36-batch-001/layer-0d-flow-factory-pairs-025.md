# Layer 0-D — Flow Factory Selection Pairs (025)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-025-001 — "scrum ceremony" knowledge_edge → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-scrum-ceremony-001", "gap-project-planning-sprint-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-scrum-ceremony-001", "gap-project-planning-sprint-001"]

---

## B36-0D-025-002 — "daily standup" knowledge_edge → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-standup-001", "gap-project-planning-daily-standup-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-standup-001", "gap-project-planning-daily-standup-001"]

---

## B36-0D-025-003 — "sprint demo" + "stakeholders" → DOCUMENT flow

A:
  sub_phase_id: "2.4"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-sprint-demo-001", "gap-project-planning-sprint-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.4"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-sprint-demo-001", "gap-project-planning-sprint-001"]

---

## B36-0D-025-004 — "sprint review" QA → QA flow

A:
  sub_phase_id: "6.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-sprint-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-6.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-sprint-001"]
