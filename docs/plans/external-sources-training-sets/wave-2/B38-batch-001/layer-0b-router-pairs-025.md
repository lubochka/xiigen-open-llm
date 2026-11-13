# Layer 0-B — Router Pairs (025)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-025-001 — SemanticVersioning routing: QA alone

Scenario: SemanticVersioningApproval
PredicateSet: ["QA"]
Routing:
- QA → Phase 7.1 (ReleaseGateFlow71)

RoutingTable:
| Predicate | Phase | Flow | Order |
|---|---|---|---|
| QA | 7.1 | ReleaseGateFlow71 | 1 |

DecisionCount: 1
TransitionType: no_contribution
CapabilityGapWarnings: []

Gate B: DecisionCount == 1 AND routing includes Phase 7.1

---

## B38-0B-025-002 — SemanticVersioning routing: QA+IMPLEMENT

Scenario: SemanticVersioningBuildApproval
PredicateSet: ["QA","IMPLEMENT"]
Routing:
- IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
- QA → Phase 7.1 (ReleaseGateFlow71) — QA last (rule 12)

RoutingTable:
| Predicate | Phase | Flow | Order |
|---|---|---|---|
| IMPLEMENT | 4.1 | BackendServiceFlow41 | 1 |
| QA | 7.1 | ReleaseGateFlow71 | 2 |

DecisionCount: 2
TransitionType: feature_addition
CapabilityGapWarnings: []

Gate B: DecisionCount == 2 AND Phase 4.1 present AND Phase 7.1 present AND QA order == last

---

## B38-0B-025-003 — CalendarVersioning knowledge edge routing

Scenario: CalVerPlanningKnowledgeEdge
PredicateSet: []
Routing: none (knowledge_edge)
ResultState: knowledge_edge
DecisionCount: 1 (sentinel)

Gate B: PredicateSet == [] AND ResultState == "knowledge_edge" AND no routing decision generated

---

## B38-0B-025-004 — SemVerBump document routing

Scenario: SemVerBumpDocumentation
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
