# Layer 0-D — Flow Factory Pairs (042) — RollbackTrigger
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-042-001 — Gap: "rollback" alone → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Rollback vocabulary is gap. GapFlow only.

## B28-factory-042-002 — IMPLEMENT+TEST → two flows, feature_addition

A: ["IMPLEMENT", "TEST"] | [ImplementationFlow41, TestingFlow] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. TEST adds TestingFlow.

## B28-factory-042-003 — SECURITY+QA → two flows, documentation

A: ["SECURITY", "QA"] | [SecurityFlow6x, ReleaseGateFlow71] | documentation
  Teaching: SECURITY without IMPLEMENT → documentation. QA last.

## B28-factory-042-004 — IMPLEMENT+DOCUMENT+QA → three flows, feature_addition

A: ["IMPLEMENT", "DOCUMENT", "QA"] | [ImplementationFlow41, DocumentationFlow62, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-factory-042-005 — Gap: "rollback threshold/policy configuration" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Rollback configuration vocabulary is gap.
