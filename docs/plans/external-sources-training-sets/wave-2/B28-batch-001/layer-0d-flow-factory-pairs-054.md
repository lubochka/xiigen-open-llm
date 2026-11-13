# Layer 0-D — Flow Factory Pairs (054) — LoadBalancerSwitch
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: factory_selection

---

## B28-factory-054-001 — Gap: "load balancer switch" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Load balancer routing vocabulary is gap. GapFlow only.

## B28-factory-054-002 — IMPLEMENT+QA → two flows, feature_addition

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. QA last gate.

## B28-factory-054-003 — TEST+QA → two flows, documentation

A: ["TEST", "QA"] | [TestingFlow, ReleaseGateFlow71] | documentation
  Teaching: TEST without IMPLEMENT → documentation. QA last.

## B28-factory-054-004 — SECURITY+QA → two flows, documentation

A: ["SECURITY", "QA"] | [SecurityFlow6x, ReleaseGateFlow71] | documentation
  Teaching: SECURITY without IMPLEMENT → documentation. QA last.

## B28-factory-054-005 — Gap: "weighted routing, round-robin configuration" → GapFlow

A: [] | GapFlow | knowledge_edge
  Teaching: Load balancing strategy configuration is gap.
