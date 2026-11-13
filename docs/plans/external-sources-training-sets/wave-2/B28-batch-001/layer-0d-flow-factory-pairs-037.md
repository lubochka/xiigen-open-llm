# Layer 0-D — Flow Factory Pairs (037) — PipelineOrchestration
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-037-001 — Gap: "orchestrate pipeline" → GapFlow

A: [] | GapFlow | knowledge_edge | not SCHEDULE_CAPABILITY_LEARNING | not GapHandler
  Teaching: Pure gap → GapFlow → knowledge_edge only.

## B28-factory-037-002 — IMPLEMENT+QA → two flows, feature_addition

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT+QA always maps to exactly two flows. IMPLEMENT drives feature_addition.

## B28-factory-037-003 — ARCHITECTURE+IMPLEMENT+QA → three flows

A: ["ARCHITECTURE", "IMPLEMENT", "QA"] | [ArchitectureFlow31, ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: Three predicates → three flows. IMPLEMENT drives feature_addition.

## B28-factory-037-004 — TEST+DOCUMENT+QA → three flows, documentation

A: ["TEST", "DOCUMENT", "QA"] | [TestingFlow, DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: TEST+DOCUMENT without IMPLEMENT → documentation TransitionType. QA adds ReleaseGateFlow71.

## B28-factory-037-005 — Gap: "CI/CD pipeline configuration" → GapFlow

A: [] | GapFlow | knowledge_edge | not SCHEDULE_CAPABILITY_LEARNING
  Teaching: Pipeline configuration is gap. GapFlow is the only output.
