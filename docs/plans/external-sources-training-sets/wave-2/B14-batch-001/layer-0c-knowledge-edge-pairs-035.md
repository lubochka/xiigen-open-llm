# Layer 0-C — Knowledge Edge Pairs (035)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-035-001 — CrossAccountIAM: "assume" as CLI action is gap

Gap: "assume the deployment role via CLI" — "assume" as a CLI operational action (not IAM policy authoring) is gap
Gap: "switch to the staging account" — operational console switch action is gap
AUTHORIZATION fires: "define the assume-role trust policy" — "define" + "trust policy" (IAM) fires AUTHORIZATION
Teaching: CLI/console operational role-switching is gap. Defining/authorizing IAM trust policies fires AUTHORIZATION.

---

## B14-edge-035-002 — CrossAccountIAM: AUTHORIZATION vs SECURITY disambiguation

AUTHORIZATION fires: "authorize inter-account access for shared services" — "authorize" + access policy context
SECURITY fires: "enforce IAM permission boundaries" — "IAM" in security binding + "permission boundaries" = enforcement = SECURITY
Both fire: "authorize the cross-account roles (AUTHORIZATION) and enforce permission boundaries (SECURITY)"
Teaching: Authorization = granting/defining roles and bindings. Security = enforcing boundaries and credential policies. Both can fire simultaneously.

---

## B14-edge-035-003 — CrossAccountIAM: "CI/CD service account" fires AUTHORIZATION

AUTHORIZATION fires: "authorize the CI/CD (Continuous Integration/Continuous Delivery) pipeline service account" — "service account" in binding context + "authorize" fires AUTHORIZATION
Gap: "create the service account in Kubernetes" (operational resource creation — no role binding)
Teaching: Creating a service account (resource creation) is operational gap. Authorizing/binding a service account fires AUTHORIZATION.

---

## B14-edge-035-004 — CrossAccountIAM: "credential rotation" fires SECURITY even with operational context

SECURITY fires: "configure credential rotation for cross-account roles" — "credential rotation" compound fires SECURITY even though "configure" is operational
Teaching: SECURITY vocabulary compounds (credential rotation, key rotation, TLS, vault, zero-trust) fire SECURITY regardless of surrounding operational verbs. Only truly zero-predicate requests produce knowledge_edge_synthetic.
