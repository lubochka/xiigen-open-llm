# Layer 0-D — Flow Factory Pairs (027)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: flow_factory

---

## B38-0D-027-001 — MonthlyRelease flow factory: QA+DOCUMENT

Scenario: MonthlyReleaseCriteriaGate
Input: "Document the monthly release criteria and sign off the release."
PredicateSet: ["QA","DOCUMENT"]

Flows created:
1. DocumentationFlow (Phase 2.1, DOCUMENT, order 1)
2. ReleaseGateFlow71 (Phase 7.1, QA, order 2 — last)

TransitionType: no_contribution
FlowCount: 2

---

## B38-0D-027-002 — QuarterlyRelease flow factory: QA+IMPLEMENT+DOCUMENT+SECURITY

Scenario: QuarterlyReleaseFourPredicateGate
Input: "Review the quarterly release service, audit the security controls, document the quarterly release criteria, and approve."
PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]

Flows created:
1. BackendServiceFlow41 (Phase 4.1, IMPLEMENT, order 1)
2. DocumentationFlow (Phase 2.1, DOCUMENT, order 2)
3. SecurityAuditFlow (Phase 3.4, SECURITY, order 3)
4. ReleaseGateFlow71 (Phase 7.1, QA, order 4 — last)

TransitionType: feature_addition
FlowCount: 4

---

## B38-0D-027-003 — HotfixTag flow factory: QA alone

Scenario: HotfixTagApproval
Input: "Approve the hotfix tag for the emergency release."
PredicateSet: ["QA"]

Flows created:
1. ReleaseGateFlow71 (Phase 7.1, QA, order 1)

TransitionType: no_contribution
FlowCount: 1

---

## B38-0D-027-004 — PatchRelease flow factory: QA+IMPLEMENT

Scenario: PatchReleaseServiceApproval
Input: "Build the patch release service and sign off the release."
PredicateSet: ["QA","IMPLEMENT"]

Flows created:
1. BackendServiceFlow41 (Phase 4.1, IMPLEMENT, order 1)
2. ReleaseGateFlow71 (Phase 7.1, QA, order 2 — last)

TransitionType: feature_addition
FlowCount: 2
