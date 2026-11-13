# Layer 0-D — Flow Factory Pairs (025) — SmokeTestSuite
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-025-001 — TEST alone → TestingFlow, no TransitionType

A: ["TEST"] | [TestingFlow] | One flow | TransitionType: documentation
  Teaching: TEST alone → documentation (no IMPLEMENT present).

## B28-factory-025-002 — TEST+QA → TestingFlow + ReleaseGateFlow71 → documentation

A: ["TEST", "QA"] | [TestingFlow, ReleaseGateFlow71] | Two flows | documentation
  Teaching: TEST without IMPLEMENT → documentation. QA adds ReleaseGateFlow71 gate.

## B28-factory-025-003 — IMPLEMENT+TEST → feature_addition

A: ["IMPLEMENT", "TEST"] | [ImplementationFlow41, TestingFlow] | Two flows | feature_addition
  Teaching: IMPLEMENT drives feature_addition regardless of TEST presence.

## B28-factory-025-004 — DOCUMENT+QA → documentation

A: ["DOCUMENT", "QA"] | [DocumentationFlow62, ReleaseGateFlow71] | Two flows | documentation
  Teaching: No IMPLEMENT → documentation.

## B28-factory-025-005 — pure gap → GapFlow → knowledge_edge

A: [] | GapFlow | knowledge_edge | not SCHEDULE_CAPABILITY_LEARNING | not GapHandler
  Teaching: "trigger smoke test pipeline" → gap → knowledge_edge.
