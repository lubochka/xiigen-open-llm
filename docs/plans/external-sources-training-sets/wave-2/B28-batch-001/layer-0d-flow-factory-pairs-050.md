# Layer 0-D — Flow Factory Pairs (050) — SyntheticMonitoring
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-050-001 — Gap: "synthetic monitoring" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Monitoring setup vocabulary is gap. GapFlow only.

## B28-factory-050-002 — IMPLEMENT+TEST+QA → three flows, feature_addition

A: ["IMPLEMENT", "TEST", "QA"] | [ImplementationFlow41, TestingFlow, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-factory-050-003 — TEST+QA → two flows, documentation

A: ["TEST", "QA"] | [TestingFlow, ReleaseGateFlow71] | documentation
  Teaching: TEST without IMPLEMENT → documentation. QA last.

## B28-factory-050-004 — Gap: "configure probes" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Monitoring probe configuration is gap.

## B28-factory-050-005 — Gap: "alerting configuration" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Alert configuration vocabulary is gap.
