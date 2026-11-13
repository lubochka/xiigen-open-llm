# Layer 0-B — Router Pairs (056)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-056-001 — WAFRules: routing for ARCHITECTURE → 3.5

"Architect the WAF (Web Application Firewall) rule topology for public-facing endpoints."
  ARCHITECTURE → 3.5 | TransitionType: feature_addition | TrustState: deterministic_generic

---

## B14-router-056-002 — WAFRules: routing for IMPLEMENT + SECURITY + API_CONTRACT

"Implement the WAF rule group Terraform module, enforce OWASP rules, and define the WAF-protected API contract."
  IMPLEMENT → 4.1 | SECURITY → 6.3 | API_CONTRACT → 2.1
  Sequential: 2.1 → 6.3 → 4.1 | TransitionType: feature_addition
  AllSubPhaseIds: [4.1, 6.3, 2.1]

---

## B14-router-056-003 — WAFRules: routing for AUTHORIZATION + SECURITY

"Restrict WAF rule management to security-admin IAM role and enforce TLS inspection."
  AUTHORIZATION → auth sub-phase | SECURITY → 6.3
  Sequential: 6.3 → AUTHORIZATION | TransitionType: feature_addition

---

## B14-router-056-004 — WAFRules: gap routing

"Enable the WAF on the load balancer."
  Zero predicates → GapHandler | "enable" is operational gap; "load balancer" is gap
  CapabilityGapWarnings: gap-devops-cloud-001
