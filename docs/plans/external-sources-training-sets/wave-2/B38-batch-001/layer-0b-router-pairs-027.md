# Layer 0-B — Router Pairs (027)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-027-001 — NightlyBuild routing: QA+IMPLEMENT

Scenario: NightlyBuildPipelineValidation
PredicateSet: ["QA","IMPLEMENT"]
Routing:
- IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
- QA → Phase 7.1 (ReleaseGateFlow71) — QA last

RoutingTable:
| Predicate | Phase | Flow | Order |
|---|---|---|---|
| IMPLEMENT | 4.1 | BackendServiceFlow41 | 1 |
| QA | 7.1 | ReleaseGateFlow71 | 2 |

DecisionCount: 2
TransitionType: feature_addition

Gate B: DecisionCount == 2 AND TransitionType == "feature_addition"

---

## B38-0B-027-002 — NightlyBuild knowledge edge routing

Scenario: NightlyBuildScheduleKnowledgeEdge
PredicateSet: []
CapabilityGapWarnings: []
ResultState: knowledge_edge
DecisionCount: 1 (sentinel — no routing decisions)

Gate B: PredicateSet == [] AND ResultState == "knowledge_edge"

---

## B38-0B-027-003 — WeeklyRelease routing: QA+IMPLEMENT

Scenario: WeeklyReleaseBuildApproval
PredicateSet: ["QA","IMPLEMENT"]
Routing:
- IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
- QA → Phase 7.1 (ReleaseGateFlow71) — QA last

RoutingTable:
| Predicate | Phase | Flow | Order |
|---|---|---|---|
| IMPLEMENT | 4.1 | BackendServiceFlow41 | 1 |
| QA | 7.1 | ReleaseGateFlow71 | 2 |

DecisionCount: 2
TransitionType: feature_addition
CapabilityGapWarnings: []

Gate B: DecisionCount == 2 AND TransitionType == "feature_addition"

---

## B38-0B-027-004 — WeeklyRelease routing: DOCUMENT only

Scenario: WeeklyReleaseProcessDocumentation
PredicateSet: ["DOCUMENT"]
Routing:
- DOCUMENT → Phase 2.1 (DocumentationFlow)

RoutingTable:
| Predicate | Phase | Flow | Order |
|---|---|---|---|
| DOCUMENT | 2.1 | DocumentationFlow | 1 |

DecisionCount: 1
TransitionType: no_contribution

Gate B: DecisionCount == 1 AND Phase 2.1 present AND TransitionType == "no_contribution"
