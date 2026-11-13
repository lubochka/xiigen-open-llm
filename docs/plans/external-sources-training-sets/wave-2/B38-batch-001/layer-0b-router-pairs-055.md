# Layer 0-B — Router Pairs (055)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-055-001 — ComplianceRelease routing: DOCUMENT alone

Input: "Document the compliance release requirements for the financial platform."
- PredicateSet: ["DOCUMENT"]
- Routing: DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-055-002 — ComplianceRelease routing: QA alone

Input: "Verify compliance with the release standards and sign off."
- PredicateSet: ["QA"]
- Routing: QA → Phase 7.1 (ReleaseGateFlow71)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-055-003 — ComplianceRelease routing: QA+DOCUMENT+SECURITY

Input: "Audit the security compliance controls, document the compliance criteria, and approve the release."
- PredicateSet: ["QA","DOCUMENT","SECURITY"]
- Routing:
  1. DOCUMENT → Phase 2.1 (DocumentationFlow)
  2. SECURITY → Phase 3.4 (SecurityAuditFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: no_contribution

---

## B38-0B-055-004 — ComplianceRelease routing: QA+IMPLEMENT+DOCUMENT+SECURITY

Input: "Review the compliance service, audit the security controls, document the compliance criteria, and approve the release."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. SECURITY → Phase 3.4 (SecurityAuditFlow)
  4. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 4
- TransitionType: feature_addition
