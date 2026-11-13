# Layer 0-B — Router Pairs (049)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-049-001 — ServiceAccountConfig: routing for ARCHITECTURE → 3.5

"Architect the service account trust model for cross-namespace workloads."
  ARCHITECTURE → 3.5 (infra vocabulary: "service account", "cross-namespace") | TransitionType: feature_addition

---

## B14-router-049-002 — ServiceAccountConfig: routing for IMPLEMENT + AUTHORIZATION + SECURITY

"Implement the IRSA (IAM Roles for Service Accounts) Terraform module, bind the db-reader ClusterRole, and enforce mTLS."
  IMPLEMENT → 4.1 | AUTHORIZATION → auth sub-phase | SECURITY → 6.3
  Sequential: 6.3 → AUTHORIZATION → 4.1 | TransitionType: feature_addition
  AllSubPhaseIds: [4.1, auth, 6.3]

---

## B14-router-049-003 — ServiceAccountConfig: routing for ARCHITECTURE + AUTHORIZATION + SECURITY

"Design the service account trust model, bind ClusterRoles, and enforce mTLS."
  ARCHITECTURE → 3.5 | AUTHORIZATION → auth sub-phase | SECURITY → 6.3
  Sequential: 3.5 → 6.3 → AUTHORIZATION | TransitionType: feature_addition
  (AUTHORIZATION does not override documentation; feature_addition because design verb creates new artifact)

---

## B14-router-049-004 — ServiceAccountConfig: gap routing

"Create a service account for the deployment."
  Zero predicates → GapHandler | "create" without IaC-artifact context = operational gap; "service account" alone is gap
  CapabilityGapWarnings: gap-devops-cloud-001
  Note: "create" fires IMPLEMENT only when followed by an IaC artifact noun in an IaC context.
