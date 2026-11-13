# Layer 0-B — Router Pairs (014)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-014-001 — QA+IMPLEMENT → 7.1, 4.1

A: ["QA", "IMPLEMENT"] | Canary controller build + QA | [ReleaseGateFlow71, ImplementationFlow41] | feature_addition

## B28-router-014-002 — QA+IMPLEMENT+TEST → 7.1, 4.1, 5.1

A: ["QA", "IMPLEMENT", "TEST"] | Canary traffic split + acceptance tests + QA | 3 flows | feature_addition

## B28-router-014-003 — QA+DOCUMENT → 7.1, DocumentationFlow62

A: ["QA", "DOCUMENT"] | Canary release criteria documentation + QA sign-off | 2 flows | documentation

## B28-router-014-004 — QA+IMPLEMENT+TEST+DOCUMENT → 7.1, 4.1, 5.1, DocumentationFlow62

A: ["QA", "IMPLEMENT", "TEST", "DOCUMENT"] | Full canary lifecycle | 4 flows | feature_addition

## B28-router-014-005 — gap: canary percentage change

A: [] | "increase canary to 20%" → GapFlow | knowledge_edge
