# Layer 0-D — Flow Factory Pairs (044) — EnvironmentPromotion
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-044-001 — Gap: "promote artifact" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Promotion vocabulary is gap. GapFlow only.

## B28-factory-044-002 — IMPLEMENT+QA → two flows, feature_addition

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. QA last gate.

## B28-factory-044-003 — DOCUMENT+QA → two flows, documentation

A: ["DOCUMENT", "QA"] | [DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: DOCUMENT without IMPLEMENT → documentation. QA last.

## B28-factory-044-004 — ARCHITECTURE+IMPLEMENT+TEST+QA → four flows, feature_addition

A: ["ARCHITECTURE", "IMPLEMENT", "TEST", "QA"] | [ArchitectureFlow31, ImplementationFlow41, TestingFlow, ReleaseGateFlow71] | feature_addition
  Teaching: Four flows. IMPLEMENT drives feature_addition. QA last.

## B28-factory-044-005 — Gap: "dev to staging to production" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Full environment chain vocabulary is gap.
