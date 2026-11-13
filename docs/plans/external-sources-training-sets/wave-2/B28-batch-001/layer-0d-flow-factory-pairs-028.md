# Layer 0-D — Flow Factory Pairs (028) — PerformanceGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-028-001 — TEST alone → TestingFlow → documentation

A: ["TEST"] | [TestingFlow] | documentation
  Teaching: "pass PerformanceGate" → TEST → documentation.

## B28-factory-028-002 — TEST+QA → TestingFlow + ReleaseGateFlow71 → documentation

A: ["TEST", "QA"] | [TestingFlow, ReleaseGateFlow71] | documentation
  Teaching: No IMPLEMENT → documentation.

## B28-factory-028-003 — IMPLEMENT+DOCUMENT → ImplementationFlow41 + DocumentationFlow62 → feature_addition

A: ["IMPLEMENT", "DOCUMENT"] | [ImplementationFlow41, DocumentationFlow62] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. DOCUMENT adds DocumentationFlow62.

## B28-factory-028-004 — ARCHITECTURE+TEST (no IMPLEMENT) → documentation

A: ["ARCHITECTURE", "TEST"] | [ArchitectureFlow31, TestingFlow] | documentation
  Teaching: No IMPLEMENT → documentation even with ARCHITECTURE present.

## B28-factory-028-005 — pure gap → GapFlow → knowledge_edge

A: [] | GapFlow | knowledge_edge
  Teaching: "monitor performance after release" → gap → knowledge_edge.
