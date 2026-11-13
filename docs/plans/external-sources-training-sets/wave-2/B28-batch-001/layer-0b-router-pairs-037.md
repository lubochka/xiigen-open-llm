# Layer 0-B — Router Pairs (037) — PipelineOrchestration
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-037-001 — "orchestrate pipeline" → gap → knowledge_edge

A: [] | "orchestrate the release pipeline" → GapFlow | knowledge_edge
  Teaching: Pure gap → knowledge_edge. No flows selected.

## B28-router-037-002 — IMPLEMENT+QA: ImplementationFlow41 + ReleaseGateFlow71

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. QA adds ReleaseGateFlow71 as final gate.

## B28-router-037-003 — ARCHITECTURE+IMPLEMENT+QA: three flows

A: ["ARCHITECTURE", "IMPLEMENT", "QA"] | [ArchitectureFlow31, ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: ARCHITECTURE first, IMPLEMENT parallel, QA last gate. IMPLEMENT drives feature_addition.

## B28-router-037-004 — TEST+DOCUMENT+QA for orchestration with criteria

A: ["TEST", "DOCUMENT", "QA"] | [TestingFlow, DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: TEST+DOCUMENT without IMPLEMENT → documentation TransitionType. QA last.

## B28-router-037-005 — Pure gap: "configure CI/CD pipeline" → knowledge_edge

A: [] | "configure CI/CD pipeline for release orchestration" → GapFlow | knowledge_edge
  Teaching: CI/CD pipeline configuration is gap. No flows. No TransitionType.
