# Layer 0-B — Router Pairs (058)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-058-001 — RegulatorySubmission routing: DOCUMENT alone

Input: "Document the regulatory submission for the FDA (Food and Drug Administration) software release."
- PredicateSet: ["DOCUMENT"]
- Routing: DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-058-002 — RegulatorySubmission routing: QA alone

Input: "Review the regulatory submission and approve the release for submission."
- PredicateSet: ["QA"]
- Routing: QA → Phase 7.1 (ReleaseGateFlow71)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-058-003 — RegulatorySubmission routing: QA+DOCUMENT+SECURITY

Input: "Audit the security compliance of the regulatory submission, document the submission criteria, and sign off."
- PredicateSet: ["QA","DOCUMENT","SECURITY"]
- Routing:
  1. DOCUMENT → Phase 2.1 (DocumentationFlow)
  2. SECURITY → Phase 3.4 (SecurityAuditFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: no_contribution

---

## B38-0B-058-004 — RegulatorySubmission routing: QA+IMPLEMENT+DOCUMENT

Input: "Build the regulatory submission service, document the submission criteria, and validate regulatory compliance."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: feature_addition

---

## B38-0B-058-005 — RegulatorySubmission routing: QA+IMPLEMENT+DOCUMENT+SECURITY

Input: "Review the regulatory service, audit the security controls, document the regulatory criteria, and approve the release."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. SECURITY → Phase 3.4 (SecurityAuditFlow)
  4. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 4
- TransitionType: feature_addition
