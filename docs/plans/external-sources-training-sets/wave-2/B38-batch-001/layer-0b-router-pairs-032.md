# Layer 0-B — Router Pairs (032)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-032-001 — SemVerBump build validation routing

Scenario: SemVerBumpAutomationValidation
PredicateSet: ["QA","IMPLEMENT"]
Routing:
- IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
- QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: feature_addition

Gate B: DecisionCount == 2 AND TransitionType == "feature_addition"

---

## B38-0B-032-002 — PreReleaseTag security routing

Scenario: PreReleaseTagSigningSecurity
PredicateSet: ["QA","SECURITY"]
Routing:
- SECURITY → Phase 3.4 (SecurityAuditFlow)
- QA → Phase 7.1 (ReleaseGateFlow71) — QA last

RoutingTable:
| Predicate | Phase | Flow | Order |
|---|---|---|---|
| SECURITY | 3.4 | SecurityAuditFlow | 1 |
| QA | 7.1 | ReleaseGateFlow71 | 2 |

DecisionCount: 2
TransitionType: no_contribution

Gate B: DecisionCount == 2 AND Phase 3.4 present AND Phase 7.1 present AND TransitionType == "no_contribution"

---

## B38-0B-032-003 — CalendarVersioning service routing

Scenario: CalVerGeneratorServiceBuild
PredicateSet: ["QA","IMPLEMENT"]
Routing:
- IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
- QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: feature_addition

Gate B: DecisionCount == 2 AND TransitionType == "feature_addition"

---

## B38-0B-032-004 — QuarterlyRelease sign-off routing

Scenario: QuarterlyReleaseSignOffCollection
PredicateSet: ["QA"]
Routing:
- QA → Phase 7.1 (ReleaseGateFlow71)

DecisionCount: 1
TransitionType: no_contribution

Gate B: DecisionCount == 1 AND Phase 7.1 present AND TransitionType == "no_contribution"
