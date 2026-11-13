# Layer 0-B — Router Pairs (015)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-015-001 — [QA] alone: hotfix approval

PredicateSet: ["QA"]
Scenario: HotfixReleaseApproval
Input: "Approve the hotfix for the login issue and verify it passes the emergency release gate."

Routing decision:
1. QA → Phase 7.1 (ReleaseGateFlow71)

DecisionCount: 1
TransitionType: no_contribution
OutputFiles: [hotfix_gate_report.md, emergency_release_approval.md]

---

## B38-0B-015-002 — [QA, IMPLEMENT]: hotfix with service

PredicateSet: ["QA", "IMPLEMENT"]
Scenario: HotfixAuthServiceRelease
Input: "Review the hotfix to the authentication service and approve the emergency release."

Routing decisions:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: feature_addition

---

## B38-0B-015-003 — [QA, DOCUMENT]: hotfix criteria

PredicateSet: ["QA", "DOCUMENT"]
Scenario: HotfixCriteriaDocumentation
Input: "Validate the hotfix exit criteria and document the release decision rationale."

Routing decisions:
1. DOCUMENT → Phase 2.1 (DocumentationFlow)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: no_contribution

---

## B38-0B-015-004 — Knowledge_edge: hotfix rollback gap

PredicateSet: []
Scenario: HotfixRollbackGap
Input: "Execute the hotfix rollout and monitor the go-live rollout after remediation."

Routing: knowledge_edge
CapabilityGapWarnings: ["execute", "rollout", "monitor", "go-live"]
DecisionCount: 1 (knowledge_edge sentinel)
TransitionType: N/A

---

## B38-0B-015-005 — [QA, IMPLEMENT]: hotfix build provenance

PredicateSet: ["QA", "IMPLEMENT"]
Scenario: HotfixBuildProvenance
Input: "Inspect the hotfix build provenance and approve the patch release."

Routing decisions:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41) — from "build"
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: feature_addition

Note: "build" fires IMPLEMENT (rule 6) even in the context "build provenance". The noun "provenance" is a descriptor of the build artifact; it does not add a separate predicate.
