# Layer 0-D — Flow Factory Pairs (029) — LoadTestGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-029-001 — TEST alone → TestingFlow → documentation

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: "clear LoadTestGate" → TEST → documentation.

## B28-factory-029-002 — TEST+QA → TestingFlow + ReleaseGateFlow71 → documentation

A: ["TEST", "QA"] | [TestingFlow, ReleaseGateFlow71] | documentation
  Teaching: No IMPLEMENT → documentation.

## B28-factory-029-003 — IMPLEMENT+TEST → feature_addition

A: ["IMPLEMENT", "TEST"] | [ImplementationFlow41, TestingFlow] | feature_addition
  Teaching: "build harness + implement service + run tests" → IMPLEMENT drives feature_addition.

## B28-factory-029-004 — DOCUMENT+SECURITY → DocumentationFlow62 + SecurityFlow6x → documentation

A: ["DOCUMENT", "SECURITY"] | [DocumentationFlow62, SecurityFlow6x] | documentation
  Teaching: No IMPLEMENT → documentation. Two flows.

## B28-factory-029-005 — pure gap → GapFlow → knowledge_edge

A: [] | GapFlow | knowledge_edge
  Teaching: "schedule LoadTestGate before cutover" → gap → knowledge_edge.
