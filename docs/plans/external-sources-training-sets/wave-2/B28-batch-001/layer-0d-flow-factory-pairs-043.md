# Layer 0-D — Flow Factory Pairs (043) — HotfixPipeline
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-043-001 — Gap: "hotfix" alone → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: "hotfix" noun alone is gap. GapFlow only.

## B28-factory-043-002 — IMPLEMENT+QA → two flows, feature_addition

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. QA last gate.

## B28-factory-043-003 — IMPLEMENT+TEST+QA → three flows, feature_addition

A: ["IMPLEMENT", "TEST", "QA"] | [ImplementationFlow41, TestingFlow, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-factory-043-004 — SECURITY+TEST+QA → three flows, documentation

A: ["SECURITY", "TEST", "QA"] | [SecurityFlow6x, TestingFlow, ReleaseGateFlow71] | documentation
  Teaching: SECURITY+TEST without IMPLEMENT → documentation. QA last.

## B28-factory-043-005 — Gap: "emergency fix, critical patch" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Emergency patch deployment vocabulary is gap.
