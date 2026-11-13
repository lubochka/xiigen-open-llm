# Layer 0-B — Router Pairs (020)
# Batch: B28-batch-001 | Scenario: B28 — Release pipeline and deployment automation
# Production date: 2026-05-06 | pair_kind: router

---

## B28-router-020-001 — QA alone → 7.1

A: ["QA"] | A/B testing release gate | [ReleaseGateFlow71] | no TransitionType contribution from QA

## B28-router-020-002 — QA+IMPLEMENT → 7.1, 4.1

A: ["QA", "IMPLEMENT"] | A/B experiment assignment service + QA | [ReleaseGateFlow71, ImplementationFlow41] | feature_addition

## B28-router-020-003 — QA+TEST → 7.1, 5.1

A: ["QA", "TEST"] | A/B variant assignment verification + QA criteria | 2 flows | documentation
  Teaching: No IMPLEMENT → TransitionType: documentation (not feature_addition)

## B28-router-020-004 — QA+IMPLEMENT+DOCUMENT → 7.1, 4.1, DocumentationFlow62

A: ["QA", "IMPLEMENT", "DOCUMENT"] | Experiment framework + criteria docs + QA | 3 flows | feature_addition

## B28-router-020-005 — gap: A/B configuration

A: [] | "configure A/B split 50/50" → GapFlow | knowledge_edge
