# Layer 0-B — Router Pairs (042)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-042-001 — APIBreakingChange routing: DOCUMENT alone

Input: "Document the API (Application Programming Interface) breaking change in the release notes."
- PredicateSet: ["DOCUMENT"]
- Routing: DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-042-002 — APIBreakingChange routing: QA alone

Input: "Review the API breaking change impact and approve the v3 release."
- PredicateSet: ["QA"]
- Routing: QA → Phase 7.1 (ReleaseGateFlow71)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-042-003 — APIBreakingChange routing: QA+DOCUMENT

Input: "Document the breaking change acceptance criteria and sign off on the API release."
- PredicateSet: ["QA","DOCUMENT"]
- Routing:
  1. DOCUMENT → Phase 2.1 (DocumentationFlow)
  2. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-042-004 — APIBreakingChange routing: QA+IMPLEMENT+DOCUMENT

Input: "Build the updated API gateway service, document the breaking change criteria, and validate compatibility."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: feature_addition

---

## B38-0B-042-005 — APIBreakingChange routing: QA+IMPLEMENT+SECURITY

Input: "Audit the security of the updated API service and approve the breaking change for release."
- PredicateSet: ["QA","IMPLEMENT","SECURITY"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. SECURITY → Phase 3.4 (SecurityAuditFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: feature_addition
