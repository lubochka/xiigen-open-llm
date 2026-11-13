# Layer 0-D — Flow Factory Pairs (034) — StaticAnalysisGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-034-001 — TEST alone → TestingFlow → documentation

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: "pass StaticAnalysisGate" → TEST → documentation.

## B28-factory-034-002 — TEST+QA → TestingFlow + ReleaseGateFlow71 → documentation

A: ["TEST", "QA"] | [TestingFlow, ReleaseGateFlow71] | documentation
  Teaching: No IMPLEMENT → documentation.

## B28-factory-034-003 — IMPLEMENT+DOCUMENT → ImplementationFlow41 + DocumentationFlow62 → feature_addition

A: ["IMPLEMENT", "DOCUMENT"] | [ImplementationFlow41, DocumentationFlow62] | feature_addition
  Teaching: "implement linting service + document criteria" → IMPLEMENT drives feature_addition.

## B28-factory-034-004 — SECURITY+TEST → SecurityFlow6x + TestingFlow → documentation

A: ["SECURITY", "TEST"] | [SecurityFlow6x, TestingFlow] | documentation
  Teaching: "secure config + validate findings" → No IMPLEMENT → documentation.

## B28-factory-034-005 — pure gap → GapFlow → knowledge_edge

A: [] | GapFlow | knowledge_edge
  Teaching: "configure StaticAnalysisGate on pull requests" → gap → knowledge_edge.
