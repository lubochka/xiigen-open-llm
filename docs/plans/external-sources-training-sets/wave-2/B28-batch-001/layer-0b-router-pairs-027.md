# Layer 0-B — Router Pairs (027) — RegressionGate
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-027-001 — "RegressionGate" → TEST → TestingFlow

A: ["TEST"] | "pass RegressionGate" | [TestingFlow]
  Teaching: RegressionGate fires TEST; routes to TestingFlow.

## B28-router-027-002 — TEST+QA → TestingFlow + ReleaseGateFlow71

A: ["TEST", "QA"] | "regression tests + QA sign-off" | [TestingFlow, ReleaseGateFlow71]
  Teaching: TEST → TestingFlow; QA → ReleaseGateFlow71. QA never drives TransitionType.

## B28-router-027-003 — TEST+IMPLEMENT → TestingFlow + ImplementationFlow41

A: ["TEST", "IMPLEMENT"] | "write regression tests + implement fix + service" | [TestingFlow, ImplementationFlow41] | feature_addition
  Teaching: TEST → TestingFlow; IMPLEMENT → ImplementationFlow41; feature_addition (IMPLEMENT present).

## B28-router-027-004 — DOCUMENT+QA → documentation (no IMPLEMENT)

A: ["DOCUMENT", "QA"] | "document criteria + QA gate" | [DocumentationFlow62, ReleaseGateFlow71] | documentation
  Teaching: No IMPLEMENT → TransitionType: documentation.

## B28-router-027-005 — pure gap → GapFlow → knowledge_edge

A: [] | "trigger RegressionGate automatically" → GapFlow | knowledge_edge
  Teaching: "trigger" alone is gap → GapFlow → knowledge_edge. Not GapHandler.
