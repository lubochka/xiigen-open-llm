# Layer 0-D — Flow Factory Selection Pairs (030)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-030-001 — "MoSCoW" + "backlog" knowledge_edge → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-moscow-001", "gap-project-planning-backlog-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-moscow-001", "gap-project-planning-backlog-001"]

---

## B36-0D-030-002 — "now-next-later" + "roadmap" knowledge_edge → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-now-next-later-001", "gap-project-planning-product-roadmap-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-now-next-later-001", "gap-project-planning-product-roadmap-001"]

---

## B36-0D-030-003 — "MoSCoW" + "specify" → DOCUMENT flow

A:
  sub_phase_id: "2.4"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-moscow-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.4"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-moscow-001"]

---

## B36-0D-030-004 — "now-next-later" + "build" → IMPLEMENT flow

A:
  sub_phase_id: "4.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-now-next-later-001", "gap-project-planning-product-roadmap-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-4.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-now-next-later-001", "gap-project-planning-product-roadmap-001"]
