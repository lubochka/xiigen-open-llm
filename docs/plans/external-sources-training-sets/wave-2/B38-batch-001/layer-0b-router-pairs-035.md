# Layer 0-B — Router Pairs (035)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-035-001 — HotfixTag architecture routing

Scenario: HotfixTaggingArchitectureApproval
PredicateSet: ["QA","ARCHITECTURE"]
Routing:
- ARCHITECTURE → Phase 5.x (ArchitectureFlow)
- QA → Phase 7.1 (ReleaseGateFlow71) — QA last

RoutingTable:
| Predicate | Phase | Flow | Order |
|---|---|---|---|
| ARCHITECTURE | 5.x | ArchitectureFlow | 1 |
| QA | 7.1 | ReleaseGateFlow71 | 2 |

DecisionCount: 2
TransitionType: no_contribution

Gate B: DecisionCount == 2 AND Phase 5.x AND Phase 7.1 AND TransitionType == "no_contribution"

---

## B38-0B-035-002 — PatchRelease build+security routing

Scenario: PatchReleaseBuildSecurityApproval
PredicateSet: ["QA","IMPLEMENT","SECURITY"]
Routing:
- IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
- SECURITY → Phase 3.4 (SecurityAuditFlow)
- QA → Phase 7.1 (ReleaseGateFlow71) — QA last

RoutingTable:
| Predicate | Phase | Flow | Order |
|---|---|---|---|
| IMPLEMENT | 4.1 | BackendServiceFlow41 | 1 |
| SECURITY | 3.4 | SecurityAuditFlow | 2 |
| QA | 7.1 | ReleaseGateFlow71 | 3 |

DecisionCount: 3
TransitionType: feature_addition

Gate B: DecisionCount == 3 AND TransitionType == "feature_addition" AND QA last

---

## B38-0B-035-003 — SemanticVersioning knowledge edge routing

Scenario: SemVerFinalizationKnowledgeEdge
PredicateSet: []
CapabilityGapWarnings: []
ResultState: knowledge_edge

Gate B: PredicateSet == [] AND ResultState == "knowledge_edge"

---

## B38-0B-035-004 — RCCandidate full four-predicate routing

Scenario: ReleaseCandidateFourPredicateValidation
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

Gate B: DecisionCount == 4 AND all four phases present AND TransitionType == "feature_addition"
