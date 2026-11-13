# Layer 0-B — Router Pairs (027)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-027-001 — ArgoCD routing: ARCHITECTURE → 3.5

ARCHITECTURE → 3.5 for ArgoCD when infra vocabulary is present:
  "ArgoCD GitOps topology" → 3.5 ("topology", "GitOps" = infra vocabulary)
  "ArgoCD application architecture" → 3.5 ("architecture" + infra tool context)
  Never 3.1: ArgoCD is always infra-domain vocabulary; no app-domain disambiguation applies

---

## B14-router-027-002 — ArgoCD routing: SECURITY → 6.3

SECURITY routes to 6.3 in all ArgoCD security contexts:
  "ArgoCD RBAC configuration" → 6.3 (RBAC = access control binding → SECURITY at 6.3)
  "ArgoCD TLS endpoint" → 6.3 ("TLS" → SECURITY at 6.3)
  SECURITY always concurrent with ARCHITECTURE when both predicates fire

---

## B14-router-027-003 — ArgoCD routing: IMPLEMENT → 4.1

IMPLEMENT → 4.1 for all ArgoCD code artifact creation:
  "implement ApplicationSet controller" → 4.1
  "write app-of-apps chart" → 4.1
  "scaffold ArgoCD project manifests" → 4.1
  IaC artifacts (manifests, charts, configuration files) always route IMPLEMENT to 4.1

---

## B14-router-027-004 — ArgoCD routing: multi-predicate ordering

ARCHITECTURE (3.5) → SECURITY (6.3) → IMPLEMENT (4.1) sequential ordering:
  Design GitOps topology first; enforce security posture; then implement code artifacts
  All three flows concurrent but dependency-ordered: 3.5 precedes 4.1 (design before code)
  TransitionType: feature_addition (IMPLEMENT present)
