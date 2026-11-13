# Layer 0-B — Router Pairs (019)
# Batch: B38-batch-001 | Scenario: B38 — Release gates, go-live checklists, and project delivery
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-019-001 — [QA, DOCUMENT]: release criteria documentation

PredicateSet: ["QA", "DOCUMENT"]
Scenario: ReleaseCriteriaDoc
Input: "Document the release criteria for the v4.0 release and have the product manager approve."

Routing decisions:
1. DOCUMENT → Phase 2.1 (DocumentationFlow)
2. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 2
TransitionType: no_contribution
OutputFiles: [release_criteria_v4_0.md, product_manager_approval.md]

---

## B38-0B-019-002 — [QA, IMPLEMENT, DOCUMENT]: criteria with service

PredicateSet: ["QA", "IMPLEMENT", "DOCUMENT"]
Scenario: InventoryServiceReleaseCriteria
Input: "Document the release criteria for the inventory service and review the acceptance criteria."

Routing decisions:
1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
2. DOCUMENT → Phase 2.1 (DocumentationFlow)
3. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 3
TransitionType: feature_addition

---

## B38-0B-019-003 — [DOCUMENT] alone: criteria without QA

PredicateSet: ["DOCUMENT"]
Scenario: ExitCriteriaDocument
Input: "Document all exit criteria and readiness criteria for the production release."

Routing decision:
1. DOCUMENT → Phase 2.1 (DocumentationFlow)

DecisionCount: 1
TransitionType: no_contribution
OutputFiles: [exit_criteria.md, readiness_criteria.md]

Note: This is a DOCUMENT-only scenario. No QA terms present. "Document" fires DOCUMENT but not QA. Routing goes to 2.1 only.

---

## B38-0B-019-004 — [QA, DOCUMENT, SECURITY]: criteria with security audit

PredicateSet: ["QA", "DOCUMENT", "SECURITY"]
Scenario: SecurityCriteriaAudit
Input: "Audit the security criteria before documenting the final release criteria for sign-off."

Routing decisions:
1. DOCUMENT → Phase 2.1 (DocumentationFlow)
2. SECURITY → Phase 3.4 (SecurityAuditFlow)
3. QA → Phase 7.1 (ReleaseGateFlow71) — QA last

DecisionCount: 3
TransitionType: no_contribution (no IMPLEMENT)

Note: Three routing decisions from three predicates. QA still last. SECURITY from "audit+security" co-occurrence (rule 19). No IMPLEMENT → no_contribution despite three active predicates.
