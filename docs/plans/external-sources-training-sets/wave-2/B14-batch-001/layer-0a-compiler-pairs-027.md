# Layer 0-A — Compiler Pairs (027)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-027-001 — ArgoCD: ARCHITECTURE trigger

A: "Design the ArgoCD GitOps topology for multi-cluster application delivery."
  CompiledPredicateIds: ["ARCHITECTURE"]
  ARCHITECTURE: "design" + "topology" + "GitOps" (infra context) + "cluster" (design context)
  Gap: "ArgoCD" (tool name alone is gap)

---

## B14-compiler-027-002 — ArgoCD: IMPLEMENT trigger

A: "Implement the ArgoCD ApplicationSet controller and write the app-of-apps chart."
  CompiledPredicateIds: ["IMPLEMENT"]
  IMPLEMENT: "implement" + "ApplicationSet controller" (code artifact) | "write" + "app-of-apps chart" (code artifact)
  Gap: "ArgoCD" (tool name); fired by the implement/write verbs + artifact nouns

---

## B14-compiler-027-003 — ArgoCD: SECURITY trigger

A: "Configure RBAC (Role-Based Access Control) for ArgoCD and enforce TLS on the API server."
  CompiledPredicateIds: ["SECURITY"]
  SECURITY: "RBAC" (access control binding) + "TLS" | "configure" is operational but SECURITY fires from RBAC/TLS vocabulary
  Note: RBAC in access control binding context fires SECURITY, not AUTHORIZATION

---

## B14-compiler-027-004 — ArgoCD: gap vocabulary

A: "Sync the ArgoCD application and refresh the repository."
  CompiledPredicateIds: []
  Gap: "sync" (operational), "refresh" (operational), "ArgoCD" (tool name)
  TrustState: knowledge_edge_synthetic

---

## B14-compiler-027-005 — ArgoCD: ARCHITECTURE + IMPLEMENT + SECURITY

A: "Design the GitOps pipeline topology, implement the ApplicationSet, and enforce TLS on the sync endpoint."
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT", "SECURITY"]
  ARCHITECTURE: "design" + "topology" + "GitOps" | IMPLEMENT: "implement" + "ApplicationSet" | SECURITY: "TLS"
  TransitionType: feature_addition (IMPLEMENT present)
