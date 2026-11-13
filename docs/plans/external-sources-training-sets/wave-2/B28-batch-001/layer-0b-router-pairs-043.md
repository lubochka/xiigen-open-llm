# Layer 0-B — Router Pairs (043) — HotfixPipeline
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-043-001 — "hotfix" alone → gap → knowledge_edge

A: [] | "cut a hotfix branch and push to production" → GapFlow | knowledge_edge
  Teaching: "hotfix" alone is gap vocabulary. No flows.

## B28-router-043-002 — IMPLEMENT+QA for hotfix implementation

A: ["IMPLEMENT", "QA"] | [ImplementationFlow41, ReleaseGateFlow71] | feature_addition
  Teaching: IMPLEMENT drives feature_addition. QA last gate.

## B28-router-043-003 — IMPLEMENT+TEST+QA for hotfix with regression tests

A: ["IMPLEMENT", "TEST", "QA"] | [ImplementationFlow41, TestingFlow, ReleaseGateFlow71] | feature_addition
  Teaching: Three flows. IMPLEMENT drives feature_addition. QA last.

## B28-router-043-004 — SECURITY+TEST+QA for security hotfix

A: ["SECURITY", "TEST", "QA"] | [SecurityFlow6x, TestingFlow, ReleaseGateFlow71] | documentation
  Teaching: SECURITY+TEST without IMPLEMENT → documentation. QA last.

## B28-router-043-005 — Pure gap: "emergency fix, critical patch" → knowledge_edge

A: [] | "deploy emergency fix and apply critical patch" → GapFlow | knowledge_edge
  Teaching: Emergency and patch deployment vocabulary is gap.
