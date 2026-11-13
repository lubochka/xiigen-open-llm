# Layer 0-D — Flow Factory Pairs (033) — CodeCoverageGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-033-001 — TEST alone → TestingFlow → documentation

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: "pass CodeCoverageGate" → TEST → documentation.

## B28-factory-033-002 — TEST+QA → TestingFlow + ReleaseGateFlow71 → documentation

A: ["TEST", "QA"] | [TestingFlow, ReleaseGateFlow71] | documentation
  Teaching: No IMPLEMENT → documentation.

## B28-factory-033-003 — TEST+IMPLEMENT → feature_addition

A: ["TEST", "IMPLEMENT"] | [TestingFlow, ImplementationFlow41] | feature_addition
  Teaching: "write tests + implement service logic" → IMPLEMENT drives feature_addition.

## B28-factory-033-004 — DOCUMENT+QA → DocumentationFlow62 + ReleaseGateFlow71 → documentation

A: ["DOCUMENT", "QA"] | [DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: "document coverage criteria + QA approve" → documentation.

## B28-factory-033-005 — pure gap → GapFlow → knowledge_edge

A: [] | GapFlow | knowledge_edge
  Teaching: "enable code coverage in CI pipeline" → gap → knowledge_edge.
