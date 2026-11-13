# Layer 0-C — Knowledge Edge Pairs (050)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-050-001 — RBACRoles: gap vocabulary — RBAC runtime operations

Confirmed gap vocabulary for RBAC runtime operations (zero predicates):
  Action verbs: list (roles), get (role), describe (role — kubectl), check (access — kubectl auth can-i), impersonate, switch, debug (RBAC), audit (RBAC audit log review — operational)
  Runtime nouns: kubectl auth, can-i result, impersonation token, audit log entry

---

## B14-edge-050-002 — RBACRoles: disambiguation — "grant" always fires AUTHORIZATION

"grant" in ANY access context fires AUTHORIZATION:
  "grant read role" → AUTHORIZATION | "grant IAM role" → AUTHORIZATION | "grant cluster-admin" → AUTHORIZATION
  "grant write access to bucket" → AUTHORIZATION (access grant = AUTHORIZATION)
  Exception: "grant access to the documentation" (documentary context) → could be DOCUMENT? No: "grant access" is always AUTHORIZATION (access grant vocabulary).
  Rule: "grant" + role/permission/access vocabulary always = AUTHORIZATION. No exceptions in B14.

---

## B14-edge-050-003 — RBACRoles: disambiguation — "define" for RBAC vs ARCHITECTURE

"define a ClusterRole" → AUTHORIZATION ("ClusterRole" is AUTHORIZATION vocabulary; "define" in RBAC context fires AUTHORIZATION, not ARCHITECTURE)
"define the RBAC architecture" → ARCHITECTURE ("define" + "architecture" in design context = ARCHITECTURE)
"define the access policy" → AUTHORIZATION ("define" + "access policy" = AUTHORIZATION)
"define the API contract" → API_CONTRACT ("define" + "API contract" = API_CONTRACT)
Rule: "define" verb routes to different predicates based on the following noun. ARCHITECTURE only when noun is a design/structure artifact.

---

## B14-edge-050-004 — RBACRoles: IMPLEMENT boundary — RBAC IaC artifacts

IMPLEMENT fires for: ClusterRole manifest, RoleBinding manifest, RBAC Terraform module, role definition Helm chart values
  "implement the ClusterRole manifest" → IMPLEMENT | "write RBAC role bindings" → IMPLEMENT
  "list all ClusterRoles" → gap | "check RBAC permissions" → gap
