# Layer 0-B — Router Pairs (013)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-013-001 — [QA, DOCUMENT] routes to 7.1 + 2.1 (environment promotion)

PredicateSet: ["QA", "DOCUMENT"]
Scenario: EnvironmentPromotionGate
Input: "Verify that the staging environment meets all promotion criteria before approving."

Routing decisions:
1. DOCUMENT → Phase 2.1 (DocumentationFlow)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last (rule 12)

RoutingTable:
| Predicate | Target Phase | Flow | Order |
|---|---|---|---|
| DOCUMENT | 2.1 | DocumentationFlow | 1 |
| QA | 7.1 | ReleaseGateFlow71 | 2 |

DecisionCount: 2
TransitionType: no_contribution
OutputFiles: [promotion_criteria.md, release_gate_report.md]
DomainId: software-development

Note: "environment" does not generate a routing decision — it is context vocabulary, not a predicate trigger.

---

## B38-0B-013-002 — [QA, IMPLEMENT, DOCUMENT] routes to three phases

PredicateSet: ["QA", "IMPLEMENT", "DOCUMENT"]
Scenario: AuthServiceEnvironmentPromotion
Input: "Approve the auth service promotion from staging to production after verifying the gate criteria."

Routing decisions:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
2. DOCUMENT → Phase 2.1 (DocumentationFlow)
3. QA → Phase 7.1 (ReleaseGateFlow71) — QA last (rule 12)

DecisionCount: 3
TransitionType: feature_addition (IMPLEMENT present)

---

## B38-0B-013-003 — [IMPLEMENT] alone: single routing to 4.1

PredicateSet: ["IMPLEMENT"]
Scenario: EnvironmentPromotionBuildCheck
Input: "Execute the environment promotion runbook to deploy the build to prod."

Routing decision:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)

DecisionCount: 1
TransitionType: feature_addition (IMPLEMENT present)
CapabilityGapWarnings: ["execute", "runbook", "deploy"] — these do NOT generate routing decisions

Note: Gap warnings are metadata only; they do not generate routing entries in the routing table.

---

## B38-0B-013-004 — Zero predicates → knowledge_edge routing

PredicateSet: []
Scenario: EnvironmentCutoverGap
Input: "Run the environment cutover and monitor the promotion rollout."

Routing decision:
- PredictedPhaseId: null
- PredictedSubPhaseId: null
- KnowledgeEdge: true
- FailureClassification: "no_predicate_match_capability_gap"

DecisionCount: 1 (knowledge_edge sentinel)
TransitionType: N/A
CapabilityGapWarnings: ["cutover", "monitor", "rollout"]

Result: knowledge_edge — no routing to any active flow.
