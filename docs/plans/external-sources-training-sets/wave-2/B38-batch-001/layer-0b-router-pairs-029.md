# Layer 0-B — Router Pairs (029)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-029-001 — HotfixTag routing: QA alone

Scenario: HotfixTagEmergencyApproval
PredicateSet: ["QA"]
Routing:
- QA → Phase 7.1 (ReleaseGateFlow71)

DecisionCount: 1
TransitionType: no_contribution

Gate B: DecisionCount == 1 AND Phase 7.1 present

---

## B38-0B-029-002 — HotfixTag routing: QA+IMPLEMENT

Scenario: HotfixTagBuildApproval
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

## B38-0B-029-003 — PatchRelease routing: QA+DOCUMENT

Scenario: PatchReleaseNotesSignOff
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

Gate B: DecisionCount == 2 AND Phase 2.1 AND Phase 7.1 AND TransitionType == "no_contribution"

---

## B38-0B-029-004 — PatchRelease routing: QA+SECURITY

Scenario: PatchReleaseSecurityAudit
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

Gate B: DecisionCount == 2 AND Phase 3.4 AND Phase 7.1 AND TransitionType == "no_contribution"
