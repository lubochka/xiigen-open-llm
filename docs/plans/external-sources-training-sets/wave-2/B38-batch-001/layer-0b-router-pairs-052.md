# Layer 0-B — Router Pairs (052)
# Batch: B38-batch-001 | Scenario: B38 — Release management vocabulary
# Production date: 2026-05-06 | pair_kind: router

---

## B38-0B-052-001 — AttestationPolicy routing: DOCUMENT alone

Input: "Document the attestation policy for the release supply chain."
- PredicateSet: ["DOCUMENT"]
- Routing: DOCUMENT → Phase 2.1 (DocumentationFlow)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-052-002 — AttestationPolicy routing: QA alone

Input: "Review the attestation policy and sign off the release."
- PredicateSet: ["QA"]
- Routing: QA → Phase 7.1 (ReleaseGateFlow71)
- DecisionCount: 1
- TransitionType: no_contribution

---

## B38-0B-052-003 — AttestationPolicy routing: QA+DOCUMENT+SECURITY

Input: "Document the attestation acceptance criteria, audit the policy security, and approve."
- PredicateSet: ["QA","DOCUMENT","SECURITY"]
- Routing:
  1. DOCUMENT → Phase 2.1 (DocumentationFlow)
  2. SECURITY → Phase 3.4 (SecurityAuditFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: no_contribution

---

## B38-0B-052-004 — AttestationPolicy routing: QA+IMPLEMENT+DOCUMENT

Input: "Build the attestation service, document the attestation criteria, and validate the policy."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 3
- TransitionType: feature_addition

---

## B38-0B-052-005 — AttestationPolicy routing: QA+IMPLEMENT+DOCUMENT+SECURITY

Input: "Review the attestation service, audit the attestation security controls, document the policy criteria, and approve the release."
- PredicateSet: ["QA","IMPLEMENT","DOCUMENT","SECURITY"]
- Routing:
  1. IMPLEMENT → Phase 4.1 (BackendServiceFlow41)
  2. DOCUMENT → Phase 2.1 (DocumentationFlow)
  3. SECURITY → Phase 3.4 (SecurityAuditFlow)
  4. QA → Phase 7.1 (ReleaseGateFlow71) — last
- DecisionCount: 4
- TransitionType: feature_addition
