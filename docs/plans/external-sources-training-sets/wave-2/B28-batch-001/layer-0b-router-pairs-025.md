# Layer 0-B — Router Pairs (025) — SmokeTestSuite
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-025-001 — "SmokeTestSuite" → TEST → routes to TestingFlow

A: ["TEST"] | "run SmokeTestSuite" | [TestingFlow]
  Teaching: SmokeTestSuite fires TEST which routes to TestingFlow.

## B28-router-025-002 — TEST+QA → TestingFlow + ReleaseGateFlow71

A: ["TEST", "QA"] | "run smoke tests + QA sign-off" | [TestingFlow, ReleaseGateFlow71]
  Teaching: TEST routes to TestingFlow; QA routes to ReleaseGateFlow71. Two flows.

## B28-router-025-003 — IMPLEMENT+TEST → ImplementationFlow41 + TestingFlow

A: ["IMPLEMENT", "TEST"] | "build smoke harness + run tests" | [ImplementationFlow41, TestingFlow]
  Teaching: IMPLEMENT routes to 4.1; TEST routes to TestingFlow. TransitionType: feature_addition (IMPLEMENT present).

## B28-router-025-004 — pure gap → GapFlow → knowledge_edge

A: [] | "trigger smoke test pipeline" → GapFlow | knowledge_edge
  Teaching: "trigger" alone → gap → GapFlow → knowledge_edge. Not SCHEDULE_CAPABILITY_LEARNING.

## B28-router-025-005 — DOCUMENT+QA: no IMPLEMENT → documentation

A: ["DOCUMENT", "QA"] | "document criteria + QA sign-off" | [DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: Without IMPLEMENT, TransitionType: documentation.
