# Layer 0-B — Router Pairs (034) — StaticAnalysisGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-034-001 — "StaticAnalysisGate" → TEST → TestingFlow

A: ["TEST"] | "pass StaticAnalysisGate" | [TestingFlow]
  Teaching: Static analysis gate fires TEST; routes to TestingFlow.

## B28-router-034-002 — TEST+QA → TestingFlow + ReleaseGateFlow71

A: ["TEST", "QA"] | "run static analysis + QA sign-off" | [TestingFlow, ReleaseGateFlow71]
  Teaching: TEST → TestingFlow; QA → ReleaseGateFlow71.

## B28-router-034-003 — IMPLEMENT+DOCUMENT → ImplementationFlow41 + DocumentationFlow62

A: ["IMPLEMENT", "DOCUMENT"] | "implement linting service + document analysis criteria" | [ImplementationFlow41, DocumentationFlow62] | feature_addition
  Teaching: IMPLEMENT drives feature_addition.

## B28-router-034-004 — SECURITY+TEST → SecurityFlow6x + TestingFlow

A: ["SECURITY", "TEST"] | "secure analysis config + validate findings" | [SecurityFlow6x, TestingFlow] | documentation
  Teaching: No IMPLEMENT → documentation.

## B28-router-034-005 — pure gap → knowledge_edge

A: [] | "configure StaticAnalysisGate on pull requests" → GapFlow | knowledge_edge
  Teaching: "configure" → gap → knowledge_edge.
