# Layer 0-B — Router Pairs (031)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-031-001 — HotfixTag full predicate set routing

Scenario: HotfixTagFullPredicateReview
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

## B38-0B-031-002 — NightlyBuild document routing

Scenario: NightlyBuildCriteriaDocumentation
PredicateSet: ["DOCUMENT"]
Routing:
- DOCUMENT → Phase 2.1 (DocumentationFlow)

DecisionCount: 1
TransitionType: no_contribution

Gate B: DecisionCount == 1 AND Phase 2.1 present

---

## B38-0B-031-003 — WeeklyRelease routing: QA+IMPLEMENT with gap

Scenario: WeeklyReleaseServiceApprovalWithRolloutGap
PredicateSet: ["QA","IMPLEMENT"]
CapabilityGapWarnings: ["rollout"]
Routing:
- IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
- QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: feature_addition
Note: "rollout" gap does NOT generate routing decision

Gate B: DecisionCount == 2 AND "rollout" in CapabilityGapWarnings AND TransitionType == "feature_addition"

---

## B38-0B-031-004 — RCCandidate full three-predicate routing

Scenario: ReleaseCandidateFullValidation
PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
Routing:
- IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
- DOCUMENT → Phase 2.1 (DocumentationFlow)
- QA → Phase 7.1 (ReleaseGateFlow71) — QA last

RoutingTable:
| Predicate | Phase | Flow | Order |
|---|---|---|---|
| IMPLEMENT | 4.1 | BackendServiceFlow41 | 1 |
| DOCUMENT | 2.1 | DocumentationFlow | 2 |
| QA | 7.1 | ReleaseGateFlow71 | 3 |

DecisionCount: 3
TransitionType: feature_addition

Gate B: DecisionCount == 3 AND TransitionType == "feature_addition"
