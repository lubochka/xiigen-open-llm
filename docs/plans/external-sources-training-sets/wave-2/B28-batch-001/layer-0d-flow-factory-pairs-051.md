# Layer 0-D — Flow Factory Pairs (051) — ChaosEngineeringGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-051-001 — TEST: "chaos engineering" → TestingFlow, documentation

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: "chaos engineering" fires TEST → TestingFlow. No IMPLEMENT = documentation.

## B28-factory-051-002 — IMPLEMENT+TEST+QA → three flows, feature_addition

A: ["IMPLEMENT", "TEST", "QA"] | [ImplementationFlow41, TestingFlow, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-factory-051-003 — TEST+DOCUMENT+QA → three flows, documentation

A: ["TEST", "DOCUMENT", "QA"] | [TestingFlow, DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: TEST+DOCUMENT without IMPLEMENT → documentation. QA last.

## B28-factory-051-004 — Gap: "fault injection configuration" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Chaos configuration vocabulary is gap.

## B28-factory-051-005 — Gap: "steady state hypothesis definition" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Chaos methodology vocabulary is gap.
