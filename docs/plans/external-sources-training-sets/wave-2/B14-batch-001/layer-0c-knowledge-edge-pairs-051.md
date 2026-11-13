# Layer 0-C — Knowledge Edge Pairs (051)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-051-001 — NetworkPolicy: gap vocabulary — network runtime operations

Confirmed gap vocabulary for network policy runtime operations (zero predicates):
  Action verbs: apply (kubectl), patch (kubectl), delete (policy), watch (network), trace (network path), check (connectivity), ping, curl, nslookup, dig, traceroute, netstat
  Runtime nouns: network trace, connection state, flow log, pod IP, CIDR block (runtime), NAT (Network Address Translation) table

---

## B14-edge-051-002 — NetworkPolicy: SECURITY boundary — zero-trust network compound phrases

SECURITY fires for NetworkPolicy context:
  "zero-trust deny-all NetworkPolicy baseline" → SECURITY ("zero-trust")
  "enforce deny-all NetworkPolicy" → SECURITY ("enforce" + security context)
  "mTLS for all intra-mesh communication" → SECURITY ("mTLS")
  "apply the NetworkPolicy YAML" → gap ("apply" is kubectl operational gap)
  "patch the NetworkPolicy" → gap

---

## B14-edge-051-003 — NetworkPolicy: AUTHORIZATION boundary — network access permissions

AUTHORIZATION fires for NetworkPolicy access grants/restrictions:
  "permit ingress from namespace" → AUTHORIZATION ("permit" in access policy context)
  "restrict egress to database tier" → AUTHORIZATION ("restrict" in access policy)
  "grant cross-namespace access via NetworkPolicy" → AUTHORIZATION ("grant")
  These are policy definitions (AUTHORIZATION), not runtime operations (gap).

---

## B14-edge-051-004 — NetworkPolicy: IMPLEMENT boundary — NetworkPolicy IaC artifacts

IMPLEMENT fires for: Kubernetes NetworkPolicy manifest, Calico GlobalNetworkPolicy Terraform module, namespace isolation configuration, Cilium network policy configuration
  "implement the Kubernetes NetworkPolicy manifest" → IMPLEMENT
  "apply the NetworkPolicy" → gap (kubectl apply = operational)
  Rule: "implement" + manifest/module/configuration = IMPLEMENT; "apply" (kubectl) = gap.
