# Layer 0-B — Router Pairs (003)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: router

---

## B38-0B-003-001 — [QA, DOCUMENT] → 7.1 + 2.1

PredicateSet: ["QA", "DOCUMENT"]
Scenario: DeploymentReadinessCheck / ReleaseGateReview
Input: "Review the deployment readiness criteria and validate the production environment is ready."

Routing decisions:
1. DOCUMENT → Phase 2.1 (Documentation — Technical specs and requirements)
2. QA → Phase 7.1 (Release and Deployment Automation) — QA last

RoutingTable:
| Predicate | Target Phase | Flow | Order |
|---|---|---|---|
| DOCUMENT | 2.1 | DocumentationFlow | 1 |
| QA | 7.1 | ReleaseGateFlow71 | 2 (last) |

DecisionCount: 2
TransitionType: no_contribution
OutputFiles: [readiness_criteria_doc.md, release_gate_report.md]
DomainId: software-development

---

## B38-0B-003-002 — Why DOCUMENT routes to 2.1, not 7.1

DOCUMENT predicate is owned by Phase 2 (Documentation sub-phases: 2.1 Technical documentation, 2.4 User documentation).
In B38 specifically, DOCUMENT fires from "criteria" and routes to 2.1 (technical specs and requirements context).
DOCUMENT does NOT route to 7.1 even in a release context — the release gate is handled by QA alone.

This means for [QA, DOCUMENT]:
- Phase 7.1 receives the QA predicate
- Phase 2.1 receives the DOCUMENT predicate
- Two independent flows run: DocumentationFlow (criteria content) + ReleaseGateFlow71 (gate check)

---

## B38-0B-003-003 — [QA, DOCUMENT] routing with three gap warnings

PredicateSet: ["QA", "DOCUMENT"]
Input: "Complete the readiness criteria checklist review for the deployment rollout."

CapabilityGapWarnings: ["deployment", "checklist", "rollout"]

Routing:
- DOCUMENT → 2.1 (criteria)
- QA → 7.1 (review)

Three gap warnings do not affect routing. Routing is driven by predicates only. Gap warnings are metadata attached to the routing result.

DecisionCount: 2 (not 2+3=5 — gap warnings are not routing decisions)

---

## B38-0B-003-004 — Rule 14 reminder: DOCUMENT routes to Phase 2, not Phase 7

Rule 14 states: DOCUMENT routes to Phase 2 documentation sub-phase (2.1 or 2.4).

In B38 test cases involving "release criteria", the consistent mapping is:
- criteria → DOCUMENT → Phase 2.1

This is important because a v6-03 corruption might try to route DOCUMENT to 7.1 on the grounds that "we're in a release context". The correct behavior is: DOCUMENT always routes to Phase 2 regardless of the surrounding scenario.

Phase 7.1 is exclusively for QA predicate routing in B38.
