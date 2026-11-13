# Layer 0-B — Router Pairs (015)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-015-001 — QA+IMPLEMENT → 7.1, 4.1

A: ["QA", "IMPLEMENT"] | Rollback service build + QA gate | [ReleaseGateFlow71, ImplementationFlow41] | feature_addition

## B28-router-015-002 — QA+IMPLEMENT+TEST → 7.1, 4.1, 5.1

A: ["QA", "IMPLEMENT", "TEST"] | Rollback trigger + verification + QA sign-off | 3 flows | feature_addition

## B28-router-015-003 — QA+DOCUMENT → 7.1, DocumentationFlow62

A: ["QA", "DOCUMENT"] | Rollback runbook documentation + QA review | 2 flows | documentation

## B28-router-015-004 — QA+IMPLEMENT+TEST+DOCUMENT → 7.1, 4.1, 5.1, DocumentationFlow62

A: ["QA", "IMPLEMENT", "TEST", "DOCUMENT"] | Full rollback lifecycle | 4 flows | feature_addition

## B28-router-015-005 — gap: rollback execution

A: [] | "roll back to v2.1" → GapFlow | knowledge_edge (pure gap — rollback execution vocabulary)
