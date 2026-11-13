# Layer 0-D — Flow Factory Selection Pairs (036)
# Batch: B36-batch-001 | Scenario: B36 — Agile planning vocabulary gap detection
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B36-0D-036-001 — Full Round 3 factory selection reference table

A:
  Task: "State the complete factory selection reference for all Round 3 gap vocabulary."

B:
  FactorySelectionTable_Round3:
    knowledge_edge: true → "knowledge-edge-missing-flow" | TrustState: knowledge_edge_synthetic
    sub_phase: "2.4" → "universal-nine-node-development-flow-2.4" | TrustState: deterministic_generic
    sub_phase: "3.4" → "universal-nine-node-development-flow-3.4" | TrustState: deterministic_generic
    sub_phase: "4.1" → "universal-nine-node-development-flow-4.1" | TrustState: deterministic_generic
    sub_phase: "5.1" → "universal-nine-node-development-flow-5.1" | TrustState: deterministic_generic
    sub_phase: "6.1" → "universal-nine-node-development-flow-6.1" | TrustState: deterministic_generic

  Round 3 gap vocabulary (scrum ceremony, daily standup, sprint demo, backlog refinement,
    product increment, MVP, launch readiness, go-live checklist, feature flag, OKRs, KPIs,
    north-star metric, impact mapping, t-shirt sizing, RICE scoring, MoSCoW, now-next-later,
    hypothesis, experiment, story splitting):
    All route to knowledge-edge-missing-flow when no predicate fires.

---

## B36-0D-036-002 — "hypothesis" + "experiment" + "RICE" + "MoSCoW" + "now-next-later" → synthetic flow

A:
  sub_phase_id: null
  knowledge_edge: true
  capability_gap_ids: ["gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-moscow-001", "gap-project-planning-now-next-later-001"]

B:
  SelectedFlowId: "knowledge-edge-missing-flow"
  TrustState: "knowledge_edge_synthetic"
  FallbackUsed: false
  CapabilityLearningSignal:
    signal: "SCHEDULE_CAPABILITY_LEARNING"
    capabilityGapIds: ["gap-project-planning-hypothesis-001", "gap-project-planning-experiment-001", "gap-project-planning-rice-scoring-001", "gap-project-planning-moscow-001", "gap-project-planning-now-next-later-001"]

---

## B36-0D-036-003 — Five predicates → five flows (QA-last)

A:
  sub_phase_ids: ["3.4", "4.1", "5.1", "2.4", "6.1"]
  knowledge_edge: false
  capability_gap_warnings: ["gap-project-planning-feature-flag-001", "gap-project-planning-hypothesis-001", "gap-project-planning-okr-001"]

B:
  FlowSelections:
    - SelectedFlowId: "universal-nine-node-development-flow-3.4" | TrustState: "deterministic_generic" | ExecutionOrder: "parallel"
    - SelectedFlowId: "universal-nine-node-development-flow-4.1" | TrustState: "deterministic_generic" | ExecutionOrder: "parallel"
    - SelectedFlowId: "universal-nine-node-development-flow-5.1" | TrustState: "deterministic_generic" | ExecutionOrder: "parallel"
    - SelectedFlowId: "universal-nine-node-development-flow-2.4" | TrustState: "deterministic_generic" | ExecutionOrder: "parallel"
    - SelectedFlowId: "universal-nine-node-development-flow-6.1" | TrustState: "deterministic_generic" | ExecutionOrder: "last (QA-last)"
  CapabilityGapWarnings: ["gap-project-planning-feature-flag-001", "gap-project-planning-hypothesis-001", "gap-project-planning-okr-001"]

---

## B36-0D-036-004 — TrustState canonical rule for all B36 files

A:
  Task: "State the TrustState rule for all B36 flow selections."

B:
  TrustStateRule:
    knowledge_edge: true → TrustState: "knowledge_edge_synthetic"
    Any sub_phase_id (null-registered) → TrustState: "deterministic_generic"
  Note: "B36 has no specifically registered sub-phases. All non-knowledge-edge sub-phases
    use the universal nine-node flow with TrustState: 'deterministic_generic'."
  Applies to: all B36 files 001–060.
