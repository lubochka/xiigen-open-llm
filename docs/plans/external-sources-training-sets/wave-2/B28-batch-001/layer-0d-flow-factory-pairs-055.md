# Layer 0-D — Flow Factory Pairs (055) — DatabaseMigrationGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-055-001 — TEST: "database migration gate" → TestingFlow, documentation

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: Migration gate fires TEST → TestingFlow. No IMPLEMENT = documentation.

## B28-factory-055-002 — IMPLEMENT+TEST+QA → three flows, feature_addition

A: ["IMPLEMENT", "TEST", "QA"] | [ImplementationFlow41, TestingFlow, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-factory-055-003 — TEST deduplicated: validate+verify → one TestingFlow

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: "validate" and "verify" both fire TEST; deduplicate to one TestingFlow.

## B28-factory-055-004 — TEST+DOCUMENT+QA → three flows, documentation

A: ["TEST", "DOCUMENT", "QA"] | [TestingFlow, DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: TEST+DOCUMENT without IMPLEMENT → documentation. QA last.

## B28-factory-055-005 — Gap: "rollback migration path configuration" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Migration configuration vocabulary is gap.
