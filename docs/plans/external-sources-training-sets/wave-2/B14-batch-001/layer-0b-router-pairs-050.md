# Layer 0-B — Router Pairs (050)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: router

---

## B14-router-050-001 — RBACRoles: routing for AUTHORIZATION alone

"Define a ClusterRole for namespace-admin operations."
  AUTHORIZATION → auth sub-phase | TransitionType: feature_addition | TrustState: deterministic_generic

---

## B14-router-050-002 — RBACRoles: routing for ARCHITECTURE + AUTHORIZATION + IMPLEMENT

"Architect the RBAC hierarchy, grant ClusterRole bindings, and implement the role manifests."
  ARCHITECTURE → 3.5 | AUTHORIZATION → auth sub-phase | IMPLEMENT → 4.1
  Sequential: 3.5 → AUTHORIZATION → 4.1 | TransitionType: feature_addition
  AllSubPhaseIds: [3.5, auth, 4.1]

---

## B14-router-050-003 — RBACRoles: routing for DOCUMENT alone

"Document the RBAC role matrix ADR."
  DOCUMENT → 6.2 | TransitionType: documentation | TrustState: deterministic_generic

---

## B14-router-050-004 — RBACRoles: routing for AUTHORIZATION + DOCUMENT

"Grant the read-only ClusterRole and document the access control runbook."
  AUTHORIZATION → auth sub-phase | DOCUMENT → 6.2
  Sequential: AUTHORIZATION → 6.2 | TransitionType: documentation (DOCUMENT present, IMPLEMENT absent)
  Note: AUTHORIZATION + DOCUMENT without IMPLEMENT → documentation TransitionType.
