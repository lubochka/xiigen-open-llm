# Layer 0-B — Router Pairs (016)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-016-001 — QA+DOCUMENT → 7.1, DocumentationFlow62

A: ["QA", "DOCUMENT"] | Feature flag criteria documentation + QA sign-off | 2 flows | documentation

## B28-router-016-002 — QA+IMPLEMENT → 7.1, 4.1

A: ["QA", "IMPLEMENT"] | Feature flag service build + QA | [ReleaseGateFlow71, ImplementationFlow41] | feature_addition

## B28-router-016-003 — QA+IMPLEMENT+TEST → 7.1, 4.1, 5.1

A: ["QA", "IMPLEMENT", "TEST"] | Flag evaluation logic + flag resolution tests + QA | 3 flows | feature_addition

## B28-router-016-004 — QA alone → 7.1

A: ["QA"] | Feature flag rollout gate | [ReleaseGateFlow71] | no TransitionType contribution

## B28-router-016-005 — gap: feature flag toggle

A: [] | "enable dark-mode flag for 10% of users" → GapFlow | knowledge_edge
