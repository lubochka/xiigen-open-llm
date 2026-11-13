# Layer 0-B — Router Pairs (033) — CodeCoverageGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-033-001 — "CodeCoverageGate" → TEST → TestingFlow

A: ["TEST"] | "pass CodeCoverageGate" | [TestingFlow]
  Teaching: Code coverage gate fires TEST; routes to TestingFlow.

## B28-router-033-002 — TEST+QA → TestingFlow + ReleaseGateFlow71

A: ["TEST", "QA"] | "verify coverage + QA sign-off" | [TestingFlow, ReleaseGateFlow71]
  Teaching: TEST → TestingFlow; QA → ReleaseGateFlow71. QA does not drive TransitionType.

## B28-router-033-003 — TEST+IMPLEMENT → TestingFlow + ImplementationFlow41

A: ["TEST", "IMPLEMENT"] | "write tests + implement service logic" | [TestingFlow, ImplementationFlow41] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. Both flows instantiated.

## B28-router-033-004 — DOCUMENT+QA → DocumentationFlow62 + ReleaseGateFlow71

A: ["DOCUMENT", "QA"] | "document coverage criteria + QA approve policy" | [DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: No IMPLEMENT → documentation.

## B28-router-033-005 — pure gap → knowledge_edge

A: [] | "enable code coverage reporting in CI pipeline" → GapFlow | knowledge_edge
  Teaching: "enable" + "CI pipeline" → gap → knowledge_edge.
