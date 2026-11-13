# Layer 0-B — Router Pairs (021)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-021-001 — [QA] alone: incident response review

PredicateSet: ["QA"]
Scenario: IncidentResponseRelease
Input: "Review the incident response for the release failure and approve the remediation plan."

Routing decision:
1. QA → Phase 7.1 (ReleaseGateFlow71)

DecisionCount: 1
TransitionType: no_contribution
OutputFiles: [incident_review_report.md, remediation_approval.md]

---

## B38-0B-021-002 — [QA, IMPLEMENT]: incident with service fix

PredicateSet: ["QA", "IMPLEMENT"]
Scenario: IncidentAuthServiceFix
Input: "Verify the incident-related fix to the auth service and sign off on the emergency release."

Routing decisions:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: feature_addition

---

## B38-0B-021-003 — [QA, DOCUMENT]: incident criteria documentation

PredicateSet: ["QA", "DOCUMENT"]
Scenario: IncidentResolutionCriteria
Input: "Document the incident resolution criteria and validate that all acceptance criteria are closed."

Routing decisions:
1. DOCUMENT → Phase 2.1 (DocumentationFlow)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: no_contribution

---

## B38-0B-021-004 — Knowledge_edge: incident rollback gap

PredicateSet: []
Scenario: IncidentRollbackGap
Input: "Execute the incident rollback and monitor the go-live rollout after remediation."

Routing: knowledge_edge
CapabilityGapWarnings: ["execute", "rollback", "monitor", "go-live", "rollout"]
DecisionCount: 1 (knowledge_edge sentinel)
TransitionType: N/A

---

## B38-0B-021-005 — [QA, IMPLEMENT, DOCUMENT]: full incident release

PredicateSet: ["QA", "IMPLEMENT", "DOCUMENT"]
Scenario: IncidentFullServiceRelease
Input: "Review the incident service fix, document the release criteria for the re-release, and approve the hotfix deployment."

Routing decisions:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
2. DOCUMENT → Phase 2.1 (DocumentationFlow)
3. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 3
TransitionType: feature_addition
CapabilityGapWarnings: ["deployment"]
