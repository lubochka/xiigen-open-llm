# Layer 0-B — Router Pairs (047)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-047-001 — CVENotification routing: DOCUMENT+SECURITY

Input: "Document the CVE (Common Vulnerabilities and Exposures) notification for the patched component."
- PredicateSet: ["DOCUMENT","SECURITY"]
- Routing:
  1. DOCUMENT → Phase 2.1 (DocumentationFlow)
  2. SECURITY → Phase 3.4 (SecurityAuditFlow)
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-047-002 — CVENotification routing: SECURITY alone

Input: "Register the CVE with the national vulnerability database."
- PredicateSet: ["SECURITY"]
- Routing: SECURITY → Phase 3.4 (SecurityAuditFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-047-003 — CVENotification routing: QA+SECURITY

Input: "Review the CVE notification and approve the security patch release."
- PredicateSet: ["QA","SECURITY"]
- Routing:
  1. SECURITY → Phase 3.4 (SecurityAuditFlow)
  2. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 2
- TransitionType: no_contribution

---

## B38-0B-047-004 — CVENotification routing: QA+DOCUMENT+SECURITY

Input: "Audit the CVE and security controls, document the vulnerability criteria, and sign off the patch."
- PredicateSet: ["QA","DOCUMENT","SECURITY"]
- Routing:
  1. DOCUMENT → Phase 2.1 (DocumentationFlow)
  2. SECURITY → Phase 3.4 (SecurityAuditFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: no_contribution

---

## B38-0B-047-005 — CVENotification routing: QA+IMPLEMENT+DOCUMENT+SECURITY

Input: "Build the patch service, audit the CVE, document the security criteria, and approve the fix."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. SECURITY → Phase 3.4 (SecurityAuditFlow)
  4. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 4
- TransitionType: feature_addition
