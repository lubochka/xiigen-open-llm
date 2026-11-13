# Layer 0-B — Router Pairs (023)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-023-001 — QA+IMPLEMENT → 7.1, 4.1

A: ["QA", "IMPLEMENT"] | Hotfix service build + QA gate | [ReleaseGateFlow71, ImplementationFlow41] | feature_addition

## B28-router-023-002 — QA+IMPLEMENT+TEST → 7.1, 4.1, 5.1

A: ["QA", "IMPLEMENT", "TEST"] | Hotfix + regression suite + QA sign-off | 3 flows | feature_addition

## B28-router-023-003 — QA alone → 7.1

A: ["QA"] | Emergency hotfix gate before cutover | [ReleaseGateFlow71] | no TransitionType contribution

## B28-router-023-004 — QA+IMPLEMENT+TEST+SECURITY → 7.1, 4.1, 5.1, 6.x

A: ["QA", "IMPLEMENT", "TEST", "SECURITY"] | Secure hotfix release | 4 flows | feature_addition

## B28-router-023-005 — gap: hotfix deployment command

A: [] | "deploy hotfix to production immediately" → GapFlow | knowledge_edge
