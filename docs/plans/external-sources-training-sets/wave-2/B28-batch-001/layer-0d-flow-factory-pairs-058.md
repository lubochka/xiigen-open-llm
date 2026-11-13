# Layer 0-D — Flow Factory Pairs (058) — SLAVerification
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-058-001 — TEST: "SLA verification" → TestingFlow, documentation

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: "SLA verification" fires TEST → TestingFlow. No IMPLEMENT = documentation.

## B28-factory-058-002 — IMPLEMENT+TEST+QA → three flows, feature_addition

A: ["IMPLEMENT", "TEST", "QA"] | [ImplementationFlow41, TestingFlow, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-factory-058-003 — TEST+DOCUMENT+QA → three flows, documentation

A: ["TEST", "DOCUMENT", "QA"] | [TestingFlow, DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: TEST+DOCUMENT without IMPLEMENT → documentation. QA last.

## B28-factory-058-004 — Gap: "SLA compliance monitoring" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: "SLA compliance" alone without verification verb is gap.

## B28-factory-058-005 — Gap: "uptime, error budget tracking" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: SLA metric vocabulary is gap.
