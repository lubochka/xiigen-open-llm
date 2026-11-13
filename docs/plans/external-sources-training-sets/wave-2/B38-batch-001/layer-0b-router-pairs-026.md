# Layer 0-B — Router Pairs (026)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-026-001 — PreReleaseTag routing: QA alone

Scenario: PreReleaseTagVerification
PredicateSet: ["QA"]
Routing:
- QA → Phase 7.1 (ReleaseGateFlow71)

RoutingTable:
| Predicate | Phase | Flow | Order |
|---|---|---|---|
| QA | 7.1 | ReleaseGateFlow71 | 1 |

DecisionCount: 1
TransitionType: no_contribution

Gate B: DecisionCount == 1 AND Phase 7.1 present

---

## B38-0B-026-002 — PreReleaseTag routing: QA+IMPLEMENT

Scenario: PreReleaseTagPipelineBuild
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

## B38-0B-026-003 — RCCandidate routing: QA alone

Scenario: ReleaseCandidateSignOff
PredicateSet: ["QA"]
Routing:
- QA → Phase 7.1 (ReleaseGateFlow71)

DecisionCount: 1
TransitionType: no_contribution

Gate B: DecisionCount == 1 AND Phase 7.1 present AND TransitionType == "no_contribution"

---

## B38-0B-026-004 — RCCandidate routing: QA+DOCUMENT

Scenario: ReleaseCandidateDocumentedApproval
PredicateSet: ["QA","DOCUMENT"]
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

Gate B: DecisionCount == 2 AND Phase 2.1 present AND Phase 7.1 present AND TransitionType == "no_contribution"
