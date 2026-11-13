# Layer 0-C — Knowledge Edge Pairs (059)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-059-001 — ZeroTrustConfig: SECURITY compound phrase — "zero-trust" always fires

"zero-trust" compound fires SECURITY regardless of surrounding verb context (per rule 036-004):
  "enable zero-trust mode" → SECURITY fires (zero-trust compound phrase present)
  "configure zero-trust" → SECURITY fires
  "zero-trust topology" → SECURITY + ARCHITECTURE (topology also fires ARCHITECTURE with design verb)
  Exception: negation suppresses: "without zero-trust" → SECURITY does NOT fire.

---

## B14-edge-059-002 — ZeroTrustConfig: disambiguation — "zero-trust" with gap verb still fires SECURITY

Rule confirmed with example from router-059-004:
  "Enable zero-trust mode on the Istio mesh." → SECURITY fires (zero-trust compound phrase is present regardless of "enable" being an operational gap verb)
  This is an exception to the general rule that gap verbs suppress predicates.
  Reason: SECURITY compound phrases (mTLS enforcement, zero-trust policy, TLS termination, etc.) fire from the noun/compound, not from the verb.

---

## B14-edge-059-003 — ZeroTrustConfig: AUTHORIZATION boundary — zero-trust identity grants

AUTHORIZATION fires for zero-trust identity-based access control:
  "grant SPIFFE (Secure Production Identity Framework for Everyone) identity-based ClusterRole" → AUTHORIZATION
  "define permission boundaries for zero-trust workload identities" → AUTHORIZATION ("permission boundary")
  "restrict service-to-service calls via AuthorizationPolicy RoleBinding" → AUTHORIZATION ("RoleBinding", "restrict")

---

## B14-edge-059-004 — ZeroTrustConfig: IMPLEMENT boundary — zero-trust IaC artifacts

IMPLEMENT fires for:
  zero-trust NetworkPolicy manifest, Istio PeerAuthentication manifest, AuthorizationPolicy manifest, zero-trust Terraform module, mTLS configuration values file
  "implement zero-trust NetworkPolicy manifests" → IMPLEMENT
  "implement AuthorizationPolicy configuration" → IMPLEMENT
  "enable zero-trust mode" → gap for IMPLEMENT (enable is operational); SECURITY fires from "zero-trust" compound.
