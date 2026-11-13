# Layer 0-B — Router Pairs (046)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-046-001 — SecurityAdvisory routing: DOCUMENT+SECURITY

Input: "Document the security advisory for the v7 release patch."
- PredicateSet: ["DOCUMENT","SECURITY"]
- Routing:
  1. DOCUMENT → Phase 2.1 (DocumentationFlow)
  2. SECURITY → Phase 3.4 (SecurityAuditFlow)
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-046-002 — SecurityAdvisory routing: SECURITY alone

Input: "Publish the security advisory to the customer portal."
- PredicateSet: ["SECURITY"]
- CapabilityGapWarnings: ["publish"]
- Routing: SECURITY → Phase 3.4 (SecurityAuditFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-046-003 — SecurityAdvisory routing: QA+SECURITY

Input: "Review the security advisory and approve the patch release."
- PredicateSet: ["QA","SECURITY"]
- Routing:
  1. SECURITY → Phase 3.4 (SecurityAuditFlow)
  2. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-046-004 — SecurityAdvisory routing: QA+DOCUMENT+SECURITY

Input: "Audit the security advisory content, document the patch release criteria, and sign off."
- PredicateSet: ["QA","DOCUMENT","SECURITY"]
- Routing:
  1. DOCUMENT → Phase 2.1 (DocumentationFlow)
  2. SECURITY → Phase 3.4 (SecurityAuditFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: no_contribution

---

## B38-0B-046-005 — SecurityAdvisory routing: QA+IMPLEMENT+DOCUMENT+SECURITY

Input: "Review the security advisory for the payment service, document the patch criteria, audit security controls, and approve."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. SECURITY → Phase 3.4 (SecurityAuditFlow)
  4. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 4
- TransitionType: feature_addition
