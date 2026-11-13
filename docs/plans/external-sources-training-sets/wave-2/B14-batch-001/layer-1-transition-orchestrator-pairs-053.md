# Layer 1 — Transition Orchestrator Pairs (053)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: transition_orchestrator

---

## B14-trans-053-001 — OPAGatekeeper + FalcoRuntime + AdmissionControllers: ARCHITECTURE → 3.5 vs 3.1

All three security-policy topics:
  "Architect the OPA policy framework" → 3.5 (infra vocabulary: "framework", "cluster")
  "Architect the Falco detection topology" → 3.5 (infra vocabulary: "topology")
  "Architect the admission controller chain" → 3.5 (infra vocabulary: "chain" in cluster context)
  All route to 3.5 (not 3.1) because the vocabulary is infrastructure-level, not app-domain-level.

---

## B14-trans-053-002 — WAFRules + DDoSProtection: both combine SECURITY with API_CONTRACT potential

WAFRules: "define WAF-protected API contract" → API_CONTRACT + SECURITY
DDoSProtection: does not typically combine with API_CONTRACT (DDoS is infrastructure protection, not API contract definition)
Rule: API_CONTRACT fires for WAFRules (endpoint exposure vocabulary) but not typically for DDoSProtection (infrastructure mitigation vocabulary).

---

## B14-trans-053-003 — ZeroTrustConfig: fires most predicates out of all security topics

ZeroTrustConfig maximum predicate firing:
  "Architect zero-trust topology + enforce deny-all mTLS + grant SPIFFE ClusterRoles + implement manifests + document ADR"
  → ARCHITECTURE + SECURITY + AUTHORIZATION + IMPLEMENT + DOCUMENT = 5 predicates
  TransitionType: feature_addition | Sequential: 3.5 → 6.3 → AUTHORIZATION → 4.1 → 6.2
  ZeroTrustConfig is the only security topic that regularly fires AUTHORIZATION (via identity-based access).

---

## B14-trans-053-004 — RBACRoles + ServiceAccountConfig: both primarily fire AUTHORIZATION

RBACRoles: AUTHORIZATION is the primary predicate; others (IMPLEMENT, ARCHITECTURE) are secondary
ServiceAccountConfig: AUTHORIZATION is the primary predicate (Workload Identity, IRSA bindings)
Contrast: NetworkPolicy, PodSecurityPolicy, FalcoRuntime, OPAGatekeeper primarily fire SECURITY
Pattern: Identity/role topics → AUTHORIZATION-primary. Enforcement/hardening topics → SECURITY-primary.
