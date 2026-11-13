# Layer 0-B — Router Pairs (055) — DatabaseMigrationGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-055-001 — TEST: "database migration gate" → TestingFlow

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: "database migration gate" fires TEST → TestingFlow. No IMPLEMENT = documentation.

## B28-router-055-002 — IMPLEMENT+TEST+QA for migration runner service

A: ["IMPLEMENT", "TEST", "QA"] | [ImplementationFlow41, TestingFlow, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-router-055-003 — TEST: "validate schema migration" + "verify data migration" → TestingFlow

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: Both "validate" and "verify" fire TEST (deduplicated). Single TestingFlow.

## B28-router-055-004 — TEST+DOCUMENT+QA for migration gate with criteria

A: ["TEST", "DOCUMENT", "QA"] | [TestingFlow, DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: TEST+DOCUMENT without IMPLEMENT → documentation. QA last.

## B28-router-055-005 — Pure gap: "rollback migration path, migration timeout" → knowledge_edge

A: [] | "configure rollback migration path and set migration timeout" → GapFlow | knowledge_edge
  Teaching: Migration configuration vocabulary is gap.
