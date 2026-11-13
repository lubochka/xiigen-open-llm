# Layer 0-B — Router Pairs (059)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-059-001 — ZeroTrustConfig: routing for ARCHITECTURE + SECURITY

"Architect the zero-trust network architecture and enforce deny-all mTLS."
  ARCHITECTURE → 3.5 | SECURITY → 6.3 (zero-trust, mTLS)
  Sequential: 3.5 → 6.3 | TransitionType: feature_addition | AllSubPhaseIds: [3.5, 6.3]

---

## B14-router-059-002 — ZeroTrustConfig: routing for all 5 predicates

"Architect the zero-trust topology, enforce deny-all mTLS, grant SPIFFE ClusterRoles, implement AuthorizationPolicy manifests, and document the ADR."
  ARCHITECTURE → 3.5 | SECURITY → 6.3 | AUTHORIZATION → auth sub-phase | IMPLEMENT → 4.1 | DOCUMENT → 6.2
  Sequential: 3.5 → 6.3 → AUTHORIZATION → 4.1 → 6.2 | TransitionType: feature_addition (IMPLEMENT present)
  AllSubPhaseIds: [3.5, 6.3, auth, 4.1, 6.2]

---

## B14-router-059-003 — ZeroTrustConfig: routing for SECURITY alone

"Enforce zero-trust deny-all baseline across all namespaces."
  SECURITY → 6.3 | TransitionType: feature_addition | TrustState: deterministic_generic
  Note: SECURITY alone = feature_addition (security constraint is a net-new policy artifact).

---

## B14-router-059-004 — ZeroTrustConfig: gap routing

"Enable zero-trust mode on the Istio mesh."
  Zero predicates → GapHandler | "enable" is operational gap; "Istio" is gap platform name
  CapabilityGapWarnings: gap-devops-cloud-001
  Note: "zero-trust" appears in this request but is preceded by "enable" (operational gap verb). The gap verb context does not fire SECURITY — SECURITY fires only when "zero-trust" appears with a SECURITY-context verb (enforce, harden, configure in security context).
  Correction: Actually "zero-trust" is a SECURITY compound phrase that fires REGARDLESS of surrounding verb context per rule 036-004.
  Revised: SECURITY → 6.3 | TransitionType: feature_addition | AllSubPhaseIds: [6.3]
