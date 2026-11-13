# Layer 0-B — Router Pairs (013)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-013-001 — QA+IMPLEMENT → 7.1, 4.1

A: ["QA", "IMPLEMENT"] | Blue-green service + QA gate | [ReleaseGateFlow71, ImplementationFlow41]

## B28-router-013-002 — QA alone → 7.1

A: ["QA"] | Blue-green acceptance gate | [ReleaseGateFlow71] | no TransitionType contribution from QA

## B28-router-013-003 — QA+IMPLEMENT+DOCUMENT → 7.1, 4.1, DocumentationFlow62

A: ["QA", "IMPLEMENT", "DOCUMENT"] | Blue-green runbook | 3 flows | feature_addition

## B28-router-013-004 — QA+TEST+IMPLEMENT → 7.1, 5.1, 4.1

A: ["QA", "TEST", "IMPLEMENT"] | Blue-green smoke validation | 3 flows | feature_addition

## B28-router-013-005 — gap: blue-green cutover

A: [] | "switch traffic to green" → GapFlow | knowledge_edge (not SCHEDULE_CAPABILITY_LEARNING)
