# Layer 0-A — Compiler Pairs (054)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-054-001 — OPAGatekeeper: ARCHITECTURE fires on OPA design vocabulary

"architect the OPA (Open Policy Agent) policy enforcement architecture" → ARCHITECTURE ("architect", "architecture")
"design the constraint template hierarchy for the cluster" → ARCHITECTURE ("design", "hierarchy")
"structure the Rego policy library and constraint framework" → ARCHITECTURE ("structure", "framework")
"install OPA" → gap ("install" is operational gap)

---

## B14-compiler-054-002 — OPAGatekeeper: IMPLEMENT fires on OPA IaC artifacts

"implement the OPA Gatekeeper ConstraintTemplate manifest" → IMPLEMENT ("implement" + "manifest")
"write the Rego policy module for pod security constraints" → IMPLEMENT ("write" + "module" — policy code)
"scaffold the Gatekeeper configuration Helm chart" → IMPLEMENT ("scaffold" + "chart")

---

## B14-compiler-054-003 — OPAGatekeeper: SECURITY fires on OPA enforcement vocabulary

"enforce OPA Gatekeeper security constraints on all namespaces" → SECURITY ("enforce" in security context)
"harden cluster policies with deny-all baseline via OPA" → SECURITY ("harden")
"configure mTLS between OPA webhook and Kubernetes API server" → SECURITY ("mTLS")

---

## B14-compiler-054-004 — OPAGatekeeper: AUTHORIZATION fires on OPA access vocabulary

"grant OPA controller ClusterRole to the gatekeeper service account" → AUTHORIZATION ("grant", "ClusterRole", "service account")
"restrict which namespaces OPA policies apply to via RoleBinding" → AUTHORIZATION ("restrict", "RoleBinding")

---

## B14-compiler-054-005 — OPAGatekeeper: disambiguation — "policy" is context-dependent

"policy" noun: fires SECURITY when in security/enforcement context; fires AUTHORIZATION when in access-grant context
  "security policy" → SECURITY | "access policy" → AUTHORIZATION | "OPA policy module" → IMPLEMENT (IaC code artifact)
  "design the policy framework" → ARCHITECTURE ("design" + "framework")
  The "policy" noun alone does not fire; surrounding verb + context determines which predicate.
