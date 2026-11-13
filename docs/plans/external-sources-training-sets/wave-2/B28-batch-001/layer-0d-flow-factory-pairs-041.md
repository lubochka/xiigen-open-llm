# Layer 0-D — Flow Factory Pairs (041) — ManualIntervention
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-041-001 — Gap: "manual intervention" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Manual intervention vocabulary is gap. GapFlow only.

## B28-factory-041-002 — IMPLEMENT+TEST → two flows, feature_addition

A: ["IMPLEMENT", "TEST"] | [ImplementationFlow41, TestingFlow] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. No QA = no ReleaseGateFlow71.

## B28-factory-041-003 — DOCUMENT+QA → two flows, documentation

A: ["DOCUMENT", "QA"] | [DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: DOCUMENT without IMPLEMENT → documentation. QA last.

## B28-factory-041-004 — ARCHITECTURE+IMPLEMENT+QA → three flows, feature_addition

A: ["ARCHITECTURE", "IMPLEMENT", "QA"] | [ArchitectureFlow31, ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-factory-041-005 — Gap: "configure pipeline pause/resume" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Pipeline pause/resume configuration is gap.
