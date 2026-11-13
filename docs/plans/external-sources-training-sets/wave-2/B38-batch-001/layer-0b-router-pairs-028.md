# Layer 0-B — Router Pairs (028)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-028-001 — MonthlyRelease routing: QA+DOCUMENT with gap

Scenario: MonthlyReleaseGateWithDeploymentGap
PredicateSet: ["QA","DOCUMENT"]
CapabilityGapWarnings: ["deployment"]
Routing:
- DOCUMENT → Phase 2.1 (DocumentationFlow)
- QA → Phase 7.1 (ReleaseGateFlow71) — QA last

RoutingTable:
| Predicate | Phase | Flow | Order |
|---|---|---|---|
| DOCUMENT | 2.1 | DocumentationFlow | 1 |
| QA | 7.1 | ReleaseGateFlow71 | 2 |

DecisionCount: 2
TransitionType: no_contribution
Note: CapabilityGapWarnings do NOT generate routing decisions.

Gate B: DecisionCount == 2 AND "deployment" in CapabilityGapWarnings AND TransitionType == "no_contribution"

---

## B38-0B-028-002 — MonthlyRelease routing: QA+IMPLEMENT

Scenario: MonthlyReleaseServiceApproval
PredicateSet: ["QA","IMPLEMENT"]
Routing:
- IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
- QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: feature_addition

Gate B: DecisionCount == 2 AND TransitionType == "feature_addition"

---

## B38-0B-028-003 — QuarterlyRelease routing: QA+IMPLEMENT+DOCUMENT+SECURITY

Scenario: QuarterlyReleaseFourPredicate
PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
Routing:
- IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
- DOCUMENT → Phase 2.1 (DocumentationFlow)
- SECURITY → Phase 3.4 (SecurityAuditFlow)
- QA → Phase 7.1 (ReleaseGateFlow71) — QA last

RoutingTable:
| Predicate | Phase | Flow | Order |
|---|---|---|---|
| IMPLEMENT | 4.1 | BackendServiceFlow41 | 1 |
| DOCUMENT | 2.1 | DocumentationFlow | 2 |
| SECURITY | 3.4 | SecurityAuditFlow | 3 |
| QA | 7.1 | ReleaseGateFlow71 | 4 |

DecisionCount: 4
TransitionType: feature_addition

Gate B: DecisionCount == 4 AND all four phases present AND QA last AND TransitionType == "feature_addition"

---

## B38-0B-028-004 — QuarterlyRelease knowledge edge routing

Scenario: QuarterlyReleasePlanningKnowledgeEdge
PredicateSet: []
CapabilityGapWarnings: ["cutover"]
ResultState: knowledge_edge
DecisionCount: 1 (sentinel)

Gate B: PredicateSet == [] AND "cutover" in CapabilityGapWarnings AND ResultState == "knowledge_edge"
