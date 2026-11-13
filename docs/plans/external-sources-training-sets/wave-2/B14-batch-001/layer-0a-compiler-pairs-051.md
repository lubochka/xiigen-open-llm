# Layer 0-A — Compiler Pairs (051)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-051-001 — NetworkPolicy: ARCHITECTURE fires on network policy design vocabulary

"architect the NetworkPolicy topology for zero-trust namespace isolation" → ARCHITECTURE ("architect", "topology")
"design the network policy model with deny-all baseline" → ARCHITECTURE ("design", "model")
"structure egress and ingress policy boundaries" → ARCHITECTURE ("structure", "boundaries")
"apply the NetworkPolicy" → gap ("apply" is operational gap in kubectl context)

---

## B14-compiler-051-002 — NetworkPolicy: IMPLEMENT fires on network policy IaC artifacts

"implement the Kubernetes NetworkPolicy manifest" → IMPLEMENT ("implement" + "manifest")
"write the Calico GlobalNetworkPolicy Terraform module" → IMPLEMENT ("write" + "module")
"scaffold the namespace isolation NetworkPolicy configuration" → IMPLEMENT ("scaffold" + "configuration")

---

## B14-compiler-051-003 — NetworkPolicy: SECURITY fires on network security vocabulary

"enforce zero-trust deny-all NetworkPolicy baseline" → SECURITY ("zero-trust", "enforce")
"harden network ingress with TLS termination policy" → SECURITY ("harden", "TLS")
"configure mTLS for all intra-mesh service communication" → SECURITY ("mTLS")

---

## B14-compiler-051-004 — NetworkPolicy: AUTHORIZATION fires on network access control

"permit ingress from the monitoring namespace to the app namespace" → AUTHORIZATION ("permit" in access policy context)
"restrict egress to the database tier only" → AUTHORIZATION ("restrict" in access policy context)
"grant cross-namespace access via NetworkPolicy RoleBinding" → AUTHORIZATION ("grant", "RoleBinding")

---

## B14-compiler-051-005 — NetworkPolicy: gap vocabulary — network runtime operations

Confirmed gap verbs for network policy operations (fire zero predicates):
  apply (kubectl), patch (kubectl), delete (policy), watch (network events), trace (network path), check (connectivity), ping, curl, nslookup, dig
  "apply NetworkPolicy YAML" → gap | "patch network policy" → gap | "check network connectivity" → gap
