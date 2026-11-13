# Layer 0-A — Compiler Pairs (049)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-049-001 — ServiceAccountConfig: ARCHITECTURE fires on service account design

"architect the service account trust model for cross-namespace workloads" → ARCHITECTURE ("architect", "model")
"design the service account binding topology" → ARCHITECTURE ("design", "topology")
"structure the Workload Identity Federation (WIF) architecture" → ARCHITECTURE ("structure", "architecture")
"create a service account" → gap ("create" without IaC-artifact context is operational gap)

---

## B14-compiler-049-002 — ServiceAccountConfig: IMPLEMENT fires on service account IaC artifacts

"implement the service account Kubernetes manifest" → IMPLEMENT ("implement" + "manifest")
"write the Workload Identity Terraform module" → IMPLEMENT ("write" + "module")
"scaffold the IRSA (IAM Roles for Service Accounts) configuration" → IMPLEMENT ("scaffold" + "configuration")

---

## B14-compiler-049-003 — ServiceAccountConfig: AUTHORIZATION fires on service account binding vocabulary

"bind the database-reader ClusterRole to the db-service service account" → AUTHORIZATION ("ClusterRole", "service account binding")
"grant the db-access IAM role to the application service account" → AUTHORIZATION ("grant", "IAM role", "service account")
"define a RoleBinding for the app service account to storage namespace" → AUTHORIZATION ("RoleBinding", "service account")

---

## B14-compiler-049-004 — ServiceAccountConfig: SECURITY fires on service account credential vocabulary

"enforce mTLS for all service account inter-service calls" → SECURITY ("mTLS")
"rotate service account token secrets on 24-hour schedule" → SECURITY ("rotate" + "secret" + "token")
"restrict service account OIDC (OpenID Connect) token lifetime to 1 hour" → SECURITY (credential/token restriction in security context)

---

## B14-compiler-049-005 — ServiceAccountConfig: compound predicate compilation

"Design the service account trust model, bind the db-reader ClusterRole, and enforce mTLS."
  "design" → ARCHITECTURE | "trust model" → ARCHITECTURE | "bind" + "ClusterRole" → AUTHORIZATION | "enforce" + "mTLS" → SECURITY
  CompiledPredicateIds: ["ARCHITECTURE", "AUTHORIZATION", "SECURITY"]
