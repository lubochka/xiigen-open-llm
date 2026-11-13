# Layer 0-B — Router Pairs (058)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-058-001 — TLSCertRotation: routing for ARCHITECTURE → 3.5

"Architect the TLS (Transport Layer Security) certificate lifecycle management topology."
  ARCHITECTURE → 3.5 | TransitionType: feature_addition | TrustState: deterministic_generic

---

## B14-router-058-002 — TLSCertRotation: routing for IMPLEMENT + SECURITY

"Implement the cert-manager CertificateRequest manifest and enforce 90-day TLS rotation."
  IMPLEMENT → 4.1 | SECURITY → 6.3 (TLS, certificate rotation = key rotation)
  Sequential: 6.3 → 4.1 | TransitionType: feature_addition | AllSubPhaseIds: [4.1, 6.3]

---

## B14-router-058-003 — TLSCertRotation: routing for AUTHORIZATION + IMPLEMENT + SECURITY

"Grant cert-manager ClusterRole, implement PKI Terraform module, and enforce TLS rotation."
  AUTHORIZATION → auth sub-phase | IMPLEMENT → 4.1 | SECURITY → 6.3
  Sequential: 6.3 → AUTHORIZATION → 4.1 | TransitionType: feature_addition

---

## B14-router-058-004 — TLSCertRotation: gap routing

"Renew the TLS certificate for the ingress controller."
  Zero predicates → GapHandler | "renew" is operational gap
  CapabilityGapWarnings: gap-devops-cloud-001

"Check certificate expiry date."
  Zero predicates → GapHandler | "check" is operational gap
