# Layer 0-A — Compiler Pairs (013)
# Batch: B14-batch-001 | Scenario: B14 — DevOps cloud infrastructure and deployment topology
# Production date: 2026-05-06 | pair_kind: compiler

---

## B14-compiler-013-001 — ARCHITECTURE: EKS cluster topology

A: "Design the EKS (Elastic Kubernetes Service) cluster topology for the microservices platform."
  "design" → ARCHITECTURE | "cluster topology" → ARCHITECTURE ("topology") | "microservices platform" → ARCHITECTURE ("layer")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "EKS", "Elastic Kubernetes Service" → cloud platform name, gap

---

## B14-compiler-013-002 — ARCHITECTURE: node group architecture

A: "Architect the node group structure for the Kubernetes cluster — dedicated node pools per workload class."
  "architect" → ARCHITECTURE | "node group structure" → ARCHITECTURE ("structure") | "node pools" → ARCHITECTURE ("layer")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "Kubernetes", "node pools" → platform vocabulary, gap

---

## B14-compiler-013-003 — ARCHITECTURE+IMPLEMENT: control plane setup

A: "Design the EKS control plane architecture and implement the managed node group configuration."
  "design" → ARCHITECTURE | "control plane architecture" → ARCHITECTURE | "implement" → IMPLEMENT | "node group configuration" → IMPLEMENT (concrete config)
  CompiledPredicateIds: ["ARCHITECTURE", "IMPLEMENT"]
  Gap: "EKS", "managed node group" → platform vocabulary, gap

---

## B14-compiler-013-004 — ARCHITECTURE: namespace isolation topology

A: "Define the namespace isolation topology — production, staging, and development namespaces with boundary controls."
  "define" → ARCHITECTURE | "namespace isolation topology" → ARCHITECTURE ("topology") | "boundary controls" → ARCHITECTURE ("boundary")
  CompiledPredicateIds: ["ARCHITECTURE"]
  Gap: "namespace", "production", "staging" → operational vocabulary, gap

---

## B14-compiler-013-005 — gap: "upgrade the cluster"

A: "Upgrade the EKS cluster to version 1.29."
  "upgrade" → gap (operational maintenance; not in IMPLEMENT lexicon as upgrade action) | "EKS cluster" → gap | "version 1.29" → gap
  CompiledPredicateIds: []
  Teaching: "upgrade" is an operational verb that is gap. To fire IMPLEMENT: "implement the cluster version migration procedure."
