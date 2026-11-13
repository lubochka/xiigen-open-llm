# Layer 0-D — Flow Factory Selection Pairs (027)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-027-001 — "launch readiness" knowledge_edge → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-launch-readiness-001", "gap-project-planning-release-cycle-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-launch-readiness-001", "gap-project-planning-release-cycle-001"]

---

## B36-0D-027-002 — "go-live checklist" + "stakeholders" → DOCUMENT flow

A:
  sub_phase_id: "2.4"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-go-live-checklist-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-2.4"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-go-live-checklist-001"]

---

## B36-0D-027-003 — "go-live checklist" + "review" → QA flow

A:
  sub_phase_id: "6.1"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-go-live-checklist-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-6.1"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-go-live-checklist-001"]

---

## B36-0D-027-004 — "launch readiness" + "design" → ARCHITECTURE flow

A:
  sub_phase_id: "3.4"
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-launch-readiness-001"]

B:
  SelectedFlowId: "universal-nine-node-development-flow-3.4"
  TrustState: "deterministic_generic"
  CapabilityGapWarnings: ["gap-project-planning-launch-readiness-001"]
