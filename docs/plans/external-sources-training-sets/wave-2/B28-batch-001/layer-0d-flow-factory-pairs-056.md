# Layer 0-D — Flow Factory Pairs (056) — FeatureFlagFlip
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-056-001 — Gap: "feature flag flip" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Feature flag flip vocabulary is gap. GapFlow only.

## B28-factory-056-002 — IMPLEMENT+QA → two flows, feature_addition

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. QA last gate.

## B28-factory-056-003 — TEST+QA → two flows, documentation

A: ["TEST", "QA"] | [TestingFlow, ReleaseGateFlow71] | documentation
  Teaching: TEST without IMPLEMENT → documentation. QA last.

## B28-factory-056-004 — DOCUMENT+QA → two flows, documentation

A: ["DOCUMENT", "QA"] | [DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: DOCUMENT without IMPLEMENT → documentation. QA last.

## B28-factory-056-005 — Gap: "toggle flag state" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Flag toggle vocabulary is gap.
