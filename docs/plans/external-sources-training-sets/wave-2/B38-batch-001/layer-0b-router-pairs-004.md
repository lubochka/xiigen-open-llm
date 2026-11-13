# Layer 0-B — Router Pairs (004)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: router

---

## B38-0B-004-001 — [QA, IMPLEMENT] → 7.1 + 4.1 (build artifacts scenario)

PredicateSet: ["QA", "IMPLEMENT"]
Scenario: BuildProvenanceAudit
Input: "Audit the build artifacts and approve the release for the order service component."

Routing decisions:
1. IMPLEMENT → Phase 4.1 (Backend service implementation) → BackendServiceFlow41
2. QA → Phase 7.1 (Release and Deployment Automation) → ReleaseGateFlow71 (QA last)

RoutingTable:
| Predicate | Target Phase | Flow | Order |
|---|---|---|---|
| IMPLEMENT | 4.1 | BackendServiceFlow41 | 1 |
| QA | 7.1 | ReleaseGateFlow71 | 2 (last) |

DecisionCount: 2
TransitionType: feature_addition
OutputFiles: [build_provenance_report.md, order_service_review.md, release_gate_approval.md]
DomainId: software-development

---

## B38-0B-004-002 — [QA, IMPLEMENT] routing: feature_addition confirmed

When IMPLEMENT is in the predicate set, TransitionType = feature_addition.
This applies regardless of whether IMPLEMENT fired from "service", "build", "pipeline", or any other IMPLEMENT lexicon term.

For the build artifacts scenario:
- "build" fires IMPLEMENT (rule 6)
- IMPLEMENT routes to 4.1 — BackendServiceFlow41
- This represents code-level work being reviewed
- The feature_addition label acknowledges that code (the build) is part of the contribution

---

## B38-0B-004-003 — Phase 4.1 routing: what IMPLEMENT delivers

Phase 4.1 (Backend service implementation) output for build artifacts audit:
- N03: IMPLEMENT routing decision recorded
- N05: BackendServiceFlow41 executes (reviews build artifact content)
- N07: IMPLEMENT contribution artifacts generated

For build-provenance audit specifically, Phase 4.1 produces:
- build_provenance_audit.md (artifact integrity review)
- release_candidate_validation.md (candidate artifacts listed)

Phase 7.1 then produces (after IMPLEMENT, because QA is last):
- release_gate_report.md (approval decision)
- audit_trail.md (sign-off record)

---

## B38-0B-004-004 — Comparison: [QA, IMPLEMENT] vs [QA] routing

[QA] only:
- 1 routing decision
- 1 flow (ReleaseGateFlow71)
- 2 output files
- TransitionType: no_contribution

[QA, IMPLEMENT]:
- 2 routing decisions
- 2 flows (BackendServiceFlow41 + ReleaseGateFlow71)
- 3 output files
- TransitionType: feature_addition
- QA executes after IMPLEMENT (ordering rule)

The presence of "build", "service", or "pipeline" in the input is the decisive factor that shifts from no_contribution to feature_addition.
