# Layer 0-D — Flow Factory Pairs (027) — RegressionGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-027-001 — TEST alone → TestingFlow → documentation

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: "pass RegressionGate" → TEST → TestingFlow → documentation.

## B28-factory-027-002 — TEST+QA → TestingFlow + ReleaseGateFlow71 → documentation

A: ["TEST", "QA"] | [TestingFlow, ReleaseGateFlow71] | documentation
  Teaching: Two flows. No IMPLEMENT → documentation.

## B28-factory-027-003 — TEST+IMPLEMENT → feature_addition

A: ["TEST", "IMPLEMENT"] | [TestingFlow, ImplementationFlow41] | feature_addition
  Teaching: IMPLEMENT drives feature_addition over TEST.

## B28-factory-027-004 — DOCUMENT+QA → documentation

A: ["DOCUMENT", "QA"] | [DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: "document criteria + QA gate" → documentation.

## B28-factory-027-005 — pure gap → GapFlow → knowledge_edge

A: [] | GapFlow | knowledge_edge
  Teaching: "trigger RegressionGate automatically" → gap → knowledge_edge.
