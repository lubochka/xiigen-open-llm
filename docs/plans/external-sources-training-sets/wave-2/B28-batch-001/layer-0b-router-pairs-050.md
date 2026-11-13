# Layer 0-B — Router Pairs (050) — SyntheticMonitoring
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-050-001 — "synthetic monitoring" → gap → knowledge_edge

A: [] | "enable synthetic monitoring on production endpoints" → GapFlow | knowledge_edge
  Teaching: Monitoring setup vocabulary is gap. No flows.

## B28-router-050-002 — IMPLEMENT+TEST+QA for synthetic monitor service

A: ["IMPLEMENT", "TEST", "QA"] | [ImplementationFlow41, TestingFlow, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-router-050-003 — TEST+QA for synthetic test suite execution

A: ["TEST", "QA"] | [TestingFlow, ReleaseGateFlow71] | documentation
  Teaching: TEST without IMPLEMENT → documentation. QA last.

## B28-router-050-004 — Pure gap: "configure synthetic transaction probes" → knowledge_edge

A: [] | "configure synthetic transaction probes for checkout flow" → GapFlow | knowledge_edge
  Teaching: Monitoring probe configuration vocabulary is gap.

## B28-router-050-005 — Pure gap: "alerting for monitoring failures" → knowledge_edge

A: [] | "set up alerting for synthetic monitoring failures" → GapFlow | knowledge_edge
  Teaching: Alerting configuration vocabulary is gap.
