# Layer 0-A — Compiler Pairs (050)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-050-001 — RBACRoles: AUTHORIZATION fires on RBAC vocabulary

"define a ClusterRole for namespace-admin operations" → AUTHORIZATION ("ClusterRole")
"create a RoleBinding between the deploy-role and CI (Continuous Integration) service account" → AUTHORIZATION ("RoleBinding", "service account binding")
"grant read-only ClusterRole to the monitoring service account" → AUTHORIZATION ("grant", "ClusterRole", "service account")
"list all RBAC roles" → gap ("list" is operational gap)

---

## B14-compiler-050-002 — RBACRoles: IMPLEMENT fires on RBAC IaC artifacts

"implement the RBAC ClusterRole manifest" → IMPLEMENT ("implement" + "manifest")
"write the role binding Terraform module" → IMPLEMENT ("write" + "module")
"scaffold RBAC role definitions in the Helm chart" → IMPLEMENT ("scaffold" + code artifact)

---

## B14-compiler-050-003 — RBACRoles: ARCHITECTURE fires when RBAC is in design context

"architect the RBAC role hierarchy for multi-tenant access" → ARCHITECTURE ("architect", "hierarchy")
"design the role separation model across namespaces" → ARCHITECTURE ("design", "model")
"structure ClusterRole and namespace-scoped Role boundaries" → ARCHITECTURE ("structure", "boundaries")

---

## B14-compiler-050-004 — RBACRoles: DOCUMENT fires on RBAC documentation

"document the RBAC role matrix ADR" → DOCUMENT ("document" + "ADR")
"write the access control runbook for RBAC role management" → DOCUMENT ("write" + "runbook")

---

## B14-compiler-050-005 — RBACRoles: disambiguation — "grant" is always AUTHORIZATION

"grant" verb: always AUTHORIZATION when followed by role/permission vocabulary
  "grant read role" → AUTHORIZATION | "grant IAM role" → AUTHORIZATION | "grant cluster-admin" → AUTHORIZATION
  Exception: "grant permission" without a role noun = ambiguous → resolved by following noun
  "grant write access to the bucket" → AUTHORIZATION (access grant in policy context)
