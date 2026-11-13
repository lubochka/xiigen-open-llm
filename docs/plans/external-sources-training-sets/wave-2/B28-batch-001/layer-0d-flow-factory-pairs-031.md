# Layer 0-D — Flow Factory Pairs (031) — DependencyCheckGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-031-001 — SECURITY alone → SecurityFlow6x → documentation

A: ["SECURITY"] | [SecurityFlow6x] | documentation
  Teaching: "pass DependencyCheckGate" → SECURITY → documentation.

## B28-factory-031-002 — SECURITY+QA → SecurityFlow6x + ReleaseGateFlow71 → documentation

A: ["SECURITY", "QA"] | [SecurityFlow6x, ReleaseGateFlow71] | documentation
  Teaching: No IMPLEMENT → documentation.

## B28-factory-031-003 — IMPLEMENT+DOCUMENT → ImplementationFlow41 + DocumentationFlow62 → feature_addition

A: ["IMPLEMENT", "DOCUMENT"] | [ImplementationFlow41, DocumentationFlow62] | feature_addition
  Teaching: IMPLEMENT drives feature_addition.

## B28-factory-031-004 — SECURITY+TEST → SecurityFlow6x + TestingFlow → documentation

A: ["SECURITY", "TEST"] | [SecurityFlow6x, TestingFlow] | documentation
  Teaching: No IMPLEMENT → documentation. Both flows.

## B28-factory-031-005 — pure gap → GapFlow → knowledge_edge

A: [] | GapFlow | knowledge_edge
  Teaching: "update npm dependencies before rollout" → gap → knowledge_edge.
