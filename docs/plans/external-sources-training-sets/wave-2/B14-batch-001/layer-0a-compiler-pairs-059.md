# Layer 0-A — Compiler Pairs (059)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-059-001 — ZeroTrustConfig: ARCHITECTURE fires on zero-trust design vocabulary

"architect the zero-trust network architecture for the platform" → ARCHITECTURE ("architect", "architecture")
"design the zero-trust identity and access topology" → ARCHITECTURE ("design", "topology")
"structure the zero-trust perimeter model with identity-first boundaries" → ARCHITECTURE ("structure", "model", "boundaries")
"enable zero-trust mode" → gap ("enable" is operational gap)

---

## B14-compiler-059-002 — ZeroTrustConfig: IMPLEMENT fires on zero-trust IaC artifacts

"implement the zero-trust NetworkPolicy and AuthorizationPolicy manifests" → IMPLEMENT ("implement" + "manifests")
"write the Istio zero-trust PeerAuthentication Terraform module" → IMPLEMENT ("write" + "module")
"scaffold the zero-trust configuration for all service-to-service communication" → IMPLEMENT ("scaffold" + "configuration")

---

## B14-compiler-059-003 — ZeroTrustConfig: SECURITY fires on zero-trust security vocabulary

"enforce zero-trust deny-all baseline across all namespaces" → SECURITY ("zero-trust", "enforce")
"harden infrastructure with zero-trust mTLS and identity verification" → SECURITY ("harden", "zero-trust", "mTLS")
"configure zero-trust TLS certificate chain for all workloads" → SECURITY ("zero-trust policy" compound phrase)
Note: "zero-trust" alone always fires SECURITY — it is a SECURITY compound phrase trigger.

---

## B14-compiler-059-004 — ZeroTrustConfig: AUTHORIZATION fires on zero-trust access control

"grant zero-trust access via SPIFFE (Secure Production Identity Framework for Everyone) identity and ClusterRole" → AUTHORIZATION ("grant", "ClusterRole")
"restrict service-to-service calls via AuthorizationPolicy RoleBinding" → AUTHORIZATION ("RoleBinding", "restrict")
"define permission boundaries for zero-trust workload identities" → AUTHORIZATION ("permission boundary", "define" in access context)

---

## B14-compiler-059-005 — ZeroTrustConfig: multi-predicate compilation

"Architect the zero-trust topology, enforce deny-all mTLS, grant SPIFFE-based ClusterRoles, implement AuthorizationPolicy manifests, and document the ADR."
  CompiledPredicateIds: ["ARCHITECTURE", "SECURITY", "AUTHORIZATION", "IMPLEMENT", "DOCUMENT"]
  FlowCount: 5 | TransitionType: feature_addition (IMPLEMENT present)
