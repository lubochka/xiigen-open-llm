# Layer 0-D — Flow Factory Pairs (052) — CapacityVerification
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-052-001 — TEST: "capacity verification" → TestingFlow, documentation

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: "capacity verification" fires TEST → TestingFlow. No IMPLEMENT = documentation.

## B28-factory-052-002 — IMPLEMENT+TEST+QA → three flows, feature_addition

A: ["IMPLEMENT", "TEST", "QA"] | [ImplementationFlow41, TestingFlow, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-factory-052-003 — TEST: "verify scaling" mixed with gap → TestingFlow only

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: "configure scaling" is gap; "verify scaling" fires TEST. Only TEST flow is selected.

## B28-factory-052-004 — TEST+DOCUMENT+QA → three flows, documentation

A: ["TEST", "DOCUMENT", "QA"] | [TestingFlow, DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: TEST+DOCUMENT without IMPLEMENT → documentation. QA last.

## B28-factory-052-005 — Gap: "capacity plan review" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Capacity planning review vocabulary is gap.
