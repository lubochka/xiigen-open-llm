# Layer 0-B — Router Pairs (041)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-041-001 — DeprecationNotice routing: DOCUMENT alone

Input: "Document the deprecation notice for the v1 API (Application Programming Interface) endpoint."
- PredicateSet: ["DOCUMENT"]
- Routing: DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-041-002 — DeprecationNotice routing: QA alone

Input: "Review the deprecation notice and approve the API sunset timeline."
- PredicateSet: ["QA"]
- Routing: QA → Phase 7.1 (ReleaseGateFlow71)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-041-003 — DeprecationNotice routing: QA+DOCUMENT

Input: "Document the deprecation acceptance criteria and sign off the notice for release."
- PredicateSet: ["QA","DOCUMENT"]
- Routing:
  1. DOCUMENT → Phase 2.1 (DocumentationFlow)
  2. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-041-004 — DeprecationNotice routing: QA+IMPLEMENT+DOCUMENT

Input: "Build the deprecation notification service, document the deprecation criteria, and verify compliance."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: feature_addition

---

## B38-0B-041-005 — DeprecationNotice routing: QA+IMPLEMENT+SECURITY

Input: "Audit the security of the deprecation notification service and approve the deprecation plan."
- PredicateSet: ["QA","IMPLEMENT","SECURITY"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. SECURITY → Phase 3.4 (SecurityAuditFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: feature_addition
