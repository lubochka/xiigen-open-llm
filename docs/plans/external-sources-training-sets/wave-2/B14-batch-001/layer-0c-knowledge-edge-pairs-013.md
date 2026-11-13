# Layer 0-C — Knowledge Edge Pairs (013)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: knowledge_edge

---

## B14-edge-013-001 — "cluster" alone does not fire ARCHITECTURE

A: "Restart the Kubernetes cluster nodes."
  "restart" → gap | "Kubernetes cluster nodes" → gap
  CompiledPredicateIds: []
  Gap signal: gap-devops-cloud-001 — "cluster" as an operational target without design context is gap.
  Critical boundary: "cluster topology" fires ARCHITECTURE (design of the cluster). "cluster" alone (operational target) is gap.

---

## B14-edge-013-002 — "node group" design fires ARCHITECTURE

A: "Define the node group architecture — on-demand nodes for stateful workloads, Spot nodes for batch processing."
  "define" → ARCHITECTURE | "node group architecture" → ARCHITECTURE
  CompiledPredicateIds: ["ARCHITECTURE"]
  Teaching: "define" + "architecture" → ARCHITECTURE. "on-demand", "Spot" → gap (pricing model terms).

---

## B14-edge-013-003 — EKS namespace boundary fires ARCHITECTURE

A: "Architect the namespace boundary isolation for the EKS cluster — production namespace blocked from dev namespace access."
  "architect" → ARCHITECTURE | "namespace boundary isolation" → ARCHITECTURE ("boundary")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "EKS cluster" → platform name, gap. Teaching: "boundary" is an ARCHITECTURE structural noun.

---

## B14-edge-013-004 — "install the cluster" is gap

A: "Install Kubernetes on the bare metal servers."
  "install" → gap | "Kubernetes" → gap | "bare metal servers" → gap
  CompiledPredicateIds: []
  Gap signal: gap-devops-cloud-001 — "install" is an operational action gap.
  Teaching: To fire IMPLEMENT: "implement the Kubernetes installation automation for bare metal servers."
