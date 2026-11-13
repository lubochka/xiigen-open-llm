# Layer 0-B — Router Pairs (006)
# Batch: B38-batch-001 | Scenario: B38 — Project delivery — release gates and go-live checklists
# Production date: 2026-05-05 | pair_kind: router

---

## B38-0B-006-001 — [QA, IMPLEMENT] → 7.1 + 4.1 (CI/CD pipeline scenario)

PredicateSet: ["QA", "IMPLEMENT"]
Scenario: CiCdPipelineReview
Input: "Review the CI/CD pipeline configuration and approve it for the production release."

Routing decisions:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

RoutingTable:
| Predicate | Target Phase | Flow | Order |
|---|---|---|---|
| IMPLEMENT | 4.1 | BackendServiceFlow41 | 1 |
| QA | 7.1 | ReleaseGateFlow71 | 2 (last) |

DecisionCount: 2
TransitionType: feature_addition
CapabilityGapWarnings: []
OutputFiles: [pipeline_review_report.md, ci_cd_config_audit.md, release_approval.md]
DomainId: software-development

---

## B38-0B-006-002 — CI/CD pipeline routing rationale

"CI/CD pipeline" fires IMPLEMENT because pipeline is a code artifact.
The "CI/CD" qualifier confirms code-infrastructure context.
IMPLEMENT routes to 4.1 because pipeline configuration is treated as backend implementation artifact.

This is consistent with rule 6 ("build" fires IMPLEMENT even in artifact context) applied to pipeline:
- "build artifacts" → IMPLEMENT → 4.1
- "pipeline configuration" → IMPLEMENT → 4.1
- Both are code-level artifacts under implementation review

---

## B38-0B-006-003 — [QA, IMPLEMENT]: both pipeline and build examples route identically

Pipeline scenario (file 006): review CI/CD pipeline → IMPLEMENT → 4.1
Build artifacts scenario (file 004): audit build artifacts → IMPLEMENT → 4.1

Both produce:
- PredicateSet: ["QA", "IMPLEMENT"]
- Routing: 4.1 + 7.1
- TransitionType: feature_addition
- DecisionCount: 2

The IMPLEMENT predicate does not carry sub-type information into routing. Whether IMPLEMENT fired from "build", "service", or "pipeline", the routing target is always Phase 4.1.

---

## B38-0B-006-004 — Flow execution order for [QA, IMPLEMENT]

Execution order:
1. BackendServiceFlow41 (IMPLEMENT — Phase 4.1) executes first
   - Produces: code-level review output, pipeline audit
2. ReleaseGateFlow71 (QA — Phase 7.1) executes second (QA always last)
   - Consumes: output from IMPLEMENT phase as input
   - Produces: release approval decision referencing IMPLEMENT output

The QA flow in B38 can use IMPLEMENT phase output as evidence for the gate decision. This is why QA is ordered last: it needs other phase outputs to complete its review.
